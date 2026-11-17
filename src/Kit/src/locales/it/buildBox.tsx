import type { KitMessages } from '../en';

export const buildBox: KitMessages['buildBox'] = {
  placeholder: 'Vorrei un sito web che…',
  password: 'password',
  fable:
    'Fable è protetto da password mentre è in prova. Non ha limiti di tempo, quindi continua a lavorare finché il Suo sito non è finito, non finché scade il tempo.',
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
  online: 'Il Suo sito è online',
  notOnline: 'Il Suo sito è stato creato, ma non è andato online.',
  open: 'Apri il sito',
  steps: {
    guide: 'Preparazione',
    examples: 'Uno sguardo agli esempi',
    create: 'Scelta di un indirizzo per il Suo sito',
    write: 'Scrittura del Suo sito',
    improve: 'Miglioramento del Suo sito',
    check: 'Ricerca di errori',
    online: 'Pubblicazione online',
    trying: 'Prova del sito',
    looking: 'Revisione del Suo sito',
    forRecords: 'Preparazione dello spazio per le voci',
    forKeys: 'Preparazione dello spazio per chiavi e password',
    forFiles: 'Preparazione dello spazio per ciò che salva',
    records: 'Uno sguardo alle voci',
    keys: 'Verifica delle chiavi e password necessarie',
    addFile: 'Aggiunta di un file',
    removeFile: 'Rimozione di un file',
    files: 'Uno sguardo a ciò che ha salvato',
    services: 'Richiesta di usare altri servizi',
  },
  further: 'Per modificarlo in seguito',
  keep:
    'Conservi questo link. È l’unico modo per rientrare e non si può recuperare, nemmeno da parte nostra. Lo salvi nei preferiti prima di chiudere questa scheda.',
  copyLink: 'Copia il link di modifica',
  lifetime: (offline, removed) =>
    `Lo manteniamo online finché viene usato: dopo ${offline} giorni senza visite né modifiche va offline e dopo ${removed} giorni viene eliminato. Apra l’editor per rimetterlo online.`,
  openEditor: 'Apri l’editor',

  failedToStart: 'La richiesta non è partita.',
  noAnswer: 'Ha finito senza dire cosa è successo.',
  failed: 'Non ha funzionato.',
};
