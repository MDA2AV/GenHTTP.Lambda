import { recovering } from '@genhttp-lambda/kit/app';

import type * as Monaco from './monaco';

/**
 * Monaco, loaded the first time something shows code.
 *
 * It is most of what the editor weighs - several megabytes of the bundle -
 * and most of the editor shows no code at all: the simple view never does,
 * the full view only in the code, files and history sections, and a page of
 * documentation only where it has a code block. So nothing imports
 * `monaco.ts` but through here, and what only names a grammar reads
 * `grammars.ts` instead.
 */
type MonacoModule = typeof Monaco;

let loading: Promise<MonacoModule> | null = null;

const waiting: ((module: MonacoModule) => void)[] = [];

/**
 * Loads Monaco, once, and hands it to whatever waited for it before it is
 * handed to the caller - so the completions a page registered early are
 * there by the time the first editor opens.
 */
export function loadMonaco(): Promise<MonacoModule> {
  loading ??= recovering(() => import('./monaco')).then((module) => {
    for (const action of waiting.splice(0)) {
      action(module);
    }

    return module;
  });

  return loading;
}

/**
 * Does something with Monaco once something else has loaded it, without
 * loading it: what the page tells the editor - its completions, how to colour
 * and complete this lambda's code - waits until an editor is shown.
 */
export function whenMonaco(action: (module: MonacoModule) => void): void {
  if (loading) {
    void loading.then(action);
  } else {
    waiting.push(action);
  }
}
