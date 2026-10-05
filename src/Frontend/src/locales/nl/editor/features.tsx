import type { EditorMessages } from '../../en/editor';

export const features: EditorMessages['features'] = {
  hint:
    'Een concept is een kopie van je app om een wijziging op uit te proberen voordat iemand het ziet, met een eigen adres en testdata. Zet het online zodra het goed is; tot dan krijgen je bezoekers wat nu online staat.',
  newFeature: 'Nieuw concept',
  full: (limit) => `Er zijn al ${limit} concepten, en meer mag niet. Zet er eerst een online of gooi er een weg.`,
  emptyTitle: 'Geen concepten',
  emptyText:
    'Een concept is een kopie van je app om een wijziging op uit te proberen voordat die online gaat. Als de agent een wijziging voor je klaarzet om uit te proberen, vind je die hier.',
  start: 'Nieuw concept',
  askAgentNew: 'De agent om een wijziging vragen',
  noChange: 'Nog niet beschreven wat het verandert',
  behindTitle: 'Je app is veranderd sinds dit concept begon',
  behind: () => 'verouderd',
  branchTitle: 'De branch waarin dit concept staat in de git-repository van de app',
  previewOnline: 'voorvertoning draait',
  previewOutdated: 'voorvertoning toont een eerdere opslag',
  previewOffline: 'voorvertoning draait niet',
  changed: 'gewijzigd',
  openPreview: 'Uitproberen',
  openPreviewTitle: 'De voorvertoning openen in een nieuw tabblad',
  count: (open, limit) => `${open} van ${limit} concepten`,
  loading: 'Concept laden…',
  readFailed: 'Het concept kon niet worden gelezen.',

  newTitle: 'Nieuw concept',
  newText:
    'Een kopie van je app en de data ervan, met een eigen adres. Pas het aan en probeer het daar uit - je bezoekers zien er niets van tot je het online zet.',
  newTextFiles:
    'Wat je hebt getypt komt in het concept in plaats van een versie te worden, zodat je het op het eigen adres kunt uitproberen voordat het online gaat.',
  name: 'Naam',
  namePlaceholder: 'Ranglijst',
  wanted: 'Wat moet het doen?',
  wantedPlaceholder: 'Optioneel. Bewaar de tien beste scores en toon ze na elk spel.',
  olderBase: (newest) =>
    `Dit begint bij een oudere versie en is dus meteen verouderd: voordat het online kan, moet wat er tot en met versie ${newest} veranderde eerst worden overgenomen.`,
  create: 'Concept starten',
  createFailed: 'Het concept kon niet worden gestart.',
  retry: 'Opnieuw proberen',
  madeNotSaved: (name) =>
    `Het concept ‘${name}’ is gestart, maar wat je hebt getypt, kon er nog niet in worden opgeslagen. Probeer het opnieuw, of sluit dit en zoek het concept op onder Concepten.`,
  created: (name) => `Het concept ‘${name}’ is gestart.`,
  cancel: 'Annuleren',

  featureHint:
    'Een kopie van je app om deze wijziging op uit te proberen. De voorvertoning heeft een eigen adres en testdata, dus je bezoekers zien er niets van tot je het online zet.',
  askAgent: 'De agent vragen',
  askCatchUp: 'De agent vragen het bij te werken',
  catchUp: 'Werk dit concept bij naar de nieuwste versie van de app, en behoud wat het verandert.',
  editCode: 'Code bewerken',
  deployPreview: 'Voorvertoning starten',
  updatePreview: 'Voorvertoning bijwerken',
  previewDeployed: 'De voorvertoning draait.',
  previewFailed: 'De voorvertoning kon niet worden gestart.',
  previewStopped: 'De voorvertoning is gestopt.',
  previewRejected: 'De voorvertoning is niet veranderd',
  previewNotCompiling: 'De code compileert niet, dus de voorvertoning toont nog de laatste versie die wel werkte.',
  started: 'Gestart',
  changes: () => 'Gewijzigde bestanden',
  noChanges: () => 'Nog niets gewijzigd.',
  editNotes: 'Naam en notities',
  what: 'Wat verandert het?',
  whatPlaceholder: 'Voegt een ranglijst toe met de tien beste scores',
  missed: () => 'Wat er sinds het begin in je app is gewijzigd',
  missedNothing: 'Niets in de bestanden.',

  behindText: (_base, newest) =>
    `Versie ${newest} van je app is opgeslagen nadat dit concept begon. Als je het concept nu online zet, maak je ongedaan wat die versie veranderde. Het moet dus eerst worden bijgewerkt - de agent kan dat voor je doen.`,
  moveBase: 'Als bijgewerkt markeren',
  close: 'Sluiten',
  mergeTitle: (name) => `‘${name}’ online zetten`,
  mergeTitleShort: 'Maak er de nieuwe versie van je app van en zet het online',
  leaks: (path, files) =>
    `In ${files} staat een link naar ${path}, dat is je live app. Vanuit de voorvertoning lezen en wijzigen die links de echte data in plaats van de testdata. Vraag de agent om te linken zonder dat deel (‘api/items’).`,
  mergeButton: 'Online zetten',
  saveFirst: 'Sla eerst je wijzigingen op: de voorvertoning en het online zetten gebruiken wat is opgeslagen.',
  mergeAndDeploy: () => 'Online zetten',
  mergeText: (version) =>
    `Het wordt versie ${version} van je app en gaat online. De data van je app blijft zoals die is.`,
  deployTooNote: (active) => `Met één klik ga je in de versies terug naar versie ${active}.`,
  deployTooOffline: 'Je app staat nu offline; hiermee zet je hem online.',
  notCompiling: 'De code compileert niet, dus het is niet online gezet. Los het eerst op in het concept.',
  mergeFailed: 'Het concept kon niet online worden gezet.',
  merged: (version) => `Opgeslagen als versie ${version}.`,
  mergedOnline: (version) => `Versie ${version} staat online.`,

  notesTitle: 'Naam en notities',
  save: 'Opslaan',
  saveFailed: 'Dat kon niet worden opgeslagen.',

  baseTitle: 'Als bijgewerkt markeren?',
  baseText: () =>
    'Alleen een concept dat bevat wat de nieuwste versie veranderde, kan online zonder dat ongedaan te maken. Staan die wijzigingen nu in dit concept - door jou of door de agent binnengehaald - markeer het dan als bijgewerkt.',
  moveTo: () => 'Als bijgewerkt markeren',
  baseWarning: 'Dit wordt niet gecontroleerd. Staan de wijzigingen niet in het concept, dan maakt het online zetten ze ongedaan.',

  deleteTitle: (name) => `‘${name}’ verwijderen?`,
  deleteText:
    'De code, de voorvertoning en de testdata ervan worden definitief verwijderd. Je app en de versies blijven onaangeroerd.',
  keep: 'Laten staan',
  deleteForGood: 'Definitief verwijderen',
  deleteFailed: 'Het concept kon niet worden verwijderd.',
  deleted: (name) => `Het concept ‘${name}’ is verwijderd.`,

  all: 'Alle concepten',
  actions: 'Meer acties voor dit concept',
  download: 'Downloaden als zip',
  stopPreview: 'Voorvertoning offline halen',
  delete: 'Dit concept verwijderen',
  viewsLabel: 'Het concept',
  views: {
    overview: 'Concept',
    docs: 'Documentatie',
    code: 'Code',
    build: 'Build',
    tests: 'Tests',
    data: 'Testdata',
    logs: 'Logs',
  },
  missingTitle: 'Dit concept bestaat niet meer',
  missingText: 'Het is online gezet of verwijderd. In de versies zie je wat ermee gebeurd is.',
};
