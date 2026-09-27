import type { EditorMessages } from '../en/editor';

/** Die Texte des Editors auf Deutsch. */
export const editor: EditorMessages = {
  shared: {
    units: { s: 's', min: 'min', h: 'h', d: 'd' },
    never: 'nie',
    justNow: 'gerade eben',
    ago: (span) => `vor ${span}`,
    in: (span) => `in ${span}`,
    origins: {
      agent: 'Agent',
      template: 'Vorlage',
      admin: 'Betreiber',
      system: 'Plattform',
      api: 'API / Editor',
      unknown: 'unbekannt',
    },
    endings: {
      replaced: 'durch eine neuere Bereitstellung ersetzt',
      stopped: 'offline genommen',
      expired: 'wegen Nichtnutzung abgelaufen',
      admin: 'vom Betreiber offline genommen',
      ended: 'beendet',
    },
    whatThisIs: 'Erläuterung',
    byAgent: 'von einem Agenten',
    writtenByAgent: 'Von einem Agenten geschrieben',
    more: 'Mehr',
    of: (used, total) => `${used} von ${total}`,
    online: (version) => `Online · v${version}`,
    onlineTitle: (version) => `Online, Version ${version} wird ausgeliefert`,
    offline: 'Offline',
    offlineTitle: 'Offline: Es wird nichts ausgeliefert',
    premium:
      'Premium: kann unter einer eigenen Domain erreichbar sein, bietet mehr Platz für Code, Assets und Daten und bleibt unabhängig von der Nutzung online',
    demo: 'Demo: von dieser Installation online gehalten und schreibgeschützt',
    tier: (tier) => `Tarif ${tier}`,
    entrances: {
      title: 'Aufgerufen über',
      note: 'Seit dem Start des Servers, einschließlich Websocket-Verbindungen.',
    },
    chart: {
      showChart: 'Diagramm anzeigen',
      showValues: 'Werte anzeigen',
      none: 'Noch keine Messwerte.',
      time: 'Zeit',
    },
    diagnostics: {
      compiles: 'Der Code lässt sich kompilieren.',
      none: 'Noch keine Meldungen. Prüfen oder stellen Sie den Code bereit, um ihn zu kompilieren.',
      line: (line) => `Zeile ${line}`,
    },
  },

  frame: {
    title: 'Editor',
    sections: {
      overview: 'Übersicht',
      showcase: 'Showcase',
      domain: 'Domain',
      files: 'Dateien',
      versions: 'Versionen',
      deployments: 'Bereitstellungen',
      stats: 'Statistik',
      logs: 'Logs',
      code: 'Code',
    },
    sectionsLabel: 'Bereiche',
    loadFailed: 'Dieses Lambda konnte nicht geladen werden.',
    online: (version) => `Version ${version} ist online.`,
    deployFailed: 'Das Lambda konnte nicht bereitgestellt werden.',
    offline: 'Offline genommen. Der Code bleibt erhalten.',
    offlineFailed: 'Das Lambda konnte nicht offline genommen werden.',
    leave: 'Ihre ungespeicherten Änderungen am Code gehen verloren. Trotzdem fortfahren?',
    nothingTitle: 'Dieser Link führt zu keinem Lambda',
    createNew: 'Neues Lambda erstellen',
    loading: 'Ihr Lambda wird geladen …',
    moreActions: 'Weitere Aktionen',
    redeploy: (version) => `Version ${version} erneut bereitstellen`,
    takeOffline: 'Offline nehmen',
    copyLink: 'Link kopieren',
    copyPrivate: 'Privaten Link kopieren',
    privateLink: 'Mit diesem Link kann jede Person das Lambda ändern. Bitte behandeln Sie ihn vertraulich.',
    rename: 'Adresse ändern',
    download: 'Als .NET-Projekt herunterladen',
    delete: 'Lambda löschen',
    deploy: (version) => `Version ${version} bereitstellen`,
    problems: 'Kürzlich sind Fehler aufgetreten',
    demoTitle: 'Eine Demo, von dieser Installation online gehalten und schreibgeschützt.',
    demo: (start) => (
      <>
        Code, Verlauf, gespeicherte Daten und Logs dienen als Anschauungsmaterial. Um Änderungen vorzunehmen,{' '}
        {start('erstellen Sie auf dieser Grundlage ein eigenes Lambda')}.
      </>
    ),
    keep: 'Bitte bewahren Sie diesen Link sicher auf. Er ist der einzige Zugang zu diesem Lambda.',
    gotIt: 'Verstanden',
    rejected: (version) => `Version ${version} wurde nicht bereitgestellt`,
    refused: 'Die Bereitstellung wurde abgelehnt',
    openCode: 'Code öffnen',
    close: 'Schließen',
    notCompiling: 'Der Code lässt sich nicht kompilieren. Die bisher bereitgestellte Version bleibt online.',
    moved: (path) => `Jetzt erreichbar unter ${path}.`,
    deleteTitle: 'Lambda löschen?',
    cancel: 'Abbrechen',
    deleteForGood: 'Endgültig löschen',
    deleteFailed: 'Das Lambda konnte nicht gelöscht werden.',
    deleteText: (key) => (
      <>Alle Versionen, Dateien, der Verlauf und die Adresse {key} werden gelöscht. Dieser Vorgang kann nicht rückgängig gemacht werden.</>
    ),
    openInTab: 'In neuem Tab öffnen',
    open: (address) => `${address} in neuem Tab öffnen`,
    copyAddress: 'Adresse kopieren',
    renameFailed: 'Die Adresse konnte nicht geändert werden.',
    moveIt: 'Adresse ändern',
    renameText: 'Die bisherige Adresse ist sofort nicht mehr erreichbar. Bitte aktualisieren Sie alle Verweise darauf.',
  },

  summary: {
    reading: 'Status wird ermittelt …',
    hint: (since, kept, retention, tier) =>
      `Der Datenverkehr wird seit dem letzten Serverstart gezählt (${since}). ` +
      (kept
        ? `Ein Lambda bleibt online, solange es genutzt wird, und wird nach ${retention} Tagen ohne Aufrufe und Änderungen entfernt.`
        : `Dieses Lambda befindet sich im Tarif ${tier} und bleibt unabhängig von der Nutzung online und gespeichert.`),
    onlineFor: (duration, version) => (
      <>
        Online seit {duration('einiger Zeit')}, Version {version} wird ausgeliefert.
      </>
    ),
    offline: 'Offline. Bis zur Bereitstellung einer Version wird nichts ausgeliefert.',
    nothing: 'Es wurde noch kein Code geschrieben.',
    requestsToday: 'Anfragen heute',
    lastHour: (count) => `${count} in der letzten Stunde`,
    hourly: 'Anfragen pro Stunde in den letzten 24 Stunden',
    failed: 'fehlgeschlagen',
    failedTitle: (failed, rejected) =>
      `${failed} Serverfehler, ${rejected} nicht gefunden oder abgelehnt in den letzten 24 Stunden`,
    average: 'durchschnittliche Antwortzeit',
    noneYet: 'noch keiner',
    lastVisit: 'letzter Aufruf',
    problems: 'Kürzlich aufgetretene Fehler',
    openLog: 'Log öffnen',
    latest: 'Letzte Änderung',
    allVersions: 'Alle Versionen',
    noDescription: 'Keine Beschreibung',
    version: (version) => `Version ${version}`,
    notOnline: 'noch nicht online',
    wanted: 'Anforderung',
    noVersions: 'Noch keine Versionen.',
    storage: 'Speicher',
    browse: 'Anzeigen',
    code: 'Code',
    codeWhy: 'C#-Code wird kompiliert und nie ausgeliefert.',
    characters: 'Zeichen',
    assets: 'Assets',
    assetsPublic: 'Öffentlich: Der Code liefert sie aus.',
    assetsPrivate: 'Werden vom Code nicht ausgeliefert.',
    data: 'Daten',
    dataPublic: 'Öffentlich: Der Code liefert den Workspace aus.',
    dataPrivate: 'Nur für das Lambda zugänglich.',
  },

  files: {
    hint: (b) => (
      <>
        {b('Code')} wird kompiliert und nie ausgeliefert. {b('Assets')} – Seiten, Stylesheets, Bilder – werden mit jeder
        Version gespeichert und sind öffentlich, sofern der Code sie ausliefert. {b('Daten')} schreibt das Lambda zur
        Laufzeit; sie gehören zu keiner Version und sind nur öffentlich, wenn der Code sie ausliefert.
      </>
    ),
    edit: 'Diese Version bearbeiten',
    version: 'Version',
    shown: (version, online, newest) => `Version ${version}${online ? ', online' : newest ? ', neueste' : ''}`,
    optionOnline: ' (online)',
    readFailed: 'Diese Version konnte nicht gelesen werden.',
    dataFailed: 'Die Daten konnten nicht gelesen werden.',
    noVersion: 'Es ist noch keine Version vorhanden.',
    label: 'Dateien',
    code: 'Code',
    codeWhy: 'Wird in das Lambda kompiliert und nie ausgeliefert.',
    count: (files) => (files === 1 ? '1 Datei' : `${files} Dateien`),
    codeUsage: (files, used, of) => `${files}, ${used} von ${of} Zeichen`,
    usage: (files, used, of) => `${files}, ${used} von ${of}`,
    noCode: 'Diese Version enthält keinen Code.',
    assets: 'Assets',
    assetsPublic: 'Öffentlich: Diese Version liefert sie über Assets aus.',
    assetsPrivate: 'Mit dem Code gespeichert, von dieser Version jedoch nicht ausgeliefert.',
    noAssets: 'Keine in dieser Version.',
    data: 'Daten',
    dataPublic: 'Öffentlich: Diese Version liefert sie über Workspace aus.',
    dataPrivate: 'Nur für das Lambda zugänglich. Gehört zu keiner Version.',
    uploadFailed: (path) => `${path} konnte nicht hochgeladen werden.`,
    deleteFolder: (path, held) =>
      held > 0
        ? `${path} und ${held === 1 ? 'die enthaltene Datei' : `die ${held} enthaltenen Dateien`} löschen?`
        : `Den Ordner ${path} löschen?`,
    deleteFile: (path) => `${path} löschen? Das Lambda kann dann nicht mehr darauf zugreifen.`,
    deleteFailed: 'Der Eintrag konnte nicht gelöscht werden.',
    full: 'Der Datenspeicher ist voll',
    uploadInto: (folder) => `In ${folder} hochladen`,
    upload: 'Hochladen',
    reading: 'Wird gelesen …',
    noData: 'Noch keine Daten. Was das Lambda zur Laufzeit speichert, erscheint hier.',
    delete: (path) => `${path} löschen`,
    deleteShort: 'Löschen',
    fileFailed: 'Die Datei konnte nicht gelesen werden.',
    pick: 'Wählen Sie eine Datei aus, um ihren Inhalt anzuzeigen.',
    tooLarge: (name, size) => (
      <>
        {name} ist {size} groß und kann hier nicht angezeigt werden.
      </>
    ),
    download: 'Herunterladen',
    readingFile: (name) => `${name} wird gelesen …`,
    missing: (name) => `Diese Version enthält keine Datei namens ${name}.`,
    saved: 'gespeichert',
    notText: 'Keine Textdatei. Laden Sie sie herunter, um den Inhalt anzuzeigen.',
  },

  versions: {
    hint: (limit) =>
      `Jede Version enthält die Anforderung und die Änderung, sofern sie angegeben wurden. Bei mehr als ${limit} Versionen werden die ältesten entfernt; die bereitgestellte Version bleibt stets erhalten.`,
    none: 'Noch keine Versionen.',
    noDescription: 'Keine Beschreibung',
    online: 'online',
    putOnline: 'Diese Version bereitstellen',
    rollBackTitle: 'Diese ältere Version wieder bereitstellen',
    deploy: 'Bereitstellen',
    rollBack: 'Wiederherstellen',
    readFailed: 'Diese Version konnte nicht gelesen werden.',
    comparing: 'Wird verglichen …',
    unchanged: 'Keine Änderungen gegenüber der vorherigen Version.',
    first: 'Die erste Version.',
    status: { added: 'hinzugefügt', removed: 'entfernt', changed: 'geändert', same: 'unverändert' },
    browse: 'Dateien anzeigen',
    edit: 'Ab hier bearbeiten',
    binary: 'Keine Textdatei, daher kein zeilenweiser Vergleich möglich.',
    tooLarge: 'Zu groß für einen zeilenweisen Vergleich.',
  },

  deployments: {
    hint: (until) =>
      `Eine Bereitstellung bleibt online, solange sie genutzt wird${until ? ` – ohne Nutzung bis ${until}` : ''}. Jede erneute Bereitstellung und jeder Aufruf setzt diese Frist zurück.`,
    takeOffline: 'Offline nehmen',
    readFailed: 'Der Verlauf konnte nicht gelesen werden.',
    reading: 'Verlauf wird gelesen …',
    none: 'Es wurde noch nichts bereitgestellt.',
    noDescription: 'Keine Beschreibung',
    deployed: (when, by) => `Bereitgestellt am ${when} durch ${by}`,
    duration: 'Dauer der Bereitstellung',
    online: 'online',
    short: {
      replaced: 'ersetzt',
      stopped: 'offline genommen',
      expired: 'abgelaufen',
      admin: 'durch Betreiber',
      ended: 'beendet',
    },
    putBack: (version) => `Version ${version} wieder bereitstellen`,
    timeline: 'Bereitstellungen der letzten sieben Tage',
    block: (version, from, to) => `Version ${version}, ${from} bis ${to ?? 'jetzt'}`,
    weekAgo: 'vor einer Woche',
    now: 'jetzt',
  },

  stats: {
    readFailed: 'Die Kennzahlen konnten nicht gelesen werden.',
    range: 'Zeitraum',
    lastHour: 'Letzte Stunde',
    lastDay: 'Letzte 24 Stunden',
    hint: (since) =>
      `Seit dem letzten Serverstart (${since}) im Arbeitsspeicher gezählt. Nach einem Neustart beginnen die Kennzahlen von vorn.`,
    reading: 'Kennzahlen werden gelesen …',
    requests: 'Anfragen',
    websockets: (count) => `sowie ${count} Websocket-Verbindungen`,
    failed: 'fehlgeschlagen',
    serverErrors: (count) => `${count} Serverfehler`,
    rejected: 'nicht gefunden oder abgelehnt',
    average: 'durchschnittliche Antwortzeit',
    sent: (amount) => `${amount} gesendet`,
    nobody: (hour) => (hour ? 'In der letzten Stunde gab es keine Aufrufe.' : 'In den letzten 24 Stunden gab es keine Aufrufe.'),
    requestsTitle: 'Anfragen',
    per: (hour) => (hour ? 'Pro Minute.' : 'Pro 15 Minuten.'),
    answered: 'Beantwortet',
    rejectedSeries: 'Nicht gefunden oder abgelehnt',
    failedSeries: 'Fehlgeschlagen',
    timeTitle: 'Antwortzeit',
    averagePer: (hour) => (hour ? 'Durchschnitt pro Minute.' : 'Durchschnitt pro 15 Minuten.'),
    averageSeries: 'Durchschnitt',
    mostAsked: 'Häufigste Aufrufe',
    path: 'Pfad',
    requestsColumn: 'Anfragen',
    failedColumn: 'Fehler',
    averageColumn: 'Durchschnitt',
    since: 'Seit dem Serverstart.',
  },

  logs: {
    readFailed: 'Das Log konnte nicht gelesen werden.',
    hint: (capturing) =>
      'Anfragen, Ausgaben des Lambdas und aufgetretene Fehler in Echtzeit.' +
      (capturing ? '' : ' Diese Installation speichert keine Ausgaben von Lambdas; es erscheinen daher nur Anfragen und Fehler.') +
      ' Das Log wird im Arbeitsspeicher gehalten und mit allen Lambdas dieser Installation geteilt. Es reicht daher Minuten bis Stunden zurück und ist nach einem Neustart leer. Adressen von Besuchern werden nicht angezeigt.',
    search: 'Suchen',
    searchLabel: 'Log durchsuchen',
    resume: 'Neue Einträge fortlaufend anzeigen',
    pause: 'Neue Einträge anhalten',
    paused: 'Angehalten',
    live: 'Live',
    show: 'Anzeigen',
    all: 'Alle',
    requests: 'Anfragen',
    output: 'Ausgaben',
    problems: 'Fehler',
    reading: 'Log wird gelesen …',
    noProblems: 'Im aktuellen Log sind keine Fehler verzeichnet.',
    nothing: 'Noch keine Einträge. Rufen Sie die Adresse des Lambdas auf, damit Anfragen hier erscheinen.',
    noMatch: 'Keine Treffer.',
    identical: (count) => `${count} identische Einträge`,
    at: (domain) => `, über ${domain}`,
    from: (country) => `, aus ${country}`,
  },

  showcase: {
    loadFailed: 'Der Showcase-Eintrag konnte nicht geladen werden.',
    loading: 'Wird geladen …',
    title: 'ein Titel',
    description: 'eine Beschreibung',
    picture: 'ein Bild',
    updated: 'Der Showcase-Eintrag wurde aktualisiert.',
    listed: 'Die Anwendung erscheint jetzt im Showcase.',
    waiting: 'Gespeichert. Der Eintrag erscheint im Showcase, sobald das Lambda online ist.',
    saveFailed: 'Der Showcase-Eintrag konnte nicht gespeichert werden.',
    removed: 'Aus dem Showcase entfernt.',
    removeFailed: 'Der Showcase-Eintrag konnte nicht entfernt werden.',
    wrongType: 'Dies ist kein Bild im Format PNG, JPEG, GIF oder WebP.',
    tooLarge: (size, limit) => `Die Datei ist ${size} groß; zulässig sind höchstens ${limit}.`,
    unreadable: 'Die Datei konnte nicht gelesen werden.',
    hint: (tool) => (
      <>
        Der Showcase listet Lambdas, die ihre Eigentümer zur Ansicht freigegeben haben – zuletzt häufig genutzte zuerst.
        Nur mit dem Editor-Schlüssel lässt sich ein Lambda aufnehmen oder entfernen, und es wird nur angezeigt, solange es
        online ist. Agenten nutzen dafür das Werkzeug {tool}.
      </>
    ),
    open: 'Showcase öffnen',
    switch: 'Dieses Lambda im Showcase anzeigen',
    listedNow: 'Aktuell aufgeführt. Besucher des Showcase können es öffnen.',
    notListed: 'Gespeichert, aber nicht aufgeführt: Das Lambda ist offline. Nach der nächsten Bereitstellung erscheint es wieder.',
    off: 'Deaktiviert. Dieses Lambda wird nirgends aufgeführt, bis Sie diese Option aktivieren und speichern.',
    offline: 'Das Lambda ist offline; der Eintrag erscheint erst nach der Bereitstellung. Aufgeführt werden nur erreichbare Lambdas.',
    titleLabel: 'Titel',
    titlePlaceholder: 'Punktetafel für den Kneipenquiz-Abend',
    descriptionLabel: 'Beschreibung',
    descriptionPlaceholder:
      'Teams geben ihre Antworten auf dem Smartphone ein, die Spielleitung bewertet sie, und die Punktetafel aktualisiert sich für alle im Raum.',
    save: 'Änderungen speichern',
    add: 'Zum Showcase hinzufügen',
    takeOff: 'Entfernen',
    needs: (missing) =>
      `Es fehlt noch ${missing.length > 1 ? `${missing.slice(0, -1).join(', ')} und ${missing[missing.length - 1]}` : missing[0]}.`,
    tooLong: 'Einige Angaben sind zu lang.',
    allSaved: 'Alle Änderungen sind gespeichert.',
    preview: 'Vorschau',
    card: (address) => <>So sehen Besucher die Karte. Sie öffnet {address}.</>,
    confirm: 'Aus dem Showcase entfernen?',
    keep: 'Beibehalten',
    confirmText: 'Titel, Beschreibung und Bild werden gelöscht. Das Lambda selbst bleibt unverändert.',
    pictureLabel: 'Bild',
    formats: (limit) => `PNG, JPEG, GIF oder WebP, höchstens ${limit}`,
    notSaved: 'noch nicht gespeichert',
    replace: 'Legen Sie ein neues Bild hier ab, um es zu ersetzen.',
    drop: 'Legen Sie ein Bild hier ab.',
    advice: 'Am besten eignet sich ein Screenshot oder ein kurzes GIF im Format 16:10.',
    another: 'Anderes Bild wählen',
    choose: 'Datei auswählen',
    keepSaved: 'Gespeichertes Bild beibehalten',
    clear: 'Entfernen',
  },

  domain: {
    readFailed: 'Die Domain konnte nicht gelesen werden.',
    reaching: (domain) => `Anfragen an ${domain} erreichen jetzt dieses Lambda.`,
    saveFailed: 'Die Domain konnte nicht gespeichert werden.',
    removed: 'Die Domain wurde entfernt. Das Lambda bleibt unter seiner Adresse auf dieser Plattform erreichbar.',
    removeFailed: 'Die Domain konnte nicht entfernt werden.',
    hint:
      'Ein Premium-Lambda kann zusätzlich zu seiner Adresse auf dieser Plattform unter einer eigenen Domain erreichbar sein – vollständig, ab der Wurzel. Richten Sie die Domain auf diesen Server aus, tragen Sie sie hier ein, und Anfragen an sie erreichen das Lambda.',
    loading: 'Wird geladen …',
    example: 'ihre-domain.de',
    open: (domain) => `${domain} öffnen`,
    label: 'Domain des Lambdas',
    serving: (domain) => <>{domain} wird derzeit zusätzlich zur Adresse auf dieser Plattform ausgeliefert.</>,
    none: 'Noch keine. Möglich sind eine Subdomain wie shop.example.com oder eine vollständige Domain wie example.com.',
    change: 'Ändern',
    use: 'Domain verwenden',
    remove: 'Entfernen',
    confirm: 'Domain entfernen?',
    keep: 'Beibehalten',
    confirmText: (domain) => (
      <>
        Anfragen an {domain} erreichen dieses Lambda ab sofort nicht mehr. Die Adresse auf dieser Plattform bleibt ebenso
        unverändert wie die DNS-Einträge der Domain.
      </>
    ),
    point: 'Domain auf diesen Server ausrichten',
    check: 'Erneut prüfen',
    records:
      'Legen Sie beim DNS-Anbieter der Domain die folgenden beiden Einträge an. Den AAAA-Eintrag können Sie weglassen, wenn die Domain nicht über IPv6 erreichbar sein soll.',
    type: 'Typ',
    name: 'Name',
    value: 'Wert',
    pointsHere: (domain) => <>{domain} verweist auf diesen Server.</>,
    alsoElsewhere: (addresses) =>
      ` Sie wird zusätzlich zu ${addresses} aufgelöst, einer Adresse außerhalb dieses Servers – dorthin geleitete Besucher erreichen das Lambda nicht.`,
    elsewhere: (addresses) => `Die Domain wird zu ${addresses} aufgelöst und verweist noch nicht auf diesen Server.`,
    wait: 'Änderungen können einige Zeit benötigen, bis sie überall sichtbar sind – bis zur Gültigkeitsdauer (TTL) des alten Eintrags.',
    cname: 'Alternativ einen CNAME-Eintrag verwenden',
    cnameText: (target) => (
      <>
        Eine Subdomain kann stattdessen per CNAME-Eintrag auf {target} verweisen und folgt diesem Server dann auch bei
        künftigen Adressänderungen. Dies hat jedoch Nachteile:
      </>
    ),
    cnameRoot: (example) => (
      <>
        Für eine vollständige Domain ({example} selbst) ist dies nicht möglich: Der Standard erlaubt keinen CNAME neben den
        Einträgen, die jede Domain an ihrer Wurzel besitzt. Manche Anbieter bieten dafür einen ALIAS-, ANAME- oder
        „Flattening“-Eintrag an.
      </>
    ),
    cnameAlone: 'Unter demselben Namen können keine weiteren Einträge bestehen – weder MX-Einträge für E-Mail noch TXT-Einträge zur Verifizierung.',
    cnameLookup: 'Die Resolver der Besucher benötigen eine zusätzliche Abfrage.',
    copy: 'Kopieren',
    copyValue: (value) => `${value} kopieren`,
  },

  code: {
    title: 'Code',
    version: (version) => `Version ${version}`,
    edited: ', bearbeitet',
    online: ', online',
    loadFailed: 'Diese Version konnte nicht geladen werden.',
    compiles: 'Der Code lässt sich kompilieren.',
    notYet: 'Der Code lässt sich noch nicht kompilieren.',
    checkFailed: 'Der Code konnte nicht geprüft werden.',
    saved: (version) => `Als Version ${version} gespeichert.`,
    isOnline: (version) => `Version ${version} ist online.`,
    notOnline: 'Die Bereitstellung ist fehlgeschlagen. Die Meldungen des Compilers finden Sie unten.',
    failed: 'Der Vorgang ist fehlgeschlagen.',
    unchanged: 'Seit dem letzten Speichern gibt es keine Änderungen.',
    demo: 'Eine Demo, daher schreibgeschützt. Erstellen Sie auf dieser Grundlage ein eigenes Lambda, um Änderungen vorzunehmen. ',
    edit: 'Bearbeiten Sie den Code manuell. Speichern legt eine neue Version an, ohne die bereitgestellte zu verändern; Bereitstellen stellt sie online. ',
    files: (entry, cs) => (
      <>
        {entry} gibt zurück, was ausgeliefert wird, weitere {cs}-Dateien enthalten Typen, und alle übrigen Dateien werden
        unverändert ausgeliefert. Strg+S speichert, F12 springt zur Deklaration.
      </>
    ),
    newer: (version) => ` Version ${version} ist neuer als die hier geöffnete.`,
    check: 'Prüfen',
    save: 'Speichern',
    deploy: 'Bereitstellen',
    binary: (size) => `Keine Textdatei und daher nicht bearbeitbar. Sie wird unverändert ausgeliefert und ist ${size} kB groß.`,
    saveAndDeploy: 'Speichern und bereitstellen',
    saveVersion: 'Neue Version speichern',
    cancel: 'Abbrechen',
    what: 'Was ändert diese Version? Optional – die Angabe erscheint im Verlauf.',
    placeholder: 'Ergänzt ein Kontaktformular',
    goToDefinition: 'Gehe zu Definition',
  },

  tabs: {
    codeName: 'Buchstaben, Ziffern, Binde- und Unterstriche, Endung .cs',
    slashes: 'Kein Schrägstrich am Anfang oder Ende, höchstens 120 Zeichen.',
    deep: 'Höchstens sechs Ordnerebenen.',
    characters: 'Buchstaben, Ziffern, Binde- und Unterstriche sowie Punkte, getrennt durch Schrägstriche.',
    extension: 'Eine Dateiendung ist erforderlich, damit die Datei korrekt ausgeliefert wird.',
    exists: 'Eine Datei mit diesem Namen existiert bereits.',
    remove: (name) => `${name} entfernen? Der Inhalt wird ebenfalls gelöscht.`,
    there: (name) => `${name} ist bereits vorhanden.`,
    entry: 'Das Snippet: Sein Rückgabewert wird ausgeliefert',
    errors: 'enthält Fehler',
    removeFile: (name) => `${name} entfernen`,
    removeTitle: 'Datei entfernen',
    placeholder: 'Types.cs oder site/index.html',
    newFile: 'Neue Datei',
    uploadTitle: 'Datei hochladen – etwa ein Bild, eine Schriftart oder eine Seite',
    upload: 'Datei hochladen',
  },
};
