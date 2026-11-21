
export const logs = {
  readFailed: 'The log could not be read.',
  hint: (capturing: boolean) =>
    'Requests, what the lambda printed and what went wrong, as it happens.' +
    (capturing ? '' : ' This installation does not keep what lambdas print, so only requests and errors appear.') +
    " Held in memory and shared with every lambda here, so it reaches back minutes to hours, and is empty after a restart. Visitors' addresses are not shown.",
  featureHint: (capturing: boolean) =>
    'What the preview of this draft answered, printed and threw, as it happens.' +
    (capturing ? '' : ' This installation does not keep what lambdas print, so only requests and errors appear.') +
    " Kept apart from your app's own log. Held in memory, so it reaches back minutes to hours.",
  nothingPreview: "Nothing yet. Open the draft's preview and its requests appear here.",
  search: 'Search',
  searchLabel: 'Search the log',
  resume: 'Show new lines as they come',
  pause: 'Stop adding new lines while you read',
  paused: 'Paused',
  live: 'Live',
  show: 'Show',
  all: 'Everything',
  requests: 'Requests',
  output: 'What it printed',
  problems: 'Problems',
  reading: 'Reading the log…',
  noProblems: 'Nothing has gone wrong that the log still remembers.',
  nothing: "Nothing yet. Open the lambda's address and its requests appear here.",
  noMatch: 'Nothing matches.',
  identical: (count: number) => `${count} identical lines`,
  at: (domain: string) => `, at ${domain}`,
  from: (country: string) => `, from ${country}`,
};
