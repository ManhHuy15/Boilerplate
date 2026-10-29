import { API_ROUTES } from "@/shared/constants/api";
import { HTTP_METHODS } from "@/shared/constants/http";
import { apiRequest } from "@/shared/lib/apiClient";
import type { Account, AuthSession, Credentials } from "@/features/auth/types/auth";
import type { ApiResponse } from "@/shared/types/api";
import { isAccount, isAuthSession, isNull } from "@/features/auth/utils/authGuards";

export const authService = {
  login(credentials: Credentials): Promise<ApiResponse<AuthSession>> {
    return apiRequest(API_ROUTES.auth.login, isAuthSession, {
      method: HTTP_METHODS.post,
      body: JSON.stringify(credentials),
    });
  },

  register(credentials: Credentials): Promise<ApiResponse<AuthSession>> {
    return apiRequest(API_ROUTES.auth.register, isAuthSession, {
      method: HTTP_METHODS.post,
      body: JSON.stringify(credentials),
    });
  },

  refresh(): Promise<ApiResponse<AuthSession>> {
    return apiRequest(API_ROUTES.auth.refresh, isAuthSession, {
      method: HTTP_METHODS.post,
    });
  },

  logout(): Promise<ApiResponse<null>> {
    return apiRequest(API_ROUTES.auth.logout, isNull, {
      method: HTTP_METHODS.post,
    });
  },

  getMe(
    accessToken: string,
    onUnauthorized: (rejectedAccessToken: string) => Promise<string | null>,
  ): Promise<ApiResponse<Account>> {
    return apiRequest(API_ROUTES.auth.me, isAccount, {
      accessToken,
      onUnauthorized,
    });
  },
};
