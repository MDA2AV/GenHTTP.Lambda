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
    features: 'Mudando com segurança',
    files: 'Mais de um arquivo',
    page: 'Servindo uma página',
    spa: 'Um front-end, passo a passo',
    storage: 'Os dois lugares onde ficam os arquivos',
    keeping: 'Guardando dados',
    sockets: 'WebSockets',
    limits: 'O que não é permitido',
    away: 'Levando seu código embora',
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
    ['Visão geral', () => <>Se está no ar, quantas requisições teve hoje e quantas falharam, a última mudança e quanto espaço ainda sobra.</>],
    [
      'Mudar',
      (k) => (
        <>
          Diga o que deve ficar diferente e o agente deste servidor faz isso enquanto você acompanha. Ele trabalha num
          rascunho, testa lá e mescla na próxima versão quando funciona. Desligue{' '}
          {k.b('Colocar no ar quando terminar')} para testar o rascunho você mesmo antes.
        </>
      ),
    ],
    ['Rascunhos', () => <>Mudanças feitas ao lado da lambda: cada uma é testada num endereço próprio e mesclada na próxima versão quando estiver tudo certo. Aberto, um rascunho tem código, dados e log próprios.</>],
    ['Arquivos', () => <>Os arquivos de uma versão: o código e os assets, o próprio programa. Um cadeado ou um globo mostra se o público consegue acessar.</>],
    ['Dados', () => <>O que a lambda guarda enquanto roda, compartilhado por todas as versões: o workspace. Veja o que tem nele, envie e exclua arquivos, ou desligue.</>],
    ['Versões', () => <>O que cada versão mudou, o que foi pedido e a diferença para a anterior. Faça deploy ou volte uma versão por aqui, ou comece um rascunho a partir de qualquer uma delas.</>],
    ['Deploys', () => <>O que esteve no ar e quando, e o que tirou do ar.</>],
    ['Métricas', () => <>Requisições, falhas, tempos de resposta e os caminhos mais acessados, na última hora ou nas últimas 24 horas.</>],
    ['Logs', () => <>As requisições, o que ela imprimiu e o stack trace de qualquer erro, em tempo real.</>],
    [
      'Código',
      (k) => (
        <>
          Para escrever à mão. {k.b('Verificar')} compila, {k.b('Salvar')} cria uma versão, {k.b('Fazer deploy')} coloca
          no ar. Num rascunho, {k.b('Salvar')} mantém a mudança no rascunho e {k.b('Fazer deploy da prévia')} coloca no
          ar no endereço do rascunho. {k.code('Ctrl-S')} salva; {k.code('F12')} vai para uma declaração.
        </>
      ),
    ],
  ],
  sections: (k) => (
    <>
      Toda seção funciona do mesmo jeito: o título, um {k.b('ⓘ')} que explica, as ações à direita e, quando há mais de
      uma visualização, uma fileira de abas embaixo. No código, as abas são os arquivos.
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
    change: 'Guarda as mensagens no workspace para não sumirem num reinício',
  },
  why2: (k) => (
    <>
      Os agentes passam os mesmos dois campos para {k.code('write_code')}. Em {k.b('Código')}, salvar pede a mudança.
      Os dois são opcionais. Uma especificação longa é cortada em 4.000 caracteres, e uma mudança em 500, em vez de ser
      recusada. Um rascunho tem os próprios dois campos, e a versão em que ele é mesclado fica com eles.
    </>
  ),

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
        Comece em {k.b('Rascunhos')}, ou a partir de qualquer versão. Ele é uma cópia do código e dos assets dessa
        versão, e dos dados da lambda.
      </>
    ),
    (k) => (
      <>
        Mude quantas vezes precisar, em {k.b('Código')} ou pedindo ao agente. {k.b('Fazer deploy da prévia')} coloca no
        ar num endereço próprio, {k.code('/features/…/')}, com a própria cópia dos dados. Os visitantes da lambda não
        veem nada disso, e nada do que ele grava chega aos dados da lambda.
      </>
    ),
    (k) => (
      <>
        Quando estiver tudo certo, {k.b('Mesclar')} faz dele a próxima versão, com as notas dele, e coloca no ar na
        hora, se você quiser. O rascunho some junto, com a prévia e a cópia dos dados.
      </>
    ),
  ],
  featureSample: 'Ranking',
  featuresAside: () => (
    <>
      Dá para trabalhar em vários rascunhos ao mesmo tempo. Só um baseado na versão mais recente pode ser mesclado, para
      que uma mesclagem nunca desfaça uma versão salva depois que o rascunho começou. Se outro foi mesclado antes,
      traga as mudanças dele (ou peça ao agente) e depois baseie o rascunho na versão mais recente. Nada é mesclado
      sozinho; isso é de propósito.
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
    ['o que guarda', 'o código e os assets: o programa, inclusive o front-end', 'tudo o que a lambda grava ou alguém envia'],
    ['quando muda', 'nunca: uma mudança é uma nova versão', 'no momento em que algo é gravado'],
    ['um deploy', 'coloca exatamente esses arquivos no ar', 'nunca mexe neles'],
    ['voltar uma versão', 'traz os arquivos antigos de volta', 'nenhum efeito: são os mesmos para todas as versões'],
    ['um rascunho', 'começa como uma cópia deles', 'trabalha numa cópia deles'],
    ['quando somem', 'com as versões antigas, depois do limite', 'com a lambda, ou quando você desliga'],
  ],
  reachedAs: 'acessado no código como',
  storageAside:
    'Os dois não podem ser um lugar só. Se fossem, um deploy ou apagaria tudo o que a lambda gravou desde então, ou nada nunca poderia sair dos arquivos que vão com ela. Um jogo com ranking quer a segunda opção; a página que ele serve quer a primeira. Então a página fica na versão, e o ranking nos dados.',

  keeping: (k) => (
    <>
      {k.code('Workspace')} é um diretório privado que sua lambda pode ler e gravar. É o lugar para tudo o que precisa
      durar mais que uma requisição, ou que um deploy.
    </>
  ),
  keeping2: (k) => (
    <>
      Também tem {k.code('ReadBytes')}, {k.code('WriteBytes')}, {k.code('Delete')}, {k.code('List')},{' '}
      {k.code('CreateFolder')} e {k.code('Tree')}/{k.code('Files')}/{k.code('App')} para servir o conteúdo. Nada mais
      no sistema de arquivos é acessível.
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

  limits:
    'Seu código roda em um servidor compartilhado, então parte do C# é recusada antes de compilar: iniciar processos, abrir seus próprios sockets, carregar assemblies, acessar o sistema de arquivos fora do seu workspace e usar reflection para contornar qualquer uma dessas regras.',
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
    </>
  ),
  awayAside:
    'Bom saber antes de criar qualquer coisa aqui: o que você escreve é seu e sai inteiro. Rodar nesta máquina não prende seu código a ela.',

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
      acompanha tudo isso no painel de controle.
    </>
  ),
  more: 'Saiba mais →',
  make: 'Criar uma lambda',
};
