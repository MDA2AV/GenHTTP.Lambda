import type { KitMessages } from '../en';

export const buildBox: KitMessages['buildBox'] = {
  placeholder: 'Quero um site que…',
  shortcut: 'Ctrl + Enter',
  buildIt: 'Criar meu site',
  builtBy: 'Criado por',
  password: 'senha',
  fable:
    'O Fable está protegido por senha enquanto está em teste. Ele roda sem limite de tempo: continua até o seu site ficar pronto, não até o tempo acabar.',
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
  asked: 'Seu pedido',
  leaveOpen: 'Deixe esta página aberta: o link para alterar o seu site depois só aparece aqui, quando ele estiver pronto.',
  log: 'O que a IA fez',
  online: 'Seu site está no ar',
  notOnline: 'Seu site foi criado, mas não foi para o ar.',
  open: 'Abrir o seu site',
  steps: {
    guide: 'Preparando tudo',
    examples: 'Olhando exemplos',
    create: 'Escolhendo um endereço para o seu site',
    write: 'Escrevendo o seu site',
    improve: 'Melhorando o seu site',
    check: 'Procurando erros',
    online: 'Colocando no ar',
    trying: 'Testando',
    looking: 'Revisando o seu site',
    forRecords: 'Abrindo espaço para os registros',
    forKeys: 'Abrindo espaço para chaves e senhas',
    forFiles: 'Abrindo espaço para o que ele salvar',
    records: 'Olhando os registros',
    keys: 'Vendo de quais chaves e senhas ele precisa',
    addFile: 'Adicionando um arquivo',
    removeFile: 'Removendo um arquivo',
    files: 'Olhando o que ele salvou',
  },

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
