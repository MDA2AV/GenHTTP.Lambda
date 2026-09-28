import type { Messages } from '../en';

export const landing: Messages['landing'] = {
  eyebrow: 'Eine Plattform für Agentic Coding',
  headline: 'Beschreiben Sie eine App.',
  headlineAccent: 'Ihr Agent stellt sie online.',
  intro:
    'Umfragen, Gästebücher, Bestenlisten, kleine Shops. Sagen Sie unserem Agenten oder Ihrem eigenen, was Sie brauchen – und Sie bekommen eine fertige App mit Link zum Teilen. Sie können sie jederzeit weiter verbessern, auch lange nach der ersten Version.',
  build: 'App bauen',
  ownAgent: 'Eigenen Agenten nutzen',
  free: 'Kostenlos. Kein Konto, keine Installation.',
  seeIt: 'In Aktion ansehen',

  videoTitle: 'Vom Satz zur laufenden App',
  videoText:
    'Ein privates Browserfenster, kein Konto, ein einziger Prompt. Danach die fertige App, geöffnet über ihren Link – genau so, wie jeder Besucher sie sieht.',
  videoNote: 'Das Bauen läuft im Zeitraffer, alles andere in Echtzeit.',
  tryIt: 'Selbst ausprobieren',

  oneShotTitle: 'Mehr als ein erster Wurf',
  oneShotText:
    'Die meisten Generatoren liefern ein Ergebnis und lassen Sie damit allein. Bei uns läuft die App gleich dort, wo sie entstanden ist. Sie und Ihr Agent können jederzeit daran weiterarbeiten.',
  steps: [
    {
      title: 'Idee beschreiben',
      body: 'Sagen Sie in eigenen Worten, was Sie möchten – dem Agenten auf dieser Seite oder Ihrem eigenen. Kein Code, kein Setup, kein Konto.',
      alt: 'Die Seite „App bauen“ mit einem Prompt für eine Mittagsumfrage',
    },
    {
      title: 'App und Link bekommen',
      body: 'Die App wird gebaut und online gestellt. Sie bekommen eine öffentliche Adresse zum Teilen. Die App speichert ihre Daten – Stimmen, Punkte, Nachrichten –, damit alle denselben Stand sehen.',
      alt: 'Die fertige Mittagsumfrage im Browser',
    },
    {
      title: 'Weiter verbessern',
      body: 'Zu jeder App gehört ein privater Editor-Link. Geben Sie ihn mit dem nächsten Wunsch an Ihren Agenten oder öffnen Sie ihn selbst. Jede Änderung wird eine neue Version, die Adresse bleibt gleich.',
      alt: 'Das Kontrollzentrum der Umfrage: ihre Versionen, jeweils mit Wunsch, Änderung und Diff zur vorherigen',
    },
  ],
  weekLater: 'Eine Woche später',
  weekAsk:
    'Hier ist der Editor-Link für meine Mittagsumfrage. Bitte beende die Abstimmung freitags um 11 Uhr und zeig den Gewinner ganz oben an.',
  weekAnswer:
    'Erledigt. Version 4 ist unter derselben Adresse live. Version 3 lässt sich jederzeit zurückholen.',

  agentsTitle: 'Bringen Sie Ihren Lieblingsagenten mit',
  agentsText:
    'Sie arbeiten schon mit Claude oder einem anderen Assistenten? Verbinden Sie ihn mit dieser Adresse. Dann baut er hier Apps, stellt sie online und ändert sie – direkt aus dem Chat, den Sie ohnehin offen haben.',
  thenAsk: (em) => <>Danach genügt ein Satz: {em('Bau eine Anmeldeliste für unser Team-Event und stell sie online')}.</>,

  contactTitle: 'Sprechen Sie mit uns',
  contactText:
    'Sie brauchen Hilfe, planen etwas Größeres oder suchen eine Lösung nach Maß? Wir freuen uns auf Ihre Nachricht.',
  mailTitle: 'Schreiben Sie uns',
  mailText: 'Für Projekte, Anfragen und alles, was Sie lieber unter vier Augen besprechen.',
  discordTitle: 'Discord beitreten',
  discordText: 'Zeigen Sie, was Sie gebaut haben, holen Sie sich Hilfe und sprechen Sie direkt mit dem Team.',
  discordLink: 'Der GenHTTP-Discord',
};
