import { useEffect, type ReactNode } from 'react';
import { useLocation } from 'react-router-dom';

import {
  IconBook, IconCheck, IconCode, IconDatabase, IconDraft, IconEye, IconGlobe, IconHistory, IconKey, IconPencil, IconSpark, IconStar,
} from '@genhttp-lambda/kit/components/Icons';
import { Reveal } from '@genhttp-lambda/kit/components/Reveal';
import { Link } from '@genhttp-lambda/kit/i18n/links';
import { REPOSITORY } from '@genhttp-lambda/kit/installation';
import { usePublicPage } from '@genhttp-lambda/kit/meta';

import { Screen } from '../components/Screen';
import { CONTACT_MAIL } from '../contact';
import { useT } from '../i18n';
import type { Messages } from '../locales/en';

type Said = Messages['platform'];

type Key = keyof Said['features'];

/** How each feature is drawn: its icon, and the picture its section shows. */
const LOOKS: Record<Key, { icon: (p: { className?: string }) => ReactNode; screen?: Omit<ScreenOf, 'alt'> }> = {
  build: { icon: IconSpark, screen: { name: 'app', single: true, width: 1800, height: 1140, address: 'kestrel-courts.genhttp.run' } },
  change: { icon: IconPencil, screen: { name: 'change', width: 1800, height: 1050, address: 'genhttp.dev/editor/••••••/change' } },
  drafts: { icon: IconDraft, screen: { name: 'draft', single: true, width: 1800, height: 1140, address: 'genhttp.dev/features/3fa9c1…/' } },
  history: { icon: IconHistory, screen: { name: 'history', width: 1800, height: 1050, address: 'genhttp.dev/editor/••••••/history' } },
  about: { icon: IconBook, screen: { name: 'about', width: 1800, height: 1050, address: 'genhttp.dev/editor/••••••/docs' } },
  records: { icon: IconDatabase, screen: { name: 'records', width: 1800, height: 1050, address: 'genhttp.dev/editor/••••••/database' } },
  keys: { icon: IconKey, screen: { name: 'keys', width: 1422, height: 375, address: 'genhttp.dev/editor/••••••/secrets' } },
  analytics: { icon: IconEye, screen: { name: 'analytics', width: 1800, height: 1050, address: 'genhttp.dev/editor/••••••/analytics' } },
  domain: { icon: IconGlobe },
  code: { icon: IconCode, screen: { name: 'code', width: 1800, height: 1050, address: 'genhttp.dev/editor/••••••/code' } },
  publish: { icon: IconStar, screen: { name: 'source', width: 1800, height: 1050, address: 'genhttp.dev/en/source/kestrel-courts' } },
};

interface ScreenOf {
  name: string;
  single?: boolean;
  width: number;
  height: number;
  address: string;
  alt: string;
}

/** The three parts of the page, and the features in each, in the order they are read. */
const GROUPS: { id: string; features: Key[] }[] = [
  { id: 'make', features: ['build', 'change'] },
  { id: 'behind', features: ['drafts', 'history', 'about', 'records', 'keys', 'analytics', 'domain'] },
  { id: 'yours', features: ['code', 'publish'] },
];

/**
 * What every app gets, for whoever wonders what is behind "describe it and it
 * is online": building and changing it briefly, since the other pages say
 * that at length, and then what keeps an app useful once people use it - each
 * in a section of its own, with a screenshot of the real editor.
 *
 * Every picture shows one example app, so the page reads as one app's story
 * rather than as a catalogue: a tennis club's court booking, built in the
 * summer and changed since. The pictures were taken from a local
 * installation of the platform - nothing in them is drawn.
 */
