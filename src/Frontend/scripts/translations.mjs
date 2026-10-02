#!/usr/bin/env node
/**
 * Translates what a change wrote in English, without anybody reading a whole
 * catalog.
 *
 *   node scripts/translations.mjs extract [--base <ref>] [--force] [--stale] [--redo <path>]...
 *   node scripts/translations.mjs apply
 *   node scripts/translations.mjs check
 *   node scripts/translations.mjs show <file> <path>
 *
 * `extract` compares the English catalogs (src/locales/en, and the English
 * titles and descriptions in src/pages.json) with the base of the branch, and
 * writes one request per language into .translations/: the register and the
 * fixed words of that language, and each sentence that is new or changed, with
 * what it said before and what the language says now. A translator - the
 * `translator` agent, one per language - reads its request alone and writes
 * its answer beside it. `apply` puts the answers into the catalogs, checks that
 * each kept what is not words (parameters, interpolations, the kit's calls,
 * links), and removes what English no longer has.
 *
 * A sentence is a "leaf": a property whose value is not an object or an array
 * literal - a string, a template, a function returning one or some JSX. A new
 * object is one item as a whole. An array is walked element by element while
 * its length stays the same, and is one item once it changes.
 *
 * What the branch already translated counts as done: a changed English
 * sentence is asked for only where the language still says what it said at
 * the base. After changing English again once translated, commit and extract
 * with --base HEAD; --force asks for every changed sentence regardless.
 *
 * --stale also asks for every sentence whose English was changed in a later
 * commit than its translation and says something else now (by git blame), and
 * --redo asks for a sentence, or everything under a path, to be checked again.
 */

import { execFileSync } from 'node:child_process';
import fs from 'node:fs';
import path from 'node:path';

import ts from 'typescript';

const FRONTEND = path.resolve(import.meta.dirname, '..');
const LOCALES = path.join(FRONTEND, 'src', 'locales');
const PAGES = path.join(FRONTEND, 'src', 'pages.json');
const GLOSSARY = path.join(LOCALES, 'glossary.json');
const WORK = path.join(FRONTEND, '.translations');
const REPOSITORY = git(['rev-parse', '--show-toplevel']).trim();
const STEERING = path.join(REPOSITORY, 'CLAUDE.md');

/** How the answers and requests name the pseudo catalog of page titles. */
const PAGES_FILE = 'pages.json';

const SOURCE = 'en';

// #region Files and git

function git(args, options = {})
{
  return execFileSync('git', args, { cwd: FRONTEND, encoding: 'utf8', maxBuffer: 64 * 1024 * 1024, ...options });
}

/** The text of a file of the repository at a commit, or null where it did not exist. */
function atCommit(ref, absolute)
{
  const relative = path.relative(REPOSITORY, absolute).split(path.sep).join('/');

  try
  {
    return git(['show', `${ref}:${relative}`], { stdio: ['ignore', 'pipe', 'ignore'] });
  }
  catch
  {
    return null;
  }
}

function onDisk(absolute)
{
  return fs.existsSync(absolute) ? fs.readFileSync(absolute, 'utf8') : null;
}

/** The languages, read from the list the site itself uses. */
function languages()
{
  const text = fs.readFileSync(path.join(FRONTEND, 'src', 'i18n', 'languages.ts'), 'utf8');
  const list = /export const LANGUAGES = \[([^\]]*)\]/.exec(text);

  if (!list)
  {
    throw new Error('Could not find LANGUAGES in src/i18n/languages.ts.');
  }

  return [...list[1].matchAll(/'([^']+)'/g)].map((match) => match[1]);
}

/** The catalog files of a language, relative to its folder. */
function catalogFiles(language)
{
  const root = path.join(LOCALES, language);
  const found = [];

  const visit = (folder) =>
  {
    for (const entry of fs.readdirSync(folder, { withFileTypes: true }))
    {
      const full = path.join(folder, entry.name);

      if (entry.isDirectory())
      {
        visit(full);
      }
      else if (/\.tsx?$/.test(entry.name) && entry.name !== 'index.ts')
      {
        found.push(path.relative(root, full).split(path.sep).join('/'));
      }
    }
  };

  visit(root);
  return found.sort();
}

// #endregion

// #region Reading a catalog

/**
 * The tree of a catalog file: its exported objects by name, each node being an
 * object (children by key), an array (children by index) or a leaf.
 *
 * `read` gives the text of another file the catalog imports from (at the same
 * commit), so a property that is only a name - the editor's `shared: SHARED` -
 * is followed to the object it names.
 */
function parseCatalog(text, fileName, read)
{
  const source = ts.createSourceFile(fileName, text, ts.ScriptTarget.Latest, true, ts.ScriptKind.TSX);
  const roots = new Map();

  for (const statement of source.statements)
  {
    if (!ts.isVariableStatement(statement) || !statement.modifiers?.some((m) => m.kind === ts.SyntaxKind.ExportKeyword))
    {
      continue;
    }

    for (const declaration of statement.declarationList.declarations)
    {
      if (ts.isIdentifier(declaration.name) && declaration.initializer)
      {
        roots.set(declaration.name.text, toNode(declaration.initializer, source, null, fileName, read));
      }
    }
  }

  return { source, roots };
}

function unwrap(expression)
{
  while (ts.isParenthesizedExpression(expression) || ts.isAsExpression(expression) || ts.isSatisfiesExpression(expression))
  {
    expression = expression.expression;
  }

  return expression;
}

function propertyName(property)
{
  const name = property.name;

  if (ts.isIdentifier(name) || ts.isStringLiteral(name) || ts.isNumericLiteral(name))
  {
    return name.text;
  }

  return name.getText();
}

/** The doc comment written above a node, without its stars. */
function docComment(node, source)
{
  const ranges = ts.getLeadingCommentRanges(source.text, node.getFullStart()) ?? [];
  const text = ranges
    .map((range) => source.text.slice(range.pos, range.end))
    .filter((comment) => comment.startsWith('/**'))
    .map((comment) => comment.replace(/^\/\*\*|\*\/$/g, '').replace(/^\s*\* ?/gm, '').trim())
    .join(' ')
    .replace(/\s+/g, ' ')
    .trim();

  return text || null;
}

function toNode(initializer, source, property, fileName, read)
{
  let expression = unwrap(initializer);
  let home = source;

  if (ts.isIdentifier(expression) && read)
  {
    const followed = follow(expression.text, source, fileName, read);

    if (followed)
    {
      ({ expression, source: home } = followed);
      expression = unwrap(expression);
    }
  }

  const node = {
    expression: home === source ? initializer : expression,
    source: home,
    property,
    comment: property ? docComment(property, source) : null,
  };

  if (ts.isObjectLiteralExpression(expression))
  {
    node.kind = 'object';
    node.children = new Map();

    for (const member of expression.properties)
    {
      if (ts.isPropertyAssignment(member))
      {
        node.children.set(propertyName(member), toNode(member.initializer, home, member, fileName, read));
      }
      else if (ts.isShorthandPropertyAssignment(member))
      {
        // `{ units }` names a variable of the file; there are none in the catalogs today.
        throw new Error(`${fileName}: shorthand property "${member.name.text}" is not supported in a catalog.`);
      }
    }
  }
  else if (ts.isArrayLiteralExpression(expression))
  {
    node.kind = 'array';
    node.children = expression.elements.map((element) => toNode(element, home, null, fileName, read));
  }
  else
  {
    node.kind = 'leaf';
  }

  return node;
}

