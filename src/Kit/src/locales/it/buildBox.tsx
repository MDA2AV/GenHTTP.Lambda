import type { KitMessages } from '../en';

export const buildBox: KitMessages['buildBox'] = {
  placeholder: 'Vorrei un’app che…',
  password: 'password',
  unlimited:
    'Questo modello richiede la password e poi lavora senza limite di tempo: va avanti finché la Sua app non è finita, invece di fermarsi allo scadere del tempo.',
  ideas: [
    'un’app per la nostra associazione dove i soci si iscrivono agli eventi',
    'una lista per la cena in cui ognuno porta un piatto, così non ci sono doppioni',
    'un libro degli ospiti per il nostro matrimonio',
    'un sondaggio in cui tutti votano e vedono i risultati',
    'una classifica per la nostra serata quiz settimanale',
    'una pagina di compleanno dove gli amici lasciano i loro auguri',
  ],
  ahead: (waiting) =>
    waiting === 1 ? 'C’è un’app prima della Sua: poi tocca a Lei.' : `Ci sono ${waiting} app prima della Sua.`,
  starting: 'Avvio…',
  online: 'La Sua app è online',
  notOnline: 'La Sua app è stata creata, ma non è andata online.',
  open: 'Apri l’app',
  steps: {
    guide: 'Preparazione',
    examples: 'Uno sguardo agli esempi',
    create: 'Scelta di un indirizzo per la Sua app',
    write: 'Scrittura della Sua app',
    improve: 'Miglioramento della Sua app',
    check: 'Ricerca di errori',
    online: 'Pubblicazione online',
    trying: 'Prova dell’app',
    looking: 'Revisione della Sua app',
    forRecords: 'Preparazione dello spazio per le voci',
    forKeys: 'Preparazione dello spazio per chiavi e password',
    forFiles: 'Preparazione dello spazio per ciò che salva',
    records: 'Uno sguardo alle voci',
    keys: 'Verifica delle chiavi e password necessarie',
    addFile: 'Aggiunta di un file',
    removeFile: 'Rimozione di un file',
    files: 'Uno sguardo a ciò che ha salvato',
    services: 'Richiesta di usare altri servizi',
    site: (site) => `Consultazione di ${site}`,
  },
  further: 'Il suo link di modifica',
  keep:
    'Conservi questo link in un posto sicuro. È l’unico modo per accedere alla Sua app da un altro dispositivo e non si può recuperare, nemmeno da parte nostra.',
  copyLink: 'Copia il link di modifica',
  lifetime: (offline, removed) =>
    `Lo manteniamo online finché viene usato: dopo ${offline} giorni senza visite né modifiche va offline e dopo ${removed} giorni viene eliminato. Può rimetterlo online qui o nel suo editor.`,

  failedToStart: 'La richiesta non è partita.',
  noAnswer: 'Ha finito senza dire cosa è successo.',
  failed: 'Non ha funzionato.',
  profiles: {
    enterprise: {
      placeholder: 'Ci serve un’app che…',
      ideas: [
        'un modulo per richiedere le ferie, approvato dal responsabile',
        'un calendario per prenotare le nostre sale riunioni',
        'una checklist per l’inserimento dei nuovi colleghi',
        'un inventario della nostra attrezzatura e di chi la usa',
        'un elenco delle richieste di assistenza aperte e di chi se ne occupa',
        'un modulo di feedback per i nostri corsi di formazione',
      ],
    },
  },
};
