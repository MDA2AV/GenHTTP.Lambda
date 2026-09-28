import type { Messages } from '../en';

export const build: Messages['build'] = {
  offTitle: 'Hier nicht aktiviert',
  off: (write, mcp) => (
    <>
      Diese Installation hat keinen Build-Agenten. Sie können den Code aber {write('selbst schreiben')} oder Ihren
      eigenen Claude mit {mcp} verbinden.
    </>
  ),

  title: 'Sagen Sie, was Sie möchten.',
  intro:
    'Die App wird gebaut und online gestellt. Sie bekommen einen Link, den Sie an alle schicken können. Außerdem merkt sich die App Punkte, Nachrichten oder Einträge, damit alle dasselbe sehen. Kein Konto, keine Installation.',
  placeholder: 'Bau mir …',
  working: 'Läuft …',
  shortcut: 'Strg + Enter',
  building: 'Wird gebaut',
  buildIt: 'Bauen',
  builtBy: 'Gebaut von',
  password: 'Passwort',
  fable:
    'Fable ist in der Testphase und durch ein Passwort geschützt. Es hat kein Zeitlimit und arbeitet, bis die App fertig ist – nicht, bis die Zeit abläuft.',
  onlyNew:
    'Hier entstehen nur neue Apps. Um etwas zu ändern, das Sie schon gebaut haben, öffnen Sie seinen Editor-Link und sagen Sie unter „Ändern“, was anders sein soll.',
  ideas: [
    'eine Pinnwand, auf der jeder eine kurze Nachricht hinterlassen kann',
    'eine Highscore-Liste für ein Würfelspiel',
    'eine Umfrage, bei der alle das Ergebnis sehen',
    'ein Gästebuch für meine Hochzeit',
    'einen Countdown bis zu einem Datum, den alle sehen',
  ],
  ahead: (waiting) =>
    waiting === 1 ? 'Noch ein Build vor Ihrem – Sie sind als Nächstes dran.' : `Noch ${waiting} Builds vor Ihrem.`,
  starting: 'Startet …',

  yourApp: 'Ihre App',
  further: 'So geht es weiter',
  keep: 'Heben Sie diesen Link gut auf. Er ist der einzige Weg zurück, und niemand kann ihn wiederherstellen – auch wir nicht. Setzen Sie ein Lesezeichen, bevor Sie den Tab schließen.',
  change:
    'Um sie zu ändern, öffnen Sie den Editor-Link und sagen Sie unter „Ändern“, was anders sein soll – genau wie hier. Das kann auch Ihr eigener Coding-Agent, wie unten beschrieben.',
  copyLink: 'Editor-Link kopieren',
  lifetime: (offline, removed) =>
    `Die App bleibt online, solange sie genutzt wird. Nach ${offline} Tagen ohne Besuche oder Änderungen geht sie offline, nach ${removed} Tagen wird sie gelöscht. Mit „Deployen“ im Editor ist sie wieder online.`,
  openEditor: 'Editor öffnen',
  another: 'Noch etwas bauen',

  keepGoing: 'Mit Ihrem eigenen Agenten weitermachen',
  orOwn: 'Oder Ihren eigenen Agenten nutzen',
  ownText:
    'Hinter dem Feld oben arbeitet ein Claude auf unserem Server. Haben Sie schon einen eigenen Agenten? Dann verbinden Sie ihn stattdessen. Er kann dasselbe – ein Lambda anlegen, den Code schreiben, es online stellen –, nur ohne Tageslimit und ohne Umweg über diese Seite.',
  thenAsk: 'Dann sagen Sie ihm, was Sie möchten – genau wie hier.',
  claudeWeb: 'Claude im Web',
  claudeWebHow:
    'Einstellungen, dann „Connectors“, dann „Add custom connector“. Fügen Sie die Adresse oben als URL des Remote-MCP-Servers ein. Kein Key, kein Login.',
  howToChange:
    'So ändern Sie auch eine fertige App: Geben Sie Ihrem Agenten den Editor-Link und sagen Sie ihm, was er tun soll.',
  more: 'Mehr zum eigenen Agenten',

  failedToStart: 'Die Anfrage kam nicht durch.',
  noAnswer: 'Fertig, aber ohne Rückmeldung, was passiert ist.',
  failed: 'Das hat nicht geklappt.',
};
