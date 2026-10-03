import type { Messages } from '../en';

export const guide: Messages['guide'] = {
  title: 'Come funziona',
  intro:
    'Scrivi uno snippet di C#. Quello che restituisce va online a un indirizzo pubblico, in HTTPS, in pochi secondi. Qui trovi tutto, nell’ordine in cui lo incontrerai.',
  contents: 'Indice',

  parts: {
    what: 'Cos’è una lambda',
    first: 'La tua prima lambda',
    editor: 'Il pannello di controllo',
    why: 'Spiegare il perché',
    written: 'Documentazione e test',
    features: 'Cambiarla senza rischi',
    files: 'Più di un file',
    page: 'Servire una pagina',
    spa: 'Un front-end, passo per passo',
    storage: 'I due posti dove stanno i file',
    database: 'Salvare le voci',
    keeping: 'Salvare i file',
    secrets: 'Chiavi e password',
    sockets: 'WebSocket',
    limits: 'Cosa non puoi fare',
    away: 'Portarla via',
    open: 'Pubblicare il codice',
    agents: 'Lasciar fare a un agente',
  },

  what: [
    (k) => (
      <>
        Una lambda è uno snippet che restituisce un handler GenHTTP. La piattaforma lo compila, lo carica e monta quello
        che ha restituito sotto il tuo indirizzo. Niente progetto, niente file di build, niente istruzioni{' '}
        {k.code('using')}: tutti i moduli GenHTTP sono già importati.
      </>
    ),
    (k) => (
      <>
        Questa è una lambda completa. Dopo il deploy su {k.code('/lambda/your-key/')}, risponde a ogni richiesta con la
        parola «hello».
      </>
    ),
  ],
  whatAside: (k) => (
    <>
      Lo snippet è fatto di {k.em('istruzioni')}, non di una classe. L’ultima cosa che fa è restituire qualcosa in grado
      di servire le richieste: un handler, o un builder che ne crea uno.
    </>
  ),

  first: [
    (k) => (
      <>
        Premi {k.b('Crea la mia lambda')}. Ricevi un indirizzo pubblico e una chiave di modifica. La chiave è l’unico
        modo per rientrare, quindi conservala. Nessuno può recuperarla per te.
      </>
    ),
    () => (
      <>
        Arrivi nel suo pannello di controllo, con un piccolo servizio REST già scritto come prima versione. È solo un
        punto di partenza.
      </>
    ),
    (k) => (
      <>
        Passa la chiave di modifica a un agente e digli cosa creare: scrive nuove versioni tramite{' '}
        {k.link('/#agents', 'MCP')}. Oppure apri {k.b('Codice')} e scrivi tu il codice: {k.b('Verifica')} compila
        senza salvare niente e ti mostra cosa dice il compilatore, con file e riga.
      </>
    ),
    (k) => (
      <>
        Premi {k.b('Deploy')}. Ora è online. Prima non è raggiungibile, e ogni nuovo deploy allunga il tempo in cui resta
        online.
      </>
    ),
  ],

  editor: (k) => (
    <>
      Il link di modifica apre un pannello di controllo, non un semplice editor di testo: qui gran parte del codice la
      scrivono gli agenti, quindi la prima cosa che vedi è come sta andando la tua lambda. La barra laterale mostra la
      lambda (se è online, il suo indirizzo e un pulsante quando una versione più recente aspetta di andare online) e le
      sue sezioni. Le azioni che servono di rado, come cambiare l’indirizzo o eliminarla, sono nel menu {k.b('⋯')}.
    </>
  ),
  bits: [
    ['Panoramica', () => <>Cos’è l’app, se è online, quante richieste ha ricevuto oggi e quante sono fallite, l’ultima modifica e quanto spazio resta.</>],
    ['Documentazione', () => <>Cos’è l’app, per chi è e perché, e perché è costruita così: la scrivono gli agenti e resta con ogni versione.</>],
    [
      'Modifica',
      (k) => (
        <>
          Scrivi cosa deve cambiare e l’agente di questo server lo fa sotto i tuoi occhi. Prova la modifica su una
          bozza, una copia con un indirizzo tutto suo, e la mette online quando funziona. Disattiva{' '}
          {k.b('Metti online a lavoro finito')} per provare prima tu la bozza.
          Lavora solo sulla tua app: una richiesta che non la riguarda, o che serve a fare danni, viene rifiutata, e ti dice perché.
        </>
      ),
    ],
    ['Bozze', () => <>Modifiche provate prima di andare online, ognuna a un indirizzo tutto suo e su dati di prova tutti suoi. Una volta aperta, una bozza ha il suo codice, i suoi dati di prova e i suoi log. La sezione compare appena c’è una bozza.</>],
    ['File', () => <>I file di una versione: il codice e gli asset, cioè il programma vero e proprio. Un lucchetto o un globo indica se sono pubblici.</>],
    ['Dati', () => <>Quello che la lambda conserva mentre gira, condiviso da tutte le versioni: il database, il workspace e le chiavi e password, ognuno con la sua scheda. Guarda le tabelle e i file, carica file, imposta chiavi e password o attiva e disattiva un tipo. La vista semplice lo mostra appena l’app conserva qualcosa.</>],
    ['Versioni', () => <>Cosa ha cambiato ogni versione, cosa era stato chiesto e le differenze rispetto alla precedente. Da qui fai il deploy o torni indietro, oppure avvii una bozza da una qualsiasi di esse.</>],
    ['Deployment', () => <>Cosa è stato online e quando, e cosa l’ha fermato.</>],
    ['Statistiche', () => <>Richieste, errori, tempi di risposta e i percorsi più richiesti, nell’ultima ora o nelle ultime 24 ore.</>],
    ['Log', () => <>Le richieste, cosa ha stampato e lo stack trace di ogni errore, in tempo reale.</>],
    [
      'Codice',
      (k) => (
        <>
          Per scriverlo a mano. {k.b('Verifica')} compila, {k.b('Salva')} crea una versione, {k.b('Deploy')} la mette
          online. In una bozza, {k.b('Salva')} lo tiene nella bozza e lo mostra all’indirizzo della bozza.{' '}
          {k.code('Ctrl-S')} salva; {k.code('F12')} va alla dichiarazione.
        </>
      ),
    ],
    ['Test', () => <>Come viene testata automaticamente l’app, con gli script e i dati di test che servono. Solo nella vista completa.</>],
  ],
  sections: (k) => (
    <>
      Ogni sezione funziona allo stesso modo: il titolo, il pulsante {k.b('ⓘ')} che la spiega, le azioni a destra e,
      dove ci sono più viste, una fila di schede sotto. Nel codice, le schede sono i file. La vista completa raccoglie
      le sezioni in gruppi: come la gente la trova, dove si fa una modifica, il programma e i suoi dati, e come gira.
    </>
  ),
  editorAside:
    'Traffico e log restano in memoria: servono per tenere d’occhio le cose, non per conservarle. Un riavvio del server li azzera. Le versioni e la cronologia dei deployment invece vengono salvate.',

  why: (k) => (
    <>
      Una versione è il codice più, se vuoi, due note: {k.b('la specifica')}, cioè cosa vuole l’utente e perché, se
      possibile con parole sue, e {k.b('la modifica')}, una riga su cosa fa la versione. Compaiono accanto al diff nella
      cronologia delle versioni, così il {k.em('perché')} resta vicino al {k.em('cosa')}: per te e per il prossimo agente
      che leggerà la cronologia prima di cambiare qualcosa.
    </>
  ),
  whySample: {
    specification: 'Un guestbook che la gente può firmare; le voci devono sopravvivere a un riavvio',
    change: 'Salva le voci nel database così sopravvivono a un riavvio',
  },
  why2: (k) => (
    <>
      Gli agenti passano gli stessi due campi a {k.code('write_code')}. In {k.b('Codice')}, quando salvi ti viene
      chiesta la modifica. Sono entrambi facoltativi: una specifica lunga viene tagliata a 4000 caratteri e una modifica
      a 500, invece di essere rifiutata. Una bozza ha le sue due note, e la versione in cui viene integrata le
      riprende.
    </>
  ),

  written: (k) => (
    <>
      Ogni versione conserva, accanto al suo programma, quello che è scritto su di lei: la sua {k.b('documentazione')}{' '}
      (cos’è l’app, per chi è e perché, e perché è costruita così) e i suoi {k.b('test')}: come verificare
      automaticamente che funziona, con gli script e i dati di test che servono. Gli agenti li scrivono insieme a una
      nuova lambda e li tengono aggiornati a ogni modifica. Il prossimo agente che modifica la lambda li legge per prima
      cosa, così sa a cosa serve l’app e cosa deve continuare a funzionare: cose che il codice da solo non dice.
    </>
  ),
  writtenFiles: [
    ['.lambda/docs/product.md', 'cos’è l’app, per chi è, cosa ci fa la gente e perché'],
    ['.lambda/docs/decisions.md', 'le decisioni tecniche, e perché sono state prese'],
    ['.lambda/tests/README.md', 'come viene testata automaticamente l’app, e come eseguire i test'],
    ['.lambda/tests/…', 'gli script e i dati di test usati dai test'],
  ],
  written2: (k) => (
    <>
      Sono file della versione come tutti gli altri, nella cartella {k.code('.lambda')}: la cronologia mostra cosa ci
      ha cambiato una versione, tornando indietro torna la documentazione che valeva per quella versione, e una bozza ne
      ha una copia sua che va online insieme a lei. Non vengono mai compilati né serviti, e contano nello spazio a
      disposizione per gli asset di una versione.
    </>
  ),
  written3: (k) => (
    <>
      Nel pannello di controllo, {k.b('Documentazione')} mostra le pagine da leggere, e {k.b('Test')} come viene
      testata l’app e i file che ci sono accanto; la versione si sceglie come per i suoi file. Lì si può anche
      modificare una pagina, e così si salva la versione successiva. La vista semplice chiama la documentazione{' '}
      {k.b('Informazioni')} e mostra solo a cosa serve l’app: per correggerla, dillo all’agente.
    </>
  ),
  writtenAside:
    'Sono scritti nella lingua che usi con l’agente, per chi modificherà l’app dopo, che sia una persona o un agente. Non sono una copia del codice: dicono a cosa serve l’app, e perché.',

  features: (k) => (
    <>
      Una versione, una volta salvata, non cambia più, ed è per questo che vale la pena conservarle tutte: ognuna si può
      confrontare e rimettere online esattamente com’era. Per cambiare una lambda che la gente usa, prova prima la
      modifica in una {k.b('bozza')}.
    </>
  ),
  featureSteps: [
    (k) => (
      <>
        Avviala da una versione qualsiasi in {k.b('Versioni')}, oppure lascia che lo faccia l’agente. È una copia del
        codice, degli asset, della documentazione e dei test di quella versione, e dei dati della lambda.
      </>
    ),
    (k) => (
      <>
        Modificala tutte le volte che serve, in {k.b('Codice')} o chiedendolo all’agente. La sua anteprima risponde a un
        indirizzo tutto suo, {k.code('/features/…/')}, con dati di prova tutti suoi. I visitatori della lambda non ne
        vedono niente, e niente di quello che scrive arriva ai dati della lambda.
      </>
    ),
    (k) => (
      <>
        Quando è a posto, premi {k.b('Metti online')}: diventa la prossima versione, con le sue note, e va online. La
        bozza sparisce insieme a lei, con la sua anteprima e i suoi dati di prova.
      </>
    ),
  ],
  featureSample: 'Classifica',
  featuresAside: () => (
    <>
      Si può lavorare a più bozze insieme. Solo una bozza aggiornata alla versione più recente può andare online, così
      non annulla mai una versione salvata dopo il suo avvio. Se prima ne è andata online un’altra, porta dentro le sue
      modifiche (o chiedilo all’agente) e segna la bozza come aggiornata. Niente va online da solo, ed è voluto. L’API
      chiama una bozza feature, e metterla online merge.
    </>
  ),

  files: (k) => (
    <>
      I tipi non devono per forza stare sotto il codice che li usa. In {k.b('Codice')}, premi {k.b('+')} accanto ai
      file: il nuovo file viene compilato insieme allo snippet, nello stesso namespace, quindi non devi importare niente
      per usarlo. Un nome senza estensione viene considerato C#.
    </>
  ),

  page: 'Ci sono due modi per servire una pagina, più uno per quello che la gente carica accanto.',
  inlineTitle: 'Una pagina, scritta nel codice',
  inline: 'Va bene per le cose piccole. La pagina fa parte dello snippet.',
  folderTitle: 'Una cartella di file veri',
  folder:
    'Quello che ti serve per qualsiasi cosa con un foglio di stile e uno script. I file si aggiungono come un file C# e vengono serviti esattamente come li hai scritti. Niente li compila.',
  workspaceTitle: 'File caricati, dai dati',
  workspace:
    'Per quello che la gente carica o che la lambda crea (foto, documenti), servito accanto all’app. Non per le pagine dell’app stessa: quelle vanno in una cartella di file, dove seguono le versioni insieme al codice che le usa.',

  spa: (k) => (
    <>
      Il secondo modo, per intero. Ogni demo serve la sua pagina così, da una cartella chiamata {k.code('web')}: apri{' '}
      {k.link('/editor/demo-crud', 'demo-crud')} per vederne una. Le demo sono in sola lettura, e la loro chiave di
      modifica è il loro nome.
    </>
  ),
  spaSteps: [
    (k) => (
      <>
        In {k.b('Codice')}, premi {k.b('+')} accanto ai file e scrivi {k.code('site/index.html')}. Un nome con una barra
        mette il file in una cartella; un nome con un’estensione viene trattato come il tipo di file che indica.
      </>
    ),
    (k) => (
      <>
        Aggiungi {k.code('site/app.css')} e {k.code('site/app.js')} allo stesso modo. La pagina li richiama per nome,
        come in {k.code('href="app.css"')}, perché la cartella è la radice di ciò che viene servito, non una parte
        dell’indirizzo.
      </>
    ),
    (k) => (
      <>
        Per tutto ciò che non è testo, come un’immagine o un font, apri un file in {k.code('site')} e premi il pulsante
        di caricamento accanto ai file: finisce nella stessa cartella. Un PNG non si può scrivere in un editor di testo,
        quindi si passa da lì.
      </>
    ),
    (k) => <>In {k.code('lambda.cs')}, servi la cartella:</>,
    (k) => (
      <>
        Premi {k.b('Deploy')}. {k.code('site/index.html')} risponde su {k.code('/')}, {k.code('site/app.css')} su{' '}
        {k.code('/app.css')}, e qualsiasi indirizzo che non corrisponde a un file riceve la pagina: così un front-end con
        un suo routing funziona anche quando qualcuno ricarica la pagina su un deep link.
      </>
    ),
    () => <>Aggiungi un’API accanto e la pagina avrà qualcosa con cui parlare:</>,
  ],

  storage: (k) => (
    <>
      Una lambda tiene i file in due posti, e l’editor li mostra separati: {k.b('File')} contiene i file di una versione
      (il programma) e {k.b('Dati')} contiene il workspace (quello che il programma conserva). La differenza è{' '}
      {k.em('di chi sono')}. I file di una versione appartengono a quella versione; i dati appartengono alla lambda, e
      tutte le versioni li condividono.
    </>
  ),
  savedWithCode: 'In una versione',
  workspaceColumn: 'Nei dati',
  table: [
    ['cosa contiene', 'il codice e gli asset: il programma, front-end compreso, con la sua documentazione e i suoi test', 'quello che scrive la lambda o che carica qualcuno'],
    ['quando cambia', 'mai: una modifica è una nuova versione', 'appena ci viene scritto qualcosa'],
    ['un deploy', 'mette online esattamente questi file', 'non li tocca mai'],
    ['tornare indietro', 'riporta i vecchi file', 'nessun effetto: tutte le versioni li condividono'],
    ['una bozza', 'parte da una copia di questi file', 'lavora su una copia dei dati'],
    ['quando sparisce', 'con le versioni vecchie, oltre il limite', 'con la lambda, o quando disattivi il workspace'],
  ],
  reachedAs: 'dal codice si raggiunge con',
  storageAside:
    'Non possono stare in un unico posto. Se ci stessero, un deploy cancellerebbe tutto ciò che la lambda ha scritto nel frattempo, oppure non si potrebbe mai togliere niente da ciò che pubblica. Un gioco con una classifica vuole la seconda cosa; la pagina che serve vuole la prima. Quindi la pagina va nella versione, e la classifica nei dati.',

  database: (k) => (
    <>
      Le voci (messaggi, account, ordini, voti) vanno nel {k.b('database')}: un database SQLite tutto della lambda, da
      attivare in {k.b('Dati')}. Il codice apre una connessione con {k.code('Database.GetConnection()')} e legge e
      scrive i dati tramite {k.link('https://learn.microsoft.com/ef/core/', 'Entity Framework Core')}, con un contesto
      tutto suo che mappa le tabelle:
    </>
  ),
  database2: (k) => (
    <>
      Le sue tabelle le creano le {k.b('migrazioni')}: file SQL che arrivano con la versione in {k.code('migrations/')},
      applicati in ordine da {k.link('https://evolve-db.netlify.app/', 'Evolve')} all’avvio della lambda, ognuno una
      volta sola, così una nuova versione esegue solo quello che è nuovo. Non modificare mai una migrazione già
      applicata: una modifica a una tabella è il file successivo.
    </>
  ),
  database3: (k) => (
    <>
      Come tutti i dati, il database è condiviso da tutte le versioni, deploy e ripristini non lo toccano, e una bozza
      lavora su una sua copia. In {k.b('Dati')} vedi le sue tabelle e le righe che contengono, che la vista semplice
      chiama voci. {k.b('Scarica come progetto .NET')} lo porta con sé come semplice file SQLite.
    </>
  ),
  databaseAside: (k) => (
    <>
      Crea un contesto dove ti serve e poi rilascialo, e usalo in modo sincrono: {k.code('ToList')} e{' '}
      {k.code('SaveChanges')}, non {k.code('ToListAsync')} e {k.code('SaveChangesAsync')}. Le tabelle le creano le
      migrazioni, mai Entity Framework. La demo {k.link('/editor/demo-crud', 'demo-crud')} fa tutto questo.
    </>
  ),

  keeping: (k) => (
    <>
      {k.code('Workspace')} è una cartella privata che la tua lambda può leggere e scrivere: il posto per i file, come
      le foto caricate da qualcuno, un documento che crea, un modello che legge. Le voci vanno nel database, e anche
      quello che si sa di un file (chi l’ha caricato, quando) è una voce.
    </>
  ),
  keeping2: (k) => (
    <>
      Ci sono anche {k.code('ReadBytes')}, {k.code('WriteBytes')}, {k.code('Delete')}, {k.code('List')},{' '}
      {k.code('CreateFolder')}, e {k.code('Tree')}/{k.code('Files')}/{k.code('App')} per servirlo. Il resto del file
      system non è raggiungibile.
    </>
  ),

  secrets: (k) => (
    <>
      Una chiave API, una password o un token va nei {k.b('secret')}, non nel codice, dove l’avrebbero ogni versione,
      ogni download e chiunque legga la cronologia. Il codice legge un secret per nome:
    </>
  ),
  secrets2: (k) => (
    <>
      Attiva i secret in {k.b('Dati')} e imposta lì il valore. Una volta salvato, non viene più mostrato, né a te né a
      un agente: puoi solo sostituirlo. L’elenco dice quali nomi legge il codice senza che ci sia ancora un valore, e la
      panoramica li chiede. {k.code('Secret.Exists')} dice se un secret è impostato, per il codice che funziona anche
      senza. Come tutti i dati, i secret sono condivisi da tutte le versioni, e una bozza lavora su una copia.
    </>
  ),
  secretsAside: (k) => (
    <>
      Sono salvati cifrati, con una chiave che non sta nel database. In un progetto scaricato,{' '}
      {k.code('Secret.Read("NAME")')} legge la variabile d’ambiente {k.code('NAME')}: i valori restano qui.
    </>
  ),

  sockets: (k) => (
    <>
      Supportati sul serio, non aggiunti all’ultimo momento. La demo {k.link('/editor/demo-game', 'demo-game')} abbina i
      giocatori e gestisce ogni partita sul server. La forma più semplice sono tre callback:
    </>
  ),
  socketsAside: (k) => (
    <>
      C’è un tranello in cui cadono tutti: un browser non può impostare header nell’handshake di un websocket. Passa
      quello che serve all’handler nella query, dove lo legge da {k.code('connection.Request.Header.Query')}, oppure
      manda i dati segreti come primo messaggio.
    </>
  ),
  sockets2: (k) => (
    <>
      Quando la pagina deve solo ascoltare (un contatore, un feed, una classifica), gli eventi inviati dal server sono
      più semplici: una sola risposta lunga su cui il server continua a scrivere e che il browser riapre da solo. La
      demo {k.link('/editor/demo-live', 'demo-live')} invia così ogni voto a tutti quelli che stanno guardando. In
      entrambi i casi è il server a inviare ciò che è cambiato. Una pagina che chiede di nuovo ogni pochi secondi
      manda una richiesta ogni volta, che sia cambiato qualcosa o no, ed è comunque in ritardo.
    </>
  ),

  limits:
    'Il tuo codice gira su un server condiviso, quindi una parte di C# viene rifiutata prima ancora della compilazione: avviare processi, aprire socket propri, caricare assembly, accedere al file system fuori dal workspace e usare la reflection per aggirare uno di questi limiti. Lo stesso vale per attendere un task con .Result o .Wait() invece di usare await: le richieste girano su un thread per core, e il task dovrebbe terminare proprio sul thread che lo sta aspettando.',
  limits2:
    'Tutto il resto c’è, compresa l’intera API dei moduli GenHTTP. Se qualcosa viene rifiutato, vedi quale riga e perché, non solo che non ha funzionato.',

  away: (k) => (
    <>
      Con {k.b('Scarica come progetto .NET')}, nell’editor, ti porti a casa tutto: una solution da aprire, avviare con{' '}
      {k.code('dotnet run')} e tenere. Le basta il pacchetto GenHTTP, e include un {k.code('Dockerfile')} per
      compilarla ed eseguirla come container.
    </>
  ),
  away2: (k) => (
    <>
      Il tuo snippet diventa {k.code('Project.cs')}, e {k.code('Program.cs')} serve ciò che restituisce. Gli altri file
      arrivano esattamente come li hai scritti. {k.code('Workspace')} e {k.code('Assets')} diventano due cartelle
      accanto al programma, con gli stessi metodi, a parte in una cartella {k.code('Platform')}, quindi nel tuo codice non
      devi cambiare niente.
      {' '}{k.code('Secret')} lì legge le variabili d’ambiente con lo stesso nome; i valori restano qui. Anche la
      documentazione e i test vengono con te, in {k.code('docs')} e {k.code('tests')}.
      {' '}{k.code('Database')} apre {k.code('database/database.db')}, che il download porta con sé insieme alle voci
      che la tua app ha conservato.
    </>
  ),
  awayAside:
    'Meglio saperlo prima di creare qualsiasi cosa qui: quello che scrivi è tuo e te lo porti via intero. Farlo girare su questo server non ti lega a questo server.',

  open: (k) => (
    <>
      Se quello che hai creato può servire a qualcun altro, pubblicane il codice: apri {k.b('Open source')} nel
      pannello di controllo, scegli una licenza (MIT, se non ne vuoi un’altra) e attiva l’opzione. Il codice ottiene una
      pagina tutta sua tra le {k.link('/source', 'app open source')}, dove chiunque può leggerlo, dargli una stella e
      scaricare qualsiasi versione come lo stesso progetto che ti dà {k.b('Scarica come progetto .NET')}, con accanto la
      licenza.
    </>
  ),
  open2: () => (
    <>
      Viene pubblicata ogni versione, anche quelle precedenti, con la sua documentazione, i suoi test e la modifica che
      ha fatto. Quello che l’app conserva non viene mai pubblicato (le sue voci, i file che ha salvato, i valori delle
      sue chiavi e password), e nemmeno quello che hai chiesto con le tue parole o chi usa l’app. Se disattivi
      l’opzione, la pagina sparisce; le sue stelle restano, per quando pubblicherai di nuovo il codice.
    </>
  ),
  openAside:
    'Tutto quello che c’è nel codice diventa pubblico, comprese le versioni precedenti. Una chiave o una password va tra le chiavi e password in Dati, mai nel codice, che sia pubblicato o no.',

  agents: (k) => (
    <>
      C’è un endpoint MCP su {k.code('/mcp')}. Collegaci un agente e potrà fare tutto quello che fa l’editor: leggere la
      guida, leggere una demo per intero, scrivere file, compilarli e fare il deploy. Sotto c’è la stessa API.
    </>
  ),
  agents2: (k) => (
    <>
      Mentre lavora spiega il perché ({k.code('write_code')} riceve la specifica e la modifica) e può controllare quello
      che ha pubblicato: {k.code('read_logs')} restituisce le richieste recenti della lambda, cosa ha stampato e lo stack
      trace di ogni eccezione. È così che un agente scopre che il suo codice funziona, invece di darlo per scontato. Tu
      vedi le stesse cose nel pannello di controllo. Man mano scrive la documentazione e i test, li legge prima di
      cambiare qualcosa ed esegue i test sull’indirizzo di una bozza prima di metterla online. A una pagina pensata
      per essere trovata dà un titolo, una descrizione, un’icona e un’anteprima per quando qualcuno ne condivide il
      link. In fondo alle pagine che costruisce aggiunge una piccola riga che dice che sono state fatte con GenHTTP
      Lambda: diglielo se preferisci non averla, e la toglie.
    </>
  ),
  more: 'Scopri di più →',
  make: 'Crea una lambda',
};
