import type { Messages } from '../en';

export const create: Messages['create'] = {
  title: 'Crea un lambda',
  whatTitle: 'Cosa desidera realizzare?',
  whatText:
    'Scelga l’opzione più vicina: partirà da una copia di un’applicazione già funzionante, interamente modificabile. In alternativa può partire da zero.',
  seeIt: 'Vedi in funzione',
  startFrom: 'Usa questo modello',
  starters: {
    'demo-crud': {
      title: 'Gestire elementi',
      description: 'Un elenco da ampliare, modificare e spuntare: attività, note, segnalibri o un piccolo inventario.',
    },
    'demo-registration': {
      title: 'Registrazione e accesso',
      description: 'Account con registrazione e accesso, e pagine visibili solo agli utenti autenticati.',
    },
    'demo-game': {
      title: 'Un gioco multigiocatore',
      description: 'Un’applicazione utilizzata da più persone contemporaneamente, in tempo reale nel browser.',
    },
    'demo-files': {
      title: 'Condividere file e immagini',
      description: 'Immagini o documenti caricati e visibili a tutti.',
    },
    'demo-live': {
      title: 'Aggiornamenti in tempo reale',
      description: 'Una pagina che si aggiorna automaticamente a ogni cambiamento: voti, punteggi, una dashboard.',
    },
    empty: {
      title: 'Lambda vuoto',
      description: 'Parta da un lambda vuoto e realizzi la propria idea.',
    },
  },

  addressTitle: 'Scegliere un indirizzo',
  fromDemo: (title) => <>{title} – il Suo lambda parte come copia della demo, interamente modificabile.</>,
  fromNothing: 'Il Suo lambda parte vuoto, pronto per il Suo progetto.',
  pickAgain: 'Scegli un’altra opzione',
  publicKey: 'Chiave pubblica',
  free: (key) => `«${key}» è disponibile.`,
  keyHint: 'Lettere minuscole, cifre e trattini, almeno tre caratteri. Lasci vuoto per una chiave casuale.',
  accept: 'Accetto i termini di servizio',
  fullTerms: 'Leggi i termini di servizio completi',
  back: 'Indietro',
  creating: 'Creazione…',
  submit: 'Crea lambda',
  keepLink: 'La schermata successiva mostra il Suo link di modifica. È l’unico accesso: lo conservi con cura.',
  failed: 'Impossibile creare il lambda.',
};
