import type { Messages } from '../en';

export const guide: Messages['guide'] = {
  title: 'Como funciona',
  intro:
    'Você escreve um trecho de C#. O que ele retornar fica hospedado em um endereço público, via HTTPS, em poucos segundos. Esta página apresenta toda a plataforma, na ordem em que você vai conhecê-la.',
  contents: 'Conteúdo',

  parts: {
    what: 'O que é um lambda',
    first: 'Seu primeiro lambda',
    editor: 'O centro de controle',
    why: 'Documentar as alterações',
    files: 'Vários arquivos',
    page: 'Servir uma página',
    spa: 'Um front-end, passo a passo',
    storage: 'Os dois locais onde ficam os arquivos',
    keeping: 'Guardar dados',
    sockets: 'WebSockets',
    limits: 'Restrições',
    away: 'Exportar o código',
    agents: 'Trabalhar com um agente',
  },

  what: [
    (k) => (
      <>
        Um lambda é um trecho de código que retorna um handler do GenHTTP. A plataforma o compila, o carrega e publica o
        resultado no seu próprio endereço. Não há projeto, arquivo de build nem instrução {k.code('using')}: todos os
        módulos do GenHTTP já estão importados.
      </>
    ),
    (k) => (
      <>
        Isso já é um lambda completo. Implantado em {k.code('/lambda/your-key/')}, ele responde a cada requisição com a
        palavra “hello”.
      </>
    ),
  ],
  whatAside: (k) => (
    <>
      O trecho é composto de {k.em('instruções')}, não de uma classe. A última coisa que ele faz é retornar algo capaz de
      atender requisições: um handler ou um builder de handler.
    </>
  ),

  first: [
    (k) => (
      <>
        Clique em {k.b('Criar lambda')}. Você recebe um endereço público e uma chave de edição. A chave é o único acesso e
        ninguém pode recuperá-la por você, portanto guarde-a.
      </>
    ),
    () => (
      <>
        Você é direcionado ao centro de controle, com um pequeno serviço REST já escrito como primeira versão. Ele serve
        apenas como ponto de partida.
      </>
    ),
    (k) => (
      <>
        Entregue a chave de edição a um agente e descreva o que ele deve criar – ele escreve novas versões via{' '}
        {k.link('/#agents', 'MCP')}. Ou abra {k.b('Código')} e escreva você mesmo: {k.b('Verificar')} compila sem salvar
        nada e mostra as mensagens do compilador, com arquivo e linha.
      </>
    ),
    (k) => (
      <>
        Clique em {k.b('Implantar')}. Agora ele está no ar; antes disso, nada fica acessível. Cada nova implantação
        prolonga o tempo em que ele permanece no ar.
      </>
    ),
  ],

  editor: (k) => (
    <>
      O link de edição abre um centro de controle em vez de um simples editor de texto: a maior parte do código aqui é
      escrita por agentes, então a primeira informação exibida é o estado do seu lambda. A barra lateral mostra se ele está
      no ar, seu endereço, um botão quando há uma versão mais recente aguardando implantação e suas seções. Ações pouco
      frequentes, como alterar o endereço ou excluí-lo, ficam no menu {k.b('⋯')}.
    </>
  ),
  bits: [
    ['Visão geral', () => <>Se está no ar, quantas requisições recebeu hoje e quantas falharam, a última alteração e o espaço disponível.</>],
    ['Arquivos', () => <>Os arquivos de uma versão e seus dados – o que o lambda salva durante a execução. Um cadeado ou um globo indica se estão acessíveis publicamente.</>],
    ['Versões', () => <>O que cada versão alterou, o que foi solicitado e a diferença em relação à anterior. Implante ou reverta a partir daqui.</>],
    ['Implantações', () => <>O que esteve no ar, quando, e o que o tirou do ar.</>],
    ['Estatísticas', () => <>Requisições, falhas, tempos de resposta e os caminhos mais acessados, na última hora ou nas últimas 24 horas.</>],
    ['Logs', () => <>Suas requisições, o que ele imprimiu e o stack trace de qualquer erro, em tempo real.</>],
    [
      'Código',
      (k) => (
        <>
          Edição manual. {k.b('Verificar')} compila, {k.b('Salvar')} cria uma versão e {k.b('Implantar')} a coloca no ar.{' '}
          {k.code('Ctrl+S')} salva; {k.code('F12')} vai para uma declaração.
        </>
      ),
    ],
  ],
  sections: (k) => (
    <>
      Todas as seções funcionam da mesma forma: um título, um {k.b('ⓘ')} com a explicação, as ações à direita e – quando há
      mais de uma visualização – uma fileira de seletores abaixo. No código, esses seletores são os arquivos.
    </>
  ),
  editorAside:
    'O tráfego e o log são mantidos em memória, para acompanhamento e não para arquivamento: uma reinicialização do servidor os zera. As versões e o histórico de implantações são armazenados de forma permanente.',

  why: (k) => (
    <>
      Uma versão é o código e, opcionalmente, duas anotações: {k.b('a especificação')}, ou seja, o que o usuário deseja e
      por quê, de preferência com as palavras dele, e {k.b('a alteração')}, uma linha sobre o que a versão faz. Elas são
      exibidas ao lado do diff no histórico de versões, para que o {k.em('porquê')} permaneça junto ao {k.em('quê')} –
      para você e para o próximo agente que ler o histórico antes de fazer alterações.
    </>
  ),
  whySample: {
    specification: 'Um livro de visitas que as pessoas possam assinar; as entradas devem sobreviver a uma reinicialização',
    change: 'Guarda as entradas no workspace para que sobrevivam a uma reinicialização',
  },
  why2: (k) => (
    <>
      Os agentes enviam os mesmos dois campos para {k.code('write_code')}. Em {k.b('Código')}, ao salvar, a alteração é
      solicitada. Ambos são opcionais; uma especificação longa é cortada em 4000 caracteres e uma alteração em 500, em vez
      de ser recusada.
    </>
  ),

  files: (k) => (
    <>
      Os tipos não precisam ficar abaixo do código que os utiliza. Em {k.b('Código')}, clique em {k.b('+')} ao lado dos
      arquivos: o novo arquivo é compilado junto com o trecho, no mesmo namespace, sem necessidade de importação. Um nome
      sem extensão é tratado como C#.
    </>
  ),

  page: 'Há três maneiras; a adequada depende de onde a página está.',
  inlineTitle: 'Uma página escrita no próprio código',
  inline: 'Adequada para algo pequeno. A página faz parte do trecho.',
  folderTitle: 'Uma pasta de arquivos',
  folder:
    'A opção indicada para qualquer coisa com folha de estilo e script. Os arquivos são adicionados da mesma forma que um arquivo C# e servidos exatamente como foram escritos, sem compilação.',
  workspaceTitle: 'A partir do workspace',
  workspace: 'Quando a página é enviada em vez de escrita e deve poder mudar sem uma nova implantação.',

  spa: (k) => (
    <>
      A segunda opção, em detalhes. Todas as demos servem sua página dessa forma a partir de uma pasta chamada{' '}
      {k.code('web')} – abra {k.link('/editor/demo-crud', 'demo-crud')} para ver um exemplo. As demos são somente leitura;
      a chave de edição delas é o próprio nome.
    </>
  ),
  spaSteps: [
    (k) => (
      <>
        Em {k.b('Código')}, clique em {k.b('+')} ao lado dos arquivos e digite {k.code('site/index.html')}. Um nome com
        barra coloca o arquivo em uma pasta; um nome com extensão é tratado como o tipo de arquivo indicado.
      </>
    ),
    (k) => (
      <>
        Adicione {k.code('site/app.css')} e {k.code('site/app.js')} da mesma forma. Sua página se refere a eles pelo nome,
        como em {k.code('href="app.css"')}, pois a pasta é a raiz do que é servido e não faz parte do endereço.
      </>
    ),
    (k) => (
      <>
        Para arquivos que não são texto, como uma imagem ou uma fonte, abra um arquivo em {k.code('site')} e use o botão de
        envio ao lado dos arquivos: ele será colocado na mesma pasta.
      </>
    ),
    (k) => <>Em {k.code('lambda.cs')}, sirva a pasta:</>,
    (k) => (
      <>
        Clique em {k.b('Implantar')}. {k.code('site/index.html')} responde em {k.code('/')}, {k.code('site/app.css')} em{' '}
        {k.code('/app.css')}, e qualquer endereço sem arquivo correspondente é respondido com a página – assim, um
        front-end com roteamento próprio continua funcionando quando alguém recarrega um link interno.
      </>
    ),
    () => <>Adicione uma API ao lado para que a página tenha com o que se comunicar:</>,
  ],

  storage: (k) => (
    <>
      A seção {k.b('Arquivos')} mostra ambos – os arquivos de uma versão e o workspace como {k.b('Dados')} – e informa
      quais são acessíveis publicamente. Os arquivos do código são alterados em {k.b('Código')}; os dados podem ser
      enviados e excluídos em {k.b('Arquivos')}. Porém, eles não são a mesma coisa, e a diferença está em{' '}
      {k.em('quando cada um muda')}.
    </>
  ),
  savedWithCode: 'Salvo com o seu código',
  workspaceColumn: 'Workspace',
  table: [
    ['o que contém', 'todos os arquivos do seu lambda, incluindo o C#', 'tudo o que foi escrito ou enviado'],
    ['quando muda', 'ao clicar em Salvar ou Implantar', 'no momento em que algo é gravado'],
    ['uma implantação', 'substitui tudo', 'não o altera'],
    ['reverter uma versão', 'restaura os arquivos anteriores', 'nenhum efeito'],
    ['clonar o lambda', 'é copiado', 'não é copiado'],
  ],
  reachedAs: 'acessado no código como',
  storageAside:
    'Eles não podem ser um único diretório. Se fossem, uma implantação apagaria tudo o que o lambda gravou desde então, ou nada jamais poderia ser removido do que ele publica. Um jogo com ranking precisa da segunda opção; a página que ele serve, da primeira.',

  keeping: (k) => (
    <>
      {k.code('Workspace')} é um diretório privado que seu lambda pode ler e gravar. É o lugar para tudo o que precisa
      durar além de uma requisição ou de uma implantação.
    </>
  ),
  keeping2: (k) => (
    <>
      Também estão disponíveis {k.code('ReadBytes')}, {k.code('WriteBytes')}, {k.code('Delete')}, {k.code('List')},{' '}
      {k.code('CreateFolder')} e {k.code('Tree')}/{k.code('Files')}/{k.code('App')} para servi-lo. O restante do sistema
      de arquivos não é acessível.
    </>
  ),

  sockets: (k) => (
    <>
      Totalmente suportados. A demo {k.link('/editor/demo-game', 'demo-game')} forma pares de jogadores e executa cada
      partida no servidor. A forma mais simples consiste em três callbacks:
    </>
  ),
  socketsAside: (k) => (
    <>
      Um ponto que merece atenção: o navegador não consegue definir cabeçalhos no handshake de um WebSocket. Passe o que o
      handler precisa na query, onde ele lê a partir de {k.code('connection.Request.Header.Query')}, ou envie informações
      sigilosas na primeira mensagem.
    </>
  ),

  limits:
    'Seu código é executado em um servidor compartilhado, portanto parte do C# é recusada antes da compilação: iniciar processos, abrir sockets próprios, carregar assemblies, acessar o sistema de arquivos fora do seu workspace e usar reflexão para contornar essas restrições.',
  limits2:
    'Todo o restante está disponível, incluindo toda a API de módulos do GenHTTP. Se algo for recusado, você é informado da linha e do motivo.',

  away: (k) => (
    <>
      {k.b('Baixar como projeto .NET')}, no editor, entrega tudo como um projeto .NET: uma solução que você pode abrir,
      executar com {k.code('dotnet run')} e manter. Ela tem uma única referência de pacote e nenhuma dependência desta
      plataforma.
    </>
  ),
  away2: (k) => (
    <>
      Seu trecho se torna o corpo de {k.code('Program.cs')}, dentro de um host que serve o que ele retorna. Seus demais
      arquivos são mantidos exatamente como foram escritos. {k.code('Workspace')} e {k.code('Assets')} se tornam duas
      pastas ao lado do código, com os mesmos métodos, portanto nada no seu código precisa mudar.
    </>
  ),
  awayAside:
    'Vale saber antes de começar: o código que você escreve pertence a você e pode ser exportado integralmente. Executá-lo nesta plataforma não o vincula a ela.',

  agents: (k) => (
    <>
      Há um endpoint MCP em {k.code('/mcp')}. Um agente conectado a ele pode fazer tudo o que o editor faz: ler o guia,
      consultar uma demo completa, gravar arquivos, compilá-los e implantá-los. Ambos usam a mesma API.
    </>
  ),
  agents2: (k) => (
    <>
      O agente registra seus motivos ao longo do processo – {k.code('write_code')} recebe a especificação e a alteração – e
      pode verificar o que implantou: {k.code('read_logs')} retorna as requisições recentes do lambda, o que ele imprimiu e
      o stack trace de qualquer exceção. É assim que um agente confirma que seu código funciona, em vez de presumir. Você
      acompanha as mesmas informações no centro de controle.
    </>
  ),
  more: 'Saiba mais →',
  make: 'Criar um lambda',
};
