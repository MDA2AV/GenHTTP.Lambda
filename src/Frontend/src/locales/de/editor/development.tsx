import type { EditorMessages } from '../../en/editor';

export const development: EditorMessages['development'] = {
  title: 'Entwicklungsbereich',
  hint: 'Woraus die Assets einer Version gebaut werden, wo eine Toolchain sie baut: das Projekt ihres Frontends mit seinen Quellen, seiner Konfiguration und seiner Lock-Datei. Es wird mit jeder Version gespeichert und nie kompiliert oder ausgeliefert. Wer es ändert – Ihr Agent, in einem Klon – baut es dort, wo er arbeitet, und speichert es zusammen mit dem Ergebnis: Diese Plattform baut nichts. Deshalb wird es hier gelesen, nicht bearbeitet.',
  overview: 'Übersicht',
  files: 'Dateien',
  scope: (version) =>
    `Woraus die Assets von Version ${version} gebaut werden – mit ihr gespeichert, nie kompiliert oder ausgeliefert und gebaut von dem, der es ändert, nie hier.`,
  scopeDraft: 'Woraus die Assets dieses Entwurfs gebaut werden – mit ihm gespeichert, nie kompiliert oder ausgeliefert und gebaut von dem, der es ändert, nie hier.',
  reading: 'Entwicklungsbereich wird gelesen…',
  readFailed: 'Der Entwicklungsbereich konnte nicht gelesen werden.',

  emptyTitle: (version) => `Kein Entwicklungsbereich in Version ${version}`,
  emptyTitleDraft: 'Kein Entwicklungsbereich in diesem Entwurf',
  emptyText: (code) => (
    <>
      Wird ein Frontend mit einer Toolchain gebaut – React, Vue oder Svelte mit Vite, TypeScript, Tailwind –, liegt
      sein Projekt hier, mit jeder Version: das, woraus die Assets gebaut werden. Ihr Agent baut es dort, wo er
      arbeitet, und speichert die Quellen zusammen mit dem, was sie ergeben haben – in einem Klon ist das der Ordner{' '}
      {code('dev/')}. Ein Frontend aus reinem HTML, CSS und JavaScript braucht keinen.
    </>
  ),
  emptyHow: (code) => (
    <>{code('AGENTS.md')} in einem Klon sagt einem Coding-Agenten, wie er einen einrichtet.</>
  ),

  projects: 'Projekte',
  atTheTop: 'der Entwicklungsbereich selbst',
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
  builtWith: 'Gebaut mit',
  build: 'Build',
  noBuild: 'Kein Build-Script in seiner package.json.',
  into: 'Baut nach',
  intoAssets: (folder, files, size) => (
    <>
      {folder} der Assets – {files === 1 ? '1 Datei' : `${files} Dateien`}, {size} in dieser Version
    </>
  ),
  intoNothing: (folder) => <>{folder} der Assets – der in dieser Version nichts enthält</>,
  packages: 'Pakete',
  packagesCount: (runtime, tooling) =>
    `${runtime === 1 ? '1 zum Ausführen' : `${runtime} zum Ausführen`}, ${tooling === 1 ? '1 zum Bauen' : `${tooling} zum Bauen`}`,
  showPackages: 'Anzeigen',
  hidePackages: 'Ausblenden',
  runtime: 'Zum Ausführen',
  tooling: 'Zum Bauen',
  missing: (page, files) => (
    <>
      {page} verweist auf {files.length === 1 ? 'eine Datei' : `${files.length} Dateien`}, die nicht unter den Assets {files.length === 1 ? 'ist' : 'sind'}
      {' '}({files.slice(0, 3).join(', ')}{files.length > 3 ? ', …' : ''}): Was der Build geschrieben hat, wurde nicht
      vollständig gespeichert, und die Seite lädt nicht.
    </>
  ),
  noLock: 'Keine Lock-Datei: Der nächste Build kann andere Versionen seiner Pakete installieren als der letzte.',
  noIgnore: 'Keine .gitignore: Was seine Toolchain installiert und baut, kann in einer Version landen.',

  inVersion: (version) => `In Version ${version}`,
  inDraft: 'In diesem Entwurf',
  comparedWith: (version) => `gegenüber Version ${version}`,
  first: 'Die erste Version, die ihn hat.',
  both: (here, assets) =>
    `${here === 1 ? '1 Datei' : `${here} Dateien`} hier geändert und ${assets === 1 ? '1 Datei' : `${assets} Dateien`} der Assets.`,
  hereOnly: (here) =>
    `${here === 1 ? '1 Datei' : `${here} Dateien`} hier geändert, aber keine der Assets: Sofern die Änderung keinen Build brauchte, erhalten Besucher, was sie vorher erhielten.`,
  builtOnly: (folder) => (
    <>Was nach {folder} gebaut wird, hat sich geändert, hier aber nichts: Eine Änderung an dem, was der Build geschrieben hat, macht der nächste Build rückgängig.</>
  ),
  assetsOnly: 'Hier hat sich nichts geändert.',
  unchanged: 'Weder hier noch in den Assets hat sich etwas geändert.',
  showChanges: 'Änderungen anzeigen',
  hideChanges: 'Änderungen ausblenden',
  noChanges: 'Hier hat sich nichts geändert.',

  readme: 'Wie er gebaut wird',
  noReadme: (code) => (
    <>
      Nichts sagt, wie er gebaut wird. Eine {code('README.md')} oben im Entwicklungsbereich – die Befehle und wohin
      der Build schreibt – ist das, woraus der nächste Agent baut.
    </>
  ),
  readOnly: 'Schreibgeschützt: Er wird dort geändert, wo er gebaut wird.',
  noFiles: 'Keine Dateien.',
};
