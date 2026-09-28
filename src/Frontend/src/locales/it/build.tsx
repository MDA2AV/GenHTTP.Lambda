import type { Messages } from '../en';

export const build: Messages['build'] = {
  title: 'Dall’idea al sito web.',
  intro:
    'Descriva il sito web o l’app che ha in mente. L’AI lo crea per Lei, noi lo ospitiamo sui nostri server ed è subito online, con un link da inviare a chiunque. Senza programmare, senza hosting da configurare, senza account.',
  placeholder: 'Vorrei un sito web che…',
  working: 'al lavoro…',
  shortcut: 'ctrl + invio',
  building: 'Creazione in corso',
  buildIt: 'Crea il mio sito',
  builtBy: 'Creato da',
  password: 'password',
  fable:
    'Fable è protetto da password mentre è in prova. Non ha limiti di tempo, quindi continua a lavorare finché il Suo sito non è finito, non finché scade il tempo.',
  onlyNew:
    'Qui si creano siti nuovi. Per modificarne uno esistente, apra il suo link di modifica e descriva in «Modifica» cosa deve cambiare.',
  ideas: [
    'un sito per la nostra associazione dove i soci si iscrivono agli eventi',
    'un libro degli ospiti per il nostro matrimonio',
    'un sondaggio in cui tutti votano e vedono i risultati',
    'una classifica per la nostra serata quiz settimanale',
    'un conto alla rovescia per la nostra inaugurazione, visibile a tutti',
  ],
  ahead: (waiting) =>
    waiting === 1 ? 'C’è un sito prima del Suo: poi tocca a Lei.' : `Ci sono ${waiting} siti prima del Suo.`,
  starting: 'Avvio…',

  points: [
    {
      title: 'Descritto, non programmato',
      text: 'Dica con parole Sue cosa deve fare il Suo sito. Non servono conoscenze tecniche né di programmazione.',
    },
    {
      title: 'Hosting incluso',
      text: 'Il Suo sito funziona sui nostri server. Di hosting, sicurezza e aggiornamenti ci occupiamo noi: non deve configurare né gestire nulla.',
    },
    {
      title: 'Online in pochi minuti',
      text: 'Riceve subito un link da condividere. Il sito può anche ricordare dati (iscrizioni, voti, punteggi), così tutti vedono le stesse cose.',
    },
  ],

  yourApp: 'Il Suo sito',
  further: 'Per modificarlo in seguito',
  keep:
    'Conservi questo link. È l’unico modo per rientrare e non si può recuperare, nemmeno da parte nostra. Lo salvi nei preferiti prima di chiudere questa scheda.',
  change:
    'Per modificare il Suo sito, apra il link di modifica e descriva in «Modifica» cosa deve cambiare, come qui. Può farlo anche il Suo assistente AI, come descritto qui sotto.',
  copyLink: 'Copia il link di modifica',
  lifetime: (offline, removed) =>
    `Lo manteniamo online finché viene usato: dopo ${offline} giorni senza visite né modifiche va offline e dopo ${removed} giorni viene eliminato. Apra l’editor per rimetterlo online.`,
  openEditor: 'Apri l’editor',
  another: 'Crea un altro sito',

  keepGoing: 'Continui con il Suo assistente AI',
  orOwn: 'Oppure usi il Suo assistente AI',
  ownText:
    'Usa già Claude o un altro assistente AI? Lo colleghi qui: crea e modifica siti per Lei allo stesso modo. Li ospitiamo noi, quindi non c’è comunque nulla da configurare. Nessun limite giornaliero.',
  ownTitle: 'Crei il Suo sito con il Suo assistente AI',
  ownOnly:
    'Colleghi Claude o un altro assistente AI all’indirizzo qui sotto, poi descriva il sito che desidera. L’assistente lo crea, noi lo ospitiamo sui nostri server ed è subito online, con un link da condividere.',
  thenAsk:
    'Poi gli dica cosa desidera, per esempio: «Crea un sito per il nostro coro con il calendario dei nostri concerti».',
  howToChange:
    'Così si modifica un sito anche in seguito: dia al Suo assistente il link di modifica e gli dica cosa deve cambiare.',

  failedToStart: 'La richiesta non è partita.',
  noAnswer: 'Ha finito senza dire cosa è successo.',
  failed: 'Non ha funzionato.',
};
