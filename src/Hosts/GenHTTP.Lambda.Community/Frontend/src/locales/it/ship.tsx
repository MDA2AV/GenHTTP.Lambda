import type { Messages } from '../en';

export const ship: Messages['ship'] = {
  eyebrow: 'Collega il tuo agente',
  title: 'Pubblica dall’agente che usi già.',
  intro:
    'Claude Code, Codex, Cursor o qualsiasi agente che parla MCP ottiene questa installazione come strumenti: crea un’app, prova una modifica in una bozza, la mette online e ne legge i log. Ogni app risponde a un indirizzo suo, con un database suo.',
  stepsTitle: 'Tre passi per un’app online',
  step: (n) => `Passo ${n}`,
  steps: [
    {
      title: 'Collega una volta sola',
      body: 'Aggiungi al tuo agente l’indirizzo del server MCP di questa installazione. Non serve nessuna chiave: chi ha la chiave di modifica di un’app può modificare quell’app, e nient’altro.',
    },
    {
      title: 'Chiedigli di creare o pubblicare',
      body: 'Descrivi una nuova app, o chiedigli di pubblicare quella del tuo progetto. Scrive il codice, controlla che compili e la mette online.',
    },
    {
      title: 'Conserva il link di modifica',
      body: 'Ricevi l’indirizzo dell’app e il suo link di modifica. L’indirizzo è per tutti; il link di modifica è il modo in cui tu - o il tuo agente - modifichi l’app in seguito.',
    },
  ],
  sayLike: 'Poi scrivi qualcosa come',
  asks: ['Crea un libro degli ospiti, con una pagina in cui approvo i messaggi.', 'Pubblica qui questa app e mandami il link.'],
  noAgent: 'Non hai un agente? Usa l’agente di creazione',

  questionsTitle: 'Da sapere',
  questions: (create) => [
    [
      'Quali agenti possono collegarsi?',
      'Qualsiasi agente che sappia aggiungere un server MCP remoto via HTTP: Claude Code, Claude, Codex, Cursor, VS Code e altri, all’indirizzo qui sopra.',
    ],
    [
      'Come modifico un’app in seguito?',
      'Dai al tuo agente il link di modifica dell’app con la modifica successiva, oppure apri tu il link: l’editor mostra le sue versioni, il codice, le voci e i log. Una modifica viene provata in una bozza, con una copia dei dati, prima di andare online.',
    ],
    [
      'Posso lavorare su un’app con git?',
      'Sì. Ogni app è un repository git: clonala dal suo editor, fai push di un commit su main per salvare una versione, oppure fai push di un branch per avviare una bozza con un’anteprima tutta sua.',
    ],
    [
      'Dove vanno le chiavi API e le password?',
      'Non nel codice. L’agente le legge per nome, e i valori li inserisci tu nell’editor, dove nessuno li rilegge - nemmeno l’agente.',
    ],
    [
      'Posso scrivere un’app senza un agente?',
      <>Sì: {create('scrivi il codice da te')}, partendo da una delle demo. Un’app è un handler GenHTTP in C#, compilato su questo server.</>,
    ],
  ],
  profiles: {
    enterprise: {
      asks: [
        'Crea un modulo per richiedere le ferie, con una pagina in cui il responsabile approva ogni richiesta.',
        'Pubblica qui questa app e mandami il link.',
      ],
    },
  },
};