export function Platform() {
  usePublicPage('/platform');

  const said = useT().platform;
  const location = useLocation();

  // a link from another page may name a section; it is there once laid out
  useEffect(() => {
    const id = location.hash.slice(1);

    if (!id) {
      return;
    }

    const timer = window.setTimeout(() => document.getElementById(id)?.scrollIntoView({ block: 'start' }), 80);

    return () => window.clearTimeout(timer);
  }, [location]);

  // anchors, so they lead somewhere without a script - and scroll smoothly where one runs
  const jump = (id: string) => (event: React.MouseEvent) => {
    const target = document.getElementById(id);

    if (target) {
      event.preventDefault();
      target.scrollIntoView({ behavior: 'smooth', block: 'start' });
      history.replaceState(null, '', `#${id}`);
    }
  };

  const titles: Record<string, { title: string; text: string }> = {
    make: { title: said.makeTitle, text: said.makeText },
    behind: { title: said.behindTitle, text: said.behindText },
    yours: { title: said.yoursTitle, text: said.yoursText },
  };

  let index = 0;

  return (
    <div className="relative w-full overflow-x-clip">
      <div className="aurora-field" aria-hidden="true">
        <div className="aurora aurora-a" />
        <div className="aurora aurora-b" />
        <div className="aurora aurora-c" />
        <div className="aurora-clearing" />
      </div>

      {/* ------------------------------------------------------------- the hero */}

      <section className="relative z-10 -mt-[3.75rem] px-5 pb-14 pt-[calc(3.75rem+3.5rem)] text-center sm:px-6 sm:pt-[calc(3.75rem+5rem)]">
        <p className="rise text-balance text-xs font-medium uppercase tracking-[0.2em] text-accent-600 dark:text-accent-400">{said.eyebrow}</p>

        <h1 className="rise mx-auto mt-5 max-w-5xl text-4xl font-bold leading-[1.05] tracking-tight sm:text-6xl" style={{ animationDelay: '60ms' }}>
          {said.title}
          <span className="block text-accent-700 dark:text-accent-400">{said.titleAccent}</span>
        </h1>

        <p
          className="rise mx-auto mt-6 max-w-2xl text-base leading-relaxed text-grey-800 sm:text-lg dark:text-grey-200"
          style={{ animationDelay: '120ms' }}
        >
          {said.intro}
        </p>

        <div className="rise mt-8 flex flex-wrap items-center justify-center gap-x-4 gap-y-3" style={{ animationDelay: '200ms' }}>
          <Link to="/build" className="btn-primary px-7 py-3.5 text-base">
            {said.build}
          </Link>
          <Link to="/ship" className="btn-ghost px-5 py-3.5 text-base">
            {said.ownAgent}
          </Link>
        </div>

        <p className="rise mt-5 text-sm text-grey-700 dark:text-grey-300" style={{ animationDelay: '260ms' }}>
          {said.free}
        </p>
      </section>

      {/* the app's editor, which every section below is a part of */}
      <div className="relative z-10 mx-auto w-full max-w-5xl px-5 sm:px-6">
        <Reveal>
          <Screen
            name="overview"
            width={1800}
            height={1050}
            alt={said.heroAlt}
            address="genhttp.dev/editor/••••••"
            zoom={said.zoom}
            close={said.close}
          />
          <p className="mx-auto mt-4 max-w-2xl text-center text-xs leading-relaxed text-grey-600 dark:text-grey-400">{said.example}</p>
        </Reveal>
      </div>

      {/* ---------------------------------------------------- every feature, at a glance */}

      <nav aria-label={said.contents} className="relative z-10 mx-auto w-full max-w-4xl px-5 pt-16 sm:px-6">
        <p className="text-center text-xs font-medium uppercase tracking-[0.2em] text-grey-600 dark:text-grey-400">{said.contents}</p>
        <ul className="mt-5 flex flex-wrap justify-center gap-2">
          {GROUPS.flatMap((group) => group.features).map((key) => {
            const Icon = LOOKS[key].icon;

            return (
              <li key={key}>
                <a
                  href={`#${key}`}
                  onClick={jump(key)}
                  className="chip rounded-full border border-grey-300 bg-white/70 px-3.5 py-1.5 text-[13px] text-grey-800 backdrop-blur transition-colors hover:border-accent-500/60 hover:text-accent-600 dark:border-ink-800 dark:bg-ink-900/70 dark:text-grey-200 dark:hover:border-accent-400/60 dark:hover:text-accent-400"
                >
                  <Icon className="h-3.5 w-3.5" />
                  {said.features[key].name}
                </a>
              </li>
            );
          })}
        </ul>
      </nav>

      {/* ------------------------------------------------------------- the parts */}

      {GROUPS.map((group) => (
        <section key={group.id} id={group.id} aria-labelledby={`${group.id}-title`} className="relative z-10 mx-auto w-full max-w-6xl scroll-mt-20 px-5 pt-24 sm:px-6 lg:pt-32">
          <Reveal>
            <div className="border-t border-grey-300 pt-8 dark:border-ink-800">
              <h2 id={`${group.id}-title`} className="text-3xl font-bold tracking-tight sm:text-4xl">
                {titles[group.id].title}
              </h2>
              <p className="mt-3 max-w-2xl text-[15px] leading-relaxed text-grey-700 dark:text-grey-300">{titles[group.id].text}</p>
            </div>
          </Reveal>

          <div className="mt-14 space-y-20 lg:mt-16 lg:space-y-28">
            {group.features.map((key) => (
              <Feature key={key} id={key} said={said} flip={index++ % 2 === 1} />
            ))}
          </div>
        </section>
      ))}

      {/* ----------------------------------------------------------------- close */}

      <section className="relative z-10 px-5 pb-24 pt-28 text-center sm:px-6 lg:pt-36">
        <Reveal>
          <h2 className="text-3xl font-bold tracking-tight sm:text-4xl">
            {said.closeTitle} <span className="text-accent-600 dark:text-accent-400">{said.closeAccent}</span>
          </h2>
          <div className="mt-8 flex flex-wrap items-center justify-center gap-3">
            <Link to="/build" className="btn-primary px-7 py-3.5 text-base">
              {said.closeBuild}
            </Link>
            <Link to="/ship" className="btn-ghost px-5 py-3.5 text-base">
              {said.closeShip}
            </Link>
          </div>
          <p className="mx-auto mt-8 max-w-xl text-sm leading-relaxed text-grey-700 dark:text-grey-300">
            {said.openSource(
              (text) => (
                <Link to="/self-hosting" className="text-accent-600 hover:underline dark:text-accent-400">
                  {text}
                </Link>
              ),
              (text) => (
                <a href={REPOSITORY} target="_blank" rel="noreferrer" className="text-accent-600 hover:underline dark:text-accent-400">
                  {text}
                </a>
              ),
            )}
          </p>
        </Reveal>
      </section>
    </div>
  );
}

