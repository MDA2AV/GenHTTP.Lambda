import type { Messages } from '../en';

export const build: Messages['build'] = {
  title: 'Website maken met AI.',
  intro:
    'Beschrijf in je eigen woorden de website of app die je voor ogen hebt. AI bouwt hem voor je, wij hosten hem en binnen een paar minuten staat hij online, met een link die je naar iedereen kunt sturen. Gratis, zonder programmeren, zonder account.',
  placeholder: 'Ik wil een website die…',
  shortcut: 'ctrl + enter',
  buildIt: 'Maak mijn website',
  builtBy: 'Gemaakt door',
  password: 'wachtwoord',
  fable:
    'Fable zit achter een wachtwoord zolang we het uitproberen. Het heeft geen tijdslimiet. Het werkt dus door tot je app af is, niet tot de tijd om is.',
  onlyNew:
    'Hier maak je nieuwe websites. Wil je een bestaande aanpassen? Open de editorlink en beschrijf onder ‘Aanpassen’ wat er anders moet.',
  ideas: [
    'een website voor onze vereniging waar leden zich voor activiteiten aanmelden',
    'een inschrijflijst voor onze buurtborrel, zodat niet iedereen hetzelfde hapje meeneemt',
    'een gastenboek voor onze bruiloft',
    'een poll waarin mensen stemmen en de uitslag zien',
    'een ranglijst voor onze wekelijkse quizavond',
    'een verjaardagspagina waar vrienden hun felicitaties achterlaten',
  ],
  ahead: (waiting) =>
    waiting === 1 ? 'Er is nog één website voor de jouwe – daarna ben jij aan de beurt.' : `Er zijn nog ${waiting} websites voor de jouwe.`,
  starting: 'Starten…',
  asked: 'Jouw vraag',
  leaveOpen: 'Laat deze pagina open: de link om je website later aan te passen zie je alleen hier, zodra hij klaar is.',
  log: 'Wat de AI deed',
  online: 'Je website staat online',
  notOnline: 'Je website is gemaakt, maar staat niet online.',
  open: 'Je website openen',
  steps: {
    guide: 'Voorbereiden',
    examples: 'Voorbeelden bekijken',
    create: 'Een adres voor je website kiezen',
    write: 'Je website schrijven',
    improve: 'Je website verbeteren',
    check: 'Controleren op fouten',
    online: 'Online zetten',
    trying: 'Uitproberen',
    looking: 'Je website bekijken',
    forRecords: 'Ruimte maken voor de items',
    forKeys: 'Ruimte maken voor sleutels en wachtwoorden',
    forFiles: 'Ruimte maken voor wat de website opslaat',
    records: 'De items bekijken',
    keys: 'Nagaan welke sleutels en wachtwoorden nodig zijn',
    addFile: 'Een bestand toevoegen',
    removeFile: 'Een bestand verwijderen',
    files: 'Bekijken wat er is opgeslagen',
  },

  points: [
    {
      title: 'Zonder programmeren',
      text: 'Zeg in je eigen woorden wat je website moet doen, zoals je het aan een vriend zou vertellen. AI bouwt hem voor je, technische kennis is niet nodig.',
    },
    {
      title: 'Gratis hosting inbegrepen',
      text: 'Je website draait op onze servers. Geen hostingpakket, server of domeinnaam om te kopen, niets te installeren. Beveiliging en updates regelen wij.',
    },
    {
      title: 'Binnen enkele minuten online',
      text: 'Je krijgt meteen een link om te delen. De website onthoudt wat mensen invullen, zoals aanmeldingen, stemmen, berichten en scores, zodat iedereen hetzelfde ziet.',
    },
  ],

  questionsTitle: 'Voor je begint',
  questions: (offline, removed) => [
    [
      'Kan AI echt gratis een website voor me maken?',
      `Ja. Beschrijf hem in je eigen woorden en AI bouwt hem, zet hem online en geeft je de link. Geen account, geen creditcard, geen proefperiode. Hij blijft online zolang mensen hem gebruiken: na ${offline} dagen zonder bezoek of wijziging gaat hij offline, en na ${removed} dagen wordt hij verwijderd.`,
    ],
    [
      'Heb ik hosting, een server of een domeinnaam nodig?',
      'Nee. Je website draait op onze servers, met hosting, beveiliging en updates inbegrepen. Je krijgt meteen een link, dus je hoeft ook geen domeinnaam te kopen.',
    ],
    [
      'Kan ik een app maken zonder te kunnen programmeren?',
      'Ja. Je ziet nooit code. Zeg wat hij moet doen, zoals je het aan een vriend zou vertellen, en AI doet de rest: een website, een kleine app of een spel.',
    ],
    [
      'Kunnen mensen iets invullen, zoals aanmeldingen, stemmen of berichten?',
      'Ja. Je website onthoudt wat mensen invullen. Iedereen die de link opent, ziet dus dezelfde inzendingen, stemmen en scores.',
    ],
    [
      'Hoe openen anderen mijn website?',
      'Met de link, in elke browser, op hun telefoon of computer. Er hoeft niets geïnstalleerd te worden en er zit geen app store tussen.',
    ],
    [
      'Hoe pas ik mijn website later aan?',
      'Open de editorlink die je bij je website krijgt en beschrijf wat er anders moet, net als hier. Vind je een wijziging niet goed? Dan ga je gewoon terug naar hoe het eerst was.',
    ],
  ],

  yourApp: 'Je website',
  further: 'Om hem later aan te passen',
  keep:
    'Bewaar deze link goed. Het is de enige weg terug, en hij is niet te herstellen, ook niet door ons. Zet hem in je bladwijzers voordat je dit tabblad sluit.',
  change:
    'Wil je je website aanpassen? Open de editorlink en beschrijf onder ‘Aanpassen’ wat er anders moet, net als hier. Je eigen AI-assistent kan het ook, zoals hieronder beschreven.',
  copyLink: 'Editorlink kopiëren',
  lifetime: (offline, removed) =>
    `We houden hem online zolang hij gebruikt wordt: na ${offline} dagen zonder bezoek of wijzigingen gaat hij offline, en na ${removed} dagen wordt hij verwijderd. Open de editor om hem weer online te zetten.`,
  openEditor: 'Editor openen',
  another: 'Nog een website maken',

  keepGoing: 'Ga verder met je eigen AI-assistent',
  orOwn: 'Of gebruik je eigen AI-assistent',
  ownText:
    'Gebruik je al Claude of een andere AI-assistent? Koppel hem hier, dan maakt en wijzigt hij websites voor je op dezelfde manier. Wij hosten ze, dus je hoeft nog steeds niets in te stellen. Er is geen daglimiet.',
  ownTitle: 'Maak je website met je AI-assistent',
  ownOnly:
    'Koppel Claude of een andere AI-assistent aan het adres hieronder en beschrijf de website die je wilt. Hij maakt hem, wij hosten hem op onze servers en hij staat meteen online, met een link om te delen.',
  thenAsk:
    'Vertel hem daarna wat je wilt, bijvoorbeeld: “Maak een website voor ons koor met een agenda van onze concerten.”',
  howToChange: 'Zo pas je een website later ook aan: geef je assistent de editorlink en vertel wat er anders moet.',

  failedToStart: 'Starten is niet gelukt.',
  noAnswer: 'Klaar, maar zonder te zeggen wat er gebeurd is.',
  failed: 'Dat is niet gelukt.',
};
