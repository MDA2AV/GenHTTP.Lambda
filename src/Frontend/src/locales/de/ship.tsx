import type { Messages } from '../en';

export const ship: Messages['ship'] = {
  title: 'Vom eigenen Rechner ins Netz.',
  intro:
    'Sie haben mit Ihrem Coding-Agenten eine Anwendung erstellt, die bislang nur lokal läuft. Bitten Sie den Agenten, sie hier zu veröffentlichen. Wenige Minuten später ist sie über einen öffentlichen Link erreichbar und kann Daten speichern – mehrere Personen können sie also gemeinsam nutzen, darin spielen, chatten und Beiträge verfassen.',
  facts: ['Kostenlos', 'Ohne Konto', 'Ohne Installation'],
  connect: 'Agenten verbinden',
  seeOthers: 'Veröffentlichte Anwendungen ansehen',

  stepsTitle: 'Drei Schritte, einer davon ist ein einziger Satz',
  step: (n) => `Schritt ${n}`,
  steps: [
    {
      title: 'Einmalig verbinden',
      body: 'Fügen Sie Claude, Cursor oder Ihrem bevorzugten Agenten eine Adresse hinzu. Das dauert weniger als eine Minute und ist nur einmal erforderlich.',
    },
    {
      title: 'Veröffentlichung anfordern',
      body: 'Bitten Sie den Agenten, die Anwendung hier online zu stellen. Er verpackt sie, veröffentlicht sie und prüft, ob sie erreichbar ist.',
    },
    {
      title: 'Link teilen',
      body: 'Sie erhalten eine öffentliche Adresse und einen privaten Editor-Link. Die Adresse geben Sie weiter. Den Editor-Link bewahren Sie auf – mit ihm ändern Sie die Anwendung später.',
    },
  ],

  togetherTitle: 'Mehr als eine Webseite: ein gemeinsamer Ort.',
  together:
    'Die meisten Hosting-Angebote liefern jedem Besucher eine eigene Kopie der Anwendung aus – jede Person nutzt sie für sich allein. Hier verfügt jede Anwendung über einen eigenen Speicher und eine Live-Verbindung zu allen, die sie gerade geöffnet haben. Eine Aktion erscheint sofort bei allen anderen, und Beiträge bleiben dauerhaft erhalten.',
  together2:
    'Sie benötigen weder eine separate Datenbank noch einen zusätzlichen Dienst. Beschreiben Sie die gewünschte Funktion einfach in eigenen Worten.',
  kinds: [
    { name: 'Mehrspieler-Spiele', ask: 'Bis zu acht Personen sollen derselben Runde beitreten und die Züge der anderen live sehen können.' },
    { name: 'Chaträume', ask: 'Füge einen Raum hinzu, in dem alle mit dem Link schreiben können, und speichere die letzten hundert Nachrichten.' },
    { name: 'Gemeinsame Listen', ask: 'Mach aus der Packliste eine Liste, die das gesamte Team gleichzeitig bearbeiten kann.' },
    { name: 'Punktestände und Rekorde', ask: 'Führe eine Bestenliste mit der besten Zeit jeder Person und zeige die zehn besten auf dem Startbildschirm.' },
    { name: 'Kleine Communitys', ask: 'Hochzeitsgäste sollen Fotos auf einer gemeinsamen Pinnwand teilen und bewerten können.' },
  ],
  quote: (text) => `„${text}“`,

  connectTitle: 'Agenten einmalig verbinden',
  connectText:
    'Übergeben Sie Ihrem Agenten diese Adresse. Danach kann er hier veröffentlichen – ohne Schlüssel und ohne Anmeldung.',
  sayLike: 'Anschließend genügt in Ihrem Projekt eine Anfrage wie',
  asks: [
    'Veröffentliche diese Anwendung auf GenHTTP Lambda und sende mir den Link.',
    'Speichere die Highscores zentral, sodass alle dieselbe Bestenliste sehen.',
  ],

  domainChip: 'Für wachsende Anwendungen',
  domainTitle: 'Eine eigene Domain',
  domainText:
    'Dieselbe Anwendung und derselbe Editor-Link, jedoch unter einer Adresse, die Ihnen gehört – einprägsamer und professioneller, sobald die Anwendung weiterempfohlen wird.',
  domainSubject: 'Eigene Domain für meine Anwendung',
  domainAsk: 'Eigene Domain anfragen',

  questionsTitle: 'Häufige Fragen',
  questions: (offline, removed, showcase, terms) => [
    [
      'Ist die Nutzung tatsächlich kostenlos?',
      <>
        Ja. Keine Kreditkarte, keine Testphase, kein Konto. Ihre Anwendung bleibt online, solange sie genutzt wird. Nach{' '}
        {offline} Tagen ohne Aufruf oder Änderung wird sie offline genommen, nach {removed} Tagen entfernt.
      </>,
    ],
    [
      'Muss meine Anwendung auf eine bestimmte Weise aufgebaut sein?',
      'Nein, darum kümmert sich Ihr Agent. Seiten, Bilder und Stylesheets werden unverändert übernommen, und serverseitige Logik passt der Agent an diese Plattform an. Sie beschreiben die gewünschte Funktion, die Umsetzung übernimmt der Agent.',
    ],
    [
      'Wie ändere ich die Anwendung später?',
      'Mit dem Editor-Link, den Sie bei der Veröffentlichung erhalten haben. Übergeben Sie ihn Ihrem Agenten zusammen mit der gewünschten Änderung oder öffnen Sie ihn im Browser. Jede Änderung wird als neue Version unter derselben Adresse gespeichert, und frühere Versionen lassen sich jederzeit wiederherstellen.',
    ],
    [
      'Wer kann meine Anwendung sehen?',
      <>
        Alle Personen, denen Sie den Link geben. Die Anwendung wird nirgends aufgeführt, es sei denn, Sie nehmen sie selbst
        in den {showcase('Showcase')} auf.
      </>,
    ],
    [
      'Gibt es Inhalte, die nicht veröffentlicht werden dürfen?',
      <>
        Ja, etwa alles, was Menschen schadet oder sie täuscht. Die {terms('Nutzungsbedingungen')} sind kurz und
        verständlich formuliert.
      </>,
    ],
  ],

  closeTitle: 'Lokal funktioniert es bereits.',
  closeAccent: 'Jetzt auch für alle anderen.',
  noAgent: 'Ohne Agent? Hier erstellen',
  closeFacts: 'Kostenlos. Ohne Konto. Ohne Installation.',

  scene: {
    label:
      'Ein Agent wird gebeten, eine Anwendung zu veröffentlichen. Die Adresse wechselt von localhost zu einem öffentlichen Link, und weitere Personen treten bei.',
    ask: 'Stelle mein Quizspiel online, damit meine Freunde mitspielen können.',
    live: 'Die Anwendung ist online. Hier ist Ihr Link.',
    publishing: 'Wird veröffentlicht …',
    public: 'Öffentlich',
    onlyYou: 'Nur lokal',
    app: 'Quizabend am Freitag',
    playing: 'aktiv',
    you: 'Sie',
  },

  compareTitle: 'Der kürzeste Weg von „funktioniert“ zu „verfügbar“',
  compareText:
    'Vercel, Cloudflare und Lovable sind hervorragende Plattformen für den Betrieb von Anwendungen. Sie setzen jedoch eine Registrierung voraus und, sobald Daten zwischen Besuchern geteilt werden sollen, einen zusätzlichen Dienst, der eingerichtet werden muss. Die Übersicht zeigt den Aufwand bei einem Start ohne bestehendes Konto.',
  rows: [
    'Start ohne Konto',
    'Veröffentlichung aus dem bereits genutzten Agenten',
    'Geteilte Live-Daten: Chat, Mehrspieler, Rekorde',
    'Kosten bis zum ersten Link',
  ],
  us: ['Ja', 'Einmal verbinden, dann anfragen', 'In jeder Anwendung enthalten', 'Kostenlos'],
  rivals: [
    ['Registrierung erforderlich', 'Nach Anmeldung in eigenen Tools', 'Zusätzlicher Datenbankdienst', 'Kostenloser Tarif'],
    ['Registrierung erforderlich', 'Nach Anmeldung in eigenen Tools', 'Möglich, mit Einrichtung', 'Kostenloser Tarif'],
    ['Registrierung erforderlich', 'Im eigenen Editor erstellt', 'Über ein angebundenes Backend', 'Kostenloser Tarif, begrenzte Credits'],
  ],
  compareNote:
    'Stand: September 2026, für eine Person ohne bestehendes Konto. Tarife und Funktionen anderer Anbieter können sich ändern; maßgeblich sind deren eigene Angaben.',

  yourAgent: 'Ihr Agent',
  terminal: 'Terminal',
  setups: {
    claudeCode: 'Führen Sie diesen Befehl einmalig in einem Terminal aus. Jedes danach geöffnete Projekt kann hier veröffentlichen.',
    claude: (strong) => (
      <>
        Öffnen Sie in Claude im Web oder auf dem Desktop die {strong('Einstellungen')}, dann {strong('Connectors')}, und
        wählen Sie {strong('Add custom connector')}. Fügen Sie die obige Adresse ein und speichern Sie. Weitere Schritte
        sind nicht erforderlich.
      </>
    ),
    cursor: 'Fügen Sie diesen Eintrag den MCP-Einstellungen von Cursor oder der unten genannten Datei hinzu und laden Sie neu.',
    vscode: 'Speichern Sie diese Datei in Ihrem Projekt und starten Sie den Server anschließend in der MCP-Ansicht von Copilot Chat.',
  },
  elsewhere:
    'Sie verwenden ein anderes Werkzeug? Windsurf, Codex, Zed und die meisten weiteren Agenten können in ihren Einstellungen einen entfernten MCP-Server einbinden. Verwenden Sie dafür die obige Adresse.',
};
