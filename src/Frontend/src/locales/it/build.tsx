import type { Messages } from '../en';

export const build: Messages['build'] = {
  title: 'Creare un sito web con l’AI.',
  intro:
    'Descriva con parole Sue il sito web o l’app che ha in mente. L’AI lo crea per Lei, noi lo ospitiamo ed è online in pochi minuti, con un link da inviare a chiunque. Gratis, senza programmare, senza registrazione.',
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
    'una lista per la cena in cui ognuno porta un piatto, così non ci sono doppioni',
    'un libro degli ospiti per il nostro matrimonio',
    'un sondaggio in cui tutti votano e vedono i risultati',
    'una classifica per la nostra serata quiz settimanale',
    'una pagina di compleanno dove gli amici lasciano i loro auguri',
  ],
  ahead: (waiting) =>
    waiting === 1 ? 'C’è un sito prima del Suo: poi tocca a Lei.' : `Ci sono ${waiting} siti prima del Suo.`,
  starting: 'Avvio…',

  points: [
    {
      title: 'Senza saper programmare',
      text: 'Dica con parole Sue cosa deve fare il Suo sito, come lo spiegherebbe a un amico. L’AI lo crea per Lei, senza bisogno di conoscenze tecniche.',
    },
    {
      title: 'Hosting gratuito incluso',
      text: 'Il Suo sito funziona sui nostri server. Nessun piano di hosting, nessun server e nessun dominio da comprare, niente da installare: di sicurezza e aggiornamenti ci occupiamo noi.',
    },
    {
      title: 'Online in pochi minuti',
      text: 'Riceve subito un link da condividere. Il sito ricorda ciò che le persone inseriscono (iscrizioni, voti, messaggi, punteggi), così tutti vedono le stesse cose.',
    },
  ],

  questionsTitle: 'Prima di iniziare',
  questions: (offline, removed) => [
    [
      'L’AI può davvero creare un sito web gratis per me?',
      `Sì. Lo descriva con parole Sue: l’AI lo crea, lo mette online e Le dà il link. Senza registrazione, senza carta di credito, senza periodo di prova. Resta online finché viene usato: dopo ${offline} giorni senza visite né modifiche va offline e dopo ${removed} giorni viene eliminato.`,
    ],
    [
      'Mi serve un hosting, un server o un dominio?',
      'No. Il Suo sito funziona sui nostri server, con hosting, sicurezza e aggiornamenti inclusi. Riceve subito un link, quindi non deve comprare nemmeno un dominio.',
    ],
    [
      'Posso creare un’app senza saper programmare?',
      'Sì. Non vedrà mai una riga di codice. Dica cosa deve fare, come lo spiegherebbe a un amico, e l’AI fa il resto: un sito web, una piccola app o un gioco.',
    ],
    [
      'Le persone possono iscriversi, votare, lasciare messaggi?',
      'Sì. Il Suo sito ricorda ciò che le persone inseriscono, così chiunque apra il link vede le stesse iscrizioni, gli stessi voti e punteggi.',
    ],
    [
      'Come lo aprono gli altri?',
      'Con il link, in qualsiasi browser, da telefono o da computer. Non c’è niente da installare e nessun app store di mezzo.',
    ],
    [
      'Come lo modifico in seguito?',
      'Apra il link di modifica che riceve con il Suo sito e descriva cosa deve cambiare, come qui. Se una modifica non Le piace, può tornare a com’era prima.',
    ],
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
