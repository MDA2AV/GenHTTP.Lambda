import type { SourceMessages } from '../en/source';

/** Os textos das páginas de código aberto, /source e cada projeto abaixo dela, em português europeu. */
export const source: SourceMessages = {
  shell: {
    section: 'Código aberto',
    home: 'GenHTTP Lambda, a página inicial',
  },

  lambda: {
    label: 'O que é uma lambda?',
    text: 'Uma app web no GenHTTP Lambda: alguém diz o que quer, um agente de IA escreve-a em C#, e em poucos minutos está online num endereço próprio – com todas as versões guardadas e o que cada uma mudou.',
    build: 'Criar a tua app',
  },

  catalog: {
    eyebrow: 'Código aberto',
    title: 'Vê como são feitas as apps daqui',
    intro:
      'Lambdas cujos donos publicaram o código: todas as versões, o que cada uma mudou, a documentação e os testes. Lê-o aqui, ou transfere um projeto que corre em qualquer sítio onde o .NET corra.',
    searchLabel: 'Pesquisar nos projetos',
    searchPlaceholder: 'Pesquisa pelo nome ou pelo que faz',
    orderLabel: 'Ordenar por',
    orders: {
      stars: 'Mais estrelas',
      updated: 'Alterados recentemente',
      published: 'Publicados recentemente',
    },
    counted: (total) => (total === 1 ? '1 projeto' : `${total} projetos`),
    failed: 'Não foi possível carregar os projetos.',
    loadingMore: 'A carregar mais…',
    showMore: 'Mostrar mais',
    nothingTitle: 'Ainda não há nada publicado',
    nothing: (tab) => (
      <>
        Criaste algo com que outros possam aprender? Abre o painel de controlo, escolhe {tab('Código aberto')},
        seleciona uma licença, e o código aparece aqui.
      </>
    ),
    noMatchTitle: 'Nenhum resultado',
    noMatch: (query) => `Nenhum projeto publicado menciona «${query}».`,
    clear: 'Mostrar todos os projetos',
    yoursTitle: 'Publica o teu código',
    yours: (tab) => (
      <>
        Abre o painel de controlo da tua lambda e escolhe {tab('Código aberto')}, ou pede ao agente que a criou para a
        publicar. Só quem tem a chave de edição o pode fazer, com a licença que escolher – e o que a app guarda, os
        registos, os ficheiros e as chaves dela, nunca faz parte disso.
      </>
    ),
    build: 'Criar algo',
    online: 'Online',
    offline: 'Offline',
    changed: (ago) => `alterado ${ago}`,
    stars: (count) => (count === 1 ? '1 estrela' : `${count} estrelas`),
  },

  project: {
    loading: 'A carregar o código…',
    failed: 'Não foi possível carregar o código.',
    missingTitle: 'Não há código publicado aqui',
    missing: 'O dono pode ter retirado a publicação, ou nunca existiu uma lambda neste endereço.',
    all: 'Todos os projetos',
    by: (name) => `por ${name}`,
    versions: (count) => (count === 1 ? '1 versão' : `${count} versões`),
    onlineAt: (address) => <>Online em {address}</>,
    offline: 'Offline neste momento',
    openApp: 'Abrir a app',
    opens: (address) => `Abre ${address} num novo separador`,
    published: (ago) => `Publicado ${ago}`,
    changed: (ago) => `Alterado ${ago}`,
    picture: (name) => `${name}: captura de ecrã`,
    tabsLabel: 'O que ler',
    tabs: {
      code: 'Código',
      docs: 'Documentação',
      tests: 'Testes',
      changes: 'Alterações',
    },
  },

  versions: {
    label: 'Versão',
    choose: 'Ler outra versão',
    newest: 'mais recente',
    online: 'online',
    older: (version, ago, newest) =>
      `Estás a ler a versão ${version}, guardada ${ago}. A mais recente é a versão ${newest}.`,
    toNewest: 'Ler a mais recente',
    noChange: 'Sem nota sobre o que mudou',
  },

  star: {
    star: 'Estrela',
    add: 'Dar uma estrela a este projeto',
    remove: 'Retirar a tua estrela',
    count: (count) => (count === 1 ? '1 estrela' : `${count} estrelas`),
    failed: 'Não foi possível guardar a estrela.',
  },
  clone: {
    button: 'Código',
    title: 'Clonar com git',
    what: (oldest, newest) =>
      oldest === newest
        ? `A sua versão é o commit de main, com a etiqueta v${newest}.`
        : `Cada versão vem como um commit de main, com as etiquetas v${oldest} a v${newest} - main é a mais recente.`,
    readOnly:
      'Só de leitura. Para construir a partir dela, começa uma lambda tua e leva estes ficheiros para lá - o AGENTS.md no clone explica como, e a licença o que podes fazer.',
  },

  download: {
    title: (version) => `Versão ${version} como projeto`,
    what:
      'Um projeto .NET 10 com um Dockerfile, a documentação, os testes e a licença. O que a app guarda (os registos, os ficheiros que guardou, as chaves) não faz parte dele.',
    zip: 'Transferir ZIP',
    preparing: 'A preparar o projeto…',
    slow: 'Da primeira vez que uma versão é transferida, é empacotada enquanto esperas.',
    failed: 'Não foi possível preparar o projeto. Tenta outra vez daqui a pouco.',
    run: 'Correr o projeto',
    local: 'Com o SDK do .NET 10:',
    container: 'Ou num contentor:',
    agent: 'Ou entrega a pasta ao teu agente de programação e continua a partir dela – respeitando a licença.',
    copy: 'Copiar',
    copied: 'Copiado',
  },

  tree: {
    label: 'Ficheiros',
    files: (count) => (count === 1 ? '1 ficheiro' : `${count} ficheiros`),
    packing: 'A empacotar esta versão…',
    packingSlow: 'Uma versão é empacotada da primeira vez que alguém a lê, o que demora um pouco se for grande.',
    failed: 'Não foi possível carregar os ficheiros desta versão.',
    legend: 'O que é o quê',
    kinds: {
      code: 'O código da própria lambda',
      asset: 'O que ela serve: páginas, scripts, estilos, imagens – e as migrações da base de dados',
      docs: 'O que é, e porque está construída desta forma',
      tests: 'Como é testada',
      dev: 'Aquilo a partir do qual os assets são compilados: o projeto do seu front-end',
      platform: 'O que substitui a plataforma',
      project: 'O host, o build, o contentor e a licença',
    },
    short: {
      code: 'Código',
      asset: 'Servido',
      docs: 'Docs',
      tests: 'Testes',
      dev: 'Dev',
      platform: 'Plataforma',
      project: 'Projeto',
    },
  },

  file: {
    loading: 'A carregar…',
    failed: 'Não foi possível carregar este ficheiro.',
    missing: (path) => `Não existe ${path} nesta versão.`,
    binary: 'Este ficheiro não é texto.',
    tooLarge: 'Este ficheiro é demasiado longo para mostrar aqui.',
    download: 'Transferir',
    raw: 'Original',
    rawTitle: 'Abrir o ficheiro tal como está',
    copy: 'Copiar',
    copied: 'Copiado',
    lines: (count) => (count === 1 ? '1 linha' : `${count} linhas`),
    plain: 'Mostrado sem cores, porque é longo.',
    line: (line) => `Linha ${line}`,
  },

  docs: {
    pages: 'Páginas',
    product: 'O que é',
    decisions: 'Decisões',
    loading: 'A carregar…',
    failed: 'Não foi possível carregar esta página.',
    noneTitle: 'Não há nada escrito sobre esta versão',
    none: 'A documentação estaria em docs/: o que é a app, para quem é e porque está construída desta forma.',
  },

  tests: {
    files: 'Scripts e dados',
    noneTitle: 'Esta versão não diz nada sobre os seus testes',
    none: 'Como é testada estaria em tests/README.md, com os scripts que corre ao lado.',
  },

  changes: {
    title: 'Todas as versões, da mais recente para a mais antiga',
    intro: 'Uma versão nunca muda depois de guardada. Cada uma diz numa linha o que mudou.',
    agent: 'Escrito por um agente',
    online: 'online',
    browse: 'Ler o código',
    noChange: 'Sem nota',
  },

  licenses: {
    MIT: 'Qualquer pessoa o pode usar, alterar e redistribuir, em qualquer coisa, desde que a licença e o aviso de copyright o acompanhem.',
    'Apache-2.0': 'Como a MIT, com uma licença de patentes de todos os que contribuíram, e as alterações assinaladas como tal.',
    'BSD-3-Clause': 'Como a MIT, e ninguém pode usar o nome dos autores para promover o que fez a partir do código.',
    'MPL-2.0': 'As alterações a estes ficheiros ficam sob a mesma licença; podem ser combinados com código sob qualquer outra.',
    'GPL-3.0-or-later': 'Quem o redistribuir, alterado ou não, redistribui também o código-fonte sob a mesma licença.',
    'AGPL-3.0-or-later': 'Como a GPL, e disponibilizar uma cópia alterada a pessoas através da rede conta como redistribuí-la.',
    Unlicense: 'Cedido ao domínio público: qualquer pessoa pode fazer o que quiser com ele, sem condições.',
  },

  kinds: {
    Permissive: 'Permissiva',
    Copyleft: 'Copyleft',
    PublicDomain: 'Domínio público',
  },
};
