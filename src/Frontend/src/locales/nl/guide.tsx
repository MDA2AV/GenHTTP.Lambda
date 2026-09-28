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
        {k.link('/#agents', 'MCP')}. Of open {k.b('Code')} en schrijf het zelf: {k.b('Controleren')} compileert
        zonder iets op te slaan en laat zien wat de compiler ervan vindt, met bestand en regel.
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
          Zeg wat er anders moet, en de agent op deze server doet het terwijl jij meekijkt: hij leest de code, past hem
          aan, controleert of alles compileert en zet het online als nieuwe versie. Zet{' '}
          {k.b('Online zetten als het klaar is')} uit als je het eerst wilt bekijken.
        </>
      ),
    ],
    ['Bestanden', () => <>De bestanden van een versie, en de data: wat de lambda opslaat terwijl hij draait. Een slotje of een wereldbol laat zien of ze openbaar bereikbaar zijn.</>],
    ['Versies', () => <>Wat elke versie veranderde en wat er gevraagd werd, en het verschil met de vorige. Van hieruit deploy je of zet je een versie terug.</>],
    ['Deployments', () => <>Wat wanneer online stond, en waardoor het offline ging.</>],
    ['Statistieken', () => <>Requests, fouten, responstijden en de meest opgevraagde paden, over het afgelopen uur of de afgelopen dag.</>],
    ['Logs', () => <>Requests, output en de stacktrace van alles wat misging, live.</>],
    [
      'Code',
      (k) => (
        <>
          Zelf schrijven. {k.b('Controleren')} compileert, {k.b('Opslaan')} maakt een versie, {k.b('Deployen')} zet
          hem online. {k.code('Ctrl-S')} slaat op; {k.code('F12')} springt naar een declaratie.
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
      Agents geven dezelfde twee velden mee aan {k.code('write_code')}. In {k.b('Code')} vraagt opslaan om de
      wijziging. Beide zijn optioneel. Een lange specificatie wordt afgekapt op 4000 tekens en een wijziging op 500,
      in plaats van geweigerd.
    </>
  ),

  files: (k) => (
    <>
      Types hoeven niet onder de code te staan die ze gebruikt. Klik in {k.b('Code')} op {k.b('+')} naast de
      bestanden. Het nieuwe bestand wordt naast de snippet gecompileerd, in dezelfde namespace, dus je hoeft niets te
      importeren om erbij te kunnen. Een naam zonder extensie wordt als C# gezien.
    </>
  ),

  page: 'Er zijn drie manieren. Welke je wilt, hangt af van waar de pagina staat.',
  inlineTitle: 'Eén pagina, inline geschreven',
  inline: 'Prima voor iets kleins. De pagina zit in de snippet zelf.',
  folderTitle: 'Een map met echte bestanden',
  folder:
    'De juiste keuze voor alles met een stylesheet en een script. Je voegt de bestanden toe zoals een C#-bestand, en ze worden precies zo geserveerd als je ze schreef. Er wordt niets gecompileerd.',
  workspaceTitle: 'Vanuit de workspace',
  workspace: 'Voor een pagina die je uploadt in plaats van schrijft, en die je wilt aanpassen zonder opnieuw te deployen.',

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
      Het onderdeel {k.b('Bestanden')} laat ze allebei zien: de bestanden van een versie, en de workspace als{' '}
      {k.b('Data')}. Je ziet ook welke openbaar bereikbaar zijn. Codebestanden pas je aan in {k.b('Code')}; data kun
      je uploaden en verwijderen in {k.b('Bestanden')}. Toch zijn het niet hetzelfde, en het verschil zit in{' '}
      {k.em('wanneer ze veranderen')}.
    </>
  ),
  savedWithCode: 'Opgeslagen met je code',
  workspaceColumn: 'Workspace',
  table: [
    ['wat erin staat', 'elk bestand van je lambda, de C# inbegrepen', 'alles wat is weggeschreven of geüpload'],
    ['wanneer het verandert', 'als je op Opslaan of Deployen klikt', 'zodra er iets naar wordt geschreven'],
    ['een deploy', 'vervangt alles', 'raakt het nooit aan'],
    ['een versie terugzetten', 'haalt de oude bestanden terug', 'geen effect'],
    ['de lambda klonen', 'gaat mee', 'gaat niet mee'],
  ],
  reachedAs: 'in code te bereiken als',
  storageAside:
    'Het kan niet één en dezelfde map zijn. Dan zou een deploy alles wissen wat je lambda sindsdien had weggeschreven, of zou er nooit iets weg kunnen uit wat hij meelevert. Een spel met een ranglijst wil het tweede; de pagina die het serveert wil het eerste.',

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
      kunt draaien met {k.code('dotnet run')} en mag houden. Er zit één package reference in en geen spoor van dit platform.
    </>
  ),
  away2: (k) => (
    <>
      Je snippet wordt de body van {k.code('Program.cs')}, in een host die serveert wat hij teruggeeft. Je andere
      bestanden komen precies mee zoals je ze schreef. {k.code('Workspace')} en {k.code('Assets')} worden twee mappen
      naast de code, met dezelfde methodes, dus er hoeft niets in je code te veranderen.
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
