import type { NextConfig } from "next";

const defaultApiBaseUrl = "http://localhost:8080";
const apiBaseUrl = (process.env.API_BASE_URL ?? defaultApiBaseUrl).replace(/\/$/, "");

const nextConfig: NextConfig = {
  output: "standalone",
  reactStrictMode: true,
  async rewrites() {
    return [
      {
        source: "/api/:path*",
        destination: `${apiBaseUrl}/api/:path*`,
      },
    ];
  },
};

export default nextConfig;
