import type { EditorMessages } from '../../en/editor';
import { many } from './language';

export const files: EditorMessages['files'] = {
  version: 'Versie',
  shown: (version, online, newest) => `Versie ${version}${online ? ', online' : newest ? ', nieuwste' : ''}`,
  optionOnline: ' (online)',
  count: (files) => many(files, 'bestand', 'bestanden'),
  usage: (files, used, of) => `${files}, ${used} van ${of}`,
  dataPublic: 'Openbaar: de code die online staat, serveert de data met Workspace.',
  dataPrivate: 'Alleen voor de lambda zelf. Hoort bij geen enkele versie.',
  uploadFailed: (path) => `${path} kon niet worden geüpload.`,
  deleteFolder: (path, held) =>
    held > 0
      ? `${path} verwijderen, met ${held === 1 ? 'het bestand' : `de ${held} bestanden`} erin?`
      : `De map ${path} verwijderen?`,
  deleteFile: (path) => `${path} verwijderen? De lambda kan het dan niet meer vinden.`,
  deleteFailed: 'Verwijderen is niet gelukt.',
  full: 'De data-opslag is vol',
  uploadInto: (folder) => `Uploaden naar ${folder}`,
  upload: 'Uploaden',
  reading: 'Lezen…',
  noData: 'Nog niets. Wat de lambda opslaat terwijl hij draait, verschijnt hier.',
  delete: (path) => `${path} verwijderen`,
  deleteShort: 'Verwijderen',
  fileFailed: 'Het bestand kon niet worden gelezen.',
  pick: 'Kies een bestand om te zien wat erin staat.',
  tooLarge: (name, size) => (
    <>
      {name} is {size}, te groot om hier te tonen.
    </>
  ),
  download: 'Downloaden',
  readingFile: (name) => `${name} lezen…`,
  missing: (name) => `Deze versie heeft geen bestand met de naam ${name}.`,
  saved: 'opgeslagen',
  notText: 'Geen tekst. Download het om erin te kijken.',
};
