import * as monaco from 'monaco-editor/esm/vs/editor/editor.api';

// a custom build: the full editor, but only the C# grammar
import 'monaco-editor/esm/vs/editor/editor.all.js';
import 'monaco-editor/esm/vs/basic-languages/csharp/csharp.contribution';
import { language as csharp } from 'monaco-editor/esm/vs/basic-languages/csharp/csharp';

import EditorWorker from 'monaco-editor/esm/vs/editor/editor.worker?worker';

import type { Completion, Diagnostic } from './api';

self.MonacoEnvironment = {
  getWorker: () => new EditorWorker(),
};

/**
 * Replaces the shipped C# grammar with one that tells a type from a member.
 *
 * Inline rather than imported from its own module: that module imports this
 * one back, and a cycle between them leaves whichever loads second holding an
 * undefined monaco.
 */
function registerGrammar(): void {
  const language = structuredClone(csharp);

  const tokenizer = language.tokenizer as Record<string, unknown[]>;

  tokenizer.root = [
    // a capitalised word not reached through a dot: a type name
    [/@?[A-Z]\w*/, { token: 'type', next: '@qualified' }],
    ...tokenizer.root,
  ];

  tokenizer.qualified = [
    // after a dot and followed by a bracket: a method being called
    [/[a-zA-Z_]\w*(?=\s*\()/, { token: 'member.call' }],

    // after a dot, capitalised: a property or a nested type
    [/[A-Z]\w*/, { token: 'member' }],

    ...tokenizer.qualified,
  ];

  monaco.languages.setMonarchTokensProvider('csharp', language);
}

// syntax colours are the four Google brand hues in the tints that hold their
// contrast on a dark surface, so the editor belongs to the same palette
registerGrammar();

monaco.editor.defineTheme('lambda-dark', {
  base: 'vs-dark',
  inherit: true,
  rules: [
    { token: 'comment', foreground: '9aa0a6', fontStyle: 'italic' },
    { token: 'string', foreground: '81c995' },
    { token: 'keyword', foreground: '8ab4f8' },
    { token: 'number', foreground: 'fdd663' },
    { token: 'type', foreground: '78d9ec' },
    { token: 'member', foreground: 'd7aefb' },
    { token: 'member.call', foreground: 'fdd663' },
    { token: 'identifier', foreground: 'e8eaed' },
    // what the semantic pass adds once the server has looked at the code
    { token: 'parameter', foreground: 'f6aea9' },
    { token: 'local', foreground: 'e8eaed' },
    { token: 'method', foreground: 'fdd663' },
    { token: 'property', foreground: 'd7aefb' },
  ],
  colors: {
    'editor.background': '#202124',
    'editor.foreground': '#e8eaed',
    'editor.lineHighlightBackground': '#292a2d',
    'editorLineNumber.foreground': '#5f6368',
    'editorLineNumber.activeForeground': '#9aa0a6',
    'editorGutter.background': '#202124',
    'editorIndentGuide.background1': '#3c4043',
    'editorWidget.background': '#292a2d',
    'editorWidget.border': '#3c4043',
    'editorSuggestWidget.background': '#292a2d',
    'editorSuggestWidget.border': '#3c4043',
    'editorSuggestWidget.selectedBackground': '#3c4043',
    'editorCursor.foreground': '#8ab4f8',
    'editor.selectionBackground': '#1a73e855',
    'scrollbarSlider.background': '#3c404380',
  },
});

monaco.editor.defineTheme('lambda-light', {
  base: 'vs',
  inherit: true,
  rules: [
    { token: 'comment', foreground: '5f6368', fontStyle: 'italic' },
    { token: 'string', foreground: '188038' },
    { token: 'keyword', foreground: '1a73e8' },
    { token: 'number', foreground: 'e37400' },
    { token: 'type', foreground: '129eaf' },
    { token: 'member', foreground: '8430ce' },
    { token: 'member.call', foreground: 'b06000' },
    { token: 'identifier', foreground: '202124' },
    { token: 'parameter', foreground: 'a50e0e' },
    { token: 'local', foreground: '202124' },
    { token: 'method', foreground: 'b06000' },
    { token: 'property', foreground: '8430ce' },
  ],
  colors: {
    'editor.background': '#ffffff',
    'editor.foreground': '#202124',
    'editor.lineHighlightBackground': '#f8f9fa',
    'editorLineNumber.foreground': '#bdc1c6',
    'editorLineNumber.activeForeground': '#5f6368',
    'editorWidget.border': '#dadce0',
    'editorSuggestWidget.border': '#dadce0',
    'editorCursor.foreground': '#1a73e8',
    'editor.selectionBackground': '#1a73e833',
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
