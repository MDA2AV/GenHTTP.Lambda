import type { KitMessages } from '../en';

export const buildBox: KitMessages['buildBox'] = {
  placeholder: 'Quero um site que…',
  password: 'palavra-passe',
  fable:
    'O Fable está protegido por palavra-passe enquanto está em testes. Corre sem limite de tempo, por isso continua até o teu site estar pronto, e não até o tempo acabar.',
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
    services: 'A pedir para usar outros serviços',
  },
  further: 'O teu link de edição',
  keep:
    'Guarda este link num sítio seguro. É a única forma de entrares no teu site a partir de outro dispositivo e não pode ser recuperado, nem por nós.',
  copyLink: 'Copiar o link de edição',
  lifetime: (offline, removed) =>
    `Mantemo-lo online enquanto for usado: após ${offline} dias sem visitas nem alterações fica offline e após ${removed} dias é removido. Podes voltar a pô-lo online aqui ou no respetivo editor.`,

  failedToStart: 'Não foi possível enviar o pedido.',
  noAnswer: 'Terminou sem dizer o que aconteceu.',
  failed: 'Não correu bem.',
};
