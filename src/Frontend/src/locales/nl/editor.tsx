import type { EditorMessages } from '../en/editor';

/** Een aantal met het juiste woord: "1 bestand", "3 bestanden". */
const many = (count: number, one: string, more: string) => (count === 1 ? `1 ${one}` : `${count} ${more}`);

/** De teksten van de editor in het Nederlands. */
export const editor: EditorMessages = {
  shared: {
    units: { s: 's', min: 'min', h: 'u', d: 'd' },
    amount: (value, unit) => `${value} ${unit}`,
    pair: (larger, smaller) => `${larger} ${smaller}`,
    never: 'nooit',
    justNow: 'zojuist',
    ago: (span) => `${span} geleden`,
    in: (span) => `over ${span}`,
    origins: {
      agent: 'agent',
      template: 'sjabloon',
      admin: 'beheerder',
      system: 'platform',
      api: 'API / editor',
      unknown: 'onbekend',
    },
    endings: {
      replaced: 'vervangen door een nieuwere deployment',
      stopped: 'offline gehaald',
      expired: 'verlopen door inactiviteit',
      admin: 'offline gehaald door de beheerder',
      ended: 'beëindigd',
    },
    whatThisIs: 'Wat is dit?',
    byAgent: 'door een agent',
    writtenByAgent: 'Geschreven door een agent',
    more: 'Meer',
    of: (used, total) => `${used} van ${total}`,
    online: (version) => `Online · v${version}`,
    onlineTitle: (version) => `Online, versie ${version} draait`,
    offline: 'Offline',
    offlineTitle: 'Offline: er wordt niets geserveerd',
    premium:
      'Premium: kan op een eigen domein draaien, heeft meer ruimte voor code, assets en data, en blijft online hoe stil het ook is',
    demo: 'Demo: door deze installatie online gehouden, alleen-lezen',
    tier: (tier) => `Pakket: ${tier}`,
    entrances: {
      title: 'Bereikt via',
      note: 'Sinds de start van de server, websocketverbindingen meegeteld.',
    },
    chart: {
      showChart: 'Grafiek tonen',
      showValues: 'Waarden tonen',
      none: 'Nog geen metingen.',
      time: 'Tijd',
    },
    diagnostics: {
      compiles: 'De code compileert.',
      none: 'Nog geen meldingen. Klik op Controleren of Deployen om je code te compileren.',
      line: (line) => `regel ${line}`,
    },
  },

  frame: {
    title: 'Editor',
    sections: {
      overview: 'Overzicht',
      change: 'Aanpassen',
      features: 'Concepten',
      showcase: 'Showcase',
      domain: 'Domein',
      files: 'Bestanden',
      data: 'Data',
      versions: 'Versies',
      deployments: 'Deployments',
      stats: 'Statistieken',
      logs: 'Logs',
      code: 'Code',
    },
    sectionsLabel: 'Onderdelen',
    loadFailed: 'Deze lambda kon niet worden geladen.',
    online: (version) => `Versie ${version} staat online.`,
    deployFailed: 'De lambda kon niet worden gedeployd.',
    offline: 'Offline gehaald. De code staat er nog.',
    offlineFailed: 'De lambda kon niet offline worden gehaald.',
    leave: 'Je niet-opgeslagen wijzigingen in de code gaan verloren. Toch weggaan?',
    nothingTitle: 'Deze link opent niets',
    createNew: 'Nieuwe lambda aanmaken',
    loading: 'Je lambda laden…',
    moreActions: 'Meer acties',
    redeploy: (version) => `Versie ${version} opnieuw deployen`,
    takeOffline: 'Offline halen',
    copyLink: 'Link kopiëren',
    copyPrivate: 'Editorlink kopiëren',
    privateLink: 'Iedereen met deze link kan de lambda aanpassen. Houd hem voor jezelf.',
    rename: 'Adres wijzigen',
    download: 'Downloaden als .NET-project',
    delete: 'Deze lambda verwijderen',
    deploy: (version) => `Versie ${version} deployen`,
    problems: 'Er ging onlangs iets mis',
    demoTitle: 'Een demo, online gehouden door deze installatie. Alleen-lezen.',
    demo: (start) => (
      <>
        Bekijk de code, de geschiedenis, wat hij opslaat en de logs: daar is hij voor. Wil je iets aanpassen?{' '}
        {start('Maak er je eigen lambda van')}.
      </>
    ),
    keep: 'Bewaar deze link. Het is de enige weg terug naar deze lambda.',
    gotIt: 'Begrepen',
    rejected: (version) => `Versie ${version} is niet online gegaan`,
    refused: 'De deployment is geweigerd',
    openCode: 'Code openen',
    close: 'Sluiten',
    notCompiling: 'De code compileert niet. Wat eerst online stond, staat er nog.',
    moved: (path) => `Nu op ${path}.`,
    deleteTitle: 'Deze lambda verwijderen?',
    cancel: 'Annuleren',
    deleteForGood: 'Definitief verwijderen',
    deleteFailed: 'De lambda kon niet worden verwijderd.',
    deleteText: (key) => (
      <>Alle versies, alle data, de geschiedenis en het adres {key} gaan mee. Dit kun je niet ongedaan maken.</>
    ),
    openInTab: 'Openen in nieuw tabblad',
    open: (address) => `${address} openen in een nieuw tabblad`,
    copyAddress: 'Adres kopiëren',
    renameFailed: 'Het adres kon niet worden gewijzigd.',
    moveIt: 'Verhuizen',
    renameText: 'Het oude adres werkt meteen niet meer. Pas dus alles aan wat ernaar linkt.',
    changing: 'De agent past deze lambda aan',
    waiting: 'Een wijziging wacht op de agent',
    follow: 'Meekijken',
    changeRunning: 'Er loopt een wijziging',
    changeEnded: 'Er is een wijziging afgelopen',
  },

  change: {
    hint: 'Zeg wat er anders moet, en de agent op deze server doet het. Hij werkt in een concept (een kopie van de lambda met een eigen adres), dus je bezoekers zien niets tot het klaar is. Daarna wordt het de volgende versie en gaat het online, of het wacht in het concept tot je het eerst zelf hebt uitgeprobeerd.',
    reading: 'Verbinden met de agent…',
    readFailed: 'De agent kon niet worden bereikt.',
    label: 'Wat moet er anders?',
    placeholder: 'Bewaar de tien beste scores in plaats van vijf, voeg een dark mode toe, …',
    placeholderNext: 'Wat moet er nog meer anders?',
    send: 'Wijziging doorvoeren',
    sending: 'Versturen…',
    goOnline: 'Online zetten als het klaar is',
    goOnlineOn: 'Zodra het in het concept werkt, voegt de agent het samen tot de volgende versie en zet hij die online. Tot die tijd blijft online wat er nu staat.',
    goOnlineOff: 'De agent laat het in het concept staan. Probeer het uit op het eigen adres van het concept, en voeg het samen als je tevreden bent.',
    where: 'Werken in',
    whereTitle: 'Een nieuw concept, of een open concept om mee verder te gaan',
    newFeature: 'Nieuw concept',
    full: (limit) =>
      `Deze lambda heeft ${limit} open concepten, en meer mag niet. Kies er een om mee verder te gaan, of voeg er eerst een samen of verwijder er een.`,
    password: 'Wachtwoord',
    fable: 'Fable neemt er de tijd voor: geen tijdslimiet, geen limiet op het aantal stappen, en een eigen wachtrij.',
    left: (left, perDay) => `Vandaag nog ${left} van ${perDay}`,
    leftTitle: 'De bouwpagina en dit onderdeel delen één daglimiet.',
    noneLeft: (time) => `Dat was de laatste voor vandaag. Vanaf ${time} kan het weer.`,
    ideas: ['Zorg dat het goed werkt op een telefoon', 'Voeg een dark mode toe', 'Laat het er verzorgder uitzien'],
    fixLog: 'Los de fouten in de logs op',
    how: [
      {
        title: 'Hij leest wat er al is',
        text: 'De code, en wat er bij eerdere versies gevraagd werd. Zo blijft werken wat al werkte.',
      },
      {
        title: 'Hij werkt in een concept',
        text: 'Een kopie met een eigen adres en eigen data: daar voert hij de wijziging door en probeert hij die uit. Jij ziet elke stap.',
      },
      {
        title: 'Hij zet het online',
        text: 'Als de volgende versie, of hij laat het eerst aan jou om uit te proberen. Met één klik ga je terug naar de vorige versie.',
      },
    ],
    asked: 'Jouw vraag',
    goesOnline: 'gaat online als het klaar is',
    review: 'blijft in het concept staan, zodat je het kunt uitproberen',
    inFeature: (name) => `in het concept ‘${name}’`,
    queued: (ahead) =>
      ahead === 1
        ? 'In de wachtrij: er gaat nog één taak voor.'
        : `In de wachtrij: er gaan nog ${ahead} taken voor.`,
    starting: 'Starten…',
    working: 'Bezig met de wijziging',
    leaveOpen: 'De agent gaat door, ook als je een ander onderdeel opent of deze pagina sluit.',
    stop: 'Stoppen',
    stopping: 'Stoppen…',
    stopTitle: 'Deze wijziging stoppen?',
    stopText:
      'Wat de agent al heeft opgeslagen, blijft bewaard: in het concept, of als versie. Wat online staat, blijft online, tenzij hij de wijziging al heeft samengevoegd en online heeft gezet.',
    keepGoing: 'Laten doorgaan',
    stopIt: 'Stoppen',
    log: 'Wat de agent deed',
    took: (time) => `duurde ${time}`,
    steps: {
      guide: 'Leest de platformgids',
      demos: "Bekijkt de demo's",
      read: 'Leest de code',
      readFile: (file) => <>Leest {file}</>,
      logs: 'Leest de logs',
      logsPreview: 'Leest de logs van de voorvertoning',
      readFeature: 'Leest het concept',
      create: 'Maakt een lambda aan',
      feature: 'Start een concept',
      featureStarted: (name) => <>Heeft het concept {name} gestart</>,
      update: 'Werkt de notities van het concept bij',
      rebase: (version) => `Baseert het concept op versie ${version}`,
      merge: 'Voegt het concept samen',
      discard: 'Verwijdert een concept',
      write: (files) => <>Wijzigt {files}</>,
      writeAll: (files) => <>Schrijft {files}</>,
      removing: (files) => <>, verwijdert {files}</>,
      more: (count) => `+${count} meer`,
      check: 'Compileert de code',
      deploy: 'Zet het online',
      deployPreview: 'Zet de voorvertoning online',
      deployVersion: (version) => `Zet versie ${version} online`,
      upload: (path) => <>Slaat {path} op</>,
      delete: (path) => <>Verwijdert {path}</>,
      list: 'Bekijkt de opgeslagen bestanden',
      other: (tool) => `Gebruikt ${tool}`,
    },
    marks: {
      version: (version) => `v${version}`,
      online: 'online',
      previewOnline: 'voorvertoning online',
      compiles: 'compileert',
      errors: (count) => many(count, 'fout', 'fouten'),
      problems: (count) => many(count, 'fout', 'fouten'),
      clean: 'geen fouten',
      refused: 'geweigerd',
    },
    results: {
      online: (version) => `Versie ${version} staat online`,
      ready: (version) => `Versie ${version} staat klaar`,
      readyNote: 'De code compileert. Bekijk wat er veranderd is en deploy de versie als je tevreden bent.',
      saved: (version) => `Opgeslagen als versie ${version}`,
      notOnline: 'De versie is niet online gegaan.',
      broken: (version) => `Versie ${version} is opgeslagen, maar compileert niet`,
      stillOnline: (version) => `Versie ${version} staat nog online.`,
      nothingOnline: 'Er is niets nieuws online gegaan.',
      unchanged: 'Er is niets veranderd',
      stopped: 'Gestopt',
      stoppedSaved: (version) => `Daarvoor was versie ${version} al opgeslagen.`,
      stoppedFeature: (name) => `Wat de agent tot dan toe deed, staat in het concept ‘${name}’.`,
      feature: (name) => `Klaar om uit te proberen in het concept ‘${name}’`,
      featureBroken: (name) => `Het concept ‘${name}’ compileert nog niet`,
      tryIt: 'Probeer het uit op het eigen adres van het concept. Ben je tevreden, voeg het dan samen: dan wordt het de volgende versie.',
      previewOffline: 'De voorvertoning staat niet online. Deploy hem vanuit het concept om het uit te proberen.',
      previewStill: 'De voorvertoning toont nog wat het laatst compileerde.',
      notMerged: 'Het is niet samengevoegd, dus er is niets nieuws online gegaan.',
      failed: 'De wijziging is niet gelukt',
      timeout:
        'De tijd was op voordat de agent iets had veranderd. Vraag om iets kleiners, of probeer het opnieuw: de ene keer komt hij verder dan de andere.',
      turns:
        'De agent had al zijn stappen opgebruikt voordat hij iets had veranderd. Vraag om iets kleiners, of splits het op in twee wijzigingen.',
      timedOut: 'De tijd was op, dus verder dan dit kwam de agent niet.',
      usedUp: 'De agent had al zijn stappen opgebruikt, dus verder dan dit kwam hij niet.',
      unauthorised:
        'De agent kon niet inloggen, dus er is niets gebeurd. Dat ligt aan de inloggegevens van deze server, niet aan wat jij vroeg.',
      nothing: 'De agent stopte zonder iets te veranderen of te zeggen waarom.',
    },
    seeChanges: 'Wijzigingen bekijken',
    open: 'Openen',
    openPreview: 'Voorvertoning openen',
    openFeature: 'Concept openen',
    deploy: (version) => `Versie ${version} deployen`,
    undo: (version) => `Terug naar versie ${version}`,
    undoTitle:
      'De wijziging ongedaan maken: de versie die eerst online stond, gaat weer online. De nieuwe blijft in de geschiedenis.',
    again: 'Opnieuw proberen',
    toast: {
      online: (version) => `De wijziging staat online als versie ${version}.`,
      saved: (version) => `De wijziging is opgeslagen als versie ${version}.`,
      feature: (name) => `De wijziging staat klaar om uit te proberen in het concept ‘${name}’.`,
      unchanged: 'De agent heeft niets veranderd.',
      failed: 'De wijziging is niet gelukt.',
      stopped: 'De wijziging is gestopt.',
    },
    offTitle: 'Deze installatie heeft geen agent',
    off: 'Deze server heeft geen eigen agent om wijzigingen mee te maken. Jouw eigen agent kan het wel: koppel hem aan het adres hieronder, geef hem de editorlink en vertel wat er anders moet.',
    own: 'Je eigen agent gebruiken',
    ownText:
      'Elke agent die MCP spreekt, kan deze lambda aanpassen: koppel hem aan dit adres, geef hem de editorlink en vertel wat er anders moet. Hij heeft geen daglimiet en geen tijdslimiet.',
    mcp: 'MCP-adres',
    editorLink: 'Editorlink (houd hem voor jezelf)',
  },

  summary: {
    reading: 'Status laden…',
    hint: (since, kept, retention, tier) =>
      `Het verkeer wordt geteld sinds de laatste start van de server (${since}). ` +
      (kept
        ? `Een lambda blijft online zolang mensen hem gebruiken. Na ${retention} dagen zonder bezoek en zonder wijzigingen wordt hij verwijderd.`
        : `Deze lambda heeft het pakket ${tier}. Daardoor blijft hij online en bewaard, hoe stil het ook wordt.`),
    onlineFor: (duration, version) => (
      <>
        Al {duration('een tijdje')} online, met versie {version}.
      </>
    ),
    offline: 'Offline. Er wordt niets geserveerd tot je een versie deployt.',
    nothing: 'Er is nog niets geschreven.',
    requestsToday: 'requests vandaag',
    lastHour: (count) => `${count} in het afgelopen uur`,
    hourly: 'Requests per uur, afgelopen dag',
    failed: 'mislukt',
    failedTitle: (failed, rejected) =>
      `${many(failed, 'serverfout', 'serverfouten')}, ${rejected} niet gevonden of geweigerd, afgelopen dag`,
    average: 'gemiddelde responstijd',
    noneYet: 'nog geen',
    lastVisit: 'laatste bezoek',
    problems: 'Er ging onlangs iets mis',
    openLog: 'Logs openen',
    latest: 'Laatste wijziging',
    allVersions: 'Alle versies',
    noDescription: 'Geen beschrijving',
    version: (version) => `Versie ${version}`,
    notOnline: 'nog niet online',
    wanted: 'Wat er gevraagd werd',
    noVersions: 'Nog geen versies.',
    inProgress: 'In de maak',
    allFeatures: 'Alle concepten',
    previewOnline: 'De voorvertoning staat online',
    previewOffline: 'De voorvertoning staat offline',
    behind: 'loopt achter',
    storage: 'Opslag',
    inVersion: (version) => `In versie ${version}`,
    noVersion: 'In de versie',
    inData: 'In de data',
    sharedByAll: 'Gedeeld door elke versie',
    browse: 'Bekijken',
    code: 'Code',
    codeWhy: 'C# wordt gecompileerd, nooit geserveerd.',
    characters: 'tekens',
    assets: 'Assets',
    assetsPublic: 'Openbaar: de code serveert ze.',
    assetsPrivate: 'Niet geserveerd door de code.',
    data: 'Data',
    workspace: 'Workspace',
    workspaceOff: 'uitgezet',
    dataPublic: 'Openbaar: de code serveert de workspace.',
    dataPrivate: 'Alleen voor de lambda zelf.',
  },

  files: {
    hint: (b) => (
      <>
        De bestanden van één versie: het programma. {b('Code')} wordt gecompileerd en nooit geserveerd.{' '}
        {b('Assets')}, zoals pagina's, scripts, stylesheets en afbeeldingen, worden met de code opgeslagen, samen ermee
        gedeployd en teruggezet, en zijn openbaar als de code ze serveert. Wat de lambda bewaart terwijl hij draait,
        staat hier niet: dat is zijn {b('Data')}.
      </>
    ),
    scope: (version, data) => (
      <>
        Deze bestanden horen bij versie {version} en veranderen mee. Wat de lambda bewaart terwijl hij draait, is voor
        elke versie hetzelfde en staat onder {data('Data')}.
      </>
    ),
    edit: 'Deze versie bewerken',
    version: 'Versie',
    shown: (version, online, newest) => `Versie ${version}${online ? ', online' : newest ? ', nieuwste' : ''}`,
    optionOnline: ' (online)',
    readFailed: 'Die versie kon niet worden gelezen.',
    noVersion: 'Er is nog geen versie om te tonen.',
    label: 'Bestanden',
    code: 'Code',
    codeWhy: 'Gecompileerd in de lambda, nooit geserveerd.',
    count: (files) => many(files, 'bestand', 'bestanden'),
    codeUsage: (files, used, of) => `${files}, ${used} van ${of} tekens`,
    usage: (files, used, of) => `${files}, ${used} van ${of}`,
    noCode: 'Geen code in deze versie.',
    assets: 'Assets',
    assetsPublic: 'Openbaar: deze versie serveert ze met Assets.',
    assetsPrivate: 'Opgeslagen met de code, maar deze versie serveert ze niet.',
    noAssets: 'Geen assets in deze versie.',
    data: 'Data',
    dataPublic: 'Openbaar: de code die online staat, serveert de data met Workspace.',
    dataPrivate: 'Alleen voor de lambda zelf. Hoort bij geen enkele versie.',
    uploadFailed: (path) => `${path} kon niet worden geüpload.`,
    deleteFolder: (path, held) =>
      held > 0
        ? `${path} verwijderen, met ${held === 1 ? 'het bestand' : `de ${held} bestanden`} erin?`
        : `De map ${path} verwijderen?`,
    deleteFile: (path) => `${path} verwijderen? De lambda kan het dan niet meer vinden.`,
    deleteFailed: 'Verwijderen is niet gelukt.',
    full: 'De data-opslag is vol',
    uploadInto: (folder) => `Uploaden naar ${folder}`,
    upload: 'Uploaden',
    reading: 'Lezen…',
    noData: 'Nog niets. Wat de lambda opslaat terwijl hij draait, verschijnt hier.',
    delete: (path) => `${path} verwijderen`,
    deleteShort: 'Verwijderen',
    fileFailed: 'Het bestand kon niet worden gelezen.',
    pick: 'Kies een bestand om te zien wat erin staat.',
    tooLarge: (name, size) => (
      <>
        {name} is {size}, te groot om hier te tonen.
      </>
    ),
    download: 'Downloaden',
    readingFile: (name) => `${name} lezen…`,
    missing: (name) => `Deze versie heeft geen bestand met de naam ${name}.`,
    saved: 'opgeslagen',
    notText: 'Geen tekst. Download het om erin te kijken.',
  },

  data: {
    hint:
      'Data is wat de lambda bewaart terwijl hij draait. Het hoort bij de lambda, niet bij een versie: elke versie leest en schrijft dezelfde data, en niets wat je met versies doet, verandert eraan. Een concept wordt uitgeprobeerd op een kopie ervan. De data verdwijnt pas als de lambda wordt verwijderd, of als je dat soort data uitzet.',
    facts: [
      ['Gedeeld door elke versie', 'Welke versie er ook online staat, hij leest en schrijft dezelfde data.'],
      ['Blijft bij een deploy', 'Deployen, terugzetten of een concept samenvoegen raakt het nooit aan.'],
      ['Jij zet het aan of uit', 'Elk soort staat alleen aan zolang jij dat wilt. Zet je er een uit, dan wordt verwijderd wat erin zit.'],
    ],
    featureHint:
      'De data waarmee dit concept werkt: een kopie van die van de lambda, gemaakt toen het concept begon. De voorvertoning leest en schrijft de kopie, dus wat je hier uitprobeert, raakt nooit aan wat de lambda bewaart. Voeg je het concept samen, dan wordt de kopie weggegooid en blijft de data van de lambda zoals die is.',
    recopy: 'De data van de lambda opnieuw kopiëren',
    recopyTitle: 'Deze kopie vervangen door wat de lambda nu bevat',
    recopyConfirm: 'De data van de lambda opnieuw kopiëren?',
    recopyText:
      'Alles in deze kopie, ook wat de voorvertoning erin schreef, wordt vervangen door wat de lambda nu bevat. De data van de lambda zelf blijft onaangeroerd.',
    keepCopy: 'Deze kopie houden',
    recopied: 'De kopie is ververst. De voorvertoning gebruikt hem vanaf het volgende request.',
    recopyFailed: 'De data kon niet opnieuw worden gekopieerd.',
    copyContents: 'Wat er in de kopie van de workspace staat',
    kinds: {
      workspace: {
        name: 'Workspace',
        what: 'Bestanden die de lambda leest en schrijft terwijl hij draait: uploads, gegevens, alles wat hij bewaart.',
      },
    },
    on: 'Aan',
    off: 'Uit',
    byDefault: 'Standaard aan',
    usage: (items, used, of) => `${items} · ${used} van ${of}`,
    offText: 'Uitgezet. Er staat niets in, en code die het gebruikt, mislukt tot je het weer aanzet.',
    switchLabel: (name) => `${name} aan- of uitzetten`,
    confirmOff: (name) => `De ${name.toLowerCase()} uitzetten?`,
    confirmText: (items, size) =>
      `Alles wat erin staat (${items}, ${size}) wordt definitief verwijderd. Dit kun je niet ongedaan maken.`,
    confirmEmpty: 'Er staat niets in, dus er gaat niets verloren.',
    inUse: 'De versie die online staat, gebruikt het. Waar hij dat doet, mislukt hij tot je het weer aanzet.',
    deleteAndOff: 'Uitzetten en verwijderen',
    keep: 'Aan laten',
    switchedOn: (name) => `De ${name.toLowerCase()} staat aan. De lambda kan hem gebruiken vanaf zijn volgende request.`,
    switchedOff: (name) => `De ${name.toLowerCase()} staat uit, en wat erin stond is verwijderd.`,
    switchFailed: 'Omzetten is niet gelukt.',
    readFailed: 'De data kon niet worden gelezen.',
    demo: 'Een demo: de data is er om te bekijken, niet om te veranderen.',
    contents: 'Wat er in de workspace staat',
    browse: 'Bestanden',
    offBrowse: 'De workspace staat uit, dus er zijn geen bestanden om te tonen.',
  },

  features: {
    hint:
      'Een versie verandert nooit meer als hij eenmaal is opgeslagen. Een wijziging maak je in plaats daarvan in een concept: dat begint als kopie van een versie en van de data van de lambda, je kunt het uitproberen op een eigen adres terwijl bezoekers blijven krijgen wat er online staat, en het wordt de volgende versie als je het samenvoegt.',
    newFeature: 'Nieuw concept',
    full: (limit) => `Deze lambda heeft ${limit} open concepten, en meer mag niet. Voeg er eerst een samen of verwijder er een.`,
    emptyTitle: 'Er wordt nergens aan gewerkt',
    emptyText:
      'Start een concept om de lambda aan te passen zonder wat er online staat aan te raken. Jij of de agent kan het zo vaak aanpassen als nodig en het uitproberen op zijn eigen adres.',
    start: 'Concept starten',
    askAgentNew: 'De agent om een wijziging vragen',
    noChange: 'Nog niet beschreven wat het verandert',
    behindTitle: 'Er is een nieuwere versie opgeslagen nadat het concept begon',
    behind: (newest) => `versie ${newest} is nieuwer`,
    previewOnline: 'voorvertoning online',
    previewOutdated: 'voorvertoning niet bijgewerkt',
    previewOffline: 'voorvertoning offline',
    changed: 'gewijzigd',
    openPreview: 'Voorvertoning',
    openPreviewTitle: 'De voorvertoning openen in een nieuw tabblad',
    count: (open, limit) => `${open} van ${limit} concepten open`,
    loading: 'Concept laden…',
    readFailed: 'Het concept kon niet worden gelezen.',

    newTitle: 'Concept starten',
    newText:
      'Een concept begint als kopie van een versie (de code en de assets) en van de data van de lambda. Pas het aan en probeer het uit op een eigen adres terwijl bezoekers blijven krijgen wat er online staat; voeg het samen zodra het goed is.',
    newTextFiles:
      'Wat je in de code hebt getypt, komt erin, in plaats van dat het een versie wordt. Probeer het uit op het eigen adres van het concept, en voeg het samen zodra het goed is.',
    name: 'Naam',
    namePlaceholder: 'Ranglijst',
    wanted: 'Wat moet het doen?',
    wantedPlaceholder: 'Optioneel. Bewaar de tien beste scores en toon ze na elk spel.',
    olderBase: (newest) =>
      `Niet de nieuwste: voordat het kan worden samengevoegd, moet het eerst overnemen wat de versies tot en met ${newest} veranderden.`,
    create: 'Starten',
    createFailed: 'Het concept kon niet worden gestart.',
    retry: 'Opnieuw proberen',
    madeNotSaved: (name) =>
      `Het concept ‘${name}’ is gestart, maar wat je hebt getypt, kon er nog niet in worden opgeslagen. Probeer het opnieuw, of sluit dit en zoek het concept op onder Concepten.`,
    created: (name) => `Het concept ‘${name}’ is gestart.`,
    cancel: 'Annuleren',

    featureHint:
      'Een wijziging waaraan naast de lambda wordt gewerkt. De voorvertoning draait de code ervan met een eigen kopie van de data, dus bezoekers van de lambda zien er niets van. Samenvoegen maakt er de volgende versie van.',
    askAgent: 'De agent vragen',
    askCatchUp: 'De agent vragen het bij te werken',
    catchUp: 'Werk dit concept bij naar de nieuwste versie van de app, en behoud wat het verandert.',
    editCode: 'Code bewerken',
    deployPreview: 'Voorvertoning deployen',
    updatePreview: 'Voorvertoning bijwerken',
    previewDeployed: 'De voorvertoning staat online.',
    previewFailed: 'De voorvertoning kon niet online worden gezet.',
    previewStopped: 'De voorvertoning staat offline.',
    previewRejected: 'De voorvertoning is niet veranderd',
    previewNotCompiling: 'De code compileert niet. De voorvertoning toont nog wat hij eerder toonde.',
    started: 'Gestart',
    changes: (version) => `Wat het verandert ten opzichte van versie ${version}`,
    noChanges: (version) => `Nog niets: het bevat precies wat versie ${version} bevat.`,
    editNotes: 'Naam en notities',
    what: 'Wat verandert het?',
    whatPlaceholder: 'Voegt een ranglijst toe met de tien beste scores',
    missed: (from, to) =>
      to - from === 1 ? `Wat versie ${to} veranderde` : `Wat versies ${from + 1} tot en met ${to} veranderden`,
    missedNothing: 'Niets in de bestanden.',

    behindText: (base, newest) =>
      `Het begon bij versie ${base}, en sindsdien is versie ${newest} opgeslagen. Het nu samenvoegen zou ongedaan maken wat die veranderde. Haal die wijzigingen naar het concept (of vraag de agent dat te doen) en geef daarna aan dat het gebaseerd is op versie ${newest}.`,
    moveBase: 'Op een andere versie baseren',
    close: 'Sluiten',
    mergeTitle: (name) => `‘${name}’ samenvoegen`,
    mergeTitleShort: 'Er de volgende versie van maken',
    leaks: (path, files) =>
      `In ${files} wordt naar ${path} gelinkt met het volledige pad. Vanuit de voorvertoning is dat de lambda die online staat, met de echte data, niet de kopie van dit concept. Relatieve paden (‘api/items’) blijven in de voorvertoning.`,
    mergeButton: 'Samenvoegen',
    saveFirst: 'Sla eerst de code op: de voorvertoning en het samenvoegen gebruiken wat is opgeslagen.',
    mergeAndDeploy: (version) => `Samenvoegen en versie ${version} online zetten`,
    mergeText: (version) =>
      `Het wordt versie ${version}. Het concept verdwijnt dan, met zijn voorvertoning en zijn kopie van de data. De data van de lambda zelf blijft zoals die is.`,
    deployTooNote: (active) => `Met één klik ga je in de versies terug naar versie ${active}.`,
    deployTooOffline: 'De lambda staat nu offline; hiermee gaat hij online.',
    notCompiling: 'De code compileert niet, dus het concept is niet samengevoegd. Los het eerst op in het concept.',
    mergeFailed: 'Het concept kon niet worden samengevoegd.',
    merged: (version) => `Samengevoegd als versie ${version}.`,
    mergedOnline: (version) => `Samengevoegd als versie ${version}, en online.`,

    notesTitle: 'Naam en notities',
    save: 'Opslaan',
    saveFailed: 'Dat kon niet worden opgeslagen.',

    baseTitle: 'Op een andere versie baseren',
    baseText: (base) =>
      `Het is gebaseerd op versie ${base}. Alleen een concept dat op de nieuwste versie is gebaseerd, kan worden samengevoegd. Zo maakt samenvoegen nooit ongedaan wat er is opgeslagen nadat het concept begon. Bevat het alles wat een nieuwere versie veranderde, geef dat dan hier aan.`,
    moveTo: (version) => `Baseren op versie ${version}`,
    baseWarning: 'Er wordt niet gecontroleerd of die wijzigingen echt in het concept zitten. Samenvoegen zonder die wijzigingen maakt ze ongedaan.',

    deleteTitle: (name) => `‘${name}’ verwijderen?`,
    deleteText:
      'De code, de voorvertoning en de kopie van de data worden definitief verwijderd. De lambda en zijn versies blijven onaangeroerd.',
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
      code: 'Code',
      data: 'Data',
      logs: 'Logs',
    },
    missingTitle: 'Dit concept bestaat niet meer',
    missingText: 'Het is samengevoegd tot een versie, of verwijderd. In de versies zie je wat ermee gebeurd is.',
  },

  versions: {
    hint: (limit) =>
      `Een versie is het programma (de code en de assets) en verandert nooit meer als hij eenmaal is opgeslagen. Zo kun je elke versie vergelijken en precies zoals hij was weer online zetten. Elke versie bewaart wat er gevraagd werd en wat er veranderde. Wil je de lambda aanpassen, start dan een concept: dat wordt de volgende versie zodra het goed is. Bij meer dan ${limit} versies verdwijnen de oudste. De versie die online staat, verdwijnt nooit.`,
    none: 'Nog geen versies.',
    noDescription: 'Geen beschrijving',
    online: 'online',
    putOnline: 'Deze versie online zetten',
    rollBackTitle: 'Deze oudere versie weer online zetten',
    deploy: 'Deployen',
    rollBack: 'Terugzetten',
    readFailed: 'Deze versie kon niet worden gelezen.',
    comparing: 'Vergelijken…',
    unchanged: 'Niets veranderd ten opzichte van de vorige versie.',
    first: 'De eerste versie.',
    status: { added: 'toegevoegd', removed: 'verwijderd', changed: 'gewijzigd', same: 'gelijk' },
    browse: 'Bestanden bekijken',
    edit: 'Vanaf hier bewerken',
    feature: 'Vanaf hier een concept starten',
    featureTitle:
      'Naast de lambda aan een wijziging van deze versie werken, en die samenvoegen tot de volgende versie zodra het goed is',
    binary: 'Geen tekst, dus er zijn geen regels om te vergelijken.',
    tooLarge: 'Te groot om regel voor regel te vergelijken.',
  },

  deployments: {
    hint: (until) =>
      `Een deployment blijft online zolang mensen hem gebruiken${until ? `. Gebruikt niemand hem, dan blijft hij online tot ${until}` : ''}. Opnieuw deployen, of elk bezoek, zet die klok weer op nul.`,
    takeOffline: 'Offline halen',
    readFailed: 'De geschiedenis kon niet worden gelezen.',
    reading: 'Geschiedenis laden…',
    none: 'Er is nog niets gedeployd.',
    noDescription: 'Geen beschrijving',
    deployed: (when, by) => `Gedeployd op ${when} (${by})`,
    duration: 'Hoe lang hij online stond',
    online: 'online',
    short: {
      replaced: 'vervangen',
      stopped: 'offline gehaald',
      expired: 'verlopen',
      admin: 'door beheerder',
      ended: 'beëindigd',
    },
    putBack: (version) => `Versie ${version} weer online zetten`,
    timeline: 'Wat er de afgelopen zeven dagen online stond',
    block: (version, from, to) => `Versie ${version}, ${from} tot ${to ?? 'nu'}`,
    weekAgo: 'een week geleden',
    now: 'nu',
  },

  stats: {
    readFailed: 'De cijfers konden niet worden gelezen.',
    range: 'Periode',
    lastHour: 'Afgelopen uur',
    lastDay: 'Afgelopen dag',
    hint: (since) =>
      `In het geheugen geteld sinds de laatste start van de server, ${since}. Na een herstart beginnen deze cijfers opnieuw.`,
    reading: 'Cijfers laden…',
    requests: 'requests',
    websockets: (count) => `en ${many(count, 'websocketverbinding', 'websocketverbindingen')}`,
    failed: 'mislukt',
    serverErrors: (count) => many(count, 'serverfout', 'serverfouten'),
    rejected: 'niet gevonden of geweigerd',
    average: 'gemiddelde responstijd',
    sent: (amount) => `${amount} verstuurd`,
    nobody: (hour) =>
      hour ? 'Niemand heeft hem het afgelopen uur aangeroepen.' : 'Niemand heeft hem de afgelopen dag aangeroepen.',
    requestsTitle: 'Requests',
    per: (hour) => (hour ? 'Per minuut.' : 'Per 15 minuten.'),
    answered: 'Beantwoord',
    rejectedSeries: 'Niet gevonden of geweigerd',
    failedSeries: 'Mislukt',
    timeTitle: 'Responstijd',
    averagePer: (hour) => (hour ? 'Het gemiddelde per minuut.' : 'Het gemiddelde per 15 minuten.'),
    averageSeries: 'Gemiddeld',
    mostAsked: 'Meest opgevraagd',
    path: 'Pad',
    requestsColumn: 'Requests',
    failedColumn: 'Mislukt',
    averageColumn: 'Gemiddeld',
    since: 'Sinds de start van de server.',
  },

  logs: {
    readFailed: 'De logs konden niet worden gelezen.',
    hint: (capturing) =>
      'Requests, output van de lambda en wat er misging, live.' +
      (capturing ? '' : " Deze installatie bewaart de output van lambda's niet, dus je ziet alleen requests en fouten.") +
      " De logs staan in het geheugen, dat alle lambda's hier delen. Ze gaan dus minuten tot uren terug, en zijn leeg na een herstart. Adressen van bezoekers worden niet getoond.",
    featureHint: (capturing) =>
      'Requests, output en fouten van de voorvertoning van dit concept, live.' +
      (capturing ? '' : " Deze installatie bewaart de output van lambda's niet, dus je ziet alleen requests en fouten.") +
      ' Ze staan los van de logs van de lambda zelf, die de voorvertoning nooit tonen. De logs staan in het geheugen, dus ze gaan minuten tot uren terug.',
    nothingPreview: 'Nog niets. Open de voorvertoning van het concept, dan verschijnen de requests hier.',
    search: 'Zoeken',
    searchLabel: 'Zoeken in de logs',
    resume: 'Nieuwe regels tonen zodra ze binnenkomen',
    pause: 'Geen nieuwe regels toevoegen terwijl je leest',
    paused: 'Gepauzeerd',
    live: 'Live',
    show: 'Tonen',
    all: 'Alles',
    requests: 'Requests',
    output: 'Output',
    problems: 'Problemen',
    reading: 'Logs laden…',
    noProblems: 'Er is niets misgegaan, voor zover de logs nog weten.',
    nothing: 'Nog niets. Open het adres van de lambda, dan verschijnen de requests hier.',
    noMatch: 'Geen resultaten.',
    identical: (count) => `${count} identieke regels`,
    at: (domain) => `, op ${domain}`,
    from: (country) => `, uit ${country}`,
  },

  showcase: {
    loadFailed: 'De showcase kon niet worden geladen.',
    loading: 'Laden…',
    title: 'een titel',
    description: 'een beschrijving',
    picture: 'een afbeelding',
    updated: 'Je vermelding in de showcase is bijgewerkt.',
    listed: 'Hij staat nu op de showcasepagina.',
    waiting: 'Opgeslagen. Hij verschijnt op de showcasepagina zodra de lambda online is.',
    saveFailed: 'Je vermelding in de showcase kon niet worden opgeslagen.',
    removed: 'Van de showcasepagina gehaald.',
    removeFailed: 'Je vermelding kon niet uit de showcase worden gehaald.',
    wrongType: 'Dat is geen PNG-, JPEG-, GIF- of WebP-afbeelding.',
    tooLarge: (size, limit) => `Dat is ${size}; een afbeelding mag maximaal ${limit} zijn.`,
    unreadable: 'Dat bestand kon niet worden gelezen.',
    hint: (tool) => (
      <>
        De showcasepagina toont lambda's die hun eigenaars willen laten zien, recent gebruikte eerst. Alleen wie de
        editorsleutel heeft, kan een lambda erop zetten of eraf halen, en hij staat er alleen zolang hij online is. Een
        agent kan hetzelfde met de tool {tool}.
      </>
    ),
    open: 'Showcase openen',
    switch: 'Deze lambda op de showcasepagina tonen',
    listedNow: 'Staat erop. Iedereen die door de showcase bladert, kan hem openen.',
    notListed: 'Opgeslagen, maar niet zichtbaar: de lambda is offline. Hij verschijnt weer zodra hij opnieuw is gedeployd.',
    off: 'Uit. Er wordt nergens iets van deze lambda getoond tot je dit aanzet en opslaat.',
    offline: "De lambda is offline, dus de vermelding wacht tot hij gedeployd is. Alleen lambda's die reageren, staan erop.",
    titleLabel: 'Titel',
    titlePlaceholder: 'Scorebord voor de pubquiz',
    descriptionLabel: 'Beschrijving',
    descriptionPlaceholder:
      'Teams vullen hun antwoorden in op hun telefoon, de quizmaster kijkt ze na en het scorebord werkt voor iedereen in de zaal bij.',
    save: 'Wijzigingen opslaan',
    add: 'Toevoegen aan de showcase',
    takeOff: 'Eraf halen',
    needs: (missing) =>
      `Nog nodig: ${missing.length > 1 ? `${missing.slice(0, -1).join(', ')} en ${missing[missing.length - 1]}` : missing[0]}.`,
    tooLong: 'Sommige velden zijn te lang.',
    allSaved: 'Alles is opgeslagen.',
    preview: 'Voorbeeld',
    card: (address) => <>Dit is de kaart die bezoekers zien. Hij opent {address}.</>,
    confirm: 'Uit de showcase halen?',
    keep: 'Laten staan',
    confirmText: 'De titel, beschrijving en afbeelding worden verwijderd. De lambda zelf blijft precies zoals hij is.',
    pictureLabel: 'Afbeelding',
    formats: (limit) => `PNG, JPEG, GIF of WebP, tot ${limit}`,
    notSaved: 'nog niet opgeslagen',
    replace: 'Sleep hier een nieuwe heen om hem te vervangen.',
    drop: 'Sleep hier een afbeelding heen.',
    advice: 'Een screenshot, of een korte GIF van de app in gebruik, werkt het best in 16:10.',
    another: 'Andere kiezen',
    choose: 'Bestand kiezen',
    keepSaved: 'Opgeslagen afbeelding houden',
    clear: 'Wissen',
  },

  domain: {
    readFailed: 'Het domein kon niet worden gelezen.',
    reaching: (domain) => `Requests naar ${domain} komen nu bij deze lambda uit.`,
    saveFailed: 'Het domein kon niet worden opgeslagen.',
    removed: 'Het domein is verwijderd. De lambda reageert nog gewoon op zijn adres hier.',
    removeFailed: 'Het domein kon niet worden verwijderd.',
    hint:
      'Een premium lambda kan naast zijn adres hier ook op een eigen domein draaien, helemaal vanaf de root. Laat het domein naar deze server wijzen en vul het hier in. Dan komen requests naar dat domein bij de lambda uit.',
    loading: 'Laden…',
    example: 'jouw-domein.nl',
    open: (domain) => `${domain} openen`,
    label: 'Het domein waarop hij reageert',
    serving: (domain) => <>Draait nu op {domain}, naast zijn adres hier.</>,
    none: 'Nog geen. Een subdomein zoals shop.example.com, of een heel domein zoals example.com.',
    change: 'Wijzigen',
    use: 'Dit domein gebruiken',
    remove: 'Verwijderen',
    confirm: 'Domein verwijderen?',
    keep: 'Laten staan',
    confirmText: (domain) => (
      <>
        Requests naar {domain} komen meteen niet meer bij deze lambda uit. Het adres hier blijft zoals het is, en de
        DNS-instellingen van het domein ook.
      </>
    ),
    point: 'Laat het domein naar deze server wijzen',
    check: 'Opnieuw controleren',
    records:
      'Voeg deze twee records toe bij de partij die de DNS van het domein beheert. Laat het AAAA-record weg als je liever niet via IPv6 bereikbaar bent.',
    type: 'Type',
    name: 'Naam',
    value: 'Waarde',
    pointsHere: (domain) => <>{domain} wijst hierheen.</>,
    alsoElsewhere: (addresses) =>
      ` Het verwijst ook naar ${addresses}, en dat is niet deze server. Bezoekers die daar terechtkomen, bereiken de lambda niet.`,
    elsewhere: (addresses) => `Het verwijst naar ${addresses}, en dat is nog niet deze server.`,
    wait: 'Het kan even duren voordat een wijziging overal zichtbaar is, maximaal de TTL van het oude record.',
    cname: 'Een CNAME-record gebruiken',
    cnameText: (target) => (
      <>
        Een subdomein kan ook met een CNAME-record naar {target} wijzen. Dan volgt het deze server als zijn adressen
        ooit veranderen. Er zitten wel nadelen aan:
      </>
    ),
    cnameRoot: (example) => (
      <>
        Het werkt niet voor een heel domein ({example} zelf): de standaard staat geen CNAME toe naast de records die
        elk domein op zijn root heeft. Sommige providers bieden daar een ALIAS-, ANAME- of ‘flattened’ record voor.
      </>
    ),
    cnameAlone: 'Er kan niets anders op dezelfde naam staan: geen MX-record voor mail, geen TXT-record voor verificaties.',
    cnameLookup: 'De resolvers van bezoekers doen één lookup extra voordat ze aankomen.',
    copy: 'Kopiëren',
    copyValue: (value) => `${value} kopiëren`,
  },

  code: {
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
    files: (entry, cs) => (
      <>
        {entry} geeft terug wat er geserveerd wordt, andere {cs}-bestanden bevatten types, en elk ander bestand wordt
        geserveerd zoals het is. Ctrl-S slaat op, F12 springt naar een declaratie.
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
  },

  tabs: {
    codeName: 'Letters, cijfers, streepjes en underscores, eindigend op .cs',
    slashes: 'Geen slash aan het begin of eind, en korter dan 120 tekens.',
    deep: 'Maximaal zes mappen diep.',
    characters: 'Letters, cijfers, streepjes, underscores en punten, gescheiden door slashes.',
    extension: 'Het bestand heeft een extensie nodig, zodat het als het juiste type geserveerd wordt.',
    exists: 'Er is al een bestand met die naam.',
    remove: (name) => `${name} verwijderen? De inhoud gaat mee.`,
    there: (name) => `${name} bestaat al.`,
    entry: 'De snippet: wat hij teruggeeft, wordt geserveerd',
    errors: 'bevat fouten',
    removeFile: (name) => `${name} verwijderen`,
    removeTitle: 'Dit bestand verwijderen',
    placeholder: 'Types.cs of site/index.html',
    newFile: 'Nieuw bestand',
    uploadTitle: 'Een bestand uploaden: een afbeelding, een font, een pagina',
    upload: 'Bestand uploaden',
  },
};
