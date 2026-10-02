import type { EditorMessages } from '../../en/editor';

export const openSource: EditorMessages['openSource'] = {
  loading: 'Lädt …',
  loadFailed: 'Ob der Code veröffentlicht ist, konnte nicht gelesen werden.',
  hint: (tool) => (
    <>
      Ein veröffentlichtes Lambda kann jeder auf seiner Seite unter Open Source lesen, mit einem Stern versehen und
      herunterladen – jede Version davon, unter der Lizenz Ihrer Wahl, und nie die Daten, die es aufbewahrt. Nur wer
      den Editor-Schlüssel hat, kann es veröffentlichen oder zurückziehen. Ein Agent kann dasselbe mit seinem Tool{' '}
      {tool}.
    </>
  ),
  hintSimple:
    'Jeder kann auf einer eigenen Seite lesen, wie Ihre App gemacht ist, und unter der Lizenz Ihrer Wahl darauf aufbauen – nie mit dem, was sie aufbewahrt. Nur Sie können den Code veröffentlichen oder zurückziehen.',
  open: 'Seite des Codes öffnen',
  switch: 'Den Code dieser App veröffentlichen',
  publishedNow: (license) => `Unter ${license} veröffentlicht. Jeder kann ihn lesen und herunterladen.`,
  off: 'Aus. Niemand sieht den Code, bis Sie ihn veröffentlichen.',
  keptStars: (stars) =>
    stars === 1
      ? 'Sein Stern bleibt erhalten, falls Sie ihn wieder veröffentlichen.'
      : `Seine ${stars} Sterne bleiben erhalten, falls Sie ihn wieder veröffentlichen.`,
  published: 'Veröffentlicht. Jeder kann den Code jetzt lesen.',
  saved: 'Gespeichert.',
  saveFailed: 'Der Code konnte nicht veröffentlicht werden.',
  withdrawn: 'Zurückgezogen. Seine Seite gibt es nicht mehr.',
  withdrawFailed: 'Der Code konnte nicht zurückgezogen werden.',
  whatTitle: 'Was veröffentlicht wird',
  what: [
    'Der Code – wie er jetzt ist, und jeder frühere Stand davon',
    'Alles, was die App zeigt: ihre Seiten, ihre Gestaltung und ihre Bilder',
    'Was über sie geschrieben ist: wofür sie da ist und wie sie getestet wird',
    'Jede Änderung, die sie durchlaufen hat, in je einer Zeile',
  ],
  neverTitle: 'Was nie veröffentlicht wird',
  never: [
    'Was sie aufbewahrt: ihre Einträge, was sie gespeichert hat, ihre Schlüssel und Passwörter',
    'Worum Sie gebeten haben, in Ihren eigenen Worten',
    'Wer sie nutzt: ihre Besucher und was sie getan haben',
    'Der Editor-Link',
  ],
  careful:
    'Alles im Code wird öffentlich, auch seine früheren Stände. Ein Passwort oder Schlüssel gehört nie in den Code – sondern zu den Schlüsseln und Passwörtern unter Daten, die nie veröffentlicht werden.',
  licenseLabel: 'Lizenz',
  licenseHint: 'Was andere mit dem Code tun dürfen. MIT, die gängigste, erlaubt jedem fast alles damit, solange Ihr Name dabeibleibt.',
  readLicense: 'Lizenz lesen',
  authorLabel: 'Name in der Lizenz',
  optional: 'optional',
  authorPlaceholder: (key) => `Die Autoren von ${key}`,
  authorHint: 'Ihr Name oder der Ihrer Organisation, angezeigt auf der Seite des Codes und in der Lizenz. Bleibt das Feld leer, nennt die Lizenz die Autoren dieser App.',
  publish: 'Veröffentlichen',
  save: 'Änderungen speichern',
  allSaved: 'Alles gespeichert.',
  takeDown: 'Zurückziehen',
  confirm: 'Den Code zurückziehen?',
  confirmText:
    'Seine Seite und seine Downloads verschwinden sofort. Wer ihn schon heruntergeladen hat, behält ihn unter der Lizenz, unter der er ihn bekommen hat. Seine Sterne bleiben erhalten, falls Sie ihn wieder veröffentlichen.',
  keep: 'Veröffentlicht lassen',
  stars: (count) => (count === 1 ? '1 Stern' : `${count} Sterne`),
  sidebar: 'Quellcode',
};
