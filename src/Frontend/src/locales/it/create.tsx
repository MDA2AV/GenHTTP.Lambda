import type { Messages } from '../en';

export const create: Messages['create'] = {
  title: 'Crea una lambda',
  whatTitle: 'Cosa vuoi creare?',
  whatText:
    'Scegli l’opzione più vicina: parti da una copia di qualcosa che funziona già, e la cambi come vuoi. Oppure parti da zero.',
  seeIt: 'Vedila in azione',
  startFrom: 'Parti da questa',
  starters: {
    'demo-crud': {
      title: 'Tieni traccia delle cose',
      description: 'Una lista dove aggiungere, modificare e spuntare voci: attività, note, segnalibri o un piccolo inventario.',
    },
    'demo-registration': {
      title: 'Fai registrare le persone',
      description: 'Account con cui registrarsi e accedere, e pagine che vedono solo loro.',
    },
    'demo-game': {
      title: 'Un gioco da fare insieme',
      description: 'Un gioco per più persone alla volta, in tempo reale nel browser.',
    },
    'demo-files': {
      title: 'Condividi file e foto',
      description: 'Le persone caricano foto o documenti, e tutti gli altri li vedono.',
    },
    'demo-live': {
      title: 'Mostra le cose in tempo reale',
      description: 'Una pagina che si aggiorna da sola appena qualcosa cambia: voti, punteggi, una dashboard.',
    },
    empty: {
      title: 'Qualcos’altro',
      description: 'Parti da una lambda vuota e crea quello che hai in mente.',
    },
  },

  addressTitle: 'Scegli un indirizzo',
  fromDemo: (title) => <>{title}: la tua lambda parte come copia della demo, e puoi cambiare tutto.</>,
  fromNothing: 'La tua lambda parte vuota, pronta per quello che hai in mente.',
  pickAgain: 'Cambia scelta',
  publicKey: 'Chiave pubblica',
  free: (key) => `«${key}» è libera.`,
  keyHint: 'Lettere minuscole, numeri e trattini. Almeno tre caratteri. Lascia vuoto per averne una casuale.',
  accept: 'Accetto i termini di servizio',
  fullTerms: 'Leggi i termini di servizio completi',
  back: 'Indietro',
  creating: 'Creazione…',
  submit: 'Crea la mia lambda',
  keepLink: 'Nella prossima schermata vedrai il tuo link di modifica. È l’unico modo per rientrare: conservalo.',
  failed: 'Impossibile creare la lambda.',
};
