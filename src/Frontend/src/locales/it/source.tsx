import type { SourceMessages } from '../en/source';

/** I testi delle pagine del codice pubblicato, /source e ogni progetto, in italiano. */
export const source: SourceMessages = {
  shell: {
    section: 'Open source',
    home: 'GenHTTP Lambda, la pagina iniziale',
  },

  lambda: {
    label: 'Cos’è una lambda?',
    text: 'Un’app web su GenHTTP Lambda: qualcuno dice cosa vuole, un agente AI la scrive in C# e in pochi minuti è online a un indirizzo tutto suo, con ogni versione conservata insieme a quello che ha cambiato.',
    build: 'Crea la tua',
  },

  catalog: {
    eyebrow: 'Open source',
    title: 'Scopri come sono fatte le app create qui',
    intro:
      'Lambda i cui proprietari hanno pubblicato il codice: ogni versione, cosa ha cambiato, la documentazione e i test. Leggilo qui, o scarica un progetto che gira ovunque giri .NET.',
    searchLabel: 'Cerca tra i progetti',
    searchPlaceholder: 'Cerca per nome o per quello che fa',
    orderLabel: 'Ordine',
    orders: {
      stars: 'Più stelle',
      updated: 'Modificati di recente',
      published: 'Pubblicati di recente',
    },
    counted: (total) => (total === 1 ? '1 progetto' : `${total} progetti`),
    failed: 'Impossibile caricare i progetti.',
    loadingMore: 'Caricamento…',
    showMore: 'Mostra altri',
    nothingTitle: 'Non è ancora stato pubblicato niente',
    nothing: (tab) => (
      <>
        Hai creato qualcosa da cui altri potrebbero imparare? Apri il suo pannello di controllo, vai su{' '}
        {tab('Open source')}, scegli una licenza e il suo codice comparirà qui.
      </>
    ),
    noMatchTitle: 'Nessun risultato',
    noMatch: (query) => `Nessun progetto pubblicato contiene «${query}».`,
    clear: 'Mostra tutti i progetti',
    yoursTitle: 'Pubblica il tuo',
    yours: (tab) => (
      <>
        Apri il pannello di controllo della tua lambda e scegli {tab('Open source')}, oppure chiedi all’agente che l’ha
        creata di pubblicarla. Può farlo solo chi ha la chiave di modifica, con la licenza che sceglie, e quello che l’app
        conserva (le sue voci, i suoi file e le sue chiavi) non ne fa mai parte.
      </>
    ),
    build: 'Crea qualcosa',
    online: 'Online',
    offline: 'Offline',
    changed: (ago) => `ultima modifica: ${ago}`,
    stars: (count) => (count === 1 ? '1 stella' : `${count} stelle`),
  },

  project: {
    loading: 'Caricamento del codice…',
    failed: 'Impossibile caricare il codice.',
    missingTitle: 'Qui non c’è nessun codice pubblicato',
    missing: 'Il proprietario potrebbe averlo ritirato, oppure a questo indirizzo non c’è mai stata una lambda.',
    all: 'Tutti i progetti',
    by: (name) => `di ${name}`,
    versions: (count) => (count === 1 ? '1 versione' : `${count} versioni`),
    onlineAt: (address) => <>Online all’indirizzo {address}</>,
    offline: 'Offline in questo momento',
    openApp: 'Apri l’app',
    opens: (address) => `Apre ${address} in una nuova scheda`,
    published: (ago) => `Pubblicato: ${ago}`,
    changed: (ago) => `Ultima modifica: ${ago}`,
    picture: (name) => `${name}: come si presenta`,
    tabsLabel: 'Cosa leggere',
    tabs: {
      code: 'Codice',
      docs: 'Documentazione',
      tests: 'Test',
      changes: 'Modifiche',
    },
  },

  versions: {
    label: 'Versione',
    choose: 'Leggi un’altra versione',
    newest: 'più recente',
    online: 'online',
    older: (version, ago, newest) =>
      `Stai leggendo la versione ${version} (salvata: ${ago}). La più recente è la versione ${newest}.`,
    toNewest: 'Leggi la più recente',
    noChange: 'Nessuna nota su cosa ha cambiato',
  },

  star: {
    star: 'Stella',
    add: 'Metti una stella a questo progetto',
    remove: 'Togli la tua stella',
    count: (count) => (count === 1 ? '1 stella' : `${count} stelle`),
    failed: 'Impossibile salvare la stella.',
  },
  clone: {
    button: 'Codice',
    title: 'Clona con git',
    what: (oldest, newest) =>
      oldest === newest
        ? `La sua versione è il commit di main, con il tag v${newest}.`
        : `Ogni versione arriva come commit di main, con i tag da v${oldest} a v${newest}: main è la più recente.`,
    readOnly:
      'Sola lettura. Per costruirci sopra, crea una lambda tua e porta qui questi file: AGENTS.md nel clone spiega come, e la licenza cosa puoi fare.',
  },

  download: {
    title: (version) => `La versione ${version} come progetto`,
    what:
      'Un progetto .NET 10 con un Dockerfile, la sua documentazione, i suoi test e la sua licenza. Quello che l’app conserva (le sue voci, i file che ha salvato, le sue chiavi) non ne fa parte.',
    zip: 'Scarica lo ZIP',
    preparing: 'Preparazione del progetto…',
    slow: 'Al primo download una versione viene impacchettata mentre aspetti.',
    failed: 'Impossibile preparare il progetto. Riprova tra un momento.',
    run: 'Avvialo',
    local: 'Con l’SDK di .NET 10:',
    container: 'Oppure in un container:',
    agent: 'Oppure passa la cartella al tuo agente di programmazione e costruisci da lì, rispettando la sua licenza.',
    copy: 'Copia',
    copied: 'Copiato',
  },

  tree: {
    label: 'File',
    files: (count) => (count === 1 ? '1 file' : `${count} file`),
    packing: 'Preparazione di questa versione…',
    packingSlow: 'Una versione viene impacchettata la prima volta che qualcuno la legge: per una grande ci vuole un momento.',
    failed: 'Impossibile caricare i file di questa versione.',
    legend: 'Cosa c’è dove',
    kinds: {
      code: 'Il codice della lambda',
      asset: 'Ciò che viene servito: pagine, script, stili, immagini, e le migrazioni del database',
      docs: 'Cos’è, e perché è costruita così',
      tests: 'Come viene testata',
      platform: 'Ciò che sostituisce la piattaforma',
      project: 'L’host, la build, il container e la licenza',
    },
    short: {
      code: 'Codice',
      asset: 'Servito',
      docs: 'Doc',
      tests: 'Test',
      platform: 'Piattaforma',
      project: 'Progetto',
    },
  },

  file: {
    loading: 'Caricamento…',
    failed: 'Impossibile caricare questo file.',
    missing: (path) => `In questa versione non c’è nessun file ${path}.`,
    binary: 'Questo file non è testo.',
    tooLarge: 'Questo file è troppo lungo per mostrarlo qui.',
    download: 'Scarica',
    raw: 'Raw',
    rawTitle: 'Apri il file così com’è',
    copy: 'Copia',
    copied: 'Copiato',
    lines: (count) => (count === 1 ? '1 riga' : `${count} righe`),
    plain: 'Mostrato senza colori: è lungo.',
    line: (line) => `Riga ${line}`,
  },

  docs: {
    pages: 'Pagine',
    product: 'Cos’è',
    decisions: 'Decisioni',
    loading: 'Caricamento…',
    failed: 'Impossibile caricare questa pagina.',
    noneTitle: 'Non c’è niente di scritto su questa versione',
    none: 'La sua documentazione sarebbe in docs/: cos’è l’app, per chi è e perché è costruita così.',
  },

  tests: {
    files: 'Script e dati',
    noneTitle: 'Questa versione non dice niente sui suoi test',
    none: 'Come viene testata sarebbe scritto in tests/README.md, con accanto gli script che esegue.',
  },

  changes: {
    title: 'Tutte le versioni, dalla più recente',
    intro: 'Una versione, una volta salvata, non cambia più. Ognuna dice in una riga cosa ha cambiato.',
    agent: 'Scritto da un agente',
    online: 'online',
    browse: 'Leggi il codice',
    noChange: 'Nessuna nota',
  },

  licenses: {
    MIT: 'Chiunque può usarlo, modificarlo e ridistribuirlo, in qualsiasi progetto, purché la licenza e l’avviso di copyright restino con il codice.',
    'Apache-2.0': 'Come MIT, con in più una licenza sui brevetti da parte di chiunque abbia contribuito, e le modifiche segnalate come tali.',
    'BSD-3-Clause': 'Come MIT, e nessuno può usare il nome degli autori per promuovere quello che ne ha ricavato.',
    'MPL-2.0': 'Le modifiche a questi file restano sotto la stessa licenza; i file si possono combinare con codice sotto qualsiasi altra.',
    'GPL-3.0-or-later': 'Chi lo ridistribuisce, modificato o no, deve ridistribuirne anche il codice sorgente con la stessa licenza.',
    'AGPL-3.0-or-later': 'Come la GPL, e offrire una copia modificata ad altri tramite la rete conta come ridistribuirla.',
    Unlicense: 'Donato al pubblico dominio: chiunque può farne quello che vuole, senza condizioni.',
  },

  kinds: {
    Permissive: 'Permissiva',
    Copyleft: 'Copyleft',
    PublicDomain: 'Pubblico dominio',
  },
};
