import type { Messages } from '../en';

export const build: Messages['build'] = {
  title: 'Website mit KI erstellen.',
  intro:
    'Beschreiben Sie in eigenen Worten die Website oder App, die Sie sich vorstellen. Die KI erstellt sie für Sie, wir hosten sie, und in wenigen Minuten ist sie online – mit einem Link, den Sie an alle schicken können. Kostenlos, ohne Programmierkenntnisse, ohne Anmeldung.',
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

};
