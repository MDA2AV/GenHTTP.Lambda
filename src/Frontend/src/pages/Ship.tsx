import { useEffect, useRef, useState } from 'react';

import { CopyField } from '../components/CopyField';
import { IconCheck, IconCopy, IconGlobe, IconLock, IconMail, IconSend } from '../components/Icons';
import { Reveal } from '../components/Reveal';
import { useT } from '../i18n';
import { Link } from '../i18n/links';
import { usePublicPage } from '../meta';
import { useBrowserValue, useLifetimes, useOrigin } from '../site';

const SOLUTIONS = 'solutions@genhttp.dev';

/**
 * For people who already built something with an agent and have it running
 * on their own machine. They do not need to be sold on building - they need
 * to hear that getting it in front of other people is one more sentence, and
 * that what they get back can do what a static host cannot: hold the state
 * that several people share.
 */
export function Ship() {
  usePublicPage('/ship');

  const said = useT().ship;

  const connect = useRef<HTMLElement>(null);
  const { origin, host } = useOrigin();

  const { offlineDays: offline, retentionDays: removed } = useLifetimes();

  const toConnect = () => connect.current?.scrollIntoView({ behavior: 'smooth', block: 'start' });

  return (
    <div className="relative w-full overflow-x-clip">
      <div className="aurora-field" aria-hidden="true">
        <div className="aurora aurora-a" />
        <div className="aurora aurora-b" />
        <div className="aurora aurora-c" />
        <div className="aurora-clearing" />
      </div>

      {/* ------------------------------------------------------------- the hero */}

      <section className="relative z-10 -mt-[3.75rem] px-5 pb-20 pt-[calc(3.75rem+2.5rem)] sm:px-6 lg:flex lg:min-h-[92vh] lg:items-center lg:pb-24">
        <div className="mx-auto grid w-full max-w-6xl items-center gap-12 lg:grid-cols-[1.05fr_1fr] lg:gap-16">
          <div>
            <h1 className="rise text-[2.6rem] font-bold leading-[1.02] tracking-tight sm:text-6xl">
              {said.title}
            </h1>

            <p
              className="rise mt-6 max-w-xl text-base leading-relaxed text-grey-800 sm:text-lg dark:text-grey-200"
              style={{ animationDelay: '80ms' }}
            >
              {said.intro}
            </p>

            <ul
              className="rise mt-6 flex flex-wrap gap-x-5 gap-y-2 text-sm font-medium text-grey-800 dark:text-grey-200"
              style={{ animationDelay: '140ms' }}
            >
              {said.facts.map((fact) => (
                <li key={fact} className="flex items-center gap-1.5">
                  <IconCheck className="h-4 w-4 text-emerald-600 dark:text-emerald-400" />
                  {fact}
                </li>
              ))}
            </ul>

            <div className="rise mt-9 flex flex-wrap items-center gap-3" style={{ animationDelay: '200ms' }}>
              <button type="button" onClick={toConnect} className="btn-primary px-7 py-3.5 text-base">
                {said.connect}
              </button>
              <Link to="/showcase" className="btn-ghost px-5 py-3.5 text-base">
                {said.seeOthers}
              </Link>
            </div>
          </div>

          <div className="rise" style={{ animationDelay: '260ms' }}>
            <GoingPublic host={host} />
          </div>
        </div>
      </section>

      {/* ------------------------------------------------------------ the steps */}

      <section className="relative z-10 mx-auto w-full max-w-6xl px-5 pb-24 sm:px-6" aria-labelledby="steps">
        <Reveal>
          <h2 id="steps" className="max-w-2xl text-2xl font-bold tracking-tight sm:text-3xl">
            {said.stepsTitle}
          </h2>
        </Reveal>

        <ol className="mt-10 grid gap-10 md:grid-cols-3 md:gap-8">
          {said.steps.map((step, i) => (
            <Reveal key={step.title} delay={80 + i * 80}>
              <li className="border-t-2 border-accent-500 pt-5 dark:border-accent-400">
                <span className="text-sm font-semibold tabular-nums text-accent-600 dark:text-accent-400">
                  {said.step(i + 1)}
                </span>
                <h3 className="mt-1 text-lg font-semibold">{step.title}</h3>
                <p className="mt-2 text-[15px] leading-relaxed text-grey-700 dark:text-grey-300">{step.body}</p>
              </li>
            </Reveal>
          ))}
        </ol>
      </section>

      {/* -------------------------------------------------------- together, live */}

      <section className="relative z-10 mx-auto w-full max-w-6xl px-5 pb-24 sm:px-6" aria-labelledby="together">
        <div className="grid gap-10 lg:grid-cols-[1fr_1.35fr] lg:gap-16">
          <Reveal>
            <h2 id="together" className="text-2xl font-bold tracking-tight sm:text-3xl">
              {said.togetherTitle}
            </h2>
            <p className="mt-4 text-[15px] leading-relaxed text-grey-700 dark:text-grey-300">{said.together}</p>
            <p className="mt-4 text-[15px] leading-relaxed text-grey-700 dark:text-grey-300">{said.together2}</p>
          </Reveal>

          <ul className="divide-y divide-grey-200 border-y border-grey-200 dark:divide-ink-800 dark:border-ink-800">
            {said.kinds.map((item, i) => (
              <Reveal key={item.name} delay={60 + i * 60}>
                <li className="grid gap-1 py-5 sm:grid-cols-[11rem_1fr] sm:gap-6">
                  <span className="flex items-center gap-2.5 font-semibold">
                    <span aria-hidden="true" className={`h-2.5 w-2.5 shrink-0 rounded-full ${DOTS[i]}`} />
                    {item.name}
                  </span>
                  <span className="pl-5 text-[15px] leading-relaxed text-grey-700 sm:pl-0 dark:text-grey-300">
                    {said.quote(item.ask)}
                  </span>
                </li>
              </Reveal>
            ))}
          </ul>
        </div>
      </section>

      {/* -------------------------------------------------------- the comparison */}

      <Comparison />

      {/* ------------------------------------------------------ connect an agent */}

      <section
        id="connect"
        ref={connect}
        className="relative z-10 mx-auto w-full max-w-4xl scroll-mt-16 px-5 pb-24 sm:px-6"
        aria-labelledby="connect-title"
      >
        <Reveal>
          <h2 id="connect-title" className="text-center text-2xl font-bold tracking-tight sm:text-3xl">
            {said.connectTitle}
          </h2>
          <p className="mx-auto mt-3 max-w-xl text-center text-[15px] leading-relaxed text-grey-700 dark:text-grey-300">
            {said.connectText}
          </p>
        </Reveal>

        <Reveal delay={100}>
          <div className="mx-auto mt-8 max-w-xl">
            <CopyField value={`${origin}/mcp`} tone="accent" />
          </div>
        </Reveal>

        <Reveal delay={160}>
          <AgentSetup origin={origin} />
        </Reveal>

        <Reveal delay={220}>
          <div className="mt-10">
            <h3 className="text-center font-semibold">{said.sayLike}</h3>
            <div className="mx-auto mt-4 grid max-w-2xl gap-3">
              {said.asks.map((ask) => (
                <SayThis key={ask} text={ask} />
              ))}
            </div>
          </div>
        </Reveal>
      </section>

      {/* ------------------------------------------------------ a domain of its own */}

      <section className="relative z-10 mx-auto w-full max-w-6xl px-5 pb-24 sm:px-6" aria-labelledby="domain">
        <Reveal>
          <div className="grid items-center gap-8 border border-logo-500/40 bg-white/70 p-6 backdrop-blur sm:p-10 md:grid-cols-[1fr_1fr] dark:border-logo-400/30 dark:bg-ink-900/70">
            <div>
              <span className="chip rounded-full bg-logo-500/10 text-logo-700 dark:bg-logo-400/10 dark:text-logo-400">
                {said.domainChip}
              </span>
              <h2 id="domain" className="mt-4 text-2xl font-bold tracking-tight sm:text-3xl">
                {said.domainTitle}
              </h2>
              <p className="mt-3 text-[15px] leading-relaxed text-grey-700 dark:text-grey-300">{said.domainText}</p>
              <a
                href={`mailto:${SOLUTIONS}?subject=${encodeURIComponent(said.domainSubject)}`}
                className="btn mt-6 border border-logo-500/50 text-logo-700 hover:bg-logo-500/10 dark:border-logo-400/40 dark:text-logo-400 dark:hover:bg-logo-400/10"
              >
                <IconMail />
                {said.domainAsk}
              </a>
            </div>

            <div aria-hidden="true" className="space-y-3">
              <AddressBar muted>{host}/lambda/q7x2k9</AddressBar>
              <div className="flex justify-center text-logo-500 dark:text-logo-400">
                <svg viewBox="0 0 24 24" className="h-5 w-5" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round">
                  <path d="M12 4v16M6 14l6 6 6-6" />
                </svg>
              </div>
              <AddressBar secure>quiznight.club</AddressBar>
            </div>
          </div>
        </Reveal>
      </section>

      {/* ------------------------------------------------------------- questions */}

      <section className="relative z-10 mx-auto w-full max-w-3xl px-5 pb-24 sm:px-6" aria-labelledby="questions">
        <h2 id="questions" className="text-center text-2xl font-bold tracking-tight sm:text-3xl">
          {said.questionsTitle}
        </h2>

        <div className="mt-8 divide-y divide-grey-200 border-y border-grey-200 dark:divide-ink-800 dark:border-ink-800">
          {said.questions(offline, removed, inline('/showcase'), inline('/terms')).map(([question, answer]) => (
            <details key={question} className="group">
              <summary className="flex min-h-[3.5rem] cursor-pointer list-none items-center justify-between gap-4 py-4 text-[15px] font-medium [&::-webkit-details-marker]:hidden">
                {question}
                <span aria-hidden="true" className="text-xl leading-none text-grey-500 transition-transform group-open:rotate-45">+</span>
              </summary>
              <p className="pb-5 pr-8 text-sm leading-relaxed text-grey-700 dark:text-grey-300">{answer}</p>
            </details>
          ))}
        </div>
      </section>

      {/* ----------------------------------------------------------------- close */}

      <section className="relative z-10 px-5 pb-24 text-center sm:px-6">
        <Reveal>
          <h2 className="text-3xl font-bold tracking-tight sm:text-4xl">{said.closeTitle}</h2>
          <p className="mt-2 text-3xl font-bold tracking-tight text-accent-600 sm:text-4xl dark:text-accent-400">
            {said.closeAccent}
          </p>
          <div className="mt-8 flex flex-wrap items-center justify-center gap-3">
            <button type="button" onClick={toConnect} className="btn-primary px-7 py-3.5 text-base">
              <IconSend className="h-4 w-4" />
              {said.connect}
            </button>
            <Link to="/build" className="btn-ghost px-5 py-3.5 text-base">
              {said.noAgent}
            </Link>
          </div>
          <p className="mt-5 text-sm text-grey-600 dark:text-grey-400">{said.closeFacts}</p>
        </Reveal>
      </section>
    </div>
  );
}

