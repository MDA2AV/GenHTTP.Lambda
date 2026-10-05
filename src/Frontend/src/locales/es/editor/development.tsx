import type { EditorMessages } from '../../en/editor';

export const development: EditorMessages['development'] = {
  title: 'Espacio de desarrollo',
  hint: 'Aquello a partir de lo que se compilan los recursos de una versión, allí donde una cadena de herramientas los compila: el proyecto de su frontend, con sus fuentes, su configuración y su archivo de bloqueo. Se guarda con cada versión y nunca se compila ni se sirve. Quien lo cambia (tu agente, en un clon) lo compila donde trabaja y lo guarda junto con lo que compiló: esta plataforma no compila nada. Por eso aquí se lee, no se edita.',
  overview: 'Resumen',
  files: 'Archivos',
  scope: (version) =>
    `Aquello a partir de lo que se compilan los recursos de la versión ${version}: se guarda con ella, nunca se compila ni se sirve, y lo compila quien lo cambia, nunca aquí.`,
  scopeDraft: 'Aquello a partir de lo que se compilan los recursos de este borrador: se guarda con él, nunca se compila ni se sirve, y lo compila quien lo cambia, nunca aquí.',
  reading: 'Leyendo el espacio de desarrollo…',
  readFailed: 'No se pudo leer el espacio de desarrollo.',

  emptyTitle: (version) => `Sin espacio de desarrollo en la versión ${version}`,
  emptyTitleDraft: 'Sin espacio de desarrollo en este borrador',
  emptyText: (code) => (
    <>
      Cuando un frontend se compila con una cadena de herramientas (React, Vue o Svelte con Vite, TypeScript,
      Tailwind), su proyecto se guarda aquí, con cada versión: aquello a partir de lo que se compilan los recursos. Tu
      agente lo compila donde trabaja y guarda las fuentes junto con lo que compilaron; en un clon es la carpeta{' '}
      {code('dev/')}. Un frontend de HTML, CSS y JavaScript sin más no necesita ninguno.
    </>
  ),
  emptyHow: (code) => (
    <>{code('AGENTS.md')} en un clon le explica a un agente de programación cómo crear uno.</>
  ),

  projects: 'Proyectos',
  atTheTop: 'el propio espacio de desarrollo',
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
  builtWith: 'Compilado con',
  build: 'Compilación',
  noBuild: 'Sin script de compilación en su package.json.',
  into: 'Compila en',
  intoAssets: (folder, files, size) => (
    <>
      {folder} de los recursos: {files === 1 ? '1 archivo' : `${files} archivos`}, {size} en esta versión
    </>
  ),
  intoNothing: (folder) => <>{folder} de los recursos, que no contiene nada en esta versión</>,
  packages: 'Paquetes',
  packagesCount: (runtime, tooling) =>
    `${runtime === 1 ? '1 para ejecutar' : `${runtime} para ejecutar`}, ${tooling === 1 ? '1 para compilar' : `${tooling} para compilar`}`,
  showPackages: 'Mostrarlos',
  hidePackages: 'Ocultarlos',
  runtime: 'Para ejecutar',
  tooling: 'Para compilar',
  missing: (page, files) => (
    <>
      {page} hace referencia a {files.length === 1 ? 'un archivo' : `${files.length} archivos`} que no{' '}
      {files.length === 1 ? 'está' : 'están'} entre los recursos ({files.slice(0, 3).join(', ')}{files.length > 3 ? ', …' : ''}): lo que escribió la
      compilación no se guardó completo y la página no carga.
    </>
  ),
  noLock: 'Sin archivo de bloqueo: la próxima compilación puede instalar otras versiones de sus paquetes que la anterior.',
  noIgnore: 'Sin .gitignore: lo que instala y compila su cadena de herramientas puede acabar en una versión.',

  inVersion: (version) => `En la versión ${version}`,
  inDraft: 'En este borrador',
  comparedWith: (version) => `respecto a la versión ${version}`,
  first: 'La primera versión que lo tiene.',
  both: (here, assets) =>
    `${here === 1 ? '1 archivo cambió' : `${here} archivos cambiaron`} aquí, y ${assets === 1 ? '1 archivo' : `${assets} archivos`} de los recursos.`,
  hereOnly: (here) =>
    `${here === 1 ? '1 archivo cambió' : `${here} archivos cambiaron`} aquí, y ninguno de los recursos: a menos que el cambio no necesitara compilación, los visitantes ven lo mismo que antes.`,
  builtOnly: (folder) => (
    <>Cambió lo que se compila en {folder} y nada de aquí: un cambio hecho en lo que escribió la compilación lo deshace la próxima compilación.</>
  ),
  assetsOnly: 'Aquí no cambió nada.',
  unchanged: 'No cambió nada aquí ni en los recursos.',
  showChanges: 'Mostrar los cambios',
  hideChanges: 'Ocultar los cambios',
  noChanges: 'Aquí no cambió nada.',

  readme: 'Cómo se compila',
  noReadme: (code) => (
    <>
      Nada explica cómo se compila. Un {code('README.md')} al principio del espacio de desarrollo, con los comandos y
      el destino de la compilación, es lo que usará el próximo agente.
    </>
  ),
  readOnly: 'Solo lectura: se cambia donde se compila.',
  noFiles: 'Sin archivos.',
};
