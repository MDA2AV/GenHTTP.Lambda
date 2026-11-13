import type { ReactNode } from 'react';

/**
 * How a sentence marks up the words in it: a catalog says which words are
 * code or a link, and the page that draws it says what those look like. So
 * every language is styled alike, and the styling lives in one place.
 */
export interface Kit {
  /** Code in the middle of a sentence. */
  code: (text: string) => ReactNode;
  /** A word to find on the screen, like the name of a button. */
  b: (text: string) => ReactNode;
  em: (text: string) => ReactNode;
  /** A link within the site, around the words the language has for it. */
  link: (to: string, text: string) => ReactNode;
}
