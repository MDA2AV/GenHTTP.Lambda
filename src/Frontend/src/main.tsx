import React from 'react';
import { createRoot } from 'react-dom/client';
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

createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <BrowserRouter>
      <App />
    </BrowserRouter>
  </React.StrictMode>,
);
