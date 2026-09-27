import type { Messages } from '../en';

export const ship: Messages['ship'] = {
  title: 'Do seu computador para todas as telas.',
  intro:
    'Você criou algo com seu agente de programação, mas ele só funciona na sua máquina. Peça ao agente que o publique aqui. Poucos minutos depois, a aplicação terá um link público que qualquer pessoa pode abrir e poderá guardar dados, permitindo que várias pessoas joguem, conversem e publiquem nela em conjunto.',
  facts: ['Gratuito', 'Sem conta', 'Sem instalação'],
  connect: 'Conectar seu agente',
  seeOthers: 'Ver aplicações publicadas',

  stepsTitle: 'Três passos, e um deles é uma frase',
  step: (n) => `Passo ${n}`,
  steps: [
    {
      title: 'Conexão única',
      body: 'Adicione um endereço ao Claude, ao Cursor ou ao agente com que você trabalha. Leva menos de um minuto e é necessário apenas uma vez.',
    },
    {
      title: 'Solicitação de publicação',
      body: 'Peça ao agente que coloque a aplicação no ar aqui. Ele a empacota, publica e verifica se está respondendo.',
    },
    {
      title: 'Compartilhamento do link',
      body: 'Você recebe um endereço público e um link de edição privado. O primeiro pode ser compartilhado com qualquer pessoa; guarde o segundo, pois é com ele que você alterará a aplicação depois.',
    },
  ],

  togetherTitle: 'Mais do que uma página: um espaço compartilhado.',
  together:
    'A maioria das hospedagens entrega a cada visitante uma cópia própria da aplicação, e cada um a utiliza isoladamente. Aqui, cada aplicação tem sua própria memória e uma conexão em tempo real com todas as pessoas que a têm aberta. Uma ação de uma pessoa aparece imediatamente para as demais, e o que é publicado permanece salvo.',
  together2:
    'Sem banco de dados para contratar e sem serviços adicionais para integrar. Basta descrever a funcionalidade desejada.',
  kinds: [
    { name: 'Jogos multijogador', ask: 'Permita que até oito pessoas participem da mesma partida e vejam as jogadas umas das outras em tempo real.' },
    { name: 'Salas de bate-papo', ask: 'Adicione uma sala em que todos com o link possam conversar e guarde as últimas cem mensagens.' },
    { name: 'Listas compartilhadas', ask: 'Transforme a lista de bagagem em uma lista que toda a equipe possa editar ao mesmo tempo.' },
    { name: 'Pontuações e recordes', ask: 'Mantenha um ranking com o melhor tempo de cada pessoa e mostre os dez primeiros na tela inicial.' },
    { name: 'Pequenas comunidades', ask: 'Permita que os convidados do casamento publiquem fotos em um mural comum e curtam as dos demais.' },
  ],
  quote: (text) => `“${text}”`,

  connectTitle: 'Conecte seu agente uma única vez',
  connectText:
    'Informe este endereço ao seu agente. A partir daí, ele saberá publicar aqui, sem chave e sem login.',
  sayLike: 'Depois, no seu projeto, basta pedir algo como',
  asks: [
    'Publique esta aplicação no GenHTTP Lambda e me envie o link.',
    'Torne as pontuações compartilhadas, para que todos vejam o mesmo ranking.',
  ],

  domainChip: 'Quando a aplicação ganha escala',
  domainTitle: 'Um domínio próprio',
  domainText:
    'A mesma aplicação e o mesmo link de edição, mas em um endereço que pertence a você: mais fácil de divulgar, de memorizar e com uma apresentação mais profissional quando compartilhado.',
  domainSubject: 'Um domínio para minha aplicação',
  domainAsk: 'Solicitar um domínio próprio',

  questionsTitle: 'Perguntas frequentes',
  questions: (offline, removed, showcase, terms) => [
    [
      'É realmente gratuito?',
      <>
        Sim. Sem cartão, sem período de teste e sem conta. Sua aplicação permanece no ar enquanto for utilizada. Após{' '}
        {offline} dias sem nenhum acesso ou alteração, ela sai do ar e, após {removed} dias, é removida.
      </>,
    ],
    [
      'Minha aplicação precisa seguir uma estrutura específica?',
      'Não, seu agente cuida disso. Páginas, imagens e estilos são publicados como estão, e o agente adapta a esta plataforma tudo o que precisa ser executado no servidor. Você descreve o que a aplicação deve fazer, e o agente se encarrega da implementação.',
    ],
    [
      'Como faço alterações depois?',
      'Com o link de edição recebido na publicação. Entregue-o ao seu agente com a próxima alteração ou abra-o no navegador. Cada alteração se torna uma nova versão no mesmo endereço, e você pode voltar a uma versão anterior a qualquer momento.',
    ],
    [
      'Quem pode ver minha aplicação?',
      <>
        Qualquer pessoa a quem você enviar o link. Ela não é listada em lugar nenhum, a menos que você decida adicioná-la à{' '}
        {showcase('vitrine')}.
      </>,
    ],
    [
      'Há algo que não posso publicar?',
      <>
        Sim, por exemplo qualquer conteúdo que prejudique ou engane pessoas. Os {terms('termos de serviço')} são curtos e
        escritos de forma clara.
      </>,
    ],
  ],

  closeTitle: 'Funciona na sua máquina.',
  closeAccent: 'Faça funcionar para todos.',
  noAgent: 'Sem agente? Crie aqui',
  closeFacts: 'Gratuito. Sem conta. Sem instalação.',

  scene: {
    label:
      'Um agente recebe o pedido de publicar uma aplicação. O endereço muda de localhost para um link público, e outras pessoas entram.',
    ask: 'Coloque meu jogo de perguntas no ar para que meus amigos possam participar.',
    live: 'Está no ar. Este é o seu link.',
    publishing: 'Publicando…',
    public: 'Público',
    onlyYou: 'Local',
    app: 'Noite de perguntas de sexta',
    playing: 'online',
    you: 'Você',
  },

  compareTitle: 'O caminho mais curto de “funciona” para “experimente”',
  compareText:
    'Vercel, Cloudflare e Lovable são ótimas plataformas para executar aplicações. No entanto, exigem cadastro e, assim que sua aplicação precisa compartilhar dados entre visitantes, a configuração de um serviço adicional. Veja a comparação para quem começa do zero.',
  rows: [
    'Começar sem conta',
    'Publicar a partir do agente que você já usa',
    'Dados compartilhados em tempo real: chat, multijogador, recordes',
    'Custo até o primeiro link',
  ],
  us: ['Sim', 'Uma conexão, depois basta pedir', 'Incluído em cada aplicação', 'Gratuito'],
  rivals: [
    ['Cadastro necessário', 'Com ferramentas próprias, após login', 'Adicionar um serviço de banco de dados', 'Plano gratuito'],
    ['Cadastro necessário', 'Com ferramentas próprias, após login', 'Possível, com configuração', 'Plano gratuito'],
    ['Cadastro necessário', 'Em editor próprio', 'Por meio de um backend conectado', 'Plano gratuito, créditos limitados'],
  ],
  compareNote:
    'Situação em setembro de 2026, para quem não possui conta em nenhum serviço. Planos e recursos de outros serviços mudam; consulte os detalhes diretamente com cada um.',

  yourAgent: 'Seu agente',
  terminal: 'Terminal',
  setups: {
    claudeCode: 'Execute isto uma vez em um terminal. Todo projeto aberto depois poderá publicar aqui.',
    claude: (strong) => (
      <>
        No Claude na web ou no desktop, abra as {strong('Configurações')}, depois {strong('Connectors')}, e escolha{' '}
        {strong('Add custom connector')}. Cole o endereço acima e salve. Nenhuma outra etapa é necessária.
      </>
    ),
    cursor: 'Adicione isto às configurações MCP do Cursor, ou ao arquivo indicado abaixo, e recarregue.',
    vscode: 'Salve este arquivo no seu projeto e, em seguida, inicie o servidor na visualização MCP do Copilot Chat.',
  },
  elsewhere:
    'Usa outra ferramenta? Windsurf, Codex, Zed e a maioria dos agentes permitem adicionar um servidor MCP remoto nas configurações. Informe a eles o endereço acima.',
};
