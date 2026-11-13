import type { EditorMessages } from '../en';

export const egress: EditorMessages['egress'] = {
  hint:
    'Ob der Code sich mit anderen Servern verbinden darf: eine API aufrufen, einen Socket öffnen, E-Mails senden. Lambdas teilen sich einen Server, daher wird das Lambda für Lambda erlaubt – ein Premium-Lambda darf es, jedes andere, sobald der Betreiber dieser Website es erlaubt hat.',

  /** The state, as the heading of the card at the top of the section. */
  included: 'In Premium enthalten',
  includedText: 'Der Code darf sich mit anderen Servern verbinden: Premium schließt das ein.',
  allowed: 'Erlaubt',
  /** `ago` is a relative time, such as "3 days ago". */
  allowedText: (ago) => <>Der Betreiber dieser Website hat es {ago} erlaubt. Der Code darf sich mit anderen Servern verbinden.</>,
  requested: 'Wartet auf den Betreiber',
  requestedText: (ago) => (
    <>Angefragt {ago}. Sobald der Betreiber dieser Website es erlaubt, wird Code mit solchen Verbindungen kompiliert – bis dahin wird er abgelehnt.</>
  ),
  declined: 'Abgelehnt',
  declinedText: (ago) => <>Der Betreiber dieser Website hat es {ago} abgelehnt. Sie können erneut anfragen und genauer beschreiben, wofür es gedacht ist.</>,
  none: 'Nicht erlaubt',
  noneText:
    'Der Code darf sich nicht mit anderen Servern verbinden: Code, der eine API aufruft, einen Socket öffnet oder E-Mails sendet, wird nicht kompiliert. Fragen Sie es unten an, wenn das Lambda es braucht.',
  /** Above the note of the operator, as they wrote it. */
  operatorSaid: 'Der Betreiber schrieb',
  /** Above what the owner said the lambda connects to, and why. */
  purposeSaid: 'Wofür es gedacht ist',

  /** Above the card, when the code online or the newest connects and may not. */
  connects: 'Der Code verbindet sich mit anderen Servern, was dieses Lambda nicht darf: Er wird nicht kompiliert, bis das erlaubt ist.',
  /** The same, while a request waits. */
  connectsWaiting: 'Der Code verbindet sich mit anderen Servern: Er wird kompiliert, sobald der Betreiber es erlaubt.',

  /** The form that asks for it. */
  askTitle: 'Anfragen',
  askAgainTitle: 'Erneut anfragen',
  editTitle: 'Wofür es gedacht ist',
  purpose: 'Womit verbindet es sich, und warum?',
  purposeHint: 'Nennen Sie die Server oder Dienste und was das Lambda damit macht. Der Betreiber dieser Website liest es und entscheidet.',
  purposeExample: 'api.open-meteo.com, für die Wettervorhersage auf der Startseite',
  send: 'Anfrage senden',
  save: 'Speichern',
  cancel: 'Abbrechen',
  edit: 'Verwendungszweck ändern',
  asked: 'Angefragt. Der Betreiber dieser Website entscheidet.',
  askFailed: 'Die Anfrage konnte nicht gesendet werden.',
  withdraw: 'Anfrage zurückziehen',
  withdrawn: 'Die Anfrage wurde zurückgezogen.',
  giveUp: 'Darauf verzichten',
  givenUp: 'Der Code darf sich nicht mehr mit anderen Servern verbinden.',
  giveUpTitle: 'Auf Verbindungen zu anderen Servern verzichten?',
  giveUpText:
    'Ab der nächsten Anfrage darf sich der Code nicht mehr mit anderen Servern verbinden, und die Version, die online ist, funktioniert dort nicht mehr, wo sie es tut. Um es zurückzubekommen, fragen Sie erneut an.',
  keep: 'Behalten',
  failed: 'Das hat nicht funktioniert.',

  /** What counts as connecting, as a short list beside the card. */
  coversTitle: 'Was zählt',
  covers: ['HttpClient und WebRequest', 'Sockets und WebSocket-Clients', 'E-Mail (SmtpClient)', 'DNS und Ping', 'XML, das von einer Adresse geladen wird'],
  /** Under the list: what needs none of this. */
  notCovered:
    'Nicht, was im Browser geschieht: Die Seiten des Lambdas dürfen selbst von anderen Websites laden, und Besucher, Webhooks und Websockets erreichen das Lambda wie bisher. Eine öffentliche API, die Browser aufrufen dürfen, rufen Sie am besten von der Seite aus auf – dafür ist keine Erlaubnis nötig.',

  /** In the dialog that says a deployment was refused, when the reason is the egress. */
  refused: 'Der Code verbindet sich mit anderen Servern',
  refusedText: 'Das darf dieses Lambda noch nicht. Fragen Sie es an, und der Betreiber dieser Website entscheidet.',
  openEgress: 'Anfragen',
};
