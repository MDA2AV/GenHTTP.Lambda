import type { Messages } from '../en';

export const guide: Messages['guide'] = {
  title: 'Zo werkt het',
  intro:
    'Je schrijft een stukje C#. Wat dat teruggeeft, staat binnen een paar seconden online op een openbaar adres, via HTTPS. Hier staat alles, in de volgorde waarin je het tegenkomt.',
  contents: 'Inhoud',

  parts: {
    what: 'Wat een lambda is',
    first: 'Je eerste lambda',
    editor: 'Het dashboard',
    why: 'Uitleggen waarom',
    written: 'Documentatie en tests',
    features: 'Veilig aanpassen',
    files: 'Meer dan één bestand',
    page: 'Een pagina serveren',
    spa: 'Een frontend, stap voor stap',
    storage: 'De twee plekken voor bestanden',
    database: 'Records bewaren',
    keeping: 'Bestanden bewaren',
    secrets: 'Sleutels en wachtwoorden',
    sockets: 'Websockets',
    limits: 'Wat niet mag',
    away: 'Alles meenemen',
    open: 'De code publiceren',
    agents: 'Het aan een agent overlaten',
  },

  what: [
    (k) => (
      <>
        Een lambda is een snippet die een GenHTTP-handler teruggeeft. Het platform compileert hem, laadt hem en hangt
        wat hij teruggeeft onder je eigen adres. Er is geen project, geen buildbestand en geen {k.code('using')}{' '}
        nodig. Alle GenHTTP-modules zijn al voor je geïmporteerd.
      </>
    ),
    (k) => (
      <>
        Dat is een complete lambda. Gedeployd op {k.code('/lambda/your-key/')} beantwoordt hij elk request met het
        woord hello.
      </>
    ),
  ],
  whatAside: (k) => (
    <>
      De snippet bestaat uit {k.em('statements')}, niet uit een class. Het laatste wat hij doet, is iets teruggeven
      dat requests kan afhandelen: een handler, of een builder daarvoor.
    </>
  ),

  first: [
    (k) => (
      <>
        Klik op {k.b('Lambda aanmaken')}. Je krijgt een openbaar adres en een editorsleutel. De sleutel is de enige
        weg terug, dus bewaar hem goed. Niemand kan hem voor je herstellen.
      </>
    ),
    () => (
      <>
        Je komt terecht in het dashboard, waar als eerste versie al een kleine REST-service klaarstaat. Dat is maar
        een beginpunt.
      </>
    ),
    (k) => (
      <>
        Geef de editorsleutel aan een agent en vertel wat hij moet bouwen. Hij schrijft nieuwe versies via{' '}
        {k.link('/#agents', 'MCP')}. Of open {k.b('Code')} en schrijf het zelf: {k.b('Controleren')} compileert zonder
        iets op te slaan en laat zien wat de compiler ervan vindt, met bestand en regel.
      </>
    ),
    (k) => (
      <>
        Klik op {k.b('Deployen')}. Nu staat hij online. Tot dan is er niets bereikbaar. Opnieuw deployen verlengt hoe
        lang hij online blijft.
      </>
    ),
  ],

  editor: (k) => (
    <>
      De editorlink opent geen tekstvak maar een dashboard. De meeste code hier schrijven agents, dus het eerste wat
      je ziet, is hoe het met je lambda gaat. In de zijbalk staat de lambda zelf: of hij online is, zijn adres, en een
      knop als er een nieuwere versie klaarstaat om online te gaan. Daaronder staan de onderdelen. Wat je zelden doet,
      zoals het adres wijzigen of de lambda verwijderen, zit daar achter het menu {k.b('⋯')}.
    </>
  ),
  bits: [
    ['Overzicht', () => <>Wat de app is, of hij online is, hoeveel requests hij vandaag had en hoeveel daarvan misgingen, de laatste wijziging, en hoeveel ruimte er nog over is.</>],
    ['Documentatie', () => <>Wat de app is, voor wie hij is en waarom, en waarom hij gebouwd is zoals hij is – geschreven door agents, bewaard bij elke versie.</>],
    [
      'Aanpassen',
      (k) => (
        <>
          Zeg wat er anders moet, en de agent op deze server doet het terwijl jij meekijkt. Hij werkt in een concept,
          probeert het daar uit en voegt het samen tot de volgende versie zodra het werkt. Zet{' '}
          {k.b('Online zetten als het klaar is')} uit als je het concept eerst zelf wilt uitproberen.
          Hij werkt alleen aan je app: een verzoek dat er niets mee te maken heeft, of dat schade moet aanrichten, wijst hij af, en hij zegt waarom.
        </>
      ),
    ],
    ['Concepten', () => <>Wijzigingen waaraan naast de lambda wordt gewerkt: elk concept probeer je uit op een eigen adres, en het wordt samengevoegd tot de volgende versie zodra het goed is. Open je een concept, dan heeft het zijn eigen code, data en logs.</>],
    ['Bestanden', () => <>De bestanden van een versie: de code en assets, het programma zelf. Een slotje of een wereldbol laat zien of ze openbaar bereikbaar zijn.</>],
    ['Data', () => <>Wat de lambda bewaart terwijl hij draait, gedeeld door elke versie: de database, de workspace en de secrets, elk met een eigen tabblad. Bekijk de tabellen en bestanden, upload bestanden, stel secrets in of zet een soort aan of uit. De eenvoudige weergave toont het zodra de app iets bewaart.</>],
    ['Versies', () => <>Wat elke versie veranderde en wat er gevraagd werd, en het verschil met de vorige. Van hieruit deploy je of zet je een versie terug, en vanuit elke versie kun je een concept starten.</>],
    ['Deployments', () => <>Wat wanneer online stond, en waardoor het offline ging.</>],
    ['Statistieken', () => <>Requests, fouten, responstijden en de meest opgevraagde paden, over het afgelopen uur of de afgelopen dag.</>],
    ['Logs', () => <>Requests, output en de stacktrace van alles wat misging, live.</>],
    [
      'Code',
      (k) => (
        <>
          Zelf schrijven. {k.b('Controleren')} compileert, {k.b('Opslaan')} maakt een versie, {k.b('Deployen')} zet hem
          online. In een concept bewaart {k.b('Opslaan')} de code in het concept, en zet {k.b('Voorvertoning deployen')}{' '}
          hem online op het adres van het concept. {k.code('Ctrl-S')} slaat op; {k.code('F12')} springt naar een
          declaratie.
        </>
      ),
    ],
    ['Tests', () => <>Hoe de app automatisch getest wordt, met de scripts en testdata daarvoor. Alleen in de volledige weergave.</>],
  ],
  sections: (k) => (
    <>
      Elk onderdeel werkt hetzelfde: een titel, een {k.b('ⓘ')} met uitleg, acties rechts en, als er meer dan één
      weergave is, een rij tabs eronder. Bij de code zijn de tabs de bestanden. De volledige weergave deelt de
      onderdelen in groepen in: hoe mensen hem vinden, waar een wijziging gemaakt wordt, het programma en zijn data,
      en hoe hij draait.
    </>
  ),
  editorAside:
    'Het verkeer en de logs staan in het geheugen. Ze zijn om mee te kijken, niet om te bewaren: na een herstart van de server beginnen ze opnieuw. Versies en de deploygeschiedenis worden wel opgeslagen.',

  why: (k) => (
    <>
      Een versie is de code, plus eventueel twee notities: {k.b('de specificatie')} (wat de gebruiker wil en waarom,
      zo veel mogelijk in eigen woorden) en {k.b('de wijziging')} (één regel over wat de versie doet). Ze staan naast de
      diff in de versiegeschiedenis. Zo blijft het {k.em('waarom')} bewaard naast het {k.em('wat')}, voor jou en voor
      de volgende agent die de geschiedenis leest voordat hij iets verandert.
    </>
  ),
  whySample: {
    specification: 'Een gastenboek dat mensen kunnen tekenen; berichten moeten een herstart overleven',
    change: 'Bewaart berichten in de database, zodat ze een herstart overleven',
  },
  why2: (k) => (
    <>
      Agents geven dezelfde twee velden mee aan {k.code('write_code')}. In {k.b('Code')} wordt bij het opslaan om de
      wijziging gevraagd. Beide zijn optioneel. Een lange specificatie wordt afgekapt op 4000 tekens en een wijziging
      op 500, in plaats van geweigerd. Een concept heeft zijn eigen twee, en de versie waarin het wordt samengevoegd,
      neemt ze over.
    </>
  ),

  written: (k) => (
    <>
      Bij elke versie hoort, naast het programma, wat erover geschreven is: de {k.b('documentatie')} – wat de app is,
      voor wie hij is en waarom, en waarom hij gebouwd is zoals hij is – en de {k.b('tests')}: hoe je automatisch
      controleert dat hij werkt, met de scripts en testdata daarvoor. Agents schrijven ze bij een nieuwe lambda en
      houden ze bij met elke wijziging. De volgende agent die de lambda aanpast, leest ze eerst, zodat hij weet waar
      de app voor is en wat moet blijven werken – wat de code alleen niet vertelt.
    </>
  ),
  writtenFiles: [
    ['.lambda/docs/product.md', 'wat de app is, voor wie hij is, wat mensen ermee doen en waarom'],
    ['.lambda/docs/decisions.md', 'de technische beslissingen, en waarom ze genomen zijn'],
    ['.lambda/tests/README.md', 'hoe de app automatisch getest wordt, en hoe je de tests uitvoert'],
    ['.lambda/tests/…', 'de scripts en testdata die de tests gebruiken'],
  ],
  written2: (k) => (
    <>
      Het zijn bestanden van de versie zoals alle andere, in de map {k.code('.lambda')}: de geschiedenis laat zien
      wat een versie erin veranderde, terugzetten haalt de documentatie terug die voor die versie gold, en een concept
      heeft een eigen kopie die met het concept mee online gaat. Ze worden nooit gecompileerd en nooit geserveerd, en
      tellen mee voor de ruimte die de assets van een versie mogen innemen.
    </>
  ),
  written3: (k) => (
    <>
      In het dashboard toont {k.b('Documentatie')} de pagina's om te lezen, en {k.b('Tests')} hoe de app getest wordt
      en de bestanden ernaast; de versie kies je net als bij de bestanden. Je kunt een pagina daar ook bewerken; dat
      slaat de volgende versie op. De eenvoudige weergave noemt de documentatie {k.b('Over de app')} en toont alleen
      waar de app voor is – wil je dat verbeteren, zeg het dan tegen de agent.
    </>
  ),
  writtenAside:
    'Ze worden geschreven in de taal die je met de agent gebruikt, voor wie de app hierna aanpast – een mens of een agent. Geen kopie van de code: waar hij voor is, en waarom.',

  features: (k) => (
    <>
      Een versie verandert nooit meer als hij eenmaal is opgeslagen, en juist daardoor is elke versie het bewaren
      waard: je kunt ze allemaal vergelijken en precies zoals ze waren weer online zetten. Wil je een lambda aanpassen
      die mensen gebruiken, start dan in plaats daarvan een {k.b('concept')}.
    </>
  ),
  featureSteps: [
    (k) => (
      <>
        Start het onder {k.b('Concepten')}, of vanuit een willekeurige versie. Het is een kopie van de code, assets,
        documentatie en tests van die versie, en van de data van de lambda.
      </>
    ),
    (k) => (
      <>
        Pas het zo vaak aan als nodig, in {k.b('Code')} of door het aan de agent te vragen.{' '}
        {k.b('Voorvertoning deployen')} zet het online op een eigen adres, {k.code('/features/…/')}, met een eigen
        kopie van de data. Bezoekers van de lambda zien er niets van, en niets wat het wegschrijft, komt in de data van
        de lambda terecht.
      </>
    ),
    (k) => (
      <>
        Klik op {k.b('Samenvoegen')} zodra het goed is: het wordt de volgende versie, met zijn notities, en gaat meteen
        online als je dat wilt. Het concept verdwijnt dan, met zijn voorvertoning en zijn kopie van de data.
      </>
    ),
  ],
  featureSample: 'Ranglijst',
  featuresAside: () => (
    <>
      Je kunt aan meerdere concepten tegelijk werken. Alleen een concept dat op de nieuwste versie is gebaseerd, kan
      worden samengevoegd. Zo maakt samenvoegen nooit een versie ongedaan die is opgeslagen nadat het concept begon.
      Is er eerst een ander samengevoegd, haal dan de wijzigingen daarvan binnen (of vraag de agent dat te doen) en
      baseer het concept daarna op de nieuwste versie. Niets wordt vanzelf samengevoegd; dat is met opzet.
    </>
  ),

  files: (k) => (
    <>
      Types hoeven niet onder de code te staan die ze gebruikt. Klik in {k.b('Code')} op {k.b('+')} naast de
      bestanden. Het nieuwe bestand wordt naast de snippet gecompileerd, in dezelfde namespace, dus je hoeft niets te
      importeren om erbij te kunnen. Een naam zonder extensie wordt als C# gezien.
    </>
  ),

  page: 'Er zijn twee manieren om een pagina te serveren, en nog een voor wat mensen ernaast uploaden.',
  inlineTitle: 'Eén pagina, inline geschreven',
  inline: 'Prima voor iets kleins. De pagina zit in de snippet zelf.',
  folderTitle: 'Een map met echte bestanden',
  folder:
    'De juiste keuze voor alles met een stylesheet en een script. Je voegt de bestanden toe zoals een C#-bestand, en ze worden precies zo geserveerd als je ze schreef. Er wordt niets gecompileerd.',
  workspaceTitle: 'Geüploade bestanden, uit de data',
  workspace:
    "Voor wat mensen uploaden of wat de lambda aanmaakt, zoals foto's en documenten, geserveerd naast de app. Niet voor de pagina's van de app zelf: die horen in een map met bestanden, zodat ze in dezelfde versie zitten als de code die ze nodig heeft.",

  spa: (k) => (
    <>
      De tweede manier, helemaal uitgewerkt. Elke demo serveert zijn pagina zo, vanuit een map die {k.code('web')}{' '}
      heet. Open {k.link('/editor/demo-crud', 'demo-crud')} om er een te bekijken. Demo's zijn alleen-lezen; hun
      editorsleutel is hun naam.
    </>
  ),
  spaSteps: [
    (k) => (
      <>
        Klik in {k.b('Code')} op {k.b('+')} naast de bestanden en typ {k.code('site/index.html')}. Een slash in de naam
        zet het bestand in een map; de extensie bepaalt wat voor bestand het is.
      </>
    ),
    (k) => (
      <>
        Voeg {k.code('site/app.css')} en {k.code('site/app.js')} op dezelfde manier toe. Je pagina verwijst ernaar met
        alleen de naam, zoals {k.code('href="app.css"')}. De map is namelijk de root van wat er geserveerd wordt, geen
        deel van het adres.
      </>
    ),
    (k) => (
      <>
        Voor alles wat geen tekst is, zoals een afbeelding of een font, open je een bestand in {k.code('site')} en klik
        je op de uploadknop naast de bestanden: het komt in dezelfde map terecht. Een PNG kun je niet in een
        teksteditor typen, dus zo krijg je hem erin.
      </>
    ),
    (k) => <>Serveer de map in {k.code('lambda.cs')}:</>,
    (k) => (
      <>
        Klik op {k.b('Deployen')}. {k.code('site/index.html')} antwoordt op {k.code('/')}, {k.code('site/app.css')} op{' '}
        {k.code('/app.css')}, en elk adres dat bij geen enkel bestand past, krijgt de pagina als antwoord. Zo blijft een
        frontend met eigen routing werken als iemand een deeplink herlaadt.
      </>
    ),
    () => <>Zet er een API naast, dan heeft de pagina iets om mee te praten:</>,
  ],

  storage: (k) => (
    <>
      Een lambda bewaart bestanden op twee plekken, en de editor toont ze apart: {k.b('Bestanden')} bevat de
      bestanden van een versie (het programma), en {k.b('Data')} bevat de workspace (wat het programma bewaart). Het
      verschil zit in {k.em('van wie ze zijn')}. De bestanden van een versie horen bij die versie; de data hoort bij de
      lambda, en elke versie deelt die.
    </>
  ),
  savedWithCode: 'In een versie',
  workspaceColumn: 'In de data',
  table: [
    ['wat erin staat', 'de code en assets: het programma, frontend inbegrepen – en de documentatie en tests ervan', 'alles wat de lambda wegschrijft of iemand uploadt'],
    ['wanneer het verandert', 'nooit: een wijziging is een nieuwe versie', 'zodra er iets naar wordt geschreven'],
    ['een deploy', 'zet precies deze bestanden online', 'raakt het nooit aan'],
    ['terugzetten', 'haalt de oude bestanden terug', 'geen effect: elke versie deelt het'],
    ['een concept', 'begint als kopie ervan', 'werkt met een kopie ervan'],
    ['wanneer het verdwijnt', 'met oude versies, boven de limiet', 'met de lambda, of als je het uitzet'],
  ],
  reachedAs: 'in code te bereiken als',
  storageAside:
    'Het kan niet één en dezelfde plek zijn. Dan zou een deploy alles wissen wat je lambda sindsdien had weggeschreven, of zou er nooit iets weg kunnen uit wat hij meelevert. Een spel met een ranglijst wil het tweede; de pagina die het serveert wil het eerste. Dus de pagina gaat in de versie, en de ranglijst in de data.',

  database: (k) => (
    <>
      Records – berichten, accounts, bestellingen, stemmen – horen in de {k.b('database')}: een eigen
      SQLite-database van de lambda, die je aanzet onder {k.b('Data')}. De code opent een verbinding met{' '}
      {k.code('Database.GetConnection()')} en praat er in SQL mee:
    </>
  ),
  database2: (k) => (
    <>
      De tabellen worden gemaakt door {k.b('migraties')}: SQL-bestanden die met de versie meekomen in{' '}
      {k.code('migrations/')}, en die {k.link('https://evolve-db.netlify.app/', 'Evolve')} op volgorde toepast als de
      lambda start – elk één keer, dus een nieuwe versie voert alleen uit wat nieuw is. Verander nooit een migratie die
      al is toegepast; een wijziging aan een tabel is het volgende bestand.
    </>
  ),
  database3: (k) => (
    <>
      Zoals alle data wordt de database gedeeld door elke versie, laten deploys en terugzetten hem met rust, en werkt
      een concept op een kopie. Onder {k.b('Data')} zie je de tabellen en wat erin staat – de eenvoudige weergave noemt
      ze items. {k.b('Downloaden als .NET-project')} neemt hem mee als gewoon SQLite-bestand.
    </>
  ),
  databaseAside: (k) => (
    <>
      Open een verbinding waar je hem nodig hebt en sluit hem daarna weer, en gebruik hem synchroon:{' '}
      {k.code('ExecuteReader')}, niet {k.code('ExecuteReaderAsync')}. Waarden gaan erin als parameters, nooit in de SQL
      zelf. De demo {k.link('/editor/demo-crud', 'demo-crud')} doet het allemaal voor.
    </>
  ),

  keeping: (k) => (
    <>
      {k.code('Workspace')} is een privémap waarin je lambda mag lezen en schrijven: de plek voor bestanden – foto's
      die iemand uploadt, een document dat hij maakt, een model dat hij laadt. Records horen in de database, en wat je
      over een bestand weet – wie het uploadde, en wanneer – is ook een record.
    </>
  ),
  keeping2: (k) => (
    <>
      Er zijn ook {k.code('ReadBytes')}, {k.code('WriteBytes')}, {k.code('Delete')}, {k.code('List')},{' '}
      {k.code('CreateFolder')}, en {k.code('Tree')}/{k.code('Files')}/{k.code('App')} om hem te serveren. Verder is
      niets op het bestandssysteem bereikbaar.
    </>
  ),

  secrets: (k) => (
    <>
      Een API-sleutel, een wachtwoord of een token hoort in de {k.b('secrets')}, niet in de code – waar elke versie,
      elke download en iedereen die de geschiedenis leest hem zou hebben. De code leest een secret op naam:
    </>
  ),
  secrets2: (k) => (
    <>
      Zet secrets aan onder {k.b('Data')} en stel de waarde daar in. Eenmaal opgeslagen wordt hij nooit meer getoond –
      niet aan jou en niet aan een agent; je kunt hem alleen vervangen. De lijst laat zien welke namen de code leest
      waarvoor nog geen waarde is ingesteld, en het overzicht vraagt erom. {k.code('Secret.Exists')} zegt of er een is
      ingesteld, voor code die ook zonder kan. Zoals alle data delen alle versies de secrets, en een concept werkt op
      een kopie.
    </>
  ),
  secretsAside: (k) => (
    <>
      Ze worden versleuteld opgeslagen, met een sleutel die niet in de database staat. In een gedownload project leest{' '}
      {k.code('Secret.Read("NAME")')} de omgevingsvariabele {k.code('NAME')} – de waarden zelf blijven hier.
    </>
  ),

  sockets: (k) => (
    <>
      Ondersteund, en niet als bijzaak. De demo {k.link('/editor/demo-game', 'demo-game')} koppelt spelers aan elkaar
      en laat elk spel op de server draaien. De simpelste vorm is drie callbacks:
    </>
  ),
  socketsAside: (k) => (
    <>
      Waar iedereen in trapt: een browser kan geen headers meesturen bij een websocket-handshake. Geef wat de handler
      nodig heeft mee in de query, waar hij het leest uit {k.code('connection.Request.Header.Query')}, of stuur
      geheimen als eerste bericht.
    </>
  ),

  limits:
    'Je code draait op een gedeelde server, dus een deel van C# wordt al vóór het compileren geweigerd: processen starten, eigen sockets openen, assemblies laden, het bestandssysteem buiten je workspace benaderen, en reflection om daar omheen te komen.',
  limits2:
    'Al het andere is er, inclusief de hele module-API van GenHTTP. Wordt er iets geweigerd, dan hoor je welke regel en waarom, niet alleen dat het misging.',

  away: (k) => (
    <>
      Met {k.b('Downloaden als .NET-project')} in de editor krijg je alles mee: een solution die je kunt openen,
      kunt draaien met {k.code('dotnet run')} en mag houden. Hij heeft alleen het GenHTTP-package nodig, en er zit een{' '}
      {k.code('Dockerfile')} bij om hem als container te bouwen en te draaien.
    </>
  ),
  away2: (k) => (
    <>
      Je snippet wordt {k.code('Project.cs')}, en {k.code('Program.cs')} serveert wat hij teruggeeft. Je andere
      bestanden komen precies mee zoals je ze schreef. {k.code('Workspace')} en {k.code('Assets')} worden twee mappen
      naast het programma, met dezelfde methodes, apart in een map {k.code('Platform')} - dus er hoeft niets in je code
      te veranderen.
      {' '}{k.code('Secret')} leest daar omgevingsvariabelen met dezelfde naam; de waarden blijven hier. De
      documentatie en de tests komen mee in {k.code('docs')} en {k.code('tests')}.
      {' '}{k.code('Database')} opent {k.code('database/database.db')}, dat de download meelevert met de records die
      je app bewaarde.
    </>
  ),
  awayAside:
    'Goed om te weten voordat je hier iets bouwt: wat je schrijft is van jou, en je neemt het in zijn geheel mee. Dat je code hier draait, betekent niet dat hij hier vastzit.',

  open: (k) => (
    <>
      Kan wat je gebouwd hebt iemand anders helpen, publiceer dan de code: open {k.b('Open source')} in het
      dashboard, kies een licentie – MIT, tenzij je een andere wilt – en zet het aan. De code krijgt een eigen pagina
      tussen de {k.link('/source', 'open-source-apps')}, waar iedereen hem kan lezen, een ster kan geven en elke versie
      kan downloaden als hetzelfde project dat {k.b('Downloaden als .NET-project')} je geeft, met de licentie erbij.
    </>
  ),
  open2: () => (
    <>
      Elke versie wordt gepubliceerd, ook de eerdere, met de documentatie, de tests en de wijziging die elke versie
      maakte. Wat de app bewaart, wordt nooit gepubliceerd – de records, de bestanden die hij opsloeg, de waarden van
      zijn sleutels en wachtwoorden – en ook niet wat je in je eigen woorden vroeg, of wie de app gebruikt. Zet je het
      uit, dan is de pagina weg; de sterren blijven bewaard voor als je de code opnieuw publiceert.
    </>
  ),
  openAside:
    'Alles in de code wordt openbaar, de eerdere versies inbegrepen. Een sleutel of wachtwoord hoort bij de sleutels en wachtwoorden onder Data, nooit in de code – gepubliceerd of niet.',

  agents: (k) => (
    <>
      Er is een MCP-endpoint op {k.code('/mcp')}. Koppel er een agent aan en hij kan alles wat de editor kan: de
      handleiding lezen, een demo helemaal lezen, bestanden schrijven, ze compileren en deployen. Eronder zit dezelfde API.
    </>
  ),
  agents2: (k) => (
    <>
      Onderweg legt hij uit waarom: {k.code('write_code')} krijgt de specificatie en de wijziging mee. En hij kan bekijken
      wat hij heeft gedeployd: {k.code('read_logs')} geeft de recente requests van de lambda, de output en de
      stacktrace van elke exception. Zo controleert een agent of zijn code werkt, in plaats van het aan te nemen.
      Jij ziet hetzelfde in het dashboard. Hij schrijft de documentatie en de tests terwijl hij werkt, leest ze voordat
      hij iets verandert, en voert de tests uit op het adres van een concept voordat hij het concept online zet.
    </>
  ),
  more: 'Meer daarover →',
  make: 'Maak er zelf een',
};
