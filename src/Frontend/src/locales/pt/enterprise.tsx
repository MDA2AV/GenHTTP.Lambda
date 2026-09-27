import type { Messages } from '../en';

export const enterprise: Messages['enterprise'] = {
  eyebrow: 'Empresas',
  title: 'Experimente gratuitamente, opere com autonomia',
  intro:
    'Tudo aqui é gratuito e não exige conta. Quando sua organização precisar de aplicações que permaneçam no ar permanentemente, com login próprio, tenha uma instalação exclusiva – na nuvem ou em seus próprios servidores.',

  free: 'Gratuito',
  freeTagline: 'Para experimentar',
  forever: 'sem prazo',
  buildOne: 'Criar uma aplicação',
  freeFeatures: (offline, removed) => [
    'Lambdas ilimitados, sem conta',
    'O agente integrado ou o seu próprio via MCP',
    'No ar enquanto for utilizado',
    `Fora do ar após ${offline} dias sem acessos, removido após ${removed} dias`,
    'Disponível em um caminho do servidor compartilhado',
  ],
  freeNote: 'Sem cartão e sem cadastro. Crie um lambda e ele é seu.',

  name: 'Enterprise',
  tagline: 'Para organizações que desejam uma instância própria',
  perUser: 'por usuário / mês',
  contact: 'Fale conosco',
  features: [
    'Instância própria, na nuvem ou em seus servidores',
    'Um único serviço executa todas as aplicações',
    'Login com o seu próprio SSO',
    'Suas regras de governança e conformidade integradas',
    'Aplicações permanecem no ar e nunca são removidas',
    'Seus próprios agentes via MCP',
    'Suporte prioritário',
  ],
  users: (count) => <>{count} usuários</>,
  perMonth: ' / mês',
  price: (amount) => `US$ ${amount}`,
  perUserPrice: (amount) => `US$ ${amount} por usuário / mês`,

  compareTitle: 'Compare os planos',
  compareText:
    'Ambos funcionam na mesma plataforma. A diferença está em por quanto tempo sua aplicação é mantida e onde ela é executada.',
  included: 'Incluído',
  notIncluded: 'Não incluído',
  groups: (offline, removed) => [
    {
      title: 'Desenvolvimento',
      rows: [
        ['Lambdas', 'Ilimitados', 'Ilimitados'],
        ['Agente integrado', true, false],
        ['Seu próprio agente via MCP', true, true],
        ['Editor, versões e logs', true, true],
        ['Vitrine', true, 'Própria'],
      ],
    },
    {
      title: 'Hospedagem',
      rows: [
        ['Retirado do ar sem uso', `Após ${offline} dias`, 'Nunca'],
        ['Removido sem uso', `Após ${removed} dias`, 'Nunca'],
        ['Instância', 'Compartilhada', 'Própria'],
        ['Execução', 'Em nossa nuvem', 'Nuvem ou servidores próprios'],
        ['O que você opera', 'Nada', 'Um único serviço'],
        ['Domínios próprios', false, true],
      ],
    },
    {
      title: 'Controle',
      rows: [
        ['Login', 'Não necessário', 'Seu próprio SSO'],
        ['Suas regras de governança e conformidade para agentes', false, true],
        ['Console de administração', false, true],
        ['Dados separados dos de outros clientes', false, true],
        ['Suporte', 'Comunidade', 'Prioritário'],
      ],
    },
  ],

  questionsTitle: 'Perguntas frequentes',
  questions: [
    ['Preciso de uma conta para começar?', 'Não. Um lambda gratuito exige apenas o link de edição que você recebe ao criá-lo.'],
    [
      'Quem conta como usuário no Enterprise?',
      'Todas as pessoas que fazem login pelo seu SSO – seja para desenvolver no editor, seja para usar uma aplicação implantada na sua instalação. Quem acessa uma aplicação sem fazer login não é contabilizado.',
    ],
    [
      'O agente integrado está incluído no Enterprise?',
      'Não. Sua equipe utiliza o próprio agente – Claude, Claude Code ou qualquer outro compatível com MCP – e o conecta à sua instalação, com o plano que já possui junto ao fornecedor.',
    ],
    [
      'Como os agentes conhecem nossas regras de conformidade?',
      'Integramos suas regras de governança e conformidade às informações que a plataforma fornece aos agentes via MCP. Cada agente conectado pela sua equipe as recebe ao escrever código, de modo que as aplicações seguem suas regras sem que todos precisem conhecê-las em detalhe.',
    ],
    [
      'Precisamos de Kubernetes ou de um cluster?',
      'Não. Todas as aplicações são executadas dentro de um único serviço, portanto não há pods a distribuir nem orquestração por aplicação. Operar a instalação significa operar esse único serviço.',
    ],
    [
      'Onde uma instalação Enterprise é executada?',
      'Onde você preferir. Podemos hospedá-la em nossa nuvem, ou ela pode ser executada em uma conta de nuvem sua ou em seus próprios servidores – em qualquer ambiente que execute contêineres. Em ambos os casos, ajudamos na configuração e a mantemos atualizada.',
    ],
  ],
  anythingElse: (mail) => <>Outras dúvidas? Escreva para {mail}.</>,
};
