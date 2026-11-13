import MarkdownIt from 'markdown-it';

/**
 * Markdown as the documentation and the tests of a lambda are written, turned
 * into markup - for the editor, and for the page of a published source.
 *
 * Rendered with markdown-it with raw HTML off, which is its default and the
 * point: the pages are written by agents and shown on this site, the editor
 * among it, whose address is the key to a lambda. What would be markup is
 * shown as text, and a link to anything but a page, an address or mail is not
 * a link.
 *
 * A page lives among the other files of its version, so a relative link to
 * another page opens that page where it is shown, and a relative picture - a
 * diagram beside the page - is shown from the version. Where both are found
 * is the caller's to say: the editor has the files at hand, a published
 * source has an address for each.
 *
 * Code blocks are marked with their language and left for the caller to
 * colour, since the editor colours with Monaco, which a public page does not
 * download.
 */

/** Where a page is shown, and what it finds beside it. */
export interface MarkdownPlace {
  /** The page's own name, which relative links and pictures are resolved against. */
  name: string;
  /** Where a picture of the version is shown from, by its name - or nothing where there is none. */
  picture: (name: string) => string | null;
  /** Whether a name is another page that opens where this one is shown. */
  page: (name: string) => boolean;
}

/** The grammars a code block may name, by the names the editor knows them under. */
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
  sql: 'sql',
  yaml: 'yaml',
  yml: 'yaml',
  dockerfile: 'dockerfile',
};

/** The pictures a page may show from beside it, by their extension. */
export const PICTURES: Record<string, string> = {
  png: 'image/png',
  jpg: 'image/jpeg',
  jpeg: 'image/jpeg',
  gif: 'image/gif',
  webp: 'image/webp',
  svg: 'image/svg+xml',
  avif: 'image/avif',
};

/** What a picture is, by its name - or nothing for a name that is no picture. */
export const pictureType = (name: string): string | undefined => PICTURES[name.split('.').pop()?.toLowerCase() ?? ''];

interface Env {
  place: MarkdownPlace;
  slugs: Map<string, number>;
}

const parser = new MarkdownIt({ html: false, linkify: true, typographer: false });

/** Where a relative path from a page leads, as the name of a file of the version. */
export function resolve(from: string, path: string): string {
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

  // a page of this version opens where this one is shown; anything else
  // relative has nowhere to lead, so it stays words
  const target = resolve(env.place.name, href);

  token.attrs = (token.attrs ?? []).filter(([key]) => key !== 'href');

  if (target.toLowerCase().endsWith('.md') && env.place.page(target)) {
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

  // markdown-it has already refused what is not http, https or a picture's data
  const shown = absolute(src) ? src : env.place.picture(resolve(env.place.name, src));

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

parser.renderer.rules.fence = (tokens, index) => {
  const token = tokens[index];
  const language = LANGUAGES[token.info.trim().split(/\s+/)[0]?.toLowerCase() ?? ''];
  const code = parser.utils.escapeHtml(token.content);

  return language ? `<pre><code data-language="${language}">${code}</code></pre>\n` : `<pre><code>${code}</code></pre>\n`;
};

/** A page, as markup to be put into an element with the markdown class. */
export function renderMarkdown(source: string, place: MarkdownPlace): string {
  const env: Env = { place, slugs: new Map() };

  return parser
    .render(source, env)
    // a list of things to do, the way it is written on a repository
    .replace(/<li>(<p>)?\[( |x|X)\]\s/g, (_, paragraph: string | undefined, mark: string) =>
      `<li class="task">${paragraph ?? ''}<input type="checkbox" disabled${mark === ' ' ? '' : ' checked'}> `)
    // a wide table scrolls on its own rather than widening the page
    .replace(/<table>/g, '<div class="table"><table>')
    .replace(/<\/table>/g, '</table></div>');
}
