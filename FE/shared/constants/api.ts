export const API_ROUTES = {
  auth: {
    root: "/api/auth",
    register: "/api/auth/register",
    login: "/api/auth/login",
    refresh: "/api/auth/refresh",
    logout: "/api/auth/logout",
    me: "/api/auth/me",
  },
  health: "/api/health",
} as const;
