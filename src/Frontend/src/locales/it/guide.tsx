import type { Messages } from '../en';

export const guide: Messages['guide'] = {
  title: 'Come funziona',
  intro:
    'Si scrive un frammento di codice C#. Ciò che restituisce viene pubblicato in pochi secondi a un indirizzo pubblico, via HTTPS. Questa pagina descrive l’intera piattaforma nell’ordine in cui la si incontra.',
  contents: 'Indice',

  parts: {
    what: 'Che cos’è un lambda',
    first: 'Il primo lambda',
    editor: 'Il centro di controllo',
    why: 'Documentare le modifiche',
    files: 'Più file',
    page: 'Servire una pagina',
    spa: 'Un front-end, passo dopo passo',
    storage: 'I due luoghi in cui risiedono i file',
    keeping: 'Conservare i dati',
    sockets: 'WebSocket',
    limits: 'Limitazioni',
    away: 'Esportare il codice',
    agents: 'Lavorare con un agente',
  },

  what: [
    (k) => (
      <>
        Un lambda è un frammento di codice che restituisce un handler GenHTTP. La piattaforma lo compila, lo carica e
        rende disponibile il risultato al Suo indirizzo. Non servono progetto, file di build né istruzioni{' '}
        {k.code('using')}: tutti i moduli GenHTTP sono già importati.
      </>
    ),
    (k) => (
      <>
        Questo è già un lambda completo. Distribuito all’indirizzo {k.code('/lambda/your-key/')}, risponde a ogni
        richiesta con la parola «hello».
      </>
    ),
  ],
  whatAside: (k) => (
    <>
      Il frammento è composto da {k.em('istruzioni')}, non da una classe. Come ultima operazione restituisce un oggetto in
      grado di gestire le richieste: un handler oppure un builder per un handler.
    </>
  ),

  first: [
    (k) => (
      <>
        Selezioni {k.b('Crea lambda')}. Riceverà un indirizzo pubblico e una chiave di modifica. La chiave è l’unico
        accesso e nessuno può recuperarla al posto Suo: la conservi con cura.
      </>
    ),
    () => (
      <>
        Si accede al centro di controllo, dove un piccolo servizio REST è già presente come prima versione. Si tratta solo
        di un punto di partenza.
      </>
    ),
    (k) => (
      <>
        Consegni la chiave di modifica a un agente e descriva ciò che deve realizzare: scriverà nuove versioni tramite{' '}
        {k.link('/#agents', 'MCP')}. In alternativa apra {k.b('Codice')} e scriva autonomamente: {k.b('Verifica')}{' '}
        compila senza salvare e mostra i messaggi del compilatore, con file e riga.
      </>
    ),
    (k) => (
      <>
        Selezioni {k.b('Distribuisci')}. Il lambda è ora online; prima non è raggiungibile. Ogni nuova distribuzione ne
        prolunga la permanenza online.
      </>
    ),
  ],

  editor: (k) => (
    <>
      Il link di modifica apre un centro di controllo anziché un semplice editor di testo: gran parte del codice viene
      scritta da agenti, perciò la prima informazione mostrata è lo stato del lambda. La barra laterale indica se è online,
      il suo indirizzo, un pulsante quando una versione più recente attende la distribuzione, e le varie sezioni. Le
      azioni meno frequenti, come cambiare l’indirizzo o eliminarlo, si trovano nel menu {k.b('⋯')}.
    </>
  ),
  bits: [
    ['Panoramica', () => <>Lo stato online, le richieste ricevute oggi e quelle non riuscite, l’ultima modifica e lo spazio disponibile.</>],
    ['File', () => <>I file di una versione e i dati che il lambda salva durante l’esecuzione. Un lucchetto o un globo indica se sono accessibili pubblicamente.</>],
    ['Versioni', () => <>Le modifiche e le richieste di ciascuna versione, e la differenza rispetto alla precedente. Da qui si distribuisce o si ripristina.</>],
    ['Distribuzioni', () => <>Cosa è stato online, quando, e cosa lo ha disattivato.</>],
    ['Statistiche', () => <>Richieste, errori, tempi di risposta e percorsi più richiesti, nell’ultima ora o nelle ultime 24 ore.</>],
    ['Log', () => <>Le richieste, l’output del lambda e lo stack trace di ogni errore, in tempo reale.</>],
    [
      'Codice',
      (k) => (
        <>
          Modifica manuale. {k.b('Verifica')} compila, {k.b('Salva')} crea una versione, {k.b('Distribuisci')} la mette
          online. {k.code('Ctrl+S')} salva; {k.code('F12')} passa a una dichiarazione.
        </>
      ),
    ],
  ],
  sections: (k) => (
    <>
      Tutte le sezioni hanno la stessa struttura: un titolo, un {k.b('ⓘ')} con la spiegazione, le azioni sulla destra e –
      quando esistono più viste – una fila di selettori sotto. Per il codice, i selettori sono i file.
    </>
  ),
  editorAside:
    'Il traffico e il log sono conservati in memoria, per il monitoraggio e non per l’archiviazione: un riavvio del server li azzera. Le versioni e la cronologia delle distribuzioni vengono salvate in modo permanente.',

  why: (k) => (
    <>
      Una versione è costituita dal codice e, facoltativamente, da due note: {k.b('la specifica')}, ossia ciò che
      l’utente desidera e perché, possibilmente con le sue parole, e {k.b('la modifica')}, una riga su ciò che fa la
      versione. Sono mostrate accanto al diff nella cronologia delle versioni, così il {k.em('perché')} resta accanto al{' '}
      {k.em('cosa')} – per Lei e per il prossimo agente che consulterà la cronologia prima di intervenire.
    </>
  ),
  whySample: {
    specification: 'Un guestbook da firmare; le voci devono sopravvivere a un riavvio',
    change: 'Salva le voci nel workspace affinché sopravvivano a un riavvio',
  },
  why2: (k) => (
    <>
      Gli agenti passano gli stessi due campi a {k.code('write_code')}. In {k.b('Codice')}, al salvataggio viene richiesta
      la modifica. Entrambi sono facoltativi; una specifica lunga viene troncata a 4000 caratteri e una modifica a 500,
      anziché essere rifiutata.
    </>
  ),

  files: (k) => (
    <>
      I tipi non devono trovarsi sotto il codice che li utilizza. In {k.b('Codice')}, selezioni {k.b('+')} accanto ai
      file: il nuovo file viene compilato insieme al frammento, nello stesso namespace, senza bisogno di importazioni. Un
      nome senza estensione viene considerato un file C#.
    </>
  ),

  page: 'Esistono tre modalità; la scelta dipende da dove si trova la pagina.',
  inlineTitle: 'Una pagina scritta nel codice',
  inline: 'Adatta a contenuti semplici. La pagina fa parte del frammento.',
  folderTitle: 'Una cartella di file',
  folder:
    'La soluzione indicata per qualsiasi contenuto con fogli di stile e script. I file si aggiungono come un file C# e vengono serviti esattamente come sono scritti, senza compilazione.',
  workspaceTitle: 'Dal workspace',
  workspace: 'Quando la pagina viene caricata anziché scritta e deve poter cambiare senza una nuova distribuzione.',

  spa: (k) => (
    <>
      La seconda modalità, in dettaglio. Ogni demo serve la propria pagina in questo modo da una cartella chiamata{' '}
      {k.code('web')}: apra {k.link('/editor/demo-crud', 'demo-crud')} per consultarne una. Le demo sono in sola lettura;
      la loro chiave di modifica coincide con il nome.
    </>
  ),
  spaSteps: [
    (k) => (
      <>
        In {k.b('Codice')}, selezioni {k.b('+')} accanto ai file e digiti {k.code('site/index.html')}. Un nome con barra
        colloca il file in una cartella; un nome con estensione viene trattato come il tipo di file indicato.
      </>
    ),
    (k) => (
      <>
        Aggiunga {k.code('site/app.css')} e {k.code('site/app.js')} nello stesso modo. La pagina vi fa riferimento per nome,
        ad esempio {k.code('href="app.css"')}, poiché la cartella costituisce la radice di ciò che viene servito e non fa
        parte dell’indirizzo.
      </>
    ),
    (k) => (
      <>
        Per i file non testuali, come un’immagine o un font, apra un file in {k.code('site')} e utilizzi il pulsante di
        caricamento accanto ai file: verrà collocato nella stessa cartella.
      </>
    ),
    (k) => <>In {k.code('lambda.cs')}, serva la cartella:</>,
    (k) => (
      <>
        Selezioni {k.b('Distribuisci')}. {k.code('site/index.html')} risponde su {k.code('/')}, {k.code('site/app.css')}{' '}
        su {k.code('/app.css')}, e qualsiasi indirizzo senza file corrispondente riceve la pagina: un front-end con routing
        proprio funziona quindi anche quando si ricarica un link interno.
      </>
    ),
    () => <>Aggiunga un’API accanto, con cui la pagina possa comunicare:</>,
  ],

  storage: (k) => (
    <>
      La sezione {k.b('File')} mostra entrambi – i file di una versione e il workspace come {k.b('Dati')} – e indica quali
      sono accessibili pubblicamente. I file del codice si modificano in {k.b('Codice')}; i dati possono essere caricati ed
      eliminati in {k.b('File')}. Non sono però la stessa cosa, e la differenza sta in{' '}
      {k.em('quando cambiano')}.
    </>
  ),
  savedWithCode: 'Salvato con il codice',
  workspaceColumn: 'Workspace',
  table: [
    ['contenuto', 'tutti i file del lambda, C# incluso', 'tutto ciò che è stato scritto o caricato'],
    ['quando cambia', 'al salvataggio o alla distribuzione', 'non appena vi viene scritto qualcosa'],
    ['una distribuzione', 'sostituisce tutto', 'non lo modifica'],
    ['il ripristino di una versione', 'riporta i file precedenti', 'nessun effetto'],
    ['la clonazione del lambda', 'viene copiato', 'non viene copiato'],
  ],
  reachedAs: 'accessibile dal codice come',
  storageAside:
    'Non possono essere un’unica directory. Altrimenti una distribuzione cancellerebbe tutto ciò che il lambda ha scritto nel frattempo, oppure nulla potrebbe mai essere rimosso da ciò che pubblica. Una classifica richiede la seconda proprietà; la pagina che la mostra, la prima.',

  keeping: (k) => (
    <>
      {k.code('Workspace')} è una directory privata che il lambda può leggere e scrivere. È il luogo per tutto ciò che deve
      sopravvivere a una richiesta o a una distribuzione.
    </>
  ),
  keeping2: (k) => (
    <>
      Sono disponibili anche {k.code('ReadBytes')}, {k.code('WriteBytes')}, {k.code('Delete')}, {k.code('List')},{' '}
      {k.code('CreateFolder')} e {k.code('Tree')}/{k.code('Files')}/{k.code('App')} per servirla. Il resto del file system
      non è accessibile.
    </>
  ),

  sockets: (k) => (
    <>
      Pienamente supportati. La demo {k.link('/editor/demo-game', 'demo-game')} abbina i giocatori ed esegue ogni partita
      sul server. La forma più semplice prevede tre callback:
    </>
  ),
  socketsAside: (k) => (
    <>
      Un aspetto da tenere presente: il browser non può impostare header durante l’handshake di un WebSocket. Passi ciò che
      serve all’handler nella query, da cui lo legge tramite {k.code('connection.Request.Header.Query')}, oppure invii le
      informazioni riservate nel primo messaggio.
    </>
  ),

  limits:
    'Il codice viene eseguito su un server condiviso, perciò alcune funzionalità di C# vengono rifiutate prima della compilazione: avviare processi, aprire socket propri, caricare assembly, accedere al file system al di fuori del workspace e usare la reflection per aggirare tali restrizioni.',
  limits2:
    'Tutto il resto è disponibile, inclusa l’intera API dei moduli GenHTTP. In caso di rifiuto, vengono indicati la riga e il motivo.',

  away: (k) => (
    <>
      {k.b('Scarica come progetto .NET')}, nell’editor, fornisce l’intero lambda come progetto .NET: una soluzione che si
      può aprire, eseguire con {k.code('dotnet run')} e conservare. Contiene un solo riferimento a pacchetto e nessuna
      dipendenza da questa piattaforma.
    </>
  ),
  away2: (k) => (
    <>
      Il frammento diventa il corpo di {k.code('Program.cs')}, inserito in un host che serve ciò che restituisce. Gli altri
      file vengono mantenuti esattamente come scritti. {k.code('Workspace')} e {k.code('Assets')} diventano due cartelle
      accanto al codice, con gli stessi metodi: il codice non deve essere modificato.
    </>
  ),
  awayAside:
    'È bene saperlo prima di iniziare: il codice che scrive appartiene a Lei e può essere esportato integralmente. Eseguirlo su questa piattaforma non La vincola a essa.',

  agents: (k) => (
    <>
      All’indirizzo {k.code('/mcp')} è disponibile un endpoint MCP. Un agente collegato può fare tutto ciò che consente
      l’editor: leggere la guida, consultare una demo per intero, scrivere file, compilarli e distribuirli. Entrambi
      utilizzano la stessa API.
    </>
  ),
  agents2: (k) => (
    <>
      L’agente documenta le proprie scelte – {k.code('write_code')} riceve la specifica e la modifica – e può verificare ciò
      che ha distribuito: {k.code('read_logs')} restituisce le richieste recenti del lambda, il suo output e lo stack trace
      di ogni eccezione. È così che un agente verifica che il codice funzioni, anziché presumerlo. Le stesse informazioni
      sono disponibili nel centro di controllo.
    </>
  ),
  more: 'Maggiori informazioni →',
  make: 'Crea un lambda',
};
