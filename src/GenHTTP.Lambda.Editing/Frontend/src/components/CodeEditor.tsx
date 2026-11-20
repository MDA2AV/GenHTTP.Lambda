import { Suspense, lazy } from 'react';

import { loadMonaco } from '../monacoLoader';
import type { CodeEditorProps } from './MonacoEditor';

// Monaco and the editor around it, the first time code is shown
const Monaco = lazy(() =>
  loadMonaco()
    .then(() => import('./MonacoEditor'))
    .then((module) => ({ default: module.MonacoEditor })),
);

/**
 * The code editor, which loads Monaco the first time one is shown: most of
 * the editor's pages show no code, and need not download it (see
 * monacoLoader.ts). Until it is there, the space it will take stays empty,
 * so nothing moves when it arrives.
 */
export function CodeEditor(props: CodeEditorProps) {
  return (
    <Suspense fallback={<div className="h-full w-full" />}>
      <Monaco {...props} />
    </Suspense>
  );
}
