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
    features: 'Veilig aanpassen',
    files: 'Meer dan één bestand',
    page: 'Een pagina serveren',
    spa: 'Een frontend, stap voor stap',
    storage: 'De twee plekken voor bestanden',
    keeping: 'Data bewaren',
    sockets: 'Websockets',
    limits: 'Wat niet mag',
    away: 'Alles meenemen',
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
    ['Overzicht', () => <>Of hij online is, hoeveel requests hij vandaag had en hoeveel daarvan misgingen, de laatste wijziging, en hoeveel ruimte er nog over is.</>],
    [
      'Aanpassen',
      (k) => (
        <>
          Zeg wat er anders moet, en de agent op deze server doet het terwijl jij meekijkt. Hij werkt in een concept,
          probeert het daar uit en voegt het samen tot de volgende versie zodra het werkt. Zet{' '}
          {k.b('Online zetten als het klaar is')} uit als je het concept eerst zelf wilt uitproberen.
        </>
      ),
    ],
    ['Concepten', () => <>Wijzigingen waaraan naast de lambda wordt gewerkt: elk concept probeer je uit op een eigen adres, en het wordt samengevoegd tot de volgende versie zodra het goed is. Open je een concept, dan heeft het zijn eigen code, data en logs.</>],
    ['Bestanden', () => <>De bestanden van een versie: de code en assets, het programma zelf. Een slotje of een wereldbol laat zien of ze openbaar bereikbaar zijn.</>],
    ['Data', () => <>Wat de lambda bewaart terwijl hij draait, gedeeld door elke versie: de workspace. Kijk erin, upload en verwijder bestanden, of zet hem uit.</>],
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
  ],
  sections: (k) => (
    <>
      Elk onderdeel werkt hetzelfde: een titel, een {k.b('ⓘ')} met uitleg, acties rechts en, als er meer dan één
      weergave is, een rij tabs eronder. Bij de code zijn de tabs de bestanden.
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
    change: 'Bewaart berichten in de workspace, zodat ze een herstart overleven',
  },
  why2: (k) => (
    <>
      Agents geven dezelfde twee velden mee aan {k.code('write_code')}. In {k.b('Code')} wordt bij het opslaan om de
      wijziging gevraagd. Beide zijn optioneel. Een lange specificatie wordt afgekapt op 4000 tekens en een wijziging
      op 500, in plaats van geweigerd. Een concept heeft zijn eigen twee, en de versie waarin het wordt samengevoegd,
      neemt ze over.
    </>
  ),

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
        Start het onder {k.b('Concepten')}, of vanuit een willekeurige versie. Het is een kopie van de code en assets
        van die versie, en van de data van de lambda.
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
    ['wat erin staat', 'de code en assets: het programma, frontend inbegrepen', 'alles wat de lambda wegschrijft of iemand uploadt'],
    ['wanneer het verandert', 'nooit: een wijziging is een nieuwe versie', 'zodra er iets naar wordt geschreven'],
    ['een deploy', 'zet precies deze bestanden online', 'raakt het nooit aan'],
    ['terugzetten', 'haalt de oude bestanden terug', 'geen effect: elke versie deelt het'],
    ['een concept', 'begint als kopie ervan', 'werkt met een kopie ervan'],
    ['wanneer het verdwijnt', 'met oude versies, boven de limiet', 'met de lambda, of als je het uitzet'],
  ],
  reachedAs: 'in code te bereiken als',
  storageAside:
    'Het kan niet één en dezelfde plek zijn. Dan zou een deploy alles wissen wat je lambda sindsdien had weggeschreven, of zou er nooit iets weg kunnen uit wat hij meelevert. Een spel met een ranglijst wil het tweede; de pagina die het serveert wil het eerste. Dus de pagina gaat in de versie, en de ranglijst in de data.',

  keeping: (k) => (
    <>
      {k.code('Workspace')} is een privémap waarin je lambda mag lezen en schrijven. Hier hoort alles wat langer moet
      bestaan dan een request, of een deployment.
    </>
  ),
  keeping2: (k) => (
    <>
      Er zijn ook {k.code('ReadBytes')}, {k.code('WriteBytes')}, {k.code('Delete')}, {k.code('List')},{' '}
      {k.code('CreateFolder')}, en {k.code('Tree')}/{k.code('Files')}/{k.code('App')} om hem te serveren. Verder is
      niets op het bestandssysteem bereikbaar.
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
    </>
  ),
  awayAside:
    'Goed om te weten voordat je hier iets bouwt: wat je schrijft is van jou, en je neemt het in zijn geheel mee. Dat je code hier draait, betekent niet dat hij hier vastzit.',

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
      Jij ziet hetzelfde in het dashboard.
    </>
  ),
  more: 'Meer daarover →',
  make: 'Maak er zelf een',
};
