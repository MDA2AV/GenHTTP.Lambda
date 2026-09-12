import * as monaco from 'monaco-editor/esm/vs/editor/editor.api';

// a custom build: the full editor, but only the C# grammar
import 'monaco-editor/esm/vs/editor/editor.all.js';
import 'monaco-editor/esm/vs/basic-languages/csharp/csharp.contribution';

import EditorWorker from 'monaco-editor/esm/vs/editor/editor.worker?worker';

import type { Completion, Diagnostic } from './api';

self.MonacoEnvironment = {
  getWorker: () => new EditorWorker(),
};

monaco.editor.defineTheme('lambda-dark', {
  base: 'vs-dark',
  inherit: true,
  rules: [
    { token: 'comment', foreground: '6b7a90', fontStyle: 'italic' },
    { token: 'string', foreground: 'b8e4a0' },
    { token: 'keyword', foreground: '8ab4f8' },
    { token: 'number', foreground: 'f0b47a' },
    { token: 'type', foreground: '7fd3c8' },
  ],
  colors: {
    'editor.background': '#10141c',
    'editor.lineHighlightBackground': '#151a24',
    'editorLineNumber.foreground': '#3a4658',
    'editorLineNumber.activeForeground': '#7b8ba3',
    'editorGutter.background': '#10141c',
    'editorIndentGuide.background1': '#1b2230',
    'editorWidget.background': '#151a24',
    'editorWidget.border': '#273040',
    'editorSuggestWidget.background': '#151a24',
    'editorSuggestWidget.border': '#273040',
    'editorSuggestWidget.selectedBackground': '#273040',
    'scrollbarSlider.background': '#27304080',
  },
});

monaco.editor.defineTheme('lambda-light', {
  base: 'vs',
  inherit: true,
  rules: [{ token: 'comment', foreground: '64748b', fontStyle: 'italic' }],
  colors: {
    'editor.background': '#ffffff',
    'editor.lineHighlightBackground': '#f6f8fb',
  },
});

const kinds: Record<string, monaco.languages.CompletionItemKind> = {
  class: monaco.languages.CompletionItemKind.Class,
  interface: monaco.languages.CompletionItemKind.Interface,
  enum: monaco.languages.CompletionItemKind.Enum,
  struct: monaco.languages.CompletionItemKind.Struct,
  attribute: monaco.languages.CompletionItemKind.Property,
  snippet: monaco.languages.CompletionItemKind.Snippet,
};

let provider: monaco.IDisposable | null = null;

/**
 * Teaches the editor the vocabulary of the platform: every type a lambda can
 * use without a using statement, plus the snippets the server suggests.
 */
export function registerCompletions(completions: Completion[]): void {
  provider?.dispose();

  provider = monaco.languages.registerCompletionItemProvider('csharp', {
    provideCompletionItems: (model, position) => {
      const word = model.getWordUntilPosition(position);

      const range: monaco.IRange = {
        startLineNumber: position.lineNumber,
        endLineNumber: position.lineNumber,
        startColumn: word.startColumn,
        endColumn: word.endColumn,
      };

      return {
        suggestions: completions.map((item) => ({
          label: item.label,
          kind: kinds[item.kind] ?? monaco.languages.CompletionItemKind.Text,
          detail: item.detail,
          insertText: item.insert ? escape(item.insert) : item.label,
          insertTextRules: item.insert
            ? monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet
            : undefined,
          range,
        })),
      };
    },
  });
}

/** Shows the messages of the last build right where they happened. */
export function showDiagnostics(model: monaco.editor.ITextModel, diagnostics: Diagnostic[]): void {
  const markers = diagnostics
    .filter((d) => d.line > 0)
    .map((d) => ({
      severity:
        d.severity === 'Warning' ? monaco.MarkerSeverity.Warning : monaco.MarkerSeverity.Error,
      message: `${d.id}: ${d.message}`,
      startLineNumber: d.line,
      startColumn: d.column,
      endLineNumber: d.line,
      endColumn: model.getLineMaxColumn(Math.min(d.line, model.getLineCount())),
    }));

  monaco.editor.setModelMarkers(model, 'lambda', markers);
}

const escape = (text: string) => text.replace(/\\/g, '\\\\').replace(/\$/g, '\\$');

export { monaco };
