import type { MouseEvent, RefObject } from 'react';

/**
 * Rendered markdown on the page (see markdown.ts): a link to another page of
 * the version opens it where it is shown, and one to a heading of this page
 * scrolls there rather than going into the address, which is the page's own.
 *
 * Apart from the editor's Markdown, which colours code with Monaco, so the
 * page of a published source can show the same markup without downloading
 * the editor.
 */
export function MarkdownPage({ html, holder, onOpen }: {
  html: string;
  holder: RefObject<HTMLDivElement>;
  onOpen?: (name: string) => void;
}) {
  function click(event: MouseEvent<HTMLDivElement>) {
    const link = (event.target as HTMLElement).closest('a');

    if (!link) {
      return;
    }

    const page = link.getAttribute('data-page');

    if (page) {
      event.preventDefault();
      onOpen?.(page);
      return;
    }

    const anchor = link.getAttribute('href');

    if (anchor?.startsWith('#')) {
      event.preventDefault();
      holder.current?.querySelector(`[id="${CSS.escape(anchor.slice(1))}"]`)?.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
  }

  // eslint-disable-next-line jsx-a11y/click-events-have-key-events, jsx-a11y/no-static-element-interactions
  return <div ref={holder} className="markdown" onClick={click} dangerouslySetInnerHTML={{ __html: html }} />;
}
