import type { Messages } from '../en';

export const build: Messages['build'] = {
  offTitle: 'Não disponível nesta instalação',
  off: (write, mcp) => (
    <>
      Esta instalação não possui um agente de criação. Ainda assim, você pode {write('escrever o código você mesmo')} ou
      conectar seu próprio Claude a {mcp}.
    </>
  ),

  title: 'Descreva o que você precisa.',
  intro:
    'A aplicação é criada e publicada, e você recebe um link que pode enviar a qualquer pessoa. Sem conta, sem instalação – e com capacidade de guardar dados, como pontuações, mensagens ou inscrições, para que todos vejam o mesmo conteúdo.',
  placeholder: 'crie um…',
  working: 'em andamento…',
  shortcut: 'Ctrl + Enter',
  building: 'Criando',
  buildIt: 'Criar',
  builtBy: 'Criado com',
  password: 'senha',
  fable:
    'O Fable está protegido por senha durante a fase de testes. Ele funciona sem limite de tempo e continua até que a aplicação esteja concluída.',
  onlyNew:
    'Aqui são criadas apenas aplicações novas. Para evoluir algo que você já criou, entregue o link de edição ao seu próprio agente de programação – veja abaixo.',
  ideas: [
    'um mural onde qualquer pessoa pode deixar uma mensagem curta',
    'um placar para um jogo de dados',
    'uma enquete com resultados visíveis',
    'um livro de visitas para o nosso casamento',
    'uma contagem regressiva compartilhada até uma data',
  ],
  ahead: (waiting) =>
    waiting === 1 ? 'Há uma criação antes da sua – você é o próximo.' : `Há ${waiting} criações antes da sua.`,
  starting: 'Iniciando…',

  yourApp: 'Sua aplicação',
  further: 'Para continuar o desenvolvimento',
  keep: 'Guarde este link. Ele é o único acesso e não pode ser recuperado – nem por nós. Adicione-o aos favoritos antes de fechar esta aba.',
  change:
    'Esta página cria apenas aplicações novas. Para alterar esta, conecte seu próprio agente de programação conforme descrito abaixo, entregue a ele o link de edição e descreva a alteração desejada.',
  copyLink: 'Copiar o link de edição',
  lifetime: (offline, removed) =>
    `A aplicação permanece no ar enquanto for utilizada: após ${offline} dias sem acessos ou alterações, ela sai do ar e, após ${removed} dias, é removida. Para colocá-la no ar novamente, abra o editor e clique em Implantar.`,
  openEditor: 'Abrir o editor',
  another: 'Criar outra aplicação',

  keepGoing: 'Continuar com seu próprio agente',
  orOwn: 'Ou use seu próprio agente',
  ownText:
    'O campo acima utiliza um Claude executado neste servidor. Se você já possui seu próprio agente, pode conectá-lo aqui: ele terá as mesmas capacidades – criar um lambda, escrever o código, publicá-lo – sem limite diário e sem passar por esta página.',
  thenAsk: 'Em seguida, descreva o que você precisa, da mesma forma que aqui.',
  claudeWeb: 'Claude na web',
  claudeWebHow:
    'Configurações, depois “Connectors” e em seguida “Add custom connector”. Cole o endereço acima como URL do servidor MCP remoto. Não é necessária chave nem login.',
  howToChange:
    'É também assim que se altera uma aplicação existente: entregue o link de edição ao seu agente e descreva a alteração.',
  more: 'Saiba mais sobre o uso de um agente',

  failedToStart: 'Não foi possível iniciar a solicitação.',
  noAnswer: 'O processo foi concluído sem informar o resultado.',
  failed: 'A operação não foi concluída.',
};
