import { Suspense, lazy } from 'react';

import { recovering } from '@genhttp-lambda/kit/app';

import { loadMonaco } from '../monacoLoader';
import type { CodeEditorProps } from './MonacoEditor';

// Monaco and the editor around it, the first time code is shown
const Monaco = lazy(() =>
  loadMonaco()
    // a chunk of its own as well, which a deploy since the page opened may have replaced
    .then(() => recovering(() => import('./MonacoEditor')))
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
