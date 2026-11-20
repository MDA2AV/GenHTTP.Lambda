import type { KitMessages } from '../en';

export const buildBox: KitMessages['buildBox'] = {
  placeholder: 'Ich möchte eine Website, die …',
  password: 'Passwort',
  unlimited:
    'Dieses Modell braucht das Passwort und läuft dann ohne Zeitlimit: Es arbeitet weiter, bis Ihre Website fertig ist, und nicht nur, bis die Zeit abgelaufen ist.',
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
  profiles: {
    enterprise: {
      placeholder: 'Wir brauchen eine App, die …',
      unlimited:
        'Dieses Modell braucht das Passwort und läuft dann ohne Zeitlimit: Es arbeitet weiter, bis Ihre App fertig ist, und nicht nur, bis die Zeit abgelaufen ist.',
      ideas: [
        'ein Formular für Urlaubsanträge, die die Teamleitung freigibt',
        'ein Buchungskalender für unsere Besprechungsräume',
        'eine Checkliste für das Onboarding neuer Kolleginnen und Kollegen',
        'eine Übersicht über unsere Geräte und wer sie hat',
        'eine Liste offener Supportanfragen und wer sie bearbeitet',
        'ein Feedbackformular für unsere Schulungen',
      ],
      ahead: (waiting) => (waiting === 1 ? 'Eine App ist vor Ihrer an der Reihe – danach sind Sie dran.' : `${waiting} Apps sind vor Ihrer an der Reihe.`),
      online: 'Ihre App ist online',
      notOnline: 'Ihre App wurde erstellt, ist aber nicht online gegangen.',
      open: 'App öffnen',
      steps: {
        create: 'Wählt eine Adresse für Ihre App',
        write: 'Schreibt Ihre App',
        improve: 'Verbessert Ihre App',
        looking: 'Sieht sich Ihre App an',
      },
      keep: 'Heben Sie diesen Link gut auf. Er ist der einzige Weg zu Ihrer App von einem anderen Gerät aus, und niemand kann ihn wiederherstellen – auch nicht die Betreiber dieser Website.',
    },
  },
};
