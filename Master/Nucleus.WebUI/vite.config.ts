import { fileURLToPath, URL } from 'node:url';
import { defineConfig, type Plugin } from 'vite';
import react from '@vitejs/plugin-react';
import tailwindcss from '@tailwindcss/vite';

/**
 * Content Security Policy for the packaged UI (not the dev server, which needs inline HMR code).
 * Scripts and fonts are local; data comes from the host bridge and the public Hub API; images may be
 * remote game art (IGDB, Steam CDN) or local files on the assets virtual host.
 */
const CSP = [
  "default-src 'self'",
  "script-src 'self'",
  "style-src 'self' 'unsafe-inline'",
  "font-src 'self'",
  "img-src 'self' data: blob: https:",
  "connect-src 'self' https://hub.splitscreen.me",
  "object-src 'none'",
  "base-uri 'self'",
  "form-action 'none'",
].join('; ');

const csp = (): Plugin => ({
  name: 'nucleus-csp',
  apply: 'build',
  transformIndexHtml: (html) =>
    html.replace('<meta charset="utf-8" />', `<meta charset="utf-8" />\n    <meta http-equiv="Content-Security-Policy" content="${CSP}" />`),
});

// The build is served by WebView2 from a virtual host folder mapping (https://ui.nucleus.example/ → webui\):
// relative asset URLs, a single index.html, hash routing, no server-side fallback.
export default defineConfig({
  base: './',
  plugins: [react(), tailwindcss(), csp()],
  resolve: {
    alias: { '@': fileURLToPath(new URL('./src', import.meta.url)) },
  },
  build: {
    outDir: 'dist',
    emptyOutDir: true,
    target: 'chrome120',
    assetsInlineLimit: 0,
    sourcemap: false,
  },
  server: { port: 5173, strictPort: true },
  preview: { port: 4173, strictPort: true },
});