/** The exported object an imported name stands for, in the file it is imported from. */
function follow(name, source, fileName, read)
{
  for (const statement of source.statements)
  {
    if (!ts.isImportDeclaration(statement) || !statement.importClause?.namedBindings)
    {
      continue;
    }

    const bindings = statement.importClause.namedBindings;

    if (!ts.isNamedImports(bindings) || !bindings.elements.some((element) => element.name.text === name))
    {
      continue;
    }

    const specifier = statement.moduleSpecifier.text;
    const base = path.resolve(path.dirname(fileName), specifier);

    for (const candidate of [`${base}.ts`, `${base}.tsx`, path.join(base, 'index.ts')])
    {
      const text = read(candidate);

      if (text !== null)
      {
        const imported = parseCatalog(text, candidate, null);
        const root = imported.roots.get(name);
        return root ? { expression: root.expression, source: root.source } : null;
      }
    }
  }

  return null;
}

/** Page titles and descriptions as a catalog: one root per route, its language's strings as leaves. */
function parsePages(text, language)
{
  if (text === null)
  {
    return null;
  }

  const pages = JSON.parse(text);
  const roots = new Map();

  for (const [route, page] of Object.entries(pages))
  {
    const strings = page.text?.[language];

    if (!strings)
    {
      continue;
    }

    const children = new Map();
    const words = {};

    // The social image of a language is drawn, not translated.
    for (const [key, value] of Object.entries(strings))
    {
      if (key !== 'image')
      {
        children.set(key, { kind: 'leaf', json: JSON.stringify(value), comment: null });
        words[key] = value;
      }
    }

    roots.set(route, { kind: 'object', children, json: JSON.stringify(words, null, 2), comment: null });
  }

  return { roots, pages };
}

/** The text of a node, its lines after the first one moved left by the indentation of the line it starts on. */
function textOf(node)
{
  if (node.json !== undefined)
  {
    return node.json;
  }

  const source = node.source;
  const start = node.expression.getStart(source);
  const text = source.text.slice(start, node.expression.end);
  return dedent(text, indentationAt(source.text, start));
}

function indentationAt(text, position)
{
  const lineStart = text.lastIndexOf('\n', position - 1) + 1;
  return /^[ \t]*/.exec(text.slice(lineStart))[0];
}

function dedent(text, indentation)
{
  return text
    .split('\n')
    .map((line, index) => (index > 0 && line.startsWith(indentation) ? line.slice(indentation.length) : line))
    .join('\n');
}

function indent(text, indentation)
{
  return text
    .split('\n')
    .map((line, index) => (index > 0 && line.length > 0 ? indentation + line : line))
    .join('\n');
}

/** Whitespace does not make a sentence different. */
function same(a, b)
{
  return textOf(a).replace(/\s+/g, ' ') === textOf(b).replace(/\s+/g, ' ');
}

function formatPath(segments)
{
  return segments
    .map((segment, index) =>
    {
      if (typeof segment === 'number')
      {
        return `[${segment}]`;
      }

      if (/^[A-Za-z_$][\w$]*$/.test(segment))
      {
        return index === 0 ? segment : `.${segment}`;
      }

      return index === 0 ? segment : `[${JSON.stringify(segment)}]`;
    })
    .join('');
}

function child(node, segment)
{
  if (!node)
  {
    return undefined;
  }

  if (typeof segment === 'number')
  {
    return node.kind === 'array' ? node.children[segment] : undefined;
  }

  return node.kind === 'object' ? node.children.get(segment) : undefined;
}

function nodeAt(roots, segments)
{
  let node = roots?.get(segments[0]);

  for (const segment of segments.slice(1))
  {
    node = child(node, segment);
  }

  return node;
}

// #endregion

// #region One catalog file, in every version that matters

/**
 * A catalog file of one language, as the base of the branch had it and as it
 * is now. Page titles are read the same way from pages.json.
 */
function load(file, language, base)
{
  if (file === PAGES_FILE)
  {
    return {
      base: base ? parsePages(atCommit(base, PAGES), language) : null,
      now: parsePages(onDisk(PAGES), language),
    };
  }

  const absolute = path.join(LOCALES, language, file);
  const parse = (text, read) => (text === null ? null : parseCatalog(text, absolute, read));

  return {
    base: base ? parse(atCommit(base, absolute), (other) => atCommit(base, other)) : null,
    now: parse(onDisk(absolute), onDisk),
  };
}

// #endregion

// #region extract

/**
 * Walks the English tree as it is now beside the English of the base and the
 * language as it is now and at the base, and says what the language needs.
 */
function compare(context, segments, english, before, target, targetBefore, items, removals)
{
  if (!target)
  {
    items.push({ path: segments, kind: before ? 'missing' : 'new', english, before: null, current: null });
    return;
  }

  if (english.kind === 'object' && target.kind === 'object')
  {
    for (const [key, node] of english.children)
    {
      compare(context, [...segments, key], node, child(before, key), target.children.get(key), child(targetBefore, key), items, removals);
    }

    for (const key of target.children.keys())
    {
      if (!english.children.has(key))
      {
        removals.push([...segments, key]);
      }
    }

    return;
  }

  const walkable =
    english.kind === 'array' &&
    target.kind === 'array' &&
    before?.kind === 'array' &&
    before.children.length === english.children.length &&
    target.children.length === english.children.length;

  if (walkable)
  {
    english.children.forEach((node, index) =>
      compare(context, [...segments, index], node, before.children[index], target.children[index], child(targetBefore, index), items, removals),
    );

    return;
  }

  if (context.redo.some((prefix) => covers(prefix, segments)))
  {
    items.push({ path: segments, kind: 'redo', english, before: null, current: target });
    return;
  }

  const changed = !before || before.kind !== english.kind || !same(before, english);
  const mismatched = target.kind !== english.kind;

  if (!changed && !mismatched)
  {
    const then = context.stale ? staleness(context, segments, english, target) : null;

    if (then)
    {
      items.push({ path: segments, kind: 'stale', english, before: then, current: target });
    }

    return;
  }

  // New in English and already in the language, or changed and already
  // translated: written in this branch, so done unless asked for again.
  const written = !before || !targetBefore || !same(targetBefore, target);

  if (written && !mismatched && !context.force)
  {
    return;
  }

  items.push({ path: segments, kind: before ? 'changed' : 'new', english, before: before ?? null, current: target });
}

