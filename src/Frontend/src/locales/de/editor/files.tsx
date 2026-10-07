import type { EditorMessages } from '../../en/editor';

export const files: EditorMessages['files'] = {
  version: 'Version',
  shown: (version, online, newest) => `Version ${version}${online ? ', online' : newest ? ', neueste' : ''}`,
  optionOnline: ' (online)',
  count: (files) => (files === 1 ? '1 Datei' : `${files} Dateien`),
  usage: (files, used, of) => `${files}, ${used} von ${of}`,
  dataPublic: 'Öffentlich: Der Code, der online ist, liefert sie mit Workspace aus.',
  dataPrivate: 'Privat, nur für das Lambda. Gehört zu keiner Version.',
  uploadFailed: (path) => `${path} konnte nicht hochgeladen werden.`,
  deleteFolder: (path, held) =>
    held > 0
      ? `${path} und ${held === 1 ? 'die Datei darin' : `die ${held} Dateien darin`} löschen?`
      : `Den Ordner ${path} löschen?`,
  deleteFile: (path) => `${path} löschen? Das Lambda findet die Datei dann nicht mehr.`,
  deleteFailed: 'Das konnte nicht gelöscht werden.',
  full: 'Der Datenspeicher ist voll',
  uploadInto: (folder) => `In ${folder} hochladen`,
  upload: 'Hochladen',
  reading: 'Wird gelesen …',
  noData: 'Noch nichts. Was das Lambda zur Laufzeit speichert, erscheint hier.',
  delete: (path) => `${path} löschen`,
  deleteShort: 'Löschen',
  fileFailed: 'Die Datei konnte nicht gelesen werden.',
  pick: 'Wählen Sie eine Datei, um ihren Inhalt zu sehen.',
  tooLarge: (name, size) => (
    <>
      {name} ist {size} groß – zu groß für die Anzeige hier.
    </>
  ),
  download: 'Herunterladen',
  readingFile: (name) => `${name} wird gelesen …`,
  missing: (name) => `Diese Version hat keine Datei namens ${name}.`,
  saved: 'gespeichert',
  notText: 'Kein Text. Laden Sie die Datei herunter, um hineinzusehen.',
};
