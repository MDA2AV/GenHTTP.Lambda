import type { EditorMessages } from '../en/editor';

/** I testi dell’editor in italiano. */
export const editor: EditorMessages = {
  shared: {
    units: { s: 's', min: 'min', h: 'h', d: 'g' },
    never: 'mai',
    justNow: 'proprio ora',
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
      replaced: 'sostituito da una distribuzione più recente',
      stopped: 'disattivato',
      expired: 'scaduto per inutilizzo',
      admin: 'disattivato dall’operatore',
      ended: 'terminato',
    },
    whatThisIs: 'Spiegazione',
    byAgent: 'da un agente',
    writtenByAgent: 'Scritto da un agente',
    more: 'Altro',
    of: (used, total) => `${used} di ${total}`,
    online: (version) => `Online · v${version}`,
    onlineTitle: (version) => `Online, versione ${version} in uso`,
    offline: 'Offline',
    offlineTitle: 'Offline: nulla viene servito',
    premium:
      'Premium: può rispondere a un dominio dedicato, dispone di più spazio per codice, risorse e dati e resta online a prescindere dall’utilizzo',
    demo: 'Demo: mantenuta online da questa installazione, in sola lettura',
    tier: (tier) => `Piano ${tier}`,
    entrances: {
      title: 'Raggiunto tramite',
      note: 'Dall’avvio del server, connessioni WebSocket incluse.',
    },
    chart: {
      showChart: 'Mostra grafico',
      showValues: 'Mostra valori',
      none: 'Ancora nessuna misurazione.',
      time: 'Ora',
    },
    diagnostics: {
      compiles: 'Il codice viene compilato correttamente.',
      none: 'Ancora nessun messaggio. Verifichi o distribuisca il codice per compilarlo.',
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
      deployments: 'Distribuzioni',
      stats: 'Statistiche',
      logs: 'Log',
      code: 'Codice',
    },
    sectionsLabel: 'Sezioni',
    loadFailed: 'Impossibile caricare questo lambda.',
    online: (version) => `La versione ${version} è online.`,
    deployFailed: 'Impossibile distribuire il lambda.',
    offline: 'Disattivato. Il codice è stato conservato.',
    offlineFailed: 'Impossibile disattivare il lambda.',
    leave: 'Le modifiche non salvate al codice andranno perse. Uscire comunque?',
    nothingTitle: 'Questo link non apre alcun lambda',
    createNew: 'Crea un nuovo lambda',
    loading: 'Caricamento del lambda…',
    moreActions: 'Altre azioni',
    redeploy: (version) => `Ridistribuisci la versione ${version}`,
    takeOffline: 'Disattiva',
    copyLink: 'Copia il link',
    copyPrivate: 'Copia il link privato',
    privateLink: 'Chiunque disponga di questo link può modificare il lambda. Lo mantenga riservato.',
    rename: 'Modifica l’indirizzo',
    download: 'Scarica come progetto .NET',
    delete: 'Elimina questo lambda',
    deploy: (version) => `Distribuisci la versione ${version}`,
    problems: 'Si sono verificati errori di recente',
    demoTitle: 'Una demo, mantenuta online da questa installazione e in sola lettura.',
    demo: (start) => (
      <>
        Codice, cronologia, dati e log sono consultabili a titolo di esempio. Per modificarla,{' '}
        {start('crei un proprio lambda a partire da essa')}.
      </>
    ),
    keep: 'Conservi questo link con cura. È l’unico accesso a questo lambda.',
    gotIt: 'Ho capito',
    rejected: (version) => `La versione ${version} non è stata messa online`,
    refused: 'La distribuzione è stata rifiutata',
    openCode: 'Apri il codice',
    close: 'Chiudi',
    notCompiling: 'Il codice non viene compilato. La versione precedentemente online resta online.',
    moved: (path) => `Ora raggiungibile all’indirizzo ${path}.`,
    deleteTitle: 'Eliminare questo lambda?',
    cancel: 'Annulla',
    deleteForGood: 'Elimina definitivamente',
    deleteFailed: 'Impossibile eliminare il lambda.',
    deleteText: (key) => (
      <>Verranno eliminati tutte le versioni, i file, la cronologia e l’indirizzo {key}. L’operazione non è reversibile.</>
    ),
    openInTab: 'Apri in una nuova scheda',
    open: (address) => `Apri ${address} in una nuova scheda`,
    copyAddress: 'Copia l’indirizzo',
    renameFailed: 'Impossibile modificare l’indirizzo.',
    moveIt: 'Modifica l’indirizzo',
    renameText: 'Il vecchio indirizzo smette di funzionare immediatamente; aggiorni tutti i link che vi rimandano.',
  },

  summary: {
    reading: 'Lettura dello stato…',
    hint: (since, kept, retention, tier) =>
      `Il traffico viene conteggiato dall’ultimo avvio del server (${since}). ` +
      (kept
        ? `Un lambda resta online finché viene utilizzato e viene rimosso dopo ${retention} giorni senza visite né modifiche.`
        : `Questo lambda appartiene al piano ${tier}, che lo mantiene online e memorizzato a prescindere dall’utilizzo.`),
    onlineFor: (duration, version) => (
      <>
        Online da {duration('un po’ di tempo')}, versione {version} in uso.
      </>
    ),
    offline: 'Offline. Nulla viene servito finché non viene distribuita una versione.',
    nothing: 'Non è ancora stato scritto nulla.',
    requestsToday: 'richieste oggi',
    lastHour: (count) => `${count} nell’ultima ora`,
    hourly: 'Richieste all’ora nelle ultime 24 ore',
    failed: 'non riuscite',
    failedTitle: (failed, rejected) =>
      `${failed} errori del server, ${rejected} non trovate o rifiutate, nelle ultime 24 ore`,
    average: 'tempo medio di risposta',
    noneYet: 'ancora nessuna',
    lastVisit: 'ultima visita',
    problems: 'Errori recenti',
    openLog: 'Apri il log',
    latest: 'Ultima modifica',
    allVersions: 'Tutte le versioni',
    noDescription: 'Nessuna descrizione',
    version: (version) => `Versione ${version}`,
    notOnline: 'non ancora online',
    wanted: 'Richiesta originale',
    noVersions: 'Ancora nessuna versione.',
    storage: 'Spazio',
    browse: 'Sfoglia',
    code: 'Codice',
    codeWhy: 'Il C# viene compilato e mai servito.',
    characters: 'caratteri',
    assets: 'Risorse',
    assetsPublic: 'Pubbliche: il codice le serve.',
    assetsPrivate: 'Non servite dal codice.',
    data: 'Dati',
    dataPublic: 'Pubblici: il codice serve il workspace.',
    dataPrivate: 'Riservati al lambda.',
  },

  files: {
    hint: (b) => (
      <>
        Il {b('Codice')} viene compilato e mai servito. Le {b('Risorse')} – pagine, fogli di stile, immagini – vengono
        salvate con ogni versione e sono pubbliche se il codice le serve. I {b('Dati')} sono ciò che il lambda scrive durante
        l’esecuzione; non fanno parte di alcuna versione e sono pubblici solo se il codice li serve.
      </>
    ),
    edit: 'Modifica questa versione',
    version: 'Versione',
    shown: (version, online, newest) => `Versione ${version}${online ? ', online' : newest ? ', la più recente' : ''}`,
    optionOnline: ' (online)',
    readFailed: 'Impossibile leggere questa versione.',
    dataFailed: 'Impossibile leggere i dati.',
    noVersion: 'Non è ancora disponibile alcuna versione.',
    label: 'File',
    code: 'Codice',
    codeWhy: 'Compilato nel lambda, mai servito.',
    count: (files) => (files === 1 ? '1 file' : `${files} file`),
    codeUsage: (files, used, of) => `${files}, ${used} di ${of} caratteri`,
    usage: (files, used, of) => `${files}, ${used} di ${of}`,
    noCode: 'Questa versione non contiene codice.',
    assets: 'Risorse',
    assetsPublic: 'Pubbliche: questa versione le serve con Assets.',
    assetsPrivate: 'Salvate con il codice, ma non servite da questa versione.',
    noAssets: 'Nessuna in questa versione.',
    data: 'Dati',
    dataPublic: 'Pubblici: questa versione li serve con Workspace.',
    dataPrivate: 'Riservati al lambda. Non fanno parte di alcuna versione.',
    uploadFailed: (path) => `Impossibile caricare ${path}.`,
    deleteFolder: (path, held) =>
      held > 0
        ? `Eliminare ${path} e ${held === 1 ? 'il file che contiene' : `i ${held} file che contiene`}?`
        : `Eliminare la cartella ${path}?`,
    deleteFile: (path) => `Eliminare ${path}? Il lambda non potrà più accedervi.`,
    deleteFailed: 'Impossibile eliminare.',
    full: 'Lo spazio per i dati è esaurito',
    uploadInto: (folder) => `Carica in ${folder}`,
    upload: 'Carica',
    reading: 'Lettura…',
    noData: 'Ancora nessun dato. Ciò che il lambda salva durante l’esecuzione comparirà qui.',
    delete: (path) => `Elimina ${path}`,
    deleteShort: 'Elimina',
    fileFailed: 'Impossibile leggere il file.',
    pick: 'Selezioni un file per visualizzarne il contenuto.',
    tooLarge: (name, size) => (
      <>
        {name} occupa {size}, troppo per essere visualizzato qui.
      </>
    ),
    download: 'Scarica',
    readingFile: (name) => `Lettura di ${name}…`,
    missing: (name) => `Questa versione non contiene alcun file denominato ${name}.`,
    saved: 'salvato',
    notText: 'Non è un file di testo. Lo scarichi per consultarne il contenuto.',
  },

  versions: {
    hint: (limit) =>
      `Ogni versione conserva la richiesta e la modifica, se l’autore le ha indicate. Oltre ${limit} versioni, le più vecchie vengono rimosse; la versione online non viene mai rimossa.`,
    none: 'Ancora nessuna versione.',
    noDescription: 'Nessuna descrizione',
    online: 'online',
    putOnline: 'Metti online questa versione',
    rollBackTitle: 'Rimetti online questa versione precedente',
    deploy: 'Distribuisci',
    rollBack: 'Ripristina',
    readFailed: 'Impossibile leggere questa versione.',
    comparing: 'Confronto in corso…',
    unchanged: 'Nessuna modifica rispetto alla versione precedente.',
    first: 'La prima versione.',
    status: { added: 'aggiunto', removed: 'rimosso', changed: 'modificato', same: 'invariato' },
    browse: 'Sfoglia i file',
    edit: 'Modifica a partire da qui',
    binary: 'Non è un file di testo, quindi non è possibile un confronto riga per riga.',
    tooLarge: 'Troppo grande per un confronto riga per riga.',
  },

  deployments: {
    hint: (until) =>
      `Una distribuzione resta online finché viene utilizzata${until ? ` – in assenza di utilizzo, fino al ${until}` : ''}. Ogni nuova distribuzione o visita azzera questo termine.`,
    takeOffline: 'Disattiva',
    readFailed: 'Impossibile leggere la cronologia.',
    reading: 'Lettura della cronologia…',
    none: 'Non è ancora stato distribuito nulla.',
    noDescription: 'Nessuna descrizione',
    deployed: (when, by) => `Distribuito il ${when} da ${by}`,
    duration: 'Durata online',
    online: 'online',
    short: {
      replaced: 'sostituito',
      stopped: 'disattivato',
      expired: 'scaduto',
      admin: 'dall’operatore',
      ended: 'terminato',
    },
    putBack: (version) => `Rimetti online la versione ${version}`,
    timeline: 'Versioni online negli ultimi sette giorni',
    block: (version, from, to) => `Versione ${version}, dal ${from} ${to ? `al ${to}` : 'a oggi'}`,
    weekAgo: 'una settimana fa',
    now: 'ora',
  },

  stats: {
    readFailed: 'Impossibile leggere le statistiche.',
    range: 'Periodo',
    lastHour: 'Ultima ora',
    lastDay: 'Ultime 24 ore',
    hint: (since) =>
      `Conteggiato in memoria dall’ultimo avvio del server (${since}). Un riavvio azzera questi valori.`,
    reading: 'Lettura delle statistiche…',
    requests: 'richieste',
    websockets: (count) => `e ${count} connessioni WebSocket`,
    failed: 'non riuscite',
    serverErrors: (count) => `${count} errori del server`,
    rejected: 'non trovate o rifiutate',
    average: 'tempo medio di risposta',
    sent: (amount) => `${amount} inviati`,
    nobody: (hour) => (hour ? 'Nessuna richiesta nell’ultima ora.' : 'Nessuna richiesta nelle ultime 24 ore.'),
    requestsTitle: 'Richieste',
    per: (hour) => (hour ? 'Al minuto.' : 'Ogni 15 minuti.'),
    answered: 'Evase',
    rejectedSeries: 'Non trovate o rifiutate',
    failedSeries: 'Non riuscite',
    timeTitle: 'Tempo di risposta',
    averagePer: (hour) => (hour ? 'Media al minuto.' : 'Media ogni 15 minuti.'),
    averageSeries: 'Media',
    mostAsked: 'Percorsi più richiesti',
    path: 'Percorso',
    requestsColumn: 'Richieste',
    failedColumn: 'Errori',
    averageColumn: 'Media',
    since: 'Dall’avvio del server.',
  },

  logs: {
    readFailed: 'Impossibile leggere il log.',
    hint: (capturing) =>
      'Richieste, output del lambda ed errori, in tempo reale.' +
      (capturing ? '' : ' Questa installazione non conserva l’output dei lambda, quindi compaiono solo richieste ed errori.') +
      ' Il log è conservato in memoria e condiviso con tutti i lambda di questa installazione: copre da pochi minuti a qualche ora e si svuota dopo un riavvio. Gli indirizzi dei visitatori non vengono mostrati.',
    search: 'Cerca',
    searchLabel: 'Cerca nel log',
    resume: 'Mostra le nuove righe man mano che arrivano',
    pause: 'Sospendi l’aggiunta di nuove righe',
    paused: 'In pausa',
    live: 'In tempo reale',
    show: 'Mostra',
    all: 'Tutto',
    requests: 'Richieste',
    output: 'Output',
    problems: 'Errori',
    reading: 'Lettura del log…',
    noProblems: 'Il log attuale non contiene errori.',
    nothing: 'Ancora nessuna voce. Apra l’indirizzo del lambda e le richieste compariranno qui.',
    noMatch: 'Nessun risultato.',
    identical: (count) => `${count} righe identiche`,
    at: (domain) => `, tramite ${domain}`,
    from: (country) => `, da ${country}`,
  },

  showcase: {
    loadFailed: 'Impossibile caricare la scheda in vetrina.',
    loading: 'Caricamento…',
    title: 'un titolo',
    description: 'una descrizione',
    picture: 'un’immagine',
    updated: 'La scheda in vetrina è stata aggiornata.',
    listed: 'L’applicazione è ora in vetrina.',
    waiting: 'Salvato. La scheda comparirà in vetrina non appena il lambda sarà online.',
    saveFailed: 'Impossibile salvare la scheda in vetrina.',
    removed: 'Rimosso dalla vetrina.',
    removeFailed: 'Impossibile rimuovere la scheda dalla vetrina.',
    wrongType: 'Non è un’immagine PNG, JPEG, GIF o WebP.',
    tooLarge: (size, limit) => `Il file occupa ${size}; la dimensione massima consentita è ${limit}.`,
    unreadable: 'Impossibile leggere il file.',
    hint: (tool) => (
      <>
        La vetrina elenca i lambda che i proprietari hanno scelto di mostrare, a partire da quelli utilizzati più di
        recente. Solo chi possiede la chiave di modifica può inserire o rimuovere un lambda, che compare solo finché è
        online. Un agente può fare lo stesso con lo strumento {tool}.
      </>
    ),
    open: 'Apri la vetrina',
    switch: 'Mostra questo lambda in vetrina',
    listedNow: 'Attualmente in vetrina. Chi consulta la vetrina può aprirlo.',
    notListed: 'Salvato ma non visibile: il lambda è offline. Ricomparirà dopo la prossima distribuzione.',
    off: 'Disattivato. Questo lambda non viene mostrato da nessuna parte finché non attiva questa opzione e salva.',
    offline: 'Il lambda è offline; la scheda attenderà la distribuzione. Vengono mostrati solo i lambda raggiungibili.',
    titleLabel: 'Titolo',
    titlePlaceholder: 'Tabellone della serata quiz',
    descriptionLabel: 'Descrizione',
    descriptionPlaceholder:
      'Le squadre inseriscono le risposte dallo smartphone, il conduttore le valuta e il tabellone si aggiorna per tutta la sala.',
    save: 'Salva le modifiche',
    add: 'Aggiungi alla vetrina',
    takeOff: 'Rimuovi',
    needs: (missing) =>
      `Manca ancora ${missing.length > 1 ? `${missing.slice(0, -1).join(', ')} e ${missing[missing.length - 1]}` : missing[0]}.`,
    tooLong: 'Alcuni campi sono troppo lunghi.',
    allSaved: 'Tutte le modifiche sono salvate.',
    preview: 'Anteprima',
    card: (address) => <>Questa è la scheda che vedranno i visitatori. Apre {address}.</>,
    confirm: 'Rimuovere dalla vetrina?',
    keep: 'Mantieni',
    confirmText: 'Titolo, descrizione e immagine verranno eliminati. Il lambda resta invariato.',
    pictureLabel: 'Immagine',
    formats: (limit) => `PNG, JPEG, GIF o WebP, fino a ${limit}`,
    notSaved: 'non ancora salvata',
    replace: 'Trascini qui una nuova immagine per sostituirla.',
    drop: 'Trascini qui un’immagine.',
    advice: 'L’ideale è uno screenshot o una breve GIF dell’applicazione in uso, in formato 16:10.',
    another: 'Scegli un’altra immagine',
    choose: 'Scegli un file',
    keepSaved: 'Mantieni quella salvata',
    clear: 'Rimuovi',
  },

  domain: {
    readFailed: 'Impossibile leggere il dominio.',
    reaching: (domain) => `Le richieste a ${domain} raggiungono ora questo lambda.`,
    saveFailed: 'Impossibile salvare il dominio.',
    removed: 'Il dominio è stato rimosso. Il lambda resta raggiungibile al suo indirizzo su questa piattaforma.',
    removeFailed: 'Impossibile rimuovere il dominio.',
    hint:
      'Un lambda Premium può rispondere a un dominio dedicato – per intero, a partire dalla radice – oltre che al suo indirizzo su questa piattaforma. Punti il dominio su questo server, lo inserisca qui e le richieste indirizzate a esso raggiungeranno il lambda.',
    loading: 'Caricamento…',
    example: 'il-suo-dominio.it',
    open: (domain) => `Apri ${domain}`,
    label: 'Dominio a cui risponde',
    serving: (domain) => <>{domain} è attualmente servito, oltre all’indirizzo su questa piattaforma.</>,
    none: 'Ancora nessuno. Un sottodominio come shop.example.com o un dominio completo come example.com.',
    change: 'Modifica',
    use: 'Usa questo dominio',
    remove: 'Rimuovi',
    confirm: 'Rimuovere il dominio?',
    keep: 'Mantieni',
    confirmText: (domain) => (
      <>
        Le richieste a {domain} smetteranno immediatamente di raggiungere questo lambda. L’indirizzo su questa piattaforma
        resta invariato, così come la configurazione DNS del dominio.
      </>
    ),
    point: 'Puntare il dominio su questo server',
    check: 'Verifica di nuovo',
    records:
      'Presso il gestore DNS del dominio, aggiunga i due record seguenti. Il record AAAA può essere omesso se il dominio non deve essere raggiungibile via IPv6.',
    type: 'Tipo',
    name: 'Nome',
    value: 'Valore',
    pointsHere: (domain) => <>{domain} punta a questo server.</>,
    alsoElsewhere: (addresses) =>
      ` Viene risolto anche in ${addresses}, che non è questo server: i visitatori indirizzati lì non raggiungeranno il lambda.`,
    elsewhere: (addresses) => `Viene risolto in ${addresses}, che non è ancora questo server.`,
    wait: 'Una modifica può richiedere tempo per essere visibile ovunque, fino alla durata (TTL) del record precedente.',
    cname: 'Utilizzare invece un record CNAME',
    cnameText: (target) => (
      <>
        Un sottodominio può puntare a {target} tramite un record CNAME, seguendo così questo server anche in caso di
        cambiamento degli indirizzi. Questa soluzione presenta però alcuni svantaggi:
      </>
    ),
    cnameRoot: (example) => (
      <>
        Non può essere usata per un dominio completo ({example} stesso): lo standard non consente un CNAME accanto ai record
        che ogni dominio possiede alla radice. Alcuni provider offrono a questo scopo un record ALIAS, ANAME o «appiattito».
      </>
    ),
    cnameAlone: 'Nessun altro record può coesistere sullo stesso nome: né MX per la posta, né TXT per le verifiche.',
    cnameLookup: 'I resolver dei visitatori effettuano una query aggiuntiva.',
    copy: 'Copia',
    copyValue: (value) => `Copia ${value}`,
  },

  code: {
    title: 'Codice',
    version: (version) => `versione ${version}`,
    edited: ', modificata',
    online: ', online',
    loadFailed: 'Impossibile caricare questa versione.',
    compiles: 'Il codice viene compilato correttamente.',
    notYet: 'Il codice non viene ancora compilato.',
    checkFailed: 'Impossibile verificare il codice.',
    saved: (version) => `Salvato come versione ${version}.`,
    isOnline: (version) => `La versione ${version} è online.`,
    notOnline: 'La messa online non è riuscita. Consulti di seguito i messaggi del compilatore.',
    failed: 'L’operazione non è riuscita.',
    unchanged: 'Nessuna modifica dall’ultimo salvataggio.',
    demo: 'Si tratta di una demo, quindi in sola lettura. Crei un proprio lambda a partire da essa per modificarla. ',
    edit: 'Modifichi il codice manualmente. Il salvataggio crea una nuova versione senza toccare quella online; la distribuzione la mette online. ',
    files: (entry, cs) => (
      <>
        {entry} restituisce ciò che viene servito, gli altri file {cs} contengono tipi e ogni altro file viene servito così
        com’è. Ctrl+S salva, F12 passa a una dichiarazione.
      </>
    ),
    newer: (version) => ` La versione ${version} è più recente di quella aperta qui.`,
    check: 'Verifica',
    save: 'Salva',
    deploy: 'Distribuisci',
    binary: (size) => `Non è un file di testo, quindi non può essere modificato. Viene servito così com’è e occupa ${size} kB.`,
    saveAndDeploy: 'Salva e distribuisci',
    saveVersion: 'Salva una nuova versione',
    cancel: 'Annulla',
    what: 'Cosa cambia? Facoltativo: viene mostrato nella cronologia.',
    placeholder: 'Aggiunge un modulo di contatto',
    goToDefinition: 'Vai alla definizione',
  },

  tabs: {
    codeName: 'Lettere, cifre, trattini e trattini bassi, con estensione .cs',
    slashes: 'Nessuna barra all’inizio o alla fine, massimo 120 caratteri.',
    deep: 'Al massimo sei livelli di cartelle.',
    characters: 'Lettere, cifre, trattini, trattini bassi e punti, separati da barre.',
    extension: 'È necessaria un’estensione affinché il file venga servito correttamente.',
    exists: 'Esiste già un file con questo nome.',
    remove: (name) => `Rimuovere ${name}? Anche il contenuto verrà eliminato.`,
    there: (name) => `${name} esiste già.`,
    entry: 'Il frammento principale: ciò che restituisce viene servito',
    errors: 'contiene errori',
    removeFile: (name) => `Rimuovi ${name}`,
    removeTitle: 'Rimuovi questo file',
    placeholder: 'Types.cs o site/index.html',
    newFile: 'Nuovo file',
    uploadTitle: 'Carica un file: un’immagine, un font, una pagina',
    upload: 'Carica un file',
  },
};
