import type { Messages } from '../en';

export const ship: Messages['ship'] = {
  eyebrow: 'Kostenloses Hosting für Vibe-Coding-Apps',
  title: 'Von localhost auf jeden Bildschirm.',
  intro:
    'Sie haben mit Claude Code, Codex oder Cursor eine App gebaut, aber sie läuft nur auf Ihrem Rechner. Bitten Sie Ihren Agenten, sie hier online zu stellen. Wenige Minuten später hat sie einen öffentlichen Link, eine eigene Datenbank und eine Live-Verbindung zu allen, die sie gerade offen haben – so können Leute darin gemeinsam spielen, chatten und posten.',
  facts: ['Kostenlos', 'Ohne Anmeldung', 'Ohne Kreditkarte', 'Ohne Installation'],
  connect: 'Agenten verbinden',
  seeOthers: 'Sehen, was andere gebaut haben',

  stepsTitle: 'Web-App in drei Schritten online stellen',
  step: (n) => `Schritt ${n}`,
  steps: [
    {
      title: 'Einmal verbinden',
      body: 'Fügen Sie Claude Code, Codex, Cursor oder Ihrem Agenten eine Adresse hinzu, einen Remote-MCP-Server. Das dauert keine Minute und ist nur einmal nötig.',
    },
    {
      title: 'Veröffentlichen lassen',
      body: 'Sagen Sie Ihrem Agenten, er soll die App hier online stellen. Er packt sie ein, deployt sie und prüft, ob sie erreichbar ist – ohne GitHub-Repo, ohne Deploy-Pipeline, ohne Docker.',
    },
    {
      title: 'Link teilen',
      body: 'Sie bekommen eine öffentliche Adresse und einen privaten Editor-Link. Die Adresse schicken Sie an alle. Den Editor-Link behalten Sie – damit ändern Sie die App später.',
    },
  ],

  togetherTitle: 'Mehr als Hosting. Datenbank und Multiplayer inklusive.',
  together:
    'Bei den meisten Hostern bekommt jeder Besucher seine eigene Kopie der App – und alle spielen allein: Was ein Browser im localStorage ablegt, sieht der nächste nie. Hier hat jede App eine eigene Datenbank und eine Live-Verbindung zu allen, die sie gerade offen haben. Macht jemand einen Zug, sehen ihn alle anderen sofort. Und was gepostet wird, ist morgen noch da.',
  together2:
    'Keine Anmeldung bei Supabase oder Firebase, kein Backend zum Anbinden, kein Server zum Mieten. Beschreiben Sie es einfach so, wie Sie es einem Freund erzählen würden.',
  kinds: [
    { name: 'Multiplayer-Spiele', ask: 'Lass bis zu acht Freunde derselben Runde beitreten und die Züge der anderen live sehen.' },
    { name: 'Chaträume', ask: 'Füg einen Raum hinzu, in dem alle mit dem Link chatten können, und speichere die letzten hundert Nachrichten.' },
    { name: 'Gemeinsame Listen', ask: 'Mach aus der Packliste eine, die das ganze Team gleichzeitig bearbeiten kann.' },
    { name: 'Bestenlisten', ask: 'Merk dir die Bestzeit jedes Spielers und zeig die Top 10 auf dem Startbildschirm.' },
    { name: 'Kleine soziale Netzwerke', ask: 'Lass Hochzeitsgäste Fotos auf eine gemeinsame Pinnwand stellen und die der anderen liken.' },
  ],
  quote: (text) => `„${text}“`,

  connectTitle: 'Claude Code, Codex oder Cursor einmal verbinden',
  connectText:
    'Geben Sie Ihrem Agenten diese Adresse unseres MCP-Servers. Ab dann weiß er, wie er hier veröffentlicht – ohne Key und ohne Login.',
  sayLike: 'Dann sagen Sie in Ihrem Projekt zum Beispiel:',
  asks: [
    'Veröffentliche diese App auf GenHTTP Lambda und schick mir den Link.',
    'Speichere die Highscores zentral, damit alle dieselbe Bestenliste sehen.',
  ],

  domainChip: 'Wenn es gut ankommt',
  domainTitle: 'Eine eigene Domain für Ihre App',
  domainText:
    'Dieselbe App, derselbe Editor-Link – aber unter einer Adresse, die Ihnen gehört. Leichter zu sagen, leichter zu merken. Und sie macht etwas her, wenn sich die App herumspricht.',
  domainSubject: 'Eine Domain für meine App',
  domainAsk: 'Domain anfragen',

  questionsTitle: 'Bevor Sie Ihre App online stellen',
  questions: (offline, removed, showcase, terms) => [
    [
      'Ist das wirklich kostenlos?',
      <>
        Ja. Ohne Anmeldung, ohne Kreditkarte, ohne Testphase. Ihre App bleibt online, solange sie genutzt wird. Nach{' '}
        {offline} Tagen ohne Besuch oder Änderung geht sie offline, nach {removed} Tagen wird sie gelöscht.
      </>,
    ],
    [
      'Können Claude Code, Codex oder Cursor meine App hier online stellen?',
      'Ja, und jeder andere Agent, der einen Remote-MCP-Server einbinden kann. Verbinden Sie ihn einmal mit der Adresse oben und bitten Sie ihn, die App zu veröffentlichen: Er deployt sie, prüft, ob sie erreichbar ist, und schickt Ihnen den Link.',
    ],
    [
      'Warum können meine Freunde meinen localhost-Link nicht öffnen?',
      'Weil localhost Ihr eigener Rechner ist: Die Adresse funktioniert nur dort und nur, solange die App läuft. Ein Tunnel leiht ihr eine öffentliche Adresse, solange Ihr Laptop an ist. Hier veröffentlicht, läuft die App auf unseren Servern, und der Link funktioniert auch bei zugeklapptem Laptop.',
    ],
    [
      'Brauche ich einen Server, ein Backend oder Supabase?',
      'Nein. Jede App bekommt eine eigene Datenbank, einen Dateispeicher und eine Live-Verbindung zu allen, die sie gerade offen haben. Sie mieten keinen Server und richten keinen zweiten Dienst ein – und auf Ihrer Seite muss auch nichts weiterlaufen.',
    ],
    [
      'Kann ich mein Spiel ohne eigenen Server multiplayerfähig machen?',
      'Ja. Was ein Browser im localStorage ablegt, sieht der nächste nie, also muss der gemeinsame Teil auf einem Server liegen – hier auf unserem. Bitten Sie Ihren Agenten, das Spiel multiplayerfähig zu machen, und jeder Zug erreicht alle, die es gerade offen haben.',
    ],
    [
      'Muss meine App auf eine bestimmte Art gebaut sein?',
      'Nein, darum kümmert sich Ihr Agent. Seiten, Bilder und Styles werden übernommen, wie sie sind. Was auf dem Server laufen muss, passt der Agent an diese Plattform an. Sie beschreiben, was die App tun soll – den Rest übersetzt er.',
    ],
    [
      'Wie ändere ich die App später?',
      'Mit dem Editor-Link, den Sie beim Veröffentlichen bekommen haben. Geben Sie ihn mit dem nächsten Wunsch an Ihren Agenten oder öffnen Sie ihn im Browser. Jede Änderung wird eine neue Version unter derselben Adresse. Ältere Versionen können Sie jederzeit zurückholen.',
    ],
    [
      'Kann ich per git pushen?',
      'Ja. Jede App ist zugleich ein git-Repository: Klonen Sie es über die Adresse unter „Klonen“ im Editor, ändern Sie es mit Ihren eigenen Werkzeugen oder Ihrem Agenten und pushen Sie. Jeder Commit auf main wird die nächste Version, und ein gepushter Branch wird ein Entwurf mit eigener Vorschau.',
    ],
    [
      'Wohin gehören meine API-Keys?',
      'Nicht in den Code. Ihr Agent nennt nur den Namen des Keys, den Wert tragen Sie selbst im Editor ein. Niemand liest ihn wieder aus – weder der Editor noch der Agent.',
    ],
    [
      'Kann ich meinen Code mitnehmen?',
      'Ja, er gehört Ihnen. Laden Sie ihn jederzeit im Editor herunter – als Projekt, das eigenständig läuft, samt Datenbank – oder klonen Sie ihn mit git, jede Version inklusive.',
    ],
    [
      'Wer kann meine App sehen?',
      <>
        Alle, denen Sie den Link geben. Die App wird nirgends gelistet – außer Sie nehmen sie selbst in den{' '}
        {showcase('Showcase')} auf.
      </>,
    ],
    [
      'Gibt es etwas, das ich nicht veröffentlichen darf?',
      <>
        Ja, ein paar Dinge – etwa alles, was Menschen schadet oder sie täuscht. Die {terms('Nutzungsbedingungen')} sind
        kurz und klar formuliert.
      </>,
    ],
  ],

  closeTitle: 'Auf Ihrem Rechner läuft es schon.',
  closeAccent: 'Jetzt auch bei allen anderen.',
  noAgent: 'Kein Agent? Hier bauen',
  closeFacts: 'Kostenlos. Ohne Anmeldung. Ohne Installation.',

  scene: {
    label:
      'Ein Agent soll eine App veröffentlichen. Die Adresse wechselt von localhost zu einem öffentlichen Link, und Leute kommen dazu.',
    ask: 'Stell mein Quizspiel online, damit meine Freunde mitspielen können.',
    live: 'Ist online. Hier ist der Link.',
    publishing: 'Wird veröffentlicht …',
    public: 'Öffentlich',
    onlyYou: 'Nur Sie',
    app: 'Quizabend am Freitag',
    playing: (count) => <>{count} dabei</>,
    you: 'Sie',
  },

  compareTitle: 'Der kürzeste Weg von „läuft“ zu „probier’s aus“',
  compareText:
    'Vercel, Cloudflare und Lovable sind großartige Plattformen. Aber dort beginnt alles mit einer Registrierung. Und sobald Ihre App Daten zwischen Besuchern teilen soll, müssen Sie einen zweiten Dienst einrichten. So sieht es aus, wenn Sie bei null anfangen.',
  rows: [
    'Start ohne Konto',
    'Veröffentlichen aus Ihrem Agenten',
    'Datenbank und Live-Daten: Chat, Multiplayer, Rekorde',
    'Kosten bis zum ersten Link',
  ],
  us: ['Ja', 'Einmal verbinden, dann fragen', 'In jede App eingebaut', 'Kostenlos'],
  rivals: [
    ['Registrierung nötig', 'Nach Login in eigenen Tools', 'Datenbankdienst dazubuchen', 'Free-Tarif'],
    ['Registrierung nötig', 'Nach Login in eigenen Tools', 'Möglich, mit Einrichtung', 'Free-Tarif'],
    ['Registrierung nötig', 'Im eigenen Editor gebaut', 'Über ein angebundenes Backend', 'Free-Tarif, begrenzte Credits'],
  ],
  compareNote:
    'Stand: September 2026, für jemanden ohne Konto bei einem der Anbieter. Tarife und Funktionen anderer Dienste ändern sich – Details bitte dort prüfen.',

};
