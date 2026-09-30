import type { SourceMessages } from '../en/source';

/** De teksten van de pagina's met gepubliceerde code, /source en elk project daaronder, in het Nederlands. */
export const source: SourceMessages = {
  shell: {
    section: 'Open source',
    home: 'GenHTTP Lambda, de startpagina',
  },

  lambda: {
    label: 'Wat is een lambda?',
    text: 'Een webapp op GenHTTP Lambda: iemand beschrijft wat hij wil, een AI-agent schrijft het in C#, en binnen een paar minuten staat het online op een eigen adres – elke versie bewaard, met wat die veranderde.',
    build: 'Bouw er zelf een',
  },

  catalog: {
    eyebrow: 'Open source',
    title: 'Zie hoe de apps hier gemaakt zijn',
    intro:
      "Lambda's waarvan de eigenaars de code hebben gepubliceerd: elke versie, wat die veranderde, de documentatie en de tests. Lees het hier, of download een project dat overal draait waar .NET draait.",
    searchLabel: 'Projecten doorzoeken',
    searchPlaceholder: 'Zoek op naam of op wat het doet',
    orderLabel: 'Volgorde',
    orders: {
      stars: 'Meeste sterren',
      updated: 'Onlangs gewijzigd',
      published: 'Nieuw gepubliceerd',
    },
    counted: (total) => (total === 1 ? '1 project' : `${total} projecten`),
    failed: 'De projecten konden niet worden geladen.',
    loadingMore: 'Meer laden…',
    showMore: 'Meer tonen',
    nothingTitle: 'Nog niets gepubliceerd',
    nothing: (tab) => (
      <>
        Iets gebouwd waar anderen van kunnen leren? Open het dashboard, ga naar {tab('Open source')}, kies een
        licentie, en de code verschijnt hier.
      </>
    ),
    noMatchTitle: 'Niets gevonden',
    noMatch: (query) => `Geen enkel gepubliceerd project bevat ‘${query}’.`,
    clear: 'Alle projecten tonen',
    yoursTitle: 'Publiceer je eigen code',
    yours: (tab) => (
      <>
        Open het dashboard van je lambda en kies {tab('Open source')}, of vraag de agent die hem bouwde om hem te
        publiceren. Alleen wie de editorsleutel heeft, kan dat, onder de licentie die hij kiest – en wat de app
        bewaart, zijn records, bestanden en sleutels, hoort er nooit bij.
      </>
    ),
    build: 'Bouw iets',
    online: 'Online',
    offline: 'Offline',
    changed: (ago) => `gewijzigd ${ago}`,
    stars: (count) => (count === 1 ? '1 ster' : `${count} sterren`),
  },

  project: {
    loading: 'Broncode laden…',
    failed: 'De broncode kon niet worden geladen.',
    missingTitle: 'Hier staat geen gepubliceerde broncode',
    missing: 'Misschien heeft de eigenaar de publicatie ingetrokken, of er heeft op dit adres nooit een lambda gestaan.',
    all: 'Alle projecten',
    by: (name) => `door ${name}`,
    versions: (count) => (count === 1 ? '1 versie' : `${count} versies`),
    onlineAt: (address) => <>Online op {address}</>,
    offline: 'Nu offline',
    openApp: 'App openen',
    opens: (address) => `Opent ${address} in een nieuw tabblad`,
    published: (ago) => `Gepubliceerd ${ago}`,
    changed: (ago) => `Gewijzigd ${ago}`,
    picture: (name) => `${name}, zoals de app eruitziet`,
    tabsLabel: 'Wat je kunt lezen',
    tabs: {
      code: 'Code',
      docs: 'Documentatie',
      tests: 'Tests',
      changes: 'Wijzigingen',
    },
  },

  versions: {
    label: 'Versie',
    choose: 'Een andere versie lezen',
    newest: 'nieuwste',
    online: 'online',
    older: (version, ago, newest) => `Je leest versie ${version}, opgeslagen ${ago}. De nieuwste is versie ${newest}.`,
    toNewest: 'De nieuwste lezen',
    noChange: 'Geen notitie over wat die veranderde',
  },

  star: {
    star: 'Ster',
    add: 'Geef dit project een ster',
    remove: 'Je ster terugnemen',
    count: (count) => (count === 1 ? '1 ster' : `${count} sterren`),
    failed: 'De ster kon niet worden opgeslagen.',
  },

  download: {
    button: 'Downloaden',
    title: (version) => `Versie ${version} als project`,
    what:
      'Een .NET 10-project met een Dockerfile, de documentatie, de tests en de licentie. Wat de app bewaart – de records, de bestanden die hij opsloeg, de sleutels – zit er niet in.',
    zip: 'ZIP downloaden',
    preparing: 'Het project voorbereiden…',
    slow: 'Bij de eerste download van een versie wordt die ingepakt terwijl je wacht.',
    failed: 'Het project kon niet worden voorbereid. Probeer het zo nog eens.',
    run: 'Zo draai je het',
    local: 'Met de .NET 10 SDK:',
    container: 'Of in een container:',
    agent: 'Of geef de map aan je eigen codeeragent en bouw erop verder – binnen de licentie.',
    copy: 'Kopiëren',
    copied: 'Gekopieerd',
  },

  tree: {
    label: 'Bestanden',
    files: (count) => (count === 1 ? '1 bestand' : `${count} bestanden`),
    packing: 'Deze versie inpakken…',
    packingSlow: 'Een versie wordt ingepakt de eerste keer dat iemand hem leest; bij een grote versie duurt dat even.',
    failed: 'De bestanden van deze versie konden niet worden geladen.',
    legend: 'Wat is wat',
    kinds: {
      code: 'De eigen code van de lambda',
      asset: "Wat hij serveert: pagina's, scripts, stijlen, afbeeldingen – en zijn databasemigraties",
      docs: 'Wat het is, en waarom het zo gebouwd is',
      tests: 'Hoe het getest wordt',
      platform: 'Wat in de plaats van het platform komt',
      project: 'De host, de build, de container en de licentie',
    },
    short: {
      code: 'Code',
      asset: 'Geserveerd',
      docs: 'Docs',
      tests: 'Tests',
      platform: 'Platform',
      project: 'Project',
    },
  },

  file: {
    loading: 'Laden…',
    failed: 'Dit bestand kon niet worden geladen.',
    missing: (path) => `Deze versie heeft geen ${path}.`,
    binary: 'Dit bestand is geen tekst.',
    tooLarge: 'Dit bestand is te lang om hier te tonen.',
    download: 'Downloaden',
    raw: 'Onbewerkt',
    rawTitle: 'Het bestand openen zoals het is',
    copy: 'Kopiëren',
    copied: 'Gekopieerd',
    lines: (count) => (count === 1 ? '1 regel' : `${count} regels`),
    plain: 'Zonder kleuren getoond: het is lang.',
    line: (line) => `Regel ${line}`,
  },

  docs: {
    pages: "Pagina's",
    product: 'Wat het is',
    decisions: 'Beslissingen',
    loading: 'Laden…',
    failed: 'Deze pagina kon niet worden geladen.',
    noneTitle: 'Over deze versie is niets geschreven',
    none: 'De documentatie zou in docs/ staan: wat de app is, voor wie hij is, en waarom hij gebouwd is zoals hij is.',
  },

  tests: {
    files: 'Scripts en data',
    noneTitle: 'Deze versie zegt niets over zijn tests',
    none: 'Hoe de app getest wordt, zou in tests/README.md staan, met de scripts die daarbij horen ernaast.',
  },

  changes: {
    title: 'Elke versie, de nieuwste eerst',
    intro: 'Een versie verandert nooit meer als hij eenmaal is opgeslagen. Elke versie zegt in één regel wat hij veranderde.',
    agent: 'Geschreven door een agent',
    online: 'online',
    browse: 'De code lezen',
    noChange: 'Geen notitie',
  },

  licenses: {
    MIT: 'Iedereen mag het gebruiken, aanpassen en doorgeven, waarin dan ook, zolang de licentie en de copyrightvermelding erbij blijven.',
    'Apache-2.0': 'Zoals MIT, met een octrooilicentie van iedereen die eraan bijdroeg, en wijzigingen gemarkeerd als wijzigingen.',
    'BSD-3-Clause': 'Zoals MIT, en niemand mag de namen van de auteurs gebruiken om reclame te maken voor wat hij ervan maakte.',
    'MPL-2.0': 'Wijzigingen aan deze bestanden blijven onder dezelfde licentie; ze mogen worden gecombineerd met code onder elke andere licentie.',
    'GPL-3.0-or-later': 'Wie het doorgeeft, gewijzigd of niet, geeft de broncode ervan door onder dezelfde licentie.',
    'AGPL-3.0-or-later': 'Zoals de GPL, en een gewijzigde kopie via het netwerk aan mensen aanbieden telt als doorgeven.',
    Unlicense: 'Aan het publieke domein gegeven: iedereen mag er alles mee doen, zonder voorwaarden.',
  },

  kinds: {
    Permissive: 'Permissief',
    Copyleft: 'Copyleft',
    PublicDomain: 'Publiek domein',
  },
};
