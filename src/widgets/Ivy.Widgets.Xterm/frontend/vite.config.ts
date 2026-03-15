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
  define: {
    // Define process.env.NODE_ENV for libraries that check it
    'process.env.NODE_ENV': JSON.stringify('production'),
  },
  build: {
    lib: {
      // Entry point for the library
      entry: resolve(__dirname, 'src/index.ts'),
      // Use IIFE format for browser globals compatibility
      formats: ['iife'],
      // Output filename
      fileName: () => 'Ivy_Widgets_Xterm.js',
      // Global variable name - matches the C# namespace (dots replaced by underscores)
      name: 'Ivy_Widgets_Xterm',
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
        // Use extend: false to create a proper global variable
        extend: false,
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
