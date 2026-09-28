import type { Messages } from '../en';

export const build: Messages['build'] = {
  title: 'Von der Idee zur Website.',
  intro:
    'Beschreiben Sie die Website oder App, die Sie sich vorstellen. Eine KI erstellt sie für Sie, wir betreiben sie auf unseren Servern, und sie ist sofort online – mit einem Link, den Sie an alle schicken können. Ohne Programmieren, ohne Hosting einzurichten, ohne Konto.',
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
    'ein Gästebuch für unsere Hochzeit',
    'eine Umfrage, bei der alle abstimmen und das Ergebnis sehen',
    'eine Punktetafel für unseren wöchentlichen Quizabend',
    'ein Countdown bis zu unserer Eröffnung, den alle sehen können',
  ],
  ahead: (waiting) =>
    waiting === 1 ? 'Eine Website ist vor Ihrer an der Reihe – danach sind Sie dran.' : `${waiting} Websites sind vor Ihrer an der Reihe.`,
  starting: 'Startet …',

  points: [
    {
      title: 'Beschrieben, nicht programmiert',
      text: 'Sagen Sie in eigenen Worten, was Ihre Website können soll. Programmier- oder Technikkenntnisse sind nicht nötig.',
    },
    {
      title: 'Hosting inklusive',
      text: 'Ihre Website läuft auf unseren Servern. Um Hosting, Sicherheit und Updates kümmern wir uns – Sie müssen nichts einrichten oder warten.',
    },
    {
      title: 'In Minuten online',
      text: 'Sie erhalten sofort einen Link zum Teilen. Die Website kann sich auch etwas merken – Einträge, Stimmen, Punkte –, damit alle dasselbe sehen.',
    },
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
