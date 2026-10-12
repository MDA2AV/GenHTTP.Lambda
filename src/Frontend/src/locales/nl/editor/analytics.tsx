import type { EditorMessages } from '../../en/editor';

export const analytics: EditorMessages['analytics'] = {
  hint: 'Door het platform voor je geteld: geen cookies, geen code in je app en niets wat je bezoekers hoeven te accepteren.',
  loading: 'De bezoeken worden opgehaald…',
  loadFailed: 'De bezoeken konden niet worden opgehaald.',

  offTitle: 'Zie wie je app bezoekt',
  offText:
    'Hoeveel mensen er per dag komen, welke pagina’s ze openen, welke sites hen sturen, uit welke landen en op welke apparaten – dag voor dag geteld, vanaf het moment dat je het aanzet.',
  offPrivate: 'Geen cookies en geen banner die je bezoekers moeten accepteren',
  offAddresses: 'Er worden geen adressen bewaard – alleen hoeveel bezoeken er per dag waren',
  offKept: (days) =>
    days >= 365
      ? 'Een jaar bewaard en alles wordt aan jou getoond'
      : `Een jaar bewaard – de laatste ${days} dagen worden getoond, alles met Premium`,
  switchOn: 'Analytics aanzetten',
  switchedOn: 'Analytics staat aan. De eerste bezoeken verschijnen binnen een paar minuten.',
  switchOnFailed: 'Analytics kon niet worden aangezet.',
  demo: 'De bezoekers van een demo worden niet geteld. Een eigen kopie kan haar bezoekers wel tellen.',

  range: 'Periode',
  days: (days) => (days >= 365 ? '12 maanden' : `${days} dagen`),
  premiumRange: 'Premium toont tot een jaar',

  visitors: 'Bezoekers',
  visitorsHint: 'Verschillende mensen, elk één keer per dag geteld.',
  views: 'Paginaweergaven',
  viewsHint: 'Elke keer dat iemand een pagina opende.',
  hits: 'Requests',
  hitsHint: 'Alles wat er van je app gevraagd werd: de pagina’s, en de plaatjes en scripts die ze laden.',
  versus: (days) => `ten opzichte van de ${days} dagen ervoor`,
  noChange: 'ongewijzigd',
  inDays: (days) => (days >= 365 ? 'in de afgelopen 12 maanden' : `in de afgelopen ${days} dagen`),

  perDay: {
    visitors: 'Bezoekers per dag',
    views: 'Paginaweergaven per dag',
    hits: 'Requests per dag',
  },
  perWeek: {
    visitors: 'Bezoekers per week',
    views: 'Paginaweergaven per week',
    hits: 'Requests per week',
  },
  showValues: 'Waarden tonen',
  showChart: 'Grafiek tonen',
  day: 'Dag',
  week: 'Week van',

  nobody: 'In deze dagen heeft niemand de app bezocht.',
  nobodyYet: 'Sinds analytics aanstaat heeft nog niemand de app bezocht. De eerste bezoeken verschijnen binnen een paar minuten.',

  pages: 'Pagina’s',
  pagesHint: 'De pagina’s die mensen openden, de meest geopende eerst.',
  requested: 'Meest opgevraagd',
  requestedHint: 'Alles wat er van je app gevraagd werd, per adres – er zijn geen pagina’s die mensen openen.',
  sources: 'Waar bezoekers vandaan komen',
  sourcesHint: 'De sites die mensen hierheen stuurden, per pagina waarop ze zaten. Zoekmachines laten alleen weten dát zij het waren, niet wat er gezocht is.',
  direct: 'Ingetypt, een bladwijzer of een app',
  countries: 'Landen',
  countriesHint: 'Waar de internetadressen van de bezoekers geregistreerd staan – meestal, niet altijd, waar ze zijn.',
  unknownCountry: 'Onbekend',
  devices: 'Apparaten',
  device: {
    desktop: 'Computer',
    mobile: 'Telefoon',
    tablet: 'Tablet',
    bot: 'Bots',
    other: 'Overig',
  },
  browsers: 'Browsers',
  systems: 'Besturingssystemen',
  crawlers: 'Zoekmachines en AI',
  crawlersHint: 'Programma’s die langskwamen: zoekmachines die je pagina’s indexeren, AI-assistenten die ze lezen, chats die een voorbeeld van een link tonen.',
  noCrawlers: 'In deze dagen is er geen zoekmachine of AI langs geweest.',
  other: 'Overige',
  otherBots: 'Overige programma’s',
  showAll: (count) => `Alle ${count} tonen`,
  showFewer: 'Minder tonen',
  share: (part, whole) => `${part} van ${whole}`,
  none: 'Nog niets.',

  moreKept: (since) => `Geteld sinds ${since} – Premium toont alles, tot een jaar.`,
  premiumAsk: 'Vraag naar Premium',
  premiumSubject: 'Premium voor mijn app',
  premiumBody: (address) => `Hallo,\n\nIk wil graag alle analytics van mijn app zien: ${address}\n\n`,

  footer: (minutes) =>
    minutes === 1 ? 'Per dag geteld, in UTC. Elke minuut bijgewerkt.' : `Per dag geteld, in UTC. Elke ${minutes} minuten bijgewerkt.`,
  updated: (time) => `Laatst bijgewerkt om ${time}.`,
  since: (date) => `Geteld sinds ${date}.`,

  switchOff: 'Analytics uitzetten',
  switchOffTitle: 'Analytics uitzetten?',
  switchOffText: 'Het tellen stopt en alles wat tot nu toe geteld is, wordt verwijderd. Dit kan niet ongedaan worden gemaakt.',
  keep: 'Blijven tellen',
  switchOffConfirm: 'Uitzetten en verwijderen',
  switchedOff: 'Analytics staat uit en wat geteld was, is verwijderd.',
  switchOffFailed: 'Analytics kon niet worden uitgezet.',
};
