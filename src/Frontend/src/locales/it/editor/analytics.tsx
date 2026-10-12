import type { EditorMessages } from '../../en/editor';

export const analytics: EditorMessages['analytics'] = {
  hint: 'Contate per te dalla piattaforma: niente cookie, niente codice nella tua app e niente da accettare per i tuoi visitatori.',
  loading: 'Lettura delle visite…',
  loadFailed: 'Non è stato possibile leggere le visite.',

  offTitle: 'Scopri chi visita la tua app',
  offText:
    'Quante persone arrivano ogni giorno, quali pagine aprono, da quali siti arrivano, da quali paesi e con quali dispositivi: contati giorno per giorno, dal momento in cui lo attivi.',
  offPrivate: 'Niente cookie e nessun banner che i visitatori debbano accettare',
  offAddresses: 'Non si conserva nessun indirizzo, solo quante visite ci sono state ogni giorno',
  offKept: (days) =>
    days >= 365
      ? 'Conservati per un anno e mostrati per intero'
      : `Conservati per un anno: vengono mostrati gli ultimi ${days} giorni, tutto con Premium`,
  switchOn: 'Attiva Analytics',
  switchedOn: 'Analytics è attivo. Le prime visite compaiono entro pochi minuti.',
  switchOnFailed: 'Non è stato possibile attivare Analytics.',
  demo: 'I visitatori di una demo non vengono contati. Una copia tutta tua può contare i suoi visitatori.',

  range: 'Intervallo di tempo',
  days: (days) => (days >= 365 ? '12 mesi' : `${days} giorni`),
  premiumRange: 'Premium mostra fino a un anno',

  visitors: 'Visitatori',
  visitorsHint: 'Persone diverse, ognuna contata una volta al giorno.',
  views: 'Visualizzazioni di pagina',
  viewsHint: 'Ogni volta che qualcuno ha aperto una pagina.',
  hits: 'Richieste',
  hitsHint: 'Tutto ciò che è stato chiesto alla tua app: le sue pagine, e le immagini e gli script che caricano.',
  versus: (days) => `rispetto ai ${days} giorni precedenti`,
  noChange: 'invariato',
  inDays: (days) => (days >= 365 ? 'negli ultimi 12 mesi' : `negli ultimi ${days} giorni`),

  perDay: {
    visitors: 'Visitatori al giorno',
    views: 'Visualizzazioni di pagina al giorno',
    hits: 'Richieste al giorno',
  },
  perWeek: {
    visitors: 'Visitatori a settimana',
    views: 'Visualizzazioni di pagina a settimana',
    hits: 'Richieste a settimana',
  },
  showValues: 'Mostra i valori',
  showChart: 'Mostra il grafico',
  day: 'Giorno',
  week: 'Settimana del',

  nobody: 'In questi giorni non ha visitato nessuno.',
  nobodyYet: 'Nessuno ha visitato da quando Analytics è stato attivato. Le prime visite compaiono entro pochi minuti.',

  pages: 'Pagine',
  pagesHint: 'Le pagine aperte dalle persone, le più aperte per prime.',
  requested: 'Più richieste',
  requestedHint: 'Tutto ciò che è stato chiesto alla tua app, per indirizzo: non ha pagine che le persone aprono.',
  sources: 'Da dove arrivano i visitatori',
  sourcesHint: 'I siti che hanno mandato qui le persone, per la pagina in cui si trovavano. I motori di ricerca dicono solo di essere stati loro, non cosa è stato cercato.',
  direct: 'Digitato, un segnalibro o un’app',
  countries: 'Paesi',
  countriesHint: 'Dove sono registrati gli indirizzi internet dei visitatori: di solito, non sempre, dove si trovano.',
  unknownCountry: 'Sconosciuto',
  devices: 'Dispositivi',
  device: {
    desktop: 'Computer',
    mobile: 'Telefono',
    tablet: 'Tablet',
    bot: 'Bot',
    other: 'Altro',
  },
  browsers: 'Browser',
  systems: 'Sistemi',
  crawlers: 'Motori di ricerca e IA',
  crawlersHint: 'Programmi passati da qui: motori di ricerca che indicizzano le tue pagine, assistenti IA che le leggono, chat che mostrano l’anteprima di un link.',
  noCrawlers: 'In questi giorni non è passato nessun motore di ricerca né IA.',
  other: 'Altri',
  otherBots: 'Altri programmi',
  showAll: (count) => `Mostra tutti (${count})`,
  showFewer: 'Mostra meno',
  share: (part, whole) => `${part} su ${whole}`,
  none: 'Ancora niente.',

  moreKept: (since) => `Contate dal ${since}: con Premium vedi tutto, fino a un anno.`,
  premiumAsk: 'Chiedi di Premium',
  premiumSubject: 'Premium per la mia app',
  premiumBody: (address) => `Ciao,\n\nvorrei vedere tutte le analisi della mia app: ${address}\n\n`,

  footer: (minutes) =>
    minutes === 1 ? 'Conteggio al giorno, in UTC. Aggiornato ogni minuto.' : `Conteggio al giorno, in UTC. Aggiornato ogni ${minutes} minuti.`,
  updated: (time) => `Ultimo aggiornamento alle ${time}.`,
  since: (date) => `Conteggio dal ${date}.`,

  switchOff: 'Disattiva Analytics',
  switchOffTitle: 'Disattivare Analytics?',
  switchOffText: 'Il conteggio si ferma e tutto ciò che è stato contato finora viene eliminato. Non si può annullare.',
  keep: 'Continua a contare',
  switchOffConfirm: 'Disattiva ed elimina',
  switchedOff: 'Analytics è disattivato e ciò che era stato contato è stato eliminato.',
  switchOffFailed: 'Non è stato possibile disattivare Analytics.',
};
