import type { EditorMessages } from '../en';

export const connectivity: EditorMessages['connectivity'] = {
  hint:
    'Of de code met andere servers mag verbinden: een API aanroepen, een socket openen, mail versturen. Lambdas delen één server, dus dat wordt per lambda toegestaan - een premium lambda mag het, elke andere zodra de operator van deze site het heeft toegestaan.',

  /** The state, as the heading of the card at the top of the section. */
  included: 'Inbegrepen bij premium',
  includedText: 'De code mag met andere servers verbinden: premium bevat dat.',
  allowed: 'Toegestaan',
  /** `ago` is a relative time, such as "3 days ago". */
  allowedText: (ago) => <>De operator van deze site heeft het {ago} toegestaan. De code mag met andere servers verbinden.</>,
  requested: 'Wacht op de operator',
  requestedText: (ago) => (
    <>Gevraagd {ago}. Zodra de operator van deze site het toestaat, wordt code die verbindt gecompileerd - tot die tijd wordt die geweigerd.</>
  ),
  declined: 'Afgewezen',
  declinedText: (ago) => <>De operator van deze site heeft het {ago} afgewezen. Je kunt het opnieuw vragen en meer vertellen over waarvoor het dient.</>,
  none: 'Niet toegestaan',
  noneText:
    'De code mag niet met andere servers verbinden: code die een API aanroept, een socket opent of mail verstuurt wordt niet gecompileerd. Vraag het hieronder aan als de lambda het nodig heeft.',
  /** Above the note of the operator, as they wrote it. */
  operatorSaid: 'De operator zei',
  /** Above what the owner said the lambda connects to, and why. */
  purposeSaid: 'Waarvoor het dient',

  /** Above the card, when the code online or the newest connects and may not. */
  connects: 'De code verbindt met andere servers, wat deze lambda niet mag: de code wordt niet gecompileerd totdat dat is toegestaan.',
  /** The same, while a request waits. */
  connectsWaiting: 'De code verbindt met andere servers: ze wordt gecompileerd zodra de operator het toestaat.',

  /** The form that asks for it. */
  askTitle: 'Vraag het aan',
  askAgainTitle: 'Vraag het opnieuw aan',
  editTitle: 'Waarvoor het dient',
  purpose: 'Waarmee verbindt het, en waarom?',
  purposeHint: 'Noem de servers of diensten, en wat de lambda ermee doet. De operator van deze site leest het om te beslissen.',
  purposeExample: 'api.open-meteo.com, voor de weersverwachting op de voorpagina',
  send: 'Verstuur het verzoek',
  save: 'Opslaan',
  cancel: 'Annuleren',
  edit: 'Wijzig waarvoor het dient',
  asked: 'Gevraagd. De operator van deze site beslist.',
  askFailed: 'Het verzoek kon niet worden verstuurd.',
  withdraw: 'Verzoek intrekken',
  withdrawn: 'Het verzoek is ingetrokken.',
  giveUp: 'Opgeven',
  givenUp: 'De code mag niet langer met andere servers verbinden.',
  giveUpTitle: 'Stoppen met verbinden met andere servers?',
  giveUpText:
    'Vanaf het volgende verzoek mag de code niet meer met andere servers verbinden, en de versie die online staat werkt niet meer waar dat nodig is. Vraag het opnieuw aan om het terug te krijgen.',
  keep: 'Behouden',
  failed: 'Dat is niet gelukt.',

  /** What counts as connecting, as a short list beside the card. */
  coversTitle: 'Wat telt mee',
  covers: ['HttpClient en WebRequest', 'Sockets en WebSocket-clients', 'Mail (SmtpClient)', 'DNS en ping', 'XML geladen vanaf een adres'],
  /** Under the list: what needs none of this. */
  notCovered:
    'Niet wat in de browser gebeurt: de pagina’s van de lambda mogen zelf bij andere sites ophalen, en bezoekers, webhooks en websockets bereiken de lambda zoals altijd. Een openbare API die browsers mogen aanroepen, roep je het best aan vanaf de pagina - daarvoor is geen toestemming nodig.',

  /** In the dialog that says a deployment was refused, when the reason is the lambda's connectivity. */
  refused: 'De code verbindt met andere servers',
  refusedText: 'Deze lambda mag dat nog niet. Vraag het aan, dan beslist de operator van deze site.',
  openConnectivity: 'Vraag het aan',
};
