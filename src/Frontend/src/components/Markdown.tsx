import { useEffect, useMemo, useRef } from 'react';

import type { LambdaFile } from '../api';
import { encodeBytes } from '../bytes';
import { pictureType, renderMarkdown } from '../markdown';
import { monaco } from '../monaco';
import { MarkdownPage } from './MarkdownPage';
import type { Theme } from '../theme';

/**
 * A page of markdown in the editor, as the documentation and the tests of a
 * version are written (see markdown.ts for how it is rendered, and why raw
 * HTML is not).
 *
 * The editor has the files of the version at hand, so a relative link to
 * another page opens that page here, and a relative picture - a diagram
 * beside the page - is shown from the version rather than fetched. Code blocks
 * are coloured by the editor's own grammars.
 */
export function Markdown({ source, name, files, theme, onOpen }: {
  source: string;
  /** The page's own name, which relative links and pictures are resolved against. */
  name: string;
  /** The files of the version, where a relative picture or page is looked up. */
  files: LambdaFile[];
  theme: Theme;
  /** Opens another page of the version, when a link leads to one. */
  onOpen?: (name: string) => void;
}) {
  const holder = useRef<HTMLDivElement>(null);

  const html = useMemo(
    () =>
      renderMarkdown(source, {
        name,
        page: (target) => files.some((file) => file.name === target && file.encoding !== 'base64'),
        picture: (target) => {
          const file = files.find((f) => f.name === target);
          const type = pictureType(target);

          if (!file || !type) {
            return null;
          }

          const base64 = file.encoding === 'base64' ? file.code : encodeBytes(new TextEncoder().encode(file.code));

          return `data:${type};base64,${base64}`;
        },
      }),
    [source, name, files],
  );

  // code blocks coloured by the editor's own grammars once they are on the page
  useEffect(() => {
    const blocks = holder.current?.querySelectorAll<HTMLElement>('pre > code[data-language]') ?? [];

    let alive = true;

    monaco.editor.setTheme(theme === 'dark' ? 'lambda-dark' : 'lambda-light');

    blocks.forEach((block) => {
      const text = block.textContent ?? '';

      monaco.editor
        .colorize(text, block.dataset.language!, { tabSize: 4 })
        // monaco escapes the source as it renders it
        .then((coloured) => alive && (block.innerHTML = coloured))
        .catch(() => undefined);
    });

    return () => {
      alive = false;
    };
  }, [html, theme]);

  return <MarkdownPage html={html} holder={holder} onOpen={onOpen} />;
}
