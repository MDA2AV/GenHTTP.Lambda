import type { ReactNode } from 'react';

export const privacy = {
  title: 'Privacy policy',
  /**
   * Said by every translation: the policy that applies is the English one,
   * linked in the words given. Nothing in English, where it already is.
   */
  binding: null as ((english: (text: string) => ReactNode) => ReactNode) | null,
  intro:
    'What this site learns about you, what it does with it, how long it keeps it and who else gets to see it. In short: there are no accounts, no advertising and no tracking. The server notes who asked it for what, so that it can be kept running and abuse can be traced, and what you ask the build agent for is sent to Anthropic, whose model writes the app.',
  sections: {
    whoTitle: 'Who is responsible',
    who: 'This site is run by the person below, who is responsible for the personal data it handles under the EU General Data Protection Regulation (GDPR). Write to this address about anything on this page:',

    requestsTitle: 'What the server notes on every request',
    requests: [
      "Every request to this site, and to every lambda hosted on it, is written to the server's log: the IP address it came from, the one it says it was forwarded for, the browser or program that sent it, the address it asked for, when, and how it was answered. The server also looks up the country, town and network the IP address belongs to, in a database it keeps itself - nobody else is asked.",
      'This is how faults are found, how an overloaded server is traced to whatever overloads it, and how abuse reported to us is tracked down. The IP address is also used, in memory only, to limit how many requests and builds one visitor can make. A request cannot be answered without these details. The legal basis is our legitimate interest in running the service and keeping it safe (Art. 6(1)(f) GDPR).',
      'Administrators can read all of it. The owner of a lambda sees the country and the browser of each request to their lambda, but not the IP address.',
    ],

    logsTitle: 'How long the log is kept',
    logs: "The log is kept in two places: in the server's memory, which is emptied whenever the server restarts, and in the console output of the server, which is removed whenever the server is updated. Both have a fixed size, so every new line pushes out the oldest one, and how long a line lasts depends on how busy the site is. Nothing from the log is copied into an archive.",

    contentTitle: 'What you put here',
    content: (days: number) =>
      `A lambda is its code, its files, its settings, and the notes saved with its versions about what was asked for and what changed. All of it is stored on the server so that it can be run and edited. A free lambda is removed with all of its versions about ${days} days after it was last changed or visited, and at once when whoever holds its editor link deletes it. Anybody with the editor link can read all of it, what you put on the showcase can be seen by everybody, and administrators look at a lambda when they have to, to deal with a report or keep the server safe. The legal basis is providing the service you asked for (Art. 6(1)(b) GDPR).`,

    agentTitle: 'What you ask the build agent',
    agent: (policy: (text: string) => ReactNode) => (
      <>
        What you type into the build box, or into the Change section of a lambda's editor, is sent to Anthropic PBC in
        the United States, which runs Claude, the model that writes the app. To make a change, the agent also reads the
        lambda - its code, the notes on its versions and its log, which holds its requests and what it printed but not
        its visitors' IP addresses - and what it reads is sent there too. What Anthropic does with it is covered by{' '}
        {policy('its own privacy policy')}. The United States does not protect personal data the way the EU does. Your
        request is sent there because building or changing what you asked for needs it (Art. 6(1)(b) and Art. 49(1)(b)
        GDPR), so do not put anything into it that you would not want to share.
      </>
    ),
    agentKept:
      "The agent saves your request, often in its own words, as the note on the version it writes, and the first few hundred characters of it go into the build service's log, which has a fixed size as well. If you use your own agent instead, such as Claude or Claude Code, what you tell it goes to that agent's provider, not to us: we only receive the code and the notes it sends here.",

    lambdasTitle: 'What a lambda does is up to its owner',
    lambdas:
      'A lambda is written by whoever holds its editor link, not by us. What it asks its visitors for and what it does with that is up to them, and this page does not cover it - apart from the request log above, which the server keeps for every lambda. The terms do not allow using a lambda to collect personal data about other people; if you come across one that does, please report it.',

    mailTitle: 'When you write to us',
    mail: 'If you write to us, to report abuse or about anything else, we use your address and your message to answer you and to deal with what you wrote, and delete them once they are no longer needed for that (Art. 6(1)(f) GDPR).',

    storageTitle: 'Cookies and your browser',
    storage:
      "There is one cookie, called lang. It remembers the language you picked, so that addresses without one open in it, and it lasts a year. The browser's own storage remembers light or dark mode, a few settings of the pages you use and, for administrators, their token. None of it is used to follow you and none of it goes to anybody else: there is no analytics, no advertising, and nothing is loaded from other sites, not even fonts. Since all of it only does what you asked for, it needs no consent (§ 25(2) no. 2 TDDDG).",

    hostingTitle: 'Where it is kept',
    hosting:
      'The server all of this runs on is rented from a hosting provider in the European Union, and what this page describes is stored there.',

    rightsTitle: 'Your rights',
    rights: (mailbox: ReactNode) => (
      <>
        You can ask what we have stored about you and for a copy of it, have it corrected, deleted or its use
        restricted, and object to anything we do because of our legitimate interest (Art. 15 to 21 GDPR). Write to{' '}
        {mailbox}. There are no accounts, so we can only find what is yours if you tell us how: the IP address you
        used and roughly when, or the address of your lambda. No decision with legal or similarly significant effects
        on you is made automatically (Art. 22 GDPR).
      </>
    ),
    complaint:
      'You can also complain to a data protection authority, where you live or where we are. Ours is the State Commissioner for Data Protection and Freedom of Information of Baden-Württemberg (LfDI Baden-Württemberg).',
  },
  change: 'This policy changes when the site does. The version that applies is the one on this page.',
  updated: 'Last changed on 28 September 2026.',
};
