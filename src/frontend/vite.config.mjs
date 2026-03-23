import path from "path";
import { defineConfig } from "vite-plus";
import react from "@vitejs/plugin-react";
import tailwindcss from "@tailwindcss/vite";

import { fileURLToPath } from "url";
import { dirname } from "path";
const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);

function transferMeta(htmlServer, htmlLocal) {
  const titleMatch = htmlServer.match(/<title[^>]*>(.*?)<\/title>/i);
  const serverTitle = titleMatch ? titleMatch[1] : null;

  let result = htmlLocal;

  if (serverTitle) {
    result = result.replace(/<title[^>]*>.*?<\/title>/i, `<title>${serverTitle}</title>`);
  }

  // Transfer ivy-* meta tags
  const ivyMetaMatches = htmlServer.match(/<meta[^>]*name\s*=\s*["']ivy-[^"']*["'][^>]*>/gi);

  // Transfer ivy-custom-theme style tag
  const themeStyleMatch = htmlServer.match(/<style id="ivy-custom-theme">[\s\S]*?<\/style>/i);

  if (ivyMetaMatches || themeStyleMatch) {
    const headEndIndex = result.indexOf("</head>");
    if (headEndIndex !== -1) {
      let toInsert = "";

      if (ivyMetaMatches) {
        toInsert += ivyMetaMatches.map((meta) => ` ${meta}`).join("\n");
      }

      if (themeStyleMatch) {
        if (toInsert) toInsert += "\n";
        toInsert += ` ${themeStyleMatch[0]}`;
      }

      result = result.slice(0, headEndIndex) + toInsert + "\n " + result.slice(headEndIndex);
    }
  }

  return result;
}

const injectMeta = (mode) => {
  return {
    name: "inject-ivy-meta",
    async transformIndexHtml(localHtml) {
      if (mode === "development") {
        const host = process.env.IVY_HOST || "http://localhost:5010";
        const serverHtml = await fetch(`${host}`).then((res) => res.text());
        const transformedHtml = transferMeta(serverHtml, localHtml);
        const ivyHostTag = `<meta name="ivy-host" content="${host}" />`;
        return transformedHtml.replace("</head>", ` ${ivyHostTag}\n</head>`);
      }
      return localHtml;
    },
  };
};

const mode = process.env.NODE_ENV || "development";
export default defineConfig({
  plugins: [react(), tailwindcss(), injectMeta(mode)],
  server: {
    proxy: {
      "^/(.*\\.md|llms\\.txt)$": {
        target: process.env.IVY_HOST || "http://localhost:5010",
        changeOrigin: true,
      },
    },
  },
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },
  build: {
    target: "es2020",
    outDir: "dist",
    assetsDir: "assets",
    cssCodeSplit: true,
    sourcemap: false,
    rollupOptions: {
      output: {
        entryFileNames: "assets/[name]-[hash].js",
        chunkFileNames: "assets/[name]-[hash].js",
        assetFileNames: "assets/[name]-[hash].[ext]",
      },
    },
  },
  test: {
    include: ["**/*.test.ts"],
    exclude: ["**/e2e/**", "**/node_modules/**", "**/dist/**"],
    environment: "happy-dom",
  },
});
