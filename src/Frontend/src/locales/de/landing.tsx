import type { Messages } from '../en';

export const landing: Messages['landing'] = {
  eyebrow: 'Eine Plattform für Agentic Coding',
  headline: 'Beschreiben Sie eine Anwendung.',
  headlineAccent: 'Ihr Agent stellt sie online.',
  intro:
    'Umfragen, Gästebücher, Bestenlisten, kleine Shops: Beschreiben Sie unserem Agenten oder Ihrem eigenen, was Sie benötigen, und Sie erhalten eine funktionsfähige Anwendung mit einem Link zum Teilen. Die Anwendung bleibt bearbeitbar und lässt sich auch nach der ersten Version jederzeit weiterentwickeln.',
  build: 'Anwendung erstellen',
  ownAgent: 'Eigenen Agenten verwenden',
  free: 'Kostenlos. Ohne Konto und ohne Installation.',
  seeIt: 'So funktioniert es',

  videoTitle: 'Von der Beschreibung zur laufenden Anwendung',
  videoText:
    'Ein privates Browserfenster, kein Konto und eine einzige Anfrage auf der Seite „Erstellen“ – anschließend die fertige Anwendung, aufgerufen über ihren Link, so wie jede andere Person sie sieht.',
  videoNote: 'Der Erstellungsvorgang ist beschleunigt dargestellt, alles Weitere in Echtzeit.',
  tryIt: 'Selbst ausprobieren',

  oneShotTitle: 'Nicht nur ein einmaliges Ergebnis',
  oneShotText:
    'Die meisten Generatoren liefern ein Ergebnis und enden dort. Hier läuft die Anwendung dort weiter, wo sie erstellt wurde – Sie und Ihr Agent können sie also fortlaufend weiterentwickeln.',
  steps: [
    {
      title: 'Anforderung beschreiben',
      body: 'Beschreiben Sie in eigenen Worten, was Sie benötigen – dem Agenten auf dieser Website oder dem, den Sie bereits verwenden. Ohne Code, ohne Einrichtung, ohne Konto.',
      alt: 'Die Seite „Erstellen“ mit einer eingegebenen Anfrage für eine Mittagsumfrage',
    },
    {
      title: 'Funktionsfähige Anwendung mit Link',
      body: 'Die Anwendung wird erstellt, bereitgestellt und als öffentliche Adresse zurückgegeben, die Sie teilen können. Sie speichert ihre Daten – Stimmen, Punktestände, Nachrichten –, sodass alle Nutzer denselben Stand sehen.',
      alt: 'Die fertige Mittagsumfrage, geöffnet im Browser',
    },
    {
      title: 'Kontinuierlich verbessern',
      body: 'Zu jeder Anwendung gehört ein privater Editor-Link. Übergeben Sie ihn Ihrem Agenten mit der nächsten Änderung oder öffnen Sie ihn selbst. Jede Änderung wird zu einer neuen Version, die Adresse bleibt unverändert.',
      alt: 'Das Kontrollzentrum der Umfrage: ihre Versionen, jeweils mit der Anforderung, der Änderung und dem Unterschied zur vorherigen Version',
    },
  ],
  weekLater: 'Eine Woche später',
  weekAsk:
    'Hier ist der Editor-Link meiner Mittagsumfrage. Bitte beende die Abstimmung freitags um 11 Uhr und zeige das Ergebnis oben an.',
  weekAnswer:
    'Erledigt. Version 4 ist unter derselben Adresse online, Version 3 steht für eine Wiederherstellung weiterhin zur Verfügung.',

  agentsTitle: 'Ihren bevorzugten Agenten verwenden',
  agentsText:
    'Sie arbeiten bereits mit Claude oder einem anderen Assistenten? Verbinden Sie ihn mit dieser Adresse, und er kann hier Anwendungen erstellen, bereitstellen und aktualisieren – direkt aus der laufenden Unterhaltung.',
  agents: [
    {
      name: 'Claude im Web oder auf dem Desktop',
      how: 'Öffnen Sie die Einstellungen, dann „Connectors“, und wählen Sie „Add custom connector“. Fügen Sie die obige Adresse ein – ohne API-Schlüssel und ohne Anmeldung.',
    },
    {
      name: 'Claude Code',
      how: 'Führen Sie einmalig in einem Terminal aus:',
    },
    {
      name: 'Andere MCP-Clients',
      how: 'Cursor, VS Code, Codex und weitere MCP-Clients unterstützen entfernte Server. Konfigurieren Sie sie mit derselben Adresse.',
    },
  ],
  thenAsk: (em) => (
    <>Anschließend genügt eine Anfrage wie: {em('Erstelle eine Anmeldeliste für unser Team-Event und stelle sie online')}.</>
  ),

  contactTitle: 'Kontakt',
  contactText:
    'Sie benötigen Unterstützung, planen ein größeres Vorhaben oder suchen eine individuell entwickelte Lösung? Wir freuen uns auf Ihre Nachricht.',
  mailTitle: 'Per E-Mail',
  mailText: 'Für Projekte, Anfragen und alle Anliegen, die Sie vertraulich besprechen möchten.',
  discordTitle: 'Discord-Community',
  discordText: 'Stellen Sie Ihre Projekte vor, erhalten Sie Unterstützung und tauschen Sie sich direkt mit dem Team aus.',
  discordLink: 'Der GenHTTP-Discord',

  terms: 'Nutzungsbedingungen',
  writeCode: 'Code selbst schreiben',
  contact: 'Kontakt',
};
