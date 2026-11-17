import type { KitMessages } from '../en';

export const buildBox: KitMessages['buildBox'] = {
  placeholder: 'Ich möchte eine Website, die …',
  password: 'Passwort',
  fable:
    'Fable ist in der Testphase und durch ein Passwort geschützt. Es hat kein Zeitlimit und arbeitet, bis Ihre Website fertig ist – nicht, bis die Zeit abläuft.',
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
  further: 'Ihr Editor-Link',
  keep:
    'Heben Sie diesen Link gut auf. Er ist der einzige Weg zu Ihrer Website von einem anderen Gerät aus, und niemand kann ihn wiederherstellen – auch wir nicht.',
  copyLink: 'Editor-Link kopieren',
  lifetime: (offline, removed) =>
    `Wir halten sie online, solange sie genutzt wird: Nach ${offline} Tagen ohne Besuche oder Änderungen wird sie offline genommen, nach ${removed} Tagen gelöscht. Sie können sie hier oder in ihrem Editor wieder online stellen.`,

  failedToStart: 'Die Anfrage kam nicht durch.',
  noAnswer: 'Fertig, aber ohne Rückmeldung, was passiert ist.',
  failed: 'Das hat nicht geklappt.',
};
