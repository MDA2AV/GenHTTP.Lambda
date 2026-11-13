/*
 * The words of /build, for somebody who wants a website and does not write
 * code. They say what happens in the words such a person would search for -
 * creating a website, hosting, putting it online - and leave out the words
 * of the people who make such things.
 */
export const build = {
  title: 'Create a website with AI.',
  intro:
    'Describe the website or app you have in mind, in your own words. AI builds it for you, we host it, and it is online in minutes - with a link you can send to anyone. Free, no coding, no sign-up.',
  points: [
    {
      title: 'No coding needed',
      text: 'Say in your own words what your website should do, the way you would tell a friend. AI builds it for you - no technical knowledge needed.',
    },
    {
      title: 'Free hosting included',
      text: 'Your website runs on our servers. No hosting plan, no server and no domain to buy, nothing to install - security and updates are taken care of.',
    },
    {
      title: 'Online in minutes',
      text: 'You get a link to share straight away. It remembers what people enter - sign-ups, votes, messages, scores - so everybody sees the same.',
    },
  ],

  questionsTitle: 'Before you start',
  /** Asked the way people ask a search engine, answered in the words of the page. */
  questions: (offline: number, removed: number): [string, string][] => [
    [
      'Can AI really build a website for me for free?',
      `Yes. Describe it in your own words and AI builds it, puts it online and gives you the link. No sign-up, no credit card, no trial. It stays online for as long as people use it: after ${offline} days without a visit or a change it is taken offline, and after ${removed} it is removed.`,
    ],
    [
      'Do I need hosting, a server or a domain?',
      'No. Your website runs on our servers, with hosting, security and updates included. You get a link straight away, so there is no domain to buy either.',
    ],
    [
      'Can I make an app without knowing how to code?',
      'Yes. You never see any code. Say what it should do, the way you would tell a friend, and AI does the rest - a website, a little app or a game.',
    ],
    [
      'Can people enter things - sign-ups, votes, messages?',
      'Yes. Your website remembers what people enter, so everybody who opens the link sees the same entries, votes and scores.',
    ],
    [
      'How do other people open it?',
      'With the link, in any browser, on a phone or a computer. There is nothing to install and no app store in between.',
    ],
    [
      'How do I change it later?',
      'Open the editor link you get with your website and describe what should be different, the same way as here. If you do not like a change, you can go back to how it was before.',
    ],
  ],

};
