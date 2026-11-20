import type { KitMessages } from '../en';

export const buildBox: KitMessages['buildBox'] = {
  placeholder: 'Ik wil een website die…',
  password: 'wachtwoord',
  unlimited:
    'Dit model heeft het wachtwoord nodig en werkt dan zonder tijdslimiet: het gaat door tot je website klaar is, in plaats van tot de klok afloopt.',
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
    services: 'Vraagt om andere diensten te mogen gebruiken',
  },
  further: 'Je editorlink',
  keep:
    'Bewaar deze link op een veilige plek. Het is de enige weg naar je website vanaf een ander apparaat, en hij is niet te herstellen, ook niet door ons.',
  copyLink: 'Editorlink kopiëren',
  lifetime: (offline, removed) =>
    `We houden hem online zolang hij gebruikt wordt: na ${offline} dagen zonder bezoek of wijzigingen gaat hij offline, en na ${removed} dagen wordt hij verwijderd. Je kunt hem hier weer online zetten, of in de editor.`,

  failedToStart: 'Starten is niet gelukt.',
  noAnswer: 'Klaar, maar zonder te zeggen wat er gebeurd is.',
  failed: 'Dat is niet gelukt.',
};
