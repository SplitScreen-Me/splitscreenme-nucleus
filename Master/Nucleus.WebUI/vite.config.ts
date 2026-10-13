import { fileURLToPath, URL } from 'node:url';
import { defineConfig, type Plugin } from 'vite';
import react from '@vitejs/plugin-react';
import tailwindcss from '@tailwindcss/vite';

/**
 * Origins the UI may fetch the hub API v3 from: the public site (src/hub/urls.ts DEFAULT_HUB_URL, HubSite.DefaultUrl in
 * C#), plus NUCLEUS_HUB_ORIGINS at build time (space or comma separated, e.g. `http://localhost:3000`) for a build
 * that talks to another site set in Settings.ini [Dev] HubUrl. The UI reports a clear error for a site this list
 * does not allow (src/hub/site.ts); the dev server has no CSP.
 */
const HUB_ORIGINS = ['https://v3.splitscreen.me', ...extraHubOrigins(process.env.NUCLEUS_HUB_ORIGINS)];

function extraHubOrigins(value: string | undefined): string[] {
  return (value ?? '')
    .split(/[\s,]+/)
    .filter(Boolean)
    .map((raw) => {
      const url = new URL(raw);
      if (url.protocol !== 'https:' && url.protocol !== 'http:') throw new Error(`NUCLEUS_HUB_ORIGINS: ${raw} is not an http(s) origin`);
      return url.origin;
    });
}

/**
 * Content Security Policy for the packaged UI (not the dev server, which needs inline HMR code).
 * Scripts and fonts are local; data comes from the host bridge and the public hub API; images may be
 * remote game art (IGDB, Steam CDN) or local files on the assets virtual host.
 */
const CSP = [
  "default-src 'self'",
  "script-src 'self'",
  "style-src 'self' 'unsafe-inline'",
  "font-src 'self'",
  "img-src 'self' data: blob: https:",
  `connect-src 'self' ${[...new Set(HUB_ORIGINS)].join(' ')}`,
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
