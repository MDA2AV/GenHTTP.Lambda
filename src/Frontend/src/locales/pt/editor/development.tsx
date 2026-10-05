import type { EditorMessages } from '../../en/editor';

export const development: EditorMessages['development'] = {
  title: 'Espaço de desenvolvimento',
  hint: 'Aquilo a partir do qual os assets de uma versão são gerados, onde uma toolchain os constrói: o projeto do front-end, com seus códigos-fonte, sua configuração e seu arquivo de lock. Ele é guardado com cada versão e nunca é compilado nem servido. Quem o altera, como o seu agente num clone, faz o build onde trabalha e salva junto com o que foi gerado: esta plataforma não constrói nada. Por isso ele é lido aqui, não editado.',
  overview: 'Visão geral',
  files: 'Arquivos',
  scope: (version) =>
    `Aquilo a partir do qual os assets da versão ${version} são gerados: guardado com ela, nunca compilado nem servido, e construído por quem o altera, nunca aqui.`,
  scopeDraft: 'Aquilo a partir do qual os assets deste rascunho são gerados: guardado com ele, nunca compilado nem servido, e construído por quem o altera, nunca aqui.',
  reading: 'Lendo o espaço de desenvolvimento…',
  readFailed: 'Não foi possível ler o espaço de desenvolvimento.',

  emptyTitle: (version) => `Nenhum espaço de desenvolvimento na versão ${version}`,
  emptyTitleDraft: 'Nenhum espaço de desenvolvimento neste rascunho',
  emptyText: (code) => (
    <>
      Quando um front-end é construído com uma toolchain (React, Vue ou Svelte com Vite, TypeScript, Tailwind), o
      projeto dele fica guardado aqui, com cada versão: aquilo a partir do qual os assets são gerados. O seu agente
      faz o build onde trabalha e salva os códigos-fonte junto com o que foi gerado; num clone, é a pasta {code('dev/')}.
      Um front-end de HTML, CSS e JavaScript simples não precisa de nenhum.
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
  builtWith: 'Construído com',
  build: 'Build',
  noBuild: 'Nenhum script de build no package.json.',
  into: 'Gera em',
  intoAssets: (folder, files, size) => (
    <>
      {folder} dos assets: {files === 1 ? '1 arquivo' : `${files} arquivos`}, {size} nesta versão
    </>
  ),
  intoNothing: (folder) => <>{folder} dos assets, que não tem nada nesta versão</>,
  packages: 'Pacotes',
  packagesCount: (runtime, tooling) =>
    `${runtime === 1 ? '1 para rodar' : `${runtime} para rodar`}, ${tooling === 1 ? '1 para construir' : `${tooling} para construir`}`,
  showPackages: 'Mostrar',
  hidePackages: 'Ocultar',
  runtime: 'Para rodar',
  tooling: 'Para construir',
  missing: (page, files) => (
    <>
      {page} faz referência a {files.length === 1 ? 'um arquivo que não está' : `${files.length} arquivos que não estão`} entre
      os assets ({files.slice(0, 3).join(', ')}{files.length > 3 ? ', …' : ''}): o que o build escreveu não foi salvo
      por inteiro, e a página não carrega.
    </>
  ),
  noLock: 'Nenhum arquivo de lock: o próximo build pode instalar versões dos pacotes diferentes das do último.',
  noIgnore: 'Nenhum .gitignore: o que a toolchain instala e gera pode acabar numa versão.',

  inVersion: (version) => `Na versão ${version}`,
  inDraft: 'Neste rascunho',
  comparedWith: (version) => `em relação à versão ${version}`,
  first: 'A primeira versão que o tem.',
  both: (here, assets) =>
    `${here === 1 ? '1 arquivo alterado' : `${here} arquivos alterados`} aqui, e ${assets === 1 ? '1 arquivo' : `${assets} arquivos`} dos assets.`,
  hereOnly: (here) =>
    `${here === 1 ? '1 arquivo alterado' : `${here} arquivos alterados`} aqui, e nenhum dos assets: a menos que a mudança não precisasse de build, os visitantes veem o mesmo de antes.`,
  builtOnly: (folder) => (
    <>O que é gerado em {folder} mudou, e nada aqui mudou: uma mudança feita no que o build escreveu é desfeita pelo próximo build.</>
  ),
  assetsOnly: 'Nada mudou aqui.',
  unchanged: 'Nada mudou aqui nem nos assets.',
  showChanges: 'Mostrar as mudanças',
  hideChanges: 'Ocultar as mudanças',
  noChanges: 'Nada mudou aqui.',

  readme: 'Como ele é construído',
  noReadme: (code) => (
    <>
      Nada explica como ele é construído. Um {code('README.md')} no topo do espaço de desenvolvimento, com os comandos e
      o destino do build, é o que o próximo agente usa para construí-lo.
    </>
  ),
  readOnly: 'Somente leitura: ele é alterado onde é construído.',
  noFiles: 'Nenhum arquivo.',
};
