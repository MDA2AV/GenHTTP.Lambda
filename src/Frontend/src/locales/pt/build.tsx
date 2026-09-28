import type { Messages } from '../en';

export const build: Messages['build'] = {
  title: 'Da ideia ao site.',
  intro:
    'Descreva o site ou app que você imagina. A IA cria para você, nós hospedamos em nossos servidores e ele fica no ar na hora, com um link para mandar para quem quiser. Sem programar, sem configurar hospedagem, sem conta.',
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
    'um livro de visitas para o nosso casamento',
    'uma enquete em que as pessoas votam e veem o resultado',
    'um placar para a nossa noite de quiz semanal',
    'uma contagem regressiva para a nossa inauguração que todos podem ver',
  ],
  ahead: (waiting) =>
    waiting === 1 ? 'Há um site na frente do seu. Depois é a sua vez.' : `Há ${waiting} sites na frente do seu.`,
  starting: 'Começando…',

  points: [
    {
      title: 'Descrito, não programado',
      text: 'Diga com suas palavras o que o seu site deve fazer. Não é preciso programar nem ter conhecimento técnico.',
    },
    {
      title: 'Hospedagem incluída',
      text: 'Seu site roda em nossos servidores. Hospedagem, segurança e atualizações ficam por nossa conta: você não precisa configurar nem manter nada.',
    },
    {
      title: 'No ar em minutos',
      text: 'Você recebe na hora um link para compartilhar. O site também pode guardar dados (inscrições, votos, pontuações), para todos verem a mesma coisa.',
    },
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
