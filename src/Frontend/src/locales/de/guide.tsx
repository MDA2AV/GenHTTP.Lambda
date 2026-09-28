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
    features: 'Gefahrlos ändern',
    files: 'Mehrere Dateien',
    page: 'Eine Seite ausliefern',
    spa: 'Ein Frontend, Schritt für Schritt',
    storage: 'Zwei Orte für Dateien',
    keeping: 'Daten speichern',
    sockets: 'Websockets',
    limits: 'Was nicht erlaubt ist',
    away: 'Alles mitnehmen',
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
    ['Übersicht', () => <>Ob es online ist, wie viele Requests es heute hatte und wie viele davon fehlschlugen, die letzte Änderung und wie viel Platz noch frei ist.</>],
    [
      'Ändern',
      (k) => (
        <>
          Sagen Sie, was anders sein soll, und der Agent auf diesem Server setzt es um, während Sie zusehen. Er arbeitet
          in einem Entwurf, probiert die Änderung dort aus und übernimmt sie als nächste Version, sobald sie funktioniert.
          Schalten Sie {k.b('Nach Abschluss online stellen')} aus, um den Entwurf zuerst selbst auszuprobieren.
        </>
      ),
    ],
    ['Entwürfe', () => <>Änderungen, an denen neben dem Lambda gearbeitet wird: Jede wird unter einer eigenen Adresse ausprobiert und als nächste Version übernommen, sobald alles passt. Geöffnet hat ein Entwurf eigenen Code, eigene Daten und eigene Logs.</>],
    ['Dateien', () => <>Die Dateien einer Version: Code und Assets, das Programm selbst. Ein Schloss oder ein Globus zeigt, ob sie öffentlich erreichbar sind.</>],
    ['Daten', () => <>Was das Lambda zur Laufzeit aufbewahrt, für alle Versionen gemeinsam: der Workspace. Hineinsehen, Dateien hochladen und löschen oder ihn ausschalten.</>],
    ['Versionen', () => <>Was jede Version geändert hat, worum gebeten wurde und der Diff zur vorherigen. Von hier aus deployen oder zurückrollen – oder aus jeder von ihnen einen Entwurf beginnen.</>],
    ['Deployments', () => <>Was wann online war und warum es offline ging.</>],
    ['Statistik', () => <>Requests, Fehler, Antwortzeiten und die meistgefragten Pfade der letzten Stunde oder des letzten Tages.</>],
    ['Logs', () => <>Requests, Ausgaben und Stacktraces von allem, was schiefging – live.</>],
    [
      'Code',
      (k) => (
        <>
          Selbst schreiben. {k.b('Prüfen')} kompiliert, {k.b('Speichern')} legt eine Version an, {k.b('Deployen')} stellt
          sie online. In einem Entwurf behält {k.b('Speichern')} den Code im Entwurf, und {k.b('Vorschau deployen')}{' '}
          stellt ihn unter der Adresse des Entwurfs online. {k.code('Strg+S')} speichert, {k.code('F12')} springt zur
          Deklaration.
        </>
      ),
    ],
  ],
  sections: (k) => (
    <>
      Alle Bereiche funktionieren gleich: oben der Titel, ein {k.b('ⓘ')} mit Erklärung, rechts die Aktionen und – wo es
      mehrere Ansichten gibt – darunter eine Reihe von Tabs. Beim Code sind die Tabs seine Dateien.
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
    change: 'Speichert Einträge im Workspace, damit sie einen Neustart überstehen',
  },
  why2: (k) => (
    <>
      Agenten übergeben dieselben beiden Felder an {k.code('write_code')}. Unter {k.b('Code')} wird beim Speichern
      nach der Änderung gefragt. Beides ist optional. Zu lange Texte werden gekürzt statt abgelehnt: die Spezifikation
      nach 4000 Zeichen, die Änderung nach 500. Ein Entwurf hat seine eigenen beiden; wird er übernommen, gehen sie an
      die neue Version über.
    </>
  ),

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
        Beginnen Sie ihn unter {k.b('Entwürfe')} oder aus einer beliebigen Version. Er ist eine Kopie von Code und
        Assets dieser Version und der Daten des Lambdas.
      </>
    ),
    (k) => (
      <>
        Ändern Sie ihn so oft wie nötig – unter {k.b('Code')} oder indem Sie den Agenten darum bitten.{' '}
        {k.b('Vorschau deployen')} stellt ihn unter einer eigenen Adresse online, {k.code('/features/…/')}, mit einer
        eigenen Kopie der Daten. Besucher des Lambdas sehen nichts davon, und nichts, was er schreibt, erreicht die Daten
        des Lambdas.
      </>
    ),
    (k) => (
      <>
        Klicken Sie auf {k.b('Übernehmen')}, sobald alles passt: Der Entwurf wird zur nächsten Version, mit seinen
        Notizen, und geht auf Wunsch sofort online. Dabei verschwindet er – samt seiner Vorschau und seiner Kopie der
        Daten.
      </>
    ),
  ],
  featureSample: 'Bestenliste',
  featuresAside: () => (
    <>
      An mehreren Entwürfen kann gleichzeitig gearbeitet werden. Übernehmen lässt sich nur einer, der auf der neuesten
      Version basiert – damit das Übernehmen nie eine Version rückgängig macht, die nach dem Beginn des Entwurfs
      gespeichert wurde. Wurde zuerst ein anderer übernommen, holen Sie dessen Änderungen herein – oder bitten Sie
      den Agenten darum – und geben Sie dann an, dass der Entwurf auf der neuesten Version basiert. Nichts wird von selbst
      übernommen; das ist Absicht.
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
    ['Inhalt', 'Code und Assets: das Programm, samt Frontend', 'alles, was das Lambda schreibt oder jemand hochlädt'],
    ['Ändert sich', 'nie – eine Änderung ist eine neue Version', 'sobald etwas hineingeschrieben wird'],
    ['Ein Deployment', 'stellt genau diese Dateien online', 'lässt sie unberührt'],
    ['Zurückrollen', 'bringt die alten Dateien zurück', 'keine Wirkung: Alle Versionen teilen sie'],
    ['Ein Entwurf', 'beginnt als Kopie davon', 'arbeitet mit einer Kopie davon'],
    ['Wird gelöscht', 'mit alten Versionen, sobald das Limit überschritten ist', 'mit dem Lambda oder wenn Sie sie ausschalten'],
  ],
  reachedAs: 'im Code erreichbar als',
  storageAside:
    'Ein gemeinsamer Ort geht nicht. Sonst würde ein Deployment entweder alles löschen, was Ihr Lambda seitdem geschrieben hat – oder aus dem, was es ausliefert, ließe sich nie etwas entfernen. Ein Spiel mit Bestenliste braucht das Zweite, die Seite dazu das Erste. Also gehört die Seite in die Version und die Bestenliste in die Daten.',

  keeping: (k) => (
    <>
      {k.code('Workspace')} ist ein privates Verzeichnis, in dem Ihr Lambda lesen und schreiben darf. Hier gehört alles
      hin, was einen Request oder ein Deployment überdauern soll.
    </>
  ),
  keeping2: (k) => (
    <>
      Dazu gibt es {k.code('ReadBytes')}, {k.code('WriteBytes')}, {k.code('Delete')}, {k.code('List')},{' '}
      {k.code('CreateFolder')} und zum Ausliefern {k.code('Tree')}/{k.code('Files')}/{k.code('App')}. Sonst ist nichts
      im Dateisystem erreichbar.
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
    'Ihr Code läuft auf einem gemeinsam genutzten Server. Deshalb wird manches in C# schon vor dem Kompilieren abgelehnt: Prozesse starten, eigene Sockets öffnen, Assemblies laden, auf das Dateisystem außerhalb Ihres Workspace zugreifen – und Reflection, die all das umgehen soll.',
  limits2:
    'Alles andere ist da, auch die komplette API der GenHTTP-Module. Wird etwas abgelehnt, sehen Sie, in welcher Zeile und warum – nicht nur, dass es fehlschlug.',

  away: (k) => (
    <>
      Mit {k.b('Als .NET-Projekt herunterladen')} im Editor bekommen Sie alles als Solution, die Sie öffnen, mit{' '}
      {k.code('dotnet run')} starten und behalten können. Sie hat genau eine Paketreferenz und keine Spur dieser
      Plattform.
    </>
  ),
  away2: (k) => (
    <>
      Ihr Snippet wird zum Inhalt von {k.code('Program.cs')}, in einem Host, der ausliefert, was es zurückgibt. Ihre
      anderen Dateien kommen genau so mit, wie Sie sie geschrieben haben. {k.code('Workspace')} und {k.code('Assets')}{' '}
      werden zu zwei Ordnern neben dem Code, mit denselben Methoden – an Ihrem Code ändert sich nichts.
    </>
  ),
  awayAside:
    'Gut zu wissen, bevor Sie hier etwas bauen: Was Sie schreiben, gehört Ihnen, und Sie können es komplett mitnehmen. Dass es auf unserem Server läuft, bindet es nicht an unseren Server.',

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
      Dasselbe sehen Sie im Kontrollzentrum.
    </>
  ),
  more: 'Mehr dazu →',
  make: 'Lambda erstellen',
};
