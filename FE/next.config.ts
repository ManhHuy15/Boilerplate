import type { NextConfig } from "next";
import { API_ROUTES } from "./shared/constants/api";

const defaultApiBaseUrl = "http://localhost:8080";
const apiBaseUrl = (process.env.API_BASE_URL ?? defaultApiBaseUrl).replace(/\/$/, "");
const authProxyRoute = `${API_ROUTES.auth.root}/:path*`;

const nextConfig: NextConfig = {
  reactStrictMode: true,
  async rewrites() {
    return [
      {
        source: authProxyRoute,
        destination: `${apiBaseUrl}${authProxyRoute}`,
      },
      {
        source: API_ROUTES.health,
        destination: `${apiBaseUrl}${API_ROUTES.health}`,
      },
    ];
  },
};

export default nextConfig;
