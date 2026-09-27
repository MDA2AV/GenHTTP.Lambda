import type { ReactNode } from 'react';

/** What the frame of every page says, whatever page it is. */
export const shell = {
  main: 'Main',
  build: 'Build one',
  ship: 'Ship',
  showcase: 'Showcase',
  enterprise: 'Enterprise',
  docs: 'Docs',
  admin: 'Admin',
  lightMode: 'Switch to light mode',
  darkMode: 'Switch to dark mode',
  openMenu: 'Open the menu',
  closeMenu: 'Close the menu',
  language: 'Language',
};

/** Words more than one page needs. */
export const common = {
  loading: 'Loading…',
  loadingEditor: 'Loading the editor…',
  editorFailed: 'The editor could not be loaded',
  editorFailedWhy: 'This usually means the site was updated while this tab was open.',
  reload: 'Reload the page',
  backToStart: 'Back to the start',
  tryAgain: 'Try again',
  copy: 'Copy',
  copied: 'Copied',
  copyToClipboard: 'Copy to clipboard',
  openInNewTab: 'Open in a new tab',
  close: 'Close',
};

export const notFound = {
  title: 'Page Not Found',
  heading: 'This page does not exist',
  text: 'The link may be stale, or the lambda it pointed at has been deleted.',
};

export const missing = {
  title: 'Nothing Is Running Here',
  heading: 'Nothing is running here',
  notDeployed: (key: ReactNode) => (
    <>
      There is a lambda at {key}, but it is not deployed at the moment. Deployments in the free tier stay up
      while they are used, and are taken down after a month without visits or edits - whoever holds the
      editor link can put it back online.
    </>
  ),
  unknown: (key: ReactNode) => (
    <>
      No lambda is hosted at {key}. The key may never have existed, or the lambda behind it has been
      deleted.
    </>
  ),
  create: 'Create a lambda here',
};

export const abuse = {
  report: 'Report abuse',
  title: 'Report a lambda',
  write: 'Write to us',
  subject: 'Abuse report',
  intro:
    'Anybody can put code online here, which means somebody sometimes puts up something they should not. If a page hosted here is trying to trick people, attacking something, or using material it has no right to, tell us and we will take it down.',
  how: (mailbox: ReactNode, strong: (text: string) => ReactNode, path: ReactNode) => (
    <>
      Write to {mailbox} and include {strong('the address of the page')} - it looks like {path} - and a
      sentence about what is wrong with it. A screenshot helps. You do not need an account and you do not
      need to be a user of this site.
    </>
  ),
  next: (strong: (text: string) => ReactNode, terms: (text: string) => ReactNode) => (
    <>
      {strong('What happens next.')} A person reads it. If it breaks the {terms('terms of service')}, the
      lambda is taken offline, usually within a day. We will not tell you who put it there, and we cannot
      promise to write back about every report - but every one of them is read.
    </>
  ),
  danger:
    'If somebody is in immediate danger, or a crime is being committed, please contact your local authorities as well. We can remove a page; we cannot do anything else.',
};
