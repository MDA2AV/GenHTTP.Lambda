import type { Messages } from '../en';

export const build: Messages['build'] = {
  offTitle: 'Não está ativo aqui',
  off: (write, mcp) => (
    <>
      Esta instalação não tem agente de criação. Ainda podes {write('escrever o código tu mesmo')}, ou ligar o teu
      próprio Claude a {mcp}.
    </>
  ),

  title: 'Diz o que queres.',
  intro:
    'A tua app é criada e posta online, e recebes um link para enviares a quem quiseres. Sem conta, sem instalar nada. E guarda dados (pontuações, mensagens, inscrições), por isso quem a abre vê sempre o mesmo.',
  placeholder: 'cria um…',
  working: 'a trabalhar…',
  shortcut: 'Ctrl + Enter',
  building: 'A criar',
  buildIt: 'Criar',
  builtBy: 'Feito com',
  password: 'palavra-passe',
  fable:
    'O Fable está protegido por palavra-passe enquanto está em testes. Corre sem limite de tempo, por isso continua até a app estar pronta, e não até o tempo acabar.',
  onlyNew:
    'Aqui só se criam apps novas. Para continuar algo que já fizeste, dá o link de edição ao teu agente de programação (vê abaixo).',
  ideas: [
    'um mural onde qualquer pessoa pode deixar uma mensagem de uma linha',
    'uma tabela de recordes para um jogo de dados',
    'uma sondagem em que as pessoas votam e veem os resultados',
    'um livro de visitas para o meu casamento',
    'uma contagem decrescente para uma data, à vista de todos',
  ],
  ahead: (waiting) =>
    waiting === 1 ? 'Há um pedido à frente do teu. A seguir, é a tua vez.' : `${waiting} pedidos à frente do teu.`,
  starting: 'A começar…',

  yourApp: 'A tua app',
  further: 'Para continuar',
  keep: 'Guarda este link. É a única forma de voltares a entrar e não pode ser recuperado, nem por nós. Adiciona-o aos marcadores antes de fechares este separador.',
  change:
    'Esta página só cria coisas novas. Para alterares esta, liga o teu agente de programação como se explica abaixo, dá-lhe o link de edição e diz-lhe o que queres mudar.',
  copyLink: 'Copiar o link de edição',
  lifetime: (offline, removed) =>
    `Fica online enquanto for usada: ao fim de ${offline} dias sem visitas nem alterações fica offline e, ao fim de ${removed} dias, é removida. Para a pores online outra vez, abre o editor e clica em Fazer deploy.`,
  openEditor: 'Abrir o editor',
  another: 'Criar outra coisa',

  keepGoing: 'Continua com o teu agente',
  orOwn: 'Ou usa o teu próprio agente',
  ownText:
    'A caixa acima é um Claude a correr nesta máquina. Se já tens o teu, liga-o aqui: faz o mesmo (cria uma lambda, escreve o código, põe-na online), sem limite diário e sem passar por esta página.',
  thenAsk: 'Depois, pede-lhe o que queres, tal como farias aqui.',
  claudeWeb: 'Claude na web',
  claudeWebHow:
    'Definições, depois «Connectors», depois «Add custom connector». Cola o endereço acima como URL do servidor MCP remoto. Não há chave nem início de sessão.',
  howToChange: 'Também é assim que alteras algo depois de pronto: dá o link de edição ao teu agente e diz-lhe o que fazer.',
  more: 'Mais sobre usar um agente aqui',

  failedToStart: 'Não foi possível enviar o pedido.',
  noAnswer: 'Terminou sem dizer o que aconteceu.',
  failed: 'Não correu bem.',
};
