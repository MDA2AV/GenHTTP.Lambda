import type { ReactNode } from 'react';

type Node = ReactNode;

export const summary = {
  reading: 'Reading how it is doing…',
  readDocs: 'Read the documentation',
  hint: (since: string, kept: boolean, retention: number, tier: string) =>
    `Traffic is counted since the server last started (${since}). ` +
    (kept
      ? `A lambda stays online while people use it, and is removed after ${retention} days with no visits and no changes.`
      : `This lambda is in the ${tier} tier, which keeps it online and stored however quiet it gets.`),
  onlineFor: (duration: (text: string) => Node, version: number) => (
    <>
      Online for {duration('a while')}, serving version {version}.
    </>
  ),
  offline: 'Offline. Nothing is being served until a version is deployed.',
  nothing: 'Nothing has been written yet.',
  requestsToday: 'requests today',
  lastHour: (count: number) => `${count} in the last hour`,
  hourly: 'Requests per hour over the last day',
  failed: 'failed',
  failedTitle: (failed: number, rejected: number) =>
    `${failed} server errors, ${rejected} not found or refused, over the last day`,
  average: 'to answer, on average',
  noneYet: 'none yet',
  lastVisit: 'last visit',
  problems: 'Something went wrong recently',
  openLog: 'Open the log',
  latest: 'Latest change',
  allVersions: 'All versions',
  noDescription: 'No description',
  version: (version: number) => `Version ${version}`,
  notOnline: 'not online yet',
  wanted: 'What was wanted',
  noVersions: 'No versions yet.',
  inProgress: 'Drafts',
  allFeatures: 'All drafts',
  previewOnline: 'Its preview is running',
  previewOffline: 'Its preview is not running',
  behind: 'out of date',
  storage: 'Storage',
  /** The meter of what a version may hold, which its code and its resources share. */
  versionAllowance: 'Code and resources',
  /** The meter of what the lambda may keep, which its database and its workspace share. */
  data: 'Data',
};
