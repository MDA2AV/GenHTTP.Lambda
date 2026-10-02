import type { EditorMessages } from '../../en/editor';

export const code: EditorMessages['code'] = {
  title: 'Codice',
  version: (version) => `versione ${version}`,
  edited: ', modificata',
  online: ', online',
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
  demo: 'È una demo, quindi qui è tutto in sola lettura. Per modificarla, crea una tua lambda partendo da questa. ',
  edit: 'Modifica il codice a mano. Salvando crei una nuova versione e quello che è online resta com’è; con il deploy va online. Per provare prima una modifica, avvia una bozza. ',
  editFeature:
    'Il codice di questa bozza. Salvando resta nella bozza: per i visitatori della lambda non cambia niente. Con il deploy va online all’indirizzo della bozza, per provarla; integrando la bozza diventa la prossima versione. ',
  inFeature: (name) => `in «${name}»`,
  changedElsewhere:
    'La bozza è stata salvata altrove da quando l’hai aperta, forse dall’agente. Carica quello che è salvato prima di salvare qui; le tue modifiche non verrebbero salvate sopra.',
  readAgain: 'Carica quello che è salvato',
  files: (entry, cs, context) => (
    <>
      {entry} restituisce ciò che viene servito, gli altri file {cs} contengono i tipi e ogni altro file viene servito
      così com’è, tranne quello che si trova in {context}: la documentazione e i test, mai compilati né serviti. Ctrl-S
      salva, F12 va alla dichiarazione.
    </>
  ),
  newer: (version) => ` La versione ${version} è più recente di quella aperta qui.`,
  check: 'Verifica',
  save: 'Salva',
  deploy: 'Deploy',
  deployPreviewTitle: 'Salva e metti online la bozza al suo indirizzo per provarla',
  binary: (size) => `Non è testo, quindi non c’è niente da modificare. Viene servito così com’è e pesa ${size} kB.`,
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
};
