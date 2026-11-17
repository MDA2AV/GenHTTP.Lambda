import type { Messages } from '../en';

export const ship: Messages['ship'] = {
  eyebrow: 'Alojamento grátis de vibe coding',
  title: 'Do localhost para o ecrã de toda a gente.',
  intro:
    'Criaste uma app com o Claude Code, o Codex ou o Cursor, mas só funciona na tua máquina. Pede ao teu agente que a publique aqui. Minutos depois, tem um link público para qualquer pessoa, uma base de dados própria e uma ligação em tempo real a toda a gente que a tem aberta, por isso as pessoas podem jogar, conversar e publicar lá, todas juntas.',
  facts: ['Grátis', 'Sem registo', 'Sem cartão de crédito', 'Nada para instalar'],

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
  sayLike: 'Depois, no teu projeto, diz algo como',
  asks: [
    'Publica esta app no GenHTTP Lambda e envia-me o link.',
    'Faz com que os recordes sejam partilhados, para toda a gente ver a mesma tabela.',
  ],

  questionsTitle: 'Antes de publicares',
  questions: (offline, removed, showcase, terms, selfHosting) => [
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
      'Posso antes alojá-la no meu próprio servidor?',
      <>
        Sim. O GenHTTP Lambda é código aberto, e a sua edição Community é a mesma plataforma num só contentor Docker, para o
        teu servidor e o teu próprio agente. {selfHosting('Cloud ou alojamento próprio')} diz o que muda.
      </>,
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
  noAgent: 'Sem agente? Cria aqui',

};
