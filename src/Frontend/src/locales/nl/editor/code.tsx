import type { EditorMessages } from '../../en/editor';

export const code: EditorMessages['code'] = {
  title: 'Code',
  version: (version) => `versie ${version}`,
  edited: ', bewerkt',
  online: ', online',
  loadFailed: 'Die versie kon niet worden geladen.',
  compiles: 'De code compileert.',
  notYet: 'De code compileert nog niet.',
  checkFailed: 'De code kon niet worden gecontroleerd.',
  saved: (version) => `Opgeslagen als versie ${version}.`,
  featureSaved: 'Opgeslagen in het concept. Deploy de voorvertoning om het uit te proberen.',
  featureLoadFailed: 'Het concept kon niet worden geladen.',
  previewOnline: 'De voorvertoning staat online.',
  previewRefused: 'De voorvertoning is niet veranderd. Kijk hieronder wat de compiler zegt.',
  isOnline: (version) => `Versie ${version} staat online.`,
  notOnline: 'Niet online gegaan. Kijk hieronder wat de compiler zegt.',
  failed: 'Dat is niet gelukt.',
  unchanged: 'Er is niets veranderd sinds je laatst opsloeg.',
  demo: 'Dit is een demo, dus alles hier is alleen-lezen. Maak er je eigen lambda van om hem aan te passen. ',
  edit: 'Bewerk de code met de hand. Opslaan maakt een nieuwe versie en laat wat online staat met rust; deployen zet hem online. Wil je een wijziging eerst uitproberen, start dan een concept. ',
  editFeature:
    'De code van dit concept. Opslaan bewaart hem in het concept: voor de bezoekers van de lambda verandert er niets. Deployen zet hem online op het eigen adres van het concept, om hem uit te proberen; samenvoegen maakt van het concept de volgende versie. ',
  inFeature: (name) => `in ‘${name}’`,
  changedElsewhere: 'Het concept is ergens anders opgeslagen sinds je het opende, misschien door de agent. Laad wat er is opgeslagen voordat je hier opslaat; je wijzigingen zouden er niet overheen worden opgeslagen.',
  readAgain: 'Laden wat er is opgeslagen',
  files: (entry, cs, context) => (
    <>
      {entry} geeft terug wat er geserveerd wordt, andere {cs}-bestanden bevatten types, en elk ander bestand wordt
      geserveerd zoals het is – behalve wat in de map {context} staat: de documentatie, de tests en waaruit het is
      gebouwd, die nooit gecompileerd of geserveerd worden. Ctrl-S slaat op, F12 springt naar een declaratie.
    </>
  ),
  newer: (version) => ` Versie ${version} is nieuwer dan de versie die hier openstaat.`,
  check: 'Controleren',
  save: 'Opslaan',
  deploy: 'Deployen',
  deployPreviewTitle: 'Opslaan, en het concept online zetten op zijn eigen adres om het uit te proberen',
  binary: (size) => `Geen tekst, dus er valt niets te bewerken. Het wordt geserveerd zoals het is en is ${size} kB groot.`,
  saveAndDeploy: 'Opslaan en deployen',
  saveVersion: 'Nieuwe versie opslaan',
  fromOlder: (version, newest) =>
    `Dit gaat uit van versie ${version}, en versie ${newest} is nieuwer. Opslaan maakt het de nieuwste versie, zonder wat er na versie ${version} kwam.`,
  featureInstead: (start) => (
    <>
      Wil je iets uitproberen? {start('Zet het dan in een nieuw concept')}: dat krijgt een eigen adres, en er wordt pas
      een versie opgeslagen als het goed is.
    </>
  ),
  cancel: 'Annuleren',
  what: 'Wat verandert er? Optioneel, het komt in de geschiedenis.',
  placeholder: 'Voegt een contactformulier toe',
  goToDefinition: 'Naar definitie',
};
