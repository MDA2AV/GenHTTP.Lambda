import type { EditorMessages } from '../../en/editor';

export const code: EditorMessages['code'] = {
  title: 'Codice',
  version: (version) => `versione ${version}`,
  edited: ', modificata',
  loadFailed: 'Impossibile caricare quella versione.',
  compiles: 'Compila.',
  notYet: 'Non compila ancora.',
  checkFailed: 'Impossibile verificare il codice.',
  saved: (version) => `Salvata come versione ${version}.`,
  featureSaved: 'Salvato nella bozza. Fai il deploy dell’anteprima per provarla.',
  featureLoadFailed: 'Impossibile caricare la bozza.',
  previewOnline: 'L’anteprima è online.',
  previewRefused: 'L’anteprima non è cambiata. Guarda qui sotto cosa dice il compilatore.',
  isOnline: (version) => `La versione ${version} è online.`,
  notOnline: 'Non è andata online. Guarda qui sotto cosa dice il compilatore.',
  failed: 'Non ha funzionato.',
  unchanged: 'Nessuna modifica dall’ultimo salvataggio.',
  demo: 'È una demo, quindi qui è tutto in sola lettura. Per modificarla, crea una tua lambda partendo da questa.',
  hint: (b) => (
    <>
      I file di una versione. Il suo {b('codice')} è il programma e tutto ciò che si conserva con lui: i suoi file .cs
      vengono compilati, in qualsiasi cartella, e ogni altro file (la documentazione, i test, ciò da cui è costruito un
      front end) resta con la versione e non viene mai compilato né servito. Le sue {b('risorse')} (pagine, script,
      stili, immagini, le migrazioni del database) vengono lette e servite mentre gira, e sono pubbliche dove il codice
      le serve. Salvare crea una nuova versione e lascia stare quella online; per provare prima una modifica, avvia una
      bozza. Ctrl-S salva, F12 va alla dichiarazione.
    </>
  ),
  hintFeature: (b) => (
    <>
      I file di questa bozza: il suo {b('codice')} (i suoi file .cs vengono compilati, in qualsiasi cartella, il resto
      resta con lui) e le sue {b('risorse')}, lette e servite mentre gira. Salvando, restano nella bozza e compaiono al
      suo indirizzo; i tuoi visitatori non ne vedono nulla finché non metti online la bozza.
    </>
  ),
  inFeature: (name) => `in «${name}»`,
  changedElsewhere:
    'La bozza è stata salvata altrove da quando l’hai aperta, forse dall’agente. Carica quello che è salvato prima di salvare qui; le tue modifiche non verrebbero salvate sopra.',
  readAgain: 'Carica quello che è salvato',
  newer: (version) => `La versione ${version} è più recente di quella aperta qui.`,
  check: 'Verifica',
  save: 'Salva',
  deploy: 'Deploy',
  deployPreviewTitle: 'Salva e metti online la bozza al suo indirizzo per provarla',
  binary: (size) => `Non è testo, quindi qui non c’è niente da modificare. Pesa ${size}.`,
  saveAndDeploy: 'Salva e fai il deploy',
  saveVersion: 'Salva una nuova versione',
  fromOlder: (version, newest) =>
    `Questo codice parte dalla versione ${version}, ma la versione ${newest} è più recente. Salvando diventa la versione più recente, senza quello che è venuto dopo la versione ${version}.`,
  featureInstead: (start) => (
    <>
      Vuoi provare qualcosa? {start('Mettilo invece in una nuova bozza')}: avrà un indirizzo tutto suo, e nessuna
      versione viene salvata finché non è a posto.
    </>
  ),
  cancel: 'Annulla',
  what: 'Cosa cambia? Facoltativo: compare nella cronologia.',
  placeholder: 'Aggiunge un modulo di contatto',
  goToDefinition: 'Vai alla definizione',
  versionLabel: 'Versione',
  shown: (version, online, newest) =>
    `Versione ${version}${online ? ', online' : newest ? ', la più recente' : ''}`,
  optionOnline: ' (online)',
  switchUnsaved: 'Quello che hai cambiato qui non è salvato. Aprire comunque l’altra versione?',
  noVersion: 'Non c’è ancora nessuna versione da mostrare.',
  label: 'File',
  codeGroup: 'Codice',
  codeWhy: 'Mai servito. I file .cs vengono compilati, in qualsiasi cartella; il resto resta con la versione.',
  resources: 'Risorse',
  resourcesPublic: 'Pubbliche: questa versione le serve con Resources.',
  resourcesPrivate: 'Distribuite con la versione, ma questa versione non le serve.',
  noResources: 'Nessuna in questa versione.',
  count: (files) => (files === 1 ? '1 file' : `${files} file`),
  groupUsage: (files, size) => `${files}, ${size}`,
  usage: (used, of) => `Questa versione occupa ${used} dei ${of} che una versione può avere, tra codice e risorse.`,
  scope: (data) => (
    <>Quello che la lambda conserva mentre gira è lo stesso per ogni versione e si trova in {data('Dati')}.</>
  ),
  download: 'Scarica',
  newIn: (group) => `Nuovo file in ${group}`,
  uploadIn: (group) => `Carica in ${group}`,
  pick: 'Scegli un file per vedere cosa contiene.',
};
