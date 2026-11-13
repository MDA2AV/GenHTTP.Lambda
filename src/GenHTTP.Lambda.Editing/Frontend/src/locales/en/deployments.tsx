
export const deployments = {
  hint: (until: string | null) =>
    `A deployment stays online while people use it${until ? ` - if nobody does, until ${until}` : ''}. Deploying again, or any visit, restarts that clock.`,
  takeOffline: 'Take offline',
  readFailed: 'The history could not be read.',
  reading: 'Reading the history…',
  none: 'Nothing has been deployed yet.',
  noDescription: 'No description',
  deployed: (when: string, by: string) => `Deployed ${when} by ${by}`,
  duration: 'How long it was online',
  online: 'online',
  short: {
    replaced: 'replaced',
    stopped: 'taken offline',
    expired: 'expired',
    admin: 'by the operator',
    ended: 'ended',
  } as Record<string, string>,
  putBack: (version: number) => `Put version ${version} back online`,
  timeline: 'What was online over the last seven days',
  block: (version: number, from: string, to: string | null) => `Version ${version}, ${from} to ${to ?? 'now'}`,
  weekAgo: 'a week ago',
  now: 'now',
};
