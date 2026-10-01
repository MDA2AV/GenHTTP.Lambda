import type { Messages } from '../en';

export const build: Messages['build'] = {
  title: 'Crie um site com IA.',
  intro:
    'Descreva com suas palavras o site ou app que você imagina. A IA cria para você, nós hospedamos e ele fica no ar em minutos, com um link para mandar para quem quiser. Grátis, sem programar, sem cadastro.',
  placeholder: 'Quero um site que…',
  working: 'trabalhando…',
  shortcut: 'Ctrl + Enter',
  building: 'Criando',
  buildIt: 'Criar meu site',
  builtBy: 'Criado por',
  password: 'senha',
  fable:
    'O Fable está protegido por senha enquanto está em teste. Ele roda sem limite de tempo: continua até o app ficar pronto, não até o tempo acabar.',
  onlyNew:
    'Aqui você cria sites novos. Para mudar um que você já tem, abra o link de edição dele e descreva em “Mudar” o que deve ser diferente.',
  ideas: [
    'um site para o nosso clube onde os membros se inscrevem em eventos',
    'uma lista para a nossa confraternização, para ninguém levar o mesmo prato',
    'um livro de visitas para o nosso casamento',
    'uma enquete em que as pessoas votam e veem o resultado',
    'um ranking para a nossa noite de quiz semanal',
    'uma página de aniversário onde os amigos deixam recados',
  ],
  ahead: (waiting) =>
    waiting === 1 ? 'Há um site na frente do seu. Depois é a sua vez.' : `Há ${waiting} sites na frente do seu.`,
  starting: 'Começando…',

  points: [
    {
      title: 'Sem saber programar',
      text: 'Diga com suas palavras o que o seu site deve fazer, como você explicaria para um amigo. A IA cria para você, sem precisar de conhecimento técnico.',
    },
    {
      title: 'Hospedagem grátis incluída',
      text: 'Seu site roda em nossos servidores. Sem plano de hospedagem, sem servidor, sem domínio para comprar e sem instalar nada. Segurança e atualizações ficam por nossa conta.',
    },
    {
      title: 'No ar em minutos',
      text: 'Você recebe na hora um link para compartilhar. O site guarda o que as pessoas enviam (inscrições, votos, recados, pontuações), para todos verem a mesma coisa.',
    },
  ],

  questionsTitle: 'Antes de começar',
  questions: (offline, removed) => [
    [
      'A IA cria mesmo um site de graça para mim?',
      `Sim. Descreva com suas palavras e a IA cria o site, coloca no ar e passa o link para você. Sem cadastro, sem cartão de crédito, sem período de teste. Ele fica no ar enquanto as pessoas usarem: depois de ${offline} dias sem visitas nem mudanças, sai do ar, e depois de ${removed} dias é removido.`,
    ],
    [
      'Preciso de hospedagem, servidor ou domínio?',
      'Não. Seu site roda em nossos servidores, com hospedagem, segurança e atualizações incluídas. Você recebe um link na hora, então também não precisa comprar domínio.',
    ],
    [
      'Dá para criar um app sem saber programar?',
      'Sim. Você nunca vê código. Diga o que ele deve fazer, como explicaria para um amigo, e a IA faz o resto: um site, um pequeno app ou um jogo.',
    ],
    [
      'As pessoas podem enviar inscrições, votos e recados?',
      'Sim. Seu site guarda o que as pessoas enviam, então todo mundo que abre o link vê as mesmas inscrições, votos e pontuações.',
    ],
    [
      'Como as outras pessoas abrem o site?',
      'Pelo link, em qualquer navegador, no celular ou no computador. Não precisa instalar nada, e não tem loja de apps no meio.',
    ],
    [
      'Como eu mudo o site depois?',
      'Abra o link de edição que você recebe com o seu site e descreva o que deve ser diferente, do mesmo jeito que aqui. Se não gostar de uma mudança, dá para voltar ao que era antes.',
    ],
  ],

  yourApp: 'Seu site',
  further: 'Para mudar depois',
  keep:
    'Guarde este link. É o único jeito de voltar, e ninguém consegue recuperar, nem mesmo a gente. Salve nos favoritos antes de fechar a aba.',
  change:
    'Para mudar o seu site, abra o link de edição e descreva em “Mudar” o que deve ser diferente, do mesmo jeito que aqui. Seu próprio assistente de IA também pode fazer isso, como explicado abaixo.',
  copyLink: 'Copiar o link de edição',
  lifetime: (offline, removed) =>
    `Mantemos o site no ar enquanto ele for usado: depois de ${offline} dias sem visitas nem mudanças, ele sai do ar, e depois de ${removed} dias é removido. Abra o editor para colocá-lo no ar de novo.`,
  openEditor: 'Abrir o editor',
  another: 'Criar outro site',

  keepGoing: 'Continue com o seu assistente de IA',
  orOwn: 'Ou use o seu próprio assistente de IA',
  ownText:
    'Já usa o Claude ou outro assistente de IA? Conecte-o aqui e ele cria e muda sites para você do mesmo jeito. Nós hospedamos, então continua sem nada para configurar. Não há limite diário.',
  ownTitle: 'Crie seu site com o seu assistente de IA',
  ownOnly:
    'Conecte o Claude ou outro assistente de IA ao endereço abaixo e descreva o site que você quer. Ele cria, nós hospedamos em nossos servidores e o site fica no ar na hora, com um link para compartilhar.',
  thenAsk:
    'Depois diga o que você quer, por exemplo: “Crie um site para o nosso coral com uma agenda dos nossos concertos.”',
  howToChange:
    'É assim também que se muda um site depois: dê ao seu assistente o link de edição e diga o que deve ser diferente.',

  failedToStart: 'O pedido não foi enviado.',
  noAnswer: 'Terminou sem dizer o que aconteceu.',
  failed: 'Não deu certo.',
};
