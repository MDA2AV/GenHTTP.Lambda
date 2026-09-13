/**
 * The C# grammar Monaco ships has no declaration of its own - the package
 * publishes types for the editor API and not for the language definitions.
 */
declare module 'monaco-editor/esm/vs/basic-languages/csharp/csharp' {
  import type { languages } from 'monaco-editor/esm/vs/editor/editor.api';

  export const conf: languages.LanguageConfiguration;
  export const language: languages.IMonarchLanguage;
}
