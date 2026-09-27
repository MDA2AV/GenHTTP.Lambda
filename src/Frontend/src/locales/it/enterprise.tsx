import type { Messages } from '../en';

export const enterprise: Messages['enterprise'] = {
  eyebrow: 'Aziende',
  title: 'Prova gratuita, gestione autonoma',
  intro:
    'Tutto ciò che è disponibile qui è gratuito e senza account. Quando la Sua organizzazione ha bisogno di applicazioni sempre online, protette da un proprio sistema di accesso, può disporre di un’installazione dedicata – nel cloud oppure on-premise.',

  free: 'Gratuito',
  freeTagline: 'Per iniziare',
  forever: 'senza scadenza',
  buildOne: 'Crea un’applicazione',
  freeFeatures: (offline, removed) => [
    'Lambda illimitati, senza account',
    'L’agente integrato o il proprio tramite MCP',
    'Online finché viene utilizzato',
    `Disattivato dopo ${offline} giorni senza visite, rimosso dopo ${removed} giorni`,
    'Disponibile a un percorso dell’host condiviso',
  ],
  freeNote: 'Nessuna carta, nessuna registrazione. Crei un lambda: è Suo.',

  name: 'Enterprise',
  tagline: 'Per le organizzazioni che desiderano un’istanza dedicata',
  perUser: 'per utente / mese',
  contact: 'Contattaci',
  features: [
    'Istanza dedicata, nel cloud o on-premise',
    'Un unico servizio esegue tutte le applicazioni',
    'Accesso tramite il proprio SSO',
    'Le proprie regole di governance e conformità integrate',
    'Le applicazioni restano online e non vengono mai rimosse',
    'Agenti propri tramite MCP',
    'Supporto prioritario',
  ],
  users: (count) => <>{count} utenti</>,
  perMonth: ' / mese',
  price: (amount) => `${amount} $`,
  perUserPrice: (amount) => `${amount} $ per utente / mese`,

  compareTitle: 'Confronto dei piani',
  compareText:
    'Entrambi i piani si basano sulla stessa piattaforma. Cambiano la durata di conservazione dell’applicazione e il luogo in cui viene eseguita.',
  included: 'Incluso',
  notIncluded: 'Non incluso',
  groups: (offline, removed) => [
    {
      title: 'Sviluppo',
      rows: [
        ['Lambda', 'Illimitati', 'Illimitati'],
        ['Agente integrato', true, false],
        ['Agente proprio tramite MCP', true, true],
        ['Editor, versioni e log', true, true],
        ['Vetrina', true, 'Dedicata'],
      ],
    },
    {
      title: 'Hosting',
      rows: [
        ['Disattivazione se inutilizzato', `Dopo ${offline} giorni`, 'Mai'],
        ['Rimozione se inutilizzato', `Dopo ${removed} giorni`, 'Mai'],
        ['Istanza', 'Condivisa', 'Dedicata'],
        ['Esecuzione', 'Nel nostro cloud', 'Cloud oppure on-premise'],
        ['Cosa gestisce Lei', 'Nulla', 'Un unico servizio'],
        ['Domini personalizzati', false, true],
      ],
    },
    {
      title: 'Controllo',
      rows: [
        ['Accesso', 'Non necessario', 'Il proprio SSO'],
        ['Le proprie regole di governance e conformità per gli agenti', false, true],
        ['Console di amministrazione', false, true],
        ['Dati separati da quelli di altri clienti', false, true],
        ['Supporto', 'Community', 'Prioritario'],
      ],
    },
  ],

  questionsTitle: 'Domande frequenti',
  questions: [
    ['Serve un account per iniziare?', 'No. Per un lambda gratuito è sufficiente il link di modifica ricevuto al momento della creazione.'],
    [
      'Chi viene considerato utente nel piano Enterprise?',
      'Chiunque acceda tramite il Suo SSO, sia per sviluppare nell’editor sia per utilizzare un’applicazione distribuita sulla Sua installazione. Chi accede a un’applicazione senza effettuare l’accesso non viene conteggiato.',
    ],
    [
      'L’agente integrato è incluso nel piano Enterprise?',
      'No. Il Suo team utilizza il proprio agente – Claude, Claude Code o qualsiasi altro client compatibile con MCP – e lo collega alla Sua installazione, nell’ambito del piano già sottoscritto con il relativo fornitore.',
    ],
    [
      'Come vengono a conoscenza gli agenti delle nostre regole di conformità?',
      'Integriamo le Sue regole di governance e conformità nelle informazioni che la piattaforma fornisce agli agenti tramite MCP. Ogni agente collegato dal team le riceve durante la scrittura del codice: le applicazioni rispettano così le regole senza che tutti debbano conoscerle nel dettaglio.',
    ],
    [
      'Servono Kubernetes o un cluster?',
      'No. Tutte le applicazioni vengono eseguite all’interno di un unico servizio: non ci sono pod da distribuire né orchestrazione per singola applicazione. Gestire l’installazione significa gestire quest’unico servizio.',
    ],
    [
      'Dove viene eseguita un’installazione Enterprise?',
      'Dove preferisce. Possiamo ospitarla nel nostro cloud, oppure può essere eseguita in un Suo account cloud o sui Suoi server – ovunque sia possibile eseguire container. In entrambi i casi La assistiamo nella configurazione e la manteniamo aggiornata.',
    ],
  ],
  anythingElse: (mail) => <>Altre domande? Scriva a {mail}.</>,
};
