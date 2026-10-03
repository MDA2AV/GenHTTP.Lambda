import type { Messages } from '../en';

export const guide: Messages['guide'] = {
  title: 'Como funciona',
  intro:
    'Escreves um snippet de C#. Aquilo que ele devolve fica alojado num endereço público, com HTTPS, em poucos segundos. Aqui está tudo, pela ordem em que o vais encontrar.',
  contents: 'Índice',

  parts: {
    what: 'O que é uma lambda',
    first: 'A tua primeira lambda',
    editor: 'O painel de controlo',
    why: 'Explicar o porquê',
    written: 'Documentação e testes',
    features: 'Alterar com segurança',
    files: 'Mais do que um ficheiro',
    page: 'Servir uma página',
    spa: 'Um front-end, passo a passo',
    storage: 'Os dois sítios onde vivem os ficheiros',
    database: 'Guardar registos',
    keeping: 'Guardar ficheiros',
    secrets: 'Chaves e palavras-passe',
    sockets: 'WebSockets',
    limits: 'O que não podes fazer',
    away: 'Levar o código contigo',
    open: 'Publicar o código',
    agents: 'Deixar um agente fazer o trabalho',
  },

  what: [
    (k) => (
      <>
        Uma lambda é um snippet que devolve um handler do GenHTTP. A plataforma compila-o, carrega-o e monta o que ele
        devolveu no teu próprio endereço. Não há projeto, nem ficheiro de build, nem instruções {k.code('using')}. Todos
        os módulos do GenHTTP já vêm importados.
      </>
    ),
    (k) => (
      <>
        Isto é uma lambda completa. Publicada em {k.code('/lambda/your-key/')}, responde a todos os pedidos com a
        palavra hello.
      </>
    ),
  ],
  whatAside: (k) => (
    <>
      O snippet é feito de {k.em('instruções')}, não de uma classe. A última coisa que faz é devolver algo que consiga
      servir pedidos: um handler, ou um builder de um handler.
    </>
  ),

  first: [
    (k) => (
      <>
        Clica em {k.b('Criar a minha lambda')}. Recebes um endereço público e uma chave de edição. A chave é a única forma
        de voltares a entrar, por isso guarda-a. Ninguém a consegue recuperar por ti.
      </>
    ),
    () => (
      <>
        Vais parar ao painel de controlo, com um pequeno serviço REST já escrito como primeira versão. É só um ponto de
        partida.
      </>
    ),
    (k) => (
      <>
        Dá a chave de edição a um agente e diz-lhe o que deve criar: ele escreve novas versões via{' '}
        {k.link('/#agents', 'MCP')}. Ou abre {k.b('Código')} e escreve-o tu: {k.b('Verificar')} compila sem guardar nada
        e diz-te o que o compilador acha, com ficheiro e linha.
      </>
    ),
    (k) => (
      <>
        Clica em {k.b('Fazer deploy')}. Agora está online. Antes disso não há nada acessível, e cada novo deploy
        prolonga o tempo que fica online.
      </>
    ),
  ],

  editor: (k) => (
    <>
      O link de edição abre um painel de controlo em vez de uma caixa de texto: aqui, a maior parte do código é escrita
      por agentes, por isso a primeira coisa no ecrã é como está a tua lambda. A barra lateral tem a lambda (se está
      online, o endereço e um botão quando há uma versão mais recente à espera de ficar online) e as secções. O que se
      faz raramente, como mudar o endereço ou eliminá-la, está no menu {k.b('⋯')} que lá encontras.
    </>
  ),
  bits: [
    ['Visão geral', () => <>O que é a app, se está online, quantos pedidos teve hoje e quantos falharam, a última alteração e quanto espaço ainda sobra.</>],
    ['Documentação', () => <>O que é a app, para quem é e porquê, e porque está construída desta forma. Escrita pelos agentes e guardada com cada versão.</>],
    [
      'Alterar',
      (k) => (
        <>
          Diz o que deve ficar diferente e o agente deste servidor trata disso enquanto acompanhas. Experimenta a
          alteração num rascunho - uma cópia com um endereço próprio - e põe-na online quando funcionar. Desliga{' '}
          {k.b('Pôr online quando terminar')} para experimentares tu o rascunho primeiro.
          Só trabalha na tua app: um pedido que não tenha a ver com ela, ou que sirva para causar dano, é recusado, e ele diz porquê.
        </>
      ),
    ],
    ['Rascunhos', () => <>Alterações a ser experimentadas antes de irem online, cada uma num endereço próprio e com dados de teste próprios. Aberto, um rascunho tem o seu próprio código, dados de teste e logs. A secção existe assim que há um rascunho.</>],
    ['Ficheiros', () => <>Os ficheiros de uma versão: o código e os assets, o próprio programa. Um cadeado ou um globo indica se o público lhes consegue aceder.</>],
    ['Dados', () => <>O que a lambda guarda enquanto corre, partilhado por todas as versões: a base de dados, o workspace e as chaves e palavras-passe, cada um no seu separador. Vê as tabelas e os ficheiros, carrega ficheiros, define chaves e palavras-passe ou liga e desliga um tipo. A vista simples mostra-o assim que a app guarda alguma coisa.</>],
    ['Versões', () => <>O que cada versão mudou, o que foi pedido e a diferença para a anterior. Faz deploy ou reverte a partir daqui, ou começa um rascunho a partir de qualquer uma delas.</>],
    ['Deploys', () => <>O que esteve online e quando, e o que o pôs offline.</>],
    ['Estatísticas', () => <>Pedidos, falhas, tempos de resposta e os caminhos mais pedidos, na última hora ou nas últimas 24 horas.</>],
    ['Logs', () => <>Os pedidos, o que a lambda escreveu na consola e o stack trace de tudo o que correu mal, em tempo real.</>],
    [
      'Código',
      (k) => (
        <>
          Para o escrever à mão. {k.b('Verificar')} compila, {k.b('Guardar')} cria uma versão, {k.b('Fazer deploy')}{' '}
          põe-na online. Num rascunho, {k.b('Guardar')} mantém-no no rascunho e mostra-o no endereço do rascunho.
          {k.code('Ctrl-S')} guarda; {k.code('F12')} vai para a declaração.
        </>
      ),
    ],
    ['Testes', () => <>Como a app é testada automaticamente, com os scripts e os dados de teste para isso. Só na vista completa.</>],
  ],
  sections: (k) => (
    <>
      Todas as secções funcionam da mesma forma: o título, um {k.b('ⓘ')} que a explica, as ações à direita e, quando há
      mais do que uma vista, uma fila de separadores por baixo. No código, os separadores são os ficheiros. A vista
      completa junta as secções em grupos: como as pessoas encontram a app, onde se faz uma alteração, o programa e os
      seus dados, e como corre.
    </>
  ),
  editorAside:
    'O tráfego e os logs ficam em memória, para acompanhar e não para guardar: um reinício do servidor põe-nos a zero. As versões e o histórico de deploys ficam guardados.',

  why: (k) => (
    <>
      Uma versão é o código e, se quiseres, duas notas sobre ele: {k.b('a especificação')}, o que o utilizador quer e
      porquê, com as palavras dele sempre que possível, e {k.b('a alteração')}, uma linha sobre o que a versão faz.
      Aparecem ao lado do diff no histórico de versões, por isso o {k.em('porquê')} fica junto do {k.em('quê')}: para ti
      e para o próximo agente que ler o histórico antes de mudar alguma coisa.
    </>
  ),
  whySample: {
    specification: 'Um livro de visitas que as pessoas podem assinar; as entradas têm de sobreviver a um reinício',
    change: 'Guarda as entradas na base de dados para sobreviverem a um reinício',
  },
  why2: (k) => (
    <>
      Os agentes passam os mesmos dois campos a {k.code('write_code')}. Em {k.b('Código')}, guardar pede-te a
      alteração. Ambos são opcionais: em vez de ser recusada, uma especificação longa é cortada aos 4000 caracteres, e
      uma alteração aos 500. Um rascunho tem as suas próprias duas notas, e a versão em que é integrado fica com elas.
    </>
  ),

  written: (k) => (
    <>
      Cada versão guarda, ao lado do programa, o que está escrito sobre ela: a sua {k.b('documentação')} (o que é a
      app, para quem é e porquê, e porque está construída desta forma) e os seus {k.b('testes')}: como verificar
      automaticamente que funciona, com os scripts e os dados de teste para isso. Os agentes escrevem-nos com uma nova
      lambda e mantêm-nos atualizados a cada alteração. O próximo agente a alterar a lambda lê-os primeiro, para saber
      para que serve a app e o que tem de continuar a funcionar, algo que o código, por si só, não diz.
    </>
  ),
  writtenFiles: [
    ['.lambda/docs/product.md', 'o que é a app, para quem é, o que as pessoas fazem com ela e porquê'],
    ['.lambda/docs/decisions.md', 'as decisões técnicas, e porque foram tomadas'],
    ['.lambda/tests/README.md', 'como a app é testada automaticamente, e como correr os testes'],
    ['.lambda/tests/…', 'os scripts e os dados de teste que os testes usam'],
  ],
  written2: (k) => (
    <>
      São ficheiros da versão como quaisquer outros, na pasta {k.code('.lambda')}: o histórico mostra o que uma versão
      mudou neles, reverter traz de volta a documentação que correspondia a essa versão, e um rascunho tem uma cópia
      própria, que fica online com ele. Nunca são compilados nem servidos, e contam para o espaço que os assets de uma
      versão podem ocupar.
    </>
  ),
  written3: (k) => (
    <>
      No painel de controlo, {k.b('Documentação')} mostra as páginas para ler, e {k.b('Testes')} como a app é testada e
      os ficheiros ao lado; a versão escolhe-se tal como para os seus ficheiros. Também é possível editar lá uma página,
      o que guarda a versão seguinte. A vista simples chama à documentação {k.b('Sobre')} e mostra apenas para que
      serve a app: para a corrigir, diz ao agente.
    </>
  ),
  writtenAside:
    'São escritos na língua que usas com o agente, para quem alterar a app a seguir, seja uma pessoa ou um agente. Não são uma cópia do código: dizem para que serve, e porquê.',

  features: (k) => (
    <>
      Uma versão nunca muda depois de guardada, e é isso que faz com que valha a pena guardar cada uma: qualquer uma
      pode ser comparada e voltar a ficar online exatamente como era. Para alterar uma lambda que as pessoas usam,
      começa antes um {k.b('rascunho')}.
    </>
  ),
  featureSteps: [
    (k) => (
      <>
        Começa-o a partir de qualquer versão em {k.b('Versões')}, ou deixa o agente começá-lo. É uma cópia do código,
        dos assets, da documentação e dos testes dessa versão, e dos dados da lambda.
      </>
    ),
    (k) => (
      <>
        Altera-o as vezes que for preciso: em {k.b('Código')}, ou pedindo ao agente. A pré-visualização responde num
        endereço próprio, {k.code('/features/…/')}, com dados de teste próprios. Os visitantes da lambda não veem nada
        disto, e nada do que ele escreve chega aos dados da lambda.
      </>
    ),
    (k) => (
      <>
        {k.b('Pôr online')} quando estiver bem: passa a ser a próxima versão, com as suas notas, e fica online. O
        rascunho desaparece com isso: a pré-visualização e os dados de teste.
      </>
    ),
  ],
  featureSample: 'Ranking',
  featuresAside: (() => (
    <>
      Podes trabalhar em vários rascunhos ao mesmo tempo. Só um que esteja atualizado com a versão mais recente pode
      ficar online, para que nunca desfaça uma versão guardada depois de o rascunho ter começado. Se outro ficou
      online primeiro, traz as alterações dele - ou pede ao agente que o faça - e marca o rascunho como atualizado.
      Nada fica online sozinho; é de propósito. A API chama feature a um rascunho, e merge a pô-lo online.
    </>
  )),

  files: (k) => (
    <>
      Os tipos não têm de ficar por baixo do código que os usa. Em {k.b('Código')}, clica em {k.b('+')} ao lado dos
      ficheiros: o novo ficheiro é compilado ao lado do snippet, no mesmo namespace, por isso não é preciso importar nada.
      Um nome sem extensão é tratado como C#.
    </>
  ),

  page: 'Há duas formas de servir uma página, e mais uma para o que as pessoas carregam junto dela.',
  inlineTitle: 'Uma página, escrita no código',
  inline: 'Serve para algo pequeno. A página faz parte do snippet.',
  folderTitle: 'Uma pasta de ficheiros a sério',
  folder:
    'O que queres para qualquer coisa com uma folha de estilos e um script. Os ficheiros são adicionados da mesma forma que um ficheiro C#, e servidos exatamente como foram escritos. Nada os compila.',
  workspaceTitle: 'Ficheiros carregados, a partir dos dados',
  workspace:
    'Para o que as pessoas carregam ou a lambda cria (fotografias, documentos), servido ao lado da app. Não para as páginas da própria app: essas pertencem a uma pasta de ficheiros, onde entram nas versões juntamente com o código que precisa delas.',

  spa: (k) => (
    <>
      A segunda, em pormenor. Todas as demos servem a página assim, a partir de uma pasta chamada {k.code('web')}: abre{' '}
      {k.link('/editor/demo-crud', 'demo-crud')} para ver uma. As demos são só de leitura; a chave de edição de cada uma é
      o próprio nome.
    </>
  ),
  spaSteps: [
    (k) => (
      <>
        Em {k.b('Código')}, clica em {k.b('+')} ao lado dos ficheiros e escreve {k.code('site/index.html')}. Um nome com
        uma barra põe o ficheiro numa pasta; um nome com extensão é tratado como o tipo de ficheiro que indica.
      </>
    ),
    (k) => (
      <>
        Adiciona {k.code('site/app.css')} e {k.code('site/app.js')} da mesma forma. A tua página refere-se a eles pelo
        nome, como em {k.code('href="app.css"')}, porque a pasta é a raiz do que é servido e não faz parte do endereço.
      </>
    ),
    (k) => (
      <>
        Para o que não é texto, como uma imagem ou um tipo de letra, abre um ficheiro em {k.code('site')} e clica no botão
        de carregar ao lado dos ficheiros: vai parar à mesma pasta. Um PNG não se escreve num editor de texto, por isso é
        por aí que entra.
      </>
    ),
    (k) => <>Em {k.code('lambda.cs')}, serve a pasta:</>,
    (k) => (
      <>
        Clica em {k.b('Fazer deploy')}. {k.code('site/index.html')} responde em {k.code('/')}, {k.code('site/app.css')}{' '}
        em {k.code('/app.css')}, e qualquer endereço que não corresponda a nenhum ficheiro recebe a página. Assim, um
        front-end com routing próprio continua a funcionar quando alguém recarrega num deep link.
      </>
    ),
    () => <>Junta-lhe uma API e a página já tem com quem falar:</>,
  ],

  storage: (k) => (
    <>
      Uma lambda guarda ficheiros em dois sítios, e o editor mostra-os em separado: {k.b('Ficheiros')} tem os ficheiros
      de uma versão (o programa) e {k.b('Dados')} tem o workspace (o que o programa guarda). A diferença está em{' '}
      {k.em('a quem pertencem')}. Os ficheiros de uma versão pertencem a essa versão; os dados pertencem à lambda, e
      todas as versões os partilham.
    </>
  ),
  savedWithCode: 'Numa versão',
  workspaceColumn: 'Nos dados',
  table: [
    ['o que guarda', 'o código e os assets: o programa, incluindo o front-end, e também a sua documentação e os seus testes', 'tudo o que a lambda escreve, ou que alguém carrega'],
    ['quando muda', 'nunca: uma alteração é uma nova versão', 'no momento em que algo é escrito'],
    ['um deploy', 'põe online exatamente estes ficheiros', 'nunca lhes toca'],
    ['reverter', 'traz de volta os ficheiros antigos', 'não tem efeito: são os mesmos para todas as versões'],
    ['um rascunho', 'começa como uma cópia deles', 'trabalha numa cópia deles'],
    ['quando desaparecem', 'com as versões antigas, passado o limite', 'com a lambda, ou quando os desligas'],
  ],
  reachedAs: 'acedido no código como',
  storageAside:
    'Não podem ser um só sítio. Se fossem, um deploy ou apagava tudo o que a lambda escreveu entretanto, ou nunca se poderia remover nada do que ela traz. Um jogo com um ranking quer a segunda opção; a página que ele serve quer a primeira. Por isso, a página vai na versão, e o ranking nos dados.',

  database: (k) => (
    <>
      Os registos (entradas, contas, encomendas, votos) pertencem à {k.b('base de dados')}: uma base de dados SQLite só
      da lambda, ligada em {k.b('Dados')}. O código abre uma ligação com {k.code('Database.GetConnection()')} e lê e
      escreve nela através do {k.link('https://learn.microsoft.com/ef/core/', 'Entity Framework Core')}, com um
      contexto próprio que mapeia as tabelas:
    </>
  ),
  database2: (k) => (
    <>
      As tabelas são criadas por {k.b('migrações')}: ficheiros SQL que seguem com a versão em {k.code('migrations/')},
      aplicados por ordem pelo {k.link('https://evolve-db.netlify.app/', 'Evolve')} quando a lambda arranca – cada um
      uma só vez, por isso uma nova versão só corre o que é novo. Nunca alteres uma migração que já foi aplicada; uma
      alteração a uma tabela é o ficheiro seguinte.
    </>
  ),
  database3: (k) => (
    <>
      Como todos os dados, a base de dados é partilhada por todas as versões, os deploys e as reversões não lhe tocam,
      e um rascunho trabalha sobre uma cópia dela. Em {k.b('Dados')} vês as tabelas e o que têm; a vista simples
      chama-lhes registos. {k.b('Transferir como projeto .NET')} leva-a consigo como um simples ficheiro SQLite.
    </>
  ),
  databaseAside: (k) => (
    <>
      Cria um contexto onde precisares dele e liberta-o quando terminares, e usa-o de forma síncrona:{' '}
      {k.code('ToList')} e {k.code('SaveChanges')}, e não {k.code('ToListAsync')} e {k.code('SaveChangesAsync')}. As
      tabelas são criadas pelas migrações, nunca pelo Entity Framework. A demo{' '}
      {k.link('/editor/demo-crud', 'demo-crud')} faz tudo isto.
    </>
  ),

  keeping: (k) => (
    <>
      {k.code('Workspace')} é um diretório privado onde a tua lambda pode ler e escrever: o sítio para ficheiros –
      fotografias que alguém carrega, um documento que ela cria, um modelo que ela lê. Os registos pertencem à base de
      dados, e o que se sabe sobre um ficheiro – quem o carregou, e quando – também é um registo.
    </>
  ),
  keeping2: (k) => (
    <>
      Há também {k.code('ReadBytes')}, {k.code('WriteBytes')}, {k.code('Delete')}, {k.code('List')},{' '}
      {k.code('CreateFolder')} e {k.code('Tree')}/{k.code('Files')}/{k.code('App')} para o servir. Mais nada no sistema de
      ficheiros é acessível.
    </>
  ),

  secrets: (k) => (
    <>
      Uma chave de API, uma palavra-passe ou um token vai para os {k.b('segredos')}, não para o código – onde o teriam
      cada versão, cada transferência e quem quer que leia o histórico. O código lê um segredo pelo nome:
    </>
  ),
  secrets2: (k) => (
    <>
      Liga os segredos em {k.b('Dados')} e define lá o valor. Depois de guardado, nunca mais é mostrado – nem a ti, nem
      a um agente; só o podes substituir. A lista mostra que nomes o código lê que ainda não têm valor, e a visão geral
      pede-os. {k.code('Secret.Exists')} diz se um segredo está definido, para código que funciona sem ele. Como todos
      os dados, os segredos são partilhados por todas as versões, e um rascunho trabalha sobre uma cópia.
    </>
  ),
  secretsAside: (k) => (
    <>
      São guardados cifrados, com uma chave que não está na base de dados. Num projeto transferido,{' '}
      {k.code('Secret.Read("NAME")')} lê a variável de ambiente {k.code('NAME')} – os valores ficam aqui.
    </>
  ),

  sockets: (k) => (
    <>
      Suportados, e pensados de raiz. A demo {k.link('/editor/demo-game', 'demo-game')} emparelha jogadores e corre todos
      os jogos no servidor. A forma mais simples são três callbacks:
    </>
  ),
  socketsAside: (k) => (
    <>
      Há uma coisa que apanha toda a gente: um browser não consegue definir headers no handshake de um WebSocket. Passa o
      que o handler precisa na query, onde ele o lê a partir de {k.code('connection.Request.Header.Query')}, ou envia os
      segredos na primeira mensagem.
    </>
  ),
  sockets2: ((k) => (
    <>
      Quando a página só fica à escuta (uma contagem, um feed, um resultado em direto), os server-sent events são mais
      simples: uma única resposta longa em que o servidor vai escrevendo e que o browser volta a ligar sozinho. A demo{' '}
      {k.link('/editor/demo-live', 'demo-live')} envia assim cada voto a todos os que estão a ver. Seja como for, é o
      servidor que envia o que mudou. Uma página que pergunta de novo a cada poucos segundos faz um pedido de cada vez,
      tenha algo mudado ou não, e mesmo assim chega atrasada.
    </>
  )),

  limits:
    'O teu código corre num servidor partilhado, por isso parte do C# é recusada antes de compilar: iniciar processos, abrir sockets próprios, carregar assemblies, aceder ao sistema de ficheiros fora do teu workspace, e usar reflection para contornar qualquer uma destas regras. O mesmo vale para esperar por uma task com .Result ou .Wait() em vez de usar await: os pedidos correm numa thread por núcleo, e a task teria de terminar precisamente na thread que está à espera dela.',
  limits2:
    'Todo o resto está lá, incluindo toda a API de módulos do GenHTTP. Se algo for recusado, ficas a saber em que linha e porquê, e não apenas que falhou.',

  away: (k) => (
    <>
      {k.b('Transferir como projeto .NET')}, no editor, dá-te tudo de uma vez: uma solução que podes abrir, correr com{' '}
      {k.code('dotnet run')} e guardar. Só precisa do pacote GenHTTP e traz um {k.code('Dockerfile')} para a
      compilares e correres como contentor.
    </>
  ),
  away2: (k) => (
    <>
      O teu snippet passa a ser o {k.code('Project.cs')}, e o {k.code('Program.cs')} serve o que ele devolve. Os teus
      outros ficheiros vêm exatamente como os escreveste. {k.code('Workspace')} e {k.code('Assets')} passam a ser duas
      pastas ao lado do programa, com os mesmos métodos, à parte numa pasta {k.code('Platform')}, por isso não tens de
      mudar nada no teu código.
      {' '}{k.code('Secret')} lê aí as variáveis de ambiente com o mesmo nome; os valores ficam cá. A documentação e os
      testes vêm também, em {k.code('docs')} e {k.code('tests')}.
      {' '}{k.code('Database')} abre {k.code('database/database.db')}, que a transferência traz com os registos que a
      tua app guardou.
    </>
  ),
  awayAside:
    'Convém saber antes de começares: o que escreves é teu e sai daqui inteiro. Correr nesta máquina não o prende a ela.',

  open: (k) => (
    <>
      Se o que criaste pode ajudar outras pessoas, publica o código: abre {k.b('Código aberto')} no painel de controlo,
      escolhe uma licença (a MIT, a não ser que queiras outra) e liga a publicação. O código ganha uma página própria
      entre as {k.link('/source', 'apps de código aberto')}, onde qualquer pessoa o pode ler, dar-lhe uma estrela e
      transferir qualquer versão como o mesmo projeto que {k.b('Transferir como projeto .NET')} te dá, com a licença ao
      lado.
    </>
  ),
  open2: () => (
    <>
      Todas as versões são publicadas, incluindo as anteriores, com a documentação, os testes e a alteração que cada
      uma fez. O que a app guarda nunca é publicado (os registos, os ficheiros que guardou, os valores das chaves e
      palavras-passe), nem o que pediste pelas tuas próprias palavras, nem quem usa a app. Desliga a publicação e a
      página desaparece; as estrelas ficam guardadas para quando a voltares a ligar.
    </>
  ),
  openAside:
    'Tudo o que está no código fica público, incluindo as versões anteriores. Uma chave ou uma palavra-passe fica junto das chaves e palavras-passe em Dados, nunca no código, publicado ou não.',

  agents: (k) => (
    <>
      Há um endpoint MCP em {k.code('/mcp')}. Liga-lhe um agente e ele pode fazer tudo o que o editor faz: ler o guia, ler
      uma demo inteira, escrever ficheiros, compilá-los e fazer deploy. Por baixo, é a mesma API.
    </>
  ),
  agents2: (k) => (
    <>
      O agente explica o porquê à medida que avança ({k.code('write_code')} recebe a especificação e a alteração) e pode
      ver o que publicou: {k.code('read_logs')} responde com os pedidos recentes da lambda, o que ela escreveu na consola
      e o stack trace de tudo o que lançou. É assim que um agente descobre que o código funciona, em vez de o assumir. Tu
      vês o mesmo no painel de controlo. Escreve a documentação e os testes à medida que avança, lê-os antes de alterar
      o que quer que seja, e corre os testes no endereço de um rascunho antes de o pôr online. Uma página feita para
      ser encontrada recebe um título, uma descrição, um ícone e uma pré-visualização para quando alguém partilha o
      link. No fim das páginas que cria, acrescenta uma pequena linha a dizer que foram feitas com o GenHTTP Lambda;
      diz-lhe se preferires que não a ponha, e ele retira-a.
    </>
  ),
  more: 'Saber mais →',
  make: 'Criar uma',
};