/* ================================================================== content */

/** The colour of each kind of thing people build together, in the order the words are in. */
const DOTS = ['bg-accent-500', 'bg-logo-500', 'bg-emerald-500', 'bg-amber-500', 'bg-red-500'];

/** A link in the middle of a sentence, around whatever words the language has for it. */
function inline(to: string) {
  return (text: string) => (
    <Link to={to} className="text-accent-600 hover:underline dark:text-accent-400">
      {text}
    </Link>
  );
}

/* ======================================================= the moment it goes out */

type Phase = 0 | 1 | 2 | 3 | 4;

/** When each phase starts, in milliseconds after the scene begins. */
const TIMELINE: [Phase, number][] = [
  [1, 700],
  [2, 1900],
  [3, 3500],
  [4, 4300],
];

const LOOP = 11000;

const PEOPLE = [
  { initial: 'M', colour: 'bg-accent-500' },
  { initial: 'J', colour: 'bg-logo-500' },
  { initial: 'S', colour: 'bg-emerald-500' },
  { initial: 'A', colour: 'bg-amber-500' },
  { initial: 'K', colour: 'bg-red-500' },
];

/**
 * The page in one picture: a request in a conversation, the address in the
 * bar turning from one only this machine can open into one anybody can, and
 * then people arriving. Plays on its own and starts again; somebody who asked
 * for no motion gets the last frame, which tells the whole story anyway.
 */
