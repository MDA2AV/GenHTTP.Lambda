import type { EditorMessages } from '../../en/editor';

export const code: EditorMessages['code'] = {
  title: 'Code',
  version: (version) => `Version ${version}`,
  edited: ', bearbeitet',
  online: ', online',
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
  demo: 'Eine Demo, daher ist hier alles schreibgeschützt. Um etwas zu ändern, erstellen Sie damit ein eigenes Lambda. ',
  edit: 'Code von Hand bearbeiten. Speichern legt eine neue Version an und lässt unberührt, was online ist; Deployen stellt sie online. Um eine Änderung zuerst auszuprobieren, beginnen Sie einen Entwurf. ',
  editFeature:
    'Der Code dieses Entwurfs. Speichern behält ihn im Entwurf – für die Besucher des Lambdas ändert sich nichts. Deployen stellt ihn unter der eigenen Adresse des Entwurfs online, zum Ausprobieren; wird der Entwurf übernommen, wird er zur nächsten Version. ',
  inFeature: (name) => `in „${name}“`,
  changedElsewhere: 'Der Entwurf wurde woanders gespeichert, seit Sie ihn geöffnet haben – vielleicht vom Agenten. Laden Sie den gespeicherten Stand, bevor Sie hier speichern; Ihre Änderungen würden nicht darüber gespeichert.',
  readAgain: 'Gespeicherten Stand laden',
  files: (entry, cs, context) => (
    <>
      {entry} gibt zurück, was ausgeliefert wird. Weitere {cs}-Dateien enthalten Typen, alle anderen Dateien werden
      ausgeliefert, wie sie sind – außer denen in {context}: Das sind die Dokumentation und die Tests, die nie
      kompiliert oder ausgeliefert werden. Strg+S speichert, F12 springt zur Deklaration.
    </>
  ),
  newer: (version) => ` Version ${version} ist neuer als die hier geöffnete.`,
  check: 'Prüfen',
  save: 'Speichern',
  deploy: 'Deployen',
  deployPreviewTitle: 'Speichern und den Entwurf unter seiner eigenen Adresse online stellen, um ihn auszuprobieren',
  binary: (size) => `Kein Text, also nichts zu bearbeiten. Die Datei wird ausgeliefert, wie sie ist, und ist ${size} kB groß.`,
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
};
