import type { Messages } from '../en';

export const guide: Messages['guide'] = {
  title: 'So funktioniert es',
  intro:
    'Sie schreiben ein C#-Snippet. Was es zurückgibt, ist innerhalb weniger Sekunden unter einer öffentlichen Adresse über HTTPS erreichbar. Diese Seite beschreibt die gesamte Plattform in der Reihenfolge, in der Sie ihr begegnen.',
  contents: 'Inhalt',

  parts: {
    what: 'Was ist ein Lambda?',
    first: 'Ihr erstes Lambda',
    editor: 'Das Kontrollzentrum',
    why: 'Änderungen begründen',
    files: 'Mehrere Dateien',
    page: 'Webseiten ausliefern',
    spa: 'Ein Frontend Schritt für Schritt',
    storage: 'Die zwei Speicherorte für Dateien',
    keeping: 'Daten dauerhaft speichern',
    sockets: 'Websockets',
    limits: 'Einschränkungen',
    away: 'Code exportieren',
    agents: 'Arbeiten mit Agenten',
  },

  what: [
    (k) => (
      <>
        Ein Lambda ist ein Code-Snippet, das einen GenHTTP-Handler zurückgibt. Die Plattform kompiliert und lädt es und
        stellt das Ergebnis unter Ihrer eigenen Adresse bereit. Es sind weder ein Projekt noch eine Build-Datei oder eine{' '}
        {k.code('using')}-Anweisung erforderlich; alle GenHTTP-Module sind bereits importiert.
      </>
    ),
    (k) => (
      <>
        Dies ist bereits ein vollständiges Lambda. Unter {k.code('/lambda/your-key/')} bereitgestellt, beantwortet es jede
        Anfrage mit dem Wort „hello“.
      </>
    ),
  ],
  whatAside: (k) => (
    <>
      Das Snippet besteht aus {k.em('Anweisungen')}, nicht aus einer Klasse. Zuletzt gibt es ein Objekt zurück, das
      Anfragen beantworten kann: einen Handler oder einen Builder für einen Handler.
    </>
  ),

  first: [
    (k) => (
      <>
        Klicken Sie auf {k.b('Lambda erstellen')}. Sie erhalten eine öffentliche Adresse und einen Editor-Schlüssel. Der
        Schlüssel ist der einzige Zugang und kann nicht wiederhergestellt werden – bitte bewahren Sie ihn sicher auf.
      </>
    ),
    () => (
      <>
        Sie gelangen in das Kontrollzentrum, in dem bereits ein kleiner REST-Dienst als erste Version angelegt ist. Er
        dient lediglich als Ausgangspunkt.
      </>
    ),
    (k) => (
      <>
        Übergeben Sie den Editor-Schlüssel einem Agenten und beschreiben Sie, was entstehen soll – er schreibt neue
        Versionen über {k.link('/#agents', 'MCP')}. Alternativ öffnen Sie {k.b('Code')} und schreiben selbst:{' '}
        {k.b('Prüfen')} kompiliert, ohne zu speichern, und zeigt die Meldungen des Compilers mit Datei und Zeile.
      </>
    ),
    (k) => (
      <>
        Klicken Sie auf {k.b('Bereitstellen')}. Das Lambda ist nun online; vorher ist nichts erreichbar. Eine erneute
        Bereitstellung verlängert die Laufzeit.
      </>
    ),
  ],

  editor: (k) => (
    <>
      Der Editor-Link öffnet ein Kontrollzentrum statt eines Texteditors: Der Großteil des Codes wird von Agenten
      geschrieben, daher sehen Sie zuerst den Zustand Ihres Lambdas. Die Seitenleiste zeigt, ob es online ist, seine
      Adresse, eine Schaltfläche für eine neuere, noch nicht bereitgestellte Version sowie die einzelnen Bereiche. Seltene
      Aktionen wie das Ändern der Adresse oder das Löschen finden Sie im Menü {k.b('⋯')}.
    </>
  ),
  bits: [
    ['Übersicht', () => <>Online-Status, Anzahl der heutigen und der fehlgeschlagenen Anfragen, die letzte Änderung und der verbleibende Speicherplatz.</>],
    ['Dateien', () => <>Die Dateien einer Version sowie die Daten, die das Lambda zur Laufzeit speichert. Ein Schloss oder eine Weltkugel zeigt an, ob sie öffentlich erreichbar sind.</>],
    ['Versionen', () => <>Die Änderungen und Anforderungen jeder Version sowie der Unterschied zur vorherigen. Von hier aus lassen sich Versionen bereitstellen oder wiederherstellen.</>],
    ['Bereitstellungen', () => <>Welche Version wann online war und wodurch sie beendet wurde.</>],
    ['Statistik', () => <>Anfragen, Fehler, Antwortzeiten und die am häufigsten aufgerufenen Pfade der letzten Stunde oder des letzten Tages.</>],
    ['Logs', () => <>Anfragen, Ausgaben und Stacktraces aufgetretener Fehler in Echtzeit.</>],
    [
      'Code',
      (k) => (
        <>
          Manuelle Bearbeitung. {k.b('Prüfen')} kompiliert, {k.b('Speichern')} legt eine Version an,{' '}
          {k.b('Bereitstellen')} stellt sie online. {k.code('Strg+S')} speichert, {k.code('F12')} springt zur
          Deklaration.
        </>
      ),
    ],
  ],
  sections: (k) => (
    <>
      Alle Bereiche sind gleich aufgebaut: ein Titel, ein {k.b('ⓘ')} mit Erläuterungen, die Aktionen auf der rechten Seite
      und – bei mehreren Ansichten – eine Reihe von Auswahlfeldern darunter. Im Bereich Code sind dies die Dateien.
    </>
  ),
  editorAside:
    'Datenverkehr und Log werden im Arbeitsspeicher gehalten und dienen der Beobachtung, nicht der Archivierung: Nach einem Neustart des Servers beginnen sie von vorn. Versionen und der Bereitstellungsverlauf werden dauerhaft gespeichert.',

  why: (k) => (
    <>
      Eine Version besteht aus dem Code und optional zwei Anmerkungen: {k.b('der Spezifikation')} – was der Nutzer
      wünscht und warum, möglichst in seinen eigenen Worten – und {k.b('der Änderung')}, einer Zeile zum Inhalt der
      Version. Beide erscheinen im Versionsverlauf neben dem Diff. So bleibt das {k.em('Warum')} neben dem {k.em('Was')}{' '}
      erhalten – für Sie und für jeden Agenten, der den Verlauf vor einer Änderung liest.
    </>
  ),
  whySample: {
    specification: 'Ein Gästebuch zum Eintragen; Einträge müssen einen Neustart überstehen',
    change: 'Speichert Einträge im Workspace, damit sie einen Neustart überstehen',
  },
  why2: (k) => (
    <>
      Agenten übergeben dieselben beiden Felder an {k.code('write_code')}. Im Bereich {k.b('Code')} wird beim Speichern
      nach der Änderung gefragt. Beide Angaben sind optional; eine lange Spezifikation wird nach 4000 Zeichen, eine
      Änderung nach 500 Zeichen gekürzt statt abgelehnt.
    </>
  ),

  files: (k) => (
    <>
      Typen müssen nicht unterhalb des Codes stehen, der sie verwendet. Klicken Sie im Bereich {k.b('Code')} auf{' '}
      {k.b('+')} neben den Dateien: Die neue Datei wird zusammen mit dem Snippet im selben Namespace kompiliert, sodass
      keine Importe nötig sind. Ein Name ohne Dateiendung wird als C#-Datei behandelt.
    </>
  ),

  page: 'Es gibt drei Möglichkeiten. Welche geeignet ist, hängt davon ab, wo die Seite liegt.',
  inlineTitle: 'Eine Seite direkt im Code',
  inline: 'Geeignet für kleine Anwendungen. Die Seite ist Teil des Snippets.',
  folderTitle: 'Ein Ordner mit Dateien',
  folder:
    'Die richtige Wahl für alles mit Stylesheets und Skripten. Die Dateien werden wie C#-Dateien angelegt und unverändert ausgeliefert; sie werden nicht kompiliert.',
  workspaceTitle: 'Aus dem Workspace',
  workspace: 'Wenn die Seite hochgeladen statt geschrieben wird und sich ohne neue Bereitstellung ändern lassen soll.',

  spa: (k) => (
    <>
      Die zweite Möglichkeit im Detail. Jede Demo liefert ihre Seite auf diese Weise aus einem Ordner namens{' '}
      {k.code('web')} aus – öffnen Sie {k.link('/editor/demo-crud', 'demo-crud')} als Beispiel. Demos sind
      schreibgeschützt; ihr Editor-Schlüssel entspricht ihrem Namen.
    </>
  ),
  spaSteps: [
    (k) => (
      <>
        Klicken Sie im Bereich {k.b('Code')} auf {k.b('+')} neben den Dateien und geben Sie {k.code('site/index.html')}{' '}
        ein. Ein Name mit Schrägstrich legt die Datei in einem Ordner ab; ein Name mit Dateiendung wird als entsprechende
        Datei behandelt.
      </>
    ),
    (k) => (
      <>
        Legen Sie {k.code('site/app.css')} und {k.code('site/app.js')} auf dieselbe Weise an. Ihre Seite verweist über den
        Dateinamen darauf, etwa mit {k.code('href="app.css"')}, da der Ordner das Wurzelverzeichnis der Auslieferung bildet
        und nicht Teil der Adresse ist.
      </>
    ),
    (k) => (
      <>
        Für Binärdateien wie Bilder oder Schriftarten öffnen Sie eine Datei in {k.code('site')} und nutzen die
        Upload-Schaltfläche neben den Dateien: Die Datei wird im selben Ordner abgelegt.
      </>
    ),
    (k) => <>Liefern Sie den Ordner in {k.code('lambda.cs')} aus:</>,
    (k) => (
      <>
        Klicken Sie auf {k.b('Bereitstellen')}. {k.code('site/index.html')} antwortet unter {k.code('/')},{' '}
        {k.code('site/app.css')} unter {k.code('/app.css')}, und jede Adresse ohne passende Datei wird mit der Seite
        beantwortet – ein Frontend mit eigenem Routing funktioniert also auch beim Neuladen eines tiefen Links.
      </>
    ),
    () => <>Ergänzen Sie eine API, mit der die Seite kommunizieren kann:</>,
  ],

  storage: (k) => (
    <>
      Der Bereich {k.b('Dateien')} zeigt beides – die Dateien einer Version und den Workspace als {k.b('Daten')} – und
      gibt an, was davon öffentlich erreichbar ist. Code-Dateien bearbeiten Sie im Bereich {k.b('Code')}; Daten lassen sich
      unter {k.b('Dateien')} hochladen und löschen. Beides unterscheidet sich jedoch darin, {k.em('wann es sich ändert')}.
    </>
  ),
  savedWithCode: 'Mit dem Code gespeichert',
  workspaceColumn: 'Workspace',
  table: [
    ['Inhalt', 'alle Dateien Ihres Lambdas, einschließlich des C#-Codes', 'alles, was geschrieben oder hochgeladen wurde'],
    ['Änderung', 'beim Speichern oder Bereitstellen', 'sobald etwas geschrieben wird'],
    ['Bereitstellung', 'ersetzt den gesamten Inhalt', 'bleibt unberührt'],
    ['Wiederherstellen einer Version', 'stellt die früheren Dateien wieder her', 'keine Auswirkung'],
    ['Klonen des Lambdas', 'wird übernommen', 'wird nicht übernommen'],
  ],
  reachedAs: 'Zugriff im Code über',
  storageAside:
    'Ein gemeinsames Verzeichnis ist nicht möglich: Andernfalls würde eine Bereitstellung entweder alle seither geschriebenen Daten löschen, oder aus dem ausgelieferten Inhalt ließe sich nie etwas entfernen. Eine Bestenliste benötigt das Zweite, die zugehörige Seite das Erste.',

  keeping: (k) => (
    <>
      {k.code('Workspace')} ist ein privates Verzeichnis, das Ihr Lambda lesen und beschreiben kann. Hier gehören alle
      Daten hin, die über eine Anfrage oder eine Bereitstellung hinaus erhalten bleiben sollen.
    </>
  ),
  keeping2: (k) => (
    <>
      Außerdem stehen {k.code('ReadBytes')}, {k.code('WriteBytes')}, {k.code('Delete')}, {k.code('List')},{' '}
      {k.code('CreateFolder')} sowie {k.code('Tree')}/{k.code('Files')}/{k.code('App')} zur Auslieferung zur Verfügung.
      Auf das übrige Dateisystem besteht kein Zugriff.
    </>
  ),

  sockets: (k) => (
    <>
      Websockets werden vollständig unterstützt. Die Demo {k.link('/editor/demo-game', 'demo-game')} bringt Spieler
      zusammen und führt jede Partie auf dem Server aus. Die einfachste Form besteht aus drei Callbacks:
    </>
  ),
  socketsAside: (k) => (
    <>
      Ein häufiger Stolperstein: Browser können beim Websocket-Handshake keine Header setzen. Übergeben Sie benötigte
      Werte in der Query, aus der der Handler sie über {k.code('connection.Request.Header.Query')} liest, oder senden Sie
      vertrauliche Werte als erste Nachricht.
    </>
  ),

  limits:
    'Ihr Code läuft auf einem gemeinsam genutzten Server. Daher werden einige C#-Funktionen bereits vor dem Kompilieren abgelehnt: das Starten von Prozessen, das Öffnen eigener Sockets, das Laden von Assemblies, der Zugriff auf das Dateisystem außerhalb Ihres Workspaces sowie Reflection, die diese Einschränkungen umgehen soll.',
  limits2:
    'Alle übrigen Funktionen stehen zur Verfügung, einschließlich der vollständigen GenHTTP-Modul-API. Wird etwas abgelehnt, erfahren Sie, in welcher Zeile und aus welchem Grund.',

  away: (k) => (
    <>
      {k.b('Als .NET-Projekt herunterladen')} im Editor liefert das vollständige Lambda als .NET-Projekt: eine Solution,
      die Sie öffnen, mit {k.code('dotnet run')} ausführen und behalten können. Sie enthält eine einzige Paketreferenz und
      keinerlei Abhängigkeit von dieser Plattform.
    </>
  ),
  away2: (k) => (
    <>
      Ihr Snippet wird zum Inhalt von {k.code('Program.cs')}, eingebettet in einen Host, der das Rückgabeobjekt ausliefert.
      Weitere Dateien werden unverändert übernommen. {k.code('Workspace')} und {k.code('Assets')} werden zu zwei Ordnern
      neben dem Code mit denselben Methoden – Ihr Code muss also nicht angepasst werden.
    </>
  ),
  awayAside:
    'Wichtig vorab: Der Code, den Sie hier schreiben, gehört Ihnen und lässt sich vollständig exportieren. Der Betrieb auf dieser Plattform bindet ihn nicht an diese Plattform.',

  agents: (k) => (
    <>
      Unter {k.code('/mcp')} steht ein MCP-Endpunkt zur Verfügung. Ein damit verbundener Agent kann alles, was auch der
      Editor kann: die Anleitung lesen, Demos vollständig einsehen, Dateien schreiben, kompilieren und bereitstellen. Beide
      nutzen dieselbe API.
    </>
  ),
  agents2: (k) => (
    <>
      Der Agent dokumentiert dabei seine Gründe – {k.code('write_code')} nimmt Spezifikation und Änderung entgegen – und
      kann das Ergebnis prüfen: {k.code('read_logs')} liefert die letzten Anfragen des Lambdas, seine Ausgaben und die
      Stacktraces aufgetretener Fehler. So stellt ein Agent fest, dass sein Code funktioniert, statt es nur anzunehmen.
      Dieselben Informationen sehen Sie im Kontrollzentrum.
    </>
  ),
  more: 'Weitere Informationen →',
  make: 'Lambda erstellen',
};
