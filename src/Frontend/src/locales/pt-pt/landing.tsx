import type { Messages } from '../en';

export const landing: Messages['landing'] = {
  eyebrow: 'Apps com IA e alojamento grátis',
  headline: 'Descreve uma app.',
  // a word joiner after the hyphen keeps «põe-na» on one line
  headlineAccent: 'O teu agente põe-\u2060na online.',
  intro:
    'Sondagens, livros de visitas, rankings, jogos multijogador. Descreve o que queres ao nosso agente de IA, ou ao que já usas, e recebe uma app a funcionar, alojada por nós, com um link para partilhar. E continua editável: podes afiná-la muito depois da primeira versão.',
  build: 'Criar uma app',
  ownAgent: 'Usar o teu agente',
  free: 'Grátis. Sem registo, sem cartão de crédito, nada para instalar.',
  seeIt: 'Vê como funciona',

  videoTitle: 'De uma frase a uma app online',
  videoText:
    'Uma janela anónima, sem conta, e um único pedido na página de criação. Depois, a app pronta, aberta pelo link, tal como qualquer visitante a abriria.',
  videoNote: 'A criação está acelerada. Tudo o resto é em tempo real.',
  tryIt: 'Experimenta tu',

  oneShotTitle: 'Um gerador de apps que não fica pela primeira versão',
  oneShotText:
    'A maioria dos geradores dá-te um resultado e deixa-te a braços com ele. Aqui, a app fica a correr onde foi criada, por isso tu e o teu agente podem continuar a trabalhar nela.',
  steps: [
    {
      title: 'Diz o que queres',
      body: 'Explica por palavras tuas, ao agente deste site ou ao que já usas. Sem código, sem configurações, sem conta.',
      alt: 'A página de criação com o pedido de uma sondagem para o almoço',
    },
    {
      title: 'Recebe uma app a funcionar e um link',
      body: 'A app é criada, alojada e chega-te como um endereço público para partilhares. Sem servidor, plano de alojamento, domínio ou base de dados para configurar: essa parte é connosco. Guarda os dados (votos, pontuações, mensagens), por isso quem a abre vê sempre o mesmo.',
      alt: 'A sondagem do almoço pronta, aberta no browser',
    },
    {
      title: 'Continua a melhorá-la',
      body: 'Cada app vem com um link de edição privado. Dá-o ao teu agente com a próxima alteração, ou abre-o tu. Cada alteração passa a ser uma nova versão, e o endereço mantém-se.',
      alt: 'O painel de controlo da sondagem: as versões, cada uma com o que foi pedido, o que mudou e a diferença para a anterior',
    },
  ],
  weekLater: 'Uma semana depois',
  weekAsk:
    'Aqui está o link de edição da minha sondagem do almoço. Às sextas, fecha a votação às 11h e mostra o vencedor lá em cima, por favor.',
  weekAnswer:
    'Feito. A versão 4 já está online, no mesmo endereço, e a versão 3 continua disponível se quiseres voltar atrás.',

  agentsTitle: 'Traz o teu próprio agente: Claude, Codex, Cursor',
  agentsText:
    'Já fazes vibe coding com o Claude Code, o Codex, o Cursor ou outro assistente? Liga-o a este endereço, um servidor MCP remoto, sem chave, e ele passa a criar, publicar e atualizar apps aqui, diretamente na conversa que já tens aberta.',
  thenAsk: (em) => (
    <>Depois, é só pedir: {em('cria uma lista de inscrições para o evento da equipa e põe-na online')}.</>
  ),
  hostIt: (link) => (
    <>Criaste algo que só corre em localhost? {link('Publica aqui a tua app feita com vibe coding')}.</>
  ),

  contactTitle: 'Fala connosco',
  contactText:
    'Precisas de ajuda, estás a planear algo maior ou procuras uma solução feita à medida? Teremos todo o gosto em falar contigo.',
  mailTitle: 'Envia-nos um email',
  mailText: 'Para projetos, pedidos de informação e tudo o que preferires tratar em privado.',
  discordTitle: 'Junta-te ao Discord',
  discordText: 'Mostra o que criaste, pede ajuda para o próximo passo e fala diretamente com a equipa.',
  discordLink: 'O Discord do GenHTTP',
};
