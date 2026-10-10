import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  // Genera .next/standalone con lo mínimo para correr en Docker (node server.js).
  output: "standalone",
  cacheComponents: true,
  partialPrefetching: true,
  turbopack: {
    rules: {
      "*.css": {
        loaders: ["@tailwindcss/turbopack"],
        as: "*.css",
      },
    },
  },
};

export default nextConfig;
