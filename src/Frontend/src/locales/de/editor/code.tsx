import type { EditorMessages } from '../../en/editor';

export const code: EditorMessages['code'] = {
  title: 'Code',
  version: (version) => `Version ${version}`,
  edited: ', bearbeitet',
  loadFailed: 'Diese Version konnte nicht geladen werden.',
  compiles: 'Kompiliert.',
  notYet: 'Kompiliert noch nicht.',
  checkFailed: 'Der Code konnte nicht geprüft werden.',
  saved: (version) => `Als Version ${version} gespeichert.`,
  featureSaved: 'Im Entwurf gespeichert. Deployen Sie die Vorschau, um ihn auszuprobieren.',
  featureLoadFailed: 'Der Entwurf konnte nicht geladen werden.',
  previewOnline: 'Die Vorschau ist online.',
  previewRefused: 'Die Vorschau hat sich nicht geändert. Was der Compiler sagt, steht unten.',
  isOnline: (version) => `Version ${version} ist online.`,
  notOnline: 'Ist nicht online gegangen. Was der Compiler sagt, steht unten.',
  failed: 'Das hat nicht geklappt.',
  unchanged: 'Seit dem letzten Speichern hat sich nichts geändert.',
  demo: 'Eine Demo, daher ist hier alles schreibgeschützt. Um etwas zu ändern, erstellen Sie damit ein eigenes Lambda.',
  hint: (b) => (
    <>
      Die Dateien einer Version. Ihr {b('Code')} ist das Programm und alles, was dazu gehört: Die .cs-Dateien ganz oben
      werden kompiliert, jede andere Datei – Dokumentation, Tests, das, woraus ein Frontend gebaut wird – wird mit der
      Version gespeichert und nie kompiliert oder ausgeliefert. Ihre {b('Ressourcen')} – Seiten, Scripts, Stile, Bilder,
      die Migrationen der Datenbank – werden zur Laufzeit gelesen und ausgeliefert und sind öffentlich, wo der Code sie
      ausliefert. Speichern legt eine neue Version an und lässt das, was online ist, unberührt; um eine Änderung zuerst
      auszuprobieren, beginnen Sie einen Entwurf. Strg+S speichert, F12 springt zu einer Deklaration.
    </>
  ),
  hintFeature: (b) => (
    <>
      Die Dateien dieses Entwurfs: sein {b('Code')} – die .cs-Dateien ganz oben werden kompiliert, der Rest wird mit ihm
      gespeichert – und seine {b('Ressourcen')}, die zur Laufzeit gelesen und ausgeliefert werden. Speichern behält sie
      im Entwurf und zeigt sie unter der eigenen Adresse des Entwurfs; Ihre Besucher sehen nichts davon, bis Sie den
      Entwurf online stellen.
    </>
  ),
  inFeature: (name) => `in „${name}“`,
  changedElsewhere: 'Der Entwurf wurde woanders gespeichert, seit Sie ihn geöffnet haben – vielleicht vom Agenten. Laden Sie den gespeicherten Stand, bevor Sie hier speichern; Ihre Änderungen würden nicht darüber gespeichert.',
  readAgain: 'Gespeicherten Stand laden',
  newer: (version) => `Version ${version} ist neuer als die hier geöffnete.`,
  check: 'Prüfen',
  save: 'Speichern',
  deploy: 'Deployen',
  deployPreviewTitle: 'Speichern und den Entwurf unter seiner eigenen Adresse online stellen, um ihn auszuprobieren',
  binary: (size) => `Kein Text, also gibt es hier nichts zu bearbeiten. Die Datei ist ${size} groß.`,
  saveAndDeploy: 'Speichern und deployen',
  saveVersion: 'Neue Version speichern',
  fromOlder: (version, newest) =>
    `Das geht von Version ${version} aus, und Version ${newest} ist neuer. Speichern macht es zur neuesten Version – ohne das, was nach Version ${version} kam.`,
  featureInstead: (start) => (
    <>
      Sie wollen etwas ausprobieren? {start('Speichern Sie es stattdessen in einem neuen Entwurf')}: Er bekommt eine
      eigene Adresse, und eine Version wird erst gespeichert, wenn alles passt.
    </>
  ),
  cancel: 'Abbrechen',
  what: 'Was ändert sich? Optional – erscheint im Verlauf.',
  placeholder: 'Fügt ein Kontaktformular hinzu',
  goToDefinition: 'Zur Definition springen',
  versionLabel: 'Version',
  shown: (version, online, newest) =>
    `Version ${version}${online ? ', online' : newest ? ', neueste' : ''}`,
  optionOnline: ' (online)',
  switchUnsaved: 'Ihre Änderungen hier sind nicht gespeichert. Die andere Version trotzdem öffnen?',
  noVersion: 'Es gibt noch keine Version, die angezeigt werden kann.',
  label: 'Dateien',
  codeGroup: 'Code',
  codeWhy: 'Wird nie ausgeliefert. Die .cs-Dateien ganz oben werden kompiliert; der Rest wird mit der Version gespeichert.',
  resources: 'Ressourcen',
  resourcesPublic: 'Öffentlich: Diese Version liefert sie mit Resources aus.',
  resourcesPrivate: 'Gehören zur Version, werden von ihr aber nicht ausgeliefert.',
  noResources: 'Keine in dieser Version.',
  count: (files) => (files === 1 ? '1 Datei' : `${files} Dateien`),
  groupUsage: (files, size) => `${files}, ${size}`,
  usage: (used, of) => `Diese Version umfasst ${used} der ${of}, die eine Version haben darf – Code und Ressourcen zusammen.`,
  scope: (data) => (
    <>Was das Lambda zur Laufzeit aufbewahrt, ist für jede Version dasselbe und steht unter {data('Daten')}.</>
  ),
  download: 'Herunterladen',
  newIn: (group) => `Neue Datei in ${group}`,
  uploadIn: (group) => `In ${group} hochladen`,
  pick: 'Wählen Sie eine Datei, um zu sehen, was darin steht.',
};
