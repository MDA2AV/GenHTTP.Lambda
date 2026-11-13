import type { Messages } from '../en';

export const landing: Messages['landing'] = {
  eyebrow: 'GenHTTP Lambda · Community Edition',
  headline: 'Die Plattform, auf der Ihre Agents Apps bauen.',
  headlineAccent: 'Open Source, auf einem eigenen Server.',
  intro:
    'Beschreiben Sie eine App, und der Build-Agent schreibt sie – oder lassen Sie Claude Code, Codex oder Cursor sie über MCP bauen. Jede App ist ein C#-GenHTTP-Handler, wird auf diesem Server kompiliert und ist in Sekunden online, mit Datenbank, Schlüsseln und Passwörtern, Versionen und Git.',
  describe: 'App beschreiben',
  connect: 'Agent verbinden',
  write: 'Code selbst schreiben',

  buildTitle: 'Beschreiben Sie sie',
  buildText: 'Ein Satz genügt. Der Build-Agent schreibt die App, stellt sie online und gibt Ihnen den Link, mit dem Sie sie später ändern können.',
  noAgent: 'Mit dieser Installation ist kein Build-Agent verbunden. Verbinden Sie unten Ihren eigenen Agent – er leistet dasselbe.',

  connectTitle: 'Verbinden Sie Ihren Agent',
  connectText:
    'Ihr Coding-Agent erhält die Plattform als Werkzeuge: eine App erstellen, ihren Code schreiben und prüfen, eine Änderung in einem Entwurf mit einer Kopie der Daten ausprobieren, sie online stellen und ihre Logs lesen.',
  connectThen: 'Bitten Sie ihn dann um etwas, zum Beispiel: „Baue ein Gästebuch mit einer Seite, auf der ich die Einträge freigebe.“',

  featuresTitle: 'Was jede App bekommt',
  features: [
    { title: 'Eine Adresse', text: 'Eine eigene Adresse auf diesem Server, benannt nach ihrem Schlüssel – und eine eigene Domain, sobald sie Premium ist.' },
    { title: 'Einträge', text: 'Eine eigene SQLite-Datenbank, ihre Tabellen durch Migrationen angelegt, gelesen und geschrieben mit Entity Framework Core.' },
    { title: 'Dateien', text: 'Ein Ordner für alles, was die App speichert: Uploads, Exporte, alles, was kein Eintrag ist.' },
    { title: 'Schlüssel und Passwörter', text: 'Verschlüsselt auf dem Server abgelegt. Der Agent nennt, was er braucht, Sie geben sie ein, und niemand liest sie wieder aus.' },
    { title: 'Entwürfe und Versionen', text: 'Eine Änderung wird in einem Entwurf mit einer Kopie der Daten ausprobiert und dann online gestellt. Frühere Versionen bleiben erhalten, ein Klick führt zu ihnen zurück.' },
    { title: 'Git', text: 'Jede App ist ein Repository: Klonen Sie es, pushen Sie einen Commit, um eine Version zu speichern – mit -o deploy stellen Sie sie online –, oder einen Branch, um einen Entwurf zu starten.' },
    { title: 'Besucher', text: 'Seitenaufrufe, Herkunft der Besucher und ihre Länder – auf dem Server gezählt, ohne Cookies, sobald Sie es einschalten.' },
    { title: 'Ihr Code', text: 'Laden Sie jede App als einfaches .NET-Projekt herunter und betreiben Sie sie, wo Sie möchten, oder veröffentlichen Sie ihren Quellcode zum Lesen für andere.' },
  ],

  runsTitle: 'Wie es läuft',
  runs: [
    { title: 'Ein Prozess', text: 'Der Server, der Editor, die API und alle Apps laufen in einem Prozess und speichern alles in einem Ordner. Der Build-Agent läuft, wo es einen gibt, daneben.' },
    {
      title: 'Nach Pfad oder nach Host',
      text: 'Apps antworten unter /lambda/ auf der Adresse der Installation – oder, wenn Sie ihr eine Domain für sie geben, jede unter einer eigenen Subdomain.',
    },
    {
      title: 'Code, dem Sie vertrauen',
      text: 'Apps werden in den Server hinein kompiliert, und ihr Code wird darauf geprüft, was er nicht tun darf – aber sie teilen sich seinen Prozess. Betreiben Sie nur, was Sie auch selbst betreiben würden.',
    },
    { title: 'Open Source', text: 'Unter MIT-Lizenz: Lesen Sie es, ändern Sie es, betreiben Sie es, wo Sie möchten.' },
  ],
  repository: 'Der Quellcode auf GitHub',

  aroundTitle: 'Umsehen',
  around: {
    demos: { title: 'Mit einer Demo beginnen', text: 'Eine API über Einträge, ein Login, Live-Updates, ein Spiel: jeweils eine fertige App zum Kopieren und Ändern.' },
    showcase: { title: 'Showcase', text: 'Die Apps, die ihre Besitzer hier zeigen möchten.' },
    guide: { title: 'Anleitung', text: 'Was eine App kann und wie, in der Reihenfolge, in der Sie es brauchen.' },
    api: { title: 'REST-API', text: 'Dasselbe wie die Werkzeuge, über HTTP, beschrieben in OpenAPI.' },
    sources: { title: 'Veröffentlichte Quellcodes', text: 'Apps, deren Besitzer ihren Code veröffentlicht haben, zum Lesen und Herunterladen.' },
    admin: { title: 'Administration', text: 'Die Apps, die Limits, die Sperren und das Log, hinter dem Admin-Token.' },
  },
};
