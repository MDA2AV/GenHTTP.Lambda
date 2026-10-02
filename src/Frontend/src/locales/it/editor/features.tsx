import type { EditorMessages } from '../../en/editor';

export const features: EditorMessages['features'] = {
  hint:
    'Una bozza è una copia della tua app su cui provare una modifica prima che la veda qualcuno, con un indirizzo e dati di prova propri. Mettila online quando va bene; fino ad allora i visitatori continuano a ricevere quello che è online adesso.',
  newFeature: 'Nuova bozza',
  full: (limit) => `Ci sono già ${limit} bozze, il massimo consentito. Metti online o elimina una bozza prima.`,
  emptyTitle: 'Nessuna bozza',
  emptyText:
    'Una bozza è una copia della tua app su cui provare una modifica prima che vada online. Quando l’agente ti lascia una modifica da provare, la trovi qui.',
  start: 'Nuova bozza',
  askAgentNew: 'Chiedi una modifica all’agente',
  noChange: 'Non è ancora detto cosa cambia',
  behindTitle: 'La tua app è cambiata da quando è iniziata questa bozza',
  behind: () => 'non aggiornata',
  previewOnline: 'anteprima online',
  previewOutdated: 'l’anteprima mostra un salvataggio precedente',
  previewOffline: 'anteprima offline',
  changed: 'modificata',
  openPreview: 'Provala',
  openPreviewTitle: 'Apri l’anteprima in una nuova scheda',
  count: (open, limit) => `${open} di ${limit} bozze`,
  loading: 'Caricamento della bozza…',
  readFailed: 'Impossibile leggere la bozza.',

  newTitle: 'Nuova bozza',
  newText:
    'Una copia della tua app e dei suoi dati, con un indirizzo tutto suo. Modificala e provala lì: i visitatori non ne vedono niente finché non la metti online.',
  newTextFiles:
    'Quello che hai scritto finisce nella bozza invece di diventare una versione, così puoi provarlo al suo indirizzo prima che vada online.',
  name: 'Nome',
  namePlaceholder: 'Classifica',
  wanted: 'Cosa deve fare?',
  wantedPlaceholder: 'Facoltativo. Tieni i dieci punteggi migliori e mostrali dopo ogni partita.',
  olderBase: (newest) =>
    `Parte da una versione più vecchia, quindi nasce non aggiornata: prima di poterla mettere online, bisogna incorporare quello che è cambiato fino alla versione ${newest}.`,
  create: 'Avvia la bozza',
  createFailed: 'Impossibile avviare la bozza.',
  retry: 'Riprova',
  madeNotSaved: (name) =>
    `La bozza «${name}» è avviata, ma quello che hai scritto non è ancora stato inserito. Riprova, oppure chiudi questa finestra e cerca la bozza in Bozze.`,
  created: (name) => `La bozza «${name}» è avviata.`,
  cancel: 'Annulla',

  featureHint:
    'Una copia della tua app su cui provare questa modifica. La sua anteprima ha un indirizzo e dati di prova propri, quindi i visitatori non ne vedono niente finché non la metti online.',
  askAgent: 'Chiedi all’agente',
  askCatchUp: 'Chiedi all’agente di aggiornarla',
  catchUp: 'Aggiorna questa bozza alla versione più recente dell’app, mantenendo ciò che cambia.',
  editCode: 'Modifica il codice',
  deployPreview: 'Avvia l’anteprima',
  updatePreview: 'Aggiorna l’anteprima',
  previewDeployed: 'L’anteprima è in esecuzione.',
  previewFailed: 'Impossibile avviare l’anteprima.',
  previewStopped: 'L’anteprima è ferma.',
  previewRejected: 'L’anteprima non è cambiata',
  previewNotCompiling: 'Non compila, quindi l’anteprima mostra ancora l’ultima versione che compilava.',
  started: 'Avviata',
  changes: () => 'File modificati',
  noChanges: () => 'Ancora nessuna modifica.',
  editNotes: 'Nome e note',
  what: 'Cosa cambia?',
  whatPlaceholder: 'Aggiunge una classifica con i dieci punteggi migliori',
  missed: () => 'Cosa è cambiato nella tua app da quando è iniziata questa bozza',
  missedNothing: 'Niente nei file.',

  behindText: (_base, newest) =>
    `Dopo l’avvio di questa bozza è stata salvata la versione ${newest} della tua app. Mettere online la bozza adesso annullerebbe quello che ha cambiato, quindi prima va aggiornata: l’agente può farlo per te.`,
  moveBase: 'Segna come aggiornata',
  close: 'Chiudi',
  mergeTitle: (name) => `Metti online «${name}»`,
  mergeTitleShort: 'Falla diventare la nuova versione della tua app e mettila online',
  leaks: (path, files) =>
    `${files} contengono link a ${path}, cioè alla tua app online. Dall’anteprima, quei link leggono e modificano i suoi dati veri invece dei dati di prova. Chiedi all’agente di usare link senza quella parte («api/items»).`,
  mergeButton: 'Metti online',
  saveFirst: 'Prima salva le modifiche: l’anteprima e la messa online usano quello che è salvato.',
  mergeAndDeploy: () => 'Metti online',
  mergeText: (version) =>
    `Diventa la versione ${version} della tua app e va online. I dati della tua app restano come sono.`,
  deployTooNote: (active) => `La versione ${active} resta a un clic, nelle versioni.`,
  deployTooOffline: 'Ora la lambda è offline; così va online.',
  notCompiling: 'Non compila, quindi non è andata online. Prima sistemala nella bozza.',
  mergeFailed: 'Impossibile mettere online la bozza.',
  merged: (version) => `Salvata come versione ${version}.`,
  mergedOnline: (version) => `La versione ${version} è online.`,

  notesTitle: 'Nome e note',
  save: 'Salva',
  saveFailed: 'Impossibile salvare.',

  baseTitle: 'Segnarla come aggiornata?',
  baseText: () =>
    'Solo una bozza che contiene ciò che ha cambiato la versione più recente può andare online senza annullarlo. Se quelle modifiche ora sono in questa bozza, portate da te o dall’agente, segnala la bozza come aggiornata.',
  moveTo: () => 'Segna come aggiornata',
  baseWarning: 'Nessuno lo controlla. Se le modifiche non sono nella bozza, metterla online le annulla.',

  deleteTitle: (name) => `Scartare «${name}»?`,
  deleteText:
    'Il suo codice, la sua anteprima e i suoi dati di prova vengono eliminati per sempre. La tua app e le sue versioni non vengono toccate.',
  keep: 'Tienila',
  deleteForGood: 'Scarta',
  deleteFailed: 'Impossibile scartare la bozza.',
  deleted: (name) => `La bozza «${name}» è stata scartata.`,

  all: 'Tutte le bozze',
  actions: 'Altre azioni per questa bozza',
  download: 'Scarica come zip',
  stopPreview: 'Ferma l’anteprima',
  delete: 'Scarta questa bozza',
  viewsLabel: 'La bozza',
  views: {
    overview: 'Bozza',
    docs: 'Documentazione',
    code: 'Codice',
    tests: 'Test',
    data: 'Dati di prova',
    logs: 'Log',
  },
  missingTitle: 'Questa bozza non c’è più',
  missingText: 'È stata messa online o scartata. Le versioni mostrano che fine ha fatto.',
};
