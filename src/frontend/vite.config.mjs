import path from "path";
import { defineConfig } from "vite-plus";
import mkcert from "vite-plugin-mkcert";
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

function isLocalHost(urlString) {
  try {
    const url = new URL(urlString);
    return ["localhost", "127.0.0.1", "::1"].includes(url.hostname);
  } catch {
    return false;
  }
}

async function fetchText(url) {
  const mod = url.startsWith("https") ? await import("node:https") : await import("node:http");
  const options = isLocalHost(url) ? { rejectUnauthorized: false } : {};
  return new Promise((resolve, reject) => {
    mod
      .get(url, options, (res) => {
        let data = "";
        res.on("data", (chunk) => (data += chunk));
        res.on("end", () => resolve(data));
      })
      .on("error", reject);
  });
}

const injectMeta = (mode) => {
  return {
    name: "inject-ivy-meta",
    async transformIndexHtml(localHtml) {
      if (mode === "development") {
        const host = process.env.IVY_HOST || "https://localhost:5010";
        const serverHtml = await fetchText(`${host}`);
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
  base: "./",
  staged: {
    "src/**/*.{ts,tsx,js,jsx}": "vp check --fix",
    "../**/*.cs": "dotnet format ../Ivy-Framework.slnx --include",
  },

  plugins: [react(), tailwindcss(), mkcert(), injectMeta(mode)],
  server: {
    proxy: {
      "^/(.*\\.md|llms\\.txt)$": {
        target: process.env.IVY_HOST || "https://localhost:5010",
        changeOrigin: true,
        secure: false,
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
    include: ["**/*.test.ts", "**/*.test.tsx"],
    exclude: ["**/e2e/**", "**/node_modules/**", "**/dist/**"],
    environment: "happy-dom",
  },
});
