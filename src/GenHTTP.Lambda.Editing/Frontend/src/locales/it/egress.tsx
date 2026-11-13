import type { EditorMessages } from '../en';

export const egress: EditorMessages['egress'] = {
  hint:
    'Se il codice può connettersi ad altri server: chiamare un’API, aprire un socket, inviare e-mail. Le lambda condividono un unico server, quindi il permesso si dà lambda per lambda: una premium può, ogni altra solo dopo che l’operatore di questo sito l’ha consentito.',

  included: 'Incluso nel premium',
  includedText: 'Il codice può connettersi ad altri server: il premium lo include.',
  allowed: 'Consentito',
  allowedText: (ago) => <>L’operatore di questo sito l’ha consentito {ago}. Il codice può connettersi ad altri server.</>,
  requested: 'In attesa dell’operatore',
  requestedText: (ago) => (
    <>Richiesto {ago}. Quando l’operatore di questo sito lo consente, il codice che si connette viene compilato; fino ad allora viene rifiutato.</>
  ),
  declined: 'Rifiutato',
  declinedText: (ago) => <>L’operatore di questo sito l’ha rifiutato {ago}. Puoi chiedere di nuovo, spiegando meglio a cosa serve.</>,
  none: 'Non consentito',
  noneText:
    'Il codice non può connettersi ad altri server: il codice che chiama un’API, apre un socket o invia e-mail non viene compilato. Se la lambda ne ha bisogno, chiedilo qui sotto.',
  operatorSaid: 'L’operatore ha detto',
  purposeSaid: 'A cosa serve',

  connects: 'Il codice si connette ad altri server, cosa che questa lambda non può fare: non viene compilato finché non è consentito.',
  connectsWaiting: 'Il codice si connette ad altri server: viene compilato non appena l’operatore lo consente.',

  askTitle: 'Chiedilo',
  askAgainTitle: 'Chiedi di nuovo',
  editTitle: 'A cosa serve',
  purpose: 'A cosa si connette e perché?',
  purposeHint: 'Indica i server o i servizi e cosa ne fa la lambda. L’operatore di questo sito lo legge per decidere.',
  purposeExample: 'api.open-meteo.com, per le previsioni sulla pagina iniziale',
  send: 'Invia la richiesta',
  save: 'Salva',
  cancel: 'Annulla',
  edit: 'Modifica a cosa serve',
  asked: 'Richiesta inviata. Decide l’operatore di questo sito.',
  askFailed: 'Non è stato possibile inviare la richiesta.',
  withdraw: 'Ritira la richiesta',
  withdrawn: 'La richiesta è stata ritirata.',
  giveUp: 'Rinuncia',
  givenUp: 'Il codice non può più connettersi ad altri server.',
  giveUpTitle: 'Rinunciare alle connessioni ad altri server?',
  giveUpText:
    'Dalla prossima richiesta il codice non potrà più connettersi ad altri server e la versione online smetterà di funzionare dove lo fa. Per riaverlo, chiedilo di nuovo.',
  keep: 'Mantieni',
  failed: 'Non ha funzionato.',

  coversTitle: 'Cosa conta',
  covers: ['HttpClient e WebRequest', 'Socket e client WebSocket', 'E-mail (SmtpClient)', 'DNS e ping', 'XML caricato da un indirizzo'],
  notCovered:
    'Non riguarda ciò che avviene nel browser: le pagine della lambda possono recuperare dati da altri siti da sole, e visitatori, webhook e websocket raggiungono la lambda come sempre. Un’API pubblica che i browser possono chiamare conviene chiamarla dalla pagina: non serve alcun permesso.',

  refused: 'Il codice si connette ad altri server',
  refusedText: 'Questa lambda non può ancora farlo. Chiedilo, e decide l’operatore di questo sito.',
  openEgress: 'Chiedilo',
};
