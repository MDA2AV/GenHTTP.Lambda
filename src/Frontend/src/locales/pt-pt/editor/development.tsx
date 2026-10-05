import type { EditorMessages } from '../../en/editor';

export const development: EditorMessages['development'] = {
  title: 'Espaço de desenvolvimento',
  hint: 'Aquilo a partir do qual os assets de uma versão são compilados, onde uma toolchain os compila: o projeto do seu front-end, com as fontes, a configuração e o ficheiro de bloqueio. É guardado com cada versão e nunca é compilado nem servido. Quem o altera - o teu agente, num clone - compila-o onde trabalha e guarda-o juntamente com o que compilou: esta plataforma não compila nada. Por isso, aqui lê-se, não se edita.',
  overview: 'Visão geral',
  files: 'Ficheiros',
  scope: (version) =>
    `Aquilo a partir do qual os assets da versão ${version} são compilados - guardado com ela, nunca compilado nem servido, e compilado por quem o altera, nunca aqui.`,
  scopeDraft: 'Aquilo a partir do qual os assets deste rascunho são compilados - guardado com ele, nunca compilado nem servido, e compilado por quem o altera, nunca aqui.',
  reading: 'A ler o espaço de desenvolvimento…',
  readFailed: 'Não foi possível ler o espaço de desenvolvimento.',

  emptyTitle: (version) => `Sem espaço de desenvolvimento na versão ${version}`,
  emptyTitleDraft: 'Sem espaço de desenvolvimento neste rascunho',
  emptyText: (code) => (
    <>
      Quando um front-end é compilado com uma toolchain - React, Vue ou Svelte com Vite, TypeScript, Tailwind - o seu
      projeto fica guardado aqui, com cada versão: aquilo a partir do qual os assets são compilados. O teu agente
      compila-o onde trabalha e guarda as fontes juntamente com o que elas geraram - num clone, é a pasta{' '}
      {code('dev/')}. Um front-end de HTML, CSS e JavaScript simples não precisa de nenhum.
    </>
  ),
  emptyHow: (code) => (
    <>O {code('AGENTS.md')} num clone explica a um agente de programação como configurar um.</>
  ),

  projects: 'Projetos',
  atTheTop: 'o próprio espaço de desenvolvimento',
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
  builtWith: 'Compilado com',
  build: 'Compilar',
  noBuild: 'Sem script de compilação no seu package.json.',
  into: 'Compila para',
  intoAssets: (folder, files, size) => (
    <>
      {folder} dos assets - {files === 1 ? '1 ficheiro' : `${files} ficheiros`}, {size} nesta versão
    </>
  ),
  intoNothing: (folder) => <>{folder} dos assets - que não tem nada nesta versão</>,
  packages: 'Pacotes',
  packagesCount: (runtime, tooling) =>
    `${runtime === 1 ? '1 para executar' : `${runtime} para executar`}, ${tooling === 1 ? '1 para compilar' : `${tooling} para compilar`}`,
  showPackages: 'Mostrar',
  hidePackages: 'Ocultar',
  runtime: 'Para executar',
  tooling: 'Para compilar',
  missing: (page, files) => (
    <>
      {page} refere {files.length === 1 ? 'um ficheiro' : `${files.length} ficheiros`} que não {files.length === 1 ? 'está' : 'estão'}{' '}
      entre os assets ({files.slice(0, 3).join(', ')}{files.length > 3 ? ', …' : ''}): o que a compilação escreveu não
      foi guardado por inteiro, e a página não carrega.
    </>
  ),
  noLock: 'Sem ficheiro de bloqueio: a próxima compilação pode instalar outras versões dos seus pacotes do que a última.',
  noIgnore: 'Sem .gitignore: o que a sua toolchain instala e compila pode acabar numa versão.',

  inVersion: (version) => `Na versão ${version}`,
  inDraft: 'Neste rascunho',
  comparedWith: (version) => `em relação à versão ${version}`,
  first: 'A primeira versão que o tem.',
  both: (here, assets) =>
    `${here === 1 ? '1 ficheiro alterado' : `${here} ficheiros alterados`} aqui e ${assets === 1 ? '1 ficheiro' : `${assets} ficheiros`} dos assets.`,
  hereOnly: (here) =>
    `${here === 1 ? '1 ficheiro alterado' : `${here} ficheiros alterados`} aqui e nenhum dos assets: a menos que a alteração não precisasse de compilação, os visitantes veem o mesmo que antes.`,
  builtOnly: (folder) => (
    <>O que é compilado para {folder} mudou, e nada aqui mudou: uma alteração feita no que a compilação escreveu é desfeita pela próxima compilação.</>
  ),
  assetsOnly: 'Nada mudou aqui.',
  unchanged: 'Nada mudou aqui nem nos assets.',
  showChanges: 'Mostrar as alterações',
  hideChanges: 'Ocultar as alterações',
  noChanges: 'Nada mudou aqui.',

  readme: 'Como é compilado',
  noReadme: (code) => (
    <>
      Nada explica como é compilado. Um {code('README.md')} no topo do espaço de desenvolvimento - os comandos e o
      destino da compilação - é aquilo a partir do qual o próximo agente trabalha.
    </>
  ),
  readOnly: 'Só de leitura: altera-se onde é compilado.',
  noFiles: 'Sem ficheiros.',
};
