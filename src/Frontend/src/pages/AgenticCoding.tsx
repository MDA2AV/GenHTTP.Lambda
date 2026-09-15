import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';

import { CopyField } from '../components/CopyField';

/**
 * The page for somebody who has an agent and wants somewhere to put what it
 * builds.
 *
 * The rest of the site is written for a person holding the editor. This one is
 * written for a person holding a chat window: the point of contact is the MCP
 * address, and the editor is mentioned only as the place the keys take you
 * afterwards.
 */

const PROMPTS = [
  {
    ask: 'Build me a leaderboard for my running club and put it online.',
    got: 'A page anyone can post a time to, sorted, kept between visits.',
    like: '/lambda/example-shop/',
    likeName: 'closest example: the shop',
  },
  {
    ask: 'A shared whiteboard where everyone sees the same drawing as it happens.',
    got: 'A websocket per visitor and one drawing on the server.',
    like: '/lambda/example-arena/',
    likeName: 'closest example: the arena',
  },
  {
    ask: 'A guestbook for my wedding site that keeps the messages.',
    got: 'A form, a file that outlives the deployment, and a public address.',
    like: '/lambda/example-guestbook/',
    likeName: 'closest example: the guestbook',
  },
];

const SHOWCASE = [
  {
    name: 'Headwall',
    what: 'An aim map. Two sides, boxes to hide behind, and a wall only your head clears. The server judges every shot and rewinds a quarter of a second to the moment your browser was drawing.',
    href: '/lambda/example-shoot/',
  },
  {
    name: 'Arena',
    what: 'Everyone who opens it is in the same arena, eating each other and growing. The server holds one world and sends each player only what they can see, twenty times a second.',
    href: '/lambda/example-arena/',
  },
  {
    name: 'Tanks',
    what: 'Drive through a maze everybody shares, aim with the mouse, shoot the others. Six files, and the server decides all of it.',
    href: '/lambda/example-tanks/',
  },
  {
    name: 'Shop',
    what: 'A shelf, a basket, a checkout, and stock that actually comes off when an order is placed.',
    href: '/lambda/example-shop/',
  },
];

