import type { Messages } from '../en';

export const build: Messages['build'] = {
  offTitle: 'Non disponibile su questa installazione',
  off: (write, mcp) => (
    <>
      Questa installazione non dispone di un agente di creazione. È comunque possibile{' '}
      {write('scrivere il codice autonomamente')} oppure collegare il proprio Claude a {mcp}.
    </>
  ),

  title: 'Descriva ciò che Le serve.',
  intro:
    'L’applicazione viene creata e pubblicata, e Lei riceve un link da inviare a chiunque. Senza account né installazioni, e con la possibilità di memorizzare dati – punteggi, messaggi, voci – così che tutti vedano lo stesso contenuto.',
  placeholder: 'crea un…',
  working: 'in corso…',
  shortcut: 'Ctrl + Invio',
  building: 'Creazione in corso',
  buildIt: 'Crea',
  builtBy: 'Creato con',
  password: 'password',
  fable:
    'Fable è protetto da password durante la fase di prova. Funziona senza limiti di tempo e prosegue fino al completamento dell’applicazione.',
  onlyNew:
    'Qui vengono create solo nuove applicazioni. Per sviluppare ulteriormente un’applicazione esistente, consegni il relativo link di modifica al proprio agente di programmazione – vedere sotto.',
  ideas: [
    'una bacheca dove chiunque può lasciare un breve messaggio',
    'una classifica per un gioco di dadi',
    'un sondaggio con risultati visibili',
    'un guestbook per il nostro matrimonio',
    'un conto alla rovescia condiviso fino a una data',
  ],
  ahead: (waiting) =>
    waiting === 1 ? 'Una creazione prima della Sua – Lei è il prossimo.' : `${waiting} creazioni prima della Sua.`,
  starting: 'Avvio…',

  yourApp: 'La Sua applicazione',
  further: 'Per svilupparla ulteriormente',
  keep: 'Conservi questo link con cura. È l’unico accesso e non può essere recuperato, nemmeno da noi. Lo aggiunga ai preferiti prima di chiudere questa scheda.',
  change:
    'Questa pagina crea solo nuove applicazioni. Per modificare questa, colleghi il proprio agente di programmazione come descritto di seguito, gli consegni il link di modifica e descriva la modifica desiderata.',
  copyLink: 'Copia il link di modifica',
  lifetime: (offline, removed) =>
    `L’applicazione resta online finché viene utilizzata: dopo ${offline} giorni senza visite né modifiche viene disattivata e dopo ${removed} giorni rimossa. Per rimetterla online, apra l’editor e selezioni Distribuisci.`,
  openEditor: 'Apri l’editor',
  another: 'Crea un’altra applicazione',

  keepGoing: 'Prosegua con il proprio agente',
  orOwn: 'Oppure utilizzi il proprio agente',
  ownText:
    'Il campo qui sopra utilizza un Claude in esecuzione su questo server. Se dispone già di un proprio agente, può collegarlo qui: avrà le stesse funzionalità – creare un lambda, scrivere il codice, pubblicarlo – senza limite giornaliero e senza passare da questa pagina.',
  thenAsk: 'Quindi descriva ciò che Le serve, esattamente come qui.',
  claudeWeb: 'Claude sul web',
  claudeWebHow:
    'Impostazioni, quindi «Connectors», poi «Add custom connector». Incolli l’indirizzo qui sopra come URL del server MCP remoto. Non sono richiesti chiavi né accesso.',
  howToChange:
    'Allo stesso modo si modifica un’applicazione già creata: consegni il link di modifica al proprio agente e descriva la modifica.',
  more: 'Maggiori informazioni sull’uso di un agente',

  failedToStart: 'Impossibile avviare la richiesta.',
  noAnswer: 'Il processo si è concluso senza indicare l’esito.',
  failed: 'L’operazione non è riuscita.',
};
