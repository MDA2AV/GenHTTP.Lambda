import type { Messages } from '../en';

export const ship: Messages['ship'] = {
  eyebrow: 'Liga o teu agente',
  title: 'Publica a partir do agente que já usas.',
  intro:
    'O Claude Code, o Codex, o Cursor ou qualquer agente que fale MCP recebe esta instalação como ferramentas: cria uma app, experimenta uma alteração num rascunho, põe-na online e lê os logs. Cada app responde num endereço próprio, com a sua própria base de dados.',
  stepsTitle: 'Três passos até uma app online',
  step: (n) => `Passo ${n}`,
  steps: [
    {
      title: 'Liga uma só vez',
      body: 'Adiciona ao teu agente o endereço do servidor MCP desta instalação. Não precisa de chave: quem tem a chave de edição de uma app pode alterar essa app, e mais nada.',
    },
    {
      title: 'Pede-lhe que crie ou publique',
      body: 'Descreve uma app nova, ou pede-lhe que publique a que está no teu projeto. Ele escreve o código, verifica que compila e põe-no online.',
    },
    {
      title: 'Guarda o link de edição',
      body: 'Recebes o endereço da app e o respetivo link de edição. O endereço é para toda a gente; o link de edição é a forma de tu - ou o teu agente - alterares a app mais tarde.',
    },
  ],
  sayLike: 'Depois diz algo como',
  asks: ['Cria um livro de visitas, com uma página onde eu aprovo as entradas.', 'Publica esta app aqui e envia-me o link.'],
  noAgent: 'Não tens agente? Usa o agente de criação',

  questionsTitle: 'Bom saber',
  questions: (create) => [
    [
      'Que agentes se podem ligar?',
      'Qualquer agente que consiga adicionar um servidor MCP remoto por HTTP: Claude Code, Claude, Codex, Cursor, VS Code e outros, no endereço acima.',
    ],
    [
      'Como altero uma app mais tarde?',
      'Dá ao teu agente o link de edição da app com a alteração seguinte, ou abre tu o link: o editor mostra as suas versões, código, registos e logs. Uma alteração é experimentada num rascunho, com uma cópia dos dados, antes de ficar online.',
    ],
    [
      'Posso trabalhar numa app com git?',
      'Sim. Cada app é um repositório git: clona-a a partir do editor, faz push de um commit para main para guardar uma versão, ou faz push de um ramo para iniciar um rascunho com uma pré-visualização própria.',
    ],
    [
      'Onde ficam as chaves de API e as palavras-passe?',
      'Não no código. O agente lê-as pelo nome, e tu introduzes os valores no editor, onde ninguém os volta a ler - nem o agente.',
    ],
    [
      'Posso escrever uma app sem agente?',
      <>Sim: {create('escreve tu o código')}, a partir de uma das demos. Uma app é um handler GenHTTP em C#, compilado neste servidor.</>,
    ],
  ],
  profiles: {
    enterprise: {
      asks: [
        'Cria um formulário para pedir férias, com uma página onde o chefe de equipa aprova cada pedido.',
        'Publica esta app aqui e envia-me o link.',
      ],
    },
  },
};
