import { defineConfig } from "vite-plus";
import react from "@vitejs/plugin-react";
import { resolve } from "path";

import { fileURLToPath } from "url";
import { dirname } from "path";
const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    react(),
  ],
  define: {
    // Define process.env.NODE_ENV for libraries that check it
    "process.env.NODE_ENV": JSON.stringify("production"),
  },
  build: {
    lib: {
      // Entry point for the library
      entry: resolve(__dirname, "src/index.ts"),
      // Use IIFE format for browser globals compatibility
      formats: ["iife"],
      // Output filename
      fileName: () => "Ivy_Widgets_Leaflet.js",
      // Global variable name - matches the C# namespace (dots replaced by underscores)
      name: "Ivy_Widgets_Leaflet",
    },
    rolldownOptions: {
      // Externalize React - the host app provides these via globals
      external: ["react", "react-dom", "react-dom/client"],
      output: {
        // Global variable names for externals
        globals: {
          react: "React",
          "react-dom": "ReactDOM",
          "react-dom/client": "ReactDOM",
        },
        // Use extend: false to create a proper global variable
        extend: false,
      },
    },
    // Output to dist folder
    outDir: "dist",
    // Don't empty the output directory (in case of multiple builds)
    emptyOutDir: true,
    // Generate sourcemaps for debugging
    sourcemap: true,
    // Inline CSS into the JS bundle for external widgets
    cssCodeSplit: false,
  },
  css: {
    // Inject CSS into the page instead of extracting to file
    // This ensures the Leaflet styles are available when the widget loads
  },
});
