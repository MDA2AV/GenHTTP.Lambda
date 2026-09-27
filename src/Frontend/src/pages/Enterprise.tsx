import { useState } from 'react';

import { IconCheck, IconLayers, IconMail, IconSpark } from '../components/Icons';
import { useT } from '../i18n';
import { Link } from '../i18n/links';
import { usePublicPage } from '../meta';
import { useLifetimes } from '../site';

/** What an enterprise seat costs, per month. */
const SEAT = 5;

/** Where an enterprise enquiry goes. */
const SOLUTIONS = 'solutions@genhttp.dev';

/**
 * The same address with a name in front, so the mail being written is to
 * somebody rather than to a bare address. Only the name and the brackets are
 * escaped - most mail programs show the address as it is written here.
 */
const CONTACT = `mailto:GenHTTP%20Solutions%20%3C${SOLUTIONS}%3E`;

type Tone = 'free' | 'enterprise';

/**
 * The tiers and what they cost.
 *
 * Two of them: this installation, free for anybody, and an installation of
 * your own. The free card leads to building one, since there is nothing to
 * sign up for, and the enterprise one opens a mail to the people who would
 * set it up.
 */
export function Enterprise() {
  usePublicPage('/enterprise');

  const said = useT().enterprise;

  // both counted from the last visit or edit, not from the deployment: a free
  // lambda stays up while it is used, goes offline once it is not, and is
  // removed a while after that
  const { offlineDays: offline, retentionDays: removed } = useLifetimes();

  return (
    <div className="mx-auto w-full max-w-6xl px-5 pb-24 pt-12 sm:px-6 sm:pt-16">
      <header className="rise mx-auto max-w-2xl text-center">
        <p className="text-xs font-medium uppercase tracking-[0.2em] text-accent-500 dark:text-accent-400">{said.eyebrow}</p>
        <h1 className="mt-3 text-3xl font-bold tracking-tight sm:text-4xl">{said.title}</h1>
        <p className="mt-3 text-[15px] leading-relaxed text-slate-600 dark:text-slate-400">{said.intro}</p>
      </header>

      <div className="mx-auto mt-12 grid max-w-4xl gap-5 md:grid-cols-2 md:items-stretch">
        <Tier
          tone="free"
          name={said.free}
          tagline={said.freeTagline}
          price={said.price(0)}
          unit={said.forever}
          action={{ label: said.buildOne, href: '/build' }}
          delay={0}
          features={said.freeFeatures(offline, removed)}
        >
          <Footnote>{said.freeNote}</Footnote>
        </Tier>

        <Tier
          tone="enterprise"
          name={said.name}
          tagline={said.tagline}
          price={said.price(SEAT)}
          unit={said.perUser}
          action={{ label: said.contact, href: CONTACT }}
          delay={80}
          features={said.features}
        >
          <Estimator />
        </Tier>
      </div>

      <Comparison offline={offline} removed={removed} />

      <Questions />
    </div>
  );
}

const TONES: Record<Tone, { text: string; ring: string; fill: string; soft: string; button: string }> = {
  free: {
    text: 'text-accent-500 dark:text-accent-400',
    ring: 'border-grey-300 dark:border-ink-800',
    fill: 'bg-accent-500 dark:bg-accent-400',
    soft: 'bg-accent-500/10 dark:bg-accent-400/10',
    button: 'btn border border-grey-300 text-accent-500 hover:bg-accent-500/10 dark:border-ink-700 dark:text-accent-400 dark:hover:bg-accent-400/10',
  },
  enterprise: {
    text: 'text-enterprise-700 dark:text-enterprise-400',
    ring: 'border-enterprise-500 dark:border-enterprise-400',
    fill: 'bg-enterprise-500',
    soft: 'bg-enterprise-500/10',
    button: 'btn bg-enterprise-700 text-white shadow-sm hover:bg-enterprise-700/90 hover:shadow dark:bg-enterprise-400 dark:text-grey-900 dark:hover:bg-enterprise-400/90',
  },
};

const ICONS: Record<Tone, React.ReactNode> = {
  free: <IconSpark className="h-5 w-5" />,
  enterprise: <IconLayers className="h-5 w-5" />,
};

