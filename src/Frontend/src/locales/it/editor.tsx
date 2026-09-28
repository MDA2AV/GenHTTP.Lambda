import type { EditorMessages } from '../en/editor';

/** I testi dell’editor in italiano. */
export const editor: EditorMessages = {
  shared: {
    units: { s: 's', min: 'min', h: 'h', d: 'g' },
    amount: (value, unit) => `${value} ${unit}`,
    pair: (larger, smaller) => `${larger} ${smaller}`,
    never: 'mai',
    justNow: 'adesso',
    ago: (span) => `${span} fa`,
    in: (span) => `tra ${span}`,
    origins: {
      agent: 'agente',
      template: 'modello',
      admin: 'operatore',
      system: 'piattaforma',
      api: 'API / editor',
      unknown: 'sconosciuto',
    },
    endings: {
      replaced: 'sostituito da un deployment più recente',
      stopped: 'messo offline',
      expired: 'scaduto perché inutilizzato',
      admin: 'messo offline dall’operatore',
      ended: 'terminato',
    },
    whatThisIs: 'Cos’è',
    byAgent: 'da un agente',
    writtenByAgent: 'Scritto da un agente',
    more: 'Altro',
    of: (used, total) => `${used} di ${total}`,
    online: (version) => `Online · v${version}`,
    onlineTitle: (version) => `Online, con la versione ${version}`,
    offline: 'Offline',
    offlineTitle: 'Offline: non viene servito niente',
    premium:
      'Premium: può rispondere su un dominio tutto suo, ha più spazio per codice, asset e dati, e resta online anche quando nessuno la usa',
    demo: 'Demo: tenuta online da questa installazione, in sola lettura',
    tier: (tier) => `Piano ${tier}`,
    entrances: {
      title: 'Raggiunta tramite',
      note: 'Dall’avvio del server, connessioni websocket comprese.',
    },
    chart: {
      showChart: 'Mostra grafico',
      showValues: 'Mostra valori',
      none: 'Ancora nessuna misurazione.',
      time: 'Ora',
    },
    diagnostics: {
      compiles: 'Il codice compila.',
      none: 'Ancora nessun messaggio. Premi Verifica o Deploy per compilare il codice.',
      line: (line) => `riga ${line}`,
    },
  },

  frame: {
    title: 'Editor',
    sections: {
      overview: 'Panoramica',
      showcase: 'Vetrina',
      domain: 'Dominio',
      files: 'File',
      versions: 'Versioni',
      deployments: 'Deployment',
      stats: 'Statistiche',
      logs: 'Log',
      code: 'Codice',
    },
    sectionsLabel: 'Sezioni',
    loadFailed: 'Impossibile caricare questa lambda.',
    online: (version) => `La versione ${version} è online.`,
    deployFailed: 'Il deploy della lambda non è riuscito.',
    offline: 'Messa offline. Il codice è ancora qui.',
    offlineFailed: 'Impossibile mettere offline la lambda.',
    leave: 'Le modifiche non salvate al codice andranno perse. Vuoi uscire lo stesso?',
    nothingTitle: 'Questo link non apre niente',
    createNew: 'Crea una nuova lambda',
    loading: 'Caricamento della lambda…',
    moreActions: 'Altre azioni',
    redeploy: (version) => `Rifai il deploy della versione ${version}`,
    takeOffline: 'Metti offline',
    copyLink: 'Copia il link',
    copyPrivate: 'Copia il link privato',
    privateLink: 'Chiunque abbia questo link può modificare la lambda. Tienilo per te.',
    rename: 'Cambia indirizzo',
    download: 'Scarica come progetto .NET',
    delete: 'Elimina questa lambda',
    deploy: (version) => `Deploy della versione ${version}`,
    problems: 'Qualcosa è andato storto di recente',
    demoTitle: 'Una demo, tenuta online da questa installazione e in sola lettura.',
    demo: (start) => (
      <>
        Leggi il codice, la cronologia, cosa salva e i log: è qui per questo. Per modificarla,{' '}
        {start('crea una tua lambda partendo da questa')}.
      </>
    ),
    keep: 'Conserva questo link. È l’unico modo per rientrare in questa lambda.',
    gotIt: 'Ho capito',
    rejected: (version) => `La versione ${version} non è andata online`,
    refused: 'Il deploy è stato rifiutato',
    openCode: 'Apri il codice',
    close: 'Chiudi',
    notCompiling: 'Non compila. Quello che era online prima è ancora online.',
    moved: (path) => `Ora si trova su ${path}.`,
    deleteTitle: 'Eliminare questa lambda?',
    cancel: 'Annulla',
    deleteForGood: 'Elimina per sempre',
    deleteFailed: 'Impossibile eliminare la lambda.',
    deleteText: (key) => (
      <>Spariscono anche tutte le versioni, i file, la cronologia e l’indirizzo {key}. Non si può annullare.</>
    ),
    openInTab: 'Apri in una nuova scheda',
    open: (address) => `Apri ${address} in una nuova scheda`,
    copyAddress: 'Copia l’indirizzo',
    renameFailed: 'Impossibile cambiare l’indirizzo.',
    moveIt: 'Sposta',
    renameText: 'Il vecchio indirizzo smette subito di funzionare: aggiorna tutto ciò che ci punta.',
  },

  summary: {
    reading: 'Lettura dello stato…',
    hint: (since, kept, retention, tier) =>
      `Il traffico è contato dall’ultimo avvio del server (${since}). ` +
      (kept
        ? `Una lambda resta online finché qualcuno la usa, e viene eliminata dopo ${retention} giorni senza visite né modifiche.`
        : `Questa lambda è nel piano ${tier}, che la tiene online e salvata anche quando nessuno la usa.`),
    onlineFor: (duration, version) => (
      <>
        Online da {duration('un po’')}, con la versione {version}.
      </>
    ),
    offline: 'Offline. Non viene servito niente finché non fai il deploy di una versione.',
    nothing: 'Non è ancora stato scritto niente.',
    requestsToday: 'richieste oggi',
    lastHour: (count) => `${count} nell’ultima ora`,
    hourly: 'Richieste all’ora nelle ultime 24 ore',
    failed: 'fallite',
    failedTitle: (failed, rejected) =>
      `${failed} errori del server, ${rejected} non trovate o rifiutate, nelle ultime 24 ore`,
    average: 'tempo medio di risposta',
    noneYet: 'ancora nessuna',
    lastVisit: 'ultima visita',
    problems: 'Qualcosa è andato storto di recente',
    openLog: 'Apri il log',
    latest: 'Ultima modifica',
    allVersions: 'Tutte le versioni',
    noDescription: 'Nessuna descrizione',
    version: (version) => `Versione ${version}`,
    notOnline: 'non ancora online',
    wanted: 'Cosa è stato chiesto',
    noVersions: 'Ancora nessuna versione.',
    storage: 'Spazio',
    browse: 'Sfoglia',
    code: 'Codice',
    codeWhy: 'Il C# viene compilato, mai servito.',
    characters: 'caratteri',
    assets: 'Asset',
    assetsPublic: 'Pubblici: il codice li serve.',
    assetsPrivate: 'Non serviti dal codice.',
    data: 'Dati',
    dataPublic: 'Pubblici: il codice serve il workspace.',
    dataPrivate: 'Privati, solo per la lambda.',
  },

  files: {
    hint: (b) => (
      <>
        Il {b('Codice')} viene compilato e mai servito. Gli {b('Asset')} (pagine, stili, immagini) vengono salvati con ogni
        versione e sono pubblici se il codice li serve. I {b('Dati')} sono ciò che la lambda scrive mentre gira: non fanno
        parte di nessuna versione e sono pubblici solo se il codice li serve.
      </>
    ),
    edit: 'Modifica questa versione',
    version: 'Versione',
    shown: (version, online, newest) => `Versione ${version}${online ? ', online' : newest ? ', la più recente' : ''}`,
    optionOnline: ' (online)',
    readFailed: 'Impossibile leggere quella versione.',
    dataFailed: 'Impossibile leggere i dati.',
    noVersion: 'Non c’è ancora nessuna versione da mostrare.',
    label: 'File',
    code: 'Codice',
    codeWhy: 'Compilato nella lambda, mai servito.',
    count: (files) => (files === 1 ? '1 file' : `${files} file`),
    codeUsage: (files, used, of) => `${files}, ${used} di ${of} caratteri`,
    usage: (files, used, of) => `${files}, ${used} di ${of}`,
    noCode: 'Nessun codice in questa versione.',
    assets: 'Asset',
    assetsPublic: 'Pubblici: questa versione li serve con Assets.',
    assetsPrivate: 'Salvati con il codice, ma questa versione non li serve.',
    noAssets: 'Nessuno in questa versione.',
    data: 'Dati',
    dataPublic: 'Pubblici: questa versione li serve con Workspace.',
    dataPrivate: 'Privati, solo per la lambda. Non fanno parte di nessuna versione.',
    uploadFailed: (path) => `Impossibile caricare ${path}.`,
    deleteFolder: (path, held) =>
      held > 0
        ? `Eliminare ${path} e ${held === 1 ? 'il file che contiene' : `i ${held} file che contiene`}?`
        : `Eliminare la cartella ${path}?`,
    deleteFile: (path) => `Eliminare ${path}? La lambda non lo troverà più.`,
    deleteFailed: 'Eliminazione non riuscita.',
    full: 'Lo spazio per i dati è pieno',
    uploadInto: (folder) => `Carica in ${folder}`,
    upload: 'Carica',
    reading: 'Lettura…',
    noData: 'Ancora niente. Qui compare quello che la lambda salva mentre gira.',
    delete: (path) => `Elimina ${path}`,
    deleteShort: 'Elimina',
    fileFailed: 'Impossibile leggere il file.',
    pick: 'Scegli un file per vedere cosa contiene.',
    tooLarge: (name, size) => (
      <>
        {name} pesa {size}, troppo per mostrarlo qui.
      </>
    ),
    download: 'Scarica',
    readingFile: (name) => `Lettura di ${name}…`,
    missing: (name) => `Questa versione non ha nessun file chiamato ${name}.`,
    saved: 'salvato',
    notText: 'Non è testo. Scaricalo per vedere cosa contiene.',
  },

  versions: {
    hint: (limit) =>
      `Ogni versione conserva cosa è stato chiesto e cosa ha cambiato, se chi l’ha scritta l’ha indicato. Oltre ${limit} versioni, le più vecchie vengono eliminate; quella online mai.`,
    none: 'Ancora nessuna versione.',
    noDescription: 'Nessuna descrizione',
    online: 'online',
    putOnline: 'Metti online questa versione',
    rollBackTitle: 'Rimetti online questa versione precedente',
    deploy: 'Deploy',
    rollBack: 'Ripristina',
    readFailed: 'Impossibile leggere questa versione.',
    comparing: 'Confronto…',
    unchanged: 'Nessuna modifica rispetto alla versione precedente.',
    first: 'La prima versione.',
    status: { added: 'aggiunto', removed: 'rimosso', changed: 'modificato', same: 'invariato' },
    browse: 'Sfoglia i file',
    edit: 'Modifica da qui',
    binary: 'Non è testo, quindi non ci sono righe da confrontare.',
    tooLarge: 'Troppo grande per un confronto riga per riga.',
  },

  deployments: {
    hint: (until) =>
      `Un deployment resta online finché qualcuno lo usa${until ? `. Scadenza se nessuno lo usa: ${until}` : ''}. Un nuovo deploy, o qualsiasi visita, fa ripartire il conto alla rovescia.`,
    takeOffline: 'Metti offline',
    readFailed: 'Impossibile leggere la cronologia.',
    reading: 'Lettura della cronologia…',
    none: 'Non è ancora stato fatto nessun deploy.',
    noDescription: 'Nessuna descrizione',
    deployed: (when, by) => `Deploy: ${when} (${by})`,
    duration: 'Per quanto è stato online',
    online: 'online',
    short: {
      replaced: 'sostituito',
      stopped: 'messo offline',
      expired: 'scaduto',
      admin: 'fermato dall’operatore',
      ended: 'terminato',
    },
    putBack: (version) => `Rimetti online la versione ${version}`,
    timeline: 'Cosa è stato online negli ultimi sette giorni',
    block: (version, from, to) => `Versione ${version}, ${from} – ${to ?? 'ora'}`,
    weekAgo: 'una settimana fa',
    now: 'ora',
  },

  stats: {
    readFailed: 'Impossibile leggere le statistiche.',
    range: 'Periodo',
    lastHour: 'Ultima ora',
    lastDay: 'Ultime 24 ore',
    hint: (since) =>
      `Contate in memoria dall’ultimo avvio del server (${since}). Un riavvio le azzera.`,
    reading: 'Lettura delle statistiche…',
    requests: 'richieste',
    websockets: (count) => `e ${count} connessioni websocket`,
    failed: 'fallite',
    serverErrors: (count) => `${count} errori del server`,
    rejected: 'non trovate o rifiutate',
    average: 'tempo medio di risposta',
    sent: (amount) => `${amount} inviati`,
    nobody: (hour) => (hour ? 'Nessuna richiesta nell’ultima ora.' : 'Nessuna richiesta nelle ultime 24 ore.'),
    requestsTitle: 'Richieste',
    per: (hour) => (hour ? 'Al minuto.' : 'Ogni 15 minuti.'),
    answered: 'Riuscite',
    rejectedSeries: 'Non trovate o rifiutate',
    failedSeries: 'Fallite',
    timeTitle: 'Tempo di risposta',
    averagePer: (hour) => (hour ? 'Media al minuto.' : 'Media ogni 15 minuti.'),
    averageSeries: 'Media',
    mostAsked: 'Percorsi più richiesti',
    path: 'Percorso',
    requestsColumn: 'Richieste',
    failedColumn: 'Fallite',
    averageColumn: 'Media',
    since: 'Dall’avvio del server.',
  },

  logs: {
    readFailed: 'Impossibile leggere il log.',
    hint: (capturing) =>
      'Richieste, cosa ha stampato la lambda e cosa è andato storto, in tempo reale.' +
      (capturing ? '' : ' Questa installazione non conserva quello che stampano le lambda, quindi compaiono solo richieste ed errori.') +
      ' Il log sta in memoria ed è condiviso con tutte le lambda di questo server: copre da qualche minuto a qualche ora e si svuota dopo un riavvio. Gli indirizzi dei visitatori non vengono mostrati.',
    search: 'Cerca',
    searchLabel: 'Cerca nel log',
    resume: 'Mostra le nuove righe man mano che arrivano',
    pause: 'Blocca le nuove righe mentre leggi',
    paused: 'In pausa',
    live: 'Live',
    show: 'Mostra',
    all: 'Tutto',
    requests: 'Richieste',
    output: 'Output',
    problems: 'Errori',
    reading: 'Lettura del log…',
    noProblems: 'Nessun errore, almeno tra quelli che il log ricorda ancora.',
    nothing: 'Ancora niente. Apri l’indirizzo della lambda e qui compariranno le sue richieste.',
    noMatch: 'Nessun risultato.',
    identical: (count) => `${count} righe identiche`,
    at: (domain) => `, su ${domain}`,
    from: (country) => `, paese: ${country}`,
  },

  showcase: {
    loadFailed: 'Impossibile caricare la vetrina.',
    loading: 'Caricamento…',
    title: 'un titolo',
    description: 'una descrizione',
    picture: 'un’immagine',
    updated: 'Scheda in vetrina aggiornata.',
    listed: 'Ora è in vetrina.',
    waiting: 'Salvato. Comparirà in vetrina appena la lambda sarà online.',
    saveFailed: 'Impossibile salvare la scheda in vetrina.',
    removed: 'Tolta dalla vetrina.',
    removeFailed: 'Impossibile togliere la scheda dalla vetrina.',
    wrongType: 'Non è un’immagine PNG, JPEG, GIF o WebP.',
    tooLarge: (size, limit) => `Pesa ${size}; un’immagine può pesare al massimo ${limit}.`,
    unreadable: 'Impossibile leggere il file.',
    hint: (tool) => (
      <>
        La vetrina mostra le lambda che i proprietari hanno deciso di far vedere, prima quelle usate di recente. Solo chi
        ha la chiave di modifica può metterci una lambda o toglierla, e la lambda compare solo finché è online. Un agente
        può fare lo stesso con lo strumento {tool}.
      </>
    ),
    open: 'Apri la vetrina',
    switch: 'Mostra questa lambda in vetrina',
    listedNow: 'In vetrina. Chiunque la sfogli può aprirla.',
    notListed: 'Salvata, ma non in vetrina: la lambda è offline. Ricomparirà dopo il prossimo deploy.',
    off: 'Disattivata. Questa lambda non compare da nessuna parte finché non attivi l’opzione e salvi.',
    offline: 'La lambda è offline, quindi la scheda aspetterà il prossimo deploy. In vetrina finiscono solo le lambda che rispondono.',
    titleLabel: 'Titolo',
    titlePlaceholder: 'Classifica del quiz al pub',
    descriptionLabel: 'Descrizione',
    descriptionPlaceholder:
      'Le squadre inseriscono le risposte dal telefono, il presentatore le corregge e la classifica si aggiorna per tutti in sala.',
    save: 'Salva le modifiche',
    add: 'Aggiungi alla vetrina',
    takeOff: 'Togli dalla vetrina',
    needs: (missing) =>
      `${missing.length > 1 ? 'Mancano' : 'Manca'} ancora ${missing.length > 1 ? `${missing.slice(0, -1).join(', ')} e ${missing[missing.length - 1]}` : missing[0]}.`,
    tooLong: 'Qualche campo è troppo lungo.',
    allSaved: 'Tutto salvato.',
    preview: 'Anteprima',
    card: (address) => <>Questa è la scheda che vedono i visitatori. Apre {address}.</>,
    confirm: 'Togliere dalla vetrina?',
    keep: 'Lasciala',
    confirmText: 'Titolo, descrizione e immagine vengono eliminati. La lambda resta esattamente com’è.',
    pictureLabel: 'Immagine',
    formats: (limit) => `PNG, JPEG, GIF o WebP, fino a ${limit}`,
    notSaved: 'non ancora salvata',
    replace: 'Trascina qui un’altra immagine per sostituirla.',
    drop: 'Trascina qui un’immagine.',
    advice: 'Funziona meglio uno screenshot, o una breve GIF dell’app in uso, in 16:10.',
    another: 'Scegline un’altra',
    choose: 'Scegli un file',
    keepSaved: 'Tieni quella salvata',
    clear: 'Rimuovi',
  },

  domain: {
    readFailed: 'Impossibile leggere il dominio.',
    reaching: (domain) => `Ora le richieste a ${domain} arrivano a questa lambda.`,
    saveFailed: 'Impossibile salvare il dominio.',
    removed: 'Dominio rimosso. La lambda risponde ancora al suo indirizzo qui.',
    removeFailed: 'Impossibile rimuovere il dominio.',
    hint:
      'Una lambda Premium può rispondere su un dominio tutto suo (per intero, dalla radice in giù) oltre che al suo indirizzo qui. Punta il dominio su questo server, inseriscilo qui e le richieste arriveranno alla lambda.',
    loading: 'Caricamento…',
    example: 'il-tuo-dominio.it',
    open: (domain) => `Apri ${domain}`,
    label: 'Il dominio su cui risponde',
    serving: (domain) => <>Ora serve {domain}, oltre al suo indirizzo qui.</>,
    none: 'Ancora nessuno. Un sottodominio come shop.example.com, o un dominio intero come example.com.',
    change: 'Cambia',
    use: 'Usa questo dominio',
    remove: 'Rimuovi',
    confirm: 'Rimuovere il dominio?',
    keep: 'Tienilo',
    confirmText: (domain) => (
      <>
        Le richieste a {domain} smettono subito di arrivare a questa lambda. Il suo indirizzo qui resta com’è, e così
        anche il DNS del dominio.
      </>
    ),
    point: 'Punta il dominio su questo server',
    check: 'Controlla di nuovo',
    records:
      'Aggiungi questi due record dove gestisci il DNS del dominio. Salta il record AAAA se preferisci non essere raggiungibile via IPv6.',
    type: 'Tipo',
    name: 'Nome',
    value: 'Valore',
    pointsHere: (domain) => <>{domain} punta qui.</>,
    alsoElsewhere: (addresses) =>
      ` Risolve anche su ${addresses}, che non è questo server: i visitatori mandati lì non raggiungeranno la lambda.`,
    elsewhere: (addresses) => `Risolve su ${addresses}, che non è ancora questo server.`,
    wait: 'Una modifica può metterci un po’ a propagarsi ovunque, fino al TTL del vecchio record.',
    cname: 'Usare un record CNAME',
    cnameText: (target) => (
      <>
        Un sottodominio può invece puntare a {target} con un record CNAME, e così segue questo server se un giorno i suoi
        indirizzi cambiano. Ha però degli svantaggi:
      </>
    ),
    cnameRoot: (example) => (
      <>
        Non si può usare per un dominio intero ({example} stesso): lo standard non ammette un CNAME accanto ai record che
        ogni dominio ha alla radice. Alcuni provider offrono un record ALIAS, ANAME o «flattened» che lì funziona.
      </>
    ),
    cnameAlone: 'Sullo stesso nome non può esserci nient’altro: niente record MX per la posta, niente record TXT per le verifiche.',
    cnameLookup: 'I resolver dei visitatori fanno una ricerca DNS in più prima di arrivare.',
    copy: 'Copia',
    copyValue: (value) => `Copia ${value}`,
  },

  code: {
    title: 'Codice',
    version: (version) => `versione ${version}`,
    edited: ', modificata',
    online: ', online',
    loadFailed: 'Impossibile caricare quella versione.',
    compiles: 'Compila.',
    notYet: 'Non compila ancora.',
    checkFailed: 'Impossibile verificare il codice.',
    saved: (version) => `Salvata come versione ${version}.`,
    isOnline: (version) => `La versione ${version} è online.`,
    notOnline: 'Non è andata online. Guarda qui sotto cosa dice il compilatore.',
    failed: 'Non ha funzionato.',
    unchanged: 'Nessuna modifica dall’ultimo salvataggio.',
    demo: 'È una demo, quindi qui è tutto in sola lettura. Per modificarla, crea una tua lambda partendo da questa. ',
    edit: 'Modifica il codice a mano. Salvando crei una nuova versione e quella online non cambia; con il deploy la metti online. ',
    files: (entry, cs) => (
      <>
        {entry} restituisce ciò che viene servito, gli altri file {cs} contengono i tipi e ogni altro file viene servito
        così com’è. Ctrl-S salva, F12 va alla dichiarazione.
      </>
    ),
    newer: (version) => ` La versione ${version} è più recente di quella aperta qui.`,
    check: 'Verifica',
    save: 'Salva',
    deploy: 'Deploy',
    binary: (size) => `Non è testo, quindi non c’è niente da modificare. Viene servito così com’è e pesa ${size} kB.`,
    saveAndDeploy: 'Salva e fai il deploy',
    saveVersion: 'Salva una nuova versione',
    cancel: 'Annulla',
    what: 'Cosa cambia? Facoltativo: compare nella cronologia.',
    placeholder: 'Aggiunge un modulo di contatto',
    goToDefinition: 'Vai alla definizione',
  },

  tabs: {
    codeName: 'Lettere, numeri, trattini e underscore, con estensione .cs',
    slashes: 'Senza barra all’inizio o alla fine, e meno di 120 caratteri.',
    deep: 'Al massimo sei livelli di cartelle.',
    characters: 'Lettere, numeri, trattini, underscore e punti, separati da barre.',
    extension: 'Serve un’estensione, così il file viene servito nel formato giusto.',
    exists: 'Esiste già un file con questo nome.',
    remove: (name) => `Rimuovere ${name}? Anche il suo contenuto andrà perso.`,
    there: (name) => `${name} esiste già.`,
    entry: 'Lo snippet: quello che restituisce è ciò che viene servito',
    errors: 'contiene errori',
    removeFile: (name) => `Rimuovi ${name}`,
    removeTitle: 'Rimuovi questo file',
    placeholder: 'Types.cs o site/index.html',
    newFile: 'Nuovo file',
    uploadTitle: 'Carica un file: un’immagine, un font, una pagina',
    upload: 'Carica un file',
  },
};
