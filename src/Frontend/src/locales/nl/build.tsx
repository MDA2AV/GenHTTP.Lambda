import type { Messages } from '../en';

export const build: Messages['build'] = {
  offTitle: 'Staat hier niet aan',
  off: (write, mcp) => (
    <>
      Deze installatie heeft geen bouwagent. Je kunt de code nog steeds {write('zelf schrijven')}, of je eigen Claude
      koppelen aan {mcp}.
    </>
  ),

  title: 'Zeg wat je wilt.',
  intro:
    'Je app wordt gebouwd en online gezet, en jij krijgt een link die je naar iedereen kunt sturen. Geen account, niets te installeren. En je app kan dingen onthouden, zoals scores, berichten en inzendingen. Zo ziet iedereen die hem opent hetzelfde.',
  placeholder: 'maak een…',
  working: 'bezig…',
  shortcut: 'ctrl + enter',
  building: 'Aan het bouwen',
  buildIt: 'Bouwen',
  builtBy: 'Gebouwd door',
  password: 'wachtwoord',
  fable:
    'Fable zit achter een wachtwoord zolang we het uitproberen. Het heeft geen tijdslimiet. Het werkt dus door tot je app af is, niet tot de tijd om is.',
  onlyNew:
    'Hier maak je alleen nieuwe apps. Wil je iets aanpassen wat je al hebt gemaakt? Open de editorlink en zeg onder ‘Aanpassen’ wat er anders moet.',
  ideas: [
    'maak een prikbord waar iedereen een berichtje van één regel kan achterlaten',
    'maak een highscorelijst voor een dobbelspel',
    'maak een poll waarin mensen stemmen en de uitslag zien',
    'maak een gastenboek voor onze bruiloft',
    'maak een aftelklok naar een datum die iedereen kan zien',
  ],
  ahead: (waiting) =>
    waiting === 1
      ? 'Er staat nog één build voor je in de rij. Daarna ben jij aan de beurt.'
      : `Er staan nog ${waiting} builds voor je in de rij.`,
  starting: 'Starten…',

  yourApp: 'Je app',
  further: 'Verder bouwen',
  keep: 'Bewaar deze link goed. Het is de enige weg terug, en hij is niet te herstellen, ook niet door ons. Zet hem in je bladwijzers voordat je dit tabblad sluit.',
  change:
    'Wil je deze app aanpassen? Open de editorlink en zeg onder ‘Aanpassen’ wat er anders moet, net zoals hier. Je eigen coding agent kan het ook, zoals hieronder staat.',
  copyLink: 'Editorlink kopiëren',
  lifetime: (offline, removed) =>
    `Je app blijft online zolang hij gebruikt wordt. Na ${offline} dagen zonder bezoek of wijziging gaat hij offline, en na ${removed} dagen wordt hij verwijderd. Open de editor en klik op ‘Deployen’ om hem weer online te zetten.`,
  openEditor: 'Editor openen',
  another: 'Nog iets maken',

  keepGoing: 'Verder met je eigen agent',
  orOwn: 'Of gebruik je eigen agent',
  ownText:
    'Achter het vak hierboven zit een Claude die op deze server draait. Heb je al een eigen agent? Koppel die dan hier. Hij kan precies hetzelfde: een lambda maken, de code schrijven en alles online zetten. Zonder daglimiet, en zonder via deze pagina te gaan.',
  thenAsk: 'Vraag hem daarna wat je wilt, net zoals je hier zou doen.',
  claudeWeb: 'Claude op het web',
  claudeWebHow:
    'Instellingen, dan ‘Connectors’, dan ‘Add custom connector’. Plak het adres hierboven als URL van de remote MCP-server. Je hebt geen sleutel nodig en hoeft niet in te loggen.',
  howToChange:
    'Zo pas je ook iets aan als het eenmaal gebouwd is: geef je agent de editorlink en vertel wat hij moet doen.',
  more: 'Meer over werken met je eigen agent',

  failedToStart: 'Starten is niet gelukt.',
  noAnswer: 'Klaar, maar zonder te zeggen wat er gebeurd is.',
  failed: 'Dat is niet gelukt.',
};
