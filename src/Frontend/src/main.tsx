import React from 'react';
import { createRoot, hydrateRoot } from 'react-dom/client';
import { BrowserRouter } from 'react-router-dom';

import { App } from './App';
import { loadSite } from './i18n';
import { languageOf, preferredLanguage } from './i18n/languages';

import './index.css';

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

const app = (
  <React.StrictMode>
    {/* navigating is a transition, so a page in a language whose words are
        still on their way stays on screen until they are there */}
    <BrowserRouter future={{ v7_startTransition: true }}>
      <App />
    </BrowserRouter>
  </React.StrictMode>
);

// the words of the page come first: the markup was rendered with them, and
// taking it over while they are still on their way would draw nothing instead
const language = languageOf(window.location.pathname) ?? preferredLanguage();

const start = () => {
  // the public pages arrive rendered, for whoever reads them without running
  // this (see prerender.tsx) - those are taken over rather than drawn again
  if (root.firstElementChild !== null) {
    hydrateRoot(root, app);
  } else {
    createRoot(root).render(app);
  }
};

// a catalog that fails to arrive is asked for again by the page that needs it
loadSite(language).then(start, start);
