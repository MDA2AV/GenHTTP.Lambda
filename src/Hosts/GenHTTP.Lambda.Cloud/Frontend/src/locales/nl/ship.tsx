import type { Messages } from '../en';

export const ship: Messages['ship'] = {
  eyebrow: 'Gratis hosting voor vibe-coded apps',
  title: 'Van localhost naar ieders scherm.',
  intro:
    'Je hebt een app gebouwd met Claude Code, Codex of Cursor, maar hij draait alleen op je eigen computer. Vraag je agent om hem hier online te zetten. Een paar minuten later heeft hij een openbare link die iedereen kan openen, een eigen database en een live verbinding met iedereen die hem open heeft. Mensen kunnen er dus samen spelen, chatten en posten.',
  facts: ['Gratis', 'Geen account', 'Geen creditcard', 'Niets te installeren'],

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
  sayLike: 'Zeg dan in je project iets als',
  asks: [
    'Publiceer deze app op GenHTTP Lambda en stuur me de link.',
    'Zorg dat de highscores gedeeld worden, zodat iedereen dezelfde ranglijst ziet.',
  ],

  questionsTitle: 'Voordat je app online gaat',
  questions: (offline, removed, showcase, terms, selfHosting) => [
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
      'Kan ik er met git naartoe pushen?',
      'Ja. Elke app is ook een git-repository: kloon hem vanaf het adres onder Klonen in de editor, pas hem aan met je eigen tools of agent en push. Elke commit die je naar main pusht wordt de volgende versie, en een branch die je pusht wordt een concept met een eigen voorvertoning.',
    ],
    [
      'Waar laat ik mijn API-sleutels?',
      'Niet in de code. Je agent vraagt om een sleutel bij naam, en jij vult de waarde in de editor in. Niemand leest hem terug, de editor niet en de agent ook niet.',
    ],
    [
      'Kan ik mijn code meenemen?',
      'Ja, hij is van jou. Download hem wanneer je wilt in de editor, als een project dat op zichzelf draait, inclusief database - of kloon hem met git, met alle versies erbij.',
    ],
    [
      'Kan ik het ook op mijn eigen server hosten?',
      <>
        Ja. GenHTTP Lambda is open source, en de community-editie is hetzelfde platform als één Docker-container, voor je
        eigen server en je eigen agent. {selfHosting('Cloud of zelf hosten')} legt uit wat er verschilt.
      </>,
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
  noAgent: 'Geen agent? Bouw het hier',

};