/** Whether a --redo prefix names a sentence or something it is in. */
function covers(prefix, segments)
{
  const name = formatPath(segments);
  return name === prefix || name.startsWith(`${prefix}.`) || name.startsWith(`${prefix}[`);
}

/**
 * What English said when the language last touched a sentence, where English
 * changed in a later commit and said something else - a translation that was
 * left behind. Null where it is not.
 */
function staleness(context, segments, english, target)
{
  if (english.json !== undefined)
  {
    return null;
  }

  const englishTouched = touched(english);
  const targetTouched = touched(target);

  if (englishTouched.time <= targetTouched.time)
  {
    return null;
  }

  const key = `${targetTouched.commit} ${context.file}`;

  if (!context.history.has(key))
  {
    const absolute = path.join(LOCALES, SOURCE, context.file);
    const text = atCommit(targetTouched.commit, absolute);
    context.history.set(key, text === null ? null : parseCatalog(text, absolute, (other) => atCommit(targetTouched.commit, other)));
  }

  const then = nodeAt(context.history.get(key)?.roots, segments);
  return then && !same(then, english) ? then : null;
}

const blames = new Map();

/** The newest commit that touched the lines of a node, and when; lines not committed yet are the newest of all. */
function touched(node)
{
  const absolute = node.source.fileName;

  if (!blames.has(absolute))
  {
    const lines = [];
    const times = new Map();

    try
    {
      let commit = null;
      let line = 0;

      for (const row of git(['blame', '--porcelain', '--', path.relative(FRONTEND, absolute)]).split('\n'))
      {
        const header = /^([0-9a-f]{40}) \d+ (\d+)/.exec(row);

        if (header)
        {
          [commit, line] = [header[1], Number(header[2])];
        }
        else if (row.startsWith('committer-time '))
        {
          times.set(commit, /^0+$/.test(commit) ? Infinity : Number(row.slice('committer-time '.length)));
        }
        else if (row.startsWith('\t'))
        {
          lines[line] = commit;
        }
      }
    }
    catch
    {
      // Not committed at all: newer than anything.
    }

    blames.set(absolute, { lines, times });
  }

  const { lines, times } = blames.get(absolute);
  const source = node.source;
  const first = source.getLineAndCharacterOfPosition(node.expression.getStart(source)).line + 1;
  const last = source.getLineAndCharacterOfPosition(node.expression.end).line + 1;
  let newest = { commit: null, time: -Infinity };

  for (let line = first; line <= last; line++)
  {
    const commit = lines[line];
    const time = commit ? times.get(commit) : Infinity;

    if (time > newest.time)
    {
      newest = { commit, time };
    }
  }

  return newest;
}

function extract(options)
{
  const base = options.base ?? git(['merge-base', 'HEAD', 'origin/main']).trim();
  const targets = languages().filter((language) => language !== SOURCE);
  const files = [...catalogFiles(SOURCE), PAGES_FILE];
  const glossary = JSON.parse(fs.readFileSync(GLOSSARY, 'utf8'));
  const registers = readRegisters();

  const english = new Map(files.map((file) => [file, load(file, SOURCE, base)]));
  const context = { ...options, history: new Map() };
  const plan = { base, languages: {} };
  let total = 0;

  fs.rmSync(WORK, { recursive: true, force: true });
  fs.mkdirSync(WORK, { recursive: true });

  context.labels = labelsOf(english);

  for (const language of targets)
  {
    const work = { items: [], removals: [], requests: [] };
    const loaded = new Map(files.map((file) => [file, load(file, language, base)]));
    context.translated = (file, segments) => nodeAt(loaded.get(file)?.now?.roots, segments);

    for (const file of files)
    {
      const { base: before, now } = english.get(file);
      const target = loaded.get(file);
      context.file = file;

      for (const [root, node] of now.roots)
      {
        const items = [];
        const removals = [];

        compare(context, [root], node, before?.roots.get(root), target.now?.roots.get(root), target.base?.roots.get(root), items, removals);

        work.items.push(...items.map((item) => describe(context, file, item, now, target.now)));
        work.removals.push(...removals.map((segments) => ({ file, path: segments })));
      }

      for (const root of target.now?.roots.keys() ?? [])
      {
        if (!now.roots.has(root) && file !== PAGES_FILE)
        {
          work.removals.push({ file, path: [root] });
        }
      }
    }

    plan.languages[language] = work;
    total += work.items.length;

    if (work.items.length > 0)
    {
      const parts = requests(language, work.items, glossary, registers[language]);
      work.requests = parts.map((_, index) => (parts.length === 1 ? `${language}.request.md` : `${language}.${index + 1}.request.md`));

      parts.forEach(({ text, items }, index) =>
      {
        fs.writeFileSync(path.join(WORK, work.requests[index]), text);
        items.forEach((item) => (item.request = work.requests[index]));
      });
    }
  }

  fs.writeFileSync(path.join(WORK, 'plan.json'), JSON.stringify(plan, null, 2) + '\n');

  const asked = targets.filter((language) => plan.languages[language].items.length > 0);
  const removed = targets.reduce((sum, language) => sum + plan.languages[language].removals.length, 0);

  console.log(`Base ${base.slice(0, 10)}: ${total} sentences to translate in ${asked.length} languages, ${removed} to remove.`);

  for (const language of asked)
  {
    const { items, requests: files } = plan.languages[language];
    const sizes = files.map((file) => `${file} (${Math.round(fs.statSync(path.join(WORK, file)).size / 1024)} KB)`);
    console.log(`  ${language.padEnd(6)} ${String(items.length).padStart(3)} sentences  ${sizes.join(', ')}`);
  }

  if (asked.length > 0)
  {
    const count = asked.reduce((sum, language) => sum + plan.languages[language].requests.length, 0);
    console.log(`\nNext: ${count} \`translator\` agents, all at once (Claude Code runs 20 at a time), each told only the path of its request:`);

    for (const language of asked)
    {
      plan.languages[language].requests.forEach((file) => console.log(`  src/Frontend/.translations/${file}`));
    }

    console.log('Then: node scripts/translations.mjs apply');
  }
  else if (removed > 0)
  {
    console.log('\nNothing to translate. Run apply to remove what English no longer has.');
  }
}

/**
 * Where each label of the screen is in English - every sentence that is a
 * plain string - so a sentence that names one (`k.b('Put it online')`) can be
 * shown what the language calls it there.
 */
function labelsOf(english)
{
  const labels = new Map();

  for (const [file, { now }] of english)
  {
    for (const [segments, node] of walkLeaves(now?.roots))
    {
      const value = node.json !== undefined ? null : unwrap(node.expression);

      if (value && (ts.isStringLiteral(value) || ts.isNoSubstitutionTemplateLiteral(value)) && value.text.length <= 60 && !labels.has(value.text))
      {
        labels.set(value.text, { file, segments });
      }
    }
  }

  return labels;
}

