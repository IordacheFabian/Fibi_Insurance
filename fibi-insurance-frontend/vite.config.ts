import { defineConfig, loadEnv } from "vite";
import react from "@vitejs/plugin-react-swc";
import path from "path";
import { componentTagger } from "lovable-tagger";

// https://vitejs.dev/config/
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), "");

  const apiTarget = (() => {
    const configuredTarget = (env.VITE_API_URL || "").trim();

    if (!configuredTarget) {
      return "https://localhost:7260";
    }

    // When a localhost HTTPS port is configured with http://, ASP.NET redirects and
    // auth headers can be lost during the hop. Force HTTPS for high local ports.
    const localHttpMatch = configuredTarget.match(/^http:\/\/localhost:(\d+)$/i);
    if (localHttpMatch) {
      const port = Number(localHttpMatch[1]);
      if (port >= 7000) {
        return `https://localhost:${port}`;
      }
    }

    return configuredTarget;
  })();

  return {
    server: {
      host: "::",
      port: 8080,
      hmr: {
        overlay: false,
      },
      proxy: {
        "/api": {
          target: apiTarget,
          changeOrigin: true,
          secure: false,
        },
      },
    },
    plugins: [react(), mode === "development" && componentTagger()].filter(Boolean),
    resolve: {
      alias: {
        "@": path.resolve(__dirname, "./src"),
      },
      dedupe: ["react", "react-dom", "react/jsx-runtime", "react/jsx-dev-runtime", "@tanstack/react-query", "@tanstack/query-core"],
    },
  };
});
