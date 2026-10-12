import type { EditorMessages } from '../../en/editor';

export const analytics: EditorMessages['analytics'] = {
  hint: 'Contadas para você pela plataforma: sem cookies, sem código no seu app e sem nada para os seus visitantes aceitarem.',
  loading: 'Lendo as visitas…',
  loadFailed: 'Não foi possível ler as visitas.',

  offTitle: 'Veja quem visita o seu app',
  offText:
    'Quantas pessoas chegam por dia, quais páginas abrem, quais sites as enviam, de quais países e em quais dispositivos - contado dia a dia, a partir do momento em que você ativa.',
  offPrivate: 'Sem cookies e sem banner para os seus visitantes aceitarem',
  offAddresses: 'Nenhum endereço é guardado, apenas quantas visitas houve por dia',
  offKept: (days) =>
    days >= 365
      ? 'Guardado por um ano, e tudo é mostrado para você'
      : `Guardado por um ano - os últimos ${days} dias são mostrados, tudo com o Premium`,
  switchOn: 'Ativar o analytics',
  switchedOn: 'O analytics está ativado. As primeiras visitas aparecem em alguns minutos.',
  switchOnFailed: 'Não foi possível ativar o analytics.',
  demo: 'Os visitantes de uma demo não são contados. Uma cópia sua pode contar os visitantes.',

  range: 'Período',
  days: (days) => (days >= 365 ? '12 meses' : `${days} dias`),
  premiumRange: 'O Premium mostra até um ano',

  visitors: 'Visitantes',
  visitorsHint: 'Pessoas diferentes, cada uma contada uma vez por dia.',
  views: 'Visualizações de página',
  viewsHint: 'Cada vez que alguém abriu uma página.',
  hits: 'Requisições',
  hitsHint: 'Tudo o que o seu app recebeu: as páginas e as imagens e scripts que elas carregam.',
  versus: (days) => `em relação aos ${days} dias anteriores`,
  noChange: 'sem alteração',
  inDays: (days) => (days >= 365 ? 'nos últimos 12 meses' : `nos últimos ${days} dias`),

  perDay: {
    visitors: 'Visitantes por dia',
    views: 'Visualizações de página por dia',
    hits: 'Requisições por dia',
  },
  perWeek: {
    visitors: 'Visitantes por semana',
    views: 'Visualizações de página por semana',
    hits: 'Requisições por semana',
  },
  showValues: 'Mostrar os valores',
  showChart: 'Mostrar o gráfico',
  day: 'Dia',
  week: 'Semana de',

  nobody: 'Ninguém visitou nesses dias.',
  nobodyYet: 'Ninguém visitou desde que o analytics foi ativado. As primeiras visitas aparecem em alguns minutos.',

  pages: 'Páginas',
  pagesHint: 'As páginas que as pessoas abriram, das mais abertas para as menos abertas.',
  requested: 'Mais requisitados',
  requestedHint: 'Tudo o que o seu app recebeu, por endereço - ele não tem páginas que as pessoas abrem.',
  sources: 'De onde vêm os visitantes',
  sourcesHint: 'Os sites que enviaram pessoas para cá, pela página em que estavam. Os mecanismos de busca só informam que foram eles, não o que foi pesquisado.',
  direct: 'Digitado, um favorito ou um app',
  countries: 'Países',
  countriesHint: 'Onde os endereços de internet dos visitantes estão registrados - em geral, mas nem sempre, onde eles estão.',
  unknownCountry: 'Desconhecido',
  devices: 'Dispositivos',
  device: {
    desktop: 'Computador',
    mobile: 'Celular',
    tablet: 'Tablet',
    bot: 'Bots',
    other: 'Outros',
  },
  browsers: 'Navegadores',
  systems: 'Sistemas',
  crawlers: 'Mecanismos de busca e IA',
  crawlersHint: 'Programas que passaram por aqui: mecanismos de busca indexando as suas páginas, assistentes de IA lendo essas páginas, chats mostrando a prévia de um link.',
  noCrawlers: 'Nenhum mecanismo de busca ou IA passou por aqui nesses dias.',
  other: 'Outros',
  otherBots: 'Outros programas',
  showAll: (count) => `Mostrar todos os ${count}`,
  showFewer: 'Mostrar menos',
  share: (part, whole) => `${part} de ${whole}`,
  none: 'Nada ainda.',

  moreKept: (since) => `Contado desde ${since} - o Premium mostra tudo, até um ano.`,
  premiumAsk: 'Perguntar sobre o Premium',
  premiumSubject: 'Premium para o meu app',
  premiumBody: (address) => `Olá,\n\nGostaria de ver todas as análises do meu app: ${address}\n\n`,

  footer: (minutes) =>
    minutes === 1 ? 'Contado por dia, em UTC. Atualizado a cada minuto.' : `Contado por dia, em UTC. Atualizado a cada ${minutes} minutos.`,
  updated: (time) => `Última atualização às ${time}.`,
  since: (date) => `Contando desde ${date}.`,

  switchOff: 'Desativar o analytics',
  switchOffTitle: 'Desativar o analytics?',
  switchOffText: 'A contagem para e tudo o que foi contado até agora é apagado. Isso não pode ser desfeito.',
  keep: 'Continuar contando',
  switchOffConfirm: 'Desativar e apagar',
  switchedOff: 'O analytics está desativado, e o que foi contado foi apagado.',
  switchOffFailed: 'Não foi possível desativar o analytics.',
};
