import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// The built application is served by the .NET host from its web root, so a
// `npm run build` is all it takes to update the frontend of a running server.
const apiTarget = process.env.LAMBDA_API ?? 'http://localhost:8080';

export default defineConfig({
  plugins: [react()],
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
