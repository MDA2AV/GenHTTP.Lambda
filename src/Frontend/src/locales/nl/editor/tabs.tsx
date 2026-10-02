import type { EditorMessages } from '../../en/editor';

export const tabs: EditorMessages['tabs'] = {
  codeName: 'Letters, cijfers, streepjes en underscores, eindigend op .cs',
  slashes: 'Geen slash aan het begin of eind, en korter dan 120 tekens.',
  deep: 'Maximaal zes mappen diep.',
  characters: 'Letters, cijfers, streepjes, underscores en punten, gescheiden door slashes.',
  extension: 'Het bestand heeft een extensie nodig, zodat het als het juiste type geserveerd wordt.',
  context: 'In .lambda/ alleen docs/ en tests/ – letters, cijfers, streepjes, underscores en punten, gescheiden door slashes.',
  contextFiles: 'Documentatie en tests: onderdeel van de versie, nooit gecompileerd of geserveerd',
  exists: 'Er is al een bestand met die naam.',
  remove: (name) => `${name} verwijderen? De inhoud gaat mee.`,
  there: (name) => `${name} bestaat al.`,
  entry: 'De snippet: wat hij teruggeeft, wordt geserveerd',
  errors: 'bevat fouten',
  removeFile: (name) => `${name} verwijderen`,
  removeTitle: 'Dit bestand verwijderen',
  placeholder: 'Types.cs, site/index.html of .lambda/docs/api.md',
  newFile: 'Nieuw bestand',
  uploadTitle: 'Een bestand uploaden: een afbeelding, een font, een pagina',
  upload: 'Bestand uploaden',
};
