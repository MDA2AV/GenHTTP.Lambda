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
    files: 'Più di un file',
    page: 'Servire una pagina',
    spa: 'Un front-end, passo per passo',
    storage: 'I due posti dove stanno i file',
    keeping: 'Salvare i dati',
    sockets: 'WebSocket',
    limits: 'Cosa non puoi fare',
    away: 'Portarla via',
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
        Passa la chiave di modifica a un agente e digli cosa creare: lavora tramite {k.link('/#agents', 'MCP')}, con una
        versione per ogni cosa che chiedi. Oppure apri {k.b('Codice')} e scrivi tu il codice: {k.b('Verifica')} compila
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
      lambda (se è online, il suo indirizzo e un pulsante quando c’è qualcosa da mettere online) e le sue sezioni. Le
      azioni che servono di rado, come cambiare l’indirizzo o eliminarla, sono nel menu {k.b('⋯')}.
    </>
  ),
  bits: [
    ['Panoramica', () => <>Se è online, quante richieste ha ricevuto oggi e quante sono fallite, l’ultima modifica e quanto spazio resta.</>],
    [
      'Modifica',
      (k) => (
        <>
          Scrivi cosa deve cambiare e l’agente di questo server lo fa sotto i tuoi occhi: legge il codice, lo modifica,
          controlla che compili e lo mette online come nuova versione. Disattiva {k.b('Metti online a lavoro finito')}{' '}
          per guardarlo prima tu.
        </>
      ),
    ],
    ['File', () => <>I file di una versione: il codice e gli asset, cioè il programma vero e proprio. Un lucchetto o un globo indica se sono pubblici.</>],
    ['Dati', () => <>Quello che la lambda conserva mentre gira, condiviso da tutte le versioni: il workspace. Puoi guardarci dentro, caricare ed eliminare file, o disattivarlo.</>],
    ['Versioni', () => <>Cosa ha cambiato ogni versione, cosa era stato chiesto e le differenze rispetto alla precedente. La più recente è quella su cui si lavora. Da qui fai il deploy, torni indietro o inizi una nuova versione.</>],
    ['Deployment', () => <>Cosa è stato online e quando, e cosa l’ha fermato.</>],
    ['Statistiche', () => <>Richieste, errori, tempi di risposta e i percorsi più richiesti, nell’ultima ora o nelle ultime 24 ore.</>],
    ['Log', () => <>Le richieste, cosa ha stampato e lo stack trace di ogni errore, in tempo reale.</>],
    [
      'Codice',
      (k) => (
        <>
          Per scriverlo a mano. {k.b('Verifica')} compila, {k.b('Salva')} aggiorna la versione più recente,{' '}
          {k.b('Nuova versione')} la lascia com’è e ne inizia un’altra, {k.b('Deploy')} la mette online.{' '}
          {k.code('Ctrl-S')} salva; {k.code('F12')} va alla dichiarazione.
        </>
      ),
    ],
  ],
  sections: (k) => (
    <>
      Ogni sezione funziona allo stesso modo: il titolo, il pulsante {k.b('ⓘ')} che la spiega, le azioni a destra e,
      dove ci sono più viste, una fila di schede sotto. Nel codice, le schede sono i file.
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
    change: 'Salva le voci nel workspace così sopravvivono a un riavvio',
  },
  why2: (k) => (
    <>
      Gli agenti passano gli stessi due campi a {k.code('write_code')}. In {k.b('Codice')}, {k.b('Nuova versione')} ti
      chiede la modifica. Sono entrambi facoltativi: una specifica lunga viene tagliata a 4000 caratteri e una modifica a
      500, invece di essere rifiutata. Quando salvi in una versione esistente, le sue note restano, a meno che tu non ne
      dia di nuove.
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
    ['cosa contiene', 'il codice e gli asset: il programma, front-end compreso', 'quello che scrive la lambda o che carica qualcuno'],
    ['quando cambia', 'quando la versione viene salvata', 'appena ci viene scritto qualcosa'],
    ['un deploy', 'mette online esattamente questi file', 'non li tocca mai'],
    ['tornare indietro', 'riporta i vecchi file', 'nessun effetto: tutte le versioni li condividono'],
    ['una nuova versione', 'parte da una copia di questi file', 'nessun effetto'],
    ['quando sparisce', 'con le versioni vecchie, oltre il limite', 'con la lambda, o quando disattivi il workspace'],
  ],
  reachedAs: 'dal codice si raggiunge con',
  storageAside:
    'Non possono stare in un unico posto. Se ci stessero, un deploy cancellerebbe tutto ciò che la lambda ha scritto nel frattempo, oppure non si potrebbe mai togliere niente da ciò che pubblica. Un gioco con una classifica vuole la seconda cosa; la pagina che serve vuole la prima. Quindi la pagina va nella versione, e la classifica nei dati.',

  keeping: (k) => (
    <>
      {k.code('Workspace')} è una cartella privata che la tua lambda può leggere e scrivere. È il posto per tutto ciò
      che deve sopravvivere a una richiesta, o a un deploy.
    </>
  ),
  keeping2: (k) => (
    <>
      Ci sono anche {k.code('ReadBytes')}, {k.code('WriteBytes')}, {k.code('Delete')}, {k.code('List')},{' '}
      {k.code('CreateFolder')}, e {k.code('Tree')}/{k.code('Files')}/{k.code('App')} per servirlo. Il resto del file
      system non è raggiungibile.
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

  limits:
    'Il tuo codice gira su un server condiviso, quindi una parte di C# viene rifiutata prima ancora della compilazione: avviare processi, aprire socket propri, caricare assembly, accedere al file system fuori dal workspace e usare la reflection per aggirare uno di questi limiti.',
  limits2:
    'Tutto il resto c’è, compresa l’intera API dei moduli GenHTTP. Se qualcosa viene rifiutato, vedi quale riga e perché, non solo che non ha funzionato.',

  away: (k) => (
    <>
      Con {k.b('Scarica come progetto .NET')}, nell’editor, ti porti a casa tutto: una solution da aprire, avviare con{' '}
      {k.code('dotnet run')} e tenere. Ha un solo riferimento a un pacchetto e nessuna traccia di questa piattaforma.
    </>
  ),
  away2: (k) => (
    <>
      Il tuo snippet diventa il corpo di {k.code('Program.cs')}, dentro un host che serve ciò che restituisce. Gli altri
      file arrivano esattamente come li hai scritti. {k.code('Workspace')} e {k.code('Assets')} diventano due cartelle
      accanto al codice, con gli stessi metodi, quindi nel tuo codice non devi cambiare niente.
    </>
  ),
  awayAside:
    'Meglio saperlo prima di creare qualsiasi cosa qui: quello che scrivi è tuo e te lo porti via intero. Farlo girare su questo server non ti lega a questo server.',

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
      vedi le stesse cose nel pannello di controllo.
    </>
  ),
  more: 'Scopri di più →',
  make: 'Crea una lambda',
};
