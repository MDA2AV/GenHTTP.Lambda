import { useEffect, useRef } from 'react';

import type { Diagnostic } from '../api';
import { monaco, showDiagnostics } from '../monaco';
import type { Theme } from '../theme';

interface Props {
  value: string;
  language: string;
  theme: Theme;
  diagnostics: Diagnostic[];
  /**
   * Which document this is. Changing it swaps the document shown rather
   * than building a new editor, and each document keeps its own undo history,
   * caret and scroll position for when it is shown again.
   */
  path?: string;
  reveal?: { line: number; column: number; nonce: number };
  onChange?: (value: string) => void;
  onSave?: () => void;
  onDefinition?: (line: number, column: number) => void;
  /** Shown rather than edited - the files section reads code, it does not change it. */
  readOnly?: boolean;
}

const SINGLE = '\u0000single';

/**
 * A thin wrapper around Monaco.
 *
 * One editor for as long as the component lives, and one model per document
 * behind it. Building a new editor for every file meant a frame with nothing
 * in it and a layout pass on every switch, which is what made the page jump -
 * and threw away the undo history of the file that was left. `value` is only
 * pushed into the model when it differs, otherwise every keystroke would
 * reset the caret.
 */
export function CodeEditor({ value, language, theme, diagnostics, path, reveal, onChange, onSave, onDefinition, readOnly = false }: Props) {
  const host = useRef<HTMLDivElement>(null);
  const editor = useRef<monaco.editor.IStandaloneCodeEditor | null>(null);
  const save = useRef(onSave);
  const jump = useRef(onDefinition);
  const change = useRef(onChange);

  const models = useRef(new Map<string, monaco.editor.ITextModel>());
  const views = useRef(new Map<string, monaco.editor.ICodeEditorViewState | null>());
  const shown = useRef<string>('');

  save.current = onSave;
  jump.current = onDefinition;
  change.current = onChange;

  const document = path ?? SINGLE;

  /** The model of a document, made the first time it is shown. */
  function modelFor(key: string) {
    let model = models.current.get(key);

    if (!model || model.isDisposed()) {
      model = monaco.editor.createModel(value, language);
      models.current.set(key, model);
    }

    return model;
  }

  useEffect(() => {
    if (!host.current) {
      return;
    }

    shown.current = document;

    const instance = monaco.editor.create(host.current, {
      model: modelFor(document),
      theme: theme === 'dark' ? 'lambda-dark' : 'lambda-light',
      automaticLayout: true,
      minimap: { enabled: false },
      fontSize: 13.5,
      lineHeight: 21,
      fontFamily: '"Geist Mono", ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace',
      padding: { top: 14, bottom: 14 },
      scrollBeyondLastLine: false,
      smoothScrolling: true,
      renderLineHighlight: 'all',
      tabSize: 4,
      wordBasedSuggestions: 'currentDocument',
      suggestSelection: 'first',
      bracketPairColorization: { enabled: true },
      // the grammar colours every keystroke, the compiler refines it on a pause
      'semanticHighlighting.enabled': true,
      scrollbar: { verticalScrollbarSize: 10, horizontalScrollbarSize: 10 },
      readOnly,
      domReadOnly: readOnly,
    });

    editor.current = instance;

    const changed = instance.onDidChangeModelContent(() => change.current?.(instance.getValue()));

    instance.addCommand(monaco.KeyMod.CtrlCmd | monaco.KeyCode.KeyS, () => save.current?.());

    /*
     * Control-click, and F12, go to where a name was declared.
     *
     * Done by hand rather than through a definition provider: Monaco's own
     * navigation wants every file registered behind a URI it can open, and
     * the page that owns the files already knows how to switch between them.
     * Asking the server where the name was declared and switching through
     * that page does the same thing without pretending to be a workspace.
     */
    const clicked = instance.onMouseDown((event) => {
      const held = event.event.ctrlKey || event.event.metaKey;

      if (!held || !event.target.position || !jump.current) {
        return;
      }

      const word = instance.getModel()?.getWordAtPosition(event.target.position);

      if (word) {
        jump.current(event.target.position.lineNumber - 1, word.startColumn - 1);
      }
    });

    instance.addAction({
      id: 'lambda.goToDefinition',
      label: 'Go to definition',
      keybindings: [monaco.KeyCode.F12],
      contextMenuGroupId: 'navigation',
      run: (target) => {
        const at = target.getPosition();

        const word = at && target.getModel()?.getWordAtPosition(at);

        if (at && word && jump.current) {
          jump.current(at.lineNumber - 1, word.startColumn - 1);
        }
      },
    });

    const held = models.current;

    return () => {
      clicked.dispose();
      changed.dispose();
      instance.dispose();
      editor.current = null;

      for (const model of held.values()) {
        model.dispose();
      }

      held.clear();
    };
    // the editor is created once and driven through its own API afterwards
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // another document: the one being left remembers where it was, the one
  // being shown is put back where it was left
  useEffect(() => {
    const instance = editor.current;

    if (!instance || shown.current === document) {
      return;
    }

    views.current.set(shown.current, instance.saveViewState());

    instance.setModel(modelFor(document));
    instance.restoreViewState(views.current.get(document) ?? null);

    shown.current = document;
    // modelFor reads the props of this render, which is the point
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [document]);

  useEffect(() => {
    const instance = editor.current;

    if (instance && instance.getValue() !== value) {
      instance.setValue(value);
    }
  }, [value, document]);

  useEffect(() => {
    monaco.editor.setTheme(theme === 'dark' ? 'lambda-dark' : 'lambda-light');
  }, [theme]);

  useEffect(() => {
    const model = editor.current?.getModel();

    if (model && model.getLanguageId() !== language) {
      monaco.editor.setModelLanguage(model, language);
    }
  }, [language, document]);

  useEffect(() => {
    const model = editor.current?.getModel();

    if (model) {
      showDiagnostics(model, diagnostics);
    }
  }, [diagnostics, document]);

  useEffect(() => {
    if (!reveal || !editor.current) {
      return;
    }

    editor.current.revealLineInCenter(reveal.line);
    editor.current.setPosition({ lineNumber: reveal.line, column: reveal.column });
    editor.current.focus();
  }, [reveal]);

  return <div ref={host} className="h-full w-full" />;
}
