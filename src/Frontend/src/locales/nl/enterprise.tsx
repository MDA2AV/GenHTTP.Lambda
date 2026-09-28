import type { Messages } from '../en';

export const enterprise: Messages['enterprise'] = {
  eyebrow: 'Enterprise',
  title: 'Gratis proberen, zelf draaien',
  intro:
    'Alles hier is gratis, zonder account. Wil je team apps die voorgoed online blijven, achter een eigen login? Neem dan een eigen installatie, in de cloud of op je eigen servers.',

  free: 'Gratis',
  freeTagline: 'Om dingen uit te proberen',
  forever: 'voor altijd',
  buildOne: 'App maken',
  freeFeatures: (offline, removed) => [
    "Onbeperkt lambda's, geen account",
    'De ingebouwde agent, of je eigen via MCP',
    'Online zolang hij gebruikt wordt',
    `Offline na ${offline} dagen zonder bezoek, verwijderd na ${removed} dagen`,
    'Op een pad van de gedeelde host',
  ],
  freeNote: 'Geen creditcard, geen aanmelding. Maak een lambda aan en hij is van jou.',

  name: 'Enterprise',
  tagline: 'Voor teams die een eigen omgeving willen',
  perUser: 'per gebruiker / maand',
  contact: 'Neem contact op',
  features: [
    'Je eigen omgeving, in de cloud of op eigen servers',
    'Eén service draait alle apps',
    'Inloggen met je eigen SSO',
    'Jouw governance- en complianceregels ingebouwd',
    'Apps blijven voorgoed online, er wordt nooit iets verwijderd',
    'Gebruik je eigen agent via MCP',
    'Support met voorrang',
  ],
  users: (count) => <>{count} gebruikers</>,
  perMonth: ' / maand',
  price: (amount) => `$\u00a0${amount}`,
  perUserPrice: (amount) => `$\u00a0${amount} / gebruiker / maand`,

  compareTitle: 'Vergelijk de pakketten',
  compareText: 'Ze draaien allebei op hetzelfde platform. Het verschil: hoe lang je app blijft staan, en waar.',
  included: 'Inbegrepen',
  notIncluded: 'Niet inbegrepen',
  groups: (offline, removed) => [
    {
      title: 'Bouwen',
      rows: [
        ["Lambda's", 'Onbeperkt', 'Onbeperkt'],
        ['Ingebouwde agent', true, false],
        ['Je eigen agent via MCP', true, true],
        ['Editor, versies en logs', true, true],
        ['Showcase', true, 'Eigen'],
      ],
    },
    {
      title: 'Hosting',
      rows: [
        ['Offline bij inactiviteit', `Na ${offline} dagen`, 'Nooit'],
        ['Verwijderd bij inactiviteit', `Na ${removed} dagen`, 'Nooit'],
        ['Omgeving', 'Gedeeld', 'Eigen'],
        ['Draait', 'In onze cloud', 'Cloud of eigen servers'],
        ['Wat jij beheert', 'Niets', 'Eén service'],
        ['Eigen domeinen', false, true],
      ],
    },
    {
      title: 'Beheer',
      rows: [
        ['Inloggen', 'Niet nodig', 'Je eigen SSO'],
        ['Jouw governance- en complianceregels voor agents', false, true],
        ['Beheerconsole', false, true],
        ['Data gescheiden van andere klanten', false, true],
        ['Support', 'Community', 'Met voorrang'],
      ],
    },
  ],

  questionsTitle: 'Vragen',
  questions: [
    ['Heb ik een account nodig om te beginnen?', 'Nee. Een gratis lambda heeft alleen de editorlink nodig die je krijgt als je hem aanmaakt.'],
    [
      'Wie telt als gebruiker bij Enterprise?',
      'Iedereen die inlogt via jullie SSO, of het nu is om te bouwen in de editor of om een app te gebruiken die op jullie installatie draait. Wie een app gebruikt zonder in te loggen, telt niet mee.',
    ],
    [
      'Zit de ingebouwde agent bij Enterprise?',
      'Nee. Je team neemt een eigen agent mee, zoals Claude, Claude Code of iets anders dat MCP spreekt, en koppelt die aan jullie installatie. Dat kan met het abonnement dat jullie al bij die leverancier hebben.',
    ],
    [
      'Hoe leren agents onze complianceregels?',
      'We verwerken jullie governance- en complianceregels in wat het platform via MCP aan agents vertelt. Elke agent die je team koppelt, krijgt ze mee terwijl hij code schrijft. Zo volgen de apps jullie regels, zonder dat iedereen ze uit het hoofd hoeft te kennen.',
    ],
    [
      'Hebben we Kubernetes of een cluster nodig?',
      'Nee. Elke app draait binnen één service. Er zijn dus geen pods om te verdelen en per app valt er niets te orkestreren. Je draait alleen die ene service.',
    ],
    [
      'Waar draait een Enterprise-installatie?',
      'Waar jullie willen. We kunnen hem voor jullie hosten in onze cloud. Of hij draait in een cloudaccount van jullie of op jullie eigen servers: overal waar containers draaien. In beide gevallen helpen we jullie hem op te zetten en up-to-date te houden.',
    ],
  ],
  anythingElse: (mail) => <>Nog iets anders? Mail naar {mail}.</>,
};
