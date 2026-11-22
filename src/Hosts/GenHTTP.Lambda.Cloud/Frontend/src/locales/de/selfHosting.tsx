import type { Messages } from '../en';

export const selfHosting: Messages['selfHosting'] = {
  menu: 'Self-Hosting',

  eyebrow: 'Open Source · Cloud oder Self-Hosting',
  title: 'Nutzen Sie es hier',
  titleAccent: 'oder hosten Sie es selbst.',
  intro:
    'GenHTTP Lambda ist Open Source unter der MIT-Lizenz. genhttp.dev betreibt es für alle, die es nutzen möchten, und die Community Edition ist dieselbe Plattform als Docker-Image für alle, die sie lieber selbst betreiben. Beide bauen und hosten dieselben Apps; unterschiedlich ist, wo sie laufen und wer sich darum kümmert.',

  cloud: {
    kicker: 'Cloud',
    name: 'genhttp.dev',
    summary: 'Diese Seite öffnen und loslegen. Server, Updates und Limits sind unsere Sache.',
    points: ['Nichts zu installieren oder zu betreiben', 'Apps antworten unter einer Adresse auf genhttp.run', 'Der Build-Agent inklusive'],
    action: 'Etwas bauen',
  },
  community: {
    kicker: 'Self-Hosting',
    name: 'Community Edition',
    summary: 'Ein Docker-Container auf einer Maschine von Ihnen. Server, Updates und Limits sind Ihre Sache.',
    points: ['Ein Container, ein Volume, kein Datenbankserver', 'Apps antworten unter Ihrer Adresse', 'Der Build-Agent als zusätzlicher Container'],
    action: 'Selbst betreiben',
  },

  sameTitle: 'In beiden gleich',
  sameText: 'Die Community Edition ist keine abgespeckte Version. Jede App bekommt dieselben Dinge, wo auch immer sie läuft:',
  same: [
    'Der Build-Agent',
    'Ihr eigener Agent über MCP',
    'Die REST-API',
    'Der Editor, einfach und vollständig',
    'Entwürfe mit Testdaten',
    'Versionen und ein Weg zurück',
    'Einträge, Dateien, Schlüssel und Passwörter',
    'Analyse ohne Cookies',
    'Eigene Domains',
    'Git und Downloads',
    'Der Showcase und veröffentlichter Code',
  ],

  differTitle: 'Was sich unterscheidet',
  aspect: 'Frage',
  rows: (offline) => [
    ['Wo die Apps laufen', 'Auf unserem Server', 'Auf Ihrem, in einem Container mit einem Volume'],
    ['Der Einstieg', 'Die Bauen-Seite öffnen – ohne Konto', 'Das Image starten und seine Adresse öffnen'],
    ['Wer den Server betreut', 'Wir', 'Sie'],
    ['Was es kostet', 'Zum Ausprobieren nichts; Produktion für gemeinnützige Organisationen auf Anfrage, für Unternehmen nach Vereinbarung', 'Nichts; der Server geht auf Ihre Kosten'],
    ['Wo eine App antwortet', 'Unter einer Subdomain von genhttp.run', 'Unterhalb Ihrer Adresse oder unter Subdomains einer Domain, die Sie angeben'],
    ['Eine eigene Domain', 'Für Apps in Produktion, von Ihnen registriert', 'Für jede App, die Sie in Produktion verschieben'],
    ['Apps, die niemand nutzt', `Offline nach ${offline} Tagen ohne Besuch oder Änderung, außer in Produktion`, 'Wie Sie es einstellen'],
    ['Limits und Regeln', 'Unsere, und unsere Nutzungsbedingungen', 'Ihre, eingestellt im Admin-Panel'],
    ['Der Build-Agent', 'Inklusive, mit einer Zahl von Builds pro Tag', 'Optional, mit Ihrem eigenen Claude-Abonnement oder API-Schlüssel'],
    ['Nutzungsberichte', 'Keine', 'Einmal am Tag: die Version, die Maschine, die Optionen und Zahlen über die Apps – nie eine App oder eine Person. Sie können sie abschalten.'],
  ],

  moveTitle: 'Der Wechsel zwischen beiden',
  move:
    'Dieselben Apps laufen auf beiden, und Ihr Agent bringt eine per git hinüber: Er legt die App auf der anderen Seite an, kopiert die Dateien der alten dorthin und pusht sie. Ein Download ist ein .NET-Projekt, das überall läuft, mit seinen Einträgen in einer SQLite-Datei.',

  pickTitle: 'Was zu Ihnen passt',
  pickCloud: {
    title: 'genhttp.dev, wenn',
    points: [
      'Sie eine App online haben möchten, ohne etwas zu betreiben',
      'die Menschen, die sie nutzen, überall sein können',
      'Sie sich lieber nicht um einen Server kümmern möchten',
    ],
  },
  pickCommunity: {
    title: 'Die Community Edition, wenn',
    points: [
      'Ihre Apps und ihre Einträge auf einer Maschine von Ihnen bleiben sollen',
      'es Werkzeuge für Ihr Team im eigenen Netzwerk sind',
      'Sie die Limits und Regeln selbst festlegen möchten',
    ],
  },

  startTitle: 'Der Einstieg',
  startCloud: {
    title: 'Auf genhttp.dev',
    text: 'Beschreiben Sie auf der Bauen-Seite, was Sie brauchen, oder verbinden Sie den Agenten, den Sie schon nutzen. Ein Konto ist nicht anzulegen.',
  },
  startCommunity: {
    title: 'Auf Ihrem Server',
    text: 'Ein Befehl startet sie. Öffnen Sie die Adresse, verbinden Sie Ihren Agenten und bauen Sie – den Rest sagt die README.',
  },
  connect: 'Agent verbinden',
  readme: 'README lesen',
  dockerHub: 'Das Image auf Docker Hub',

  sourceTitle: 'Open Source',
  source: (github) => (
    <>
      GenHTTP Lambda steht unter der MIT-Lizenz. Sein Code, seine Issues und seine Releases liegen auf {github('GitHub')}.
    </>
  ),
};
