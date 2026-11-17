import type { KitMessages } from '../en';

export const buildBox: KitMessages['buildBox'] = {
  placeholder: 'Quero um site que…',
  password: 'senha',
  fable:
    'O Fable está protegido por senha enquanto está em teste. Ele roda sem limite de tempo: continua até o seu site ficar pronto, não até o tempo acabar.',
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
    services: 'Pedindo para usar outros serviços',
  },
  further: 'Seu link de edição',
  keep:
    'Guarde este link em um lugar seguro. É o único jeito de entrar no seu site a partir de outro dispositivo, e ninguém consegue recuperar, nem mesmo a gente.',
  copyLink: 'Copiar o link de edição',
  lifetime: (offline, removed) =>
    `Mantemos o site no ar enquanto ele for usado: depois de ${offline} dias sem visitas nem mudanças, ele sai do ar, e depois de ${removed} dias é removido. Você pode colocá-lo no ar de novo aqui ou no editor dele.`,

  failedToStart: 'O pedido não foi enviado.',
  noAnswer: 'Terminou sem dizer o que aconteceu.',
  failed: 'Não deu certo.',
};
