import type { Messages } from '../en';

export const build: Messages['build'] = {
  title: 'Website mit KI erstellen.',
  intro:
    'Beschreiben Sie in eigenen Worten die Website oder App, die Sie sich vorstellen. Die KI erstellt sie für Sie, wir hosten sie, und in wenigen Minuten ist sie online – mit einem Link, den Sie an alle schicken können. Kostenlos, ohne Programmierkenntnisse, ohne Anmeldung.',
  placeholder: 'Ich möchte eine Website, die …',
  working: 'Läuft …',
  shortcut: 'Strg + Enter',
  building: 'Wird erstellt',
  buildIt: 'Website erstellen',
  builtBy: 'Erstellt von',
  password: 'Passwort',
  fable:
    'Fable ist in der Testphase und durch ein Passwort geschützt. Es hat kein Zeitlimit und arbeitet, bis die App fertig ist – nicht, bis die Zeit abläuft.',
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
    isOnline: 'online',
  },

  points: [
    {
      title: 'Ohne Programmierkenntnisse',
      text: 'Sagen Sie in eigenen Worten, was Ihre Website können soll – so, wie Sie es einem Freund erzählen würden. Die KI erstellt sie für Sie, Technikkenntnisse sind nicht nötig.',
    },
    {
      title: 'Kostenloses Hosting inklusive',
      text: 'Ihre Website läuft auf unseren Servern. Sie brauchen keinen Hosting-Tarif, keinen Server und keine Domain und müssen nichts installieren. Um Sicherheit und Updates kümmern wir uns.',
    },
    {
      title: 'In Minuten online',
      text: 'Sie erhalten sofort einen Link zum Teilen. Die Website merkt sich, was Besucher eintragen – Anmeldungen, Stimmen, Nachrichten, Punkte –, damit alle dasselbe sehen.',
    },
  ],

  questionsTitle: 'Bevor Sie loslegen',
  questions: (offline, removed) => [
    [
      'Kann ich mit KI wirklich kostenlos eine Website erstellen?',
      `Ja. Beschreiben Sie sie in eigenen Worten, und die KI erstellt sie, stellt sie online und gibt Ihnen den Link. Ohne Anmeldung, ohne Kreditkarte, ohne Testphase. Sie bleibt online, solange sie genutzt wird: Nach ${offline} Tagen ohne Besuch oder Änderung wird sie offline genommen, nach ${removed} Tagen gelöscht.`,
    ],
    [
      'Brauche ich Hosting, einen Server oder eine Domain?',
      'Nein. Ihre Website läuft auf unseren Servern, Hosting, Sicherheit und Updates inklusive. Sie bekommen sofort einen Link, eine Domain müssen Sie also auch nicht kaufen.',
    ],
    [
      'Kann ich ohne Programmierkenntnisse eine App erstellen?',
      'Ja. Code bekommen Sie nie zu sehen. Sagen Sie, was sie tun soll – so, wie Sie es einem Freund erzählen würden –, und die KI erledigt den Rest: eine Website, eine kleine App oder ein Spiel.',
    ],
    [
      'Können Besucher etwas eintragen – Anmeldungen, Stimmen, Nachrichten?',
      'Ja. Ihre Website merkt sich, was eingetragen wird. Alle, die den Link öffnen, sehen dieselben Einträge, Stimmen und Punkte.',
    ],
    [
      'Wie öffnen andere meine Website?',
      'Über den Link, in jedem Browser, auf dem Handy oder am Computer. Es muss nichts installiert werden, und kein App Store steht dazwischen.',
    ],
    [
      'Wie ändere ich meine Website später?',
      'Öffnen Sie den Editor-Link, den Sie mit Ihrer Website bekommen, und beschreiben Sie, was anders sein soll – genauso wie hier. Gefällt Ihnen eine Änderung nicht, können Sie zum vorherigen Stand zurückkehren.',
    ],
  ],

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
