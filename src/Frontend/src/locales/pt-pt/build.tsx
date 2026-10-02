import type { Messages } from '../en';

export const build: Messages['build'] = {
  title: 'Cria um site com IA.',
  intro:
    'Descreve por palavras tuas o site ou a app que tens em mente. A IA cria-o por ti, nós alojamo-lo e fica online em minutos, com um link para enviares a quem quiseres. Grátis, sem programar, sem registo.',
  placeholder: 'Quero um site que…',
  shortcut: 'Ctrl + Enter',
  buildIt: 'Criar o meu site',
  builtBy: 'Criado por',
  password: 'palavra-passe',
  fable:
    'O Fable está protegido por palavra-passe enquanto está em testes. Corre sem limite de tempo, por isso continua até a app estar pronta, e não até o tempo acabar.',
  onlyNew:
    'Aqui crias sites novos. Para alterar um que já tens, abre o link de edição e descreve em “Alterar” o que deve ser diferente.',
  ideas: [
    'um site para o nosso clube onde os sócios se inscrevem em eventos',
    'uma lista para o nosso jantar partilhado, para ninguém levar o mesmo prato',
    'um livro de visitas para o nosso casamento',
    'uma sondagem em que as pessoas votam e veem os resultados',
    'uma tabela de classificação para a nossa noite de quiz semanal',
    'uma página de aniversário onde os amigos deixam mensagens de parabéns',
  ],
  ahead: (waiting) =>
    waiting === 1 ? 'Há um site à frente do teu. Depois és tu.' : `Há ${waiting} sites à frente do teu.`,
  starting: 'A começar…',
  asked: 'O teu pedido',
  leaveOpen: 'Deixa esta página aberta: o link para alterares o teu site mais tarde só aparece aqui, quando estiver pronto.',
  log: 'O que a IA fez',
  online: 'O teu site está online',
  notOnline: 'O teu site foi criado, mas não ficou online.',
  open: 'Abrir o teu site',
  steps: {
    guide: 'A preparar tudo',
    examples: 'A ver exemplos',
    create: 'A escolher um endereço para o teu site',
    write: 'A escrever o teu site',
    improve: 'A melhorar o teu site',
    check: 'A procurar erros',
    online: 'A pô-lo online',
    trying: 'A experimentá-lo',
    looking: 'A rever o teu site',
    forRecords: 'A preparar espaço para os registos',
    forKeys: 'A preparar espaço para chaves e palavras-passe',
    forFiles: 'A preparar espaço para o que ele guardar',
    records: 'A ver os registos',
    keys: 'A ver de que chaves e palavras-passe precisa',
    addFile: 'A adicionar um ficheiro',
    removeFile: 'A remover um ficheiro',
    files: 'A ver o que ele guardou',
  },

  points: [
    {
      title: 'Sem saber programar',
      text: 'Diz por palavras tuas o que o teu site deve fazer, como explicarias a um amigo. A IA cria-o por ti, sem precisares de conhecimentos técnicos.',
    },
    {
      title: 'Alojamento grátis incluído',
      text: 'O teu site corre nos nossos servidores. Sem plano de alojamento, sem servidor nem domínio para comprar, nada para instalar. Segurança e atualizações ficam a nosso cargo.',
    },
    {
      title: 'Online em minutos',
      text: 'Recebes logo um link para partilhar. O site guarda o que as pessoas escrevem (inscrições, votos, mensagens, pontuações), para que todos vejam o mesmo.',
    },
  ],

  questionsTitle: 'Antes de começares',
  questions: (offline, removed) => [
    [
      'A IA cria mesmo um site grátis para mim?',
      `Sim. Descreve-o por palavras tuas e a IA cria-o, põe-no online e dá-te o link. Sem registo, sem cartão de crédito, sem período experimental. Fica online enquanto houver quem o use: após ${offline} dias sem visitas nem alterações fica offline, e após ${removed} dias é removido.`,
    ],
    [
      'Preciso de alojamento, de um servidor ou de um domínio?',
      'Não. O teu site corre nos nossos servidores, com alojamento, segurança e atualizações incluídos. Recebes logo um link, por isso também não tens de comprar um domínio.',
    ],
    [
      'Posso criar uma app sem saber programar?',
      'Sim. Nunca vês código nenhum. Diz o que deve fazer, como explicarias a um amigo, e a IA trata do resto: um site, uma pequena app ou um jogo.',
    ],
    [
      'As pessoas podem deixar inscrições, votos e mensagens?',
      'Sim. O teu site guarda o que as pessoas escrevem, por isso quem abre o link vê as mesmas inscrições, votos e pontuações.',
    ],
    [
      'Como é que as outras pessoas o abrem?',
      'Com o link, em qualquer browser, no telemóvel ou no computador. Não há nada para instalar nem loja de apps pelo meio.',
    ],
    [
      'Como é que o altero mais tarde?',
      'Abre o link de edição que recebes com o teu site e descreve o que deve ser diferente, tal como aqui. Se não gostares de uma alteração, podes voltar ao que estava antes.',
    ],
  ],

  yourApp: 'O teu site',
  further: 'Para o alterares mais tarde',
  keep:
    'Guarda este link. É a única forma de voltares a entrar e não pode ser recuperado, nem por nós. Adiciona-o aos marcadores antes de fechares este separador.',
  change:
    'Para alterares o teu site, abre o link de edição e descreve em “Alterar” o que deve ser diferente, tal como aqui. O teu próprio assistente de IA também o pode fazer, como explicado abaixo.',
  copyLink: 'Copiar o link de edição',
  lifetime: (offline, removed) =>
    `Mantemo-lo online enquanto for usado: após ${offline} dias sem visitas nem alterações fica offline e após ${removed} dias é removido. Abre o editor para o voltares a pôr online.`,
  openEditor: 'Abrir o editor',
  another: 'Criar outro site',

  keepGoing: 'Continua com o teu assistente de IA',
  orOwn: 'Ou usa o teu próprio assistente de IA',
  ownText:
    'Já usas o Claude ou outro assistente de IA? Liga-o aqui e ele cria e altera sites por ti da mesma forma. Nós alojamo-los, por isso continuas sem nada para configurar. Não há limite diário.',
  ownTitle: 'Cria o teu site com o teu assistente de IA',
  ownOnly:
    'Liga o Claude ou outro assistente de IA ao endereço abaixo e descreve o site que queres. Ele cria-o, nós alojamo-lo nos nossos servidores e fica online de imediato, com um link para partilhar.',
  thenAsk:
    'Depois diz-lhe o que queres, por exemplo: “Cria um site para o nosso coro com um calendário dos nossos concertos.”',
  howToChange:
    'É também assim que alteras um site mais tarde: dá ao teu assistente o link de edição e diz-lhe o que deve ser diferente.',

  failedToStart: 'Não foi possível enviar o pedido.',
  noAnswer: 'Terminou sem dizer o que aconteceu.',
  failed: 'Não correu bem.',
};
