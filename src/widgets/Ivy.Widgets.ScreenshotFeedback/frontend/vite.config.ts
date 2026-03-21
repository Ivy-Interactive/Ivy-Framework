import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import { resolve } from 'path';

export default defineConfig({
  plugins: [
    react({
      jsxRuntime: 'classic',
    }),
  ],
  define: {
    'process.env.NODE_ENV': JSON.stringify('production'),
  },
  build: {
    lib: {
      entry: resolve(__dirname, 'src/index.ts'),
      formats: ['iife'],
      fileName: () => 'Ivy_Widgets_ScreenshotFeedback.js',
      name: 'Ivy_Widgets_ScreenshotFeedback',
    },
    rollupOptions: {
      external: ['react', 'react-dom'],
      output: {
        globals: {
          react: 'React',
          'react-dom': 'ReactDOM',
        },
        extend: false,
      },
    },
    outDir: 'dist',
    emptyOutDir: true,
    sourcemap: true,
    cssCodeSplit: false,
  },
});
