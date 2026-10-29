export const ACCOUNT_ROLES = {
  user: "User",
  admin: "Admin",
} as const;

export const AUTH_STATUSES = {
  loading: "loading",
  authenticated: "authenticated",
  unauthenticated: "unauthenticated",
} as const;

export const AUTH_FORM_MODES = {
  login: "login",
  register: "register",
} as const;

export const AUTH_FIELDS = {
  username: "username",
  password: "password",
} as const;

export const AUTH_CONSTRAINTS = {
  username: {
    minLength: 3,
    maxLength: 100,
  },
  password: {
    minLength: 8,
    maxLength: 128,
  },
} as const;

export const AUTH_SESSION = {
  refreshBufferMilliseconds: 60_000,
  minimumRefreshDelayMilliseconds: 1_000,
} as const;