/**
 * One feature: its name, what it does and three points beside its picture,
 * which change sides from one section to the next on a wide screen and come
 * after the words on a narrow one.
 */
function Feature({ id, said, flip }: { id: Key; said: Said; flip: boolean }) {
  const feature = said.features[id];
  const look = LOOKS[id];
  const Icon = look.icon;

  const picture = look.screen ? (
    <Screen
      {...look.screen}
      alt={feature.alt}
      zoom={said.zoom}
      close={said.close}
      badge={
        id === 'drafts' ? (
          <span className="chip shrink-0 rounded-full bg-amber-500/15 text-amber-600 dark:bg-amber-400/15 dark:text-amber-400">{said.draftBadge}</span>
        ) : undefined
      }
    />
  ) : (
    <OwnDomain alt={feature.alt} premium={said.domainOwn} />
  );

  return (
    <article id={id} aria-labelledby={`${id}-title`} className="grid scroll-mt-24 items-center gap-8 lg:grid-cols-12 lg:gap-14">
      {/* min-w-0: an address in a frame is no reason for a column to be wider than the screen */}
      <Reveal className={`min-w-0 lg:col-span-5 ${flip ? 'lg:order-2' : ''}`}>
        <span className="chip rounded-full bg-accent-500/10 text-accent-700 dark:bg-accent-400/10 dark:text-accent-400">
          <Icon className="h-3.5 w-3.5" />
          {feature.name}
        </span>
        <h3 id={`${id}-title`} className="mt-4 text-2xl font-bold tracking-tight sm:text-3xl">
          {feature.title}
        </h3>
        <p className="mt-3 text-[15px] leading-relaxed text-grey-700 dark:text-grey-300">{feature.text}</p>
        <ul className="mt-5 space-y-2.5">
          {feature.points.map((point) => (
            <li key={point} className="flex gap-2.5 text-sm text-grey-800 dark:text-grey-200">
              <IconCheck className="mt-0.5 h-4 w-4 shrink-0 text-emerald-600 dark:text-emerald-400" />
              {point}
            </li>
          ))}
        </ul>
        {id === 'domain' && (
          <a
            href={`mailto:${CONTACT_MAIL}?subject=${encodeURIComponent(said.domainSubject)}`}
            className="btn mt-6 border border-logo-500/50 text-logo-700 hover:bg-logo-500/10 dark:border-logo-400/40 dark:text-logo-400 dark:hover:bg-logo-400/10"
          >
            {said.domainAsk}
          </a>
        )}
      </Reveal>

      <Reveal delay={120} className={`min-w-0 lg:col-span-7 ${flip ? 'lg:order-1' : ''}`}>
        {picture}
      </Reveal>
    </article>
  );
}

