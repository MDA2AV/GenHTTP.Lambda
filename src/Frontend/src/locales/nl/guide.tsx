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
    files: 'Code en resources',
    page: 'Een pagina serveren',
    spa: 'Een frontend, stap voor stap',
    built: 'Waaruit het is gebouwd',
    storage: 'Een versie en zijn data',
    database: 'Records bewaren',
    keeping: 'Bestanden bewaren',
    secrets: 'Sleutels en wachtwoorden',
    sockets: 'Websockets',
    limits: 'Wat niet mag',
    away: 'Alles meenemen',
    git: 'Werken met git',
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
        Dat is een complete lambda. Gedeployd beantwoordt hij op een eigen adres, vernoemd naar zijn sleutel, zoals{' '}
        {k.code('your-key.genhttp.run')}, elk request met het woord hello.
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
          Zeg wat er anders moet, en de agent op deze server doet het terwijl jij meekijkt. Hij probeert de wijziging uit
          in een concept – een kopie met een eigen adres – en zet het online zodra het werkt. Zet{' '}
          {k.b('Online zetten als het klaar is')} uit als je het concept eerst zelf wilt uitproberen.
          Hij werkt alleen aan je app: een verzoek dat er niets mee te maken heeft, of dat schade moet aanrichten, wijst hij af, en hij zegt waarom.
        </>
      ),
    ],
    ['Concepten', () => <>Wijzigingen die worden uitgeprobeerd voordat ze online gaan, elk op een eigen adres en met eigen testdata. Open je een concept, dan heeft het zijn eigen code, testdata en logs. Het onderdeel verschijnt zodra er een concept is.</>],
    ['Data', () => <>Wat de lambda bewaart terwijl hij draait, gedeeld door elke versie: de database, de workspace en de sleutels en wachtwoorden, elk met een eigen tabblad. Bekijk de tabellen en bestanden, upload bestanden, stel sleutels en wachtwoorden in of zet een soort aan of uit. De eenvoudige weergave toont het zodra de app iets bewaart.</>],
    ['Versies', () => <>Wat elke versie veranderde en wat er gevraagd werd, en het verschil met de vorige. Van hieruit deploy je of zet je een versie terug, en vanuit elke versie kun je een concept starten.</>],
    ['Deployments', () => <>Wat wanneer online stond, en waardoor het offline ging.</>],
    ['Statistieken', () => <>Requests, fouten, responstijden en de meest opgevraagde paden, over het afgelopen uur of de afgelopen dag.</>],
    ['Logs', () => <>Requests, output en de stacktrace van alles wat misging, live.</>],
    [
      'Code',
      (k) => (
        <>
          Elk bestand van een versie, de code en de resources, in een boom naast de editor – met hoeveel ruimte ze
          innemen en of het publiek erbij kan. Kies erboven een oudere versie om die te lezen.{' '}
          {k.b('Controleren')} compileert, {k.b('Opslaan')} maakt een versie, {k.b('Deployen')} zet hem online. In een
          concept bewaart {k.b('Opslaan')} de code in het concept en toont die op het adres van het concept.{' '}
          {k.code('Ctrl-S')} slaat op; {k.code('F12')} springt naar een declaratie.
        </>
      ),
    ],
    ['Tests', () => <>Hoe de app automatisch getest wordt, met de scripts en testdata daarvoor. Alleen in de volledige weergave.</>],
  ],
  sections: (k) => (
    <>
      Elk onderdeel werkt hetzelfde: een titel, een {k.b('ⓘ')} met uitleg, acties rechts en, als er meer dan één
      weergave is, een rij tabs eronder. De volledige weergave deelt de onderdelen in groepen in: hoe mensen hem
      vinden, waar een wijziging gemaakt wordt, de versies en de data, en hoe hij draait.
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
    ['docs/product.md', 'wat de app is, voor wie hij is, wat mensen ermee doen en waarom'],
    ['docs/decisions.md', 'de technische beslissingen, en waarom ze genomen zijn'],
    ['tests/README.md', 'hoe de app automatisch getest wordt, en hoe je de tests uitvoert'],
    ['tests/…', 'de scripts en testdata die de tests gebruiken'],
  ],
  written2: (k) => (
    <>
      Het zijn bestanden van de code van de versie zoals alle andere, in de mappen {k.code('docs')} en {k.code('tests')}:
      de geschiedenis laat zien wat een versie erin veranderde, terugzetten haalt de documentatie terug die voor die
      versie gold, en een concept heeft een eigen kopie die met het concept mee online gaat. Ze worden nooit
      gecompileerd en nooit geserveerd, en tellen mee voor de ruimte die een versie mag innemen.
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
      waard: je kunt ze allemaal vergelijken en precies zoals ze waren weer online zetten. Wil je een lambda
      aanpassen die mensen gebruiken, probeer de wijziging dan eerst uit in een {k.b('concept')}.
    </>
  ),
  featureSteps: [
    (k) => (
      <>
        Start het vanuit een willekeurige versie onder {k.b('Versies')}, of laat de agent er een starten. Het is een
        kopie van de code en de resources van die versie, met de documentatie en tests erbij, en van de data van de
        lambda.
      </>
    ),
    (k) => (
      <>
        Pas het zo vaak aan als nodig, in {k.b('Code')} of door het aan de agent te vragen. De voorvertoning draait op
        een eigen adres, {k.code('/features/…/')}, met eigen testdata. Bezoekers van de lambda zien er niets van, en
        niets wat het wegschrijft, komt in de data van de lambda terecht.
      </>
    ),
    (k) => (
      <>
        Klik op {k.b('Online zetten')} zodra het goed is: het wordt de volgende versie, met zijn notities, en gaat
        online. Het concept verdwijnt dan, met zijn voorvertoning en zijn testdata.
      </>
    ),
  ],
  featureSample: 'Ranglijst',
  featuresAside: () => (
    <>
      Je kunt aan meerdere concepten tegelijk werken. Alleen een concept dat up-to-date is met de nieuwste versie
      kan online gaan, zodat het nooit een versie ongedaan maakt die is opgeslagen nadat het concept begon. Is er
      eerst een ander online gezet, haal dan de wijzigingen daarvan binnen - of vraag de agent dat te doen - en
      markeer het concept als bijgewerkt. Niets gaat vanzelf online; dat is met opzet. De API noemt een concept een
      feature, en het online zetten een merge.
    </>
  ),

  files: (k) => (
    <>
      Een versie bestaat uit een willekeurig aantal bestanden, in twee delen. De {k.b('code')} is elk bestand behalve de
      resources: de {k.code('.cs')}-bestanden worden gecompileerd, in elke map, zoals in elk C#-project, en elk ander
      bestand wordt bij de versie bewaard en nooit gecompileerd of geserveerd: de documentatie, de tests, waaruit een
      front end wordt gebouwd. De {k.b('resources')}, in {k.code('resources/')}, zijn wat de versie leest en serveert
      terwijl hij draait – pagina’s, scripts, stijlen, afbeeldingen, de migraties van de database – en zijn vanuit de
      code bereikbaar als {k.code('Resources')}.
    </>
  ),
  files2: (k) => (
    <>
      Types hoeven niet onder de code te staan die ze gebruikt. Klik in {k.b('Code')} op {k.b('+')} naast de code en
      typ een naam: een {k.code('.cs')}-bestand wordt naast de snippet gecompileerd, in dezelfde namespace, in welke map
      het ook staat, dus je hoeft niets te importeren om erbij te kunnen. Een naam zonder extensie en zonder map wordt
      als C# gezien.
    </>
  ),
  filesAside:
    'Hoe de code is ingedeeld, is aan wie hem schrijft – een map voor types, een voor de bronnen van een front end, een voor scripts. Elk .cs-bestand wordt in de app gecompileerd, dus C# die er geen deel van uitmaakt – een test, een eigen tool – hoort niet als .cs-bestand in de code. De code en de resources van een versie delen één hoeveelheid ruimte, die het overzicht laat zien.',

  page: 'Er zijn twee manieren om een pagina te serveren, en nog een voor wat mensen ernaast uploaden.',
  inlineTitle: 'Eén pagina, inline geschreven',
  inline: 'Prima voor iets kleins. De pagina zit in de snippet zelf.',
  folderTitle: 'Een map met echte bestanden',
  folder:
    'De juiste keuze voor alles met een stylesheet en een script. De bestanden zijn resources van de versie en worden precies zo geserveerd als je ze schreef. Er wordt niets gecompileerd.',
  workspaceTitle: 'Geüploade bestanden, uit de data',
  workspace:
    'Voor wat mensen uploaden of wat de lambda aanmaakt, zoals foto’s en documenten, geserveerd naast de app. Niet voor de pagina’s van de app zelf: die horen in de resources, zodat ze in dezelfde versie zitten als de code die ze nodig heeft.',

  spa: (k) => (
    <>
      De tweede manier, helemaal uitgewerkt. Elke demo serveert zijn pagina zo, vanuit {k.code('resources/web')}. Open{' '}
      {k.link('/editor/demo-crud', 'demo-crud')} om er een te bekijken. Demo’s zijn alleen-lezen; hun
      editorsleutel is hun naam.
    </>
  ),
  spaSteps: [
    (k) => (
      <>
        Klik in {k.b('Code')} op {k.b('+')} naast de resources en typ {k.code('site/index.html')}: het wordt{' '}
        {k.code('resources/site/index.html')}. Een slash in de naam zet het bestand in een map; de extensie bepaalt wat
        voor bestand het is.
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
        je op de uploadknop naast de resources: het komt in dezelfde map terecht. Een PNG kun je niet in een
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
  built: (k) => (
    <>
      Een deel van een lambda kan door een buildtool worden gemaakt in plaats van te worden geschreven zoals het wordt
      geserveerd of gecompileerd: gecompileerd, gebundeld of gegenereerd. De versie bevat wat de tool maakt – als
      resources of als code – en de bestanden waaruit de tool het maakt, horen bij de code, in een eigen map:{' '}
      {k.code('frontend/')} bijvoorbeeld, met een README die zegt hoe het wordt gebouwd. Jouw agent wijzigt die
      bestanden, draait de build op de plek waar hij werkt en slaat beide op in dezelfde versie. Dit platform bouwt
      niets.
    </>
  ),
  built2: (k) => (
    <>
      Net als de documentatie horen ze bij de versie: ze worden vergeleken in de geschiedenis, teruggezet,
      gekopieerd naar een concept, gekloond, gedownload en samen met de rest van de code gepubliceerd – en nooit
      gecompileerd of geserveerd. In het dashboard staan ze in {k.b('Code')}, samen met elk ander bestand van de
      versie.
    </>
  ),
  builtAside:
    'Wat wordt geschreven zoals het wordt geserveerd of gecompileerd, heeft dat niet nodig. Wat een build installeert of voor zichzelf bewaart – node_modules bijvoorbeeld – hoort nooit bij een versie: een .gitignore in de eigen map ervan houdt het erbuiten.',

  storage: (k) => (
    <>
      Een lambda bewaart bestanden op twee plekken, en de editor toont ze apart: {k.b('Code')} bevat de
      bestanden van een versie (het programma), en {k.b('Data')} bevat de workspace (wat het programma bewaart). Het
      verschil zit in {k.em('van wie ze zijn')}. De bestanden van een versie horen bij die versie; de data hoort bij de
      lambda, en elke versie deelt die. Elk heeft één gedeelde hoeveelheid ruimte: de code en de resources van een versie delen er
      één, en de database en de workspace van de lambda delen de andere.
    </>
  ),
  savedWithCode: 'In een versie',
  workspaceColumn: 'In de data',
  table: [
    ['wat erin staat', 'de code en de resources: het programma, frontend inbegrepen – en de documentatie, de tests en waaruit het ook is gebouwd', 'alles wat de lambda wegschrijft of iemand uploadt'],
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
      {k.code('Database.GetConnection()')} en leest en schrijft erin via{' '}
      {k.link('https://learn.microsoft.com/ef/core/', 'Entity Framework Core')}, met een eigen context die de tabellen
      mapt:
    </>
  ),
  database2: (k) => (
    <>
      De tabellen worden gemaakt door {k.b('migraties')}: SQL-bestanden die met de versie meekomen in{' '}
      {k.code('resources/migrations/')}, en die {k.link('https://evolve-db.netlify.app/', 'Evolve')} op volgorde toepast als de
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
      Maak een context waar je hem nodig hebt en ruim hem daarna weer op, en gebruik hem synchroon:{' '}
      {k.code('ToList')} en {k.code('SaveChanges')}, niet {k.code('ToListAsync')} en {k.code('SaveChangesAsync')}. De
      tabellen worden gemaakt door de migraties, nooit door Entity Framework. De demo{' '}
      {k.link('/editor/demo-crud', 'demo-crud')} doet het allemaal voor.
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
  sockets2: (k) => (
    <>
      Als de pagina alleen luistert - een teller, een feed, een scorebord - zijn server-sent events eenvoudiger: één
      lang antwoord waar de server steeds in schrijft en waarmee de browser zelf opnieuw verbindt. De{' '}
      {k.link('/editor/demo-live', 'demo-live')}-demo stuurt zo elke stem naar iedereen die meekijkt. In beide gevallen
      pusht de server wat er veranderd is. Een pagina die om de paar seconden opnieuw vraagt, stuurt elke keer een
      request, of er nu iets veranderd is of niet, en loopt toch achter.
    </>
  ),

  limits:
    'Je code draait op een gedeelde server, dus een deel van C# wordt al vóór het compileren geweigerd: processen starten, eigen sockets openen, assemblies laden, het bestandssysteem buiten je workspace benaderen, en reflection om daar omheen te komen. Net als wachten op een task met .Result of .Wait() in plaats van await: requests draaien op één thread per core, en de task zou moeten afronden op precies de thread die erop wacht.',
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
      bestanden komen precies mee zoals je ze schreef, op dezelfde plek – de resources in {k.code('resources')}, de
      documentatie in {k.code('docs')}, de tests in {k.code('tests')}. {k.code('Workspace')} en{' '}
      {k.code('Resources')} worden twee mappen naast het programma, met dezelfde methodes, apart in een map{' '}
      {k.code('Platform')} - dus er hoeft niets in je code te veranderen.
      {' '}{k.code('Secret')} leest daar omgevingsvariabelen met dezelfde naam; de waarden blijven hier.
      {' '}{k.code('Database')} opent {k.code('database/database.db')}, dat de download meelevert met de records die
      je app bewaarde.
    </>
  ),
  awayAside:
    'Goed om te weten voordat je hier iets bouwt: wat je schrijft is van jou, en je neemt het in zijn geheel mee. Dat je code hier draait, betekent niet dat hij hier vastzit.',
  git: (k) => (
    <>
      Elke lambda is ook een git-repository. {k.b('Klonen')}, in het overzicht van het dashboard en naast de code, toont
      het adres - het adres van je editor met de naam van de app erachter - en {k.code('git clone')} geeft je het
      project dat {k.b('Downloaden')} je geeft, met van elke versie een commit van {k.code('main')}, getagd als{' '}
      {k.code('v1')}, {k.code('v2')} enzovoort, en van elk concept een branch. Open het in je eigen editor, geef het aan
      je coding agent, voer het uit met {k.code('dotnet run')}.
    </>
  ),
  git2: (k) => (
    <>
      Push en het staat hier. Elke commit die je naar {k.code('main')} pusht wordt de volgende versie, met de eerste
      regel als de wijziging die hij doorvoert - eerst gecompileerd, en geweigerd als dat niet lukt - en{' '}
      {k.code('git push -o deploy')} zet hem online. Een branch die je pusht wordt een concept, met de voorvertoning
      online op een eigen adres; push hem naar {k.code('main')}, of voeg {k.code('-o merge')} toe aan de laatste push,
      en hij is de volgende versie. Wat het platform om je code heen zet om er een project van te maken -{' '}
      {k.code('Program.cs')}, het projectbestand, {k.code('Platform')} - hoort niet bij je app, dus een push die dat
      wijzigt wordt geweigerd, met uitleg waarom. {k.code('AGENTS.md')} in de repository vertelt een coding agent de
      rest.
    </>
  ),
  gitAside:
    'Het adres bevat je editorsleutel, net als het adres van de editor: wie het heeft, kan pushen. Wat je app bewaart - de items, bestanden, sleutels en wachtwoorden - staat nooit in de repository.',

  open: (k) => (
    <>
      Kan wat je gebouwd hebt iemand anders helpen, publiceer dan de code: open {k.b('Open source')} in het
      dashboard, kies een licentie – MIT, tenzij je een andere wilt – en zet het aan. De code krijgt een eigen pagina
      tussen de {k.link('/source', 'open-source-apps')}, waar iedereen hem kan lezen, een ster kan geven, elke versie
      kan downloaden als hetzelfde project dat {k.b('Downloaden')} je geeft, met de licentie erbij, of elke versie met
      git kan klonen.
    </>
  ),
  open2: () => (
    <>
      Elke versie wordt gepubliceerd, ook de eerdere, met al zijn code – de documentatie en de tests daarbij inbegrepen – en de
      wijziging die elke versie maakte. Wat de app bewaart, wordt nooit gepubliceerd – de records, de bestanden die hij
      opsloeg, de waarden van zijn sleutels en wachtwoorden – en ook niet wat je in je eigen woorden vroeg, of wie de
      app gebruikt. Zet je het uit, dan is de pagina weg; de sterren blijven bewaard voor als je de code opnieuw
      publiceert.
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
      hij iets verandert, en voert de tests uit op het adres van een concept voordat hij het concept online zet. Een
      pagina die gevonden moet worden, krijgt een titel, een beschrijving, een icoon en een preview voor als iemand de
      link deelt. Onderaan de pagina’s die hij bouwt, zet hij een klein regeltje dat ze met GenHTTP Lambda zijn gemaakt -
      zeg het hem als je dat liever niet wilt, dan haalt hij het weg.
    </>
  ),
  more: 'Meer daarover →',
  make: 'Maak er zelf een',
};
