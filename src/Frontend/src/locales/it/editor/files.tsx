import type { EditorMessages } from '../../en/editor';

export const files: EditorMessages['files'] = {
  version: 'Versione',
  shown: (version, online, newest) => `Versione ${version}${online ? ', online' : newest ? ', la più recente' : ''}`,
  optionOnline: ' (online)',
  count: (files) => (files === 1 ? '1 file' : `${files} file`),
  usage: (files, used, of) => `${files}, ${used} di ${of}`,
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
