import type { ReactNode } from 'react';

export const terms = {
  title: 'Terms of service',
  /**
   * Said by every translation: the terms that apply are the English ones,
   * linked in the words given. Nothing in English, where they already are.
   */
  binding: null as ((english: (text: string) => ReactNode) => ReactNode) | null,
  intro:
    'This is a free service for trying things out. It runs code written by strangers on shared infrastructure, which is only workable if everybody keeps to a few rules.',
  sections: {
    forbiddenTitle: 'What you may not put here',
    forbidden: [
      "No malware, no phishing, no crypto miners. Nothing that attacks, scans, floods or otherwise interferes with other systems, here or anywhere else. Nothing that harasses anybody. Nothing you have no right to publish - that includes other people's code, text, images and trademarks.",
      'Do not use a lambda to store or forward personal data about other people. There is nothing private about a public address, and this platform offers you no way to keep such data safe.',
    ],
    actionTitle: 'What we may do about it',
    action:
      'Anything deployed here can be taken offline or removed at any time, with no notice and no obligation to explain. In practice that happens when something breaks the rules above, when it threatens the machine everybody else is sharing, or when somebody reports it and they turn out to be right.',
    lastingTitle: 'How long anything lasts',
    lasting: (hours: number, days: number) =>
      `A deployment stays reachable for about ${hours} hours. A lambda you have not opened is removed, with every version of its code, about ${days} days after you last touched it. Saving or deploying counts as touching it, so anything you are working on stays. Nothing here is a backup: keep your own copy of code you care about.`,
    keyTitle: 'Your editor link is your password',
    key: 'Anybody who has the editor link can read and change that lambda, and there is no account and no password behind it. If you publish the link, you have published the ability to change it. There is no way to recover one that is lost.',
    warrantyTitle: 'No warranty',
    warranty:
      'The service is provided as it is, with no guarantee that it works, keeps working, or keeps anything you put into it. It may be restarted, changed or switched off at any time. Do not build anything on it that matters to you or to anybody else.',
    reportTitle: 'Reporting something',
    report: (mailbox: ReactNode, front: (text: string) => ReactNode) => (
      <>
        If a lambda hosted here is doing something it should not, write to {mailbox} with its address. See{' '}
        {front('the front page')} for what to include.
      </>
    ),
  },
  change: 'These terms can change. The version that applies is the one on this page.',

  /** The short version, beside the box that accepts them when a lambda is created. */
  short:
    'Lambdas run on shared infrastructure. By creating one you agree not to deploy malware, phishing pages, crypto miners, or anything that attacks, scans or floods other systems, and not to publish content you have no right to publish. Anyone who knows the editor link can change your lambda, so treat it as a password. Free tier lambdas stay online for as long as they are used: one that nobody visits and nobody edits for a month is taken offline, and removed if nothing happens for two months after that. Anything you deploy may be removed at any time.',
};
