import type { Messages } from '../en';

export const ship: Messages['ship'] = {
  title: 'Do teu portátil para o ecrã de toda a gente.',
  intro:
    'Criaste algo com o teu agente de programação, mas só funciona na tua máquina. Pede ao agente que o publique aqui. Minutos depois, tens um link público que qualquer pessoa pode abrir. E a app guarda dados, por isso as pessoas podem jogar, conversar e publicar lá, todas juntas.',
  facts: ['Grátis', 'Sem conta', 'Nada para instalar'],
  connect: 'Ligar o teu agente',
  seeOthers: 'Ver o que os outros publicaram',

  stepsTitle: 'Três passos, e um deles é uma frase',
  step: (n) => `Passo ${n}`,
  steps: [
    {
      title: 'Liga uma vez',
      body: 'Adiciona um endereço ao Claude, ao Cursor ou ao agente com que trabalhas. Demora menos de um minuto e só tens de o fazer uma vez.',
    },
    {
      title: 'Pede-lhe que publique',
      body: 'Diz-lhe para pôr a app online aqui. Ele empacota a app, publica-a e confirma que responde.',
    },
    {
      title: 'Partilha o link',
      body: 'Recebes um endereço público e um link de edição privado. Envia o primeiro a quem quiseres. Guarda o segundo: é com ele que alteras a app mais tarde.',
    },
  ],

  togetherTitle: 'Não é só uma página. É um ponto de encontro.',
  together:
    'A maioria dos serviços de alojamento dá uma cópia da app a cada visitante, e cada um joga sozinho. Aqui, cada app tem memória própria e uma ligação em tempo real a toda a gente que a tem aberta. O que uma pessoa faz aparece logo aos outros, e o que publicam continua lá amanhã.',
  together2:
    'Não tens de te registar numa base de dados nem de ligar um segundo serviço. Pede como explicarias a um amigo.',
  kinds: [
    { name: 'Jogos multijogador', ask: 'Deixa até oito amigos entrarem na mesma partida e verem as jogadas uns dos outros em tempo real.' },
    { name: 'Salas de chat', ask: 'Adiciona uma sala onde toda a gente com o link pode conversar, e guarda as últimas cem mensagens.' },
    { name: 'Listas partilhadas', ask: 'Transforma a lista do que levar numa lista que a equipa toda edita ao mesmo tempo.' },
    { name: 'Pontuações e recordes', ask: 'Guarda uma tabela com o melhor tempo de cada um e mostra o top 10 no ecrã inicial.' },
    { name: 'Pequenas redes sociais', ask: 'Deixa os convidados do casamento publicarem fotos num mural e porem gosto nas fotos dos outros.' },
  ],
  quote: (text) => `«${text}»`,

  connectTitle: 'Liga o teu agente uma vez',
  connectText: 'Dá este endereço ao teu agente. A partir daí, ele sabe publicar aqui, sem chave e sem iniciar sessão.',
  sayLike: 'Depois, no teu projeto, diz algo como',
  asks: [
    'Publica esta app no GenHTTP Lambda e envia-me o link.',
    'Faz com que os recordes sejam partilhados, para toda a gente ver a mesma tabela.',
  ],

  domainChip: 'Quando fizer sucesso',
  domainTitle: 'Dá-lhe um nome só dela',
  domainText:
    'A mesma app, o mesmo link de edição, mas num endereço que é teu. Mais fácil de dizer, mais fácil de lembrar, e com ar profissional quando as pessoas a começarem a partilhar.',
  domainSubject: 'Um domínio para a minha app',
  domainAsk: 'Perguntar por um domínio',

  questionsTitle: 'Antes que perguntes',
  questions: (offline, removed, showcase, terms) => [
    [
      'É mesmo grátis?',
      <>
        Sim. Sem cartão, sem período experimental e sem conta. A tua app fica online enquanto houver quem a use. Ao fim
        de {offline} dias sem uma única visita ou alteração, fica offline, e ao fim de {removed} dias é removida.
      </>,
    ],
    [
      'A minha app tem de ser feita de uma forma específica?',
      'Não, o teu agente trata disso. Páginas, imagens e estilos vão tal como estão, e o que tiver de correr no servidor é adaptado pelo agente a esta plataforma. Tu descreves o que a app deve fazer e ele faz a tradução.',
    ],
    [
      'Como é que a altero mais tarde?',
      'Com o link de edição que recebeste quando a publicaste. Dá-o ao teu agente com a próxima alteração, ou abre-o no browser. Cada alteração passa a ser uma nova versão no mesmo endereço, e podes voltar a uma versão anterior quando quiseres.',
    ],
    [
      'Quem pode ver a minha app?',
      <>
        Quem tiver o link. Não aparece em lado nenhum, a não ser que decidas pô-la na {showcase('montra')}.
      </>,
    ],
    [
      'Há alguma coisa que não possa publicar?',
      <>Poucas, como tudo o que prejudique ou engane pessoas. Os {terms('termos')} são curtos e escritos em linguagem simples.</>,
    ],
  ],

  closeTitle: 'Funciona na tua máquina.',
  closeAccent: 'Agora, na de toda a gente.',
  noAgent: 'Sem agente? Cria aqui',
  closeFacts: 'Grátis. Sem conta. Nada para instalar.',

  scene: {
    label: 'Um agente recebe o pedido de publicar uma app. O endereço muda de localhost para um link público, e as pessoas começam a entrar.',
    ask: 'Põe o meu quiz online para os meus amigos entrarem.',
    live: 'Já está online. Aqui tens o link.',
    publishing: 'A publicar…',
    public: 'Público',
    onlyYou: 'Só tu',
    app: 'Quiz de sexta à noite',
    playing: (count) => <>{count} a jogar</>,
    you: 'Tu',
  },

  compareTitle: 'O caminho mais curto de «funciona» a «experimenta»',
  compareText:
    'Vercel, Cloudflare e Lovable são ótimos sítios para pôr coisas a correr. Mas todos começam com um formulário de registo. E, assim que a tua app precisa de partilhar alguma coisa entre visitantes, tens de configurar um segundo serviço. Vê como fica quando começas do zero.',
  rows: [
    'Começar sem conta',
    'Publicar a partir do agente que já usas',
    'Dados partilhados em tempo real: chat, multijogador, recordes',
    'Custo até ao primeiro link',
  ],
  us: ['Sim', 'Liga uma vez e é só pedir', 'Incluído em todas as apps', 'Grátis'],
  rivals: [
    ['É preciso registo', 'Após iniciar sessão nas ferramentas deles', 'Com uma base de dados à parte', 'Plano gratuito'],
    ['É preciso registo', 'Após iniciar sessão nas ferramentas deles', 'Possível, com configuração', 'Plano gratuito'],
    ['É preciso registo', 'Criada no editor deles', 'Através de um backend ligado', 'Plano gratuito, créditos limitados'],
  ],
  compareNote:
    'Em setembro de 2026, para quem não tem conta em lado nenhum. Os planos e as funcionalidades de outros serviços mudam; confirma os detalhes junto deles.',

};
