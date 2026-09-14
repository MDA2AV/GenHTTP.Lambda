import * as monaco from 'monaco-editor/esm/vs/editor/editor.api';

// a custom build: the full editor, but only the C# grammar
import 'monaco-editor/esm/vs/editor/editor.all.js';
import 'monaco-editor/esm/vs/basic-languages/csharp/csharp.contribution';

// the grammars a shipped asset is likely to be. Highlighting only - there is
// no language service behind these, and none is wanted: the compiler has an
// opinion about the C# and nothing has one about the rest.
import 'monaco-editor/esm/vs/basic-languages/html/html.contribution';
import 'monaco-editor/esm/vs/basic-languages/css/css.contribution';
import 'monaco-editor/esm/vs/basic-languages/javascript/javascript.contribution';
import 'monaco-editor/esm/vs/basic-languages/typescript/typescript.contribution';
import { language as csharp } from 'monaco-editor/esm/vs/basic-languages/csharp/csharp';

import EditorWorker from 'monaco-editor/esm/vs/editor/editor.worker?worker';

import type { Completion, Diagnostic } from './api';

/**
 * Which grammar a file is written in, by what it is called.
 *
 * Anything unrecognised is plain text rather than a guess: colouring markup as
 * if it were code is worse than not colouring it.
 */
const GRAMMARS: Record<string, string> = {
  cs: 'csharp',
  html: 'html',
  htm: 'html',
  css: 'css',
  js: 'javascript',
  mjs: 'javascript',
  ts: 'typescript',
  json: 'json',
  svg: 'html',
  txt: 'plaintext',
  md: 'plaintext',
};

