import path from 'path';
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import tailwindcss from '@tailwindcss/vite';

const injectMeta = (mode: string) => {
  return {
    name: 'inject-ivy-meta',
    transformIndexHtml(html: string) {
      if (mode === 'development') {
        const ivyHostTag = `<meta name="ivy-host" content="${process.env.IVY_HOST || 'http://localhost:5010'}" />`;
        return html.replace('</head>', `  ${ivyHostTag}\n</head>`);
      }
      return html;
    },
  };
};

export default defineConfig(({ mode }) => ({
  plugins: [react(), tailwindcss(), injectMeta(mode)],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
  build: {
    outDir: 'dist',
    assetsDir: 'assets',
    rollupOptions: {
      input: {
        main: path.resolve(__dirname, 'index.html'),
      },
      output: {
        entryFileNames: 'assets/[name].[hash].js',
        chunkFileNames: 'assets/[name].[hash].js',
        assetFileNames: 'assets/[name].[hash].[ext]',
      },
    },
  },
}));
