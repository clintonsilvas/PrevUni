import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  /* config options here */
  reactCompiler: true,
  allowedDevOrigins: [
    "prevuni.unifenas.local",
    "unihm.edu.br"    
  ],
};

export default nextConfig;
