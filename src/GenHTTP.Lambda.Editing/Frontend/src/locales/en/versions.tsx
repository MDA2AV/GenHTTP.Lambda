
export const versions = {
  hint: (limit: number) =>
    `A version is your app as it was saved - its code and its resources - and never changes afterwards, so any of them can be compared with and put back online exactly as it was. Each keeps what was asked for and what it changed. The oldest are removed once there are more than ${limit}; the one online never is.`,
  none: 'No versions yet.',
  noDescription: 'No description',
  online: 'online',
  putOnline: 'Put this version online',
  rollBackTitle: 'Put this older version back online',
  deploy: 'Deploy',
  rollBack: 'Roll back',
  readFailed: 'This version could not be read.',
  comparing: 'Comparing…',
  unchanged: 'Nothing changed from the version before.',
  first: 'The first version.',
  status: { added: 'added', removed: 'removed', changed: 'changed', same: 'same' } as Record<string, string>,
  /** What the changed files are to the version, as headings over a list of them that holds more than one kind. */
  groups: {
    code: 'Code',
    resources: 'Resources',
  },
  /** Opens the version in the code section, to read it or to change it from there. */
  files: 'Open its files',
  docs: 'Read its documentation',
  feature: 'Start a draft from here',
  featureTitle: 'Try a change on a copy of this version, without touching what is online',
  binary: 'Not text, so there are no lines to compare.',
  tooLarge: 'Too large to compare line by line.',
};
