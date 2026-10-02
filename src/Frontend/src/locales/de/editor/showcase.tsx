import type { EditorMessages } from '../../en/editor';

export const showcase: EditorMessages['showcase'] = {
  loadFailed: 'Der Showcase konnte nicht geladen werden.',
  loading: 'Lädt …',
  title: 'Titel',
  description: 'Beschreibung',
  picture: 'Bild',
  updated: 'Der Showcase-Eintrag ist aktualisiert.',
  listed: 'Das Lambda ist jetzt im Showcase.',
  waiting: 'Gespeichert. Es erscheint im Showcase, sobald das Lambda online ist.',
  saveFailed: 'Der Showcase-Eintrag konnte nicht gespeichert werden.',
  removed: 'Aus dem Showcase genommen.',
  removeFailed: 'Der Showcase-Eintrag konnte nicht entfernt werden.',
  wrongType: 'Das ist kein PNG-, JPEG-, GIF- oder WebP-Bild.',
  tooLarge: (size, limit) => `Das sind ${size}; ein Bild darf höchstens ${limit} groß sein.`,
  unreadable: 'Die Datei konnte nicht gelesen werden.',
  hint: (tool) => (
    <>
      Der Showcase zeigt Lambdas, die ihre Besitzer zeigen möchten – zuletzt genutzte zuerst. Nur wer den
      Editor-Schlüssel hat, kann ein Lambda dort eintragen oder entfernen. Und es erscheint nur, solange es online ist.
      Ein Agent kann dasselbe mit seinem Tool {tool}.
    </>
  ),
  open: 'Showcase öffnen',
  switch: 'Dieses Lambda im Showcase zeigen',
  listedNow: 'Jetzt gelistet. Wer den Showcase durchstöbert, kann es öffnen.',
  notListed: 'Gespeichert, aber nicht gelistet: Das Lambda ist offline. Nach dem nächsten Deployment ist es wieder da.',
  off: 'Aus. Von diesem Lambda wird nirgends etwas gezeigt, bis Sie das einschalten und speichern.',
  offline: 'Das Lambda ist offline, der Eintrag wartet also auf das nächste Deployment. Gelistet werden nur Lambdas, die antworten.',
  titleLabel: 'Titel',
  titlePlaceholder: 'Punktestand fürs Kneipenquiz',
  descriptionLabel: 'Beschreibung',
  descriptionPlaceholder:
    'Teams tippen ihre Antworten aufs Handy, die Spielleitung wertet aus, und der Punktestand aktualisiert sich für alle im Raum.',
  save: 'Änderungen speichern',
  add: 'Zum Showcase hinzufügen',
  takeOff: 'Herausnehmen',
  needs: (missing) =>
    `Es fehlt noch: ${missing.length > 1 ? `${missing.slice(0, -1).join(', ')} und ${missing[missing.length - 1]}` : missing[0]}.`,
  tooLong: 'Etwas davon ist zu lang.',
  allSaved: 'Alles gespeichert.',
  preview: 'Vorschau',
  card: (address) => <>So sehen Besucher die Karte. Sie öffnet {address}.</>,
  confirm: 'Aus dem Showcase nehmen?',
  keep: 'Behalten',
  confirmText: 'Titel, Beschreibung und Bild werden gelöscht. Das Lambda selbst bleibt genau, wie es ist.',
  pictureLabel: 'Bild',
  formats: (limit) => `PNG, JPEG, GIF oder WebP, bis ${limit}`,
  notSaved: 'noch nicht gespeichert',
  replace: 'Ziehen Sie ein neues Bild hierher, um es zu ersetzen.',
  drop: 'Ziehen Sie ein Bild hierher.',
  advice: 'Am besten wirkt ein Screenshot oder ein kurzes GIF der App in Aktion, im Format 16:10.',
  another: 'Anderes Bild wählen',
  choose: 'Datei wählen',
  keepSaved: 'Gespeichertes behalten',
  clear: 'Leeren',
};
