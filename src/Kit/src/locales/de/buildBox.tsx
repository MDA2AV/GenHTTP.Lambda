import type { KitMessages } from '../en';

export const buildBox: KitMessages['buildBox'] = {
  placeholder: 'Ich möchte eine Website, die …',
  shortcut: 'Strg + Enter',
  buildIt: 'Website erstellen',
  builtBy: 'Erstellt von',
  password: 'Passwort',
  fable:
    'Fable ist in der Testphase und durch ein Passwort geschützt. Es hat kein Zeitlimit und arbeitet, bis Ihre Website fertig ist – nicht, bis die Zeit abläuft.',
  onlyNew:
    'Hier entstehen neue Websites. Um eine bestehende zu ändern, öffnen Sie ihren Editor-Link und beschreiben unter „Ändern“, was anders sein soll.',
  ideas: [
    'eine Website für unseren Verein, auf der sich Mitglieder für Termine anmelden',
    'eine Mitbringliste für unser Sommerfest, damit nicht alle dasselbe mitbringen',
    'ein Gästebuch für unsere Hochzeit',
    'eine Umfrage, bei der alle abstimmen und das Ergebnis sehen',
    'eine Bestenliste für unseren wöchentlichen Quizabend',
    'eine Geburtstagsseite, auf der Freunde ihre Glückwünsche hinterlassen',
  ],
  ahead: (waiting) =>
    waiting === 1 ? 'Eine Website ist vor Ihrer an der Reihe – danach sind Sie dran.' : `${waiting} Websites sind vor Ihrer an der Reihe.`,
  starting: 'Startet …',
  asked: 'Ihre Anfrage',
  leaveOpen: 'Lassen Sie diese Seite geöffnet – den Link, mit dem Sie Ihre Website später ändern, sehen Sie nur hier, sobald sie fertig ist.',
  log: 'Was die KI getan hat',
  online: 'Ihre Website ist online',
  notOnline: 'Ihre Website wurde erstellt, ist aber nicht online gegangen.',
  open: 'Website öffnen',
  steps: {
    guide: 'Bereitet sich vor',
    examples: 'Sieht sich Beispiele an',
    create: 'Wählt eine Adresse für Ihre Website',
    write: 'Schreibt Ihre Website',
    improve: 'Verbessert Ihre Website',
    check: 'Prüft sie auf Fehler',
    online: 'Stellt sie online',
    trying: 'Probiert sie aus',
    looking: 'Sieht sich Ihre Website an',
    forRecords: 'Schafft Platz für ihre Einträge',
    forKeys: 'Schafft Platz für Schlüssel und Passwörter',
    forFiles: 'Schafft Platz für das, was sie speichert',
    records: 'Sieht sich ihre Einträge an',
    keys: 'Prüft, welche Schlüssel und Passwörter sie braucht',
    addFile: 'Fügt eine Datei hinzu',
    removeFile: 'Entfernt eine Datei',
    files: 'Sieht sich an, was sie gespeichert hat',
    services: 'Bittet darum, andere Dienste zu nutzen',
  },

  yourApp: 'Ihre Website',
  further: 'Um sie später zu ändern',
  keep:
    'Heben Sie diesen Link gut auf. Er ist der einzige Weg zurück, und niemand kann ihn wiederherstellen – auch wir nicht. Setzen Sie ein Lesezeichen, bevor Sie den Tab schließen.',
  change:
    'Um Ihre Website zu ändern, öffnen Sie den Editor-Link und beschreiben unter „Ändern“, was anders sein soll – genauso wie hier. Das kann auch Ihr eigener KI-Assistent, wie unten beschrieben.',
  copyLink: 'Editor-Link kopieren',
  lifetime: (offline, removed) =>
    `Wir halten sie online, solange sie genutzt wird: Nach ${offline} Tagen ohne Besuche oder Änderungen wird sie offline genommen, nach ${removed} Tagen gelöscht. Im Editor können Sie sie wieder online stellen.`,
  openEditor: 'Editor öffnen',
  another: 'Weitere Website erstellen',

  keepGoing: 'Mit Ihrem eigenen KI-Assistenten weitermachen',
  orOwn: 'Oder nutzen Sie Ihren eigenen KI-Assistenten',
  ownText:
    'Sie arbeiten bereits mit Claude oder einem anderen KI-Assistenten? Verbinden Sie ihn hier, und er erstellt und ändert Websites für Sie auf dieselbe Weise. Wir betreiben sie, Sie müssen also weiterhin nichts einrichten. Ein Tageslimit gibt es nicht.',
  ownTitle: 'Erstellen Sie Ihre Website mit Ihrem KI-Assistenten',
  ownOnly:
    'Verbinden Sie Claude oder einen anderen KI-Assistenten mit der Adresse unten und beschreiben Sie die gewünschte Website. Er erstellt sie, wir betreiben sie auf unseren Servern, und sie ist sofort online – mit einem Link zum Teilen.',
  thenAsk:
    'Sagen Sie ihm dann, was Sie möchten, zum Beispiel: „Erstelle eine Website für unseren Chor mit einem Kalender unserer Konzerte.“',
  howToChange:
    'So ändern Sie eine Website auch später: Geben Sie Ihrem Assistenten den Editor-Link und sagen Sie ihm, was anders sein soll.',

  failedToStart: 'Die Anfrage kam nicht durch.',
  noAnswer: 'Fertig, aber ohne Rückmeldung, was passiert ist.',
  failed: 'Das hat nicht geklappt.',
};