/**
 * The domain, drawn rather than shot: what changes is the address in the
 * bar, which a screenshot of the editor would not show - the app's address
 * here above, the club's own below, the same page in both.
 */
function OwnDomain({ alt, premium }: { alt: string; premium: string }) {
  return (
    <figure className="surface p-6 shadow-xl shadow-grey-900/5 sm:p-10 dark:shadow-black/30" role="img" aria-label={alt}>
      <div aria-hidden="true" className="mx-auto max-w-md space-y-3">
        <Bar muted>kestrel-courts.genhttp.run</Bar>
        <div className="flex justify-center text-logo-500 dark:text-logo-400">
          <svg viewBox="0 0 24 24" className="h-5 w-5" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round">
            <path d="M12 4v16M6 14l6 6 6-6" />
          </svg>
        </div>
        <Bar premium={premium}>book.kestrellane.club</Bar>

        {/* the same page at both: the club's booking, in a few strokes */}
        <div className="mt-6 border border-grey-200 dark:border-ink-800">
          <div className="flex items-center gap-2 bg-[#1f5c45] px-3 py-2 text-[11px] font-semibold text-white">
            <span className="h-3 w-3 rounded-full bg-[#d9e85a]" />
            Kestrel Lane Tennis Club
          </div>
          <div className="grid grid-cols-4 gap-1.5 bg-white p-3 dark:bg-ink-900">
            {Array.from({ length: 12 }, (_, i) => (
              <span
                key={i}
                className={`h-4 ${[1, 2, 5, 7, 8, 11].includes(i) ? 'bg-[#f7e3d9] dark:bg-[#5a3a2e]' : 'border border-dashed border-grey-300 dark:border-ink-700'}`}
              />
            ))}
          </div>
        </div>
      </div>
    </figure>
  );
}

function Bar({ children, muted, premium }: { children: ReactNode; muted?: boolean; premium?: string }) {
  return (
    <div
      className={`flex items-center gap-2.5 border px-3 py-2.5 font-mono text-[13px] sm:text-sm ${
        muted
          ? 'border-grey-300 bg-white text-grey-600 dark:border-ink-800 dark:bg-ink-900 dark:text-grey-400'
          : 'border-logo-500/50 bg-white text-grey-900 dark:border-logo-400/40 dark:bg-ink-900 dark:text-grey-100'
      }`}
    >
      <IconGlobe className={`h-4 w-4 shrink-0 ${muted ? 'text-grey-500' : 'text-logo-500 dark:text-logo-400'}`} />
      <span className={`min-w-0 flex-1 truncate ${muted ? 'line-through decoration-grey-400/70' : ''}`}>{children}</span>
      {premium && <span className="chip shrink-0 rounded-full bg-logo-500/10 text-logo-700 dark:bg-logo-400/10 dark:text-logo-400">{premium}</span>}
    </div>
  );
}
