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
    files: 'Mais do que um ficheiro',
    page: 'Servir uma página',
    spa: 'Um front-end, passo a passo',
    storage: 'Os dois sítios onde vivem os ficheiros',
    keeping: 'Guardar dados',
    sockets: 'WebSockets',
    limits: 'O que não podes fazer',
    away: 'Levar o código contigo',
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
    ['Visão geral', () => <>Se está online, quantos pedidos teve hoje e quantos falharam, a última alteração e quanto espaço ainda sobra.</>],
    [
      'Alterar',
      (k) => (
        <>
          Diz o que deve ficar diferente e o agente deste servidor trata disso enquanto acompanhas: lê o código, altera-o,
          confirma que compila e põe-no online como uma nova versão. Desliga {k.b('Pôr online quando terminar')}{' '}
          para o veres primeiro.
        </>
      ),
    ],
    ['Ficheiros', () => <>Os ficheiros de uma versão e os dados: o que a lambda guarda enquanto corre. Um cadeado ou um globo indica se o público lhes consegue aceder.</>],
    ['Versões', () => <>O que cada versão mudou, o que foi pedido e a diferença para a anterior. Faz deploy ou reverte a partir daqui.</>],
    ['Deploys', () => <>O que esteve online e quando, e o que o pôs offline.</>],
    ['Estatísticas', () => <>Pedidos, falhas, tempos de resposta e os caminhos mais pedidos, na última hora ou nas últimas 24 horas.</>],
    ['Logs', () => <>Os pedidos, o que a lambda escreveu na consola e o stack trace de tudo o que correu mal, em tempo real.</>],
    [
      'Código',
      (k) => (
        <>
          Para o escrever à mão. {k.b('Verificar')} compila, {k.b('Guardar')} cria uma versão, {k.b('Fazer deploy')} põe-na
          online. {k.code('Ctrl-S')} guarda; {k.code('F12')} vai para a declaração.
        </>
      ),
    ],
  ],
  sections: (k) => (
    <>
      Todas as secções funcionam da mesma forma: o título, um {k.b('ⓘ')} que a explica, as ações à direita e, quando há
      mais do que uma vista, uma fila de separadores por baixo. No código, os separadores são os ficheiros.
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
    change: 'Guarda as entradas no workspace para sobreviverem a um reinício',
  },
  why2: (k) => (
    <>
      Os agentes passam os mesmos dois campos a {k.code('write_code')}. Em {k.b('Código')}, ao guardares, é-te pedida a
      alteração. Ambos são opcionais: em vez de ser recusada, uma especificação longa é cortada aos 4000 caracteres, e
      uma alteração aos 500.
    </>
  ),

  files: (k) => (
    <>
      Os tipos não têm de ficar por baixo do código que os usa. Em {k.b('Código')}, clica em {k.b('+')} ao lado dos
      ficheiros: o novo ficheiro é compilado ao lado do snippet, no mesmo namespace, por isso não é preciso importar nada.
      Um nome sem extensão é tratado como C#.
    </>
  ),

  page: 'Há três formas, e a que queres depende de onde a página está.',
  inlineTitle: 'Uma página, escrita no código',
  inline: 'Serve para algo pequeno. A página faz parte do snippet.',
  folderTitle: 'Uma pasta de ficheiros a sério',
  folder:
    'O que queres para qualquer coisa com uma folha de estilos e um script. Os ficheiros são adicionados da mesma forma que um ficheiro C#, e servidos exatamente como foram escritos. Nada os compila.',
  workspaceTitle: 'A partir do workspace',
  workspace: 'Quando a página é carregada em vez de escrita, e deve poder mudar sem um novo deploy.',

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
      A secção {k.b('Ficheiros')} mostra os dois (os ficheiros de uma versão e o workspace, como {k.b('Dados')}) e diz a
      quais o público consegue aceder. Os ficheiros do código alteram-se em {k.b('Código')}; os dados podem ser
      carregados e eliminados em {k.b('Ficheiros')}. Mas não são a mesma coisa, e a diferença está em{' '}
      {k.em('quando cada um muda')}.
    </>
  ),
  savedWithCode: 'Guardado com o código',
  workspaceColumn: 'Workspace',
  table: [
    ['o que guarda', 'todos os ficheiros da lambda, incluindo o C#', 'tudo o que foi escrito ou carregado'],
    ['quando muda', 'quando clicas em Guardar ou Fazer deploy', 'no momento em que algo é escrito nele'],
    ['um deploy', 'substitui tudo', 'nunca lhe toca'],
    ['reverter uma versão', 'traz de volta os ficheiros antigos', 'não tem efeito'],
    ['clonar a lambda', 'vai junto', 'não vai'],
  ],
  reachedAs: 'acedido no código como',
  storageAside:
    'Não podem ser um só diretório. Se fossem, um deploy ou apagava tudo o que a lambda escreveu entretanto, ou nunca se poderia remover nada do que ela traz. Um jogo com um ranking quer a segunda opção; a página que ele serve quer a primeira.',

  keeping: (k) => (
    <>
      {k.code('Workspace')} é um diretório privado onde a tua lambda pode ler e escrever. É o sítio para tudo o que tenha
      de durar mais do que um pedido, ou do que um deploy.
    </>
  ),
  keeping2: (k) => (
    <>
      Há também {k.code('ReadBytes')}, {k.code('WriteBytes')}, {k.code('Delete')}, {k.code('List')},{' '}
      {k.code('CreateFolder')} e {k.code('Tree')}/{k.code('Files')}/{k.code('App')} para o servir. Mais nada no sistema de
      ficheiros é acessível.
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

  limits:
    'O teu código corre num servidor partilhado, por isso parte do C# é recusada antes de compilar: iniciar processos, abrir sockets próprios, carregar assemblies, aceder ao sistema de ficheiros fora do teu workspace, e usar reflection para contornar qualquer uma destas regras.',
  limits2:
    'Todo o resto está lá, incluindo toda a API de módulos do GenHTTP. Se algo for recusado, ficas a saber em que linha e porquê, e não apenas que falhou.',

  away: (k) => (
    <>
      {k.b('Transferir como projeto .NET')}, no editor, dá-te tudo de uma vez: uma solução que podes abrir, correr com{' '}
      {k.code('dotnet run')} e guardar. Tem uma única referência de pacote e nenhum vestígio desta plataforma.
    </>
  ),
  away2: (k) => (
    <>
      O teu snippet passa a ser o corpo de {k.code('Program.cs')}, dentro de um host que serve o que ele devolve. Os teus
      outros ficheiros vêm exatamente como os escreveste. {k.code('Workspace')} e {k.code('Assets')} passam a ser duas
      pastas ao lado do código, com os mesmos métodos, por isso não tens de mudar nada no teu código.
    </>
  ),
  awayAside:
    'Convém saber antes de começares: o que escreves é teu e sai daqui inteiro. Correr nesta máquina não o prende a ela.',

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
      vês o mesmo no painel de controlo.
    </>
  ),
  more: 'Saber mais →',
  make: 'Criar uma',
};
