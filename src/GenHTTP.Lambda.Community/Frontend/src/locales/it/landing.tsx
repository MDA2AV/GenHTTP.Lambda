import type { Messages } from '../en';

export const landing: Messages['landing'] = {
  eyebrow: 'GenHTTP Lambda · Community edition',
  headline: 'La piattaforma su cui i tuoi agenti creano app.',
  headlineAccent: 'Open source, su un server tutto suo.',
  intro:
    'Descrivi un’app e l’agente di creazione la scrive, oppure lascia che lo facciano Claude Code, Codex o Cursor tramite MCP. Ogni app è un handler GenHTTP in C#, compilato su questo server e online in pochi secondi, con database, chiavi e password, versioni e git.',
  describe: 'Descrivi un’app',
  connect: 'Collega il tuo agente',
  write: 'Scrivi il codice tu stesso',

  buildTitle: 'Descrivila',
  buildText: 'Basta una frase. L’agente di creazione scrive l’app, la mette online e ti dà il link per modificarla in seguito.',
  noAgent: 'A questa installazione non è collegato nessun agente di creazione. Collega qui sotto il tuo agente: fa lo stesso.',

  connectTitle: 'Collega il tuo agente',
  connectText:
    'Il tuo agente di programmazione riceve la piattaforma come strumenti: creare un’app, scriverne e controllarne il codice, provare una modifica in una bozza con una copia dei dati, metterla online e leggerne i log.',
  connectThen: 'Poi chiedigli qualcosa, per esempio: «Crea un guestbook, con una pagina in cui approvo i messaggi.»',

  featuresTitle: 'Cosa ottiene ogni app',
  features: [
    { title: 'Un indirizzo', text: 'Un indirizzo tutto suo su questo server, con il nome della sua chiave, e un dominio tutto suo quando diventa premium.' },
    { title: 'Voci', text: 'Un database SQLite tutto suo, con le tabelle create da migrazioni, letto e scritto con Entity Framework Core.' },
    { title: 'File', text: 'Una cartella per ciò che l’app salva: upload, esportazioni, tutto ciò che non è una voce.' },
    { title: 'Chiavi e password', text: 'Conservate cifrate sul server. L’agente dice quali servono, tu le inserisci e nessuno può rileggerle.' },
    { title: 'Bozze e versioni', text: 'Una modifica si prova in una bozza con una copia dei dati, poi si mette online. Le versioni precedenti vengono conservate, a un clic dal tornare indietro.' },
    { title: 'Git', text: 'Ogni app è un repository: clonalo, fai push di un commit per salvare una versione (con -o deploy la metti online) oppure di un branch per avviare una bozza.' },
    { title: 'Visitatori', text: 'Visualizzazioni di pagina, provenienza dei visitatori e loro paesi: contati sul server, senza cookie, quando lo attivi.' },
    { title: 'Il tuo codice', text: 'Scarica qualsiasi app come semplice progetto .NET ed eseguila ovunque, oppure pubblica il suo sorgente perché gli altri possano leggerlo.' },
  ],

  runsTitle: 'Come funziona',
  runs: [
    { title: 'Un processo', text: 'Il server, l’editor, l’API e tutte le app girano in un unico processo e conservano tutto in una sola cartella. L’agente di build, dove c’è, gira accanto.' },
    {
      title: 'Per percorso o per host',
      text: 'Le app rispondono sotto /lambda/ all’indirizzo dell’installazione oppure, se le assegni un dominio, ognuna a un sottodominio tutto suo.',
    },
    {
      title: 'Codice di cui ti fidi',
      text: 'Le app sono compilate nel server e il loro codice viene controllato per ciò che non devono fare, ma condividono il suo processo. Esegui ciò che eseguiresti tu stesso.',
    },
    { title: 'Open source', text: 'Con licenza MIT: leggilo, modificalo, eseguilo dove vuoi.' },
  ],
  repository: 'Il sorgente su GitHub',

  aroundTitle: 'Dai un’occhiata',
  around: {
    demos: { title: 'Parti da una demo', text: 'Un’API su voci, un login, aggiornamenti in tempo reale, un gioco: ognuna è un’app completa da copiare e modificare.' },
    showcase: { title: 'Vetrina', text: 'Le app che i loro proprietari hanno scelto di mostrare qui.' },
    guide: { title: 'Guida', text: 'Cosa può fare un’app e come, nell’ordine in cui ti serve.' },
    api: { title: 'API REST', text: 'Lo stesso degli strumenti, via HTTP, descritto in OpenAPI.' },
    sources: { title: 'Sorgenti pubblicati', text: 'App i cui proprietari hanno pubblicato il codice, da leggere e scaricare.' },
    admin: { title: 'Amministrazione', text: 'Le app, i limiti, i blocchi e il log, dietro il token di amministrazione.' },
  },
};
