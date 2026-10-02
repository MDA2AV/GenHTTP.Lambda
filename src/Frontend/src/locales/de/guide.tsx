import type { Messages } from '../en';

export const guide: Messages['guide'] = {
  title: 'So funktioniert es',
  intro:
    'Sie schreiben ein C#-Snippet. Was es zurückgibt, ist in wenigen Sekunden unter einer öffentlichen HTTPS-Adresse erreichbar. Hier steht alles Wichtige – in der Reihenfolge, in der Sie es brauchen.',
  contents: 'Inhalt',

  parts: {
    what: 'Was ein Lambda ist',
    first: 'Ihr erstes Lambda',
    editor: 'Das Kontrollzentrum',
    why: 'Das Warum festhalten',
    written: 'Dokumentation und Tests',
    features: 'Gefahrlos ändern',
    files: 'Mehrere Dateien',
    page: 'Eine Seite ausliefern',
    spa: 'Ein Frontend, Schritt für Schritt',
    storage: 'Zwei Orte für Dateien',
    database: 'Datensätze speichern',
    keeping: 'Dateien speichern',
    secrets: 'Schlüssel und Passwörter',
    sockets: 'Websockets',
    limits: 'Was nicht erlaubt ist',
    away: 'Alles mitnehmen',
    open: 'Den Code veröffentlichen',
    agents: 'Mit einem Agenten arbeiten',
  },

  what: [
    (k) => (
      <>
        Ein Lambda ist ein Snippet, das einen GenHTTP-Handler zurückgibt. Die Plattform kompiliert es, lädt es und hängt
        das Ergebnis unter Ihrer eigenen Adresse ein. Kein Projekt, keine Build-Datei, keine {k.code('using')}
        -Anweisungen: Alle GenHTTP-Module sind schon importiert.
      </>
    ),
    (k) => (
      <>
        Das ist bereits ein vollständiges Lambda. Unter {k.code('/lambda/your-key/')} deployt, beantwortet es jeden
        Request mit „hello“.
      </>
    ),
  ],
  whatAside: (k) => (
    <>
      Das Snippet besteht aus {k.em('Anweisungen')}, nicht aus einer Klasse. Zum Schluss gibt es etwas zurück, das
      Requests beantworten kann: einen Handler oder einen Builder dafür.
    </>
  ),

  first: [
    (k) => (
      <>
        Klicken Sie auf {k.b('Lambda erstellen')}. Sie bekommen eine öffentliche Adresse und einen Editor-Schlüssel. Der
        Schlüssel ist der einzige Weg zurück – heben Sie ihn auf. Niemand kann ihn für Sie wiederherstellen.
      </>
    ),
    () => (
      <>
        Sie landen im Kontrollzentrum. Als erste Version ist schon ein kleiner REST-Dienst angelegt – nur als Startpunkt.
      </>
    ),
    (k) => (
      <>
        Geben Sie den Editor-Schlüssel einem Agenten und sagen Sie ihm, was er bauen soll – er schreibt über{' '}
        {k.link('/#agents', 'MCP')} neue Versionen. Oder öffnen Sie {k.b('Code')} und schreiben Sie selbst:{' '}
        {k.b('Prüfen')} kompiliert, ohne etwas zu speichern, und zeigt die Meldungen des Compilers mit Datei und Zeile.
      </>
    ),
    (k) => (
      <>
        Klicken Sie auf {k.b('Deployen')}. Jetzt ist es online – vorher ist nichts erreichbar. Jedes weitere Deployment
        verlängert, wie lange es online bleibt.
      </>
    ),
  ],

  editor: (k) => (
    <>
      Der Editor-Link öffnet ein Kontrollzentrum, kein Textfeld. Den meisten Code schreiben hier Agenten, also sehen Sie
      zuerst, wie es Ihrem Lambda geht. Die Seitenleiste zeigt, ob das Lambda online ist, seine Adresse und seine
      Bereiche. Wartet eine neuere Version darauf, online zu gehen, erscheint dort ein Button. Was Sie selten
      brauchen, etwa die Adresse ändern oder das Lambda löschen, steckt im Menü {k.b('⋯')}.
    </>
  ),
  bits: [
    ['Übersicht', () => <>Was die App ist, ob sie online ist, wie viele Requests sie heute hatte und wie viele davon fehlschlugen, die letzte Änderung und wie viel Platz noch frei ist.</>],
    ['Dokumentation', () => <>Was die App ist, für wen sie gedacht ist und warum, und warum sie so gebaut ist, wie sie ist – von Agenten geschrieben, mit jeder Version gespeichert.</>],
    [
      'Ändern',
      (k) => (
        <>
          Sagen Sie, was anders sein soll, und der Agent auf diesem Server setzt es um, während Sie zusehen. Er probiert die
          Änderung in einem Entwurf aus – einer Kopie mit eigener Adresse – und stellt sie online, sobald sie funktioniert.
          Schalten Sie {k.b('Nach Abschluss online stellen')} aus, um den Entwurf zuerst selbst auszuprobieren.
          Er arbeitet nur an Ihrer App: Eine Bitte, die nichts mit ihr zu tun hat oder Schaden anrichten soll, lehnt er ab und sagt, warum.
        </>
      ),
    ],
    ['Entwürfe', () => <>Änderungen, die ausprobiert werden, bevor sie online gehen, jede unter einer eigenen Adresse und mit eigenen Testdaten. Geöffnet hat ein Entwurf eigenen Code, eigene Testdaten und eigene Logs. Der Bereich erscheint, sobald es einen Entwurf gibt.</>],
    ['Dateien', () => <>Die Dateien einer Version: Code und Assets, das Programm selbst. Ein Schloss oder ein Globus zeigt, ob sie öffentlich erreichbar sind.</>],
    ['Daten', () => <>Was das Lambda zur Laufzeit aufbewahrt, für alle Versionen gemeinsam: die Datenbank, der Workspace und die Secrets, jeweils mit eigenem Reiter. In Tabellen und Dateien hineinsehen, Dateien hochladen, Secrets setzen oder eine Art ein- und ausschalten. Die einfache Ansicht zeigt den Bereich, sobald die App etwas aufbewahrt.</>],
    ['Versionen', () => <>Was jede Version geändert hat, worum gebeten wurde und der Diff zur vorherigen. Von hier aus deployen oder zurückrollen – oder aus jeder von ihnen einen Entwurf beginnen.</>],
    ['Deployments', () => <>Was wann online war und warum es offline ging.</>],
    ['Statistik', () => <>Requests, Fehler, Antwortzeiten und die meistgefragten Pfade der letzten Stunde oder des letzten Tages.</>],
    ['Logs', () => <>Requests, Ausgaben und Stacktraces von allem, was schiefging – live.</>],
    [
      'Code',
      (k) => (
        <>
          Selbst schreiben. {k.b('Prüfen')} kompiliert, {k.b('Speichern')} legt eine Version an, {k.b('Deployen')} stellt
          sie online. In einem Entwurf behält {k.b('Speichern')} den Code im Entwurf und zeigt ihn unter der Adresse des
          Entwurfs. {k.code('Strg+S')} speichert, {k.code('F12')} springt zur Deklaration.
        </>
      ),
    ],
    ['Tests', () => <>Wie die App automatisch getestet wird, mit den Scripts und Testdaten dafür. Nur in der vollständigen Ansicht.</>],
  ],
  sections: (k) => (
    <>
      Alle Bereiche funktionieren gleich: oben der Titel, ein {k.b('ⓘ')} mit Erklärung, rechts die Aktionen und – wo es
      mehrere Ansichten gibt – darunter eine Reihe von Tabs. Beim Code sind die Tabs seine Dateien. Die vollständige
      Ansicht fasst die Bereiche in Gruppen zusammen: wie Leute es finden, wo eine Änderung entsteht, das Programm und
      seine Daten und wie es läuft.
    </>
  ),
  editorAside:
    'Traffic und Log liegen im Arbeitsspeicher. Sie sind zum Beobachten da, nicht zum Aufbewahren: Nach einem Neustart des Servers beginnen sie von vorn. Versionen und der Deployment-Verlauf werden gespeichert.',

  why: (k) => (
    <>
      Eine Version ist der Code plus zwei optionale Notizen: {k.b('die Spezifikation')} – was der Nutzer möchte und
      warum, möglichst in seinen Worten – und {k.b('die Änderung')}, eine Zeile dazu, was die Version tut. Beide stehen im
      Versionsverlauf neben dem Diff. So bleibt das {k.em('Warum')} neben dem {k.em('Was')} erhalten – für Sie und für
      den nächsten Agenten, der den Verlauf liest, bevor er etwas ändert.
    </>
  ),
  whySample: {
    specification: 'Ein Gästebuch zum Eintragen; Einträge müssen einen Neustart überstehen',
    change: 'Speichert Einträge in der Datenbank, damit sie einen Neustart überstehen',
  },
  why2: (k) => (
    <>
      Agenten übergeben dieselben beiden Felder an {k.code('write_code')}. Unter {k.b('Code')} wird beim Speichern
      nach der Änderung gefragt. Beides ist optional. Zu lange Texte werden gekürzt statt abgelehnt: die Spezifikation
      nach 4000 Zeichen, die Änderung nach 500. Ein Entwurf hat seine eigenen beiden; wird er übernommen, gehen sie an
      die neue Version über.
    </>
  ),

  written: (k) => (
    <>
      Jede Version bewahrt neben ihrem Programm auf, was über sie geschrieben ist: ihre {k.b('Dokumentation')} – was
      die App ist, für wen sie gedacht ist und warum, und warum sie so gebaut ist, wie sie ist – und ihre{' '}
      {k.b('Tests')}: wie sich automatisch prüfen lässt, dass sie funktioniert, mit den Scripts und Testdaten dafür.
      Agenten schreiben beides mit einem neuen Lambda und halten es mit jeder Änderung aktuell. Der nächste Agent, der
      das Lambda ändert, liest es zuerst – so weiß er, wofür die App da ist und was weiter funktionieren muss. Das sagt
      der Code allein nicht.
    </>
  ),
  writtenFiles: [
    ['.lambda/docs/product.md', 'was die App ist, für wen sie gedacht ist, was Leute damit tun und warum'],
    ['.lambda/docs/decisions.md', 'die technischen Entscheidungen und warum sie getroffen wurden'],
    ['.lambda/tests/README.md', 'wie die App automatisch getestet wird und wie die Tests ausgeführt werden'],
    ['.lambda/tests/…', 'die Scripts und Testdaten, die die Tests verwenden'],
  ],
  written2: (k) => (
    <>
      Sie sind Dateien der Version wie alle anderen, im Ordner {k.code('.lambda')}: Der Verlauf zeigt, was eine Version
      daran geändert hat, Zurückrollen bringt die Dokumentation zurück, die für diese Version galt, und ein Entwurf hat
      eine eigene Kopie, die mit ihm online geht. Sie werden nie kompiliert und nie ausgeliefert und zählen zu dem, was
      die Assets einer Version umfassen dürfen.
    </>
  ),
  written3: (k) => (
    <>
      Im Kontrollzentrum zeigt {k.b('Dokumentation')} die Seiten zum Lesen und {k.b('Tests')}, wie die App getestet
      wird, samt den Dateien daneben; die Version wählen Sie wie bei ihren Dateien. Eine Seite lässt sich dort auch
      bearbeiten – das speichert die nächste Version. Die einfache Ansicht nennt die Dokumentation{' '}
      {k.b('Über die App')} und zeigt nur, wofür die App da ist. Um das zu korrigieren, sagen Sie es dem Agenten.
    </>
  ),
  writtenAside:
    'Sie sind in der Sprache geschrieben, in der Sie mit dem Agenten sprechen – für den, der die App als Nächstes ändert, ob Mensch oder Agent. Keine Kopie des Codes, sondern wofür die App da ist und warum.',

  features: (k) => (
    <>
      Eine Version ändert sich nie mehr, sobald sie gespeichert ist – und genau deshalb lohnt es sich, jede
      aufzuheben: Jede lässt sich vergleichen und genau so wieder online stellen, wie sie war. Um ein Lambda zu ändern,
      das Leute nutzen, beginnen Sie stattdessen einen {k.b('Entwurf')}.
    </>
  ),
  featureSteps: [
    (k) => (
      <>
        Beginnen Sie ihn aus einer beliebigen Version unter {k.b('Versionen')}, oder lassen Sie den Agenten einen beginnen.
        Er ist eine Kopie von Code, Assets, Dokumentation und Tests dieser Version und der Daten des Lambdas.
      </>
    ),
    (k) => (
      <>
        Ändern Sie ihn so oft wie nötig – unter {k.b('Code')} oder indem Sie den Agenten darum bitten. Seine Vorschau
        antwortet unter einer eigenen Adresse, {k.code('/features/…/')}, mit eigenen Testdaten. Besucher des Lambdas
        sehen nichts davon, und nichts, was er schreibt, erreicht die Daten des Lambdas.
      </>
    ),
    (k) => (
      <>
        Klicken Sie auf {k.b('Online stellen')}, sobald alles passt: Der Entwurf wird zur nächsten Version, mit seinen
        Notizen, und geht online. Dabei verschwindet er – samt seiner Vorschau und seinen Testdaten.
      </>
    ),
  ],
  featureSample: 'Bestenliste',
  featuresAside: () => (
    <>
      An mehreren Entwürfen kann gleichzeitig gearbeitet werden. Nur ein Entwurf, der auf dem neuesten Stand der
      Version ist, kann online gehen – damit er nie eine Version rückgängig macht, die nach seinem Beginn gespeichert
      wurde. Ging zuerst ein anderer online, holen Sie dessen Änderungen herein – oder bitten Sie den Agenten darum –
      und markieren Sie den Entwurf als aktuell. Nichts geht von selbst online; das ist Absicht. Die API nennt einen
      Entwurf „feature“ und das Online-Stellen „merge“.
    </>
  ),

  files: (k) => (
    <>
      Typen müssen nicht unter dem Code stehen, der sie nutzt. Klicken Sie unter {k.b('Code')} neben den Dateien auf{' '}
      {k.b('+')}: Die neue Datei wird zusammen mit dem Snippet im selben Namespace kompiliert, also müssen Sie nichts
      importieren. Ein Name ohne Endung gilt als C#.
    </>
  ),

  page: 'Für eine Seite gibt es zwei Wege – und einen weiteren für das, was Leute daneben hochladen.',
  inlineTitle: 'Eine Seite, direkt im Code',
  inline: 'Gut für Kleines. Die Seite ist Teil des Snippets.',
  folderTitle: 'Ein Ordner mit echten Dateien',
  folder:
    'Das Richtige für alles mit Stylesheet und Script. Die Dateien legen Sie genauso an wie eine C#-Datei. Sie werden ausgeliefert, wie sie sind – nichts wird kompiliert.',
  workspaceTitle: 'Hochgeladene Dateien, aus den Daten',
  workspace:
    'Für das, was Leute hochladen oder das Lambda erzeugt – Bilder, Dokumente –, ausgeliefert neben der App. Nicht für die Seiten der App selbst: Die gehören in einen Ordner mit Dateien, damit sie in derselben Version stecken wie der Code, der sie braucht.',

  spa: (k) => (
    <>
      Der zweite Weg im Detail. Jede Demo liefert ihre Seite so aus, aus einem Ordner namens {k.code('web')} – öffnen Sie
      zum Beispiel {k.link('/editor/demo-crud', 'demo-crud')}. Demos sind schreibgeschützt; ihr Editor-Schlüssel ist ihr
      Name.
    </>
  ),
  spaSteps: [
    (k) => (
      <>
        Klicken Sie unter {k.b('Code')} neben den Dateien auf {k.b('+')} und geben Sie {k.code('site/index.html')} ein.
        Ein Schrägstrich im Namen legt die Datei in einen Ordner. Eine Endung sagt, was für eine Datei es ist.
      </>
    ),
    (k) => (
      <>
        Legen Sie {k.code('site/app.css')} und {k.code('site/app.js')} genauso an. Ihre Seite verweist über den Namen auf
        sie, etwa mit {k.code('href="app.css"')}. Denn der Ordner ist die Wurzel dessen, was ausgeliefert wird – kein Teil
        der Adresse.
      </>
    ),
    (k) => (
      <>
        Für alles, was kein Text ist, etwa Bilder oder Schriften, öffnen Sie eine Datei in {k.code('site')} und klicken
        neben den Dateien auf den Upload-Button: Die Datei landet im selben Ordner. Ein PNG kann man nicht in einen
        Texteditor tippen – das hier ist der Weg.
      </>
    ),
    (k) => <>Liefern Sie den Ordner in {k.code('lambda.cs')} aus:</>,
    (k) => (
      <>
        Klicken Sie auf {k.b('Deployen')}. {k.code('site/index.html')} antwortet unter {k.code('/')},{' '}
        {k.code('site/app.css')} unter {k.code('/app.css')}. Jede Adresse ohne passende Datei bekommt die Seite. So
        funktioniert ein Frontend mit eigenem Routing auch, wenn jemand einen Deep Link neu lädt.
      </>
    ),
    () => <>Mit einer API daneben hat die Seite auch einen Gesprächspartner:</>,
  ],

  storage: (k) => (
    <>
      Ein Lambda hat Dateien an zwei Orten, und der Editor zeigt sie getrennt: {k.b('Dateien')} enthält die Dateien
      einer Version – das Programm –, {k.b('Daten')} den Workspace – was das Programm aufbewahrt. Der Unterschied
      ist, {k.em('wem sie gehören')}. Die Dateien einer Version gehören zu dieser Version; die Daten gehören dem
      Lambda, und alle Versionen teilen sie.
    </>
  ),
  savedWithCode: 'In einer Version',
  workspaceColumn: 'In den Daten',
  table: [
    ['Inhalt', 'Code und Assets: das Programm, samt Frontend – und seine Dokumentation und Tests', 'alles, was das Lambda schreibt oder jemand hochlädt'],
    ['Ändert sich', 'nie – eine Änderung ist eine neue Version', 'sobald etwas hineingeschrieben wird'],
    ['Ein Deployment', 'stellt genau diese Dateien online', 'lässt sie unberührt'],
    ['Zurückrollen', 'bringt die alten Dateien zurück', 'keine Wirkung: Alle Versionen teilen sie'],
    ['Ein Entwurf', 'beginnt als Kopie davon', 'arbeitet mit einer Kopie davon'],
    ['Wird gelöscht', 'mit alten Versionen, sobald das Limit überschritten ist', 'mit dem Lambda oder wenn Sie sie ausschalten'],
  ],
  reachedAs: 'im Code erreichbar als',
  storageAside:
    'Ein gemeinsamer Ort geht nicht. Sonst würde ein Deployment entweder alles löschen, was Ihr Lambda seitdem geschrieben hat – oder aus dem, was es ausliefert, ließe sich nie etwas entfernen. Ein Spiel mit Bestenliste braucht das Zweite, die Seite dazu das Erste. Also gehört die Seite in die Version und die Bestenliste in die Daten.',

  database: (k) => (
    <>
      Datensätze – Einträge, Konten, Bestellungen, Stimmen – gehören in die {k.b('Datenbank')}: eine eigene
      SQLite-Datenbank des Lambdas, die Sie unter {k.b('Daten')} einschalten. Der Code öffnet mit{' '}
      {k.code('Database.GetConnection()')} eine Verbindung und liest und schreibt über{' '}
      {k.link('https://learn.microsoft.com/ef/core/', 'Entity Framework Core')}, mit einem eigenen Kontext, der die
      Tabellen abbildet:
    </>
  ),
  database2: (k) => (
    <>
      Ihre Tabellen legen {k.b('Migrationen')} an: SQL-Dateien in {k.code('migrations/')}, die zur Version gehören und
      beim Start des Lambdas von {k.link('https://evolve-db.netlify.app/', 'Evolve')} der Reihe nach angewendet werden –
      jede genau einmal, eine neue Version führt also nur aus, was neu ist. Ändern Sie nie eine Migration, die schon
      angewendet wurde; eine Änderung an einer Tabelle ist die nächste Datei.
    </>
  ),
  database3: (k) => (
    <>
      Wie alle Daten teilen sich alle Versionen die Datenbank, Deployments und Zurückrollen lassen sie unberührt, und
      ein Entwurf arbeitet mit einer Kopie. Unter {k.b('Daten')} sehen Sie ihre Tabellen und was darin steht – die
      einfache Ansicht nennt sie Einträge. {k.b('Als .NET-Projekt herunterladen')} nimmt sie als gewöhnliche
      SQLite-Datei mit.
    </>
  ),
  databaseAside: (k) => (
    <>
      Legen Sie einen Kontext dort an, wo Sie ihn brauchen, geben Sie ihn danach wieder frei, und verwenden Sie ihn
      synchron – {k.code('ToList')} und {k.code('SaveChanges')}, nicht {k.code('ToListAsync')} und{' '}
      {k.code('SaveChangesAsync')}. Die Tabellen legen die Migrationen an, nie Entity Framework. Die Demo{' '}
      {k.link('/editor/demo-crud', 'demo-crud')} zeigt all das.
    </>
  ),

  keeping: (k) => (
    <>
      {k.code('Workspace')} ist ein privates Verzeichnis, in dem Ihr Lambda lesen und schreiben darf: der Ort für
      Dateien – Bilder, die jemand hochlädt, ein Dokument, das es erzeugt, ein Modell, das es lädt. Datensätze gehören in
      die Datenbank, und auch was man über eine Datei weiß – wer sie hochgeladen hat und wann –, ist ein Datensatz.
    </>
  ),
  keeping2: (k) => (
    <>
      Dazu gibt es {k.code('ReadBytes')}, {k.code('WriteBytes')}, {k.code('Delete')}, {k.code('List')},{' '}
      {k.code('CreateFolder')} und zum Ausliefern {k.code('Tree')}/{k.code('Files')}/{k.code('App')}. Sonst ist nichts
      im Dateisystem erreichbar.
    </>
  ),

  secrets: (k) => (
    <>
      Ein API-Schlüssel, ein Passwort oder ein Token gehört in die {k.b('Secrets')}, nicht in den Code – dort hätte ihn
      jede Version, jeder Download und jeder, der die Historie liest. Der Code liest ein Secret über seinen Namen:
    </>
  ),
  secrets2: (k) => (
    <>
      Schalten Sie Secrets unter {k.b('Daten')} ein und setzen Sie den Wert dort. Einmal gespeichert, wird er nie wieder
      angezeigt – weder Ihnen noch einem Agenten; Sie können ihn nur ersetzen. Die Liste zeigt, welche Namen der Code
      liest, für die noch kein Wert gesetzt ist, und die Übersicht fragt danach. {k.code('Secret.Exists')} sagt, ob
      eines gesetzt ist – für Code, der auch ohne auskommt. Wie alle Daten teilen sich alle Versionen die Secrets, und
      ein Entwurf arbeitet mit einer Kopie.
    </>
  ),
  secretsAside: (k) => (
    <>
      Sie werden verschlüsselt gespeichert, mit einem Schlüssel, der nicht in der Datenbank liegt. In einem
      heruntergeladenen Projekt liest {k.code('Secret.Read("NAME")')} die Umgebungsvariable {k.code('NAME')} – die Werte
      selbst bleiben hier.
    </>
  ),

  sockets: (k) => (
    <>
      Unterstützt – und von Anfang an mitgedacht. Die Demo {k.link('/editor/demo-game', 'demo-game')} bringt Spieler zusammen und
      führt jede Partie auf dem Server aus. Die einfachste Form sind drei Callbacks:
    </>
  ),
  socketsAside: (k) => (
    <>
      In diese Falle tappt jeder: Browser können beim Websocket-Handshake keine Header setzen. Übergeben Sie, was der
      Handler braucht, in der Query – er liest sie aus {k.code('connection.Request.Header.Query')}. Secrets schicken Sie
      als erste Nachricht.
    </>
  ),

  limits:
    'Ihr Code läuft auf einem gemeinsam genutzten Server. Deshalb wird manches in C# schon vor dem Kompilieren abgelehnt: Prozesse starten, eigene Sockets öffnen, Assemblies laden, auf das Dateisystem außerhalb Ihres Workspace zugreifen – und Reflection, die all das umgehen soll. Ebenso das Warten auf einen Task mit .Result oder .Wait() statt await: Anfragen laufen auf einem Thread pro Kern, und der Task müsste genau auf dem Thread fertig werden, der auf ihn wartet.',
  limits2:
    'Alles andere ist da, auch die komplette API der GenHTTP-Module. Wird etwas abgelehnt, sehen Sie, in welcher Zeile und warum – nicht nur, dass es fehlschlug.',

  away: (k) => (
    <>
      Mit {k.b('Als .NET-Projekt herunterladen')} im Editor bekommen Sie alles als Solution, die Sie öffnen, mit{' '}
      {k.code('dotnet run')} starten und behalten können. Sie braucht nur das GenHTTP-Paket und bringt ein{' '}
      {k.code('Dockerfile')} mit, um sie als Container zu bauen und zu betreiben.
    </>
  ),
  away2: (k) => (
    <>
      Ihr Snippet wird zu {k.code('Project.cs')}, und {k.code('Program.cs')} liefert aus, was es zurückgibt. Ihre
      anderen Dateien kommen genau so mit, wie Sie sie geschrieben haben. {k.code('Workspace')} und {k.code('Assets')}{' '}
      werden zu zwei Ordnern neben dem Programm, mit denselben Methoden, getrennt in einem Ordner{' '}
      {k.code('Platform')} – an Ihrem Code ändert sich nichts.
      {' '}{k.code('Secret')} liest dort gleichnamige Umgebungsvariablen; die Werte bleiben hier. Die Dokumentation und
      die Tests kommen in {k.code('docs')} und {k.code('tests')} mit.
      {' '}{k.code('Database')} öffnet {k.code('database/database.db')}, die der Download samt den Datensätzen enthält,
      die Ihre App aufbewahrt hat.
    </>
  ),
  awayAside:
    'Gut zu wissen, bevor Sie hier etwas bauen: Was Sie schreiben, gehört Ihnen, und Sie können es komplett mitnehmen. Dass es auf unserem Server läuft, bindet es nicht an unseren Server.',

  open: (k) => (
    <>
      Wenn das, was Sie gebaut haben, anderen helfen könnte, veröffentlichen Sie den Code: Öffnen Sie im
      Kontrollzentrum {k.b('Open Source')}, wählen Sie eine Lizenz – MIT, sofern Sie keine andere möchten – und schalten
      Sie die Veröffentlichung ein. Der Code bekommt eine eigene Seite unter den{' '}
      {k.link('/source', 'Open-Source-Apps')}. Dort kann ihn jeder lesen, mit einem Stern versehen und jede Version als
      dasselbe Projekt herunterladen, das Ihnen {k.b('Als .NET-Projekt herunterladen')} liefert – mit der Lizenz daneben.
    </>
  ),
  open2: () => (
    <>
      Veröffentlicht wird jede Version, auch die früheren, mit ihrer Dokumentation, ihren Tests und der Änderung, die
      sie gemacht hat. Was die App aufbewahrt, wird nie veröffentlicht – ihre Datensätze, die Dateien, die sie
      gespeichert hat, die Werte ihrer Schlüssel und Passwörter –, ebenso wenig wie das, worum Sie in Ihren eigenen
      Worten gebeten haben, oder wer die App nutzt. Schalten Sie die Veröffentlichung aus, ist die Seite weg; ihre
      Sterne bleiben erhalten, falls Sie den Code wieder veröffentlichen.
    </>
  ),
  openAside:
    'Alles im Code wird öffentlich, auch die früheren Versionen. Ein Schlüssel oder Passwort gehört zu den Schlüsseln und Passwörtern unter Daten, nie in den Code – ob veröffentlicht oder nicht.',

  agents: (k) => (
    <>
      Unter {k.code('/mcp')} gibt es einen MCP-Endpunkt. Verbinden Sie einen Agenten damit, und er kann alles, was der
      Editor kann: die Anleitung lesen, eine Demo komplett lesen, Dateien schreiben, kompilieren und deployen. Darunter
      liegt dieselbe API.
    </>
  ),
  agents2: (k) => (
    <>
      Dabei sagt er, warum er etwas tut – {k.code('write_code')} nimmt Spezifikation und Änderung entgegen. Und er kann
      prüfen, was er deployt hat: {k.code('read_logs')} liefert die letzten Requests des Lambdas, seine Ausgaben und die
      Stacktraces aller Exceptions. So weiß ein Agent, dass sein Code funktioniert, statt es nur anzunehmen.
      Dasselbe sehen Sie im Kontrollzentrum. Er schreibt dabei auch die Dokumentation und die Tests, liest sie, bevor
      er etwas ändert, und führt die Tests gegen die Adresse eines Entwurfs aus, bevor er den Entwurf online stellt.
      Eine Seite, die gefunden werden soll, bekommt einen Titel, eine Beschreibung, ein Icon und eine Vorschau für den
      Fall, dass jemand ihren Link teilt.
    </>
  ),
  more: 'Mehr dazu →',
  make: 'Lambda erstellen',
};
