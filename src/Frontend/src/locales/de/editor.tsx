import type { EditorMessages } from '../en/editor';

/** Die Texte des Editors auf Deutsch. */
export const editor: EditorMessages = {
  shared: {
    units: { s: 's', min: 'min', h: 'h', d: 'd' },
    amount: (value, unit) => `${value} ${unit}`,
    pair: (larger, smaller) => `${larger} ${smaller}`,
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
      replaced: 'durch ein neueres Deployment ersetzt',
      stopped: 'offline genommen',
      expired: 'abgelaufen, weil ungenutzt',
      admin: 'vom Betreiber offline genommen',
      ended: 'beendet',
    },
    whatThisIs: 'Was ist das?',
    byAgent: 'von einem Agenten',
    writtenByAgent: 'Von einem Agenten geschrieben',
    more: 'Mehr',
    of: (used, total) => `${used} von ${total}`,
    online: (version) => `Online · v${version}`,
    onlineTitle: (version) => `Online, liefert Version ${version} aus`,
    offline: 'Offline',
    offlineTitle: 'Offline: Es wird nichts ausgeliefert',
    premium:
      'Premium: kann unter einer eigenen Domain antworten, hat mehr Platz für Code, Assets und Daten und bleibt online, egal wie wenig los ist',
    demo: 'Demo: von dieser Installation online gehalten, schreibgeschützt',
    tier: (tier) => `Tarif ${tier}`,
    entrances: {
      title: 'Aufgerufen über',
      note: 'Seit dem Start des Servers, Websocket-Verbindungen eingeschlossen.',
    },
    chart: {
      showChart: 'Diagramm zeigen',
      showValues: 'Werte zeigen',
      none: 'Noch keine Messwerte.',
      time: 'Zeit',
    },
    diagnostics: {
      compiles: 'Der Code kompiliert.',
      none: 'Noch keine Meldungen. Prüfen oder deployen Sie, um Ihren Code zu kompilieren.',
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
      deployments: 'Deployments',
      stats: 'Statistik',
      logs: 'Logs',
      code: 'Code',
    },
    sectionsLabel: 'Bereiche',
    loadFailed: 'Dieses Lambda konnte nicht geladen werden.',
    online: (version) => `Version ${version} ist online.`,
    deployFailed: 'Das Lambda konnte nicht deployt werden.',
    offline: 'Offline genommen. Der Code ist noch da.',
    offlineFailed: 'Das Lambda konnte nicht offline genommen werden.',
    leave: 'Ihre ungespeicherten Änderungen am Code gehen verloren. Trotzdem verlassen?',
    nothingTitle: 'Dieser Link öffnet nichts',
    createNew: 'Neues Lambda erstellen',
    loading: 'Ihr Lambda lädt …',
    moreActions: 'Weitere Aktionen',
    redeploy: (version) => `Version ${version} neu deployen`,
    takeOffline: 'Offline nehmen',
    copyLink: 'Link kopieren',
    copyPrivate: 'Privaten Link kopieren',
    privateLink: 'Mit diesem Link kann jeder das Lambda ändern. Behalten Sie ihn für sich.',
    rename: 'Adresse ändern',
    download: 'Als .NET-Projekt herunterladen',
    delete: 'Lambda löschen',
    deploy: (version) => `Version ${version} deployen`,
    problems: 'Zuletzt ist etwas schiefgegangen',
    demoTitle: 'Eine Demo – von dieser Installation online gehalten und schreibgeschützt.',
    demo: (start) => (
      <>
        Lesen Sie Code, Verlauf, gespeicherte Daten und Logs – dafür ist sie da. Um etwas zu ändern,{' '}
        {start('starten Sie damit ein eigenes Lambda')}.
      </>
    ),
    keep: 'Heben Sie diesen Link auf. Er ist der einzige Weg zurück zu diesem Lambda.',
    gotIt: 'Verstanden',
    rejected: (version) => `Version ${version} ging nicht online`,
    refused: 'Das Deployment wurde abgelehnt',
    openCode: 'Code öffnen',
    close: 'Schließen',
    notCompiling: 'Der Code kompiliert nicht. Was vorher online war, bleibt online.',
    moved: (path) => `Jetzt unter ${path}.`,
    deleteTitle: 'Lambda löschen?',
    cancel: 'Abbrechen',
    deleteForGood: 'Endgültig löschen',
    deleteFailed: 'Das Lambda konnte nicht gelöscht werden.',
    deleteText: (key) => (
      <>Alle Versionen, Dateien, der Verlauf und die Adresse {key} werden gelöscht. Das lässt sich nicht rückgängig machen.</>
    ),
    openInTab: 'In neuem Tab öffnen',
    open: (address) => `${address} in neuem Tab öffnen`,
    copyAddress: 'Adresse kopieren',
    renameFailed: 'Die Adresse konnte nicht geändert werden.',
    moveIt: 'Umziehen',
    renameText: 'Die alte Adresse funktioniert sofort nicht mehr. Passen Sie alles an, was darauf verlinkt.',
  },

  summary: {
    reading: 'Status wird gelesen …',
    hint: (since, kept, retention, tier) =>
      `Der Traffic wird seit dem letzten Serverstart gezählt (${since}). ` +
      (kept
        ? `Ein Lambda bleibt online, solange es genutzt wird. Nach ${retention} Tagen ohne Besuche und ohne Änderungen wird es gelöscht.`
        : `Dieses Lambda ist im Tarif ${tier}. Es bleibt online und gespeichert, egal wie wenig los ist.`),
    onlineFor: (duration, version) => (
      <>
        Seit {duration('einer Weile')} online, liefert Version {version} aus.
      </>
    ),
    offline: 'Offline. Bis eine Version deployt wird, wird nichts ausgeliefert.',
    nothing: 'Es wurde noch nichts geschrieben.',
    requestsToday: 'Requests heute',
    lastHour: (count) => `${count} in der letzten Stunde`,
    hourly: 'Requests pro Stunde in den letzten 24 Stunden',
    failed: 'fehlgeschlagen',
    failedTitle: (failed, rejected) =>
      `${failed} Serverfehler, ${rejected} nicht gefunden oder abgelehnt, in den letzten 24 Stunden`,
    average: 'Antwortzeit im Schnitt',
    noneYet: 'noch keiner',
    lastVisit: 'letzter Besuch',
    problems: 'Zuletzt ist etwas schiefgegangen',
    openLog: 'Log öffnen',
    latest: 'Letzte Änderung',
    allVersions: 'Alle Versionen',
    noDescription: 'Keine Beschreibung',
    version: (version) => `Version ${version}`,
    notOnline: 'noch nicht online',
    wanted: 'Worum gebeten wurde',
    noVersions: 'Noch keine Versionen.',
    storage: 'Speicher',
    browse: 'Ansehen',
    code: 'Code',
    codeWhy: 'C# wird kompiliert, nie ausgeliefert.',
    characters: 'Zeichen',
    assets: 'Assets',
    assetsPublic: 'Öffentlich: Der Code liefert sie aus.',
    assetsPrivate: 'Vom Code nicht ausgeliefert.',
    data: 'Daten',
    dataPublic: 'Öffentlich: Der Code liefert den Workspace aus.',
    dataPrivate: 'Privat, nur für das Lambda.',
  },

  files: {
    hint: (b) => (
      <>
        {b('Code')} wird kompiliert und nie ausgeliefert. {b('Assets')} – Seiten, Styles, Bilder – werden mit jeder
        Version gespeichert und sind öffentlich, wenn der Code sie ausliefert. {b('Daten')} schreibt das Lambda zur
        Laufzeit. Sie gehören zu keiner Version und sind nur öffentlich, wenn der Code sie ausliefert.
      </>
    ),
    edit: 'Diese Version bearbeiten',
    version: 'Version',
    shown: (version, online, newest) => `Version ${version}${online ? ', online' : newest ? ', neueste' : ''}`,
    optionOnline: ' (online)',
    readFailed: 'Diese Version konnte nicht gelesen werden.',
    dataFailed: 'Die Daten konnten nicht gelesen werden.',
    noVersion: 'Es gibt noch keine Version.',
    label: 'Dateien',
    code: 'Code',
    codeWhy: 'Wird ins Lambda kompiliert, nie ausgeliefert.',
    count: (files) => (files === 1 ? '1 Datei' : `${files} Dateien`),
    codeUsage: (files, used, of) => `${files}, ${used} von ${of} Zeichen`,
    usage: (files, used, of) => `${files}, ${used} von ${of}`,
    noCode: 'Kein Code in dieser Version.',
    assets: 'Assets',
    assetsPublic: 'Öffentlich: Diese Version liefert sie mit Assets aus.',
    assetsPrivate: 'Mit dem Code gespeichert, aber diese Version liefert sie nicht aus.',
    noAssets: 'Keine in dieser Version.',
    data: 'Daten',
    dataPublic: 'Öffentlich: Diese Version liefert sie mit Workspace aus.',
    dataPrivate: 'Privat, nur für das Lambda. Gehört zu keiner Version.',
    uploadFailed: (path) => `${path} konnte nicht hochgeladen werden.`,
    deleteFolder: (path, held) =>
      held > 0
        ? `${path} und ${held === 1 ? 'die Datei darin' : `die ${held} Dateien darin`} löschen?`
        : `Den Ordner ${path} löschen?`,
    deleteFile: (path) => `${path} löschen? Das Lambda findet die Datei dann nicht mehr.`,
    deleteFailed: 'Das konnte nicht gelöscht werden.',
    full: 'Der Datenspeicher ist voll',
    uploadInto: (folder) => `In ${folder} hochladen`,
    upload: 'Hochladen',
    reading: 'Wird gelesen …',
    noData: 'Noch nichts. Was das Lambda zur Laufzeit speichert, erscheint hier.',
    delete: (path) => `${path} löschen`,
    deleteShort: 'Löschen',
    fileFailed: 'Die Datei konnte nicht gelesen werden.',
    pick: 'Wählen Sie eine Datei, um ihren Inhalt zu sehen.',
    tooLarge: (name, size) => (
      <>
        {name} ist {size} groß – zu groß für die Anzeige hier.
      </>
    ),
    download: 'Herunterladen',
    readingFile: (name) => `${name} wird gelesen …`,
    missing: (name) => `Diese Version hat keine Datei namens ${name}.`,
    saved: 'gespeichert',
    notText: 'Kein Text. Laden Sie die Datei herunter, um hineinzusehen.',
  },

  versions: {
    hint: (limit) =>
      `Jede Version hält fest, worum gebeten wurde und was sie geändert hat – sofern der Autor es angegeben hat. Bei mehr als ${limit} Versionen fallen die ältesten weg; die Version, die online ist, nie.`,
    none: 'Noch keine Versionen.',
    noDescription: 'Keine Beschreibung',
    online: 'online',
    putOnline: 'Diese Version online stellen',
    rollBackTitle: 'Diese ältere Version wieder online stellen',
    deploy: 'Deployen',
    rollBack: 'Zurückrollen',
    readFailed: 'Diese Version konnte nicht gelesen werden.',
    comparing: 'Wird verglichen …',
    unchanged: 'Keine Änderung zur vorherigen Version.',
    first: 'Die erste Version.',
    status: { added: 'neu', removed: 'entfernt', changed: 'geändert', same: 'gleich' },
    browse: 'Dateien ansehen',
    edit: 'Von hier aus bearbeiten',
    binary: 'Kein Text, also keine Zeilen zum Vergleichen.',
    tooLarge: 'Zu groß für einen zeilenweisen Vergleich.',
  },

  deployments: {
    hint: (until) =>
      `Ein Deployment bleibt online, solange es genutzt wird${until ? ` – ohne Besuche bis ${until}` : ''}. Jedes neue Deployment und jeder Besuch setzt diese Frist zurück.`,
    takeOffline: 'Offline nehmen',
    readFailed: 'Der Verlauf konnte nicht gelesen werden.',
    reading: 'Verlauf wird gelesen …',
    none: 'Noch nichts deployt.',
    noDescription: 'Keine Beschreibung',
    deployed: (when, by) => `Deployt am ${when} (${by})`,
    duration: 'Wie lange es online war',
    online: 'online',
    short: {
      replaced: 'ersetzt',
      stopped: 'offline genommen',
      expired: 'abgelaufen',
      admin: 'vom Betreiber',
      ended: 'beendet',
    },
    putBack: (version) => `Version ${version} wieder online stellen`,
    timeline: 'Was in den letzten sieben Tagen online war',
    block: (version, from, to) => `Version ${version}, ${from} bis ${to ?? 'jetzt'}`,
    weekAgo: 'vor einer Woche',
    now: 'jetzt',
  },

  stats: {
    readFailed: 'Die Zahlen konnten nicht gelesen werden.',
    range: 'Zeitraum',
    lastHour: 'Letzte Stunde',
    lastDay: 'Letzte 24 Stunden',
    hint: (since) =>
      `Seit dem letzten Serverstart (${since}) im Arbeitsspeicher gezählt. Nach einem Neustart beginnen die Zahlen von vorn.`,
    reading: 'Zahlen werden gelesen …',
    requests: 'Requests',
    websockets: (count) => `und ${count} Websocket-Verbindungen`,
    failed: 'fehlgeschlagen',
    serverErrors: (count) => `${count} Serverfehler`,
    rejected: 'nicht gefunden oder abgelehnt',
    average: 'Antwortzeit im Schnitt',
    sent: (amount) => `${amount} gesendet`,
    nobody: (hour) => (hour ? 'In der letzten Stunde hat es niemand aufgerufen.' : 'In den letzten 24 Stunden hat es niemand aufgerufen.'),
    requestsTitle: 'Requests',
    per: (hour) => (hour ? 'Pro Minute.' : 'Pro 15 Minuten.'),
    answered: 'Beantwortet',
    rejectedSeries: 'Nicht gefunden oder abgelehnt',
    failedSeries: 'Fehlgeschlagen',
    timeTitle: 'Antwortzeit',
    averagePer: (hour) => (hour ? 'Durchschnitt pro Minute.' : 'Durchschnitt pro 15 Minuten.'),
    averageSeries: 'Durchschnitt',
    mostAsked: 'Am häufigsten aufgerufen',
    path: 'Pfad',
    requestsColumn: 'Requests',
    failedColumn: 'Fehler',
    averageColumn: 'Schnitt',
    since: 'Seit dem Serverstart.',
  },

  logs: {
    readFailed: 'Das Log konnte nicht gelesen werden.',
    hint: (capturing) =>
      'Requests, Ausgaben des Lambdas und Fehler – live.' +
      (capturing ? '' : ' Diese Installation speichert keine Ausgaben von Lambdas, daher erscheinen nur Requests und Fehler.') +
      ' Das Log liegt im Arbeitsspeicher und wird mit allen Lambdas hier geteilt. Es reicht Minuten bis Stunden zurück und ist nach einem Neustart leer. Adressen von Besuchern werden nicht angezeigt.',
    search: 'Suchen',
    searchLabel: 'Log durchsuchen',
    resume: 'Neue Zeilen live anzeigen',
    pause: 'Keine neuen Zeilen, während Sie lesen',
    paused: 'Pausiert',
    live: 'Live',
    show: 'Zeigen',
    all: 'Alles',
    requests: 'Requests',
    output: 'Ausgaben',
    problems: 'Fehler',
    reading: 'Log wird gelesen …',
    noProblems: 'Nichts ist schiefgegangen, soweit das Log zurückreicht.',
    nothing: 'Noch nichts. Rufen Sie die Adresse des Lambdas auf, dann erscheinen hier seine Requests.',
    noMatch: 'Keine Treffer.',
    identical: (count) => `${count} identische Zeilen`,
    at: (domain) => `, über ${domain}`,
    from: (country) => `, aus ${country}`,
  },

  showcase: {
    loadFailed: 'Der Showcase konnte nicht geladen werden.',
    loading: 'Lädt …',
    title: 'Titel',
    description: 'Beschreibung',
    picture: 'Bild',
    updated: 'Der Showcase-Eintrag ist aktualisiert.',
    listed: 'Das Lambda ist jetzt im Showcase.',
    waiting: 'Gespeichert. Es erscheint im Showcase, sobald das Lambda online ist.',
    saveFailed: 'Der Showcase-Eintrag konnte nicht gespeichert werden.',
    removed: 'Aus dem Showcase genommen.',
    removeFailed: 'Der Showcase-Eintrag konnte nicht entfernt werden.',
    wrongType: 'Das ist kein PNG-, JPEG-, GIF- oder WebP-Bild.',
    tooLarge: (size, limit) => `Das sind ${size}; ein Bild darf höchstens ${limit} groß sein.`,
    unreadable: 'Die Datei konnte nicht gelesen werden.',
    hint: (tool) => (
      <>
        Der Showcase zeigt Lambdas, die ihre Besitzer zeigen möchten – zuletzt genutzte zuerst. Nur wer den
        Editor-Schlüssel hat, kann ein Lambda dort eintragen oder entfernen. Und es erscheint nur, solange es online ist.
        Ein Agent kann dasselbe mit seinem Tool {tool}.
      </>
    ),
    open: 'Showcase öffnen',
    switch: 'Dieses Lambda im Showcase zeigen',
    listedNow: 'Jetzt gelistet. Wer den Showcase durchstöbert, kann es öffnen.',
    notListed: 'Gespeichert, aber nicht gelistet: Das Lambda ist offline. Nach dem nächsten Deployment ist es wieder da.',
    off: 'Aus. Von diesem Lambda wird nirgends etwas gezeigt, bis Sie das einschalten und speichern.',
    offline: 'Das Lambda ist offline, der Eintrag wartet also auf das nächste Deployment. Gelistet werden nur Lambdas, die antworten.',
    titleLabel: 'Titel',
    titlePlaceholder: 'Punktestand fürs Kneipenquiz',
    descriptionLabel: 'Beschreibung',
    descriptionPlaceholder:
      'Teams tippen ihre Antworten aufs Handy, die Spielleitung wertet aus, und der Punktestand aktualisiert sich für alle im Raum.',
    save: 'Änderungen speichern',
    add: 'Zum Showcase hinzufügen',
    takeOff: 'Herausnehmen',
    needs: (missing) =>
      `Es fehlt noch: ${missing.length > 1 ? `${missing.slice(0, -1).join(', ')} und ${missing[missing.length - 1]}` : missing[0]}.`,
    tooLong: 'Etwas davon ist zu lang.',
    allSaved: 'Alles gespeichert.',
    preview: 'Vorschau',
    card: (address) => <>So sehen Besucher die Karte. Sie öffnet {address}.</>,
    confirm: 'Aus dem Showcase nehmen?',
    keep: 'Behalten',
    confirmText: 'Titel, Beschreibung und Bild werden gelöscht. Das Lambda selbst bleibt genau, wie es ist.',
    pictureLabel: 'Bild',
    formats: (limit) => `PNG, JPEG, GIF oder WebP, bis ${limit}`,
    notSaved: 'noch nicht gespeichert',
    replace: 'Ziehen Sie ein neues Bild hierher, um es zu ersetzen.',
    drop: 'Ziehen Sie ein Bild hierher.',
    advice: 'Am besten wirkt ein Screenshot oder ein kurzes GIF der App in Aktion, im Format 16:10.',
    another: 'Anderes Bild wählen',
    choose: 'Datei wählen',
    keepSaved: 'Gespeichertes behalten',
    clear: 'Leeren',
  },

  domain: {
    readFailed: 'Die Domain konnte nicht gelesen werden.',
    reaching: (domain) => `Requests an ${domain} erreichen jetzt dieses Lambda.`,
    saveFailed: 'Die Domain konnte nicht gespeichert werden.',
    removed: 'Die Domain ist entfernt. Das Lambda antwortet weiter unter seiner Adresse hier.',
    removeFailed: 'Die Domain konnte nicht entfernt werden.',
    hint:
      'Ein Premium-Lambda kann zusätzlich zu seiner Adresse hier unter einer eigenen Domain antworten – unter der ganzen Domain, ab der Wurzel. Lassen Sie die Domain auf diesen Server zeigen und tragen Sie sie hier ein. Dann erreichen Requests an die Domain das Lambda.',
    loading: 'Lädt …',
    example: 'ihre-domain.de',
    open: (domain) => `${domain} öffnen`,
    label: 'Domain, unter der es antwortet',
    serving: (domain) => <>Antwortet jetzt unter {domain}, zusätzlich zur Adresse hier.</>,
    none: 'Noch keine. Eine Subdomain wie shop.example.com oder eine ganze Domain wie example.com.',
    change: 'Ändern',
    use: 'Diese Domain verwenden',
    remove: 'Entfernen',
    confirm: 'Domain entfernen?',
    keep: 'Behalten',
    confirmText: (domain) => (
      <>
        Requests an {domain} erreichen dieses Lambda sofort nicht mehr. Seine Adresse hier bleibt, wie sie ist – ebenso
        die DNS-Einträge der Domain.
      </>
    ),
    point: 'Domain auf diesen Server zeigen lassen',
    check: 'Nochmal prüfen',
    records:
      'Legen Sie beim DNS-Anbieter der Domain diese zwei Einträge an. Den AAAA-Eintrag können Sie weglassen, wenn die Domain nicht über IPv6 erreichbar sein soll.',
    type: 'Typ',
    name: 'Name',
    value: 'Wert',
    pointsHere: (domain) => <>{domain} zeigt hierher.</>,
    alsoElsewhere: (addresses) =>
      ` Die Domain löst aber auch zu ${addresses} auf, und das ist nicht dieser Server – Besucher, die dort landen, erreichen das Lambda nicht.`,
    elsewhere: (addresses) => `Die Domain löst zu ${addresses} auf – das ist noch nicht dieser Server.`,
    wait: 'Bis eine Änderung überall ankommt, kann es dauern – höchstens so lange wie die TTL des alten Eintrags.',
    cname: 'Stattdessen einen CNAME-Eintrag verwenden',
    cnameText: (target) => (
      <>
        Eine Subdomain kann stattdessen per CNAME auf {target} zeigen. Dann folgt sie diesem Server, falls sich seine
        Adressen einmal ändern. Das hat Nachteile:
      </>
    ),
    cnameRoot: (example) => (
      <>
        Für eine ganze Domain ({example} selbst) geht das nicht: Der Standard erlaubt keinen CNAME neben den Einträgen, die
        jede Domain an ihrer Wurzel hat. Manche Anbieter haben dafür einen ALIAS-, ANAME- oder „Flattening“-Eintrag.
      </>
    ),
    cnameAlone: 'Unter demselben Namen darf nichts anderes stehen – kein MX-Eintrag für E-Mail, kein TXT-Eintrag für Verifizierungen.',
    cnameLookup: 'Die Resolver der Besucher brauchen eine Abfrage mehr, bis sie ankommen.',
    copy: 'Kopieren',
    copyValue: (value) => `${value} kopieren`,
  },

  code: {
    title: 'Code',
    version: (version) => `Version ${version}`,
    edited: ', bearbeitet',
    online: ', online',
    loadFailed: 'Diese Version konnte nicht geladen werden.',
    compiles: 'Kompiliert.',
    notYet: 'Kompiliert noch nicht.',
    checkFailed: 'Der Code konnte nicht geprüft werden.',
    saved: (version) => `Als Version ${version} gespeichert.`,
    isOnline: (version) => `Version ${version} ist online.`,
    notOnline: 'Ist nicht online gegangen. Was der Compiler sagt, steht unten.',
    failed: 'Das hat nicht geklappt.',
    unchanged: 'Seit dem letzten Speichern hat sich nichts geändert.',
    demo: 'Eine Demo, daher ist hier alles schreibgeschützt. Um etwas zu ändern, erstellen Sie damit ein eigenes Lambda. ',
    edit: 'Code von Hand bearbeiten. Speichern legt eine neue Version an und lässt, was online ist, unberührt. Deployen stellt sie online. ',
    files: (entry, cs) => (
      <>
        {entry} gibt zurück, was ausgeliefert wird. Weitere {cs}-Dateien enthalten Typen, alle anderen Dateien werden
        ausgeliefert, wie sie sind. Strg+S speichert, F12 springt zur Deklaration.
      </>
    ),
    newer: (version) => ` Version ${version} ist neuer als die hier geöffnete.`,
    check: 'Prüfen',
    save: 'Speichern',
    deploy: 'Deployen',
    binary: (size) => `Kein Text, also nichts zu bearbeiten. Die Datei wird ausgeliefert, wie sie ist, und ist ${size} kB groß.`,
    saveAndDeploy: 'Speichern und deployen',
    saveVersion: 'Neue Version speichern',
    cancel: 'Abbrechen',
    what: 'Was ändert sich? Optional – erscheint im Verlauf.',
    placeholder: 'Fügt ein Kontaktformular hinzu',
    goToDefinition: 'Zur Definition springen',
  },

  tabs: {
    codeName: 'Buchstaben, Ziffern, Binde- und Unterstriche, Endung .cs',
    slashes: 'Kein Schrägstrich am Anfang oder Ende, unter 120 Zeichen.',
    deep: 'Höchstens sechs Ordner tief.',
    characters: 'Buchstaben, Ziffern, Binde- und Unterstriche und Punkte, getrennt durch Schrägstriche.',
    extension: 'Die Datei braucht eine Endung, damit sie richtig ausgeliefert wird.',
    exists: 'Es gibt schon eine Datei mit diesem Namen.',
    remove: (name) => `${name} entfernen? Der Inhalt wird mitgelöscht.`,
    there: (name) => `${name} gibt es schon.`,
    entry: 'Das Snippet: Was es zurückgibt, wird ausgeliefert',
    errors: 'hat Fehler',
    removeFile: (name) => `${name} entfernen`,
    removeTitle: 'Diese Datei entfernen',
    placeholder: 'Types.cs oder site/index.html',
    newFile: 'Neue Datei',
    uploadTitle: 'Datei hochladen – ein Bild, eine Schrift, eine Seite',
    upload: 'Datei hochladen',
  },
};
