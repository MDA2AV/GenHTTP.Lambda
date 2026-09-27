import React from 'react';
import { createRoot, hydrateRoot } from 'react-dom/client';
import { BrowserRouter } from 'react-router-dom';

import { App } from './App';

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
    <BrowserRouter>
      <App />
    </BrowserRouter>
  </React.StrictMode>
);

// the public pages arrive rendered, for whoever reads them without running
// this (see prerender.tsx) - those are taken over rather than drawn again
if (root.firstElementChild !== null) {
  hydrateRoot(root, app);
} else {
  createRoot(root).render(app);
}
