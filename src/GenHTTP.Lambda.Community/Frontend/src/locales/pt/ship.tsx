import type { Messages } from '../en';

export const ship: Messages['ship'] = {
  eyebrow: 'Conecte seu agente',
  title: 'Publique a partir do agente que você já usa.',
  intro:
    'O Claude Code, o Codex, o Cursor ou qualquer agente que fale MCP recebe esta instalação como ferramentas: ele cria um app, testa uma mudança em um rascunho, a coloca no ar e lê os logs. Cada app responde em um endereço próprio, com o seu próprio banco de dados.',
  stepsTitle: 'Três passos para um app no ar',
  step: (n) => `Passo ${n}`,
  steps: [
    {
      title: 'Conecte uma vez',
      body: 'Adicione ao seu agente o endereço do servidor MCP desta instalação. Ele não precisa de chave: quem tem a chave de edição de um app pode mudar esse app, e nada além dele.',
    },
    {
      title: 'Peça para criar ou publicar',
      body: 'Descreva um app novo, ou peça para publicar o que está no seu projeto. Ele escreve o código, confere se compila e o coloca no ar.',
    },
    {
      title: 'Guarde o link de edição',
      body: 'Você recebe o endereço do app e o link de edição dele. O endereço é para todo mundo; o link de edição é como você - ou o seu agente - muda o app depois.',
    },
  ],
  sayLike: 'Depois diga algo como',
  asks: ['Crie um livro de visitas, com uma página onde eu aprovo as mensagens.', 'Publique este app aqui e me envie o link.'],
  noAgent: 'Sem agente? Use o agente de criação',

  questionsTitle: 'Bom saber',
  questions: (create) => [
    [
      'Quais agentes podem se conectar?',
      'Qualquer agente que consiga adicionar um servidor MCP remoto via HTTP: Claude Code, Claude, Codex, Cursor, VS Code e outros, no endereço acima.',
    ],
    [
      'Como mudo um app depois?',
      'Dê ao seu agente o link de edição do app junto com a próxima mudança, ou abra você mesmo o link: o editor mostra as versões, o código, os registros e os logs. Uma mudança é testada em um rascunho, com uma cópia dos dados, antes de ir ao ar.',
    ],
    [
      'Posso trabalhar em um app com git?',
      'Sim. Todo app é um repositório git: clone-o pelo editor, faça push de um commit para main para salvar uma versão, ou faça push de um branch para iniciar um rascunho com uma prévia própria.',
    ],
    [
      'Onde ficam as chaves de API e as senhas?',
      'Não no código. O agente as lê pelo nome, e você digita os valores no editor, onde ninguém os lê de volta - nem o agente.',
    ],
    [
      'Posso escrever um app sem um agente?',
      <>Sim: {create('escreva o código você mesmo')}, a partir de uma das demos. Um app é um handler C# do GenHTTP, compilado neste servidor.</>,
    ],
  ],
};
