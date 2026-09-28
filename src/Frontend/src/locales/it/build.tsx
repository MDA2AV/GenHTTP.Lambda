import type { Messages } from '../en';

export const build: Messages['build'] = {
  offTitle: 'Non disponibile qui',
  off: (write, mcp) => (
    <>
      Questa installazione non ha un agente per creare app. Puoi comunque {write('scrivere tu il codice')}, oppure
      collegare il tuo Claude a {mcp}.
    </>
  ),

  title: 'Chiedi quello che vuoi.',
  intro:
    'Il nostro agente crea l’app, la mette online e ti dà un link da mandare a chiunque. Niente account, niente da installare. E l’app ricorda i dati (punteggi, messaggi, iscrizioni), così chiunque la apra vede le stesse cose.',
  placeholder: 'crea un…',
  working: 'al lavoro…',
  shortcut: 'ctrl + invio',
  building: 'In creazione',
  buildIt: 'Crea',
  builtBy: 'Creato con',
  password: 'password',
  fable:
    'Fable è protetto da password finché è in prova. Non ha limiti di tempo: va avanti fino a lavoro finito, non fino allo scadere del tempo.',
  onlyNew:
    'Qui si creano solo app nuove. Per cambiare qualcosa che hai già fatto, apri il suo link di modifica e scrivi cosa deve cambiare nella sezione Modifica.',
  ideas: [
    'una bacheca dove chiunque può lasciare un messaggio di una riga',
    'una classifica dei punteggi per un gioco di dadi',
    'un sondaggio dove si vota e si vedono i risultati',
    'un guestbook per il mio matrimonio',
    'un conto alla rovescia per una data, visibile a tutti',
  ],
  ahead: (waiting) =>
    waiting === 1 ? 'C’è una richiesta prima della tua: poi tocca a te.' : `In coda ci sono ${waiting} richieste prima della tua.`,
  starting: 'Avvio…',

  yourApp: 'La tua app',
  further: 'Per continuare a lavorarci',
  keep: 'Conservalo. È l’unico modo per rientrare e non si può recuperare, nemmeno da parte nostra. Salvalo nei preferiti prima di chiudere questa scheda.',
  change:
    'Per modificarla, apri il link di modifica e scrivi cosa deve cambiare nella sezione Modifica, come fai qui. Può farlo anche il tuo agente, come spiegato sotto.',
  copyLink: 'Copia il link di modifica',
  lifetime: (offline, removed) =>
    `Resta online finché viene usata: dopo ${offline} giorni senza visite né modifiche va offline, e dopo ${removed} giorni viene eliminata. Per rimetterla online, apri l’editor e premi Deploy.`,
  openEditor: 'Apri l’editor',
  another: 'Crea qualcos’altro',

  keepGoing: 'Continua con il tuo agente',
  orOwn: 'Oppure usa il tuo agente',
  ownText:
    'Dietro il campo qui sopra c’è un Claude che gira su questo server. Se ne hai già uno tuo, collegalo qui: può fare le stesse cose (creare una lambda, scrivere il codice, metterla online) senza limite giornaliero e senza passare da questa pagina.',
  thenAsk: 'Poi chiedigli quello che vuoi, come faresti qui.',
  claudeWeb: 'Claude sul web',
  claudeWebHow:
    'Impostazioni, poi «Connectors», poi «Add custom connector». Incolla l’indirizzo qui sopra come URL del server MCP remoto. Niente chiavi, niente login.',
  howToChange:
    'Così puoi anche modificare un’app già creata: dai il link di modifica al tuo agente e digli cosa fare.',
  more: 'Scopri come usare il tuo agente',

  failedToStart: 'La richiesta non è partita.',
  noAnswer: 'Ha finito senza dire cosa è successo.',
  failed: 'Non ha funzionato.',
};
