import { readFileSync } from 'node:fs';
import { defineConfig, type Plugin } from 'vite';
import react from '@vitejs/plugin-react';

// The built application is served by the .NET host from its web root, so a
// `npm run build` is all it takes to update the frontend of a running server.
const apiTarget = process.env.LAMBDA_API ?? 'http://localhost:8080';

/**
 * Puts the table of public pages next to the index page, where the server
 * reads it to name each page before sending it and to write the sitemap. One
 * file for both sides, so the title a crawler sees is the one the tab shows.
 */
function pageTable(): Plugin {
  return {
    name: 'page-table',
    generateBundle() {
      this.emitFile({
        type: 'asset',
        fileName: 'pages.json',
        source: readFileSync(new URL('./src/pages.json', import.meta.url), 'utf-8'),
      });
    },
  };
}

export default defineConfig({
  plugins: [react(), pageTable()],
  build: {
    outDir: '../GenHTTP.Lambda/wwwroot',
    emptyOutDir: true,
    chunkSizeWarningLimit: 4096,
  },
  server: {
    port: 5173,
    proxy: {
      '/api': { target: apiTarget, changeOrigin: true },
      '/lambda': { target: apiTarget, changeOrigin: true },
    },
  },
});
