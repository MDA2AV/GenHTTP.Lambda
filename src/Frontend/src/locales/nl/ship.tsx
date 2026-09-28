import type { Messages } from '../en';

export const ship: Messages['ship'] = {
  title: 'Van je laptop naar ieders scherm.',
  intro:
    'Je hebt iets gebouwd met je coding agent, maar het draait alleen op je eigen computer. Vraag je agent om het hier te publiceren. Een paar minuten later heeft het een openbare link die iedereen kan openen. En het kan dingen onthouden. Mensen kunnen er dus samen spelen, chatten en posten.',
  facts: ['Gratis', 'Geen account', 'Niets te installeren'],
  connect: 'Koppel je agent',
  seeOthers: 'Bekijk wat anderen bouwden',

  stepsTitle: 'Drie stappen, en één ervan is een zin',
  step: (n) => `Stap ${n}`,
  steps: [
    {
      title: 'Eén keer koppelen',
      body: 'Voeg één adres toe aan Claude, Cursor of welke agent je ook gebruikt. Dat kost minder dan een minuut, en je doet het maar één keer.',
    },
    {
      title: 'Laat hem publiceren',
      body: 'Zeg dat hij de app hier online moet zetten. Hij pakt je app in, publiceert hem en checkt of hij reageert.',
    },
    {
      title: 'Deel de link',
      body: 'Je krijgt een openbaar adres en een editorlink die alleen voor jou is. Het adres stuur je naar wie je maar wilt. De editorlink houd je zelf: daarmee pas je de app later aan.',
    },
  ],

  togetherTitle: 'Niet zomaar een pagina. Een plek waar mensen samenkomen.',
  together:
    'De meeste hosts geven elke bezoeker een eigen kopie van je app, en iedereen speelt in zijn eentje. Hier heeft elke app een eigen geheugen en een live verbinding met iedereen die hem open heeft. Doet iemand een zet, dan zien alle anderen die meteen. En wat ze posten, staat er morgen nog.',
  together2:
    'Geen database waarvoor je je moet aanmelden, geen tweede dienst om te koppelen. Vraag het gewoon zoals je het aan een vriend zou uitleggen.',
  kinds: [
    { name: 'Multiplayergames', ask: 'Laat tot acht vrienden meedoen aan dezelfde ronde en elkaars zetten live zien.' },
    { name: 'Chatrooms', ask: 'Voeg een chatroom toe waar iedereen met de link kan praten, en bewaar de laatste honderd berichten.' },
    { name: 'Gedeelde lijsten', ask: 'Maak van de paklijst een lijst die het hele team tegelijk kan bewerken.' },
    { name: 'Scores en records', ask: 'Houd een ranglijst bij met ieders beste tijd en zet de top tien op het startscherm.' },
    { name: 'Kleine sociale netwerken', ask: "Laat bruiloftsgasten foto's op één prikbord zetten en elkaars foto's liken." },
  ],
  quote: (text) => `“${text}”`,

  connectTitle: 'Koppel je agent één keer',
  connectText:
    'Geef je agent dit adres. Vanaf dan weet hij hoe hij hier publiceert, zonder sleutel en zonder inloggen.',
  sayLike: 'Zeg dan in je project iets als',
  asks: [
    'Publiceer deze app op GenHTTP Lambda en stuur me de link.',
    'Zorg dat de highscores gedeeld worden, zodat iedereen dezelfde ranglijst ziet.',
  ],

  domainChip: 'Als het aanslaat',
  domainTitle: 'Geef hem een eigen naam',
  domainText:
    'Dezelfde app, dezelfde editorlink, maar op een adres dat van jou is. Makkelijker om hardop te zeggen, makkelijker om te onthouden. En het staat een stuk professioneler als mensen hem gaan delen.',
  domainSubject: 'Een domein voor mijn app',
  domainAsk: 'Vraag naar een eigen domein',

  questionsTitle: 'Voor je het vraagt',
  questions: (offline, removed, showcase, terms) => [
    [
      'Is het echt gratis?',
      <>
        Ja. Geen creditcard, geen proefperiode en geen account. Je app blijft online zolang mensen hem gebruiken. Na{' '}
        {offline} dagen zonder bezoek of wijziging gaat hij offline, en na {removed} dagen wordt hij verwijderd.
      </>,
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
  closeFacts: 'Gratis. Geen account. Niets te installeren.',

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
    'Live gedeelde data: chat, multiplayer, records',
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

  yourAgent: 'Je agent',
  terminal: 'Terminal',
  setups: {
    claudeCode: 'Voer dit één keer uit in een terminal. Elk project dat je daarna opent, kan hier publiceren.',
    claude: (strong) => (
      <>
        Ga in Claude op het web of op je desktop naar {strong('Instellingen')}, dan naar {strong('Connectors')}, en
        kies {strong('Add custom connector')}. Plak het adres hierboven en sla op. Meer hoeft niet.
      </>
    ),
    cursor: 'Voeg dit toe aan de MCP-instellingen van Cursor, of aan het bestand hieronder, en herlaad.',
    vscode: 'Sla dit op in je project en start de server vanuit de MCP-weergave van Copilot Chat.',
  },
  elsewhere:
    'Gebruik je iets anders? Windsurf, Codex, Zed en de meeste andere agents kunnen in hun instellingen een remote MCP-server toevoegen. Geef ze het adres hierboven.',
};
