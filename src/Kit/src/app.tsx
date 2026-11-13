import React, { Component, useEffect, type ReactNode } from 'react';
import { createRoot, hydrateRoot } from 'react-dom/client';
import { BrowserRouter, useLocation } from 'react-router-dom';

import { IconSpinner } from './components/Icons';
import { loadSite, useT } from './i18n';
import { languageOf, preferredLanguage } from './i18n/languages';

/*
 * What every bundle's application does the same way: start, recover from a
 * part of it that would not load, and put the view at the top of a new page.
 */

const RELOADED = 'lambda-reloaded-for-chunk';

/**
 * Starts a bundle's application in the page: once the words of its language
 * are there, it takes over the markup the server sent with a rendered page,
 * or draws it where there is none.
 */
export function boot(app: ReactNode): void {
  /*
   * The browser puts a reloaded page back where it was left, which for a page
   * that opens on a full screen heading means reloading into the middle of the
   * example and never seeing the top. The router decides where the view is here,
   * so the browser is told not to.
   */
  if ('scrollRestoration' in history) {
    history.scrollRestoration = 'manual';
  }

  const root = document.getElementById('root')!;

  const tree = (
    <React.StrictMode>
      {/* navigating is a transition, so a page in a language whose words are
          still on their way stays on screen until they are there */}
      <BrowserRouter future={{ v7_startTransition: true }}>{app}</BrowserRouter>
    </React.StrictMode>
  );

  // the words of the page come first: the markup was rendered with them, and
  // taking it over while they are still on their way would draw nothing instead
  const language = languageOf(window.location.pathname) ?? preferredLanguage();

  const start = () => {
    // the public pages arrive rendered, for whoever reads them without running
    // this (see prerender.tsx) - those are taken over rather than drawn again
    if (root.firstElementChild !== null) {
      hydrateRoot(root, tree);
    } else {
      createRoot(root).render(tree);
    }
  };

  // a catalog that fails to arrive is asked for again by the page that needs it
  loadSite(language).then(start, start);
}

/**
 * Loads a part of the application fetched only by whoever opens it, and
 * recovers from the one way that reliably fails: the application was deployed
 * again while this tab was open, so the index it was built from names a chunk
 * the server no longer has. Fetching the page again is all it takes, and the
 * flag keeps a genuinely broken build from turning that into a loop.
 */
export function recovering<T>(load: () => Promise<T>): Promise<T> {
  return load()
    .then((loaded) => {
      sessionStorage.removeItem(RELOADED);
      return loaded;
    })
    .catch((error: unknown) => {
      if (sessionStorage.getItem(RELOADED) === null) {
        sessionStorage.setItem(RELOADED, '1');
        window.location.reload();
      }

      throw error;
    });
}

/** Every route starts at the top, the way arriving at a page does. */
export function useScrollToTop(): void {
  const { pathname } = useLocation();

  useEffect(() => {
    window.scrollTo(0, 0);
  }, [pathname]);
}

/** While the editor is on its way. */
export function Loading() {
  const t = useT();

  return (
    <div className="flex flex-1 items-center justify-center gap-2 text-sm text-slate-500">
      <IconSpinner />
      {t.common.loadingEditor}
    </div>
  );
}

/**
 * Catches a part of the application that refuses to load. Without one the
 * failed import leaves the fallback on screen for good, which reads as a
 * spinner that never stops.
 */
export class ChunkBoundary extends Component<{ children: ReactNode; page?: boolean }, { failed: boolean }> {
  state = { failed: false };

  static getDerivedStateFromError() {
    return { failed: true };
  }

  render() {
    if (!this.state.failed) {
      return this.props.children;
    }

    return <ChunkFailure page={this.props.page} />;
  }
}

/** The editor, or a page of the published sources, that would not load. */
function ChunkFailure({ page = false }: { page?: boolean }) {
  const t = useT();

  return (
    <div className="mx-auto max-w-md px-5 py-20 text-center">
      <h1 className="text-lg font-semibold">{page ? t.common.pageFailed : t.common.editorFailed}</h1>
      <p className="mt-2 text-sm text-slate-600 dark:text-slate-400">{t.common.editorFailedWhy}</p>
      <button
        type="button"
        onClick={() => {
          sessionStorage.removeItem(RELOADED);
          window.location.reload();
        }}
        className="btn-primary mt-6"
      >
        {t.common.reload}
      </button>
    </div>
  );
}
