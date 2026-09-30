import { Route, Routes } from 'react-router-dom';

import type { Theme } from '../theme';
import { Catalog } from './Catalog';
import { Project } from './Project';
import { SourceShell } from './SourceShell';

/**
 * The published sources: /source lists them, /source/{key} is one of them.
 *
 * A part of the site of its own, fetched with its words and its highlighter
 * only by whoever opens it, in a frame of its own - but in the site's
 * languages, under the same addresses per language as every other public
 * page, and drawn in the same colours.
 */
export function SourceApp({ theme, onToggleTheme }: { theme: Theme; onToggleTheme: () => void }) {
  return (
    <SourceShell theme={theme} onToggleTheme={onToggleTheme}>
      <Routes>
        <Route index element={<Catalog />} />
        <Route path=":key/*" element={<Project />} />
      </Routes>
    </SourceShell>
  );
}
