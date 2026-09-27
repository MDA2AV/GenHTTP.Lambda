import type { EditorMessages } from '../en/editor';

/** Os textos do editor em português. */
export const editor: EditorMessages = {
  shared: {
    units: { s: 's', min: 'min', h: 'h', d: 'd' },
    never: 'nunca',
    justNow: 'agora mesmo',
    ago: (span) => `há ${span}`,
    in: (span) => `em ${span}`,
    origins: {
      agent: 'agente',
      template: 'modelo',
      admin: 'operador',
      system: 'plataforma',
      api: 'API / editor',
      unknown: 'desconhecido',
    },
    endings: {
      replaced: 'substituído por uma implantação mais recente',
      stopped: 'retirado do ar',
      expired: 'expirado por falta de uso',
      admin: 'retirado do ar pelo operador',
      ended: 'encerrado',
    },
    whatThisIs: 'Explicação',
    byAgent: 'por um agente',
    writtenByAgent: 'Escrito por um agente',
    more: 'Mais',
    of: (used, total) => `${used} de ${total}`,
    online: (version) => `No ar · v${version}`,
    onlineTitle: (version) => `No ar, servindo a versão ${version}`,
    offline: 'Fora do ar',
    offlineTitle: 'Fora do ar: nada está sendo servido',
    premium:
      'Premium: pode responder em um domínio próprio, tem mais espaço para código, recursos e dados e permanece no ar independentemente do uso',
    demo: 'Demo: mantida no ar por esta instalação e somente leitura',
    tier: (tier) => `Plano ${tier}`,
    entrances: {
      title: 'Acessado por',
      note: 'Desde a inicialização do servidor, incluindo conexões WebSocket.',
    },
    chart: {
      showChart: 'Mostrar gráfico',
      showValues: 'Mostrar valores',
      none: 'Ainda não há medições.',
      time: 'Hora',
    },
    diagnostics: {
      compiles: 'O código compila.',
      none: 'Ainda não há mensagens. Verifique ou implante o código para compilá-lo.',
      line: (line) => `linha ${line}`,
    },
  },

  frame: {
    title: 'Editor',
    sections: {
      overview: 'Visão geral',
      showcase: 'Vitrine',
      domain: 'Domínio',
      files: 'Arquivos',
      versions: 'Versões',
      deployments: 'Implantações',
      stats: 'Estatísticas',
      logs: 'Logs',
      code: 'Código',
    },
    sectionsLabel: 'Seções',
    loadFailed: 'Não foi possível carregar este lambda.',
    online: (version) => `A versão ${version} está no ar.`,
    deployFailed: 'Não foi possível implantar o lambda.',
    offline: 'Retirado do ar. O código foi mantido.',
    offlineFailed: 'Não foi possível retirar o lambda do ar.',
    leave: 'As alterações não salvas no código serão perdidas. Deseja sair mesmo assim?',
    nothingTitle: 'Este link não abre nenhum lambda',
    createNew: 'Criar um novo lambda',
    loading: 'Carregando seu lambda…',
    moreActions: 'Mais ações',
    redeploy: (version) => `Reimplantar a versão ${version}`,
    takeOffline: 'Retirar do ar',
    copyLink: 'Copiar o link',
    copyPrivate: 'Copiar o link privado',
    privateLink: 'Qualquer pessoa com este link pode alterar o lambda. Mantenha-o em sigilo.',
    rename: 'Alterar o endereço',
    download: 'Baixar como projeto .NET',
    delete: 'Excluir este lambda',
    deploy: (version) => `Implantar a versão ${version}`,
    problems: 'Ocorreram erros recentemente',
    demoTitle: 'Uma demo, mantida no ar por esta instalação e somente leitura.',
    demo: (start) => (
      <>
        Seu código, histórico, dados e logs estão disponíveis para consulta. Para alterá-la,{' '}
        {start('crie um lambda próprio a partir dela')}.
      </>
    ),
    keep: 'Guarde este link. Ele é o único acesso a este lambda.',
    gotIt: 'Entendi',
    rejected: (version) => `A versão ${version} não foi colocada no ar`,
    refused: 'A implantação foi recusada',
    openCode: 'Abrir o código',
    close: 'Fechar',
    notCompiling: 'O código não compila. A versão que estava no ar continua no ar.',
    moved: (path) => `Agora disponível em ${path}.`,
    deleteTitle: 'Excluir este lambda?',
    cancel: 'Cancelar',
    deleteForGood: 'Excluir definitivamente',
    deleteFailed: 'Não foi possível excluir o lambda.',
    deleteText: (key) => (
      <>Todas as versões, arquivos, o histórico e o endereço {key} serão excluídos. Esta ação não pode ser desfeita.</>
    ),
    openInTab: 'Abrir em uma nova aba',
    open: (address) => `Abrir ${address} em uma nova aba`,
    copyAddress: 'Copiar o endereço',
    renameFailed: 'Não foi possível alterar o endereço.',
    moveIt: 'Alterar o endereço',
    renameText: 'O endereço anterior deixa de funcionar imediatamente; atualize todos os links que apontam para ele.',
  },

  summary: {
    reading: 'Consultando o estado…',
    hint: (since, kept, retention, tier) =>
      `O tráfego é contabilizado desde a última inicialização do servidor (${since}). ` +
      (kept
        ? `Um lambda permanece no ar enquanto é utilizado e é removido após ${retention} dias sem acessos nem alterações.`
        : `Este lambda está no plano ${tier}, que o mantém no ar e armazenado independentemente do uso.`),
    onlineFor: (duration, version) => (
      <>
        No ar há {duration('algum tempo')}, servindo a versão {version}.
      </>
    ),
    offline: 'Fora do ar. Nada é servido até que uma versão seja implantada.',
    nothing: 'Nada foi escrito ainda.',
    requestsToday: 'requisições hoje',
    lastHour: (count) => `${count} na última hora`,
    hourly: 'Requisições por hora nas últimas 24 horas',
    failed: 'com falha',
    failedTitle: (failed, rejected) =>
      `${failed} erros de servidor, ${rejected} não encontradas ou recusadas, nas últimas 24 horas`,
    average: 'tempo médio de resposta',
    noneYet: 'nenhuma ainda',
    lastVisit: 'último acesso',
    problems: 'Erros recentes',
    openLog: 'Abrir o log',
    latest: 'Última alteração',
    allVersions: 'Todas as versões',
    noDescription: 'Sem descrição',
    version: (version) => `Versão ${version}`,
    notOnline: 'ainda não está no ar',
    wanted: 'O que foi solicitado',
    noVersions: 'Ainda não há versões.',
    storage: 'Armazenamento',
    browse: 'Explorar',
    code: 'Código',
    codeWhy: 'O C# é compilado e nunca servido.',
    characters: 'caracteres',
    assets: 'Recursos',
    assetsPublic: 'Público: o código os serve.',
    assetsPrivate: 'Não servidos pelo código.',
    data: 'Dados',
    dataPublic: 'Público: o código serve o workspace.',
    dataPrivate: 'Privados do lambda.',
  },

  files: {
    hint: (b) => (
      <>
        O {b('Código')} é compilado e nunca servido. Os {b('Recursos')} – páginas, estilos, imagens – são salvos com cada
        versão e são públicos se o código os servir. Os {b('Dados')} são o que o lambda grava durante a execução; não fazem
        parte de nenhuma versão e só são públicos se o código os servir.
      </>
    ),
    edit: 'Editar esta versão',
    version: 'Versão',
    shown: (version, online, newest) => `Versão ${version}${online ? ', no ar' : newest ? ', mais recente' : ''}`,
    optionOnline: ' (no ar)',
    readFailed: 'Não foi possível ler essa versão.',
    dataFailed: 'Não foi possível ler os dados.',
    noVersion: 'Ainda não há versão para exibir.',
    label: 'Arquivos',
    code: 'Código',
    codeWhy: 'Compilado no lambda, nunca servido.',
    count: (files) => (files === 1 ? '1 arquivo' : `${files} arquivos`),
    codeUsage: (files, used, of) => `${files}, ${used} de ${of} caracteres`,
    usage: (files, used, of) => `${files}, ${used} de ${of}`,
    noCode: 'Esta versão não contém código.',
    assets: 'Recursos',
    assetsPublic: 'Público: esta versão os serve com Assets.',
    assetsPrivate: 'Salvos com o código, mas esta versão não os serve.',
    noAssets: 'Nenhum nesta versão.',
    data: 'Dados',
    dataPublic: 'Público: esta versão os serve com Workspace.',
    dataPrivate: 'Privados do lambda. Não fazem parte de nenhuma versão.',
    uploadFailed: (path) => `Não foi possível enviar ${path}.`,
    deleteFolder: (path, held) =>
      held > 0
        ? `Excluir ${path} e ${held === 1 ? 'o arquivo que contém' : `os ${held} arquivos que contém`}?`
        : `Excluir a pasta ${path}?`,
    deleteFile: (path) => `Excluir ${path}? O lambda não poderá mais encontrá-lo.`,
    deleteFailed: 'Não foi possível excluir.',
    full: 'O espaço de dados está cheio',
    uploadInto: (folder) => `Enviar para ${folder}`,
    upload: 'Enviar',
    reading: 'Lendo…',
    noData: 'Ainda não há dados. O que o lambda salvar durante a execução aparecerá aqui.',
    delete: (path) => `Excluir ${path}`,
    deleteShort: 'Excluir',
    fileFailed: 'Não foi possível ler o arquivo.',
    pick: 'Selecione um arquivo para ver seu conteúdo.',
    tooLarge: (name, size) => (
      <>
        {name} tem {size}, grande demais para ser exibido aqui.
      </>
    ),
    download: 'Baixar',
    readingFile: (name) => `Lendo ${name}…`,
    missing: (name) => `Esta versão não contém nenhum arquivo chamado ${name}.`,
    saved: 'salvo',
    notText: 'Não é um arquivo de texto. Baixe-o para ver o conteúdo.',
  },

  versions: {
    hint: (limit) =>
      `Cada versão guarda o que foi solicitado e o que mudou, quando o autor informou. As mais antigas são removidas quando há mais de ${limit}; a versão no ar nunca é removida.`,
    none: 'Ainda não há versões.',
    noDescription: 'Sem descrição',
    online: 'no ar',
    putOnline: 'Colocar esta versão no ar',
    rollBackTitle: 'Colocar esta versão anterior de volta no ar',
    deploy: 'Implantar',
    rollBack: 'Reverter',
    readFailed: 'Não foi possível ler esta versão.',
    comparing: 'Comparando…',
    unchanged: 'Nenhuma alteração em relação à versão anterior.',
    first: 'A primeira versão.',
    status: { added: 'adicionado', removed: 'removido', changed: 'alterado', same: 'inalterado' },
    browse: 'Explorar seus arquivos',
    edit: 'Editar a partir daqui',
    binary: 'Não é um arquivo de texto, portanto não há linhas para comparar.',
    tooLarge: 'Grande demais para comparar linha a linha.',
  },

  deployments: {
    hint: (until) =>
      `Uma implantação permanece no ar enquanto é utilizada${until ? ` – sem uso, até ${until}` : ''}. Uma nova implantação ou qualquer acesso reinicia esse prazo.`,
    takeOffline: 'Retirar do ar',
    readFailed: 'Não foi possível ler o histórico.',
    reading: 'Lendo o histórico…',
    none: 'Nada foi implantado ainda.',
    noDescription: 'Sem descrição',
    deployed: (when, by) => `Implantado em ${when} por ${by}`,
    duration: 'Tempo no ar',
    online: 'no ar',
    short: {
      replaced: 'substituído',
      stopped: 'retirado do ar',
      expired: 'expirado',
      admin: 'pelo operador',
      ended: 'encerrado',
    },
    putBack: (version) => `Colocar a versão ${version} de volta no ar`,
    timeline: 'O que esteve no ar nos últimos sete dias',
    block: (version, from, to) => `Versão ${version}, de ${from} ${to ? `até ${to}` : 'até agora'}`,
    weekAgo: 'há uma semana',
    now: 'agora',
  },

  stats: {
    readFailed: 'Não foi possível ler os números.',
    range: 'Período',
    lastHour: 'Última hora',
    lastDay: 'Últimas 24 horas',
    hint: (since) =>
      `Contabilizado em memória desde a última inicialização do servidor (${since}). Uma reinicialização zera estes números.`,
    reading: 'Lendo os números…',
    requests: 'requisições',
    websockets: (count) => `e ${count} conexões WebSocket`,
    failed: 'com falha',
    serverErrors: (count) => `${count} erros de servidor`,
    rejected: 'não encontradas ou recusadas',
    average: 'tempo médio de resposta',
    sent: (amount) => `${amount} enviados`,
    nobody: (hour) => (hour ? 'Nenhuma requisição na última hora.' : 'Nenhuma requisição nas últimas 24 horas.'),
    requestsTitle: 'Requisições',
    per: (hour) => (hour ? 'Por minuto.' : 'Por intervalo de 15 minutos.'),
    answered: 'Respondidas',
    rejectedSeries: 'Não encontradas ou recusadas',
    failedSeries: 'Com falha',
    timeTitle: 'Tempo de resposta',
    averagePer: (hour) => (hour ? 'Média por minuto.' : 'Média por intervalo de 15 minutos.'),
    averageSeries: 'Média',
    mostAsked: 'Caminhos mais acessados',
    path: 'Caminho',
    requestsColumn: 'Requisições',
    failedColumn: 'Falhas',
    averageColumn: 'Média',
    since: 'Desde a inicialização do servidor.',
  },

  logs: {
    readFailed: 'Não foi possível ler o log.',
    hint: (capturing) =>
      'Requisições, o que o lambda imprimiu e o que deu errado, em tempo real.' +
      (capturing ? '' : ' Esta instalação não guarda o que os lambdas imprimem, portanto aparecem apenas requisições e erros.') +
      ' O log é mantido em memória e compartilhado com todos os lambdas desta instalação, por isso abrange de minutos a horas e fica vazio após uma reinicialização. Os endereços dos visitantes não são exibidos.',
    search: 'Pesquisar',
    searchLabel: 'Pesquisar no log',
    resume: 'Mostrar novas linhas à medida que chegam',
    pause: 'Pausar novas linhas durante a leitura',
    paused: 'Pausado',
    live: 'Ao vivo',
    show: 'Exibir',
    all: 'Tudo',
    requests: 'Requisições',
    output: 'Saída',
    problems: 'Erros',
    reading: 'Lendo o log…',
    noProblems: 'O log atual não registra erros.',
    nothing: 'Ainda não há entradas. Abra o endereço do lambda e as requisições aparecerão aqui.',
    noMatch: 'Nenhum resultado.',
    identical: (count) => `${count} linhas idênticas`,
    at: (domain) => `, em ${domain}`,
    from: (country) => `, de ${country}`,
  },

  showcase: {
    loadFailed: 'Não foi possível carregar a entrada da vitrine.',
    loading: 'Carregando…',
    title: 'um título',
    description: 'uma descrição',
    picture: 'uma imagem',
    updated: 'A entrada da vitrine foi atualizada.',
    listed: 'A aplicação já aparece na vitrine.',
    waiting: 'Salvo. A entrada aparecerá na vitrine assim que o lambda estiver no ar.',
    saveFailed: 'Não foi possível salvar a entrada da vitrine.',
    removed: 'Removido da vitrine.',
    removeFailed: 'Não foi possível remover a entrada da vitrine.',
    wrongType: 'Não é uma imagem PNG, JPEG, GIF ou WebP.',
    tooLarge: (size, limit) => `O arquivo tem ${size}; o máximo permitido é ${limit}.`,
    unreadable: 'Não foi possível ler o arquivo.',
    hint: (tool) => (
      <>
        A vitrine exibe os lambdas que seus proprietários decidiram mostrar, primeiro os mais utilizados recentemente.
        Somente quem tem a chave de edição pode adicionar ou remover um lambda, que só aparece enquanto estiver no ar. Um
        agente pode fazer o mesmo com a ferramenta {tool}.
      </>
    ),
    open: 'Abrir a vitrine',
    switch: 'Exibir este lambda na vitrine',
    listedNow: 'Em exibição. Qualquer pessoa que consulte a vitrine pode abri-lo.',
    notListed: 'Salvo, mas não exibido: o lambda está fora do ar. Ele reaparecerá após a próxima implantação.',
    off: 'Desativado. Este lambda não é exibido em lugar nenhum até que você ative esta opção e salve.',
    offline: 'O lambda está fora do ar, portanto a entrada aguardará a implantação. Somente lambdas que respondem são exibidos.',
    titleLabel: 'Título',
    titlePlaceholder: 'Placar da noite de perguntas',
    descriptionLabel: 'Descrição',
    descriptionPlaceholder:
      'As equipes enviam as respostas pelo celular, o apresentador as corrige e o placar é atualizado para todos na sala.',
    save: 'Salvar alterações',
    add: 'Adicionar à vitrine',
    takeOff: 'Remover',
    needs: (missing) =>
      `Ainda falta ${missing.length > 1 ? `${missing.slice(0, -1).join(', ')} e ${missing[missing.length - 1]}` : missing[0]}.`,
    tooLong: 'Alguns campos estão longos demais.',
    allSaved: 'Tudo está salvo.',
    preview: 'Pré-visualização',
    card: (address) => <>Este é o cartão que os visitantes veem. Ele abre {address}.</>,
    confirm: 'Remover da vitrine?',
    keep: 'Manter',
    confirmText: 'O título, a descrição e a imagem serão excluídos. O lambda em si permanece inalterado.',
    pictureLabel: 'Imagem',
    formats: (limit) => `PNG, JPEG, GIF ou WebP, até ${limit}`,
    notSaved: 'ainda não salva',
    replace: 'Arraste uma nova imagem para cá para substituí-la.',
    drop: 'Arraste uma imagem para cá.',
    advice: 'O ideal é uma captura de tela ou um GIF curto da aplicação em uso, no formato 16:10.',
    another: 'Escolher outra',
    choose: 'Escolher um arquivo',
    keepSaved: 'Manter a imagem salva',
    clear: 'Limpar',
  },

  domain: {
    readFailed: 'Não foi possível ler o domínio.',
    reaching: (domain) => `As requisições para ${domain} agora chegam a este lambda.`,
    saveFailed: 'Não foi possível salvar o domínio.',
    removed: 'O domínio foi removido. O lambda continua respondendo em seu endereço nesta plataforma.',
    removeFailed: 'Não foi possível remover o domínio.',
    hint:
      'Um lambda Premium pode responder em um domínio próprio – por inteiro, a partir da raiz – além do seu endereço nesta plataforma. Aponte o domínio para este servidor, informe-o aqui, e as requisições para ele chegarão ao lambda.',
    loading: 'Carregando…',
    example: 'seu-dominio.com.br',
    open: (domain) => `Abrir ${domain}`,
    label: 'Domínio em que responde',
    serving: (domain) => <>Servindo {domain} no momento, além do seu endereço nesta plataforma.</>,
    none: 'Nenhum ainda. Um subdomínio como shop.example.com ou um domínio completo como example.com.',
    change: 'Alterar',
    use: 'Usar este domínio',
    remove: 'Remover',
    confirm: 'Remover o domínio?',
    keep: 'Manter',
    confirmText: (domain) => (
      <>
        As requisições para {domain} deixam de chegar a este lambda imediatamente. O endereço nesta plataforma permanece o
        mesmo, assim como a configuração de DNS do domínio.
      </>
    ),
    point: 'Apontar o domínio para este servidor',
    check: 'Verificar novamente',
    records:
      'No provedor que gerencia o DNS do domínio, adicione estes dois registros. O registro AAAA pode ser omitido se você preferir que o domínio não seja acessível via IPv6.',
    type: 'Tipo',
    name: 'Nome',
    value: 'Valor',
    pointsHere: (domain) => <>{domain} aponta para este servidor.</>,
    alsoElsewhere: (addresses) =>
      ` Ele também é resolvido para ${addresses}, que não é este servidor – visitantes direcionados para lá não chegarão ao lambda.`,
    elsewhere: (addresses) => `Ele é resolvido para ${addresses}, que ainda não é este servidor.`,
    wait: 'Uma alteração pode levar algum tempo para ser vista em todos os lugares – até o tempo de vida (TTL) do registro anterior.',
    cname: 'Usar um registro CNAME em vez disso',
    cnameText: (target) => (
      <>
        Um subdomínio pode apontar para {target} com um registro CNAME e, assim, acompanhar este servidor caso seus
        endereços mudem. Essa opção tem desvantagens:
      </>
    ),
    cnameRoot: (example) => (
      <>
        Não pode ser usada para um domínio completo ({example} em si): o padrão não permite um CNAME junto aos registros que
        todo domínio tem na raiz. Alguns provedores oferecem para isso um registro ALIAS, ANAME ou “achatado”.
      </>
    ),
    cnameAlone: 'Nenhum outro registro pode existir com o mesmo nome – nem MX para e-mail, nem TXT para verificações.',
    cnameLookup: 'Os resolvedores dos visitantes fazem uma consulta adicional.',
    copy: 'Copiar',
    copyValue: (value) => `Copiar ${value}`,
  },

  code: {
    title: 'Código',
    version: (version) => `versão ${version}`,
    edited: ', editada',
    online: ', no ar',
    loadFailed: 'Não foi possível carregar essa versão.',
    compiles: 'O código compila.',
    notYet: 'O código ainda não compila.',
    checkFailed: 'Não foi possível verificar o código.',
    saved: (version) => `Salvo como versão ${version}.`,
    isOnline: (version) => `A versão ${version} está no ar.`,
    notOnline: 'A versão não foi colocada no ar. Veja abaixo as mensagens do compilador.',
    failed: 'A operação não foi concluída.',
    unchanged: 'Nada mudou desde o último salvamento.',
    demo: 'Esta é uma demo, portanto tudo é somente leitura. Crie um lambda próprio a partir dela para alterá-la. ',
    edit: 'Edite o código manualmente. Salvar cria uma nova versão sem alterar a que está no ar; implantar a coloca no ar. ',
    files: (entry, cs) => (
      <>
        {entry} retorna o que é servido, os demais arquivos {cs} contêm tipos e qualquer outro arquivo é servido como está.
        Ctrl+S salva; F12 vai para uma declaração.
      </>
    ),
    newer: (version) => ` A versão ${version} é mais recente que a aberta aqui.`,
    check: 'Verificar',
    save: 'Salvar',
    deploy: 'Implantar',
    binary: (size) => `Não é um arquivo de texto, portanto não pode ser editado. É servido como está e ocupa ${size} kB.`,
    saveAndDeploy: 'Salvar e implantar',
    saveVersion: 'Salvar uma nova versão',
    cancel: 'Cancelar',
    what: 'O que muda? Opcional – aparece no histórico.',
    placeholder: 'Adiciona um formulário de contato',
    goToDefinition: 'Ir para a definição',
  },

  tabs: {
    codeName: 'Letras, dígitos, hífens e sublinhados, com a extensão .cs',
    slashes: 'Sem barra no início ou no fim, e com menos de 120 caracteres.',
    deep: 'No máximo seis níveis de pastas.',
    characters: 'Letras, dígitos, hífens, sublinhados e pontos, separados por barras.',
    extension: 'É necessária uma extensão para que o arquivo seja servido corretamente.',
    exists: 'Já existe um arquivo com esse nome.',
    remove: (name) => `Remover ${name}? O conteúdo também será excluído.`,
    there: (name) => `${name} já existe.`,
    entry: 'O trecho principal: o que ele retorna é o que é servido',
    errors: 'contém erros',
    removeFile: (name) => `Remover ${name}`,
    removeTitle: 'Remover este arquivo',
    placeholder: 'Types.cs ou site/index.html',
    newFile: 'Novo arquivo',
    uploadTitle: 'Enviar um arquivo – uma imagem, uma fonte, uma página',
    upload: 'Enviar um arquivo',
  },
};
