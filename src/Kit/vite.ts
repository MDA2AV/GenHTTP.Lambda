import { cpSync, existsSync, readFileSync, readdirSync } from 'node:fs';
import path from 'node:path';

import react from '@vitejs/plugin-react';
import type { Plugin, ProxyOptions, UserConfig } from 'vite';

/*
 * How every bundle is built, so that each one's vite.config.ts says only what
 * is its own: the path it is served below and the folder it is built into.
 */

const KIT = import.meta.dirname;

/** Where the server under development answers, for whatever a bundle does not serve itself. */
const SERVER = process.env.LAMBDA_API ?? 'http://localhost:8080';

export interface BundleOptions {
  /** The folder of the bundle's frontend - its vite.config.ts's import.meta.dirname. */
  root: string;
  /** The path the component serves the bundle below: "/editor/", or "/" for a site. */
  base: string;
  /** Where it is built to, which the component copies into the host's output. */
  outDir: string;
  /** Whether the kit's pictures for link previews go along, for pages meant to be shared. */
  social?: boolean;
}

/**
 * Puts the bundle's table of pages next to its index page, where the server
 * reads it to name each page before sending it and to write the sitemap. One
 * file for both sides, so the title a crawler sees is the one the tab shows.
 */
function pageTable(root: string): Plugin {
  const table = path.join(root, 'src', 'pages.json');

  return {
    name: 'genhttp-lambda-page-table',
    apply: 'build',
    generateBundle() {
      if (existsSync(table)) {
        this.emitFile({ type: 'asset', fileName: 'pages.json', source: readFileSync(table, 'utf-8') });
      }
    },
  };
}

/**
 * Copies the kit's public files - the icons, and the pictures for link
 * previews where they are wanted - into the bundle, so every component serves
 * the icons its pages name from its own path.
 */
function kitFiles(outDir: string, social: boolean): Plugin {
  return {
    name: 'genhttp-lambda-kit-files',
    apply: 'build',
    writeBundle() {
      const from = path.join(KIT, 'public');

      for (const entry of readdirSync(from)) {
        if (entry === 'social' && !social) {
          continue;
        }

        cpSync(path.join(from, entry), path.join(outDir, entry), { recursive: true });
      }
    },
  };
}

/** The configuration of a bundle. */
export function bundle({ root, base, outDir, social = false }: BundleOptions) {
  return ({ isSsrBuild }: { isSsrBuild?: boolean }): UserConfig => {
    const out = path.resolve(root, outDir);

    // what the bundle does not serve itself goes to the server - the other
    // components' pages included, so a link between them works as it will
    const proxied: Record<string, ProxyOptions> = {};

    for (const prefix of ['/api', '/mcp', '/editor', '/admin', '/source', '/assets', '/lambda', '/features']) {
      if (!base.startsWith(prefix)) {
        // ws: lambdas open websockets too, and without it the upgrade is never
        // passed on - the browser waits in CONNECTING for ever
        proxied[prefix] = { target: SERVER, changeOrigin: true, ws: prefix === '/lambda' || prefix === '/features' };
      }
    }

    return {
      root,
      base,
      plugins: [react(), ...(isSsrBuild ? [] : [kitFiles(out, social), pageTable(root)])],
      resolve: {
        alias: { '@genhttp-lambda/kit': path.join(KIT, 'src') },
      },
      build: {
        outDir: out,
        emptyOutDir: true,
        chunkSizeWarningLimit: 4096,
        // rendering the pages for crawlers needs no copy of the pictures
        ...(isSsrBuild ? { copyPublicDir: false } : {}),
      },
      publicDir: existsSync(path.join(root, 'public')) ? path.join(root, 'public') : false,
      server: {
        proxy: proxied,
      },
    };
  };
}
