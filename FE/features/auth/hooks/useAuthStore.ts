"use client";

import { create } from "zustand";
import { AUTH_SESSION, AUTH_STATUSES } from "@/features/auth/constants/auth";
import { authService } from "@/features/auth/services/authService";
import type {
  Account,
  AuthSession,
  AuthStatus,
  Credentials,
} from "@/features/auth/types/auth";
import type { ApiResponse } from "@/shared/types/api";

const MISSING_SESSION_MESSAGE = "The server did not return an auth session.";
const MISSING_ACCOUNT_MESSAGE = "The server did not return an account.";

interface AuthStore {
  session: AuthSession | null;
  status: AuthStatus;
  initialize: () => Promise<void>;
  login: (credentials: Credentials) => Promise<void>;
  register: (credentials: Credentials) => Promise<void>;
  logout: () => Promise<void>;
  reloadAccount: () => Promise<void>;
  refreshSession: () => Promise<void>;
}

let sharedRefreshRequest: Promise<AuthSession> | null = null;
let refreshTimer: ReturnType<typeof setTimeout> | null = null;
let authRevision = 0;

function requireData<T>(response: ApiResponse<T>, errorMessage: string): T {
  if (response.data === null) {
    throw new Error(errorMessage);
  }

  return response.data;
}

function requestNewSession(): Promise<AuthSession> {
  sharedRefreshRequest ??= authService
    .refresh()
    .then((response) => requireData(response, MISSING_SESSION_MESSAGE))
    .finally(() => {
      sharedRefreshRequest = null;
    });

  return sharedRefreshRequest;
}

function cancelScheduledRefresh() {
  if (refreshTimer !== null) {
    clearTimeout(refreshTimer);
    refreshTimer = null;
  }
}

export const useAuthStore = create<AuthStore>((set, get) => {
  function clearSession() {
    cancelScheduledRefresh();
    set({ session: null, status: AUTH_STATUSES.unauthenticated });
  }

  function applySession(session: AuthSession) {
    cancelScheduledRefresh();
    set({ session, status: AUTH_STATUSES.authenticated });

    const expiresAt = Date.parse(session.accessTokenExpiresAtUtc);
    if (Number.isNaN(expiresAt)) {
      return;
    }

    const refreshInMilliseconds = Math.max(
      expiresAt - Date.now() - AUTH_SESSION.refreshBufferMilliseconds,
      AUTH_SESSION.minimumRefreshDelayMilliseconds,
    );

    refreshTimer = setTimeout(() => {
      void get().refreshSession();
    }, refreshInMilliseconds);
  }

  async function refreshSession() {
    const revisionAtStart = authRevision;

    try {
      const session = await requestNewSession();
      if (authRevision === revisionAtStart) {
        applySession(session);
      }
    } catch {
      if (authRevision === revisionAtStart) {
        clearSession();
      }
    }
  }

  return {
    session: null,
    status: AUTH_STATUSES.loading,

    initialize: refreshSession,
    refreshSession,

    login: async (credentials) => {
      const response = await authService.login(credentials);
      authRevision += 1;
      applySession(requireData(response, MISSING_SESSION_MESSAGE));
    },

    register: async (credentials) => {
      const response = await authService.register(credentials);
      authRevision += 1;
      applySession(requireData(response, MISSING_SESSION_MESSAGE));
    },

    logout: async () => {
      authRevision += 1;
      cancelScheduledRefresh();

      try {
        await authService.logout();
      } finally {
        clearSession();
      }
    },

    reloadAccount: async () => {
      const currentSession = get().session;
      if (!currentSession) {
        return;
      }
      const revisionAtStart = authRevision;

      const response = await authService.getMe(
        currentSession.accessToken,
        async (rejectedAccessToken) => {
          const latestAccessToken = get().session?.accessToken;

          if (latestAccessToken && latestAccessToken !== rejectedAccessToken) {
            return latestAccessToken;
          }

          await get().refreshSession();
          return get().session?.accessToken ?? null;
        },
      );
      const account: Account = requireData(response, MISSING_ACCOUNT_MESSAGE);

      set((state) => ({
        session:
          state.session && authRevision === revisionAtStart
            ? { ...state.session, account }
            : state.session,
      }));
    },
  };
});
