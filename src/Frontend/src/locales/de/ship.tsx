import type { Messages } from '../en';

export const ship: Messages['ship'] = {
  title: 'Vom Laptop auf jeden Bildschirm.',
  intro:
    'Sie haben mit Ihrem Coding-Agenten etwas gebaut, aber es läuft nur auf Ihrem Rechner. Bitten Sie den Agenten, es hier zu veröffentlichen. Wenige Minuten später hat es einen öffentlichen Link. Und es merkt sich Dinge – so können Leute darin gemeinsam spielen, chatten und posten.',
  facts: ['Kostenlos', 'Kein Konto', 'Keine Installation'],
  connect: 'Agenten verbinden',
  seeOthers: 'Sehen, was andere gebaut haben',

  stepsTitle: 'Drei Schritte – einer davon ist ein Satz',
  step: (n) => `Schritt ${n}`,
  steps: [
    {
      title: 'Einmal verbinden',
      body: 'Fügen Sie Claude, Cursor oder Ihrem Agenten eine Adresse hinzu. Das dauert keine Minute und ist nur einmal nötig.',
    },
    {
      title: 'Veröffentlichen lassen',
      body: 'Sagen Sie Ihrem Agenten, er soll die App hier online stellen. Er packt sie ein, veröffentlicht sie und prüft, ob sie erreichbar ist.',
    },
    {
      title: 'Link teilen',
      body: 'Sie bekommen eine öffentliche Adresse und einen privaten Editor-Link. Die Adresse schicken Sie an alle. Den Editor-Link behalten Sie – damit ändern Sie die App später.',
    },
  ],

  togetherTitle: 'Mehr als eine Seite. Ein Treffpunkt.',
  together:
    'Bei den meisten Hostern bekommt jeder Besucher seine eigene Kopie der App – und alle spielen allein. Hier hat jede App ein eigenes Gedächtnis und eine Live-Verbindung zu allen, die sie gerade offen haben. Macht jemand einen Zug, sehen ihn alle anderen sofort. Und was gepostet wird, ist morgen noch da.',
  together2:
    'Keine Datenbank, für die Sie sich anmelden müssen, kein zweiter Dienst zum Anbinden. Beschreiben Sie es einfach so, wie Sie es einem Freund erzählen würden.',
  kinds: [
    { name: 'Multiplayer-Spiele', ask: 'Lass bis zu acht Freunde derselben Runde beitreten und die Züge der anderen live sehen.' },
    { name: 'Chaträume', ask: 'Füg einen Raum hinzu, in dem alle mit dem Link chatten können, und speichere die letzten hundert Nachrichten.' },
    { name: 'Gemeinsame Listen', ask: 'Mach aus der Packliste eine, die das ganze Team gleichzeitig bearbeiten kann.' },
    { name: 'Punkte und Rekorde', ask: 'Merk dir die Bestzeit jedes Spielers und zeig die Top 10 auf dem Startbildschirm.' },
    { name: 'Kleine soziale Netzwerke', ask: 'Lass Hochzeitsgäste Fotos auf eine gemeinsame Pinnwand stellen und die der anderen liken.' },
  ],
  quote: (text) => `„${text}“`,

  connectTitle: 'Agenten einmal verbinden',
  connectText:
    'Geben Sie Ihrem Agenten diese Adresse. Ab dann weiß er, wie er hier veröffentlicht – ohne Key und ohne Login.',
  sayLike: 'Dann sagen Sie in Ihrem Projekt zum Beispiel:',
  asks: [
    'Veröffentliche diese App auf GenHTTP Lambda und schick mir den Link.',
    'Speichere die Highscores zentral, damit alle dieselbe Bestenliste sehen.',
  ],

  domainChip: 'Wenn es gut ankommt',
  domainTitle: 'Eine eigene Domain',
  domainText:
    'Dieselbe App, derselbe Editor-Link – aber unter einer Adresse, die Ihnen gehört. Leichter zu sagen, leichter zu merken. Und sie macht etwas her, wenn sich die App herumspricht.',
  domainSubject: 'Eine Domain für meine App',
  domainAsk: 'Domain anfragen',

  questionsTitle: 'Bevor Sie fragen',
  questions: (offline, removed, showcase, terms) => [
    [
      'Ist das wirklich kostenlos?',
      <>
        Ja. Keine Kreditkarte, keine Testphase, kein Konto. Ihre App bleibt online, solange sie genutzt wird. Nach{' '}
        {offline} Tagen ohne Besuch oder Änderung geht sie offline, nach {removed} Tagen wird sie gelöscht.
      </>,
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
  closeFacts: 'Kostenlos. Kein Konto. Keine Installation.',

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
    'Geteilte Live-Daten: Chat, Multiplayer, Rekorde',
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

  yourAgent: 'Ihr Agent',
  terminal: 'Terminal',
  setups: {
    claudeCode: 'Einmal im Terminal ausführen. Danach kann jedes Projekt, das Sie öffnen, hier veröffentlichen.',
    claude: (strong) => (
      <>
        Öffnen Sie in Claude (Web oder Desktop) die {strong('Einstellungen')}, dann {strong('Connectors')}, und wählen Sie{' '}
        {strong('Add custom connector')}. Adresse oben einfügen, speichern, fertig.
      </>
    ),
    cursor: 'Fügen Sie das in die MCP-Einstellungen von Cursor oder in die Datei unten ein und laden Sie neu.',
    vscode: 'Speichern Sie das in Ihrem Projekt und starten Sie den Server in der MCP-Ansicht von Copilot Chat.',
  },
  elsewhere:
    'Sie nutzen etwas anderes? Windsurf, Codex, Zed und die meisten anderen Agenten können in ihren Einstellungen einen Remote-MCP-Server hinzufügen. Nutzen Sie dafür die Adresse oben.',
};
