import { createContext, useContext } from 'react';

/**
 * The words of the parts the control center shares with the administration
 * console: badges, how long ago, how long something ran, what ended it.
 *
 * The console is English, and has these in English without asking for
 * anything. The editor is in the visitor's language, and hands the same words
 * in that language down through the context below.
 */
export const SHARED = {
  units: { s: 's', min: 'min', h: 'h', d: 'd' },
  /** A number with its unit: "3 min" here, "3分" where no space goes between them. */
  amount: (value: number, unit: string) => `${value} ${unit}`,
  /** Two amounts making one span: "3 h 12 min". */
  pair: (larger: string, smaller: string) => `${larger} ${smaller}`,
  never: 'never',
  justNow: 'just now',
  ago: (span: string) => `${span} ago`,
  in: (span: string) => `in ${span}`,

  /** Where something came from. */
  origins: {
    agent: 'agent',
    template: 'template',
    admin: 'operator',
    system: 'platform',
    api: 'API / editor',
    git: 'git push',
    unknown: 'unknown',
  } as Record<string, string>,

  /** Why something stopped being online. */
  endings: {
    replaced: 'replaced by a newer deployment',
    stopped: 'taken offline',
    expired: 'expired after going unused',
    admin: 'taken offline by the operator',
    ended: 'ended',
  } as Record<string, string>,

  whatThisIs: 'What this is',
  byAgent: 'by an agent',
  writtenByAgent: 'Written by an agent',
  /**
   * What the mark says of what the agent running on this server wrote: what
   * it ran on, as the agent names it - opus · high.
   */
  writtenByBuildAgent: (ranOn: string) => (ranOn ? `Written by the agent on this server, on ${ranOn}` : 'Written by the agent on this server'),
  byGit: 'with git',
  pushedWithGit: 'Pushed with git',
  more: 'More',
  of: (used: string, total: string) => `${used} of ${total}`,
  online: (version: number) => `Online · v${version}`,
  onlineTitle: (version: number) => `Online, serving version ${version}`,
  offline: 'Offline',
  offlineTitle: 'Offline: nothing is being served',
  /**
   * What each tier is called on a badge where the installation gives it no
   * label of its own - named by what it is for.
   */
  tiers: {
    Sandbox: 'Sandbox',
    Production: 'Production',
    System: 'System',
    Demo: 'Demo',
  } as Record<string, string>,
  /** What each tier is for, shown when the badge is pointed at. */
  tierTitles: {
    Sandbox: 'For trying something: taken offline when nobody visits or changes it for a while',
    Production: 'An app people use: kept online however quiet it gets, may answer at a domain of its own, and has more room for code, resources and data',
    System: 'Something this installation depends on: kept online, never blocked and never in the showcase',
    Demo: 'Kept online by this installation and read only',
  } as Record<string, string>,
  tier: (tier: string) => `${tier} tier`,

  /** Which way visitors came in, which the console shows as well. */
  entrances: {
    title: 'Reached through',
    note: 'Since the server started, websocket connections included.',
  },

  chart: {
    showChart: 'Show chart',
    showValues: 'Show values',
    none: 'No readings yet.',
    time: 'Time',
  },

  diagnostics: {
    compiles: 'The code compiles.',
    none: 'No messages yet. Check or deploy to compile your code.',
    line: (line: number) => `line ${line}`,
  },
};

export type SharedWords = typeof SHARED;

export const SharedWordsContext = createContext<SharedWords>(SHARED);

/** The shared words, in the language of whatever is showing them. */
export const useShared = () => useContext(SharedWordsContext);
