import type { ReactNode } from 'react';

import type { AdminCensus, AdminLambdaUses, Connectivity } from '@genhttp-lambda/kit/api';
import { IconCode, IconDatabase, IconDraft, IconFolder, IconGlobe, IconKey, IconStar, IconViewSimple } from '@genhttp-lambda/kit/components/Icons';

/** One of the platform's features a lambda may use, as the panel names it everywhere. */
export interface Feature {
  /** What the listing is narrowed by: `uses=…`. */
  id: keyof AdminCensus['uses'];
  label: string;
  /** What using it means, in a line. */
  hint: string;
  icon: (props: { className?: string }) => ReactNode;
}

const IconChart = ({ className = 'h-4 w-4' }: { className?: string }) => (
  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.75} strokeLinecap="round" strokeLinejoin="round" className={className} aria-hidden="true">
    <path d="M4 20V10M10 20V4M16 20v-7M22 20H2" />
  </svg>
);

const IconLink = ({ className = 'h-4 w-4' }: { className?: string }) => (
  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.75} strokeLinecap="round" strokeLinejoin="round" className={className} aria-hidden="true">
    <path d="M10 14a5 5 0 0 0 7.07 0l3-3a5 5 0 0 0-7.07-7.07l-1 1M14 10a5 5 0 0 0-7.07 0l-3 3a5 5 0 0 0 7.07 7.07l1-1" />
  </svg>
);

/*
 * In the order the editor has them: what a lambda keeps, then how people
 * find it, then what it is being worked on with, then what it may do.
 */
export const FEATURES: Feature[] = [
  { id: 'database', label: 'Database', hint: 'Keeps records in a database of its own', icon: IconDatabase },
  { id: 'workspace', label: 'Workspace', hint: 'Keeps files - switched on unless its owner switched it off', icon: IconFolder },
  { id: 'secrets', label: 'Secrets', hint: 'Keeps API keys and passwords', icon: IconKey },
  { id: 'analytics', label: 'Analytics', hint: 'Has its visits counted', icon: IconChart },
  { id: 'showcase', label: 'Showcase', hint: 'Is listed in the showcase', icon: IconStar },
  { id: 'source', label: 'Open source', hint: 'Has its code published', icon: IconCode },
  { id: 'domain', label: 'Domain', hint: 'Has a domain of its own configured', icon: IconLink },
  { id: 'drafts', label: 'Drafts', hint: 'Has a feature open, being worked on', icon: IconDraft },
  { id: 'connects', label: 'Connects out', hint: 'May connect to other servers, by its tier or your decision', icon: IconGlobe },
  { id: 'requested', label: 'Asks to connect', hint: 'Asks to connect to other servers, and waits for you', icon: IconGlobe },
];

export const feature = (id: string) => FEATURES.find((f) => f.id === id);

/**
 * What a lambda uses, as small marks beside it in a list. Not the workspace,
 * which nearly every lambda has, nor the domain, which the list names.
 */
export function UseMarks({ uses, connectivity }: { uses?: AdminLambdaUses | null; connectivity?: Connectivity | null }) {
  if (!uses) {
    return null;
  }

  const marks: { key: string; title: string; icon: ReactNode; tone?: string }[] = [];

  const add = (key: string, on: boolean, title: string, icon: ReactNode, tone?: string) => on && marks.push({ key, title, icon, tone });

  add('database', uses.database, 'Database', <IconDatabase className="h-3.5 w-3.5" />);
  add('secrets', uses.secrets, 'Secrets', <IconKey className="h-3.5 w-3.5" />);
  add('analytics', uses.analyticsSince != null, 'Analytics', <IconChart className="h-3.5 w-3.5" />);
  add('showcase', uses.showcase, 'In the showcase', <IconStar className="h-3.5 w-3.5" />);
  add('source', uses.source, 'Code published', <IconCode className="h-3.5 w-3.5" />);
  add('drafts', uses.drafts > 0, uses.drafts === 1 ? 'A draft open' : `${uses.drafts} drafts open`, <IconDraft className="h-3.5 w-3.5" />);
  add('requested', connectivity?.status === 'Requested', 'Asks to connect to other servers', <IconGlobe className="h-3.5 w-3.5" />, 'text-amber-600 dark:text-amber-400');
  add('connects', connectivity?.status !== 'Requested' && connectivity?.allowed === true, 'May connect to other servers', <IconGlobe className="h-3.5 w-3.5" />);
  add('simple', uses.view === 'Simple', 'Opens in the simple view, for an owner who does not write code', <IconViewSimple className="h-3.5 w-3.5" />);

  if (marks.length === 0) {
    return <span className="text-slate-400">—</span>;
  }

  return (
    <span className="inline-flex items-center gap-1.5 text-slate-500" aria-label={marks.map((m) => m.title).join(', ')}>
      {marks.map((mark) => (
        <span key={mark.key} title={mark.title} className={mark.tone}>
          {mark.icon}
        </span>
      ))}
    </span>
  );
}

/** Where a lambda's connectivity stands, in a word. */
export function connectivityWord(connectivity?: Connectivity | null): string {
  if (!connectivity) {
    return 'Not allowed';
  }

  switch (connectivity.status) {
    case 'Requested':
      return 'Asks, waiting for you';
    case 'Allowed':
      return 'Allowed by you';
    case 'Declined':
      return 'Declined';
    default:
      return connectivity.included ? 'Included with its tier' : 'Not allowed';
  }
}
