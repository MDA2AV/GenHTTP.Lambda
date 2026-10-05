import type { EditorMessages } from '../../en/editor';

export const development: EditorMessages['development'] = {
  title: 'Spazio di sviluppo',
  hint: 'Ciò da cui sono costruiti gli asset di una versione, dove una toolchain li costruisce: il progetto del suo front-end, con i sorgenti, la configurazione e il lock file. Viene conservato con ogni versione e non viene mai compilato né servito. Chi lo modifica (il tuo agente, in un clone) lo costruisce dove lavora e lo salva insieme a ciò che ha costruito: questa piattaforma non costruisce niente. Qui quindi si legge, non si modifica.',
  overview: 'Panoramica',
  files: 'File',
  scope: (version) =>
    `Ciò da cui sono costruiti gli asset della versione ${version}: conservato con essa, mai compilato né servito, e costruito da chi lo modifica, mai qui.`,
  scopeDraft: 'Ciò da cui sono costruiti gli asset di questa bozza: conservato con essa, mai compilato né servito, e costruito da chi lo modifica, mai qui.',
  reading: 'Lettura dello spazio di sviluppo…',
  readFailed: 'Non è stato possibile leggere lo spazio di sviluppo.',

  emptyTitle: (version) => `Nessuno spazio di sviluppo nella versione ${version}`,
  emptyTitleDraft: 'Nessuno spazio di sviluppo in questa bozza',
  emptyText: (code) => (
    <>
      Quando un front-end viene costruito con una toolchain (React, Vue o Svelte con Vite, TypeScript, Tailwind), il suo
      progetto sta qui, con ogni versione: ciò da cui sono costruiti gli asset. Il tuo agente lo costruisce dove
      lavora e salva i sorgenti insieme a ciò che hanno prodotto; in un clone è la cartella {code('dev/')}. Un
      front-end in semplice HTML, CSS e JavaScript non ne ha bisogno.
    </>
  ),
  emptyHow: (code) => (
    <>{code('AGENTS.md')} in un clone spiega a un agente di programmazione come prepararne uno.</>
  ),

  projects: 'Progetti',
  atTheTop: 'lo spazio di sviluppo stesso',
  kinds: {
    npm: 'npm',
    deno: 'Deno',
    cargo: 'Rust',
    go: 'Go',
    python: 'Python',
    dotnet: '.NET',
    php: 'PHP',
    ruby: 'Ruby',
    maven: 'Maven',
    gradle: 'Gradle',
    make: 'Make',
  },
  builtWith: 'Costruito con',
  build: 'Build',
  noBuild: 'Nessuno script di build nel suo package.json.',
  into: 'Costruisce in',
  intoAssets: (folder, files, size) => (
    <>
      {folder} degli asset: {files === 1 ? '1 file' : `${files} file`}, {size} in questa versione
    </>
  ),
  intoNothing: (folder) => <>{folder} degli asset, che in questa versione non contiene nulla</>,
  packages: 'Pacchetti',
  packagesCount: (runtime, tooling) =>
    `${runtime === 1 ? '1 per l’esecuzione' : `${runtime} per l’esecuzione`}, ${tooling === 1 ? '1 per la build' : `${tooling} per la build`}`,
  showPackages: 'Mostrali',
  hidePackages: 'Nascondili',
  runtime: 'Per l’esecuzione',
  tooling: 'Per la build',
  missing: (page, files) => (
    <>
      {page} rimanda a {files.length === 1 ? 'un file' : `${files.length} file`} che non {files.length === 1 ? 'è' : 'sono'}{' '}
      tra gli asset ({files.slice(0, 3).join(', ')}{files.length > 3 ? ', …' : ''}): ciò che la build ha scritto non è
      stato salvato per intero e la pagina non si carica.
    </>
  ),
  noLock: 'Nessun lock file: la prossima build potrebbe installare versioni dei pacchetti diverse da quelle dell’ultima.',
  noIgnore: 'Nessun .gitignore: ciò che la sua toolchain installa e costruisce può finire in una versione.',

  inVersion: (version) => `Nella versione ${version}`,
  inDraft: 'In questa bozza',
  comparedWith: (version) => `rispetto alla versione ${version}`,
  first: 'La prima versione che lo ha.',
  both: (here, assets) =>
    `${here === 1 ? '1 file modificato' : `${here} file modificati`} qui e ${assets === 1 ? '1 file' : `${assets} file`} degli asset.`,
  hereOnly: (here) =>
    `${here === 1 ? '1 file modificato' : `${here} file modificati`} qui e nessuno degli asset: a meno che la modifica non richiedesse una build, i visitatori vedono ciò che vedevano prima.`,
  builtOnly: (folder) => (
    <>È cambiato ciò che viene costruito in {folder} e nulla qui: una modifica fatta in ciò che la build ha scritto viene annullata dalla build successiva.</>
  ),
  assetsOnly: 'Qui non è cambiato nulla.',
  unchanged: 'Non è cambiato nulla qui né negli asset.',
  showChanges: 'Mostra le modifiche',
  hideChanges: 'Nascondi le modifiche',
  noChanges: 'Qui non è cambiato nulla.',

  readme: 'Come viene costruito',
  noReadme: (code) => (
    <>
      Niente dice come viene costruito. Un {code('README.md')} in cima allo spazio di sviluppo, con i comandi e
      la destinazione della build, è ciò da cui parte il prossimo agente.
    </>
  ),
  readOnly: 'Sola lettura: si modifica dove viene costruito.',
  noFiles: 'Nessun file.',
};
