import type { Messages } from '../en';

export const ship: Messages['ship'] = {
  title: 'Dal Suo computer a tutti gli schermi.',
  intro:
    'Ha creato qualcosa con il Suo agente di programmazione, ma funziona solo sul Suo computer. Chieda all’agente di pubblicarlo qui. Pochi minuti dopo avrà un link pubblico accessibile a chiunque, e l’applicazione potrà memorizzare dati, così che più persone possano giocare, conversare e pubblicare insieme.',
  facts: ['Gratuito', 'Senza account', 'Senza installazioni'],
  connect: 'Colleghi il Suo agente',
  seeOthers: 'Applicazioni pubblicate',

  stepsTitle: 'Tre passaggi, di cui uno è una semplice frase',
  step: (n) => `Passaggio ${n}`,
  steps: [
    {
      title: 'Collegamento unico',
      body: 'Aggiunga un indirizzo a Claude, Cursor o all’agente che utilizza. Richiede meno di un minuto ed è necessario una sola volta.',
    },
    {
      title: 'Richiesta di pubblicazione',
      body: 'Chieda all’agente di mettere l’applicazione online qui. La prepara, la pubblica e verifica che risponda.',
    },
    {
      title: 'Condivisione del link',
      body: 'Riceverà un indirizzo pubblico e un link di modifica privato. Il primo può essere condiviso con chiunque; conservi il secondo, che Le servirà per modificare l’applicazione in seguito.',
    },
  ],

  togetherTitle: 'Non solo una pagina: uno spazio condiviso.',
  together:
    'La maggior parte dei servizi di hosting fornisce a ogni visitatore una copia separata dell’applicazione, e ciascuno la utilizza da solo. Qui ogni applicazione dispone di una propria memoria e di una connessione in tempo reale con tutte le persone che l’hanno aperta. L’azione di una persona appare subito agli altri, e i contenuti pubblicati restano disponibili.',
  together2:
    'Nessun database da sottoscrivere, nessun servizio aggiuntivo da integrare. È sufficiente descrivere la funzionalità desiderata.',
  kinds: [
    { name: 'Giochi multigiocatore', ask: 'Permetti a un massimo di otto persone di partecipare alla stessa partita e di vedere le mosse degli altri in tempo reale.' },
    { name: 'Chat', ask: 'Aggiungi una stanza in cui tutti coloro che hanno il link possano scrivere e conserva gli ultimi cento messaggi.' },
    { name: 'Elenchi condivisi', ask: 'Trasforma la lista dei bagagli in un elenco che tutto il team possa modificare contemporaneamente.' },
    { name: 'Punteggi e record', ask: 'Tieni una classifica con il miglior tempo di ciascuno e mostra i primi dieci nella schermata iniziale.' },
    { name: 'Piccole community', ask: 'Permetti agli ospiti del matrimonio di pubblicare foto su una bacheca comune e di apprezzare quelle degli altri.' },
  ],
  quote: (text) => `«${text}»`,

  connectTitle: 'Colleghi il Suo agente una sola volta',
  connectText:
    'Comunichi questo indirizzo al Suo agente. Da quel momento saprà pubblicare qui, senza chiavi e senza accesso.',
  sayLike: 'Quindi, nel Suo progetto, è sufficiente una richiesta come',
  asks: [
    'Pubblica questa applicazione su GenHTTP Lambda e inviami il link.',
    'Rendi condivisi i punteggi, così che tutti vedano la stessa classifica.',
  ],

  domainChip: 'Quando l’applicazione cresce',
  domainTitle: 'Un dominio dedicato',
  domainText:
    'La stessa applicazione e lo stesso link di modifica, ma a un indirizzo di Sua proprietà: più semplice da comunicare e da ricordare, e di aspetto più professionale quando viene condiviso.',
  domainSubject: 'Un dominio per la mia applicazione',
  domainAsk: 'Richieda un dominio dedicato',

  questionsTitle: 'Domande frequenti',
  questions: (offline, removed, showcase, terms) => [
    [
      'È davvero gratuito?',
      <>
        Sì. Nessuna carta, nessun periodo di prova e nessun account. La Sua applicazione resta online finché viene
        utilizzata. Dopo {offline} giorni senza visite né modifiche viene disattivata, e dopo {removed} giorni rimossa.
      </>,
    ],
    [
      'L’applicazione deve essere realizzata in un modo particolare?',
      'No, se ne occupa il Suo agente. Pagine, immagini e fogli di stile vengono pubblicati così come sono, e l’agente adatta a questa piattaforma tutto ciò che deve essere eseguito sul server. Lei descrive cosa deve fare l’applicazione; l’implementazione è a carico dell’agente.',
    ],
    [
      'Come la modifico in seguito?',
      'Con il link di modifica ricevuto al momento della pubblicazione. Lo consegni al Suo agente insieme alla modifica successiva, oppure lo apra nel browser. Ogni modifica diventa una nuova versione allo stesso indirizzo, ed è possibile tornare a una versione precedente in qualsiasi momento.',
    ],
    [
      'Chi può vedere la mia applicazione?',
      <>
        Chiunque riceva il link da Lei. Non è elencata da nessuna parte, a meno che Lei non scelga di aggiungerla alla{' '}
        {showcase('vetrina')}.
      </>,
    ],
    [
      'Ci sono contenuti che non posso pubblicare?',
      <>
        Sì, ad esempio tutto ciò che danneggia o inganna le persone. I {terms('termini di servizio')} sono brevi e
        formulati in modo chiaro.
      </>,
    ],
  ],

  closeTitle: 'Sul Suo computer funziona.',
  closeAccent: 'Lo renda disponibile a tutti.',
  noAgent: 'Nessun agente? Crei qui',
  closeFacts: 'Gratuito. Senza account. Senza installazioni.',

  scene: {
    label:
      'A un agente viene chiesto di pubblicare un’applicazione. L’indirizzo passa da localhost a un link pubblico e altre persone si collegano.',
    ask: 'Metti online il mio quiz così che i miei amici possano partecipare.',
    live: 'L’applicazione è online. Ecco il Suo link.',
    publishing: 'Pubblicazione…',
    public: 'Pubblico',
    onlyYou: 'Locale',
    app: 'Serata quiz del venerdì',
    playing: 'online',
    you: 'Lei',
  },

  compareTitle: 'La strada più breve da «funziona» a «provalo»',
  compareText:
    'Vercel, Cloudflare e Lovable sono ottime piattaforme per eseguire applicazioni. Tuttavia richiedono una registrazione e, non appena l’applicazione deve condividere dati tra i visitatori, la configurazione di un servizio aggiuntivo. Ecco il confronto per chi parte da zero.',
  rows: [
    'Iniziare senza account',
    'Pubblicare dall’agente già in uso',
    'Dati condivisi in tempo reale: chat, multigiocatore, record',
    'Costo fino al primo link',
  ],
  us: ['Sì', 'Un collegamento, poi basta chiedere', 'Incluso in ogni applicazione', 'Gratuito'],
  rivals: [
    ['Registrazione richiesta', 'Con strumenti propri, dopo l’accesso', 'Servizio di database da aggiungere', 'Piano gratuito'],
    ['Registrazione richiesta', 'Con strumenti propri, dopo l’accesso', 'Possibile, con configurazione', 'Piano gratuito'],
    ['Registrazione richiesta', 'Nel proprio editor', 'Tramite un backend collegato', 'Piano gratuito, crediti limitati'],
  ],
  compareNote:
    'Situazione a settembre 2026, per chi non ha account presso alcun servizio. Piani e funzionalità di altri servizi possono cambiare; per i dettagli si rimanda ai rispettivi fornitori.',

  yourAgent: 'Il Suo agente',
  terminal: 'Terminale',
  setups: {
    claudeCode: 'Esegua questo comando una volta in un terminale. Ogni progetto aperto in seguito potrà pubblicare qui.',
    claude: (strong) => (
      <>
        In Claude sul web o su desktop, apra le {strong('Impostazioni')}, quindi {strong('Connectors')}, e selezioni{' '}
        {strong('Add custom connector')}. Incolli l’indirizzo qui sopra e salvi. Non sono necessari altri passaggi.
      </>
    ),
    cursor: 'Aggiunga questa voce alle impostazioni MCP di Cursor, o al file indicato di seguito, e ricarichi.',
    vscode: 'Salvi questo file nel progetto, quindi avvii il server dalla vista MCP di Copilot Chat.',
  },
  elsewhere:
    'Utilizza un altro strumento? Windsurf, Codex, Zed e la maggior parte degli altri agenti consentono di aggiungere un server MCP remoto nelle impostazioni. Indichi loro l’indirizzo qui sopra.',
};
