import MarkdownIt from 'markdown-it';
import { useEffect, useMemo, useRef, type MouseEvent } from 'react';

import type { LambdaFile } from '../api';
import { encodeBytes } from '../bytes';
import { monaco } from '../monaco';
import type { Theme } from '../theme';

/**
 * A page of markdown, as the documentation and the tests of a version are
 * written.
 *
 * Rendered with markdown-it with raw HTML off, which is its default and the
 * point: the pages are written by agents and shown in the editor, whose
 * address is the key to the lambda. What would be markup is shown as text,
 * and a link to anything but a page, an address or mail is not a link.
 *
 * A page lives among the other files of its version, so a relative link to
 * another page opens that page here, and a relative picture - a diagram
 * beside the page - is shown from the version rather than fetched.
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

  const html = useMemo(() => render(source, name, files), [source, name, files]);

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

    // within the page: scrolled to rather than put into the address, which is the editor's
    if (anchor?.startsWith('#')) {
      event.preventDefault();
      holder.current?.querySelector(`[id="${CSS.escape(anchor.slice(1))}"]`)?.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
  }

  // eslint-disable-next-line jsx-a11y/click-events-have-key-events, jsx-a11y/no-static-element-interactions
  return <div ref={holder} className="markdown" onClick={click} dangerouslySetInnerHTML={{ __html: html }} />;
}

/** The grammars a code block may name, as the editor knows them. */
const LANGUAGES: Record<string, string> = {
  cs: 'csharp',
  csharp: 'csharp',
  'c#': 'csharp',
  js: 'javascript',
  mjs: 'javascript',
  javascript: 'javascript',
  ts: 'typescript',
  typescript: 'typescript',
  html: 'html',
  xml: 'html',
  css: 'css',
  json: 'json',
  sh: 'shell',
  bash: 'shell',
  shell: 'shell',
  md: 'markdown',
  markdown: 'markdown',
};

const PICTURES: Record<string, string> = {
  png: 'image/png',
  jpg: 'image/jpeg',
  jpeg: 'image/jpeg',
  gif: 'image/gif',
  webp: 'image/webp',
  svg: 'image/svg+xml',
  avif: 'image/avif',
};

interface Env {
  name: string;
  files: LambdaFile[];
  slugs: Map<string, number>;
}

const parser = new MarkdownIt({ html: false, linkify: true, typographer: false });

/** Where a relative path from a page leads, as the name of a file of the version. */
function resolve(from: string, path: string): string {
  const parts = from.split('/').slice(0, -1);

  let plain = path.split(/[?#]/)[0];

  try {
    plain = decodeURI(plain);
  } catch {
    // written with a stray percent sign, and meant as it is
  }

  for (const part of plain.split('/')) {
    if (part === '..') {
      parts.pop();
    } else if (part !== '.' && part !== '') {
      parts.push(part);
    }
  }

  return parts.join('/');
}

const absolute = (href: string) => /^[a-z][a-z0-9+.-]*:/i.test(href) || href.startsWith('//');

parser.renderer.rules.link_open = (tokens, index, options, env: Env, self) => {
  const token = tokens[index];
  const href = token.attrGet('href') ?? '';

  if (href.startsWith('#')) {
    return self.renderToken(tokens, index, options);
  }

  if (absolute(href)) {
    token.attrSet('target', '_blank');
    token.attrSet('rel', 'noreferrer noopener');
    return self.renderToken(tokens, index, options);
  }

  // a page of this version opens here; anything else relative has nowhere
  // to lead in the editor, so it stays words
  const target = resolve(env.name, href);
  const found = env.files.find((file) => file.name === target);

  token.attrs = (token.attrs ?? []).filter(([key]) => key !== 'href');

  if (found && found.encoding !== 'base64' && target.toLowerCase().endsWith('.md')) {
    token.attrSet('href', '#');
    token.attrSet('data-page', target);
  } else {
    token.attrSet('title', href);
    token.attrSet('data-nowhere', '');
  }

  return self.renderToken(tokens, index, options);
};

parser.renderer.rules.image = (tokens, index, _options, env: Env, self) => {
  const token = tokens[index];
  const src = token.attrGet('src') ?? '';
  const alt = self.renderInlineAsText(token.children ?? [], parser.options, env);

  let shown: string | null = null;

  if (absolute(src)) {
    // markdown-it has already refused what is not http, https or a picture's data
    shown = src;
  } else {
    const target = resolve(env.name, src);
    const file = env.files.find((f) => f.name === target);
    const type = PICTURES[target.split('.').pop()?.toLowerCase() ?? ''];

    if (file && type) {
      const base64 = file.encoding === 'base64' ? file.code : encodeBytes(new TextEncoder().encode(file.code));

      shown = `data:${type};base64,${base64}`;
    }
  }

  if (!shown) {
    return `<span class="missing" title="${parser.utils.escapeHtml(src)}">${parser.utils.escapeHtml(alt || src)}</span>`;
  }

  return `<img src="${parser.utils.escapeHtml(shown)}" alt="${parser.utils.escapeHtml(alt)}" loading="lazy" referrerpolicy="no-referrer">`;
};

parser.renderer.rules.heading_open = (tokens, index, options, env: Env, self) => {
  const text = tokens[index + 1]?.content ?? '';
  const slug = text.toLowerCase().replace(/[^\p{L}\p{N}]+/gu, '-').replace(/^-|-$/g, '') || 'section';
  const seen = env.slugs.get(slug) ?? 0;

  env.slugs.set(slug, seen + 1);
  tokens[index].attrSet('id', seen === 0 ? slug : `${slug}-${seen + 1}`);

  return self.renderToken(tokens, index, options);
};

parser.renderer.rules.fence = (tokens, index, _options, _env, _self) => {
  const token = tokens[index];
  const language = LANGUAGES[token.info.trim().split(/\s+/)[0]?.toLowerCase() ?? ''];
  const code = parser.utils.escapeHtml(token.content);

  return language ? `<pre><code data-language="${language}">${code}</code></pre>\n` : `<pre><code>${code}</code></pre>\n`;
};

function render(source: string, name: string, files: LambdaFile[]): string {
  const env: Env = { name, files, slugs: new Map() };

  return parser
    .render(source, env)
    // a list of things to do, the way it is written on a repository
    .replace(/<li>(<p>)?\[( |x|X)\]\s/g, (_, paragraph: string | undefined, mark: string) =>
      `<li class="task">${paragraph ?? ''}<input type="checkbox" disabled${mark === ' ' ? '' : ' checked'}> `)
    // a wide table scrolls on its own rather than widening the page
    .replace(/<table>/g, '<div class="table"><table>')
    .replace(/<\/table>/g, '</table></div>');
}