export function languageFor(name: string): string {
  const dot = name.lastIndexOf('.');

  return (dot < 0 ? '' : GRAMMARS[name.slice(dot + 1).toLowerCase()]) ?? 'plaintext';
}

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
    // Visual Studio's dark defaults, which is what C# is expected to look like
    { token: 'comment', foreground: '57A64A', fontStyle: 'italic' },
    { token: 'string', foreground: 'D69D85' },
    { token: 'keyword', foreground: '569CD6' },
    { token: 'number', foreground: 'B5CEA8' },
    { token: 'type', foreground: '4EC9B0' },
    { token: 'interface', foreground: 'B8D7A3' },
    { token: 'struct', foreground: '86C691' },
    { token: 'enum', foreground: 'B8D7A3' },
    { token: 'method', foreground: 'DCDCAA' },
    { token: 'member.call', foreground: 'DCDCAA' },
    { token: 'property', foreground: 'DCDCAA' },
    { token: 'member', foreground: 'DCDCAA' },
    { token: 'parameter', foreground: '9CDCFE' },
    { token: 'local', foreground: '9CDCFE' },
    { token: 'namespace', foreground: 'D4D4D4' },
    { token: 'identifier', foreground: 'D4D4D4' },
  ],
  colors: {
    'editor.background': '#202124',
    'editor.foreground': '#D4D4D4',
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
    // Visual Studio's light defaults
    { token: 'comment', foreground: '008000', fontStyle: 'italic' },
    { token: 'string', foreground: 'A31515' },
    { token: 'keyword', foreground: '0000FF' },
    { token: 'number', foreground: '098658' },
    { token: 'type', foreground: '2B91AF' },
    { token: 'interface', foreground: '2B91AF' },
    { token: 'struct', foreground: '2B91AF' },
    { token: 'enum', foreground: '2B91AF' },
    { token: 'method', foreground: '74531F' },
    { token: 'member.call', foreground: '74531F' },
    { token: 'property', foreground: '74531F' },
    { token: 'member', foreground: '74531F' },
    { token: 'parameter', foreground: '1F377F' },
    { token: 'local', foreground: '1F377F' },
    { token: 'namespace', foreground: '000000' },
    { token: 'identifier', foreground: '000000' },
  ],
  colors: {
    'editor.background': '#ffffff',
    'editor.foreground': '#000000',
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
let resolver: monaco.IDisposable | null = null;

const KINDS_TO_MONACO: Record<string, monaco.languages.CompletionItemKind> = {
  class: monaco.languages.CompletionItemKind.Class,
  interface: monaco.languages.CompletionItemKind.Interface,
  struct: monaco.languages.CompletionItemKind.Struct,
  enum: monaco.languages.CompletionItemKind.Enum,
  method: monaco.languages.CompletionItemKind.Method,
  property: monaco.languages.CompletionItemKind.Property,
  field: monaco.languages.CompletionItemKind.Field,
  parameter: monaco.languages.CompletionItemKind.Variable,
  variable: monaco.languages.CompletionItemKind.Variable,
  text: monaco.languages.CompletionItemKind.Text,
};

/**
 * Asks the compiler what may be written where the caret is.
 *
 * The catalogue below offers the same few hundred names wherever you are,
 * which cannot answer the question people actually have - what comes after
 * this dot. That needs the type of what precedes it, which needs a compiler,
 * and there is one on the other end of this call.
 */
export function registerResolver(
  resolve: (code: string, line: number, column: number) => Promise<ResolvedCompletionData[]>,
): void {
  resolver?.dispose();

  resolver = monaco.languages.registerCompletionItemProvider('csharp', {
    triggerCharacters: ['.'],

    provideCompletionItems: async (model, position) => {
      const word = model.getWordUntilPosition(position);

      const range: monaco.IRange = {
        startLineNumber: position.lineNumber,
        endLineNumber: position.lineNumber,
        startColumn: word.startColumn,
        endColumn: word.endColumn,
      };

      let found: ResolvedCompletionData[];

      try {
        // Monaco counts from one, the compiler from zero
        found = await resolve(model.getValue(), position.lineNumber - 1, position.column - 1);
      } catch {
        // the catalogue is still registered and answers on its own
        return { suggestions: [] };
      }

      return {
        suggestions: found.map((item) => ({
          label: item.label,
          kind: KINDS_TO_MONACO[item.kind] ?? monaco.languages.CompletionItemKind.Text,
          detail: item.detail,
          documentation: item.documentation,
          insertText: item.label,
          range,
        })),
      };
    },
  });
}

export interface ResolvedCompletionData {
  label: string;
  kind: string;
  detail: string;
  documentation?: string;
}

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

/**
 * The kinds the server reports, in the order the encoded stream refers to them
 * by. Names match the theme rules, so a token is coloured by what it is.
 */
const KINDS = ['type', 'interface', 'struct', 'enum', 'method', 'property', 'parameter', 'local', 'namespace', 'enumMember'];

let semantics: monaco.IDisposable | null = null;

/**
 * Colours the editor from what the compiler resolved rather than from what the
 * words look like.
 *
 * The grammar in the browser runs on every keystroke and guesses; this runs on
 * a pause and knows. Monaco layers the two, so there is no moment where the
 * code is uncoloured while the server thinks.
 */
export function registerSemantics(classify: (code: string) => Promise<SemanticTokenData[]>): void {
  semantics?.dispose();

  semantics = monaco.languages.registerDocumentSemanticTokensProvider('csharp', {
    getLegend: () => ({ tokenTypes: KINDS, tokenModifiers: [] }),

    provideDocumentSemanticTokens: async (model) => {
      let tokens: SemanticTokenData[];

      try {
        tokens = await classify(model.getValue());
      } catch {
        // the editor keeps whatever the grammar gave it
        return { data: new Uint32Array() };
      }

      const data: number[] = [];

      let line = 0;
      let column = 0;

      for (const token of tokens) {
        const index = KINDS.indexOf(token.kind);

        if (index < 0) {
          continue;
        }

        // the stream is relative: each token is described as an offset from
        // the one before it, and the column restarts on a new line
        data.push(token.line - line, token.line === line ? token.column - column : token.column,
                  token.length, index, 0);

        line = token.line;
        column = token.column;
      }

      return { data: new Uint32Array(data) };
    },

    releaseDocumentSemanticTokens: () => undefined,
  });
}

export interface SemanticTokenData {
  line: number;
  column: number;
  length: number;
  kind: string;
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
