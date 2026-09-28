import type { Messages } from '../en';

export const build: Messages['build'] = {
  offTitle: 'Não está ativado aqui',
  off: (write, mcp) => (
    <>
      Esta instalação não tem agente de criação. Você ainda pode {write('escrever o código você mesmo')} ou conectar
      seu próprio Claude a {mcp}.
    </>
  ),

  title: 'Diga o que você quer.',
  intro:
    'Seu app é criado e vai para o ar. Você recebe um link e manda para quem quiser. Sem conta, sem instalar nada. E ele guarda dados (pontuações, mensagens, inscrições), então todo mundo que abre vê a mesma coisa.',
  placeholder: 'cria um…',
  working: 'trabalhando…',
  shortcut: 'Ctrl + Enter',
  building: 'Criando',
  buildIt: 'Criar',
  builtBy: 'Feito com',
  password: 'senha',
  fable:
    'O Fable está protegido por senha enquanto está em teste. Ele roda sem limite de tempo: continua até o app ficar pronto, não até o tempo acabar.',
  onlyNew:
    'Aqui você só cria apps novos. Para continuar algo que você já fez, passe o link de edição para o seu agente de código (veja abaixo).',
  ideas: [
    'um mural onde qualquer pessoa deixa um recado de uma linha',
    'um placar de recordes para um jogo de dados',
    'uma enquete em que as pessoas votam e veem o resultado',
    'um livro de visitas para o meu casamento',
    'uma contagem regressiva para uma data, que todo mundo pode ver',
  ],
  ahead: (waiting) =>
    waiting === 1 ? 'Só um pedido na sua frente. O seu é o próximo.' : `${waiting} pedidos na sua frente.`,
  starting: 'Começando…',

  yourApp: 'Seu app',
  further: 'Para continuar depois',
  keep: 'Guarde este link. É o único jeito de voltar, e ninguém consegue recuperar, nem mesmo a gente. Salve nos favoritos antes de fechar a aba.',
  change:
    'Esta página só cria coisas novas. Para mudar este app, conecte seu agente de código como explicado abaixo, passe o link de edição e diga o que você quer mudar.',
  copyLink: 'Copiar o link de edição',
  lifetime: (offline, removed) =>
    `Ele fica no ar enquanto for usado. Depois de ${offline} dias sem visitas nem mudanças, sai do ar. Depois de ${removed} dias, é removido. Para colocar de volta no ar, abra o editor e clique em Fazer deploy.`,
  openEditor: 'Abrir o editor',
  another: 'Criar outro app',

  keepGoing: 'Continue com seu próprio agente',
  orOwn: 'Ou use seu próprio agente',
  ownText:
    'A caixa acima é um Claude rodando neste servidor. Se você já tem o seu, conecte aqui: ele faz as mesmas coisas (cria uma lambda, escreve o código, coloca no ar), sem limite diário e sem passar por esta página.',
  thenAsk: 'Depois é só pedir o que você quer, do mesmo jeito que faria aqui.',
  claudeWeb: 'Claude na web',
  claudeWebHow:
    'Configurações, depois “Connectors” e “Add custom connector”. Cole o endereço acima como URL do servidor MCP remoto. Sem chave e sem login.',
  howToChange: 'É assim também que você muda algo depois de pronto: passe o link de edição para o seu agente e diga o que fazer.',
  more: 'Mais sobre usar um agente aqui',

  failedToStart: 'O pedido não foi enviado.',
  noAnswer: 'Terminou sem dizer o que aconteceu.',
  failed: 'Não deu certo.',
};
