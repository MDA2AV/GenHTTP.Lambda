import type { Messages } from '../en';

export const enterpriseLanding: Messages['enterpriseLanding'] = {
  eyebrow: 'App per il Suo lavoro',
  headline: 'Descriva l’app di cui il Suo lavoro ha bisogno.',
  headlineAccent: 'Un agente IA la realizza e la mette online qui.',
  intro:
    'Un modulo, un calendario di prenotazioni, un elenco per il Suo team: dica con parole Sue che cosa deve fare. In pochi minuti avrà un link da condividere con i colleghi, e più avanti modificherà l’app allo stesso modo, senza scrivere codice.',
  describe: 'Descrivere un’app',
  own: 'Usare il proprio agente IA',

  examplesTitle: 'Che cosa si realizza qui',
  examples: [
    { title: 'Moduli e richieste', text: 'Ferie, acquisti, feedback: le persone compilano e chi decide approva.' },
    { title: 'Prenotazioni', text: 'Sale riunioni, postazioni, attrezzature: chi ha cosa, e quando.' },
    { title: 'Elenchi e monitoraggi', text: 'Richieste di assistenza, inventario, inserimento dei nuovi colleghi: che cosa è aperto e chi se ne occupa.' },
    { title: 'Pagine per un team', text: 'Un calendario, un manuale, le novità: un solo link che tutti aprono.' },
  ],

  stepsTitle: 'Come funziona',
  steps: [
    {
      title: 'Lo descriva',
      text: 'Dica che cosa deve fare l’app, come lo spiegherebbe a un collega. Le idee sotto il campo sono un punto di partenza.',
    },
    {
      title: 'Condivida il link',
      text: 'Pochi minuti dopo l’app è online qui. Invii il suo link a chi la usa e conservi il Suo link di modifica: è il modo per cambiare l’app.',
    },
    {
      title: 'Chieda altro',
      text: 'Nello stesso campo: qualcosa di nuovo, qualcosa di diverso, oppure come sta andando: chi l’ha usata e che cosa è stato inserito. Le versioni precedenti restano salvate, così quella di prima può tornare online.',
    },
  ],

  questionsTitle: 'Da sapere',
  questions: (showcase) => [
    ['Devo saper programmare?', 'No. Lei descrive che cosa deve fare l’app e l’agente la scrive. Non deve mai guardare il codice.'],
    ['Chi può aprire la mia app?', 'Chiunque abbia il suo link e possa raggiungere questo sito. Lo invii alle persone che devono usarla.'],
    ['Dove sono conservati i dati inseriti?', 'Ogni app conserva le proprie voci su questa installazione, gestita dalla Sua organizzazione.'],
    [
      'Come torno alla mia app?',
      'Questo browser ricorda le app che ha realizzato qui, accanto al campo in cui le ha create. Su un altro dispositivo apra il link di modifica ricevuto con l’app: lo conservi in un posto sicuro, perché nessuno può recuperarlo.',
    ],
    [
      'Qualcosa non funziona. Che cosa faccio?',
      'Dica all’agente che cosa non va, come ha chiesto l’app. Se una modifica ha peggiorato le cose, metta di nuovo online la versione precedente nell’editor dell’app.',
    ],
    ['Che cosa hanno realizzato i colleghi?', <>Le app che hanno scelto di mostrare sono nella {showcase('Vetrina')}.</>],
  ],
};