interface TierProps {
  tone: Tone;
  name: string;
  tagline: string;
  price: string;
  unit: string;
  action: { label: string; href: string };
  features: string[];
  delay: number;
  children?: React.ReactNode;
}

function Tier({ tone, name, tagline, price, unit, action, features, delay, children }: TierProps) {
  const colours = TONES[tone];
  const featured = tone === 'enterprise';

  return (
    <section
      className={`rise relative flex flex-col border bg-white p-6 dark:bg-ink-900 sm:p-7 ${colours.ring} ${
        featured ? 'border-2 shadow-xl shadow-enterprise-500/10' : ''
      }`}
      style={{ animationDelay: `${delay}ms` }}
      aria-labelledby={`tier-${tone}`}
    >
      {/* the colour of the tier, across the top of its card */}
      <div aria-hidden="true" className={`absolute inset-x-0 top-0 h-1 ${colours.fill}`} />

      <div className="flex items-center gap-3">
        <span className={`inline-flex h-10 w-10 items-center justify-center ${colours.soft} ${colours.text}`}>
          {ICONS[tone]}
        </span>
        <div>
          <h2 id={`tier-${tone}`} className={`text-lg font-semibold tracking-tight ${colours.text}`}>{name}</h2>
          <p className="text-sm text-slate-500">{tagline}</p>
        </div>
      </div>

      <p className="mt-7 flex items-baseline gap-2">
        <span className="text-5xl font-bold tracking-tight text-grey-900 dark:text-grey-100">{price}</span>
        <span className="text-sm text-slate-500">{unit}</span>
      </p>

      {action.href.startsWith('mailto:') ? (
        <a href={action.href} className={`${colours.button} mt-6 w-full py-2.5`}>
          <IconMail />
          {action.label}
        </a>
      ) : (
        <Link to={action.href} className={`${colours.button} mt-6 w-full py-2.5`}>
          {action.label}
        </Link>
      )}

      <ul className="mt-7 space-y-3 border-t border-slate-200 pt-6 text-sm dark:border-ink-800">
        {features.map((feature) => (
          <li key={feature} className="flex gap-3">
            <IconCheck className={`mt-0.5 h-4 w-4 shrink-0 ${colours.text}`} />
            <span className="text-slate-700 dark:text-slate-300">{feature}</span>
          </li>
        ))}
      </ul>

      {children}
    </section>
  );
}

/** A line at the foot of a card, level with the estimator beside it. */
function Footnote({ children }: { children: React.ReactNode }) {
  return (
    <p className="mt-auto pt-7 text-xs leading-relaxed text-slate-500">
      <span className="block border-t border-dashed border-slate-200 pt-4 dark:border-ink-800">{children}</span>
    </p>
  );
}

/** What an installation costs for a team of a given size. */
function Estimator() {
  const said = useT().enterprise;
  const [users, setUsers] = useState(25);

  return (
    <div className="mt-auto pt-7">
      <div className="bg-enterprise-500/[0.07] p-4 dark:bg-enterprise-400/[0.07]">
        <div className="flex items-baseline justify-between text-sm">
          <label htmlFor="seats" className="text-slate-600 dark:text-slate-400">
            {said.users(<span className="font-semibold tabular-nums text-grey-900 dark:text-grey-100">{users}</span>)}
          </label>
          <span className="tabular-nums">
            <span className="font-semibold text-enterprise-700 dark:text-enterprise-400">{said.price(users * SEAT)}</span>
            <span className="text-slate-500">{said.perMonth}</span>
          </span>
        </div>
        <input
          id="seats"
          type="range"
          min={5}
          max={250}
          step={5}
          value={users}
          onChange={(event) => setUsers(Number(event.target.value))}
          className="mt-3 w-full accent-enterprise-500"
        />
      </div>
    </div>
  );
}

type Cell = boolean | string;

