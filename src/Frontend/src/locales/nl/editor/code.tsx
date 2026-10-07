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
  demo: 'Dit is een demo, dus alles hier is alleen-lezen. Maak er je eigen lambda van om hem aan te passen.',
  hint: (b) => (
    <>
      De bestanden van één versie. De {b('code')} is het programma en alles wat ermee wordt bewaard: de .cs-bestanden
      bovenaan worden gecompileerd, en elk ander bestand – de documentatie, de tests, waaruit een front end wordt
      gebouwd – wordt bij de versie bewaard en nooit gecompileerd of geserveerd. De {b('resources')} – pagina’s,
      scripts, stijlen, afbeeldingen, de migraties van de database – worden gelezen en geserveerd terwijl de versie
      draait, en zijn openbaar waar de code ze serveert. Opslaan maakt een nieuwe versie en laat wat online staat
      ongemoeid; wil je een wijziging eerst uitproberen, start dan een concept. Ctrl-S slaat op, F12 springt naar een
      declaratie.
    </>
  ),
  hintFeature: (b) => (
    <>
      De bestanden van dit concept: de {b('code')} – de .cs-bestanden bovenaan worden gecompileerd, de rest wordt
      erbij bewaard – en de {b('resources')}, die worden gelezen en geserveerd terwijl het draait. Opslaan bewaart ze in
      het concept en toont ze op het eigen adres van het concept; je bezoekers zien er niets van totdat je het concept
      online zet.
    </>
  ),
  inFeature: (name) => `in ‘${name}’`,
  changedElsewhere: 'Het concept is ergens anders opgeslagen sinds je het opende, misschien door de agent. Laad wat er is opgeslagen voordat je hier opslaat; je wijzigingen zouden er niet overheen worden opgeslagen.',
  readAgain: 'Laden wat er is opgeslagen',
  newer: (version) => `Versie ${version} is nieuwer dan de versie die hier openstaat.`,
  check: 'Controleren',
  save: 'Opslaan',
  deploy: 'Deployen',
  deployPreviewTitle: 'Opslaan, en het concept online zetten op zijn eigen adres om het uit te proberen',
  binary: (size) => `Geen tekst, dus er valt hier niets te bewerken. Het is ${size} groot.`,
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
  versionLabel: 'Versie',
  shown: (version, online, newest) =>
    `Versie ${version}${online ? ', online' : newest ? ', nieuwste' : ''}`,
  optionOnline: ' (online)',
  switchUnsaved: 'Wat je hier hebt gewijzigd, is niet opgeslagen. Toch de andere versie openen?',
  noVersion: 'Er is nog geen versie om te tonen.',
  label: 'Bestanden',
  codeGroup: 'Code',
  codeWhy: 'Nooit geserveerd. De .cs-bestanden bovenaan worden gecompileerd; de rest wordt bij de versie bewaard.',
  resources: 'Resources',
  resourcesPublic: 'Openbaar: deze versie serveert ze met Resources.',
  resourcesPrivate: 'Meegeleverd met de versie, maar deze versie serveert ze niet.',
  noResources: 'Geen in deze versie.',
  count: (files) => (files === 1 ? '1 bestand' : `${files} bestanden`),
  groupUsage: (files, size) => `${files}, ${size}`,
  usage: (used, of) => `Deze versie komt op ${used} van de ${of} die een versie mag hebben, code en resources samen.`,
  scope: (data) => (
    <>Wat de lambda bewaart terwijl hij draait, is voor elke versie hetzelfde en staat onder {data('Data')}.</>
  ),
  download: 'Downloaden',
  newIn: (group) => `Nieuw bestand in ${group}`,
  uploadIn: (group) => `Uploaden naar ${group}`,
  pick: 'Kies een bestand om te zien wat erin staat.',
};
