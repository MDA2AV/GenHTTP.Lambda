import type { Messages } from '../en';

export const ship: Messages['ship'] = {
  title: 'Dal tuo portatile allo schermo di tutti.',
  intro:
    'Hai creato qualcosa con il tuo agente AI, ma gira solo sul tuo computer. Chiedi all’agente di pubblicarlo qui. In pochi minuti avrà un link pubblico che chiunque può aprire. E può salvare dati, così le persone possono giocare, chattare e postare insieme.',
  facts: ['Gratis', 'Senza account', 'Niente da installare'],
  connect: 'Collega il tuo agente',
  seeOthers: 'Guarda le app degli altri',

  stepsTitle: 'Tre mosse, e una è solo una frase',
  step: (n) => `Passo ${n}`,
  steps: [
    {
      title: 'Collegalo una volta',
      body: 'Aggiungi un indirizzo a Claude, Cursor o all’agente che usi. Ci vuole meno di un minuto e lo fai una volta sola.',
    },
    {
      title: 'Chiedigli di pubblicare',
      body: 'Digli di mettere online l’app qui. L’agente la impacchetta, la pubblica e controlla che risponda.',
    },
    {
      title: 'Condividi il link',
      body: 'Ricevi un indirizzo pubblico e un link di modifica privato. Il primo mandalo a chi vuoi. Il secondo tienilo per te: ti serve per modificare l’app in futuro.',
    },
  ],

  togetherTitle: 'Non solo una pagina. Un posto dove incontrarsi.',
  together:
    'Quasi tutti gli hosting danno a ogni visitatore una copia della tua app, e ognuno gioca da solo. Qui ogni app ha la sua memoria e un collegamento in tempo reale con tutti quelli che la tengono aperta. La mossa di uno appare subito a tutti gli altri, e quello che postano resta lì anche il giorno dopo.',
  together2:
    'Nessun database a cui iscriversi, nessun altro servizio da collegare. Chiedilo come lo descriveresti a un amico.',
  kinds: [
    { name: 'Giochi multiplayer', ask: 'Fai entrare fino a otto amici nella stessa partita, con le mosse di tutti in tempo reale.' },
    { name: 'Chat di gruppo', ask: 'Aggiungi una chat dove può scrivere chiunque abbia il link, e tieni gli ultimi cento messaggi.' },
    { name: 'Liste condivise', ask: 'Trasforma la lista delle cose da portare in una lista che tutto il team modifica insieme.' },
    { name: 'Punteggi e record', ask: 'Tieni una classifica con il miglior tempo di ognuno e mostra i primi dieci nella schermata iniziale.' },
    { name: 'Piccoli social network', ask: 'Crea una bacheca dove gli invitati al matrimonio postano le loro foto e mettono like a quelle degli altri.' },
  ],
  quote: (text) => `«${text}»`,

  connectTitle: 'Collega il tuo agente, una volta sola',
  connectText:
    'Dai questo indirizzo al tuo agente. Da lì in poi sa come pubblicare qui, senza chiavi e senza login.',
  sayLike: 'Poi, nel tuo progetto, chiedi per esempio',
  asks: [
    'Pubblica questa app su GenHTTP Lambda e mandami il link.',
    'Rendi condivisi i punteggi, così tutti vedono la stessa classifica.',
  ],

  domainChip: 'Quando decolla',
  domainTitle: 'Un nome tutto suo',
  domainText:
    'Stessa app, stesso link di modifica, ma su un indirizzo tuo. Più facile da dire e da ricordare, e fa la sua figura quando la gente inizia a condividerla.',
  domainSubject: 'Un dominio per la mia app',
  domainAsk: 'Scrivici per il tuo dominio',

  questionsTitle: 'Prima che tu lo chieda',
  questions: (offline, removed, showcase, terms) => [
    [
      'È davvero gratis?',
      <>
        Sì. Niente carta, niente periodo di prova, niente account. La tua app resta online finché la gente la usa. Dopo{' '}
        {offline} giorni senza nemmeno una visita o una modifica va offline, e dopo {removed} giorni viene eliminata.
      </>,
    ],
    [
      'La mia app deve essere fatta in un certo modo?',
      'No, ci pensa il tuo agente. Pagine, immagini e stili vanno online così come sono, e quello che deve girare sul server l’agente lo adatta a questa piattaforma. Tu descrivi cosa deve fare l’app, al resto pensa lui.',
    ],
    [
      'Come la modifico dopo?',
      'Con il link di modifica che hai ricevuto quando l’hai pubblicata. Passalo al tuo agente con la prossima richiesta, oppure aprilo nel browser. Ogni modifica che chiedi diventa una versione a sé, allo stesso indirizzo, e puoi tornare a una versione precedente quando vuoi.',
    ],
    [
      'Chi può vedere la mia app?',
      <>
        Chiunque abbia il link. Non compare da nessuna parte, a meno che tu non decida di aggiungerla alla{' '}
        {showcase('vetrina')}.
      </>,
    ],
    [
      'C’è qualcosa che non posso pubblicare?',
      <>
        Poche cose, per esempio tutto ciò che danneggia o inganna le persone. I {terms('termini')} sono brevi e scritti in
        modo semplice.
      </>,
    ],
  ],

  closeTitle: 'Sul tuo computer funziona.',
  closeAccent: 'Ora fallo funzionare anche sui loro.',
  noAgent: 'Niente agente? Creala qui',
  closeFacts: 'Gratis. Senza account. Niente da installare.',

  scene: {
    label:
      'Qualcuno chiede a un agente di pubblicare un’app. L’indirizzo passa da localhost a un link pubblico, e arrivano le persone.',
    ask: 'Metti online il mio quiz, così i miei amici possono giocare.',
    live: 'È online. Ecco il tuo link.',
    publishing: 'Pubblicazione…',
    public: 'Pubblico',
    onlyYou: 'Solo tu',
    app: 'Serata quiz del venerdì',
    playing: (count) => <>{count} in gioco</>,
    you: 'Tu',
  },

  compareTitle: 'La via più breve da «funziona» a «provalo»',
  compareText:
    'Vercel, Cloudflare e Lovable sono ottimi posti dove far girare le cose. Ma si parte sempre da un modulo di registrazione. E appena la tua app deve condividere qualcosa tra i visitatori, serve anche un secondo servizio da configurare. Ecco come stanno le cose se parti da zero.',
  rows: [
    'Iniziare senza account',
    'Pubblicare dall’agente che usi già',
    'Dati condivisi in tempo reale: chat, multiplayer, record',
    'Quanto costa il primo link',
  ],
  us: ['Sì', 'Collegalo una volta, poi chiedi', 'Inclusi in ogni app', 'Gratis'],
  rivals: [
    ['Registrazione obbligatoria', 'Con i suoi strumenti, dopo il login', 'Aggiungi un servizio di database', 'Piano gratuito'],
    ['Registrazione obbligatoria', 'Con i suoi strumenti, dopo il login', 'Possibile, da configurare', 'Piano gratuito'],
    ['Registrazione obbligatoria', 'Nel suo editor', 'Tramite un backend collegato', 'Piano gratuito, crediti limitati'],
  ],
  compareNote:
    'Situazione a settembre 2026, per chi non ha un account da nessuna parte. Piani e funzioni degli altri servizi cambiano: per i dettagli controlla sui loro siti.',

  yourAgent: 'Il tuo agente',
  terminal: 'Terminale',
  setups: {
    claudeCode: 'Esegui questo comando una volta nel terminale. Da lì in poi ogni progetto che apri può pubblicare qui.',
    claude: (strong) => (
      <>
        In Claude sul web o su desktop, apri {strong('Impostazioni')}, poi {strong('Connectors')}, e scegli{' '}
        {strong('Add custom connector')}. Incolla l’indirizzo qui sopra e salva. Tutto qui.
      </>
    ),
    cursor: 'Aggiungi questo alle impostazioni MCP di Cursor, o al file qui sotto, e ricarica.',
    vscode: 'Salva questo file nel tuo progetto, poi avvia il server dalla vista MCP di Copilot Chat.',
  },
  elsewhere:
    'Usi qualcos’altro? Windsurf, Codex, Zed e quasi tutti gli altri agenti permettono di aggiungere un server MCP remoto nelle impostazioni. Dagli l’indirizzo qui sopra.',
};