function GoingPublic({ host }: { host: string }) {
  const said = useT().ship.scene;
  const still = usePrefersStill();
  const [phase, setPhase] = useState<Phase>(still ? 4 : 0);
  const [arrived, setArrived] = useState(still ? PEOPLE.length : 0);
  // the scene fades out before it starts over, rather than running backwards
  const [fading, setFading] = useState(false);

  useEffect(() => {
    if (still) {
      setPhase(4);
      setArrived(PEOPLE.length);
      return;
    }

    const timers: number[] = [];

    const play = () => {
      setPhase(0);
      setArrived(0);
      setFading(false);

      timers.push(window.setTimeout(() => setFading(true), LOOP - 600));

      TIMELINE.forEach(([next, at]) => timers.push(window.setTimeout(() => setPhase(next), at)));

      PEOPLE.forEach((_, i) => timers.push(window.setTimeout(() => setArrived(i + 1), 4500 + i * 520)));
    };

    play();

    const loop = window.setInterval(play, LOOP);

    return () => {
      window.clearInterval(loop);
      timers.forEach(window.clearTimeout);
    };
  }, [still]);

  const live = phase >= 3;

  return (
    <figure
      className="surface relative overflow-hidden shadow-2xl shadow-accent-500/10"
      aria-label={said.label}
    >
      <div className={`transition-opacity duration-500 ${fading ? 'opacity-0' : 'opacity-100'}`}>
        {/* the conversation */}
        <div className="space-y-3 border-b border-grey-200 bg-grey-50/80 p-4 sm:p-5 dark:border-ink-800 dark:bg-ink-850/60">
          <div
            className={`ml-auto w-fit max-w-[88%] rounded-2xl rounded-br-sm bg-accent-500 px-4 py-2.5 text-sm text-white transition-all duration-500 dark:bg-accent-400 dark:text-grey-900 ${
              phase >= 1 ? 'translate-y-0 opacity-100' : 'translate-y-2 opacity-0'
            }`}
          >
            {said.ask}
          </div>

          <div
            className={`flex w-fit max-w-[88%] items-center gap-2 rounded-2xl rounded-bl-sm border border-grey-200 bg-white px-4 py-2.5 text-sm text-grey-800 transition-all duration-500 dark:border-ink-800 dark:bg-ink-900 dark:text-grey-200 ${
              phase >= 2 ? 'translate-y-0 opacity-100' : 'translate-y-2 opacity-0'
            }`}
          >
            {live ? (
              <>
                <IconCheck className="h-4 w-4 shrink-0 text-emerald-600 dark:text-emerald-400" />
                {said.live}
              </>
            ) : (
              <>
                <span className="flex gap-1" aria-hidden="true">
                  {[0, 1, 2].map((dot) => (
                    <span
                      key={dot}
                      className="h-1.5 w-1.5 animate-bounce rounded-full bg-grey-500"
                      style={{ animationDelay: `${dot * 140}ms` }}
                    />
                  ))}
                </span>
                {said.publishing}
              </>
            )}
          </div>
        </div>

        {/* the browser */}
        <div className="p-4 sm:p-5">
          <div
            className={`flex items-center gap-2.5 border px-3 py-2.5 transition-colors duration-500 ${
              live
                ? 'border-emerald-500/60 bg-emerald-50 dark:border-emerald-400/40 dark:bg-emerald-950'
                : 'border-grey-300 bg-white dark:border-ink-800 dark:bg-ink-900'
            }`}
          >
            {live ? (
              <IconLock className="h-4 w-4 shrink-0 text-emerald-600 dark:text-emerald-400" />
            ) : (
              <IconGlobe className="h-4 w-4 shrink-0 text-grey-500" />
            )}

            <span className="relative min-w-0 flex-1 overflow-hidden font-mono text-[13px] sm:text-sm">
              <span
                className={`block truncate transition-all duration-500 ${
                  live ? '-translate-y-full opacity-0' : 'translate-y-0 opacity-100'
                } text-grey-700 dark:text-grey-300`}
              >
                localhost:5173
              </span>
              <span
                className={`absolute inset-0 block truncate transition-all duration-500 ${
                  live ? 'translate-y-0 opacity-100' : 'translate-y-full opacity-0'
                } text-grey-900 dark:text-grey-100`}
              >
                {host}/lambda/q7x2k9
              </span>
            </span>

            <span
              className={`shrink-0 rounded-full px-2 py-0.5 text-[11px] font-semibold transition-colors duration-500 ${
                live
                  ? 'bg-emerald-500 text-white dark:bg-emerald-400 dark:text-grey-900'
                  : 'bg-grey-200 text-grey-700 dark:bg-ink-800 dark:text-grey-300'
              }`}
            >
              {live ? said.public : said.onlyYou}
            </span>
          </div>

          {/* the app, and who is in it */}
          <div className="mt-4 border border-grey-200 p-4 dark:border-ink-800">
            <div className="flex items-center justify-between gap-3">
              <span className="text-sm font-semibold">{said.app}</span>
              <span className="flex items-center gap-1.5 text-xs text-grey-600 dark:text-grey-400">
                <span
                  className={`h-2 w-2 rounded-full ${live ? 'animate-pulse bg-emerald-500' : 'bg-grey-400'}`}
                  aria-hidden="true"
                />
                {said.playing(<span className="tabular-nums">{live ? arrived + 1 : 1}</span>)}
              </span>
            </div>

            <div className="mt-4 flex items-center">
              <Avatar initial={said.you} colour="bg-grey-700 dark:bg-grey-500" shown />
              {PEOPLE.map((person, i) => (
                <Avatar key={person.initial} initial={person.initial} colour={person.colour} shown={live && i < arrived} />
              ))}
            </div>

            <div className="mt-4 space-y-2" aria-hidden="true">
              {[72, 54, 38].map((width, i) => (
                <div key={width} className="flex items-center gap-3">
                  <span className="w-3 text-xs tabular-nums text-grey-500">{i + 1}</span>
                  <div className="h-2 flex-1 bg-grey-100 dark:bg-ink-850">
                    <div
                      className={`h-full transition-[width] duration-1000 ease-out ${PEOPLE[i].colour}`}
                      style={{ width: live && arrived > i ? `${width}%` : '0%' }}
                    />
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>
    </figure>
  );
}

function Avatar({ initial, colour, shown }: { initial: string; colour: string; shown: boolean }) {
  return (
    <span
      className={`-ml-2 flex h-9 items-center justify-center rounded-full text-xs font-semibold text-white ring-2 ring-white transition-all duration-300 first:ml-0 dark:ring-ink-900 ${colour} ${
        initial.length > 1 ? 'px-3' : 'w-9'
      } ${shown ? 'scale-100 opacity-100' : 'scale-50 opacity-0'}`}
    >
      {initial}
    </span>
  );
}

function usePrefersStill(): boolean {
  // a prerendered page is drawn in motion, and stilled right after
  return useBrowserValue(() => window.matchMedia('(prefers-reduced-motion: reduce)').matches, false);
}

function AddressBar({ children, secure, muted }: { children: React.ReactNode; secure?: boolean; muted?: boolean }) {
  return (
    <div
      className={`flex items-center gap-2.5 border px-3 py-2.5 font-mono text-[13px] sm:text-sm ${
        secure
          ? 'border-logo-500/60 bg-white text-grey-900 dark:border-logo-400/50 dark:bg-ink-900 dark:text-grey-100'
          : 'border-grey-300 bg-white text-grey-600 dark:border-ink-800 dark:bg-ink-900 dark:text-grey-400'
      }`}
    >
      {secure ? (
        <IconLock className="h-4 w-4 shrink-0 text-logo-700 dark:text-logo-400" />
      ) : (
        <IconGlobe className="h-4 w-4 shrink-0 text-grey-500" />
      )}
      <span className={`truncate ${muted ? 'line-through decoration-grey-400/70' : ''}`}>{children}</span>
    </div>
  );
}

/* ============================================================ the comparison */

/** Who is compared, in the order their cells are in. */
const RIVALS = ['Vercel', 'Cloudflare', 'Lovable'];

function Comparison() {
  const said = useT().ship;
  const rows = said.rows;

  return (
    <section className="relative z-10 mx-auto w-full max-w-6xl px-5 pb-24 sm:px-6" aria-labelledby="compare">
      <Reveal>
        <h2 id="compare" className="max-w-2xl text-2xl font-bold tracking-tight sm:text-3xl">
          {said.compareTitle}
        </h2>
        <p className="mt-3 max-w-2xl text-[15px] leading-relaxed text-grey-700 dark:text-grey-300">{said.compareText}</p>
      </Reveal>

      {/* wide screens: one table, us first and marked */}
      <Reveal delay={120} className="mt-10 hidden md:block">
        <table className="w-full table-fixed border-collapse text-sm">
          <thead>
            <tr>
              <th className="w-[28%]" />
              <th scope="col" className="border-x-2 border-t-2 border-accent-500 bg-accent-500/[0.06] px-4 py-4 text-left dark:border-accent-400 dark:bg-accent-400/[0.06]">
                <span className="text-base font-semibold text-accent-600 dark:text-accent-400">GenHTTP Lambda</span>
              </th>
              {RIVALS.map((rival) => (
                <th key={rival} scope="col" className="px-4 py-4 text-left text-base font-semibold text-grey-700 dark:text-grey-300">
                  {rival}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {rows.map((row, r) => (
              <tr key={row} className="border-t border-grey-200 dark:border-ink-800">
                <th scope="row" className="py-4 pr-4 text-left font-medium text-grey-800 dark:text-grey-200">
                  {row}
                </th>
                <td
                  className={`border-x-2 border-accent-500 bg-accent-500/[0.06] px-4 py-4 dark:border-accent-400 dark:bg-accent-400/[0.06] ${
                    r === rows.length - 1 ? 'border-b-2' : ''
                  }`}
                >
                  <Good text={said.us[r]} />
                </td>
                {RIVALS.map((rival, i) => (
                  <td key={rival} className="px-4 py-4 text-grey-600 dark:text-grey-400">
                    {said.rivals[i][r]}
                  </td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </Reveal>

      {/* phones: a row at a time, with our answer up front and theirs beneath */}
      <div className="mt-8 space-y-4 md:hidden">
        {rows.map((row, r) => (
          <Reveal key={row} delay={60 + r * 50}>
            <div className="border border-grey-200 bg-white/70 p-4 backdrop-blur dark:border-ink-800 dark:bg-ink-900/70">
              <h3 className="text-[15px] font-semibold">{row}</h3>
              <div className="mt-3 border-l-2 border-accent-500 bg-accent-500/[0.06] px-3 py-2 dark:border-accent-400 dark:bg-accent-400/[0.06]">
                <span className="block text-xs font-medium text-accent-600 dark:text-accent-400">GenHTTP Lambda</span>
                <Good text={said.us[r]} />
              </div>
              <dl className="mt-3 grid grid-cols-[auto_1fr] gap-x-4 gap-y-1.5 text-sm">
                {RIVALS.map((rival, i) => (
                  <div key={rival} className="contents">
                    <dt className="text-grey-500">{rival}</dt>
                    <dd className="text-grey-700 dark:text-grey-300">{said.rivals[i][r]}</dd>
                  </div>
                ))}
              </dl>
            </div>
          </Reveal>
        ))}
      </div>

      <p className="mt-5 text-xs leading-relaxed text-grey-500">{said.compareNote}</p>
    </section>
  );
}

function Good({ text }: { text: string }) {
  return (
    <span className="flex items-start gap-2 font-medium text-grey-900 dark:text-grey-100">
      <IconCheck className="mt-0.5 h-4 w-4 shrink-0 text-emerald-600 dark:text-emerald-400" />
      {text}
    </span>
  );
}

/* ========================================================== connecting agents */

interface Setup {
  id: 'claudeCode' | 'claude' | 'cursor' | 'vscode';
  name: string;
  /** What to paste, where there is something to paste. */
  code?: (origin: string) => string;
  where?: string;
}

const SETUPS: Setup[] = [
  {
    id: 'claudeCode',
    name: 'Claude Code',
    code: (origin) => `claude mcp add --transport http genhttp ${origin}/mcp`,
  },
  {
    id: 'claude',
    name: 'Claude',
  },
  {
    id: 'cursor',
    name: 'Cursor',
    where: '~/.cursor/mcp.json',
    code: (origin) => JSON.stringify({ mcpServers: { genhttp: { url: `${origin}/mcp` } } }, null, 2),
  },
  {
    id: 'vscode',
    name: 'VS Code',
    where: '.vscode/mcp.json',
    code: (origin) => JSON.stringify({ servers: { genhttp: { type: 'http', url: `${origin}/mcp` } } }, null, 2),
  },
];

function AgentSetup({ origin }: { origin: string }) {
  const said = useT().ship;
  const [active, setActive] = useState(SETUPS[0].id);
  const tabs = useRef<(HTMLButtonElement | null)[]>([]);

  const setup = SETUPS.find((candidate) => candidate.id === active) ?? SETUPS[0];

  const intro = setup.id === 'claude' ? said.setups.claude((text) => <strong>{text}</strong>) : said.setups[setup.id];

  // arrow keys move between the tabs, the way a tab list is expected to work
  const onKey = (event: React.KeyboardEvent, index: number) => {
    const step = event.key === 'ArrowRight' ? 1 : event.key === 'ArrowLeft' ? -1 : 0;

    if (step === 0) {
      return;
    }

    event.preventDefault();

    const next = (index + step + SETUPS.length) % SETUPS.length;

    setActive(SETUPS[next].id);
    tabs.current[next]?.focus();
  };

  return (
    <div className="mt-8 border border-grey-200 bg-white/80 backdrop-blur dark:border-ink-800 dark:bg-ink-900/80">
      <div
        role="tablist"
        aria-label={said.yourAgent}
        className="flex overflow-x-auto border-b border-grey-200 [scrollbar-width:none] dark:border-ink-800 [&::-webkit-scrollbar]:hidden"
      >
        {SETUPS.map((candidate, i) => {
          const selected = candidate.id === active;

          return (
            <button
              key={candidate.id}
              ref={(element) => {
                tabs.current[i] = element;
              }}
              type="button"
              role="tab"
              id={`tab-${candidate.id}`}
              aria-selected={selected}
              aria-controls={`panel-${candidate.id}`}
              tabIndex={selected ? 0 : -1}
              onClick={() => setActive(candidate.id)}
              onKeyDown={(event) => onKey(event, i)}
              className={`relative shrink-0 whitespace-nowrap px-4 py-3.5 text-sm font-medium transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-inset focus-visible:ring-accent-500 ${
                selected
                  ? 'text-accent-600 dark:text-accent-400'
                  : 'text-grey-600 hover:text-grey-900 dark:text-grey-400 dark:hover:text-grey-100'
              }`}
            >
              {candidate.name}
              <span
                aria-hidden="true"
                className={`absolute inset-x-3 bottom-0 h-0.5 transition-opacity ${
                  selected ? 'bg-accent-500 opacity-100 dark:bg-accent-400' : 'opacity-0'
                }`}
              />
            </button>
          );
        })}
      </div>

      <div
        role="tabpanel"
        id={`panel-${setup.id}`}
        aria-labelledby={`tab-${setup.id}`}
        className="min-h-[12rem] p-5 sm:p-6"
      >
        <p className="text-[15px] leading-relaxed text-grey-700 dark:text-grey-300">{intro}</p>
        {setup.code && <CodeBlock code={setup.code(origin)} caption={setup.where ?? said.terminal} />}
      </div>

      <p className="border-t border-grey-200 px-5 py-4 text-sm leading-relaxed text-grey-600 sm:px-6 dark:border-ink-800 dark:text-grey-400">
        {said.elsewhere}
      </p>
    </div>
  );
}

/** Text to paste somewhere else, with a button that copies all of it. */
function CodeBlock({ code, caption }: { code: string; caption: string }) {
  const [copied, copy] = useCopy(code);
  const t = useT();

  return (
    <div className="mt-4 bg-ink-950 text-grey-200">
      <div className="flex items-center justify-between gap-3 border-b border-white/10 pl-4 pr-1">
        <span className="truncate font-mono text-xs text-grey-500">{caption}</span>
        <button
          type="button"
          onClick={copy}
          className="flex h-10 shrink-0 items-center gap-1.5 px-3 text-xs font-medium text-grey-400 transition-colors hover:text-white"
        >
          {copied ? <IconCheck className="h-4 w-4 text-emerald-400" /> : <IconCopy />}
          {copied ? t.common.copied : t.common.copy}
        </button>
      </div>
      <pre className="overflow-x-auto whitespace-pre-wrap break-all p-4 font-mono text-[13px] leading-relaxed sm:break-normal">
        {code}
      </pre>
    </div>
  );
}

/** Something to say to the agent, which copies itself when pressed. */
function SayThis({ text }: { text: string }) {
  const [copied, copy] = useCopy(text);
  const t = useT();

  return (
    <button
      type="button"
      onClick={copy}
      className="group flex w-full items-center gap-3 rounded-2xl rounded-br-sm bg-accent-500 px-4 py-3 text-left text-[15px] text-white shadow-sm transition-colors hover:bg-accent-600 dark:bg-accent-400 dark:text-grey-900 dark:hover:bg-accent-400/90"
    >
      <span className="flex-1">{text}</span>
      <span className="flex shrink-0 items-center gap-1 text-xs font-medium opacity-80 group-hover:opacity-100">
        {copied ? <IconCheck className="h-4 w-4" /> : <IconCopy />}
        <span className="hidden sm:inline">{copied ? t.common.copied : t.common.copy}</span>
      </span>
    </button>
  );
}

function useCopy(value: string): [boolean, () => void] {
  const [copied, setCopied] = useState(false);

  const copy = async () => {
    try {
      await navigator.clipboard.writeText(value);
    } catch {
      return;
    }

    setCopied(true);
    window.setTimeout(() => setCopied(false), 1600);
  };

  return [copied, () => void copy()];
}
