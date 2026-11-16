import type { Messages } from '../en';

export const selfHosting: Messages['selfHosting'] = {
  menu: 'Zelf hosten',

  eyebrow: 'Open source · cloud of zelf gehost',
  title: 'Gebruik het hier,',
  titleAccent: 'of host het zelf.',
  intro:
    'GenHTTP Lambda is open source onder de MIT-licentie. genhttp.dev draait het voor iedereen die het wil gebruiken, en de community-editie is hetzelfde platform als Docker-image, voor wie het liever zelf draait. Beide bouwen en hosten dezelfde apps; wat verschilt is waar ze staan en wie ervoor zorgt.',

  cloud: {
    kicker: 'Cloud',
    name: 'genhttp.dev',
    summary: 'Open deze site en begin. De server, de updates en de limieten zijn aan ons.',
    points: ['Niets te installeren of te draaien', 'Apps antwoorden op een adres op genhttp.run', 'De bouwagent inbegrepen'],
    action: 'Bouw iets',
  },
  community: {
    kicker: 'Zelf gehost',
    name: 'Community-editie',
    summary: 'Eén Docker-container op een machine van jou. De server, de updates en de limieten zijn aan jou.',
    points: ['Eén container, één volume, geen databaseserver', 'Apps antwoorden op jouw adres', 'De bouwagent als extra container'],
    action: 'Draai het zelf',
  },

  sameTitle: 'In beide hetzelfde',
  sameText: 'De community-editie is geen kleinere versie. Elke app krijgt dezelfde dingen, waar hij ook draait:',
  same: [
    'De bouwagent',
    'Je eigen agent via MCP',
    'De REST API',
    'De editor, eenvoudig en volledig',
    'Concepten met testdata',
    'Versies, en een weg terug',
    'Items, bestanden, sleutels en wachtwoorden',
    'Analytics zonder cookies',
    'Eigen domeinen',
    'Git en downloads',
    'De showcase en gepubliceerde code',
  ],

  differTitle: 'Wat verschilt',
  aspect: 'Vraag',
  rows: (offline) => [
    ['Waar de apps draaien', 'Op onze server', 'Op die van jou, in één container met één volume'],
    ['Beginnen', 'Open de bouwpagina - geen account', 'Start de image en open dan het adres'],
    ['Wie de server beheert', 'Wij', 'Jij'],
    ['Wat het kost', 'Niets; Premium-apps op aanvraag', 'Niets; de server betaal je zelf'],
    ['Waar een app antwoordt', 'Op een subdomein van genhttp.run', 'Onder jouw adres, of op subdomeinen van een domein dat je opgeeft'],
    ['Een eigen domein', 'Voor Premium-apps', 'Voor elke app die je naar productie verplaatst'],
    ['Apps die niemand gebruikt', `Offline na ${offline} dagen zonder bezoek of wijziging, tenzij Premium`, 'Zoals jij het instelt'],
    ['Limieten en regels', 'Die van ons, en onze gebruiksvoorwaarden', 'Die van jou, ingesteld in het beheerpaneel'],
    ['De bouwagent', 'Inbegrepen, een aantal builds per dag', 'Optioneel, met je eigen Claude-abonnement'],
    ['Gebruiksrapporten', 'Geen', 'Eén keer per dag: de versie, de machine, de opties en tellingen over de apps - nooit een app of een persoon. Je kunt het uitzetten.'],
  ],

  moveTitle: 'Verhuizen tussen beide',
  move:
    'Dezelfde apps draaien op allebei, en je agent verhuist er een met git: hij maakt de app aan de andere kant aan, kopieert de bestanden van de oude over en pusht ze. Een download is een .NET-project dat overal draait, met zijn items in een SQLite-bestand.',

  pickTitle: 'Wat bij je past',
  pickCloud: {
    title: 'genhttp.dev, als',
    points: [
      'je een app online wilt zonder iets te draaien',
      'de mensen die hem gebruiken overal zitten',
      'je liever geen server beheert',
    ],
  },
  pickCommunity: {
    title: 'De community-editie, als',
    points: [
      'je apps en hun items op een machine van jou moeten blijven',
      'het hulpmiddelen voor je team zijn, binnen je eigen netwerk',
      'je de limieten en regels zelf wilt bepalen',
    ],
  },

  startTitle: 'Aan de slag',
  startCloud: {
    title: 'Op genhttp.dev',
    text: 'Beschrijf op de bouwpagina wat je nodig hebt, of koppel de agent die je al gebruikt. Je hoeft geen account te maken.',
  },
  startCommunity: {
    title: 'Op je eigen server',
    text: 'Eén commando start het. Open het adres, koppel je agent en bouw - de README vertelt de rest.',
  },
  connect: 'Koppel je agent',
  readme: 'Lees de README',
  dockerHub: 'De image op Docker Hub',

  sourceTitle: 'Open source',
  source: (github) => (
    <>
      GenHTTP Lambda wordt uitgebracht onder de MIT-licentie. De code, de issues en de releases staan op {github('GitHub')}.
    </>
  ),
};
