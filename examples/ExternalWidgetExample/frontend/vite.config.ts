import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import { resolve } from 'path';

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    react({
      // Use classic JSX transform (React.createElement) instead of automatic (jsx-runtime)
      // This allows external widgets to use the global React without needing jsx-runtime
      jsxRuntime: 'classic',
    }),
  ],
  build: {
    lib: {
      // Entry point for the library
      entry: resolve(__dirname, 'src/index.ts'),
      // Use IIFE format for browser globals compatibility
      formats: ['iife'],
      // Output filename
      fileName: () => 'SuperChartWidget.js',
      // Global variable name for the library (must match C# class name)
      name: 'SuperChart',
    },
    rollupOptions: {
      // Externalize React - the host app provides these via globals
      external: ['react', 'react-dom'],
      output: {
        // Global variable names for externals
        globals: {
          react: 'React',
          'react-dom': 'ReactDOM',
        },
        // Ensure the exports are accessible
        extend: true,
      },
    },
    // Output to dist folder
    outDir: 'dist',
    // Don't empty the output directory (in case of multiple builds)
    emptyOutDir: true,
    // Generate sourcemaps for debugging
    sourcemap: true,
  },
});
