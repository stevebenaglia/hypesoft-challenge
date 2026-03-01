import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  output: "standalone",
  reactStrictMode: true,
  async rewrites() {
    const backendUrl = process.env.INTERNAL_API_URL ?? "http://backend:5000";
    const backendPrefixes = ["categories", "products", "dashboard"];
    return backendPrefixes.map((prefix) => ({
      source: `/api/${prefix}/:path*`,
      destination: `${backendUrl}/api/${prefix}/:path*`,
    }));
  },
};

export default nextConfig;
