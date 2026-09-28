import type { Messages } from '../en';

export const build: Messages['build'] = {
  title: 'Da ideia ao site.',
  intro:
    'Descreve o site ou a app que tens em mente. A IA cria-o por ti, nós alojamo-lo nos nossos servidores e fica online de imediato, com um link para enviares a quem quiseres. Sem programar, sem configurar alojamento, sem conta.',
  placeholder: 'Quero um site que…',
  working: 'a trabalhar…',
  shortcut: 'Ctrl + Enter',
  building: 'A criar',
  buildIt: 'Criar o meu site',
  builtBy: 'Criado por',
  password: 'palavra-passe',
  fable:
    'O Fable está protegido por palavra-passe enquanto está em testes. Corre sem limite de tempo, por isso continua até a app estar pronta, e não até o tempo acabar.',
  onlyNew:
    'Aqui crias sites novos. Para alterar um que já tens, abre o link de edição e descreve em “Alterar” o que deve ser diferente.',
  ideas: [
    'um site para o nosso clube onde os sócios se inscrevem em eventos',
    'um livro de visitas para o nosso casamento',
    'uma sondagem em que as pessoas votam e veem os resultados',
    'um quadro de pontuações para a nossa noite de quiz semanal',
    'uma contagem decrescente para a nossa inauguração que todos podem ver',
  ],
  ahead: (waiting) =>
    waiting === 1 ? 'Há um site à frente do teu. Depois és tu.' : `Há ${waiting} sites à frente do teu.`,
  starting: 'A começar…',

  points: [
    {
      title: 'Descrito, não programado',
      text: 'Diz por palavras tuas o que o teu site deve fazer. Não precisas de programar nem de conhecimentos técnicos.',
    },
    {
      title: 'Alojamento incluído',
      text: 'O teu site corre nos nossos servidores. Alojamento, segurança e atualizações ficam a nosso cargo: não tens nada para configurar nem manter.',
    },
    {
      title: 'Online em minutos',
      text: 'Recebes logo um link para partilhar. O site também pode guardar dados (inscrições, votos, pontuações), para que todos vejam o mesmo.',
    },
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
