import * as monaco from 'monaco-editor/esm/vs/editor/editor.api';

// a custom build: what the code view uses of the editor, not all of it
// (editor.all.js). Left out: the diff editor, which the history does not use
// (it colours each side itself, see Changes.tsx), and whatever needs a
// language service the editor does not have - code actions and lenses,
// formatting, inline completions and edits, inlay and parameter hints,
// rename, document symbols, go to symbol (go to definition is the platform's
// own action, see MonacoEditor.tsx), colour pickers, linked editing. Also
// font zoom, transposing, in-place replace, anchor selection, section
// headers, drop or paste into the editor and the tokenization inspector.
import 'monaco-editor/esm/vs/editor/browser/coreCommands.js';
import 'monaco-editor/esm/vs/editor/browser/widget/codeEditor/codeEditorWidget.js';
import 'monaco-editor/esm/vs/editor/contrib/bracketMatching/browser/bracketMatching.js';
import 'monaco-editor/esm/vs/editor/contrib/caretOperations/browser/caretOperations.js';
import 'monaco-editor/esm/vs/editor/contrib/clipboard/browser/clipboard.js';
import 'monaco-editor/esm/vs/editor/contrib/comment/browser/comment.js';
import 'monaco-editor/esm/vs/editor/contrib/contextmenu/browser/contextmenu.js';
import 'monaco-editor/esm/vs/editor/contrib/cursorUndo/browser/cursorUndo.js';
import 'monaco-editor/esm/vs/editor/contrib/dnd/browser/dnd.js';
import 'monaco-editor/esm/vs/editor/contrib/find/browser/findController.js';
import 'monaco-editor/esm/vs/editor/contrib/folding/browser/folding.js';
import 'monaco-editor/esm/vs/editor/contrib/gotoError/browser/gotoError.js';
import 'monaco-editor/esm/vs/editor/contrib/hover/browser/hoverContribution.js';
import 'monaco-editor/esm/vs/editor/contrib/indentation/browser/indentation.js';
import 'monaco-editor/esm/vs/editor/contrib/lineSelection/browser/lineSelection.js';
import 'monaco-editor/esm/vs/editor/contrib/linesOperations/browser/linesOperations.js';
import 'monaco-editor/esm/vs/editor/contrib/links/browser/links.js';
import 'monaco-editor/esm/vs/editor/contrib/longLinesHelper/browser/longLinesHelper.js';
import 'monaco-editor/esm/vs/editor/contrib/multicursor/browser/multicursor.js';
import 'monaco-editor/esm/vs/editor/contrib/placeholderText/browser/placeholderText.contribution.js';
import 'monaco-editor/esm/vs/editor/contrib/semanticTokens/browser/documentSemanticTokens.js';
import 'monaco-editor/esm/vs/editor/contrib/semanticTokens/browser/viewportSemanticTokens.js';
import 'monaco-editor/esm/vs/editor/contrib/smartSelect/browser/smartSelect.js';
import 'monaco-editor/esm/vs/editor/contrib/snippet/browser/snippetController2.js';
import 'monaco-editor/esm/vs/editor/contrib/stickyScroll/browser/stickyScrollContribution.js';
import 'monaco-editor/esm/vs/editor/contrib/suggest/browser/suggestController.js';
import 'monaco-editor/esm/vs/editor/contrib/toggleTabFocusMode/browser/toggleTabFocusMode.js';
import 'monaco-editor/esm/vs/editor/contrib/unicodeHighlighter/browser/unicodeHighlighter.js';
import 'monaco-editor/esm/vs/editor/contrib/unusualLineTerminators/browser/unusualLineTerminators.js';
import 'monaco-editor/esm/vs/editor/contrib/wordHighlighter/browser/wordHighlighter.js';
import 'monaco-editor/esm/vs/editor/contrib/wordOperations/browser/wordOperations.js';
import 'monaco-editor/esm/vs/editor/contrib/wordPartOperations/browser/wordPartOperations.js';
import 'monaco-editor/esm/vs/editor/contrib/readOnlyMessage/browser/contribution.js';
import 'monaco-editor/esm/vs/editor/common/standaloneStrings.js';
import 'monaco-editor/esm/vs/base/browser/ui/codicons/codiconStyles.js';
// only the C# grammar of the languages Monaco ships, and the highlighting below
import 'monaco-editor/esm/vs/basic-languages/csharp/csharp.contribution';

// the grammars a shipped asset is likely to be. Highlighting only - there is
// no language service behind these, and none is wanted: the compiler has an
// opinion about the C# and nothing has one about the rest.
import 'monaco-editor/esm/vs/basic-languages/html/html.contribution';
import 'monaco-editor/esm/vs/basic-languages/css/css.contribution';
import 'monaco-editor/esm/vs/basic-languages/javascript/javascript.contribution';
import 'monaco-editor/esm/vs/basic-languages/typescript/typescript.contribution';
// and what the documentation and the tests of a version are written in
import 'monaco-editor/esm/vs/basic-languages/markdown/markdown.contribution';
import 'monaco-editor/esm/vs/basic-languages/shell/shell.contribution';
// and what else the code of a version keeps: what a front end is built from, configuration
import 'monaco-editor/esm/vs/basic-languages/scss/scss.contribution';
import 'monaco-editor/esm/vs/basic-languages/less/less.contribution';
import 'monaco-editor/esm/vs/basic-languages/yaml/yaml.contribution';
import 'monaco-editor/esm/vs/basic-languages/ini/ini.contribution';
import 'monaco-editor/esm/vs/basic-languages/xml/xml.contribution';
import { language as csharp } from 'monaco-editor/esm/vs/basic-languages/csharp/csharp';

import EditorWorker from 'monaco-editor/esm/vs/editor/editor.worker?worker';

import type { Completion, Diagnostic } from '@genhttp-lambda/kit/api';

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
