import type { Messages } from '../en';

export const build: Messages['build'] = {
  title: 'Van idee naar website.',
  intro:
    'Beschrijf de website of app die je voor ogen hebt. AI maakt hem voor je, wij hosten hem op onze servers en hij staat meteen online, met een link die je naar iedereen kunt sturen. Zonder programmeren, zonder hosting in te stellen, zonder account.',
  placeholder: 'Ik wil een website die…',
  working: 'bezig…',
  shortcut: 'ctrl + enter',
  building: 'Bezig met maken',
  buildIt: 'Maak mijn website',
  builtBy: 'Gemaakt door',
  password: 'wachtwoord',
  fable:
    'Fable zit achter een wachtwoord zolang we het uitproberen. Het heeft geen tijdslimiet. Het werkt dus door tot je app af is, niet tot de tijd om is.',
  onlyNew:
    'Hier maak je nieuwe websites. Wil je een bestaande aanpassen? Open de editorlink en beschrijf onder ‘Aanpassen’ wat er anders moet.',
  ideas: [
    'een website voor onze vereniging waar leden zich voor activiteiten aanmelden',
    'een gastenboek voor onze bruiloft',
    'een poll waarin mensen stemmen en de uitslag zien',
    'een scorebord voor onze wekelijkse quizavond',
    'een aftelklok tot onze opening die iedereen kan zien',
  ],
  ahead: (waiting) =>
    waiting === 1 ? 'Er is nog één website voor de jouwe – daarna ben jij aan de beurt.' : `Er zijn nog ${waiting} websites voor de jouwe.`,
  starting: 'Starten…',

  points: [
    {
      title: 'Beschreven, niet geprogrammeerd',
      text: 'Zeg in je eigen woorden wat je website moet doen. Je hoeft niet te programmeren en geen technische kennis te hebben.',
    },
    {
      title: 'Hosting inbegrepen',
      text: 'Je website draait op onze servers. Hosting, beveiliging en updates regelen wij – je hoeft niets in te stellen of bij te houden.',
    },
    {
      title: 'Binnen enkele minuten online',
      text: 'Je krijgt meteen een link om te delen. De website kan ook dingen onthouden, zoals inzendingen, stemmen en scores, zodat iedereen hetzelfde ziet.',
    },
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
