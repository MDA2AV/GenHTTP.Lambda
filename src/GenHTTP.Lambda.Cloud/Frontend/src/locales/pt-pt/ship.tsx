import type { Messages } from '../en';

export const ship: Messages['ship'] = {
  eyebrow: 'Alojamento grátis de vibe coding',
  title: 'Do localhost para o ecrã de toda a gente.',
  intro:
    'Criaste uma app com o Claude Code, o Codex ou o Cursor, mas só funciona na tua máquina. Pede ao teu agente que a publique aqui. Minutos depois, tem um link público para qualquer pessoa, uma base de dados própria e uma ligação em tempo real a toda a gente que a tem aberta, por isso as pessoas podem jogar, conversar e publicar lá, todas juntas.',
  facts: ['Grátis', 'Sem registo', 'Sem cartão de crédito', 'Nada para instalar'],
  connect: 'Ligar o teu agente',
  seeOthers: 'Ver o que os outros publicaram',

  stepsTitle: 'Do localhost a um link público em três passos',
  step: (n) => `Passo ${n}`,
  steps: [
    {
      title: 'Liga uma vez',
      body: 'Adiciona um endereço, um servidor MCP remoto, ao Claude Code, ao Codex, ao Cursor ou ao agente com que trabalhas. Demora menos de um minuto e só tens de o fazer uma vez.',
    },
    {
      title: 'Pede-lhe que publique',
      body: 'Diz-lhe para pôr a app online aqui. Ele empacota a app, faz o deploy e confirma que responde. Sem repositório no GitHub, sem pipeline de deploy, sem Docker.',
    },
    {
      title: 'Partilha o link',
      body: 'Recebes um endereço público e um link de edição privado. Envia o primeiro a quem quiseres. Guarda o segundo: é com ele que alteras a app mais tarde.',
    },
  ],

  togetherTitle: 'Mais do que alojamento: base de dados e multijogador incluídos.',
  together:
    'A maioria dos serviços de alojamento dá uma cópia da app a cada visitante, e cada um joga sozinho: o que um browser guarda no localStorage, o seguinte nunca vê. Aqui, cada app tem uma base de dados própria e uma ligação em tempo real a toda a gente que a tem aberta. O que uma pessoa faz aparece logo aos outros, e o que publicam continua lá amanhã.',
  together2:
    'Sem registo no Supabase ou no Firebase, sem backend para ligar, sem servidor para alugar. Pede como explicarias a um amigo.',
  kinds: [
    { name: 'Jogos multijogador', ask: 'Deixa até oito amigos entrarem na mesma partida e verem as jogadas uns dos outros em tempo real.' },
    { name: 'Salas de chat', ask: 'Adiciona uma sala onde toda a gente com o link pode conversar, e guarda as últimas cem mensagens.' },
    { name: 'Listas partilhadas', ask: 'Transforma a lista do que levar numa lista que a equipa toda edita ao mesmo tempo.' },
    { name: 'Rankings', ask: 'Guarda uma tabela com o melhor tempo de cada um e mostra o top 10 no ecrã inicial.' },
    { name: 'Pequenas redes sociais', ask: 'Deixa os convidados do casamento publicarem fotos num mural e porem gosto nas fotos dos outros.' },
  ],
  quote: (text) => `«${text}»`,

  connectTitle: 'Liga o Claude Code, o Codex ou o Cursor uma vez',
  connectText:
    'Dá ao teu agente o endereço do nosso servidor MCP. A partir daí, ele sabe publicar aqui, sem chave e sem iniciar sessão.',
  sayLike: 'Depois, no teu projeto, diz algo como',
  asks: [
    'Publica esta app no GenHTTP Lambda e envia-me o link.',
    'Faz com que os recordes sejam partilhados, para toda a gente ver a mesma tabela.',
  ],

  domainChip: 'Quando fizer sucesso',
  domainTitle: 'Dá-lhe um domínio só dela',
  domainText:
    'A mesma app, o mesmo link de edição, mas num endereço que é teu. Mais fácil de dizer, mais fácil de lembrar, e com ar profissional quando as pessoas a começarem a partilhar.',
  domainSubject: 'Um domínio para a minha app',
  domainAsk: 'Perguntar por um domínio',

  questionsTitle: 'Antes de publicares',
  questions: (offline, removed, showcase, terms) => [
    [
      'É mesmo grátis?',
      <>
        Sim. Sem registo, sem cartão de crédito e sem período experimental. A tua app fica online enquanto houver quem a
        use. Ao fim de {offline} dias sem uma única visita ou alteração, fica offline, e ao fim de {removed} dias é
        removida.
      </>,
    ],
    [
      'O Claude Code, o Codex ou o Cursor podem publicar a minha app aqui?',
      'Sim, e qualquer outro agente que consiga adicionar um servidor MCP remoto. Liga-o uma vez com o endereço acima e pede-lhe que publique: ele faz o deploy, confirma que a app responde e envia-te o link.',
    ],
    [
      'Porque é que os meus amigos não conseguem abrir o meu link de localhost?',
      'Porque localhost é o teu próprio computador: o endereço só funciona aí, e só enquanto a app está a correr. Um túnel empresta-lhe um endereço público enquanto o portátil estiver ligado. Publicada aqui, a app corre nos nossos servidores, com um link que continua a funcionar com o portátil fechado.',
    ],
    [
      'Preciso de um servidor, de um backend ou do Supabase?',
      'Não. Cada app tem a sua base de dados, armazenamento de ficheiros e uma ligação em tempo real a toda a gente que a tem aberta. Não há servidor para alugar nem segundo serviço para configurar, nem nada para manteres a correr do teu lado.',
    ],
    [
      'Posso tornar o meu jogo multijogador sem ter um servidor?',
      'Sim. O que um browser guarda no localStorage, o seguinte nunca vê, por isso a parte partilhada tem de estar num servidor, e aqui o servidor é nosso. Pede ao teu agente que torne o jogo multijogador, e cada jogada chega a toda a gente que o tem aberto.',
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
      'Posso fazer push para ela com git?',
      'Sim. Cada app é também um repositório git: clona-a a partir do endereço em Clonar, no editor, altera-a com as tuas ferramentas ou o teu agente e faz push. Cada commit enviado para main passa a ser a versão seguinte, e um ramo que envies passa a ser um rascunho com uma pré-visualização própria.',
    ],
    [
      'Onde ficam as minhas chaves de API?',
      'Fora do código. O teu agente pede uma chave pelo nome, e tu escreves o valor no editor. Ninguém a consegue ler depois, nem o editor, nem o agente.',
    ],
    [
      'Posso levar o meu código?',
      'Sim, é teu. Descarrega-o no editor quando quiseres, como um projeto que corre sozinho, com a base de dados incluída - ou clona-o com git, com todas as versões.',
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
  closeFacts: 'Grátis. Sem registo. Nada para instalar.',

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
    'Base de dados e dados em tempo real: chat, multijogador, recordes',
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
