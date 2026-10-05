import type { EditorMessages } from '../../en/editor';

export const files: EditorMessages['files'] = {
  hint: (b) => (
    <>
      I file di una versione: il programma. Il {b('Codice')} viene compilato e mai servito. Gli {b('Asset')} (pagine,
      script, stili, immagini) vengono salvati con il codice, lo seguono in ogni deploy e ripristino, e sono pubblici se
      il codice li serve. Quello che la lambda conserva mentre gira non è qui: sono i suoi {b('Dati')}.
    </>
  ),
  scope: (version, data) => (
    <>
      Appartengono alla versione {version} e cambiano con lei. Quello che la lambda conserva mentre gira è lo stesso per
      tutte le versioni, e si trova in {data('Dati')}.
    </>
  ),
  edit: 'Modifica questa versione',
  version: 'Versione',
  shown: (version, online, newest) => `Versione ${version}${online ? ', online' : newest ? ', la più recente' : ''}`,
  optionOnline: ' (online)',
  readFailed: 'Impossibile leggere quella versione.',
  noVersion: 'Non c’è ancora nessuna versione da mostrare.',
  label: 'File',
  code: 'Codice',
  codeWhy: 'Compilato nella lambda, mai servito.',
  count: (files) => (files === 1 ? '1 file' : `${files} file`),
  codeUsage: (files, used, of) => `${files}, ${used} di ${of} caratteri`,
  usage: (files, used, of) => `${files}, ${used} di ${of}`,
  noCode: 'Nessun codice in questa versione.',
  assets: 'Asset',
  assetsPublic: 'Pubblici: questa versione li serve con Assets.',
  assetsPrivate: 'Salvati con il codice, ma questa versione non li serve.',
  noAssets: 'Nessuno in questa versione.',
  context: 'Documentazione e test',
  contextWhy: 'Mai compilati e mai serviti: quello che è scritto su questa versione, per chi la legge o la modifica.',
  contextUsage: (files, size) => `${files}, ${size} - contati insieme agli asset`,
  noContext: 'Non c’è ancora niente di scritto su questa versione.',
  build: 'Build',
  buildWhy: 'Mai compilato e mai servito: ciò da cui sono costruiti il codice o gli asset, da chi li modifica.',
  data: 'Dati',
  dataPublic: 'Pubblici: il codice online li serve con Workspace.',
  dataPrivate: 'Privati, solo per la lambda. Non fanno parte di nessuna versione.',
  uploadFailed: (path) => `Impossibile caricare ${path}.`,
  deleteFolder: (path, held) =>
    held > 0
      ? `Eliminare ${path} e ${held === 1 ? 'il file che contiene' : `i ${held} file che contiene`}?`
      : `Eliminare la cartella ${path}?`,
  deleteFile: (path) => `Eliminare ${path}? La lambda non lo troverà più.`,
  deleteFailed: 'Eliminazione non riuscita.',
  full: 'Lo spazio per i dati è pieno',
  uploadInto: (folder) => `Carica in ${folder}`,
  upload: 'Carica',
  reading: 'Lettura…',
  noData: 'Ancora niente. Qui compare quello che la lambda salva mentre gira.',
  delete: (path) => `Elimina ${path}`,
  deleteShort: 'Elimina',
  fileFailed: 'Impossibile leggere il file.',
  pick: 'Scegli un file per vedere cosa contiene.',
  tooLarge: (name, size) => (
    <>
      {name} pesa {size}, troppo per mostrarlo qui.
    </>
  ),
  download: 'Scarica',
  readingFile: (name) => `Lettura di ${name}…`,
  missing: (name) => `Questa versione non ha nessun file chiamato ${name}.`,
  saved: 'salvato',
  notText: 'Non è testo. Scaricalo per vedere cosa contiene.',
};
