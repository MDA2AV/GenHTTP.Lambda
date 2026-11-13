import type { Messages } from '../en';

export const landing: Messages['landing'] = {
  eyebrow: 'AI-app-builder, hosting inbegrepen',
  headline: 'Beschrijf een app.',
  headlineAccent: 'Je agent zet hem online.',
  intro:
    'Polls, gastenboeken, ranglijsten, multiplayergames. Vertel onze AI-agent wat je nodig hebt, of gebruik de agent die je al hebt. Je krijgt een werkende app, door ons gehost, met een link om te delen. En je kunt hem blijven verbeteren, ook lang na de eerste versie.',
  build: 'Maak een app',
  ownAgent: 'Gebruik je eigen agent',
  free: 'Gratis. Geen account, geen creditcard, niets te installeren.',
  seeIt: 'Bekijk hoe het werkt',

  videoTitle: 'Van één zin naar een live app',
  videoText:
    'Een privévenster, geen account en één prompt op de pagina ‘App maken’. Daarna de kant-en-klare app, geopend via de link, precies zoals elke bezoeker hem ziet.',
  videoNote: 'Het bouwen zie je versneld. De rest is op normale snelheid.',
  tryIt: 'Probeer het zelf',

  oneShotTitle: 'Meer dan een app-generator',
  oneShotText:
    'De meeste generators geven je een resultaat en laten je ermee zitten. Hier blijft je app draaien waar hij gebouwd is. Jij en je agent werken er dus gewoon aan verder.',
  steps: [
    {
      title: 'Zeg wat je wilt',
      body: 'Beschrijf het in gewone taal, aan de agent op deze site of aan je eigen agent. Geen code, geen setup, geen account.',
      alt: 'De pagina ‘App maken’ met een prompt voor een lunchpoll',
    },
    {
      title: 'Een werkende app met een link',
      body: 'Je app wordt gebouwd en gehost, en je krijgt een openbaar adres om te delen. Geen server, hostingpakket, domeinnaam of database om in te stellen: dat deel regelen wij. De app onthoudt zijn data, zoals stemmen, scores en berichten. Iedereen die hem opent, ziet dus hetzelfde.',
      alt: 'De kant-en-klare lunchpoll, geopend in een browser',
    },
    {
      title: 'Blijf hem verbeteren',
      body: 'Elke app krijgt een editorlink die alleen voor jou is. Geef die aan je agent met je volgende wijziging, of open hem zelf. Elke wijziging wordt een nieuwe versie. Het adres blijft hetzelfde.',
      alt: 'Het dashboard van de poll: alle versies, elk met wat er gevraagd werd, wat er veranderde en het verschil met de vorige',
    },
  ],
  weekLater: 'Een week later',
  weekAsk:
    'Hier is de editorlink van mijn lunchpoll. Kun je het stemmen elke vrijdag om 11 uur sluiten en de winnaar bovenaan zetten?',
  weekAnswer: 'Klaar! Versie 4 staat live op hetzelfde adres. Wil je terug? Versie 3 is er nog.',

  agentsTitle: 'Neem je eigen agent mee: Claude, Codex, Cursor',
  agentsText:
    'Ben je al aan het vibe coden met Claude Code, Codex, Cursor of een andere assistent? Koppel hem aan dit adres, een remote MCP-server, zonder sleutel. Dan kan hij hier apps bouwen, deployen en bijwerken, gewoon vanuit het gesprek dat je al open hebt.',
  thenAsk: (em) => (
    <>Vraag dan gewoon: {em('maak een inschrijflijst voor ons teamuitje en zet hem online')}.</>
  ),
  hostIt: (link) => (
    <>Iets gebouwd dat alleen op localhost draait? {link('Zet je vibe-coded app hier online')}.</>
  ),

  contactTitle: 'Laten we praten',
  contactText:
    'Hulp nodig, iets groters van plan, of zoek je een oplossing op maat? We horen graag van je.',
  mailTitle: 'Mail ons',
  mailText: 'Voor projecten, vragen en alles wat je liever onder vier ogen bespreekt.',
  discordTitle: 'Kom erbij op Discord',
  discordText: 'Laat zien wat je hebt gebouwd, vraag hulp bij de volgende stap en praat direct met het team.',
  discordLink: 'De GenHTTP-Discord',
};
