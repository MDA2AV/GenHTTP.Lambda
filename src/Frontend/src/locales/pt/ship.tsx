import type { Messages } from '../en';

export const ship: Messages['ship'] = {
  title: 'Do seu notebook para a tela de todo mundo.',
  intro:
    'Você criou algo com seu agente de código, mas ele só roda na sua máquina. Peça para o agente publicar aqui. Em poucos minutos, o app ganha um link público que qualquer pessoa pode abrir. E ele guarda dados: as pessoas podem jogar, conversar e postar juntas.',
  facts: ['Grátis', 'Sem conta', 'Nada para instalar'],
  connect: 'Conectar seu agente',
  seeOthers: 'Ver o que outros publicaram',

  stepsTitle: 'Três passos, e um deles é uma frase',
  step: (n) => `Passo ${n}`,
  steps: [
    {
      title: 'Conecte uma vez',
      body: 'Adicione um endereço ao Claude, ao Cursor ou ao agente que você usa. Leva menos de um minuto, e é só uma vez.',
    },
    {
      title: 'Peça para publicar',
      body: 'Diga para ele colocar o app no ar aqui. Ele empacota o app, publica e confere se está respondendo.',
    },
    {
      title: 'Compartilhe o link',
      body: 'Você recebe um endereço público e um link de edição privado. Mande o primeiro para quem quiser. Guarde o segundo: é com ele que você muda o app depois.',
    },
  ],

  togetherTitle: 'Não é só uma página. É um ponto de encontro.',
  together:
    'A maioria das hospedagens entrega uma cópia do app para cada visitante, e cada um joga sozinho. Aqui, todo app tem memória própria e uma conexão ao vivo com todo mundo que está com ele aberto. O que uma pessoa faz aparece na hora para as outras, e o que elas postam continua lá amanhã.',
  together2:
    'Sem banco de dados para contratar, sem outro serviço para integrar. Peça do jeito que você explicaria para um amigo.',
  kinds: [
    { name: 'Jogos multiplayer', ask: 'Deixa até oito amigos entrarem na mesma rodada e verem as jogadas uns dos outros ao vivo.' },
    { name: 'Salas de chat', ask: 'Adiciona uma sala onde todo mundo com o link pode conversar, e guarda as últimas cem mensagens.' },
    { name: 'Listas compartilhadas', ask: 'Transforma a lista do que levar em uma lista que a equipe toda edita ao mesmo tempo.' },
    { name: 'Placares e recordes', ask: 'Cria um ranking com o melhor tempo de cada um e mostra o top 10 na tela inicial.' },
    { name: 'Mini redes sociais', ask: 'Deixa os convidados do casamento postarem fotos num mural e curtirem as dos outros.' },
  ],
  quote: (text) => `“${text}”`,

  connectTitle: 'Conecte seu agente uma vez',
  connectText: 'Passe este endereço para o seu agente. A partir daí, ele sabe publicar aqui, sem chave e sem login.',
  sayLike: 'Depois, no seu projeto, diga algo como',
  asks: [
    'Publica este app no GenHTTP Lambda e me manda o link.',
    'Deixa os recordes compartilhados, para todo mundo ver o mesmo ranking.',
  ],

  domainChip: 'Quando fizer sucesso',
  domainTitle: 'Um domínio só dele',
  domainText:
    'O mesmo app, o mesmo link de edição, mas em um endereço que é seu. Mais fácil de falar, mais fácil de lembrar, e com cara de profissional quando as pessoas começarem a compartilhar.',
  domainSubject: 'Um domínio para o meu app',
  domainAsk: 'Pergunte sobre seu domínio',

  questionsTitle: 'Antes que você pergunte',
  questions: (offline, removed, showcase, terms) => [
    [
      'É grátis mesmo?',
      <>
        Sim. Sem cartão, sem período de teste e sem conta. Seu app fica no ar enquanto as pessoas usarem. Depois de{' '}
        {offline} dias sem nenhuma visita ou mudança, ele sai do ar, e depois de {removed} dias é removido.
      </>,
    ],
    [
      'Meu app precisa ser feito de um jeito específico?',
      'Não, seu agente cuida disso. Páginas, imagens e estilos sobem do jeito que estão, e o que precisa rodar no servidor o agente adapta para esta plataforma. Você descreve o que o app deve fazer, e ele faz a tradução.',
    ],
    [
      'Como eu mudo o app depois?',
      'Com o link de edição que você recebeu ao publicar. Passe para o seu agente junto com a próxima mudança, ou abra no navegador. Cada mudança vira uma nova versão no mesmo endereço, e você pode voltar para uma versão anterior quando quiser.',
    ],
    [
      'Quem pode ver meu app?',
      <>
        Qualquer pessoa que tiver o link. O app não fica listado em lugar nenhum, a não ser que você decida mostrar
        na {showcase('vitrine')}.
      </>,
    ],
    [
      'Tem algo que eu não posso publicar?',
      <>Poucas coisas, como algo que prejudique ou engane pessoas. Os {terms('termos')} são curtos e em linguagem simples.</>,
    ],
  ],

  closeTitle: 'Funciona na sua máquina.',
  closeAccent: 'Agora, na de todo mundo.',
  noAgent: 'Sem agente? Crie aqui',
  closeFacts: 'Grátis. Sem conta. Nada para instalar.',

  scene: {
    label: 'Um agente recebe o pedido de publicar um app. O endereço muda de localhost para um link público, e as pessoas entram.',
    ask: 'Coloca meu quiz no ar para os meus amigos entrarem.',
    live: 'Pronto, está no ar. Aqui está o link.',
    publishing: 'Publicando…',
    public: 'Público',
    onlyYou: 'Só você',
    app: 'Quiz de sexta',
    playing: (count) => <>{count} jogando</>,
    you: 'Você',
  },

  compareTitle: 'O caminho mais curto entre “funciona” e “testa aí”',
  compareText:
    'Vercel, Cloudflare e Lovable são ótimos lugares para rodar coisas. Mas todos começam com um cadastro. E, quando seu app precisa compartilhar algo entre visitantes, você ainda tem que configurar outro serviço. Veja como fica quando você começa do zero.',
  rows: [
    'Começar sem conta',
    'Publicar pelo agente que você já usa',
    'Dados compartilhados ao vivo: chat, multiplayer, recordes',
    'Custo até o primeiro link',
  ],
  us: ['Sim', 'Conecte uma vez e é só pedir', 'Já vem em todo app', 'Grátis'],
  rivals: [
    ['Precisa de cadastro', 'Com login nas ferramentas deles', 'Com um banco de dados à parte', 'Plano grátis'],
    ['Precisa de cadastro', 'Com login nas ferramentas deles', 'Possível, com configuração', 'Plano grátis'],
    ['Precisa de cadastro', 'Criado no editor deles', 'Por um backend conectado', 'Plano grátis, créditos limitados'],
  ],
  compareNote:
    'Em setembro de 2026, para quem não tem conta em lugar nenhum. Planos e recursos de outros serviços mudam; confira os detalhes com eles.',

  yourAgent: 'Seu agente',
  terminal: 'Terminal',
  setups: {
    claudeCode: 'Rode isto uma vez no terminal. Todo projeto que você abrir depois já pode publicar aqui.',
    claude: (strong) => (
      <>
        No Claude na web ou no desktop, abra {strong('Configurações')}, depois {strong('Connectors')}, e escolha{' '}
        {strong('Add custom connector')}. Cole o endereço acima e salve. Só isso.
      </>
    ),
    cursor: 'Adicione isto nas configurações de MCP do Cursor, ou no arquivo abaixo, e recarregue.',
    vscode: 'Salve isto no seu projeto e inicie o servidor pelo painel MCP do Copilot Chat.',
  },
  elsewhere:
    'Usa outra ferramenta? Windsurf, Codex, Zed e a maioria dos outros agentes aceitam um servidor MCP remoto nas configurações. Passe o endereço acima para eles.',
};
