import type { EditorMessages } from '../../en/editor';

export const tabs: EditorMessages['tabs'] = {
  codeName: 'Een C#-bestand bovenaan: letters, cijfers, streepjes en underscores, beginnend met een letter en eindigend op .cs, maximaal 40 tekens.',
  name: 'Letters, cijfers en - _ . + @ ( ) [ ] { } $ ~, mappen gescheiden door slashes, geen spaties en geen naam die op een punt eindigt.',
  taken: 'Een geëxporteerde of gekloonde lambda heeft bovenaan al een bestand of map met die naam. Zet het in een map, of geef het een andere naam.',
  lambda: 'Een lambda heeft geen map .lambda/ meer: de documentatie staat in docs/, de tests in tests/.',
  assets: 'Wat een lambda serveert, staat nu in zijn resources – voeg het daar toe.',
  resourceName: 'Letters, cijfers, streepjes, underscores en punten, gescheiden door slashes, maximaal zes mappen diep – en een extensie, zodat het als het juiste soort bestand wordt geserveerd.',
  exists: 'Er is al een bestand met die naam.',
  remove: (name) => `${name} verwijderen? De inhoud gaat mee.`,
  removeFolder: (name, files) => `${name} en de ${files === 1 ? '1 bestand' : `${files} bestanden`} erin verwijderen?`,
  there: (name) => `${name} bestaat al.`,
  entry: 'De snippet: wat hij teruggeeft, wordt geserveerd',
  errors: 'bevat fouten',
  removeFile: (name) => `${name} verwijderen`,
  removeTitle: 'Verwijderen',
  codePlaceholder: 'Store.cs, docs/notes.md of frontend/app.ts',
  resourcePlaceholder: 'web/index.html',
  upload: 'Bestand uploaden',
};
