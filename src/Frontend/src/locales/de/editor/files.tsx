import type { EditorMessages } from '../../en/editor';

export const files: EditorMessages['files'] = {
  hint: (b) => (
    <>
      Die Dateien einer Version – das Programm. {b('Code')} wird kompiliert und nie ausgeliefert. {b('Assets')} –
      Seiten, Scripts, Styles, Bilder – werden mit dem Code gespeichert, mit ihm deployt und zurückgerollt und sind
      öffentlich, wenn der Code sie ausliefert. Was das Lambda zur Laufzeit aufbewahrt, steht nicht hier: Das sind
      seine {b('Daten')}.
    </>
  ),
  scope: (version, data) => (
    <>
      Diese Dateien gehören zu Version {version} und ändern sich mit ihr. Was das Lambda zur Laufzeit aufbewahrt, ist
      für alle Versionen gleich und steht unter {data('Daten')}.
    </>
  ),
  edit: 'Diese Version bearbeiten',
  version: 'Version',
  shown: (version, online, newest) => `Version ${version}${online ? ', online' : newest ? ', neueste' : ''}`,
  optionOnline: ' (online)',
  readFailed: 'Diese Version konnte nicht gelesen werden.',
  noVersion: 'Es gibt noch keine Version.',
  label: 'Dateien',
  code: 'Code',
  codeWhy: 'Wird ins Lambda kompiliert, nie ausgeliefert.',
  count: (files) => (files === 1 ? '1 Datei' : `${files} Dateien`),
  codeUsage: (files, used, of) => `${files}, ${used} von ${of} Zeichen`,
  usage: (files, used, of) => `${files}, ${used} von ${of}`,
  noCode: 'Kein Code in dieser Version.',
  assets: 'Assets',
  assetsPublic: 'Öffentlich: Diese Version liefert sie mit Assets aus.',
  assetsPrivate: 'Mit dem Code gespeichert, aber diese Version liefert sie nicht aus.',
  noAssets: 'Keine in dieser Version.',
  context: 'Dokumentation und Tests',
  contextWhy: 'Nie kompiliert und nie ausgeliefert: was über diese Version geschrieben ist, für alle, die sie lesen oder ändern.',
  contextUsage: (files, size) => `${files}, ${size} – zu den Assets gezählt`,
  noContext: 'Über diese Version ist noch nichts geschrieben.',
  build: 'Build',
  buildWhy: 'Nie kompiliert und nie ausgeliefert: das, woraus der Code oder die Assets gebaut werden, von wem sie ändert.',
  data: 'Daten',
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
