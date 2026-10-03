import type { Messages } from '../en';

export const guide: Messages['guide'] = {
  title: 'Como funciona',
  intro:
    'Você escreve um trecho de C#. O que ele retorna fica hospedado em um endereço público, com HTTPS, em poucos segundos. Aqui está tudo, na ordem em que você vai precisar.',
  contents: 'Conteúdo',

  parts: {
    what: 'O que é uma lambda',
    first: 'Sua primeira lambda',
    editor: 'O painel de controle',
    why: 'Explicando o porquê',
    written: 'Documentação e testes',
    features: 'Mudando com segurança',
    files: 'Mais de um arquivo',
    page: 'Servindo uma página',
    spa: 'Um front-end, passo a passo',
    storage: 'Os dois lugares onde ficam os arquivos',
    database: 'Guardando registros',
    keeping: 'Guardando arquivos',
    secrets: 'Chaves e senhas',
    sockets: 'WebSockets',
    limits: 'O que não é permitido',
    away: 'Levando seu código embora',
    open: 'Publicando o código',
    agents: 'Deixando um agente fazer',
  },

  what: [
    (k) => (
      <>
        Uma lambda é um trecho de código que retorna um handler do GenHTTP. A plataforma compila, carrega e monta o que
        ele retornou no seu próprio endereço. Não tem projeto, nem arquivo de build, nem {k.code('using')}. Todos os
        módulos do GenHTTP já vêm importados.
      </>
    ),
    (k) => (
      <>
        Essa é uma lambda completa. No ar em {k.code('/lambda/your-key/')}, ela responde a toda requisição com a
        palavra hello.
      </>
    ),
  ],
  whatAside: (k) => (
    <>
      O trecho é feito de {k.em('instruções')}, não de uma classe. A última coisa que ele faz é retornar algo que atenda
      requisições: um handler ou um builder de handler.
    </>
  ),

  first: [
    (k) => (
      <>
        Clique em {k.b('Criar minha lambda')}. Você recebe um endereço público e uma chave de edição. A chave é o único
        jeito de voltar, então guarde bem. Ninguém consegue recuperar para você.
      </>
    ),
    () => (
      <>
        Você cai no painel de controle, com um pequeno serviço REST já escrito como primeira versão. É só um ponto de
        partida.
      </>
    ),
    (k) => (
      <>
        Passe a chave de edição para um agente e diga o que criar: ele escreve novas versões via{' '}
        {k.link('/#agents', 'MCP')}. Ou abra {k.b('Código')} e escreva você mesmo: {k.b('Verificar')} compila sem salvar
        nada e mostra o que o compilador achou, com arquivo e linha.
      </>
    ),
    (k) => (
      <>
        Clique em {k.b('Fazer deploy')}. Pronto, está no ar. Antes disso, nada fica acessível, e cada novo deploy
        estende o tempo que ela fica no ar.
      </>
    ),
  ],

  editor: (k) => (
    <>
      O link de edição abre um painel de controle, não uma caixa de texto: a maior parte do código aqui é escrita por
      agentes, então a primeira coisa na tela é como anda a sua lambda. A barra lateral mostra a lambda (se está no
      ar, o endereço e um botão quando uma versão mais nova está esperando para ir ao ar) e as seções dela. O que se
      faz raramente, como mudar o endereço ou excluir a lambda, fica no menu {k.b('⋯')} ali.
    </>
  ),
  bits: [
    ['Visão geral', () => <>O que é o app, se está no ar, quantas requisições teve hoje e quantas falharam, a última mudança e quanto espaço ainda sobra.</>],
    ['Documentação', () => <>O que é o app, para quem é e por quê, e por que ele é construído desse jeito. Escrita pelos agentes e guardada com cada versão.</>],
    [
      'Mudar',
      (k) => (
        <>
          Diga o que deve ficar diferente e o agente deste servidor faz isso enquanto você acompanha. Ele testa a mudança
          num rascunho - uma cópia com um endereço próprio - e a coloca no ar quando funciona. Desligue{' '}
          {k.b('Colocar no ar quando terminar')} para testar o rascunho você mesmo antes.
          Ele só trabalha no seu app: um pedido que não tem a ver com ele, ou que serve para causar dano, é recusado, e ele diz por quê.
        </>
      ),
    ],
    ['Rascunhos', () => <>Mudanças testadas antes de irem ao ar, cada uma num endereço próprio e com dados de teste próprios. Aberto, um rascunho tem código, dados de teste e logs próprios. A seção aparece quando há um rascunho.</>],
    ['Arquivos', () => <>Os arquivos de uma versão: o código e os assets, o próprio programa. Um cadeado ou um globo mostra se o público consegue acessar.</>],
    ['Dados', () => <>O que a lambda guarda enquanto roda, compartilhado por todas as versões: o banco de dados, o workspace e os segredos, cada um na sua aba. Veja as tabelas e os arquivos, envie arquivos, defina segredos ou ligue e desligue um tipo. A visão simples mostra a seção assim que o app guarda alguma coisa.</>],
    ['Versões', () => <>O que cada versão mudou, o que foi pedido e a diferença para a anterior. Faça deploy ou volte uma versão por aqui, ou comece um rascunho a partir de qualquer uma delas.</>],
    ['Deploys', () => <>O que esteve no ar e quando, e o que tirou do ar.</>],
    ['Métricas', () => <>Requisições, falhas, tempos de resposta e os caminhos mais acessados, na última hora ou nas últimas 24 horas.</>],
    ['Logs', () => <>As requisições, o que ela imprimiu e o stack trace de qualquer erro, em tempo real.</>],
    [
      'Código',
      (k) => (
        <>
          Para escrever à mão. {k.b('Verificar')} compila, {k.b('Salvar')} cria uma versão, {k.b('Fazer deploy')} coloca
          no ar. Num rascunho, {k.b('Salvar')} mantém a mudança no rascunho e a mostra no endereço do rascunho.{' '}
          {k.code('Ctrl-S')} salva; {k.code('F12')} vai para uma declaração.
        </>
      ),
    ],
    ['Testes', () => <>Como o app é testado automaticamente, com os scripts e os dados de teste para isso. Só na visualização completa.</>],
  ],
  sections: (k) => (
    <>
      Toda seção funciona do mesmo jeito: o título, um {k.b('ⓘ')} que explica, as ações à direita e, quando há mais de
      uma visualização, uma fileira de abas embaixo. No código, as abas são os arquivos. Na visualização completa, as
      seções ficam agrupadas: como as pessoas encontram o app, onde uma mudança é feita, o programa e os dados dele, e
      como ele roda.
    </>
  ),
  editorAside:
    'O tráfego e o log ficam em memória, para acompanhar, não para guardar: reiniciar o servidor zera tudo. As versões e o histórico de deploys ficam salvos.',

  why: (k) => (
    <>
      Uma versão é o código e, se você quiser, duas notas sobre ele: {k.b('a especificação')}, o que o usuário quer e
      por quê, com as palavras dele quando possível, e {k.b('a mudança')}, uma linha sobre o que a versão faz. As duas
      aparecem ao lado do diff no histórico de versões. Assim, o {k.em('porquê')} fica junto do {k.em('quê')}: para
      você e para o próximo agente que ler o histórico antes de mudar alguma coisa.
    </>
  ),
  whySample: {
    specification: 'Um livro de visitas para as pessoas assinarem; as mensagens não podem sumir num reinício',
    change: 'Guarda as mensagens no banco de dados para não sumirem num reinício',
  },
  why2: (k) => (
    <>
      Os agentes passam os mesmos dois campos para {k.code('write_code')}. Em {k.b('Código')}, salvar pede a mudança.
      Os dois são opcionais. Uma especificação longa é cortada em 4.000 caracteres, e uma mudança em 500, em vez de ser
      recusada. Um rascunho tem os próprios dois campos, e a versão em que ele é mesclado fica com eles.
    </>
  ),

  written: (k) => (
    <>
      Cada versão guarda, ao lado do programa, o que está escrito sobre ela: a {k.b('documentação')} (o que é o app,
      para quem é e por quê, e por que ele é construído desse jeito) e os {k.b('testes')}: como verificar
      automaticamente que ele funciona, com os scripts e os dados de teste para isso. Os agentes escrevem os dois ao
      criar uma lambda e os mantêm atualizados a cada mudança. O próximo agente que for mudar a lambda lê os dois
      primeiro, para saber para que serve o app e o que precisa continuar funcionando, coisa que o código sozinho não
      diz.
    </>
  ),
  writtenFiles: [
    ['.lambda/docs/product.md', 'o que é o app, para quem é, o que as pessoas fazem com ele e por quê'],
    ['.lambda/docs/decisions.md', 'as decisões técnicas, e por que foram tomadas'],
    ['.lambda/tests/README.md', 'como o app é testado automaticamente, e como rodar os testes'],
    ['.lambda/tests/…', 'os scripts e os dados de teste que os testes usam'],
  ],
  written2: (k) => (
    <>
      São arquivos da versão como quaisquer outros, na pasta {k.code('.lambda')}: o histórico mostra o que uma versão
      mudou neles, voltar uma versão traz de volta a documentação que valia para ela, e um rascunho tem uma cópia
      própria, que vai para o ar com ele. Eles nunca são compilados nem servidos, e contam no espaço que os assets de
      uma versão podem ocupar.
    </>
  ),
  written3: (k) => (
    <>
      No painel de controle, {k.b('Documentação')} mostra as páginas para ler, e {k.b('Testes')} mostra como o app é
      testado e os arquivos ao lado; a versão é escolhida do mesmo jeito que para os arquivos dela. Também dá para
      editar uma página ali, o que salva a próxima versão. A visualização simples chama a documentação de{' '}
      {k.b('Sobre')} e mostra só para que serve o app; para corrigir, avise o agente.
    </>
  ),
  writtenAside:
    'Eles são escritos no idioma que você usa com o agente, para quem for mudar o app depois, seja uma pessoa ou um agente. Não são uma cópia do código: dizem para que ele serve, e por quê.',

  features: (k) => (
    <>
      Uma versão nunca muda depois de salva, e é isso que faz cada uma valer a pena: qualquer uma delas pode ser
      comparada e colocada de volta no ar exatamente como era. Para mudar uma lambda que as pessoas usam, comece um{' '}
      {k.b('rascunho')}.
    </>
  ),
  featureSteps: [
    (k) => (
      <>
        Comece a partir de qualquer versão em {k.b('Versões')}, ou deixe o agente começar um. Ele é uma cópia do
        código, dos assets, da documentação e dos testes dessa versão, e dos dados da lambda.
      </>
    ),
    (k) => (
      <>
        Mude quantas vezes precisar, em {k.b('Código')} ou pedindo ao agente. A prévia responde num endereço próprio,{' '}
        {k.code('/features/…/')}, com dados de teste próprios. Os visitantes da lambda não veem nada disso, e nada do
        que ela grava chega aos dados da lambda.
      </>
    ),
    (k) => (
      <>
        Quando estiver tudo certo, {k.b('Colocar no ar')}: ele vira a próxima versão, com as notas dele, e vai ao ar. O
        rascunho some junto, com a prévia e os dados de teste.
      </>
    ),
  ],
  featureSample: 'Ranking',
  featuresAside: () => (
    <>
      Dá para trabalhar em vários rascunhos ao mesmo tempo. Só um que esteja atualizado em relação à versão mais
      recente pode ir ao ar, para que nunca desfaça uma versão salva depois que o rascunho começou. Se outro foi ao ar
      primeiro, traga as mudanças dele (ou peça ao agente) e marque o rascunho como atualizado. Nada vai ao ar
      sozinho; isso é de propósito. A API chama um rascunho de feature, e colocá-lo no ar de merge.
    </>
  ),

  files: (k) => (
    <>
      Os tipos não precisam ficar embaixo do código que os usa. Em {k.b('Código')}, clique em {k.b('+')} ao lado dos
      arquivos: o novo arquivo é compilado junto com o trecho, no mesmo namespace, então não precisa importar nada. Um
      nome sem extensão é tratado como C#.
    </>
  ),

  page: 'Existem dois jeitos de servir uma página, e mais um para o que as pessoas enviam junto com ela.',
  inlineTitle: 'Uma página, escrita no código',
  inline: 'Bom para algo pequeno. A página faz parte do trecho.',
  folderTitle: 'Uma pasta de arquivos de verdade',
  folder:
    'O ideal para qualquer coisa com CSS e script. Os arquivos são adicionados do mesmo jeito que um arquivo C# e servidos exatamente como foram escritos. Nada compila esses arquivos.',
  workspaceTitle: 'Arquivos enviados, dos dados',
  workspace:
    'Para o que as pessoas enviam ou a lambda cria (fotos, documentos), servido junto com o app. Não para as páginas do próprio app: essas ficam numa pasta de arquivos, onde entram nas versões junto com o código que depende delas.',

  spa: (k) => (
    <>
      O segundo jeito, completo. Toda demo serve a página assim, de uma pasta chamada {k.code('web')}. Abra{' '}
      {k.link('/editor/demo-crud', 'demo-crud')} para ver uma. As demos são somente leitura; a chave de edição delas é o
      próprio nome.
    </>
  ),
  spaSteps: [
    (k) => (
      <>
        Em {k.b('Código')}, clique em {k.b('+')} ao lado dos arquivos e digite {k.code('site/index.html')}. Um nome com
        barra coloca o arquivo em uma pasta; um nome com extensão é tratado como o tipo de arquivo que ele indica.
      </>
    ),
    (k) => (
      <>
        Adicione {k.code('site/app.css')} e {k.code('site/app.js')} do mesmo jeito. Sua página se refere a eles pelo
        nome, como em {k.code('href="app.css"')}, porque a pasta é a raiz do que é servido, e não parte do endereço.
      </>
    ),
    (k) => (
      <>
        Para o que não é texto, como uma imagem ou uma fonte, abra um arquivo em {k.code('site')} e use o botão de
        enviar ao lado dos arquivos: ele vai para a mesma pasta. Não dá para digitar um PNG num editor de texto, então o
        caminho é esse.
      </>
    ),
    (k) => <>Em {k.code('lambda.cs')}, sirva a pasta:</>,
    (k) => (
      <>
        Clique em {k.b('Fazer deploy')}. {k.code('site/index.html')} responde em {k.code('/')}, {k.code('site/app.css')}{' '}
        em {k.code('/app.css')}, e qualquer endereço que não bate com nenhum arquivo recebe a página. Assim, um front-end
        com roteamento próprio continua funcionando quando alguém recarrega a página num deep link.
      </>
    ),
    () => <>Adicione uma API ao lado, e a página ganha com quem conversar:</>,
  ],

  storage: (k) => (
    <>
      Uma lambda guarda arquivos em dois lugares, e o editor mostra cada um separado: {k.b('Arquivos')} tem os
      arquivos de uma versão (o programa), e {k.b('Dados')} tem o workspace (o que o programa guarda). A diferença é{' '}
      {k.em('de quem eles são')}. Os arquivos de uma versão pertencem a essa versão; os dados pertencem à lambda, e
      todas as versões usam os mesmos.
    </>
  ),
  savedWithCode: 'Numa versão',
  workspaceColumn: 'Nos dados',
  table: [
    ['o que guarda', 'o código e os assets: o programa, inclusive o front-end, e também a documentação e os testes dele', 'tudo o que a lambda grava ou alguém envia'],
    ['quando muda', 'nunca: uma mudança é uma nova versão', 'no momento em que algo é gravado'],
    ['um deploy', 'coloca exatamente esses arquivos no ar', 'nunca mexe neles'],
    ['voltar uma versão', 'traz os arquivos antigos de volta', 'nenhum efeito: são os mesmos para todas as versões'],
    ['um rascunho', 'começa como uma cópia deles', 'trabalha numa cópia deles'],
    ['quando somem', 'com as versões antigas, depois do limite', 'com a lambda, ou quando você desliga'],
  ],
  reachedAs: 'acessado no código como',
  storageAside:
    'Os dois não podem ser um lugar só. Se fossem, um deploy ou apagaria tudo o que a lambda gravou desde então, ou nada nunca poderia sair dos arquivos que vão com ela. Um jogo com ranking quer a segunda opção; a página que ele serve quer a primeira. Então a página fica na versão, e o ranking nos dados.',

  database: (k) => (
    <>
      Registros – entradas, contas, pedidos, votos – ficam no {k.b('banco de dados')}: um banco de dados SQLite só da
      lambda, ligado em {k.b('Dados')}. O código abre uma conexão com {k.code('Database.GetConnection()')} e lê e grava
      nele por meio do {k.link('https://learn.microsoft.com/ef/core/', 'Entity Framework Core')}, com um contexto
      próprio que mapeia as tabelas:
    </>
  ),
  database2: (k) => (
    <>
      As tabelas dele são criadas por {k.b('migrações')}: arquivos SQL que vão com a versão em {k.code('migrations/')},
      aplicados em ordem pelo {k.link('https://evolve-db.netlify.app/', 'Evolve')} quando a lambda inicia – cada um uma
      única vez, então uma nova versão só executa o que é novo. Nunca mude uma migração que já foi aplicada; uma
      mudança numa tabela é o próximo arquivo.
    </>
  ),
  database3: (k) => (
    <>
      Como todos os dados, o banco de dados é compartilhado por todas as versões, fazer deploy ou voltar uma versão não
      mexe nele, e um rascunho trabalha numa cópia. Em {k.b('Dados')} você vê as tabelas e as linhas delas, que a
      visão simples chama de registros. {k.b('Baixar como projeto .NET')} leva o banco junto como um arquivo SQLite
      comum.
    </>
  ),
  databaseAside: (k) => (
    <>
      Crie um contexto onde precisar e libere-o depois, e use-o de forma síncrona – {k.code('ToList')} e{' '}
      {k.code('SaveChanges')}, não {k.code('ToListAsync')} e {k.code('SaveChangesAsync')}. Quem cria as tabelas são as
      migrações, nunca o Entity Framework. A demo {k.link('/editor/demo-crud', 'demo-crud')} faz tudo isso.
    </>
  ),

  keeping: (k) => (
    <>
      {k.code('Workspace')} é um diretório privado que sua lambda pode ler e gravar: o lugar para arquivos – fotos que
      alguém envia, um documento que ela gera, um modelo que ela carrega. Registros ficam no banco de dados, e o que se
      sabe sobre um arquivo – quem enviou, quando – também é um registro.
    </>
  ),
  keeping2: (k) => (
    <>
      Também tem {k.code('ReadBytes')}, {k.code('WriteBytes')}, {k.code('Delete')}, {k.code('List')},{' '}
      {k.code('CreateFolder')} e {k.code('Tree')}/{k.code('Files')}/{k.code('App')} para servir o conteúdo. Nada mais
      no sistema de arquivos é acessível.
    </>
  ),

  secrets: (k) => (
    <>
      Uma chave de API, uma senha ou um token vai nos {k.b('segredos')}, não no código – onde ficaria em cada versão, em
      cada download e à vista de quem ler o histórico. O código lê um segredo pelo nome:
    </>
  ),
  secrets2: (k) => (
    <>
      Ligue os segredos em {k.b('Dados')} e defina o valor ali. Depois de salvo, ele nunca mais é mostrado – nem para
      você, nem para um agente; você só pode substituí-lo. A lista mostra quais nomes o código lê que ainda não têm
      valor, e a visão geral pede por eles. {k.code('Secret.Exists')} diz se um segredo está definido, para código que
      funciona sem ele. Como todos os dados, os segredos são compartilhados por todas as versões, e um rascunho trabalha
      numa cópia.
    </>
  ),
  secretsAside: (k) => (
    <>
      Eles são guardados criptografados, com uma chave que não fica no banco de dados. Num projeto baixado,{' '}
      {k.code('Secret.Read("NAME")')} lê a variável de ambiente {k.code('NAME')} – os valores ficam aqui.
    </>
  ),

  sockets: (k) => (
    <>
      Suporte completo, pensado desde o início. A demo {k.link('/editor/demo-game', 'demo-game')} forma pares de
      jogadores e roda cada partida no servidor. A forma mais simples são três callbacks:
    </>
  ),
  socketsAside: (k) => (
    <>
      Uma coisa pega todo mundo: o navegador não consegue definir headers no handshake de um WebSocket. Passe o que o
      handler precisa na query, que ele lê de {k.code('connection.Request.Header.Query')}, ou mande segredos na primeira
      mensagem.
    </>
  ),
  sockets2: (k) => (
    <>
      Quando a página só escuta - uma contagem, um feed, um placar - os server-sent events são mais simples: uma única
      resposta longa na qual o servidor continua escrevendo e que o navegador reconecta sozinho. O demo{' '}
      {k.link('/editor/demo-live', 'demo-live')} envia cada voto a todos que estão assistindo dessa forma. De qualquer
      maneira, é o servidor que envia o que mudou. Uma página que pergunta de novo a cada poucos segundos faz uma
      requisição a cada vez, tenha algo mudado ou não, e ainda chega atrasada.
    </>
  ),

  limits:
    'Seu código roda em um servidor compartilhado, então parte do C# é recusada antes de compilar: iniciar processos, abrir seus próprios sockets, carregar assemblies, acessar o sistema de arquivos fora do seu workspace e usar reflection para contornar qualquer uma dessas regras. O mesmo vale para esperar uma task com .Result ou .Wait() em vez de usar await: as requisições rodam em uma thread por núcleo, e a task teria que terminar justamente na thread que está esperando por ela.',
  limits2:
    'Todo o resto está disponível, incluindo a API de módulos do GenHTTP inteira. Se algo for recusado, você fica sabendo a linha e o motivo, e não só que falhou.',

  away: (k) => (
    <>
      {k.b('Baixar como projeto .NET')}, no editor, entrega tudo pronto: uma solução que você pode abrir, rodar com{' '}
      {k.code('dotnet run')} e guardar. Ela só precisa do pacote GenHTTP e vem com um {k.code('Dockerfile')} para
      compilá-la e rodá-la como contêiner.
    </>
  ),
  away2: (k) => (
    <>
      Seu trecho vira o {k.code('Project.cs')}, e o {k.code('Program.cs')} serve o que ele retorna. Seus outros
      arquivos vão exatamente como você escreveu. {k.code('Workspace')} e {k.code('Assets')} viram duas pastas ao lado
      do programa, com os mesmos métodos, separadas numa pasta {k.code('Platform')}, então nada no seu código precisa
      mudar.
      {' '}{k.code('Secret')} lê lá as variáveis de ambiente de mesmo nome; os valores ficam aqui. A documentação e os
      testes vão junto, em {k.code('docs')} e {k.code('tests')}.
      {' '}{k.code('Database')} abre {k.code('database/database.db')}, que o download traz com os registros que o seu
      app guardou.
    </>
  ),
  awayAside:
    'Bom saber antes de criar qualquer coisa aqui: o que você escreve é seu e sai inteiro. Rodar nesta máquina não prende seu código a ela.',

  open: (k) => (
    <>
      Se o que você criou pode ajudar outras pessoas, publique o código: abra {k.b('Código aberto')} no painel de
      controle, escolha uma licença (a MIT, a menos que você queira outra) e ligue a publicação. O código ganha uma
      página própria entre os {k.link('/source', 'apps de código aberto')}, onde qualquer pessoa pode lê-lo, dar uma
      estrela a ele e baixar qualquer versão como o mesmo projeto que {k.b('Baixar como projeto .NET')} entrega, com a
      licença ao lado.
    </>
  ),
  open2: () => (
    <>
      Todas as versões são publicadas, inclusive as anteriores, com a documentação, os testes e a mudança que cada uma
      fez. O que o app guarda nunca é publicado (os registros, os arquivos que ele salvou, os valores das chaves e
      senhas), nem o que você pediu com as suas próprias palavras, nem quem usa o app. Desligue a publicação e a página
      some; as estrelas ficam guardadas para quando você publicar de novo.
    </>
  ),
  openAside:
    'Tudo o que está no código fica público, inclusive as versões anteriores. Uma chave ou uma senha fica junto das chaves e senhas em Dados, nunca no código, publicado ou não.',

  agents: (k) => (
    <>
      Existe um endpoint MCP em {k.code('/mcp')}. Conecte um agente a ele, e o agente pode fazer tudo o que o editor
      faz: ler o guia, ler uma demo inteira, escrever arquivos, compilar e fazer deploy. Por baixo, é a mesma API.
    </>
  ),
  agents2: (k) => (
    <>
      O agente explica o porquê enquanto trabalha ({k.code('write_code')} recebe a especificação e a mudança) e pode
      ver o que colocou no ar: {k.code('read_logs')} responde com as requisições recentes da lambda, o que ela imprimiu
      e o stack trace de qualquer exceção. É assim que ele descobre que o código funciona, em vez de supor. Você
      acompanha tudo isso no painel de controle. Ele escreve a documentação e os testes enquanto trabalha, lê os dois
      antes de mudar qualquer coisa e roda os testes no endereço de um rascunho antes de colocá-lo no ar. Uma página
      feita para ser encontrada ganha título, descrição, ícone e uma prévia para quando alguém compartilha o link. No
      rodapé das páginas que constrói, ele acrescenta uma pequena linha dizendo que foram feitas com GenHTTP Lambda;
      avise se preferir que não apareça, e ele a remove.
    </>
  ),
  more: 'Saiba mais →',
  make: 'Criar uma lambda',
};
