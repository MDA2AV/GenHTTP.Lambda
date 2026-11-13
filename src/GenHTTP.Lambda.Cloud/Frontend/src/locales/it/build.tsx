import type { Messages } from '../en';

export const build: Messages['build'] = {
  title: 'Creare un sito web con l’AI.',
  intro:
    'Descriva con parole Sue il sito web o l’app che ha in mente. L’AI lo crea per Lei, noi lo ospitiamo ed è online in pochi minuti, con un link da inviare a chiunque. Gratis, senza programmare, senza registrazione.',
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

};
