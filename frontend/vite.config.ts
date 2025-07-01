import path from "path"
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

const injectIvyHost = (mode: string) => {
 return {
  name: 'inject-ivy-host-meta',
  transformIndexHtml(html: string) {
    if (mode === 'development') {
      const metaTag = `<meta name="ivy-host" content="${process.env.IVY_HOST || 'http://localhost:5010'}" />`;
      return html.replace('</head>', `  ${metaTag}\n</head>`);
    }
    return html;
  } 
}};

export default defineConfig(({ mode }) => ({
  plugins: [react(), injectIvyHost(mode)],
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },
  build: {
    outDir: "dist", 
    assetsDir: "assets", 
    rollupOptions: {
      input: {
        main: 'index.html',
        app: 'app.html'
      },
      output: {
        entryFileNames: 'assets/[name].[hash].js',
        chunkFileNames: 'assets/[name].[hash].js',
        assetFileNames: 'assets/[name].[hash].[ext]'
      }
    }
  }
}));
