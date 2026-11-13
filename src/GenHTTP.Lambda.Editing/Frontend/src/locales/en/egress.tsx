import type { ReactNode } from 'react';

type Node = ReactNode;

/**
 * Whether the code may connect to other servers - its egress - in the full
 * view's section of that name: where it stands, what counts, and asking the
 * operator of the site for it.
 */
export const egress = {
  hint:
    'Whether the code may connect to other servers: call an API, open a socket, send mail. Lambdas share one server, so that is allowed lambda by lambda - a premium one may, any other once the operator of this site allowed it.',

  /** The state, as the heading of the card at the top of the section. */
  included: 'Included with premium',
  includedText: 'The code may connect to other servers: premium includes it.',
  allowed: 'Allowed',
  /** `ago` is a relative time, such as "3 days ago". */
  allowedText: (ago: Node) => <>The operator of this site allowed it {ago}. The code may connect to other servers.</>,
  requested: 'Waiting for the operator',
  requestedText: (ago: Node) => (
    <>Asked {ago}. Once the operator of this site allows it, code that connects compiles - until then it is refused.</>
  ),
  declined: 'Declined',
  declinedText: (ago: Node) => <>The operator of this site declined it {ago}. You can ask again, saying more about what it is for.</>,
  none: 'Not allowed',
  noneText:
    'The code may not connect to other servers: code that calls an API, opens a socket or sends mail does not compile. Ask for it below if the lambda needs it.',
  /** Above the note of the operator, as they wrote it. */
  operatorSaid: 'The operator said',
  /** Above what the owner said the lambda connects to, and why. */
  purposeSaid: 'What it is for',

  /** Above the card, when the code online or the newest connects and may not. */
  connects: 'The code connects to other servers, which this lambda may not do: it does not compile until that is allowed.',
  /** The same, while a request waits. */
  connectsWaiting: 'The code connects to other servers: it compiles once the operator allows it.',

  /** The form that asks for it. */
  askTitle: 'Ask for it',
  askAgainTitle: 'Ask again',
  editTitle: 'What it is for',
  purpose: 'What does it connect to, and why?',
  purposeHint: 'Name the servers or services, and what the lambda does with them. The operator of this site reads it to decide.',
  purposeExample: 'api.open-meteo.com, for the forecast on the front page',
  send: 'Send the request',
  save: 'Save',
  cancel: 'Cancel',
  edit: 'Change what it is for',
  asked: 'Asked. The operator of this site decides.',
  askFailed: 'The request could not be sent.',
  withdraw: 'Take the request back',
  withdrawn: 'The request is taken back.',
  giveUp: 'Give it up',
  givenUp: 'The code may no longer connect to other servers.',
  giveUpTitle: 'Give up connecting to other servers?',
  giveUpText:
    'From its next request on, the code may not connect to other servers, and the version online stops working where it does. To have it back, ask again.',
  keep: 'Keep it',
  failed: 'That did not work.',

  /** What counts as connecting, as a short list beside the card. */
  coversTitle: 'What counts',
  covers: ['HttpClient and WebRequest', 'Sockets and WebSocket clients', 'Mail (SmtpClient)', 'DNS and ping', 'XML loaded from an address'],
  /** Under the list: what needs none of this. */
  notCovered:
    'Not what happens in the browser: the pages of the lambda may fetch from other sites themselves, and visitors, webhooks and websockets reach the lambda as they always did. A public API that browsers may call is best called from the page - that needs no permission.',

  /** In the dialog that says a deployment was refused, when the reason is the egress. */
  refused: 'The code connects to other servers',
  refusedText: 'This lambda may not do that yet. Ask for it, and the operator of this site decides.',
  openEgress: 'Ask for it',
};
