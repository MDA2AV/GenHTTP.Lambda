import type { KitMessages } from '../en';

export const buildBox: KitMessages['buildBox'] = {
  placeholder: 'Quero um app que…',
  password: 'senha',
  unlimited:
    'Esta escolha precisa da senha do site e, então, recebe o tempo e os passos definidos para ela, em vez dos habituais.',
  ideas: [
    'um app para o nosso clube onde os membros se inscrevem em eventos',
    'uma lista para a nossa confraternização, para ninguém levar o mesmo prato',
    'um livro de visitas para o nosso casamento',
    'uma enquete em que as pessoas votam e veem o resultado',
    'um ranking para a nossa noite de quiz semanal',
    'uma página de aniversário onde os amigos deixam recados',
  ],
  ahead: (waiting) =>
    waiting === 1 ? 'Há um app na frente do seu. Depois é a sua vez.' : `Há ${waiting} apps na frente do seu.`,
  starting: 'Começando…',
  online: 'Seu app está no ar',
  notOnline: 'Seu app foi criado, mas não foi para o ar.',
  open: 'Abrir o seu app',
  steps: {
    guide: 'Preparando tudo',
    examples: 'Olhando exemplos',
    create: 'Escolhendo um endereço para o seu app',
    write: 'Escrevendo o seu app',
    improve: 'Melhorando o seu app',
    check: 'Procurando erros',
    online: 'Colocando no ar',
    trying: 'Testando',
    looking: 'Revisando o seu app',
    forRecords: 'Abrindo espaço para os registros',
    forKeys: 'Abrindo espaço para chaves e senhas',
    forFiles: 'Abrindo espaço para o que ele salvar',
    records: 'Olhando os registros',
    keys: 'Vendo de quais chaves e senhas ele precisa',
    addFile: 'Adicionando um arquivo',
    removeFile: 'Removendo um arquivo',
    files: 'Olhando o que ele salvou',
    services: 'Pedindo para usar outros serviços',
    site: (site) => `Olhando ${site}`,
    search: 'Pesquisando algo na web',
    attachment: 'Olhando o arquivo que você enviou',
    showcase: 'Colocando na vitrine',
    file: 'Escrevendo um arquivo para você',
  },
  further: 'Seu link de edição',
  keep:
    'Guarde este link em um lugar seguro. É o único jeito de entrar no seu app a partir de outro dispositivo, e ninguém consegue recuperar, nem mesmo a gente.',
  copyLink: 'Copiar o link de edição',
  lifetime: (offline, removed) =>
    `Mantemos o app no ar enquanto ele for usado: depois de ${offline} dias sem visitas nem mudanças, ele sai do ar, e depois de ${removed} dias é removido. Você pode colocá-lo no ar de novo aqui ou no editor dele.`,

  failedToStart: 'O pedido não foi enviado.',
  noAnswer: 'Terminou sem dizer o que aconteceu.',
  failed: 'Não deu certo.',
  profiles: {
    enterprise: {
      placeholder: 'Precisamos de um app que…',
      ideas: [
        'um formulário para solicitar férias, aprovado pelo líder da equipe',
        'um calendário de reservas para as nossas salas de reunião',
        'um checklist para integrar os novos colegas',
        'um inventário dos nossos equipamentos e de quem está com cada um',
        'uma lista de chamados de suporte abertos e de quem cuida de cada um',
        'um formulário de feedback para os nossos treinamentos',
      ],
    },
  },
};
