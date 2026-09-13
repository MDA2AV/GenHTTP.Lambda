import { useEffect, useRef } from 'react';

import type { Diagnostic } from '../api';
import { monaco, showDiagnostics } from '../monaco';
import type { Theme } from '../theme';

interface Props {
  value: string;
  theme: Theme;
  diagnostics: Diagnostic[];
  reveal?: { line: number; column: number; nonce: number };
  onChange: (value: string) => void;
  onSave: () => void;
}

/**
 * A thin wrapper around Monaco. The editor keeps its own model, so `value` is
 * only pushed in when it differs - otherwise every keystroke would reset the
 * cursor.
 */
export function CodeEditor({ value, theme, diagnostics, reveal, onChange, onSave }: Props) {
  const host = useRef<HTMLDivElement>(null);
  const editor = useRef<monaco.editor.IStandaloneCodeEditor | null>(null);
  const save = useRef(onSave);

  save.current = onSave;

  useEffect(() => {
    if (!host.current) {
      return;
    }

    const instance = monaco.editor.create(host.current, {
      value,
      language: 'csharp',
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

    return () => {
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
