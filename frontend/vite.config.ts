import path from 'path';
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
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
      const metasToInsert = ivyMetaMatches
        .map(meta => `    ${meta}`)
        .join('\n');
      result =
        result.slice(0, headEndIndex) +
        metasToInsert +
        '\n  ' +
        result.slice(headEndIndex);
    }
  }

  return result;
}

const injectMeta = (mode: string) => {
  return {
    name: 'inject-ivy-meta',
    async transformIndexHtml(localHtml: string) {
      if (mode === 'development') {
        const host = process.env.IVY_HOST || 'http://localhost:5010';
        const serverHtml = await fetch(`${host}`).then(res => res.text());
        const transformedHtml = transferMeta(serverHtml, localHtml);
        const ivyHostTag = `<meta name="ivy-host" content="${host}" />`;
        return transformedHtml.replace('</head>', `  ${ivyHostTag}\n</head>`);
      }
      return localHtml;
    },
  };
};

export default defineConfig(({ mode }) => ({
  base: '/',
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
