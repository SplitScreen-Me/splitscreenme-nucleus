import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath, URL } from 'node:url';
import { defineConfig, searchForWorkspaceRoot, type Plugin } from 'vite';
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
  // The supporters credits music (gui\audio, served by the host as https://assets.nucleus.example/).
  "media-src 'self' https://assets.nucleus.example",
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

const GUI_FOLDER = fileURLToPath(new URL('../NucleusCoopTool/gui/', import.meta.url));
const GUI_TYPES: Record<string, string> = { '.mp3': 'audio/mpeg', '.png': 'image/png', '.jpg': 'image/jpeg', '.jpeg': 'image/jpeg' };

/**
 * Dev server only: `/__gui/<path>` serves `NucleusCoopTool\gui\<path>`, what the host maps to
 * https://assets.nucleus.example/ (the mock's supporters music). Byte ranges are honoured, so audio can seek and loop.
 */
const devGuiAssets = (): Plugin => ({
  name: 'nucleus-dev-gui-assets',
  apply: 'serve',
  configureServer(server) {
    server.middlewares.use('/__gui/', (req, res, next) => {
      const relative = decodeURIComponent((req.url ?? '').split('?')[0] ?? '').replace(/^\/+/, '');
      const file = path.resolve(GUI_FOLDER, relative);
      const type = GUI_TYPES[path.extname(file).toLowerCase()];
      if (!type || !file.startsWith(GUI_FOLDER) || !fs.existsSync(file) || !fs.statSync(file).isFile()) return next();
      const size = fs.statSync(file).size;
      const range = /^bytes=(\d*)-(\d*)$/.exec(req.headers.range ?? '');
      res.setHeader('Content-Type', type);
      res.setHeader('Accept-Ranges', 'bytes');
      if (range && (range[1] || range[2])) {
        const start = range[1] ? Number(range[1]) : Math.max(0, size - Number(range[2]));
        const end = range[1] && range[2] ? Math.min(size - 1, Number(range[2])) : size - 1;
        if (start > end || start >= size) {
          res.statusCode = 416;
          res.setHeader('Content-Range', `bytes */${size}`);
          res.end();
          return;
        }
        res.statusCode = 206;
        res.setHeader('Content-Range', `bytes ${start}-${end}/${size}`);
        res.setHeader('Content-Length', String(end - start + 1));
        fs.createReadStream(file, { start, end }).pipe(res);
        return;
      }
      res.setHeader('Content-Length', String(size));
      fs.createReadStream(file).pipe(res);
    });
  },
});

// The build is served by WebView2 from a virtual host folder mapping (https://ui.nucleus.example/ → webui\):
// relative asset URLs, a single index.html, hash routing, no server-side fallback.
export default defineConfig({
  base: './',
  plugins: [react(), tailwindcss(), csp(), devGuiAssets()],
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
  // English is imported from Master/i18n/en.json, outside this folder: the dev server may serve it.
  server: { port: 5173, strictPort: true, fs: { allow: [searchForWorkspaceRoot(process.cwd()), fileURLToPath(new URL('../i18n', import.meta.url))] } },
  preview: { port: 4173, strictPort: true },
});
