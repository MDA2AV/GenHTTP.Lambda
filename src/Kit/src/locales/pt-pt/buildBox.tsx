import type { KitMessages } from '../en';

export const buildBox: KitMessages['buildBox'] = {
  placeholder: 'Quero um site que…',
  shortcut: 'Ctrl + Enter',
  buildIt: 'Criar o meu site',
  builtBy: 'Criado por',
  password: 'palavra-passe',
  fable:
    'O Fable está protegido por palavra-passe enquanto está em testes. Corre sem limite de tempo, por isso continua até o teu site estar pronto, e não até o tempo acabar.',
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
