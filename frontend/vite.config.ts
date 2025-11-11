import path from 'path';
import { defineConfig, type Plugin } from 'vite';
import react from '@vitejs/plugin-react-swc';
import tailwindcss from '@tailwindcss/vite';

function transferMeta(htmlServer: string, htmlLocal: string): string {
  const titleMatch = htmlServer.match(/<title[^>]*>(.*?)<\/title>/i);
  const serverTitle = titleMatch ? titleMatch[1] : null;

  let result = htmlLocal;

  if (serverTitle) {
    result = result.replace(
      /<title[^>]*>.*?<\/title>/i,
      `<title>${serverTitle}</title>`
    );
  }

  const ivyMetaMatches = htmlServer.match(
    /<meta[^>]*name\s*=\s*["']ivy-[^"']*["'][^>]*>/gi
  );

  if (ivyMetaMatches) {
    const headEndIndex = result.indexOf('</head>');
    if (headEndIndex !== -1) {
      const metasToInsert = ivyMetaMatches.map(meta => ` ${meta}`).join('\n');
      result =
        result.slice(0, headEndIndex) +
        metasToInsert +
        '\n ' +
        result.slice(headEndIndex);
    }
  }

  return result;
}

const injectMeta = (mode: string): Plugin => {
  return {
    name: 'inject-ivy-meta',
    async transformIndexHtml(localHtml: string) {
      if (mode === 'development') {
        const host = process.env.IVY_HOST || 'http://localhost:5010';
        const serverHtml = await fetch(`${host}`).then(res => res.text());
        const transformedHtml = transferMeta(serverHtml, localHtml);
        const ivyHostTag = `<meta name="ivy-host" content="${host}" />`;
        return transformedHtml.replace('</head>', ` ${ivyHostTag}\n</head>`);
      }
      return localHtml;
    },
  };
};

export default defineConfig(({ mode }) => ({
  plugins: [react(), tailwindcss(), injectMeta(mode)] as Plugin[],
  esbuild: {
    drop: process.env.NODE_ENV === 'production' ? ['console', 'debugger'] : [],
    legalComments: 'none',
  },
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
  build: {
    target: 'es2020',
    outDir: 'dist',
    assetsDir: 'assets',
    cssCodeSplit: true,
    sourcemap: false,
    rollupOptions: {
      input: {
        main: path.resolve(__dirname, 'index.html'),
      },
      output: {
        entryFileNames: 'assets/[name].[hash].js',
        chunkFileNames: 'assets/[name].[hash].js',
        assetFileNames: 'assets/[name].[hash].[ext]',
        // Fine-grained vendor chunking to keep initial payloads small and improve caching
        manualChunks(id) {
          if (id.includes('node_modules')) {
            if (id.includes('react') && !id.includes('recharts'))
              return 'vendor-react';
            if (
              id.includes('codemirror') ||
              id.includes('@uiw/react-codemirror')
            )
              return 'vendor-codemirror';
            if (
              id.includes('remark') ||
              id.includes('rehype') ||
              id.includes('unified') ||
              id.includes('react-markdown')
            )
              return 'vendor-markdown';
            if (id.includes('mermaid')) return 'vendor-mermaid';
            if (id.includes('reactflow')) return 'vendor-reactflow';
            if (id.includes('framer-motion')) return 'vendor-motion';
            if (id.includes('katex')) return 'vendor-katex';
            if (id.includes('lodash')) return 'vendor-lodash';
          }
          return undefined;
        },
      },
    },
  },
  test: {
    include: ['**/*.test.ts'],
    exclude: ['**/e2e/**', '**/node_modules/**', '**/dist/**'],
    environment: 'happy-dom',
  },
}));
