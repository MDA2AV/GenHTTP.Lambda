/**
 * The analytics of a lambda: who visits it, from where, to which pages - in
 * both views, so in the words of the simple view.
 */
export const analytics = {
  /** Next to the section's title, on hover. */
  hint: 'Counted for you by the platform: no cookies, no code in your app, and nothing for your visitors to accept.',
  loading: 'Reading the visits…',
  loadFailed: 'The visits could not be read.',

  /** The page shown while analytics are off. */
  offTitle: 'See who visits your app',
  offText:
    'How many people come each day, which pages they open, which sites send them, from which countries and on which devices - counted day by day, from the moment you switch it on.',
  offPrivate: 'No cookies, and no banner your visitors have to accept',
  offAddresses: 'No addresses are kept - only how many visits there were each day',
  offKept: (days: number) =>
    days >= 365
      ? 'Kept for a year, and all of it shown to you'
      : `Kept for a year - the last ${days} days are shown, all of it with Premium`,
  switchOn: 'Switch on analytics',
  switchedOn: 'Analytics are on. The first visits show up within a few minutes.',
  switchOnFailed: 'Analytics could not be switched on.',
  demo: 'The visitors of a demo are not counted. A copy of your own can count its visitors.',

  range: 'Time range',
  days: (days: number) => (days >= 365 ? '12 months' : `${days} days`),
  /** On a range the tier does not show, on hover. */
  premiumRange: 'Premium shows up to a year',

  /** The figures at the top; each one picks what the chart below shows. */
  visitors: 'Visitors',
  visitorsHint: 'Different people, each counted once a day. Search engines and other programs are not visitors.',
  views: 'Page views',
  viewsHint: 'Every time somebody opened a page.',
  hits: 'Requests',
  hitsHint: 'Everything your app was asked for: its pages, and the pictures and scripts they load.',
  /** Under a figure: how it changed against the days before the ones shown. */
  versus: (days: number) => `against the ${days} days before`,
  noChange: 'unchanged',
  /** Under a figure when there is nothing to compare with. */
  inDays: (days: number) => (days >= 365 ? 'in the last 12 months' : `in the last ${days} days`),

  /** Above the chart, for the figure picked. */
  perDay: {
    visitors: 'Visitors per day',
    views: 'Page views per day',
    hits: 'Requests per day',
  },
  perWeek: {
    visitors: 'Visitors per week',
    views: 'Page views per week',
    hits: 'Requests per week',
  },
  showValues: 'Show the values',
  showChart: 'Show the chart',
  day: 'Day',
  week: 'Week of',

  nobody: 'Nobody visited in these days.',
  nobodyYet: 'Nobody has visited since analytics were switched on. The first visits show up within a few minutes.',

  pages: 'Pages',
  pagesHint: 'The pages people opened, the most opened first.',
  requested: 'Most requested',
  requestedHint: 'Everything your app was asked for, by address - it has no pages people open.',
  sources: 'Where visitors come from',
  sourcesHint: 'The sites that sent people here, by the page they were on. Search engines only say that it was them, not what was searched.',
  direct: 'Typed in, a bookmark or an app',
  countries: 'Countries',
  countriesHint: 'Where the visitors\' internet addresses are registered - usually, not always, where they are.',
  unknownCountry: 'Unknown',
  devices: 'Devices',
  device: {
    desktop: 'Computer',
    mobile: 'Phone',
    tablet: 'Tablet',
    bot: 'Bots',
    other: 'Other',
  },
  browsers: 'Browsers',
  systems: 'Systems',
  crawlers: 'Search engines, AI and other programs',
  crawlersHint:
    'Programs that came by, by what they are. Google, Bing, OpenAI, Apple and Perplexity are named only when the visit came from an address they publish - anybody can claim to be them.',
  noCrawlers: 'No search engine, AI or other program has come by in these days.',
  /** On hover, next to the name of a crawler that was confirmed. */
  confirmed: (name: string) => `Came from an address ${name} publishes for its crawlers`,
  /** The kinds of programs that are counted without a name of their own. */
  crawlerKinds: {
    'search-engine': 'Search engines',
    'ai-crawler': 'AI crawlers',
    'social-preview': 'Link previews in chats and social networks',
    advertising: 'Advertising',
    seo: 'SEO tools',
    monitoring: 'Monitoring',
    'feed-reader': 'Feed readers',
    archiver: 'Web archives',
    academic: 'Research',
    scanner: 'Security scanners',
    'browser-automation': 'Automated browsers',
    'http-library': 'Scripts and apps',
    other: 'Other programs',
    /** Programs that called themselves Google, Bing and so on, but did not come from their addresses. */
    unconfirmed: 'Claimed to be a known crawler, not confirmed',
  },
  /** What did not fit in a day's list, gathered in one row. */
  other: 'Others',
  showAll: (count: number) => `Show all ${count}`,
  showFewer: 'Show fewer',
  /** Next to a share in a list, on hover. */
  share: (part: string, whole: string) => `${part} of ${whole}`,
  none: 'Nothing yet.',

  /** For a free app that counted more than it shows. */
  moreKept: (since: string) => `Counted since ${since} - Premium shows all of it, up to a year.`,
  premiumAsk: 'Ask about Premium',
  premiumSubject: 'Premium for my app',
  premiumBody: (address: string) => `Hello,\n\nI would like to see all of my app's analytics: ${address}\n\n`,

  /** At the foot of the page. */
  footer: (minutes: number) =>
    minutes === 1 ? 'Counted per day, in UTC. Updated every minute.' : `Counted per day, in UTC. Updated every ${minutes} minutes.`,
  updated: (time: string) => `Last update at ${time}.`,
  since: (date: string) => `Counting since ${date}.`,

  switchOff: 'Switch off analytics',
  switchOffTitle: 'Switch off analytics?',
  switchOffText: 'Counting stops, and everything counted so far is deleted. This cannot be undone.',
  keep: 'Keep counting',
  switchOffConfirm: 'Switch off and delete',
  switchedOff: 'Analytics are off, and what was counted is deleted.',
  switchOffFailed: 'Analytics could not be switched off.',
};
