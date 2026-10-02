import type { Messages } from '../en';

export const ship: Messages['ship'] = {
  eyebrow: 'Gratis hosting voor vibe-coded apps',
  title: 'Van localhost naar ieders scherm.',
  intro:
    'Je hebt een app gebouwd met Claude Code, Codex of Cursor, maar hij draait alleen op je eigen computer. Vraag je agent om hem hier online te zetten. Een paar minuten later heeft hij een openbare link die iedereen kan openen, een eigen database en een live verbinding met iedereen die hem open heeft. Mensen kunnen er dus samen spelen, chatten en posten.',
  facts: ['Gratis', 'Geen account', 'Geen creditcard', 'Niets te installeren'],
  connect: 'Koppel je agent',
  seeOthers: 'Bekijk wat anderen bouwden',

  stepsTitle: 'Je app online zetten in drie stappen',
  step: (n) => `Stap ${n}`,
  steps: [
    {
      title: 'Eén keer koppelen',
      body: 'Voeg één adres toe, een remote MCP-server, aan Claude Code, Codex, Cursor of welke agent je ook gebruikt. Dat kost minder dan een minuut, en je doet het maar één keer.',
    },
    {
      title: 'Laat hem publiceren',
      body: 'Zeg dat hij de app hier online moet zetten. Hij pakt je app in, deployt hem en checkt of hij reageert. Geen GitHub-repo, geen deploy-pipeline, geen Docker.',
    },
    {
      title: 'Deel de link',
      body: 'Je krijgt een openbaar adres en een editorlink die alleen voor jou is. Het adres stuur je naar wie je maar wilt. De editorlink houd je zelf: daarmee pas je de app later aan.',
    },
  ],

  togetherTitle: 'Niet alleen hosting. Een database en multiplayer, ingebouwd.',
  together:
    'De meeste hosts geven elke bezoeker een eigen kopie van je app, en iedereen speelt in zijn eentje: wat de ene browser in localStorage bewaart, ziet de volgende nooit. Hier heeft elke app een eigen database en een live verbinding met iedereen die hem open heeft. Doet iemand een zet, dan zien alle anderen die meteen. En wat ze posten, staat er morgen nog.',
  together2:
    'Geen Supabase of Firebase waarvoor je je moet aanmelden, geen backend om te koppelen, geen server om te huren. Vraag het gewoon zoals je het aan een vriend zou uitleggen.',
  kinds: [
    { name: 'Multiplayergames', ask: 'Laat tot acht vrienden meedoen aan dezelfde ronde en elkaars zetten live zien.' },
    { name: 'Chatrooms', ask: 'Voeg een chatroom toe waar iedereen met de link kan praten, en bewaar de laatste honderd berichten.' },
    { name: 'Gedeelde lijsten', ask: 'Maak van de paklijst een lijst die het hele team tegelijk kan bewerken.' },
    { name: 'Ranglijsten', ask: 'Houd een ranglijst bij met ieders beste tijd en zet de top tien op het startscherm.' },
    { name: 'Kleine sociale netwerken', ask: "Laat bruiloftsgasten foto's op één prikbord zetten en elkaars foto's liken." },
  ],
  quote: (text) => `“${text}”`,

  connectTitle: 'Koppel Claude Code, Codex of Cursor één keer',
  connectText:
    'Geef je agent dit adres van onze MCP-server. Vanaf dan weet hij hoe hij hier publiceert, zonder sleutel en zonder inloggen.',
  sayLike: 'Zeg dan in je project iets als',
  asks: [
    'Publiceer deze app op GenHTTP Lambda en stuur me de link.',
    'Zorg dat de highscores gedeeld worden, zodat iedereen dezelfde ranglijst ziet.',
  ],

  domainChip: 'Als het aanslaat',
  domainTitle: 'Geef hem een eigen domeinnaam',
  domainText:
    'Dezelfde app, dezelfde editorlink, maar op een adres dat van jou is. Makkelijker om hardop te zeggen, makkelijker om te onthouden. En het staat een stuk professioneler als mensen hem gaan delen.',
  domainSubject: 'Een domein voor mijn app',
  domainAsk: 'Vraag naar een eigen domein',

  questionsTitle: 'Voordat je app online gaat',
  questions: (offline, removed, showcase, terms) => [
    [
      'Is het echt gratis?',
      <>
        Ja. Geen account, geen creditcard en geen proefperiode. Je app blijft online zolang mensen hem gebruiken. Na{' '}
        {offline} dagen zonder bezoek of wijziging gaat hij offline, en na {removed} dagen wordt hij verwijderd.
      </>,
    ],
    [
      'Kunnen Claude Code, Codex of Cursor mijn app hier online zetten?',
      'Ja, en elke andere agent die een remote MCP-server kan toevoegen. Koppel hem één keer met het adres hierboven en vraag hem dan om te publiceren: hij deployt de app, checkt of hij reageert en stuurt je de link.',
    ],
    [
      'Waarom kunnen mijn vrienden mijn localhost-link niet openen?',
      'Omdat localhost je eigen computer is: het adres werkt alleen daar, en alleen zolang de app draait. Een tunnel leent hem een openbaar adres zolang je laptop aanstaat. Publiceer je hem hier, dan draait de app op onze servers, met een link die blijft werken als je laptop dicht is.',
    ],
    [
      'Heb ik een server, een backend of Supabase nodig?',
      'Nee. Elke app krijgt een eigen database, opslag voor bestanden en een live verbinding met iedereen die hem open heeft. Je huurt geen server en stelt geen tweede dienst in, en aan jouw kant hoeft ook niets te blijven draaien.',
    ],
    [
      'Kan ik mijn game multiplayer maken zonder eigen server?',
      'Ja. Wat de ene browser in localStorage bewaart, ziet de volgende nooit. Het gedeelde deel moet dus op een server staan, en hier is dat de onze. Vraag je agent om de game multiplayer te maken, en elke zet bereikt iedereen die hem open heeft.',
    ],
    [
      'Moet mijn app op een bepaalde manier gebouwd zijn?',
      "Nee, dat regelt je agent. Pagina's, afbeeldingen en stylesheets gaan online zoals ze zijn. Alles wat op de server moet draaien, past je agent aan voor dit platform. Jij beschrijft wat de app moet doen, en de agent doet de vertaalslag.",
    ],
    [
      'Hoe pas ik hem later aan?',
      'Met de editorlink die je bij het publiceren kreeg. Geef die aan je agent met je volgende wijziging, of open hem in je browser. Elke wijziging wordt een nieuwe versie op hetzelfde adres, en je kunt altijd terug naar een oudere versie.',
    ],
    [
      'Waar laat ik mijn API-sleutels?',
      'Niet in de code. Je agent vraagt om een sleutel bij naam, en jij vult de waarde in de editor in. Niemand leest hem terug, de editor niet en de agent ook niet.',
    ],
    [
      'Kan ik mijn code meenemen?',
      'Ja, hij is van jou. Download hem wanneer je wilt in de editor, als een project dat op zichzelf draait, inclusief database.',
    ],
    [
      'Wie kan mijn app zien?',
      <>
        Iedereen aan wie je de link geeft. Hij staat nergens vermeld, tenzij je hem zelf aan de {showcase('showcase')}{' '}
        toevoegt.
      </>,
    ],
    [
      'Is er iets wat ik niet mag publiceren?',
      <>
        Een paar dingen, zoals alles wat mensen schaadt of misleidt. De {terms('voorwaarden')} zijn kort en in gewone
        taal.
      </>,
    ],
  ],

  closeTitle: 'Het werkt op jouw computer.',
  closeAccent: 'Nu ook op die van hen.',
  noAgent: 'Geen agent? Bouw het hier',
  closeFacts: 'Gratis. Geen aanmelding. Niets te installeren.',

  scene: {
    label: 'Een agent krijgt de vraag om een app te publiceren. Het adres verandert van localhost in een openbare link, en er komen mensen bij.',
    ask: 'Zet mijn quizspel online, zodat mijn vrienden mee kunnen doen.',
    live: 'Hij staat live. Hier is je link.',
    publishing: 'Publiceren…',
    public: 'Openbaar',
    onlyYou: 'Alleen jij',
    app: 'Vrijdagavondquiz',
    playing: (count) => <>{count} online</>,
    you: 'Jij',
  },

  compareTitle: 'De kortste weg van “het werkt” naar “probeer het maar”',
  compareText:
    'Vercel, Cloudflare en Lovable zijn prima plekken om dingen te draaien. Maar ze beginnen met een aanmeldformulier. En zodra je app iets moet delen tussen bezoekers, komt er een tweede dienst bij die je moet instellen. Zo ziet het eruit als je vanaf nul begint.',
  rows: [
    'Beginnen zonder account',
    'Publiceren vanuit de agent die je al gebruikt',
    'Database en live data: chat, multiplayer, records',
    'Kosten van je eerste link',
  ],
  us: ['Ja', 'Eén keer koppelen, dan gewoon vragen', 'Zit in elke app', 'Gratis'],
  rivals: [
    ['Account nodig', 'Na inloggen in hun tools', 'Databasedienst toevoegen', 'Gratis plan'],
    ['Account nodig', 'Na inloggen in hun tools', 'Kan, met wat setup', 'Gratis plan'],
    ['Account nodig', 'Gebouwd in hun eigen editor', 'Via een gekoppelde backend', 'Gratis plan, beperkte credits'],
  ],
  compareNote:
    'Bijgewerkt in september 2026, voor iemand die nergens een account heeft. Abonnementen en functies van andere diensten veranderen. Kijk bij hen voor de details.',

};