/** An item as the plan keeps it: the texts it was asked with, its neighbours for tone, and the labels it names. */
function describe(context, file, item, englishCatalog, targetCatalog)
{
  const segments = item.path;
  const described = {
    file,
    path: segments,
    name: formatPath(segments),
    kind: item.kind,
    english: textOf(item.english),
    before: item.before ? textOf(item.before) : null,
    current: item.current ? textOf(item.current) : null,
    note: noteFor(englishCatalog, segments),
    neighbours: [],
  };

  if ((item.kind === 'new' || item.kind === 'missing') && segments.length > 1 && typeof segments.at(-1) === 'string')
  {
    const parent = nodeAt(englishCatalog.roots, segments.slice(0, -1));
    const keys = [...parent.children.keys()];
    const at = keys.indexOf(segments.at(-1));

    for (const key of [keys[at - 1], keys[at + 1]])
    {
      const english = key === undefined ? undefined : parent.children.get(key);
      const translated = key === undefined ? undefined : nodeAt(targetCatalog?.roots, [...segments.slice(0, -1), key]);

      if (english?.kind === 'leaf' && translated?.kind === 'leaf' && textOf(english).length <= 240 && textOf(translated).length <= 240)
      {
        described.neighbours.push({ name: formatPath([...segments.slice(0, -1), key]), english: textOf(english), translated: textOf(translated) });
      }
    }
  }

  described.labels = [];

  for (const [, label] of described.english.matchAll(/\bb\('((?:[^'\\]|\\.)+)'\)/g))
  {
    const place = context.labels.get(label.replace(/\\'/g, "'"));
    const translated = place ? context.translated(place.file, place.segments) : undefined;

    if (translated?.kind === 'leaf' && !described.labels.some((known) => known.english === label))
    {
      described.labels.push({ english: label, translated: textOf(translated) });
    }
  }

  return described;
}

/** What the English says about a sentence: its own doc comment, or the nearest one above it. */
function noteFor(catalog, segments)
{
  for (let length = segments.length; length > 0; length--)
  {
    const node = nodeAt(catalog.roots, segments.slice(0, length));

    if (node?.comment)
    {
      return length === segments.length ? node.comment : `About ${formatPath(segments.slice(0, length))}: ${node.comment}`;
    }
  }

  return null;
}

/**
 * The register of each language, read from the table in CLAUDE.md so it is
 * written down once.
 */
function readRegisters()
{
  const text = fs.readFileSync(STEERING, 'utf8');
  const registers = {};

  for (const match of text.matchAll(/^\|\s*([a-z]{2}(?:-[a-z]{2})?)\s*\|([^|\n]*)\|([^|\n]*)\|\s*$/gm))
  {
    const [, language, form, notes] = match;
    registers[language] = [form.trim(), notes.trim()].filter(Boolean).join('; ');
  }

  for (const language of languages().filter((code) => code !== SOURCE))
  {
    if (!registers[language])
    {
      throw new Error(`CLAUDE.md has no row for "${language}" in the table of registers.`);
    }
  }

  return registers;
}

const PAGE_OF_FILE = {
  'build.tsx': '/build - the page for people who do not code',
  'ship.tsx': '/ship - the page for developers',
  'landing.tsx': '/ - the landing page',
  'guide.tsx': '/docs - the guide for owners',
  'source.tsx': '/source - published code',
  'enterprise.tsx': '/enterprise',
  'showcase.tsx': '/showcase',
  'create.tsx': 'the page that creates a lambda',
  'connect.tsx': 'connecting an agent over MCP',
  'common.tsx': 'every page: the frame, the menu, the footer',
  'terms.tsx': '/terms - legal',
  'privacy.tsx': '/privacy - legal',
  'imprint.tsx': '/imprint - legal',
  [PAGES_FILE]: 'the title and description search engines show for a page',
};

function pageOf(file, name)
{
  if (/^editor\.(data\.)?simple\b/.test(name))
  {
    return 'the simple view of the editor, for owners who do not write code - no developer words';
  }

  if (name.startsWith('editor.change'))
  {
    return 'the editor\'s Change section, where an owner who may not write code tells the agent what to change - no developer words';
  }

  if (file.startsWith('editor'))
  {
    return 'the editor, where an owner controls a lambda';
  }

  return PAGE_OF_FILE[file] ?? file;
}

/** What each kind of item asks of the translator. */
const SAID = {
  new: 'New.',
  missing: 'Missing in this language.',
  changed: 'The English changed.',
  stale: 'The English changed after this was translated, and the translation was left behind.',
  redo: 'Translated before: check it against the English, correct what does not match and keep what does.',
};

/** How long a request may get, so a translator reads it in one go: the Read tool shows 2000 lines, and some 25,000 tokens, at a time. */
const REQUEST_LINES = 1500;
const REQUEST_CHARACTERS = 45_000;

/**
 * The requests of one language: its register and fixed words, then its
 * sentences - in parts when there are many, each for a translator of its own.
 */
function requests(language, items, glossary, register)
{
  const name = glossary.languages[language]?.name ?? language;
  const style = glossary.languages[language]?.style;
  const header = [`# Into ${name} (${language})`, '', `Register: ${register}`];

  if (style)
  {
    header.push(`Style: ${style}`);
  }

  header.push('', '## Fixed words', '');

  for (const term of glossary.terms)
  {
    const translation = term.keep ? 'stays in English' : term[language];

    if (translation)
    {
      header.push(`- ${term.en} → ${translation}${term.note ? ` (${term.note})` : ''}`);
    }
  }

  const blocks = items.map((item) =>
  {
    const lines = ['', `=== ${item.file} ${item.name}`, `On: ${pageOf(item.file, item.name)}. ${SAID[item.kind]}`];

    if (item.note)
    {
      lines.push(`Note: ${item.note}`);
    }

    if (item.before !== null)
    {
      lines.push('English before:', item.before, 'English now:', item.english);
    }
    else
    {
      lines.push('English:', item.english);
    }

    if (item.current !== null)
    {
      lines.push('Current translation:', item.current);
    }

    for (const neighbour of item.neighbours)
    {
      lines.push(`Beside it, ${neighbour.name}: ${neighbour.english} → ${neighbour.translated}`);
    }

    for (const label of item.labels ?? [])
    {
      lines.push(`On the screen, '${label.english}' is ${label.translated}`);
    }

    return lines.join('\n');
  });

  const parts = [[]];
  const empty = { lines: header.length, characters: header.join('\n').length };
  let length = { ...empty };

  for (const block of blocks)
  {
    const size = { lines: block.split('\n').length, characters: block.length };

    if (parts.at(-1).length > 0 && (length.lines + size.lines > REQUEST_LINES || length.characters + size.characters > REQUEST_CHARACTERS))
    {
      parts.push([]);
      length = { ...empty };
    }

    parts.at(-1).push(block);
    length = { lines: length.lines + size.lines, characters: length.characters + size.characters };
  }

  let first = 0;

  return parts.map((part, index) =>
  {
    const counted = parts.length > 1 ? `, part ${index + 1} of ${parts.length}` : '';
    const text = [...header, '', `## Sentences (${part.length}${counted})`, ...part, ''].join('\n');
    const covered = items.slice(first, first + part.length);
    first += part.length;
    return { text, items: covered };
  });
}

// #endregion

// #region apply

/** The blocks of an answer file: `=== file path`, then the expression. */
function readAnswer(text)
{
  const answers = new Map();
  let current = null;

  for (const line of text.split('\n'))
  {
    const header = /^=== (\S+) (.+?)\s*$/.exec(line);

    if (header)
    {
      current = { key: `${header[1]} ${header[2]}`, lines: [] };
      answers.set(current.key, current);
    }
    else if (current)
    {
      current.lines.push(line);
    }
  }

  return new Map([...answers].map(([key, { lines }]) => [key, lines.join('\n').trim()]));
}

function parseExpression(text)
{
  const wrapped = `const __ = (\n${text}\n);`;
  const source = ts.createSourceFile('answer.tsx', wrapped, ts.ScriptTarget.Latest, true, ts.ScriptKind.TSX);
  const diagnostics = source.parseDiagnostics ?? [];

  if (diagnostics.length > 0)
  {
    return { error: ts.flattenDiagnosticMessageText(diagnostics[0].messageText, '\n') };
  }

  const declaration = source.statements[0]?.declarationList?.declarations[0];

  if (!declaration?.initializer)
  {
    return { error: 'not an expression' };
  }

  return { source, expression: declaration.initializer };
}

/**
 * The answer with the types taken off: the catalogs of the other languages are
 * typed by the English one, and its types (ReactNode, Node, the guide's Text)
 * are not imported there - where they mean something else, like the DOM's Text.
 * Parameters the translation no longer uses go as well, the way the catalogs
 * have it: the trailing ones dropped, the others named with an underscore.
 */
function withoutTypes(text)
{
  const parsed = parseExpression(text);

  if (parsed.error)
  {
    return { error: parsed.error };
  }

  const { source } = parsed;
  const cuts = [];

  const visit = (node) =>
  {
    if (ts.isArrowFunction(node) || ts.isFunctionExpression(node))
    {
      for (const parameter of node.parameters)
      {
        if (parameter.type)
        {
          const from = (parameter.questionToken ?? parameter.name).end;
          cuts.push([from, parameter.type.end, '']);
        }
      }

      if (node.type)
      {
        const colon = source.text.lastIndexOf(':', node.type.getStart(source));
        cuts.push([colon, node.type.end, '']);
      }
    }

    if ((ts.isAsExpression(node) || ts.isSatisfiesExpression(node)) && !(ts.isTypeReferenceNode(node.type) && node.type.getText(source) === 'const'))
    {
      cuts.push([node.expression.end, node.end, '']);
      visit(node.expression);
      return;
    }

    ts.forEachChild(node, visit);
  };

  visit(source.statements[0]);
  return { text: unwrapped(tidyParameters(edit(source.text, cuts))) };
}

/** Text with replacements made, each [from, to, text], none overlapping. */
function edit(text, edits)
{
  let result = text;

  for (const [from, to, replacement] of [...edits].sort((a, b) => b[0] - a[0]))
  {
    result = result.slice(0, from) + replacement + result.slice(to);
  }

  return result;
}

/** The expression out of the declaration parseExpression wraps it in. */
function unwrapped(wrapped)
{
  return wrapped.slice('const __ = (\n'.length, wrapped.length - '\n);'.length);
}

/**
 * The sentences of an answer that are functions - the answer itself, or the
 * values of the objects and arrays it is made of - without the parameters
 * they do not use.
 */
function tidyParameters(wrapped)
{
  const source = ts.createSourceFile('answer.tsx', wrapped, ts.ScriptTarget.Latest, true, ts.ScriptKind.TSX);
  const edits = [];

  const sentence = (node) =>
  {
    node = unwrap(node);

    if (ts.isObjectLiteralExpression(node))
    {
      node.properties.forEach((property) => ts.isPropertyAssignment(property) && sentence(property.initializer));
    }
    else if (ts.isArrayLiteralExpression(node))
    {
      node.elements.forEach(sentence);
    }
    else if ((ts.isArrowFunction(node) || ts.isFunctionExpression(node)) && node.parameters.length > 0)
    {
      const used = new Set();

      const look = (inner) =>
      {
        const named = ts.isPropertyAccessExpression(inner.parent) && inner.parent.name === inner;
        const key = (ts.isPropertyAssignment(inner.parent) || ts.isJsxAttribute(inner.parent)) && inner.parent.name === inner;

        if (ts.isIdentifier(inner) && !named && !key)
        {
          used.add(inner.text);
        }

        ts.forEachChild(inner, look);
      };

      look(node.body);

      const names = node.parameters.map((parameter) => (ts.isIdentifier(parameter.name) ? parameter.name.text : null));
      let keep = names.length;

      while (keep > 0 && names[keep - 1] !== null && !used.has(names[keep - 1]))
      {
        keep--;
      }

      const kept = node.parameters.slice(0, keep).map((parameter, index) =>
      {
        const name = names[index];
        const text = parameter.getText(source);
        return name !== null && !used.has(name) && !name.startsWith('_') ? `_${text}` : text;
      });

      const original = node.parameters.map((parameter) => parameter.getText(source));

      if (kept.length !== original.length || kept.some((text, index) => text !== original[index]))
      {
        const first = node.parameters[0].getStart(source);
        const last = node.parameters.at(-1).end;
        const parenthesized = wrapped.slice(0, first).trimEnd().endsWith('(');
        edits.push(parenthesized ? [first, last, kept.join(', ')] : [first, last, `(${kept.join(', ')})`]);
      }
    }
  };

  sentence(source.statements[0].declarationList.declarations[0].initializer);
  return edit(wrapped, edits);
}

/**
 * The language's spacing as a function of a text: French puts a non-breaking
 * space before ? ! : ; and inside « », which nobody types reliably.
 */
function spacing(rules)
{
  const before = rules?.nbspBefore ? new RegExp(` (?=[${escape(rules.nbspBefore)}])`, 'g') : null;
  const after = rules?.nbspAfter ? new RegExp(`(?<=[${escape(rules.nbspAfter)}]) `, 'g') : null;

  return (words) =>
  {
    let result = before ? words.replace(before, '\u00a0') : words;
    result = after ? result.replace(after, '\u00a0') : result;
    return result;
  };
}

/** The language's typography, applied to the words only, never to code. */
function typeset(text, rules)
{
  if (!rules?.nbspBefore && !rules?.nbspAfter)
  {
    return text;
  }

  const { source } = parseExpression(text);
  const space = spacing(rules);
  const edits = [];

  const visit = (node) =>
  {
    const isCode = ts.isCallExpression(node.parent) && /(^|\.)code$/.test(node.parent.expression.getText(source));

    if (!isCode && (ts.isStringLiteral(node) || ts.isNoSubstitutionTemplateLiteral(node) || ts.isTemplateHead(node) || ts.isTemplateMiddle(node) || ts.isTemplateTail(node) || ts.isJsxText(node)))
    {
      // The text of JSX starts at its position: getStart would skip its leading spaces.
      const from = ts.isJsxText(node) ? node.pos : node.getStart(source);
      const words = source.text.slice(from, node.end);

      // A lone space between two parts of a sentence is not typography.
      if (words.trim().length > 0)
      {
        edits.push([from, node.end, space(words)]);
      }
    }

    ts.forEachChild(node, visit);
  };

  visit(source.statements[0]);
  return unwrapped(edit(source.text, edits));
}

/** The same typography for the strings of pages.json. */
function typesetJson(answer, rules)
{
  const value = JSON.parse(answer);
  const space = spacing(rules);
  return JSON.stringify(typeof value === 'string' ? space(value) : Object.fromEntries(Object.entries(value).map(([key, text]) => [key, space(text)])));
}

function escape(characters)
{
  return characters.replace(/[\\\]^-]/g, '\\$&');
}

/** The kit's calls a sentence marks its words up with; other methods (slice, toLocaleString) are a language's own business. */
const MARKUP = /^(code|b|em|link)$/;

/**
 * What a sentence has besides its words, which a translation keeps: how many
 * parameters it takes, which of them it uses and how (`k.code`, `version`),
 * the addresses it links to and the code it shows.
 */
function shapeOf(expression, source)
{
  const shape = { parameters: 0, uses: new Set(), calls: new Set(), addresses: new Set(), code: new Set() };
  const top = unwrap(expression);
  const parameters = new Map();

  if (ts.isArrowFunction(top) || ts.isFunctionExpression(top))
  {
    shape.parameters = top.parameters.length;
    top.parameters.forEach((parameter, index) =>
    {
      if (ts.isIdentifier(parameter.name))
      {
        parameters.set(parameter.name.text, `#${index}`);
      }
    });
  }

  const visit = (node) =>
  {
    if (ts.isIdentifier(node) && parameters.has(node.text) && !(ts.isParameter(node.parent) && node.parent.name === node))
    {
      const parameter = parameters.get(node.text);
      shape.uses.add(parameter);

      if (ts.isCallExpression(node.parent) && node.parent.expression === node)
      {
        shape.calls.add(`${parameter}()`);
      }
      else if (ts.isPropertyAccessExpression(node.parent) && ts.isCallExpression(node.parent.parent) && node.parent.parent.expression === node.parent && MARKUP.test(node.parent.name.text))
      {
        shape.calls.add(`${parameter}.${node.parent.name.text}()`);
      }
    }

    if (ts.isStringLiteral(node) || ts.isNoSubstitutionTemplateLiteral(node))
    {
      if (/^(\/|https?:|mailto:|#)\S*$/.test(node.text))
      {
        shape.addresses.add(node.text);
      }

      if (ts.isCallExpression(node.parent) && /(^|\.)code$/.test(node.parent.expression.getText(source)))
      {
        shape.code.add(node.text);
      }
    }

    ts.forEachChild(node, visit);
  };

  visit(top);
  return shape;
}

/**
 * Where a translation lost or gained something that is not words. Severe
 * problems break the page - a parameter or a link gone - and are refused;
 * the rest are reported, since a language may have a reason (a number
 * formatted its way, Strg+S for Ctrl-S).
 */
function shapeProblems(english, englishSource, translation, translationSource)
{
  if (unwrap(english).kind === ts.SyntaxKind.NullKeyword)
  {
    // English has nothing where other languages may have a sentence (the binding version of the terms).
    return [];
  }

  const a = shapeOf(english, englishSource);
  const b = shapeOf(translation, translationSource);
  const problems = [];
  const lost = (key) => [...a[key]].filter((value) => !b[key].has(value));
  const gained = (key) => [...b[key]].filter((value) => !a[key].has(value));
  const say = (severe, list, text) => list.length > 0 && problems.push({ severe, text: `${text} ${list.join(', ')}` });

  if (b.parameters > a.parameters)
  {
    problems.push({ severe: true, text: `takes ${b.parameters} parameters, English ${a.parameters}` });
  }

  // A parameter is named by its place (#0, #1), since a translation may call it otherwise.
  say(true, lost('uses'), 'does not use the parameters');
  say(false, gained('uses'), 'uses parameters English does not use:');
  say(true, lost('calls').filter((call) => !/\.(code|b|em)\(\)$/.test(call)), 'no longer calls');
  say(false, lost('calls').filter((call) => /\.(code|b|em)\(\)$/.test(call)), 'no longer marks up with');
  say(true, lost('addresses'), 'lost the links to');
  say(false, gained('addresses'), 'links to what English does not:');
  say(false, lost('code'), 'shows other code than English instead of');

  return problems;
}

/**
 * The same comparison over two trees, so an object item is checked leaf by
 * leaf. Each problem names where it is, below `name` (empty for the item
 * itself); a different structure is always severe.
 */
function treeProblems(english, translation, name = '')
{
  const at = (problem) => ({ ...problem, text: name ? `${name}: ${problem.text}` : problem.text });
  const severe = (text) => at({ severe: true, text });
  const below = (key) => (typeof key === 'number' ? `${name}[${key}]` : name ? `${name}.${key}` : key);
  const what = (node) => (node.kind === 'leaf' ? 'a sentence' : `an ${node.kind}`);

  if (english.kind !== translation.kind)
  {
    return [severe(`is ${what(translation)} where English has ${what(english)}`)];
  }

  if (english.kind === 'object')
  {
    const problems = [];

    for (const [key, node] of english.children)
    {
      const other = translation.children.get(key);
      problems.push(...(other ? treeProblems(node, other, below(key)) : [severe(`lacks ${key}`)]));
    }

    for (const key of translation.children.keys())
    {
      if (!english.children.has(key))
      {
        problems.push(severe(`has ${key}, which English does not`));
      }
    }

    return problems;
  }

  if (english.kind === 'array')
  {
    if (english.children.length !== translation.children.length)
    {
      return [severe(`has ${translation.children.length} entries where English has ${english.children.length}`)];
    }

    return english.children.flatMap((node, index) => treeProblems(node, translation.children[index], below(index)));
  }

  if (english.json !== undefined)
  {
    const placeholders = (json) => new Set(JSON.parse(json).match(/\{\w+\}/g) ?? []);
    const lost = [...placeholders(english.json)].filter((placeholder) => !placeholders(translation.json).has(placeholder));
    return lost.length > 0 ? [severe(`lost the placeholders ${lost.join(', ')}`)] : [];
  }

  return shapeProblems(english.expression, english.source, translation.expression, translation.source).map(at);
}

/** Splices edits into a text: each is [from, to, text, order], none overlapping. */
function splice(text, edits)
{
  const sorted = [...edits].sort((a, b) => a[0] - b[0] || a[1] - b[1] || a[3] - b[3]);
  let result = '';
  let cursor = 0;

  for (const [from, to, replacement] of sorted)
  {
    if (from < cursor)
    {
      throw new Error('Two edits of the same catalog overlap.');
    }

    result += text.slice(cursor, from) + replacement;
    cursor = to;
  }

  return result + text.slice(cursor);
}

/** The range that removes a property with its comments and its comma. */
function removal(property, text)
{
  let end = property.end;
  const comma = /^\s*,/.exec(text.slice(end));

  if (comma)
  {
    end += comma[0].length;
  }

  return [property.getFullStart(), end, ''];
}

function apply()
{
  const plan = JSON.parse(fs.readFileSync(path.join(WORK, 'plan.json'), 'utf8'));
  const glossary = JSON.parse(fs.readFileSync(GLOSSARY, 'utf8'));
  let failed = false;

  for (const [language, work] of Object.entries(plan.languages))
  {
    const answers = new Map();
    const answered = new Set();

    for (const request of work.requests)
    {
      const file = path.join(WORK, request.replace('.request.md', '.answer.md'));

      if (fs.existsSync(file))
      {
        answered.add(request);
        readAnswer(fs.readFileSync(file, 'utf8')).forEach((value, key) => answers.set(key, value));
      }
    }
    const rules = glossary.languages[language]?.typography;
    const report = { applied: 0, kept: 0, missing: [], refused: [], warnings: [], removed: 0 };
    const accepted = [];

    for (const item of work.items)
    {
      const answer = answers.get(`${item.file} ${item.name}`);

      // A sentence asked to be checked may be left out of an answer: it was right.
      const checked = (item.kind === 'redo' || item.kind === 'stale') && answered.has(item.request);
      const unchanged = answer !== undefined && item.current !== null && answer.replace(/\s+/g, ' ') === item.current.replace(/\s+/g, ' ');

      if ((answer === undefined || answer.length === 0) && checked || unchanged)
      {
        report.kept++;
        continue;
      }

      if (answer === undefined || answer.length === 0)
      {
        report.missing.push(item.name);
        continue;
      }

      const problems = validate(item, answer);
      const severe = problems.filter((problem) => problem.severe);

      if (severe.length > 0)
      {
        report.refused.push(`${item.name}: ${severe.map((problem) => problem.text).join('; ')}`);
        continue;
      }

      report.warnings.push(...problems.map((problem) => `${item.name}: ${problem.text}`));
      accepted.push({ ...item, text: item.file === PAGES_FILE ? typesetJson(answer, rules) : typeset(withoutTypes(answer).text, rules) });
    }

    const byFile = Map.groupBy([...accepted.map((item) => ({ ...item, action: 'set' })), ...work.removals.map((item) => ({ ...item, action: 'remove' }))], (item) => item.file);

    for (const [file, items] of byFile)
    {
      if (file === PAGES_FILE)
      {
        writePages(language, items, report);
      }
      else
      {
        writeCatalog(language, file, items, report);
      }
    }

    const parts = [`${report.applied} translated`];

    if (report.kept > 0)
    {
      parts.push(`${report.kept} kept`);
    }

    if (report.removed > 0)
    {
      parts.push(`${report.removed} removed`);
    }

    if (report.missing.length > 0)
    {
      parts.push(`${report.missing.length} unanswered`);
    }

    if (report.refused.length > 0)
    {
      parts.push(`${report.refused.length} refused`);
    }

    console.log(`${language.padEnd(6)} ${parts.join(', ')}`);

    for (const name of report.missing)
    {
      console.log(`         unanswered: ${name}`);
    }

    for (const line of report.refused)
    {
      console.log(`         refused: ${line}`);
    }

    for (const line of report.warnings)
    {
      console.log(`         look at: ${line}`);
    }

    failed ||= report.missing.length > 0 || report.refused.length > 0;
  }

  if (failed)
  {
    console.log('\nWhat was refused or unanswered was left as it was. Extract again to ask for exactly that.');
    process.exitCode = 1;
  }
  else
  {
    console.log('\nNext: npm run build (or npx tsc --noEmit), and read the diff of the languages you know.');
  }
}

/** Whether an answer is an expression of the same shape as its English. */
function validate(item, answer)
{
  if (item.file === PAGES_FILE)
  {
    const expected = JSON.parse(item.english);

    try
    {
      const value = JSON.parse(answer);

      if (typeof expected === 'string')
      {
        return typeof value === 'string' ? treeProblems({ kind: 'leaf', json: item.english }, { kind: 'leaf', json: answer }) : [{ severe: true, text: 'is not a JSON string' }];
      }

      const same = value && typeof value === 'object' && Object.keys(expected).every((key) => typeof value[key] === 'string');
      return same ? [] : [{ severe: true, text: `is not a JSON object with ${Object.keys(expected).join(', ')}` }];
    }
    catch
    {
      return [{ severe: true, text: 'is not JSON' }];
    }
  }

  const stripped = withoutTypes(answer);

  if (stripped.error)
  {
    return [{ severe: true, text: `does not parse: ${stripped.error}` }];
  }

  const toTree = (parsed) => toNode(parsed.expression, parsed.source, null, 'answer.tsx', null);
  return treeProblems(toTree(parseExpression(item.english)), toTree(parseExpression(stripped.text)));
}

function writeCatalog(language, file, items, report)
{
  const absolute = path.join(LOCALES, language, file);
  let text = onDisk(absolute);

  if (text === null)
  {
    text = skeleton(file, items);
  }

  const catalog = parseCatalog(text, absolute, null);
  const english = parseCatalog(onDisk(path.join(LOCALES, SOURCE, file)), path.join(LOCALES, SOURCE, file), onDisk);
  const edits = [];

  items.forEach((item, order) =>
  {
    const segments = item.path;
    const node = nodeAt(catalog.roots, segments);

    if (item.action === 'remove')
    {
      if (node?.property)
      {
        edits.push([...removal(node.property, text), order]);
        report.removed++;
      }

      return;
    }

    if (node)
    {
      const start = node.expression.getStart(catalog.source);
      edits.push([start, node.expression.end, indent(item.text, indentationAt(text, start)), order]);
      report.applied++;
      return;
    }

    const parent = nodeAt(catalog.roots, segments.slice(0, -1));
    const englishParent = nodeAt(english.roots, segments.slice(0, -1));
    const key = segments.at(-1);

    if (parent?.kind !== 'object' || englishParent?.kind !== 'object')
    {
      report.refused.push(`${item.name}: there is no object to put it in`);
      return;
    }

    edits.push([...insertion(parent, englishParent, key, item.text, text, catalog.source), order]);
    report.applied++;
  });

  fs.mkdirSync(path.dirname(absolute), { recursive: true });
  fs.writeFileSync(absolute, splice(text, edits));
}

/**
 * Where a new property goes: after the nearest property English has before
 * it, or before the nearest one after it - written the way English writes it,
 * on the same line or on the next.
 */
function insertion(parent, englishParent, key, value, text, source)
{
  const keys = [...englishParent.children.keys()];
  const at = keys.indexOf(key);
  const englishProperty = englishParent.children.get(key).property;
  const englishSource = englishProperty.getSourceFile();
  const name = englishProperty.name.getText(englishSource);
  const ownLine = englishSource.text.slice(englishProperty.name.end, englishProperty.initializer.getStart(englishSource)).includes('\n');

  const written = (indentation) =>
    ownLine ? `${name}:\n${indentation}  ${indent(value, `${indentation}  `)}` : `${name}: ${indent(value, indentation)}`;

  for (let index = at - 1; index >= 0; index--)
  {
    const previous = parent.children.get(keys[index]);

    if (previous?.property)
    {
      const indentation = indentationAt(text, previous.property.getStart(source));
      const comma = /^\s*,/.exec(text.slice(previous.property.end));

      return comma
        ? [previous.property.end + comma[0].length, previous.property.end + comma[0].length, `\n${indentation}${written(indentation)},`]
        : [previous.property.end, previous.property.end, `,\n${indentation}${written(indentation)}`];
    }
  }

  for (let index = at + 1; index < keys.length; index++)
  {
    const next = parent.children.get(keys[index]);

    if (next?.property)
    {
      const start = next.property.getStart(source);
      const indentation = indentationAt(text, start);
      return [start, start, `${written(indentation)},\n${indentation}`];
    }
  }

  // An empty object: open it.
  const brace = unwrap(parent.expression).getStart(source);
  const indentation = indentationAt(text, brace) + '  ';
  return [brace + 1, unwrap(parent.expression).end - 1, `\n${indentation}${written(indentation)},\n${indentation.slice(2)}`];
}

/** A catalog file a language does not have yet: typed the way the other files of the language are. */
function skeleton(file, items)
{
  const englishText = onDisk(path.join(LOCALES, SOURCE, file));
  const typeName = /export type (\w+) = typeof (\w+);/.exec(englishText);
  const relative = path.posix.join(path.posix.relative(path.posix.dirname(file), '.') || '.', '..', SOURCE);
  const roots = items.filter((item) => item.path.length === 1).map((item) => item.path[0]);
  const lines = [];

  if (typeName)
  {
    lines.push(`import type { ${typeName[1]} } from '${relative}/${file.replace(/\.tsx?$/, '')}';`, '');
  }
  else
  {
    lines.push(`import type { Messages } from '${relative}';`, '');
  }

  for (const root of roots)
  {
    const type = typeName && typeName[2] === root ? typeName[1] : `Messages['${root}']`;
    lines.push(`export const ${root}: ${type} = {};`, '');
  }

  return lines.join('\n');
}

function writePages(language, items, report)
{
  const pages = JSON.parse(fs.readFileSync(PAGES, 'utf8'));

  for (const item of items)
  {
    const [route, key] = item.path;
    const page = pages[route];

    if (!page)
    {
      continue;
    }

    page.text ??= {};

    if (item.action === 'remove')
    {
      if (key !== undefined && page.text[language]?.[key] !== undefined)
      {
        delete page.text[language][key];
        report.removed++;
      }

      continue;
    }

    if (key === undefined)
    {
      // A new page: its title and description. Its social image is drawn apart.
      page.text[language] = JSON.parse(item.text);
    }
    else
    {
      page.text[language] ??= {};
      page.text[language][key] = JSON.parse(item.text);
    }

    report.applied++;
  }

  fs.writeFileSync(PAGES, JSON.stringify(pages, null, 2) + '\n');
}

// #endregion

// #region check and show

/**
 * Every sentence of every language against its English: what is missing or
 * extra, and what lost a part that is not words. Only the severe ones fail it;
 * the others are for a person to look at.
 */
function check()
{
  const targets = languages().filter((language) => language !== SOURCE);
  let severe = 0;
  let mild = 0;

  for (const file of catalogFiles(SOURCE))
  {
    const english = load(file, SOURCE, null).now;

    for (const language of targets)
    {
      const target = load(file, language, null).now;

      if (!target)
      {
        console.log(`error   ${language} ${file}: missing`);
        severe++;
        continue;
      }

      for (const [root, node] of english.roots)
      {
        const other = target.roots.get(root);
        const problems = other ? treeProblems(node, other, root) : [{ severe: true, text: `${root}: missing` }];

        for (const problem of problems)
        {
          console.log(`${problem.severe ? 'error  ' : 'look at'} ${language} ${file} ${problem.text}`);
          problem.severe ? severe++ : mild++;
        }
      }
    }
  }

  console.log(severe + mild === 0 ? 'Every language has what English has.' : `\n${severe} errors, ${mild} to look at.`);
  process.exitCode = severe === 0 ? 0 : 1;
}

/** One sentence in every language, to look a word up. */
function show(file, name)
{
  for (const language of languages())
  {
    const catalog = load(file, language, null).now;
    const node = [...walkLeaves(catalog?.roots)].find(([segments]) => formatPath(segments) === name)?.[1];
    console.log(`${language.padEnd(6)} ${node ? textOf(node) : '(none)'}`);
  }
}

function* walkLeaves(roots, prefix = [])
{
  for (const [key, node] of roots ?? [])
  {
    yield* walkNode(node, [...prefix, key]);
  }
}

function* walkNode(node, segments)
{
  yield [segments, node];

  if (node.kind === 'object')
  {
    for (const [key, childNode] of node.children)
    {
      yield* walkNode(childNode, [...segments, key]);
    }
  }
  else if (node.kind === 'array')
  {
    for (const [index, childNode] of node.children.entries())
    {
      yield* walkNode(childNode, [...segments, index]);
    }
  }
}

// #endregion

const [command, ...rest] = process.argv.slice(2);

switch (command)
{
  case 'extract':
  {
    const values = (flag) => rest.flatMap((value, index) => (rest[index - 1] === flag ? [value] : []));
    extract({ base: values('--base')[0], force: rest.includes('--force'), stale: rest.includes('--stale'), redo: values('--redo') });
    break;
  }
  case 'apply':
    apply();
    break;
  case 'check':
    check();
    break;
  case 'show':
    show(rest[0], rest[1]);
    break;
  default:
    console.log('Usage: node scripts/translations.mjs extract [--base <ref>] [--force] [--stale] [--redo <path>]... | apply | check | show <file> <path>');
    process.exitCode = 2;
}
