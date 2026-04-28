import { defineConfig } from "vite-plus";
import react from "@vitejs/plugin-react";
import { resolve } from "path";

import { fileURLToPath } from "url";
import { dirname } from "path";
const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);

const ivyFrontendSrc = resolve(__dirname, "../../../frontend/src");

// The upstream textarea.css uses `@layer base` which is fine under the host's Tailwind v4
// build but explodes when this widget's Tailwind v3 tries to parse it standalone. The cosmetic
// resizer-grip styling isn't load-bearing, so we replace its contents with an empty stub.
const stubIvyTextareaCss = {
  name: "stub-ivy-textarea-css",
  enforce: "pre" as const,
  load(id: string) {
    if (id.replace(/\\/g, "/").endsWith("/components/ui/textarea/textarea.css")) {
      return "";
    }
    return null;
  },
};

export default defineConfig({
  plugins: [stubIvyTextareaCss, react()],
  resolve: {
    alias: [
      { find: /^@\/components\//, replacement: ivyFrontendSrc + "/components/" },
      { find: "@", replacement: resolve(__dirname, "src") },
    ],
  },
  define: {
    "process.env.NODE_ENV": JSON.stringify("production"),
  },
  build: {
    lib: {
      entry: resolve(__dirname, "src/index.ts"),
      formats: ["iife"],
      fileName: () => "Ivy_Widgets_PlanAdjuster.js",
      name: "Ivy_Widgets_PlanAdjuster",
    },
    rolldownOptions: {
      external: ["react", "react-dom", "react/jsx-runtime"],
      output: {
        globals: {
          react: "React",
          "react-dom": "ReactDOM",
          "react/jsx-runtime": "ReactJsxRuntime",
        },
        extend: false,
      },
    },
    outDir: "dist",
    emptyOutDir: true,
    sourcemap: true,
    cssCodeSplit: false,
  },
});
