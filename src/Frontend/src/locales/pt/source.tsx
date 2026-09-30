import type { SourceMessages } from '../en/source';

/** Os textos das páginas de código aberto, /source e cada projeto abaixo dela, em português. */
export const source: SourceMessages = {
  shell: {
    section: 'Código aberto',
    home: 'GenHTTP Lambda, a página inicial',
  },

  lambda: {
    label: 'O que é uma lambda?',
    text: 'Um app web no GenHTTP Lambda: alguém diz o que quer, um agente de IA escreve o app em C#, e em poucos minutos ele está no ar num endereço próprio – com todas as versões guardadas e o que cada uma mudou.',
    build: 'Criar o seu app',
  },

  catalog: {
    eyebrow: 'Código aberto',
    title: 'Veja como os apps daqui são feitos',
    intro:
      'Lambdas cujos donos publicaram o código: todas as versões, o que cada uma mudou, a documentação e os testes. Leia aqui ou baixe um projeto que roda em qualquer lugar onde o .NET roda.',
    searchLabel: 'Buscar nos projetos',
    searchPlaceholder: 'Busque pelo nome ou pelo que ele faz',
    orderLabel: 'Ordenar por',
    orders: {
      stars: 'Mais estrelas',
      updated: 'Alterados recentemente',
      published: 'Publicados recentemente',
    },
    counted: (total) => (total === 1 ? '1 projeto' : `${total} projetos`),
    failed: 'Não foi possível carregar os projetos.',
    loadingMore: 'Carregando mais…',
    showMore: 'Mostrar mais',
    nothingTitle: 'Nada publicado ainda',
    nothing: (tab) => (
      <>
        Criou algo com que outras pessoas podem aprender? Abra o painel de controle, escolha {tab('Código aberto')},
        selecione uma licença, e o código aparece aqui.
      </>
    ),
    noMatchTitle: 'Nenhum resultado',
    noMatch: (query) => `Nenhum projeto publicado menciona “${query}”.`,
    clear: 'Mostrar todos os projetos',
    yoursTitle: 'Publique o seu código',
    yours: (tab) => (
      <>
        Abra o painel de controle da sua lambda e escolha {tab('Código aberto')}, ou peça ao agente que a criou para
        publicá-la. Só quem tem a chave de edição pode fazer isso, com a licença que escolher – e o que o app guarda, os
        registros, os arquivos e as chaves dele, nunca faz parte disso.
      </>
    ),
    build: 'Criar algo',
    online: 'No ar',
    offline: 'Fora do ar',
    changed: (ago) => `alterado ${ago}`,
    stars: (count) => (count === 1 ? '1 estrela' : `${count} estrelas`),
  },

  project: {
    loading: 'Carregando o código…',
    failed: 'Não foi possível carregar o código.',
    missingTitle: 'Não há código publicado aqui',
    missing: 'Talvez o dono tenha retirado a publicação, ou nunca existiu uma lambda neste endereço.',
    all: 'Todos os projetos',
    by: (name) => `por ${name}`,
    versions: (count) => (count === 1 ? '1 versão' : `${count} versões`),
    onlineAt: (address) => <>No ar em {address}</>,
    offline: 'Fora do ar no momento',
    openApp: 'Abrir o app',
    opens: (address) => `Abre ${address} em uma nova aba`,
    published: (ago) => `Publicado ${ago}`,
    changed: (ago) => `Alterado ${ago}`,
    picture: (name) => `${name}: captura de tela`,
    tabsLabel: 'O que ler',
    tabs: {
      code: 'Código',
      docs: 'Documentação',
      tests: 'Testes',
      changes: 'Mudanças',
    },
  },

  versions: {
    label: 'Versão',
    choose: 'Ler outra versão',
    newest: 'mais recente',
    online: 'no ar',
    older: (version, ago, newest) =>
      `Você está lendo a versão ${version}, salva ${ago}. A mais recente é a versão ${newest}.`,
    toNewest: 'Ler a mais recente',
    noChange: 'Sem nota sobre o que mudou',
  },

  star: {
    star: 'Estrela',
    add: 'Dar uma estrela a este projeto',
    remove: 'Retirar a sua estrela',
    count: (count) => (count === 1 ? '1 estrela' : `${count} estrelas`),
    failed: 'Não foi possível salvar a estrela.',
  },

  download: {
    button: 'Baixar',
    title: (version) => `Versão ${version} como projeto`,
    what:
      'Um projeto .NET 10 com Dockerfile, a documentação, os testes e a licença. O que o app guarda (os registros, os arquivos que ele salvou, as chaves) não faz parte dele.',
    zip: 'Baixar ZIP',
    preparing: 'Preparando o projeto…',
    slow: 'Na primeira vez que uma versão é baixada, ela é empacotada enquanto você espera.',
    failed: 'Não foi possível preparar o projeto. Tente de novo daqui a pouco.',
    run: 'Rodar o projeto',
    local: 'Com o SDK do .NET 10:',
    container: 'Ou num contêiner:',
    agent: 'Ou entregue a pasta ao seu agente de programação e continue a partir dela – respeitando a licença.',
    copy: 'Copiar',
    copied: 'Copiado',
  },

  tree: {
    label: 'Arquivos',
    files: (count) => (count === 1 ? '1 arquivo' : `${count} arquivos`),
    packing: 'Empacotando esta versão…',
    packingSlow: 'Uma versão é empacotada na primeira vez que alguém a lê, o que leva um momento se ela for grande.',
    failed: 'Não foi possível carregar os arquivos desta versão.',
    legend: 'O que é o quê',
    kinds: {
      code: 'O código da própria lambda',
      asset: 'O que ela serve: páginas, scripts, estilos, imagens – e as migrações do banco de dados',
      docs: 'O que ela é, e por que é construída desse jeito',
      tests: 'Como ela é testada',
      platform: 'O que substitui a plataforma',
      project: 'O host, o build, o contêiner e a licença',
    },
    short: {
      code: 'Código',
      asset: 'Servido',
      docs: 'Docs',
      tests: 'Testes',
      platform: 'Plataforma',
      project: 'Projeto',
    },
  },

  file: {
    loading: 'Carregando…',
    failed: 'Não foi possível carregar este arquivo.',
    missing: (path) => `Não existe ${path} nesta versão.`,
    binary: 'Este arquivo não é texto.',
    tooLarge: 'Este arquivo é longo demais para mostrar aqui.',
    download: 'Baixar',
    raw: 'Original',
    rawTitle: 'Abrir o arquivo como ele é',
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
    loading: 'Carregando…',
    failed: 'Não foi possível carregar esta página.',
    noneTitle: 'Não há nada escrito sobre esta versão',
    none: 'A documentação estaria em docs/: o que é o app, para quem é e por que ele é construído desse jeito.',
  },

  tests: {
    files: 'Scripts e dados',
    noneTitle: 'Esta versão não diz nada sobre os testes',
    none: 'Como ela é testada estaria em tests/README.md, com os scripts que ela roda ao lado.',
  },

  changes: {
    title: 'Todas as versões, da mais recente para a mais antiga',
    intro: 'Uma versão nunca muda depois de salva. Cada uma diz numa linha o que mudou.',
    agent: 'Escrito por um agente',
    online: 'no ar',
    browse: 'Ler o código',
    noChange: 'Sem nota',
  },

  licenses: {
    MIT: 'Qualquer pessoa pode usar, modificar e redistribuir, em qualquer coisa, desde que a licença e o aviso de copyright acompanhem o código.',
    'Apache-2.0': 'Como a MIT, com uma licença de patentes de todos que contribuíram, e as mudanças marcadas como mudanças.',
    'BSD-3-Clause': 'Como a MIT, e ninguém pode usar o nome dos autores para promover o que fez a partir do código.',
    'MPL-2.0': 'As mudanças nestes arquivos continuam sob a mesma licença; eles podem ser combinados com código sob qualquer outra.',
    'GPL-3.0-or-later': 'Quem repassar o código, modificado ou não, repassa também o código-fonte sob a mesma licença.',
    'AGPL-3.0-or-later': 'Como a GPL, e oferecer uma cópia modificada a pessoas pela rede conta como repassá-la.',
    Unlicense: 'Entregue ao domínio público: qualquer pessoa pode fazer o que quiser com ele, sem condições.',
  },

  kinds: {
    Permissive: 'Permissiva',
    Copyleft: 'Copyleft',
    PublicDomain: 'Domínio público',
  },
};
