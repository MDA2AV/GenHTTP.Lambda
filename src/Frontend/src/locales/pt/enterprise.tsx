import type { Messages } from '../en';

export const enterprise: Messages['enterprise'] = {
  eyebrow: 'Empresas',
  title: 'Teste grátis. Rode onde quiser.',
  intro:
    'Tudo aqui é grátis e sem conta. Quando sua equipe precisar de apps que fiquem no ar para sempre, com o login da empresa, tenha uma instalação própria, na nuvem ou nos seus servidores.',

  free: 'Grátis',
  freeTagline: 'Para experimentar',
  forever: 'para sempre',
  buildOne: 'Criar um app',
  freeFeatures: (offline, removed) => [
    'Lambdas ilimitadas, sem conta',
    'O agente integrado, ou o seu via MCP',
    'No ar enquanto estiver em uso',
    `Sai do ar após ${offline} dias sem visitas e é removida após ${removed} dias`,
    'Servida em um caminho do servidor compartilhado',
  ],
  freeNote: 'Sem cartão, sem cadastro. Crie uma lambda e ela é sua.',

  name: 'Enterprise',
  tagline: 'Para equipes que querem uma instância própria',
  perUser: 'por usuário / mês',
  contact: 'Fale com a gente',
  features: [
    'Sua própria instância, na nuvem ou nos seus servidores',
    'Um único serviço roda todos os apps',
    'Login com o seu SSO',
    'Suas regras de governança e compliance já integradas',
    'Os apps ficam no ar para sempre, nada é removido',
    'Traga seu próprio agente via MCP',
    'Suporte prioritário',
  ],
  users: (count) => <>{count} usuários</>,
  perMonth: ' / mês',
  price: (amount) => `US$ ${amount}`,
  perUserPrice: (amount) => `US$ ${amount} / usuário / mês`,

  compareTitle: 'Compare os planos',
  compareText: 'Os dois rodam a mesma plataforma. O que muda é por quanto tempo seu app fica guardado, e onde.',
  included: 'Incluído',
  notIncluded: 'Não incluído',
  groups: (offline, removed) => [
    {
      title: 'Criação',
      rows: [
        ['Lambdas', 'Ilimitadas', 'Ilimitadas'],
        ['Agente integrado', true, false],
        ['Seu agente via MCP', true, true],
        ['Editor, versões e logs', true, true],
        ['Vitrine', true, 'Própria'],
      ],
    },
    {
      title: 'Hospedagem',
      rows: [
        ['Sai do ar sem uso', `Após ${offline} dias`, 'Nunca'],
        ['É removida sem uso', `Após ${removed} dias`, 'Nunca'],
        ['Instância', 'Compartilhada', 'Própria'],
        ['Onde roda', 'Na nossa nuvem', 'Nuvem ou seus servidores'],
        ['O que você opera', 'Nada', 'Um único serviço'],
        ['Domínios próprios', false, true],
      ],
    },
    {
      title: 'Controle',
      rows: [
        ['Login', 'Não precisa', 'Seu próprio SSO'],
        ['Suas regras de governança e compliance para agentes', false, true],
        ['Console de administração', false, true],
        ['Dados separados dos outros clientes', false, true],
        ['Suporte', 'Comunidade', 'Prioritário'],
      ],
    },
  ],

  questionsTitle: 'Perguntas',
  questions: [
    ['Preciso de conta para começar?', 'Não. Uma lambda grátis só precisa do link de edição que você recebe ao criar.'],
    [
      'Quem conta como usuário no Enterprise?',
      'Todo mundo que entra pelo seu SSO, seja para criar no editor ou para usar um app publicado na sua instalação. Quem acessa um app sem fazer login não conta.',
    ],
    [
      'O agente integrado está incluído no Enterprise?',
      'Não. Sua equipe traz o próprio agente (Claude, Claude Code ou qualquer outro que fale MCP) e conecta à sua instalação, no plano que vocês já têm com o fornecedor dele.',
    ],
    [
      'Como os agentes aprendem nossas regras de compliance?',
      'Integramos suas regras de governança e compliance ao que a plataforma informa aos agentes via MCP. Todo agente que sua equipe conecta recebe essas regras enquanto escreve código. Assim, os apps já saem seguindo suas regras, sem ninguém precisar saber tudo de cor.',
    ],
    [
      'Precisamos de Kubernetes ou de um cluster?',
      'Não. Todos os apps rodam dentro de um único serviço, então não há pods para distribuir nem nada para orquestrar por app. Rodar a instalação é rodar esse serviço.',
    ],
    [
      'Onde roda uma instalação Enterprise?',
      'Onde você quiser. Podemos hospedar para você na nossa nuvem, ou ela roda em uma conta de nuvem sua ou nos seus próprios servidores: em qualquer lugar que rode containers. Nos dois casos, ajudamos a configurar e a manter tudo atualizado.',
    ],
  ],
  anythingElse: (mail) => <>Mais alguma dúvida? Escreva para {mail}.</>,
};
