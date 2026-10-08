import type { Messages } from '../en';

export const enterprise: Messages['enterprise'] = {
  eyebrow: 'Empresas',
  title: 'Grátis para experimentar. Depois, é todo vosso.',
  intro:
    'Tudo aqui é grátis e sem conta. Quando a vossa equipa precisar de apps que fiquem online para sempre, com início de sessão próprio, há uma instalação só vossa, na cloud ou nos vossos servidores.',

  free: 'Grátis',
  freeTagline: 'Para experimentar',
  forever: 'para sempre',
  buildOne: 'Criar uma app',
  freeFeatures: (offline, removed) => [
    'Lambdas ilimitadas, sem conta',
    'O agente integrado, ou o vosso via MCP',
    'Online enquanto for usada',
    `Offline ao fim de ${offline} dias sem visitas, removida ao fim de ${removed} dias`,
    'Servida num subdomínio do domínio partilhado',
  ],
  freeNote: 'Sem cartão, sem registo. Criem uma lambda e é vossa.',

  name: 'Enterprise',
  tagline: 'Para equipas que querem uma instância própria',
  perUser: 'por utilizador / mês',
  contact: 'Falar connosco',
  features: [
    'Instância própria, na cloud ou nos vossos servidores',
    'Um único serviço corre todas as apps',
    'Início de sessão com o vosso SSO',
    'As vossas regras de governação e compliance incluídas',
    'As apps ficam sempre online, nada é removido',
    'O vosso próprio agente via MCP',
    'Suporte prioritário',
  ],
  users: (count) => <>{count} utilizadores</>,
  perMonth: ' / mês',
  price: (amount) => `${amount} $`,
  perUserPrice: (amount) => `${amount} $ / utilizador / mês`,

  compareTitle: 'Comparar os planos',
  compareText: 'Os dois correm na mesma plataforma. O que muda é quanto tempo a vossa app fica guardada, e onde.',
  included: 'Incluído',
  notIncluded: 'Não incluído',
  groups: (offline, removed) => [
    {
      title: 'Criação',
      rows: [
        ['Lambdas', 'Ilimitadas', 'Ilimitadas'],
        ['Agente integrado', true, false],
        ['O vosso agente via MCP', true, true],
        ['Editor, versões e logs', true, true],
        ['Montra', true, 'Própria'],
      ],
    },
    {
      title: 'Alojamento',
      rows: [
        ['Offline por falta de uso', `Ao fim de ${offline} dias`, 'Nunca'],
        ['Removida por falta de uso', `Ao fim de ${removed} dias`, 'Nunca'],
        ['Instância', 'Partilhada', 'Própria'],
        ['Onde corre', 'Na nossa cloud', 'Cloud ou servidores próprios'],
        ['O que têm de gerir', 'Nada', 'Um único serviço'],
        ['Domínios próprios', false, true],
      ],
    },
    {
      title: 'Controlo',
      rows: [
        ['Início de sessão', 'Não é preciso', 'O vosso SSO'],
        ['As vossas regras de governação e compliance para agentes', false, true],
        ['Consola de administração', false, true],
        ['Dados separados dos outros clientes', false, true],
        ['Suporte', 'Comunidade', 'Prioritário'],
      ],
    },
  ],

  questionsTitle: 'Perguntas',
  questions: [
    ['Preciso de conta para começar?', 'Não. Uma lambda grátis só precisa do link de edição que se recebe ao criá-la.'],
    [
      'Quem conta como utilizador no Enterprise?',
      'Toda a gente que inicia sessão pelo vosso SSO, seja para criar no editor ou para usar uma app publicada na vossa instalação. Quem abre uma app sem iniciar sessão não conta.',
    ],
    [
      'O agente integrado está incluído no Enterprise?',
      'Não. A vossa equipa traz o seu próprio agente (Claude, Claude Code ou qualquer outro que fale MCP) e liga-o à vossa instalação, com o plano que já tem com esse fornecedor.',
    ],
    [
      'Como é que os agentes aprendem as nossas regras de compliance?',
      'Integramos as vossas regras de governação e compliance naquilo que a plataforma diz aos agentes via MCP. Cada agente que a vossa equipa liga recebe-as enquanto escreve código, por isso as apps já saem a cumprir as vossas regras, sem que toda a gente as tenha de saber de cor.',
    ],
    [
      'Precisamos de Kubernetes ou de um cluster?',
      'Não. Todas as apps correm dentro de um único serviço, por isso não há pods para distribuir nem nada para orquestrar por app. Manter a instalação a funcionar é manter esse serviço a correr.',
    ],
    [
      'Onde corre uma instalação Enterprise?',
      'Onde escolherem. Podemos alojá-la nós, na nossa cloud, ou pode correr numa conta cloud vossa ou nos vossos próprios servidores: em qualquer sítio que corra containers. Em qualquer dos casos, ajudamos a configurá-la e a mantê-la atualizada.',
    ],
  ],
  anythingElse: (mail) => <>Mais alguma dúvida? Escrevam para {mail}.</>,
};
