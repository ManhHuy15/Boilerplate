import type {
  ACCOUNT_ROLES,
  AUTH_FORM_MODES,
  AUTH_STATUSES,
} from "@/features/auth/constants/auth";

export type AccountRole = (typeof ACCOUNT_ROLES)[keyof typeof ACCOUNT_ROLES];

export interface Account {
  id: number;
  username: string;
  role: AccountRole;
}

export interface AuthSession {
  accessToken: string;
  accessTokenExpiresAtUtc: string;
  account: Account;
}

export interface Credentials {
  username: string;
  password: string;
}

export type AuthStatus = (typeof AUTH_STATUSES)[keyof typeof AUTH_STATUSES];

export type AuthFormMode = (typeof AUTH_FORM_MODES)[keyof typeof AUTH_FORM_MODES];