function Comparison({ offline, removed }: { offline: number; removed: number }) {
  const said = useT().enterprise;

  const groups = said.groups(offline, removed);

  const heads: { tone: Tone; name: string; price: string }[] = [
    { tone: 'free', name: said.free, price: said.price(0) },
    { tone: 'enterprise', name: said.name, price: said.perUserPrice(SEAT) },
  ];

  return (
    <section className="mx-auto mt-24 max-w-4xl" aria-labelledby="compare">
      <h2 id="compare" className="text-center text-2xl font-bold tracking-tight">{said.compareTitle}</h2>
      <p className="mt-2 text-center text-sm text-slate-500">{said.compareText}</p>

      {/* wide enough to read on a phone by scrolling the table, not the page */}
      <div className="mt-10 overflow-x-auto">
        <table className="w-full min-w-[520px] border-collapse text-sm">
          <thead>
            <tr>
              <th className="w-1/2" />
              {heads.map((head) => (
                <th key={head.tone} scope="col" className="px-4 pb-4 text-center align-bottom">
                  <div className={`mx-auto mb-3 h-1 w-10 ${TONES[head.tone].fill}`} aria-hidden="true" />
                  <div className={`text-base font-semibold ${TONES[head.tone].text}`}>{head.name}</div>
                  <div className="mt-0.5 text-xs font-normal text-slate-500">{head.price}</div>
                </th>
              ))}
            </tr>
          </thead>

          {groups.map((group) => (
            <tbody key={group.title}>
              <tr>
                <th
                  colSpan={3}
                  scope="colgroup"
                  className="border-b border-slate-200 pb-2 pt-8 text-left text-xs font-medium uppercase tracking-[0.15em] text-slate-500 dark:border-ink-800"
                >
                  {group.title}
                </th>
              </tr>
              {group.rows.map(([feature, ...cells]) => (
                <tr key={feature} className="border-b border-slate-200/70 transition-colors hover:bg-grey-50 dark:border-ink-800/70 dark:hover:bg-ink-900">
                  <th scope="row" className="py-3.5 pr-4 text-left font-normal text-slate-700 dark:text-slate-300">
                    {feature}
                  </th>
                  {cells.map((cell, i) => (
                    <td
                      key={heads[i].tone}
                      className={`px-4 py-3.5 text-center ${heads[i].tone === 'enterprise' ? 'bg-enterprise-500/[0.04]' : ''}`}
                    >
                      <Value cell={cell} tone={heads[i].tone} />
                    </td>
                  ))}
                </tr>
              ))}
            </tbody>
          ))}
        </table>
      </div>
    </section>
  );
}

function Value({ cell, tone }: { cell: Cell; tone: Tone }) {
  const said = useT().enterprise;

  if (cell === true) {
    return (
      <span className={`inline-flex h-6 w-6 items-center justify-center rounded-full ${TONES[tone].soft} ${TONES[tone].text}`}>
        <IconCheck className="h-3.5 w-3.5" />
        <span className="sr-only">{said.included}</span>
      </span>
    );
  }

  if (cell === false) {
    return (
      <span className="text-slate-400 dark:text-ink-700">
        <span aria-hidden="true">—</span>
        <span className="sr-only">{said.notIncluded}</span>
      </span>
    );
  }

  return <span className="text-slate-700 dark:text-slate-300">{cell}</span>;
}

function Questions() {
  const said = useT().enterprise;

  return (
    <section className="mx-auto mt-24 max-w-3xl" aria-labelledby="questions">
      <h2 id="questions" className="text-center text-2xl font-bold tracking-tight">{said.questionsTitle}</h2>

      <div className="mt-8 divide-y divide-slate-200 border-y border-slate-200 dark:divide-ink-800 dark:border-ink-800">
        {said.questions.map(([question, answer]) => (
          <details key={question} className="group py-1">
            <summary className="flex cursor-pointer list-none items-center justify-between gap-4 py-4 text-[15px] font-medium [&::-webkit-details-marker]:hidden">
              {question}
              <span aria-hidden="true" className="text-xl leading-none text-slate-400 transition-transform group-open:rotate-45">+</span>
            </summary>
            <p className="pb-4 pr-8 text-sm leading-relaxed text-slate-600 dark:text-slate-400">{answer}</p>
          </details>
        ))}
      </div>

      <p className="mt-8 text-center text-sm text-slate-500">
        {said.anythingElse(
          <a href={CONTACT} className="text-enterprise-700 underline-offset-2 hover:underline dark:text-enterprise-400">
            {SOLUTIONS}
          </a>,
        )}
      </p>
    </section>
  );
}
