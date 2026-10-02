import type { EditorMessages } from '../../en/editor';

export const context: EditorMessages['context'] = {
  docs: {
    title: 'Documentatie',
    titleSimple: 'Over je app',
    hint: 'Wat deze app is, voor wie hij is en waarom – en waarom hij gebouwd is zoals hij is. Agents schrijven het bij elke wijziging en het wordt bij elke versie bewaard, dus een oudere versie komt terug met de documentatie die toen voor hem gold.',
    hintSimple: 'Waar je app voor is en waarom, zoals de agent het begreep uit wat je vroeg. Hij houdt dit bij met elke wijziging.',
    inDraft: 'De documentatie van dit concept. Die wordt de documentatie van je app zodra het concept online gaat.',
    pages: { product: 'Product', decisions: 'Beslissingen' },
    emptyTitle: 'Nog niets geschreven',
    emptyText: (code) => (
      <>
        Agents schrijven de documentatie bij hun wijzigingen: wat de app is, voor wie hij is en waarom in{' '}
        {code('.lambda/docs/product.md')}, en waarom hij gebouwd is zoals hij is in {code('decisions.md')}. Het hoort
        bij de versie, naast de code.
      </>
    ),
    emptySimpleTitle: 'Er is nog niets over je app geschreven',
    emptySimple: 'De agent kan beschrijven waar je app voor is en waarom, op basis van wat je vroeg – daarna houdt hij de beschrijving bij.',
    ask: 'De agent vragen het te schrijven',
    describe: 'De agent vragen hem te beschrijven',
    writePrompt: 'Schrijf de documentatie van deze app: wat hij is, voor wie hij is en waarom, en de technische beslissingen erachter.',
    describePrompt: 'Beschrijf waar deze app voor is en waarom, zodat ik het kan lezen onder ‘Over de app’.',
    decisionsPrompt: 'Leg de technische beslissingen achter deze app vast, en waarom ze genomen zijn.',
    missingProduct: 'Nog geen productpagina',
    missingProductText: 'Wat de app is, voor wie hij is, wat mensen ermee doen en waarom – in de woorden van wie erom vroeg.',
    missingDecisions: 'Nog geen beslissingen vastgelegd',
    missingDecisionsText: 'Hoe de app gebouwd is en waarom: hoe hij zijn data bewaart, waar hij van afhangt, wat er is weggelaten. Wat wie hem hierna aanpast, moet weten.',
    correctText: 'De agent schrijft dit op basis van wat je vroeg, en houdt het bij met elke wijziging. Klopt er iets niet of ontbreekt er iets? Laat het hem weten.',
    correct: 'Laat het de agent weten',
    correctPrompt: 'Verbeter de beschrijving van de app: ',
    placeholder: 'Legt uit waarom inzendingen een jaar bewaard blijven',
  },
  tests: {
    title: 'Tests',
    hint: 'Hoe deze app automatisch getest wordt, en de scripts en data die de tests gebruiken. Agents houden het bij en voeren de tests uit voordat ze een wijziging klaar noemen. Het wordt bij elke versie bewaard.',
    inDraft: 'De tests van dit concept. Ze worden de tests van je app zodra het concept online gaat – voer ze eerst uit op de voorvertoning ervan.',
    pages: { testing: 'Hoe het getest wordt' },
    emptyTitle: 'Nog geen tests',
    emptyText: (code) => (
      <>
        Hoe de app getest wordt – wat moet blijven werken, hoe je dat controleert en hoe je de scripts ervoor uitvoert
        – schrijven agents in {code('.lambda/tests/README.md')}, met de scripts en de testdata ernaast.
      </>
    ),
    ask: 'De agent vragen tests te schrijven',
    writePrompt: 'Schrijf de tests van deze app: wat moet blijven werken en hoe je dat automatisch controleert, met een script om op de voorvertoning uit te voeren.',
    missing: 'Nog niet beschreven hoe het getest wordt',
    missingText: 'Wat moet blijven werken, hoe elk onderdeel daarvan gecontroleerd wordt, en hoe je de scripts ernaast uitvoert.',
    placeholder: 'Controleert dat een volle lijst geen nieuwe inzendingen aanneemt',
  },
  files: 'Bestanden',
  noFiles: 'Geen bestanden naast de pagina’s.',
  none: 'geen',
  missingPill: 'Nog niet geschreven',
  changedIn: (version) => `Gewijzigd in versie ${version}`,
  changedInDraft: 'Gewijzigd in dit concept',
  showChanges: 'Wijzigingen tonen',
  hideChanges: 'Wijzigingen verbergen',
  noChanges: 'Er is niets veranderd.',
  edit: 'Bewerken',
  olderVersion: 'Een versie verandert nooit: een pagina bewerk je in de nieuwste versie, of in een concept.',
  writeIt: 'Zelf schrijven',
  askPage: 'De agent vragen het te schrijven',
  editInCode: 'Openen in de code',
  cancel: 'Annuleren',
  save: 'Opslaan',
  write: 'Schrijven',
  preview: 'Voorbeeld',
  writeOrPreview: 'Schrijven of voorbeeld',
  discard: 'Je wijzigingen aan deze pagina gaan verloren. Weggooien?',
  reading: 'Lezen…',
  readFailed: 'Dit kon niet worden gelezen.',
  saveFailed: 'Dat kon niet worden opgeslagen.',
  savedDraft: 'Opgeslagen in het concept.',
  savedVersion: (version) => `Opgeslagen als versie ${version}.`,
  savedOnline: (version) => `Opgeslagen als versie ${version}, en online.`,
  savedNotOnline: (version) => `Opgeslagen als versie ${version}, maar niet online gegaan.`,
  saveTitle: 'Opslaan als nieuwe versie',
  saveText: (newest) =>
    `Een versie verandert nooit, dus deze pagina wordt opgeslagen als de volgende – bovenop versie ${newest}, met al het andere zoals het is.`,
  clash: (version) => `Versie ${version} is opgeslagen sinds je begon, en die heeft deze pagina ook gewijzigd. Opslaan vervangt dat.`,
  alsoOnline: 'Ook online zetten',
  alsoOnlineNote: 'Alleen de documentatie verandert, dus bezoekers zien niets nieuws – maar wat online staat, blijft de nieuwste versie.',
  skeleton: {
    product: '# Naam van de app\n\nWat het is, in een zin of twee.\n\n## Voor wie het is\n\n## Wat mensen ermee doen\n\n## Functies, en waarom ze er zijn\n\n## Wat het niet doet\n',
    decisions: '# Beslissingen\n\n## Een beslissing\n\nWat er besloten is, waarom, en waar een wijziging rekening mee moet houden.\n',
    testing: '# Hoe het getest wordt\n\nHoe je de tests uitvoert, en op welk adres.\n\n## Wat moet blijven werken\n\n| Gedrag | Request | Verwacht |\n|---|---|---|\n| | | |\n',
  },
};