export function AgenticCoding() {
  const [origin, setOrigin] = useState('');

  useEffect(() => setOrigin(window.location.origin), []);

  return (
    <main className="mx-auto w-full max-w-4xl px-6 py-16">
      <p className="text-xs uppercase tracking-[0.2em] text-slate-400">For agentic coding</p>

      <h1 className="mt-3 max-w-2xl text-4xl font-light tracking-tight sm:text-5xl">
        Somewhere to put what your agent builds.
      </h1>

      <p className="mt-5 max-w-2xl text-lg text-slate-500">
        Give your coding agent one address and it can write a web application, put it online and
        hand you back a link you can send to anyone. No account to make, no server to rent, nothing
        to install.
      </p>

      <div className="surface mt-8 p-5">
        <p className="text-xs uppercase tracking-[0.2em] text-slate-400">The address</p>
        <div className="mt-3">
          <CopyField value={`${origin}/mcp`} tone="accent" />
        </div>
        <p className="mt-3 text-sm text-slate-500">
          That is an MCP server. Anything that speaks MCP can use it - it needs no key and nothing
          to sign up for.
        </p>
      </div>

      {/* ------------------------------------------------------ the two ways in */}

      <h2 className="mt-14 text-2xl font-light tracking-tight">Two minutes, either way</h2>

      <div className="mt-6 grid gap-4 sm:grid-cols-2">
        <div className="surface p-5">
          <h3 className="font-medium">Claude Code</h3>
          <p className="mt-1 text-sm text-slate-500">In a terminal, once:</p>
          <pre className="mt-3 overflow-x-auto rounded-md bg-slate-900 p-3 text-xs text-slate-100 dark:bg-black/40">
{`claude mcp add --transport http genhttp ${origin || 'https://genhttp.dev'}/mcp`}
          </pre>
          <p className="mt-3 text-sm text-slate-500">
            Then just ask: <em>build me a poll and put it online</em>. It will come back with two
            links.
          </p>
        </div>

        <div className="surface p-5">
          <h3 className="font-medium">Claude on the web</h3>
          <ol className="mt-2 list-decimal space-y-1.5 pl-5 text-sm text-slate-500">
            <li>Settings, then Connectors, then Add custom connector.</li>
            <li>
              Paste <code className="text-slate-600 dark:text-slate-300">{origin}/mcp</code> as the
              remote MCP server URL.
            </li>
            <li>Start a chat and ask for what you want built.</li>
          </ol>
          <p className="mt-3 text-sm text-slate-500">
            No API key and no OAuth step: the connector is open.
          </p>
        </div>
      </div>

      <p className="mt-4 text-sm text-slate-500">
        Not got an agent handy? <Link to="/build" className="underline">Type it here instead</Link>{' '}
        and one on this machine will build it for you.
      </p>

      {/* ------------------------------------------------- why not a static host */}

      <h2 className="mt-14 text-2xl font-light tracking-tight">
        The difference from a static host
      </h2>

      <p className="mt-4 max-w-2xl text-slate-500">
        GitHub Pages, Netlify and the rest will serve your agent's HTML beautifully, and then stop.
        Every visitor gets their own copy and nothing is remembered. What runs here is a real web
        server, so the thing your agent builds can:
      </p>

      <div className="mt-6 grid gap-px overflow-hidden rounded-lg border border-slate-200 bg-slate-200 sm:grid-cols-3 dark:border-ink-800 dark:bg-ink-800">
        {[
          ['Keep data', 'Scores, entries, orders, messages. Written to a workspace that survives the deployment, not to somebody’s browser.'],
          ['Join people up', 'Websockets, so two people on the same page see the same thing at the same moment.'],
          ['Answer as an API', 'Routes, JSON, an OpenAPI document. Something else can talk to it, not just a browser.'],
        ].map(([title, body]) => (
          <div key={title} className="bg-white p-5 dark:bg-ink-900">
            <h3 className="font-medium">{title}</h3>
            <p className="mt-1.5 text-sm text-slate-500">{body}</p>
          </div>
        ))}
      </div>

      <p className="mt-6 max-w-2xl text-slate-500">
        Underneath is{' '}
        <a href="https://genhttp.dev" className="underline" target="_blank" rel="noreferrer">
          GenHTTP
        </a>
        , so the pieces an application usually needs are already there rather than being written
        from scratch: authentication, including basic, bearer and API keys; websockets; static
        files and single page applications; caching, compression and range requests; an OpenAPI
        document generated from the routes. Your agent can ask the platform guide what is
        available before it writes anything.
      </p>

      {/* ----------------------------------------------------------- what to ask */}

      <h2 className="mt-14 text-2xl font-light tracking-tight">Things worth asking for</h2>

      <div className="mt-6 space-y-3">
        {PROMPTS.map((p) => (
          <div key={p.ask} className="surface p-5">
            <p className="font-medium">&ldquo;{p.ask}&rdquo;</p>
            <p className="mt-1.5 text-sm text-slate-500">{p.got}</p>
            <a href={p.like} className="mt-2 inline-block text-sm underline" target="_blank" rel="noreferrer">
              {p.likeName}
            </a>
          </div>
        ))}
      </div>

      {/* ------------------------------------------------------------- showcase */}

      <h2 className="mt-14 text-2xl font-light tracking-tight">Already running here</h2>

      <p className="mt-3 max-w-2xl text-slate-500">
        These are lambdas on this platform, open right now. Nothing to install - they are links.
      </p>

      <div className="mt-6 grid gap-4 sm:grid-cols-2">
        {SHOWCASE.map((s) => (
          <a
            key={s.name}
            href={s.href}
            target="_blank"
            rel="noreferrer"
            className="surface group p-5 transition hover:opacity-80"
          >
            <h3 className="flex items-center justify-between font-medium">
              {s.name}
              <span aria-hidden className="text-slate-400">&rarr;</span>
            </h3>
            <p className="mt-1.5 text-sm text-slate-500">{s.what}</p>
          </a>
        ))}
      </div>

      {/* ---------------------------------------------------------- the limits */}

      <h2 className="mt-14 text-2xl font-light tracking-tight">What this costs, and what it is not</h2>

      <div className="surface mt-6 p-6">
        <p className="text-slate-500">
          Nothing. Basic hosting is free and is meant to stay that way.
        </p>

        <p className="mt-4 text-slate-500">
          It is also early, and the limits say so honestly rather than pretending otherwise. A
          deployment stays online for <strong className="font-medium text-slate-700 dark:text-slate-200">a day</strong>,
          and putting it back up is one press of deploy in the editor - the link does not change.
          A lambda you have not touched for{' '}
          <strong className="font-medium text-slate-700 dark:text-slate-200">thirty days</strong> is
          removed. There is no account, so the editor key you are handed is the only way back into
          what you made: keep it somewhere.
        </p>

        <p className="mt-4 text-slate-500">
          That makes this a good place for something to exist, be shared and be played with. It is
          not yet the place for something you would be upset to lose.
        </p>
      </div>

      <div className="mt-10 flex flex-wrap items-center gap-3">
        <Link to="/build" className="btn btn-primary">Have one built for you</Link>
        <Link to="/guide" className="btn btn-ghost">Or read how it works</Link>
      </div>
    </main>
  );
}
