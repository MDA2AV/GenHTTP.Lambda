import type { Messages } from '../en';

export const enterprise: Messages['enterprise'] = {
  eyebrow: 'Enterprise',
  title: 'Provalo gratis, gestiscilo tu',
  intro:
    'Qui è tutto gratis, senza account. Quando al tuo team servono app sempre online, protette dal login aziendale, scegli un’installazione tutta tua, nel cloud o on-premise.',

  free: 'Gratis',
  freeTagline: 'Per sperimentare',
  forever: 'per sempre',
  buildOne: 'Crea un’app',
  freeFeatures: (offline, removed) => [
    'Lambda illimitate, senza account',
    'L’agente integrato, o il tuo via MCP',
    'Online finché viene usata',
    `Offline dopo ${offline} giorni senza visite, eliminata dopo ${removed} giorni`,
    'Servita su un sottodominio del dominio condiviso',
  ],
  freeNote: 'Niente carta, niente registrazione. Crea una lambda ed è tua.',

  name: 'Enterprise',
  tagline: 'Per i team che vogliono un’istanza tutta loro',
  perUser: 'per utente / mese',
  contact: 'Contattaci',
  features: [
    'La tua istanza, nel cloud o on-premise',
    'Un solo servizio fa girare tutte le app',
    'Login con il tuo SSO',
    'Le tue regole di governance e compliance, integrate',
    'App sempre online, non si elimina mai niente',
    'Usa il tuo agente via MCP',
    'Supporto prioritario',
  ],
  users: (count) => <>{count} utenti</>,
  perMonth: ' / mese',
  price: (amount) => `${amount} $`,
  perUserPrice: (amount) => `${amount} $ / utente / mese`,

  compareTitle: 'Confronta i piani',
  compareText: 'Entrambi usano la stessa piattaforma. Cambia per quanto tempo tengono la tua app, e dove.',
  included: 'Incluso',
  notIncluded: 'Non incluso',
  groups: (offline, removed) => [
    {
      title: 'Sviluppo',
      rows: [
        ['Lambda', 'Illimitate', 'Illimitate'],
        ['Agente integrato', true, false],
        ['Il tuo agente via MCP', true, true],
        ['Editor, versioni e log', true, true],
        ['Vetrina', true, 'Tutta tua'],
      ],
    },
    {
      title: 'Hosting',
      rows: [
        ['Offline se inutilizzata', `Dopo ${offline} giorni`, 'Mai'],
        ['Eliminata se inutilizzata', `Dopo ${removed} giorni`, 'Mai'],
        ['Istanza', 'Condivisa', 'Tutta tua'],
        ['Dove gira', 'Nel nostro cloud', 'Cloud o on-premise'],
        ['Cosa gestisci tu', 'Niente', 'Un solo servizio'],
        ['Domini personalizzati', false, true],
      ],
    },
    {
      title: 'Controllo',
      rows: [
        ['Login', 'Non serve', 'Il tuo SSO'],
        ['Le tue regole di governance e compliance per gli agenti', false, true],
        ['Console di amministrazione', false, true],
        ['Dati separati da quelli degli altri clienti', false, true],
        ['Supporto', 'Community', 'Prioritario'],
      ],
    },
  ],

  questionsTitle: 'Domande',
  questions: [
    ['Mi serve un account per iniziare?', 'No. Per una lambda gratuita ti basta il link di modifica che ricevi quando la crei.'],
    [
      'Chi conta come utente in Enterprise?',
      'Chiunque faccia login con il tuo SSO, per sviluppare nell’editor o per usare un’app pubblicata sulla tua installazione. Chi usa un’app senza fare login non conta.',
    ],
    [
      'Enterprise include l’agente integrato?',
      'No. Il tuo team usa il suo agente (Claude, Claude Code o qualsiasi altro client MCP) e lo collega alla tua installazione, con il piano che ha già con il fornitore.',
    ],
    [
      'Come fanno gli agenti a conoscere le nostre regole di compliance?',
      'Integriamo le vostre regole di governance e compliance in quello che la piattaforma comunica agli agenti via MCP. Ogni agente che il team collega le riceve mentre scrive il codice, così le app nascono già in regola, senza che tutti debbano conoscere le regole a memoria.',
    ],
    [
      'Ci serve Kubernetes o un cluster?',
      'No. Tutte le app girano dentro un solo servizio: niente pod da distribuire, niente da orchestrare app per app. Gestire l’installazione vuol dire gestire quel servizio.',
    ],
    [
      'Dove gira un’installazione Enterprise?',
      'Dove vuoi tu. Possiamo ospitarla noi nel nostro cloud, oppure gira su un tuo account cloud o sui tuoi server, ovunque girino i container. In ogni caso ti aiutiamo a configurarla e a tenerla aggiornata.',
    ],
  ],
  anythingElse: (mail) => <>Altre domande? Scrivi a {mail}.</>,
};
