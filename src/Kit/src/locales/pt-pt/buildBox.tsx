import type { KitMessages } from '../en';

export const buildBox: KitMessages['buildBox'] = {
  placeholder: 'Quero uma app que…',
  password: 'palavra-passe',
  unlimited:
    'Este modelo precisa da palavra-passe e depois corre sem limite de tempo: continua até a tua app ficar pronta, em vez de parar quando o tempo acaba.',
  ideas: [
    'uma app para o nosso clube onde os sócios se inscrevem em eventos',
    'uma lista para o nosso jantar partilhado, para ninguém levar o mesmo prato',
    'um livro de visitas para o nosso casamento',
    'uma sondagem em que as pessoas votam e veem os resultados',
    'uma tabela de classificação para a nossa noite de quiz semanal',
    'uma página de aniversário onde os amigos deixam mensagens de parabéns',
  ],
  ahead: (waiting) =>
    waiting === 1 ? 'Há uma app à frente da tua. Depois és tu.' : `Há ${waiting} apps à frente da tua.`,
  starting: 'A começar…',
  online: 'A tua app está online',
  notOnline: 'A tua app foi criada, mas não ficou online.',
  open: 'Abrir a tua app',
  steps: {
    guide: 'A preparar tudo',
    examples: 'A ver exemplos',
    create: 'A escolher um endereço para a tua app',
    write: 'A escrever a tua app',
    improve: 'A melhorar a tua app',
    check: 'A procurar erros',
    online: 'A pô-lo online',
    trying: 'A experimentá-lo',
    looking: 'A rever a tua app',
    forRecords: 'A preparar espaço para os registos',
    forKeys: 'A preparar espaço para chaves e palavras-passe',
    forFiles: 'A preparar espaço para o que ele guardar',
    records: 'A ver os registos',
    keys: 'A ver de que chaves e palavras-passe precisa',
    addFile: 'A adicionar um ficheiro',
    removeFile: 'A remover um ficheiro',
    files: 'A ver o que ele guardou',
    services: 'A pedir para usar outros serviços',
    site: (site) => `A ver ${site}`,
  },
  further: 'O teu link de edição',
  keep:
    'Guarda este link num sítio seguro. É a única forma de entrares na tua app a partir de outro dispositivo e não pode ser recuperado, nem por nós.',
  copyLink: 'Copiar o link de edição',
  lifetime: (offline, removed) =>
    `Mantemo-lo online enquanto for usado: após ${offline} dias sem visitas nem alterações fica offline e após ${removed} dias é removido. Podes voltar a pô-lo online aqui ou no respetivo editor.`,

  failedToStart: 'Não foi possível enviar o pedido.',
  noAnswer: 'Terminou sem dizer o que aconteceu.',
  failed: 'Não correu bem.',
  profiles: {
    enterprise: {
      placeholder: 'Precisamos de uma app que…',
      ideas: [
        'um formulário para pedir férias, aprovado pelo chefe de equipa',
        'um calendário de reservas das nossas salas de reunião',
        'uma lista de verificação para integrar novos colegas',
        'um inventário do nosso equipamento e de quem o tem',
        'uma lista de pedidos de suporte em aberto e de quem trata de cada um',
        'um formulário de feedback para as nossas formações',
      ],
    },
  },
};
