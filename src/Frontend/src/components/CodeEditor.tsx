import { useEffect, useRef } from 'react';

import type { Diagnostic } from '../api';
import { monaco, showDiagnostics } from '../monaco';
import type { Theme } from '../theme';

interface Props {
  value: string;
  language: string;
  theme: Theme;
  diagnostics: Diagnostic[];
  reveal?: { line: number; column: number; nonce: number };
  onChange: (value: string) => void;
  onSave: () => void;
  onDefinition?: (line: number, column: number) => void;
}

/**
 * A thin wrapper around Monaco. The editor keeps its own model, so `value` is
 * only pushed in when it differs - otherwise every keystroke would reset the
 * cursor.
 */
export function CodeEditor({ value, language, theme, diagnostics, reveal, onChange, onSave, onDefinition }: Props) {
  const host = useRef<HTMLDivElement>(null);
  const editor = useRef<monaco.editor.IStandaloneCodeEditor | null>(null);
  const save = useRef(onSave);
  const jump = useRef(onDefinition);

  save.current = onSave;
  jump.current = onDefinition;

  useEffect(() => {
    if (!host.current) {
      return;
    }

    const instance = monaco.editor.create(host.current, {
      value,
      language,
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
    });

    editor.current = instance;

    const changed = instance.onDidChangeModelContent(() => onChange(instance.getValue()));

    instance.addCommand(monaco.KeyMod.CtrlCmd | monaco.KeyCode.KeyS, () => save.current());

    /*
     * Control-click, and F12, go to where a name was declared.
     *
     * Done by hand rather than through a definition provider because the
     * editor keeps one model and swaps it as the file changes: Monaco's own
     * navigation wants a model per file behind a URI, and there is no second
     * model for it to open. Asking the server where the name was declared and
     * then switching files through the page that owns them does the same
     * thing without pretending to be a workspace.
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

    return () => {
      clicked.dispose();
      changed.dispose();
      instance.dispose();
      editor.current = null;
    };
    // the editor is created once and driven through its own API afterwards
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    const instance = editor.current;

    if (instance && instance.getValue() !== value) {
      instance.setValue(value);
    }
  }, [value]);

  useEffect(() => {
    monaco.editor.setTheme(theme === 'dark' ? 'lambda-dark' : 'lambda-light');
  }, [theme]);

  // one model outlives a change of file, so the grammar has to be moved with
  // it or a page of markup goes on being coloured as if it were C#
  useEffect(() => {
    const model = editor.current?.getModel();

    if (model) {
      monaco.editor.setModelLanguage(model, language);
    }
  }, [language]);

  useEffect(() => {
    const model = editor.current?.getModel();

    if (model) {
      showDiagnostics(model, diagnostics);
    }
  }, [diagnostics]);

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
