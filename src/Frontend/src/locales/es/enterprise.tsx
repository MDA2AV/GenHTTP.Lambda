import type { Messages } from '../en';

export const enterprise: Messages['enterprise'] = {
  eyebrow: 'Empresas',
  title: 'Gratis para probar, tuyo para gestionar',
  intro:
    'Todo aquí es gratis y sin cuenta. ¿Tu equipo necesita apps que sigan en línea para siempre, con su propio inicio de sesión? Consigue una instalación propia, en la nube o en tus servidores.',

  free: 'Gratis',
  freeTagline: 'Para probar cosas',
  forever: 'para siempre',
  buildOne: 'Crea tu app',
  freeFeatures: (offline, removed) => [
    'Lambdas ilimitadas, sin cuenta',
    'El agente integrado, o el tuyo por MCP',
    'En línea mientras se use',
    `Se desconecta tras ${offline} días sin visitas y se elimina a los ${removed} días`,
    'En un subdominio del dominio compartido',
  ],
  freeNote: 'Sin tarjeta y sin registro. Crea una lambda y es tuya.',

  name: 'Enterprise',
  tagline: 'Para equipos que quieren su propia instancia',
  perUser: 'por usuario al mes',
  contact: 'Escríbenos',
  features: [
    'Tu propia instancia, en la nube o en tus servidores',
    'Un solo servicio ejecuta todas las apps',
    'Inicio de sesión con tu propio SSO',
    'Tus reglas de gobernanza y cumplimiento, integradas',
    'Las apps siguen en línea para siempre; nunca se elimina nada',
    'Usa tu propio agente por MCP',
    'Soporte prioritario',
  ],
  users: (count) => <>{count} usuarios</>,
  perMonth: ' / mes',
  price: (amount) => `${amount} USD`,
  perUserPrice: (amount) => `${amount} USD por usuario al mes`,

  compareTitle: 'Compara los planes',
  compareText: 'Los dos usan la misma plataforma. Lo que cambia es cuánto tiempo guarda tu app, y dónde.',
  included: 'Incluido',
  notIncluded: 'No incluido',
  groups: (offline, removed) => [
    {
      title: 'Creación',
      rows: [
        ['Lambdas', 'Ilimitadas', 'Ilimitadas'],
        ['Agente integrado', true, false],
        ['Tu propio agente por MCP', true, true],
        ['Editor, versiones y logs', true, true],
        ['Galería', true, 'Propia'],
      ],
    },
    {
      title: 'Hosting',
      rows: [
        ['Se desconecta si no se usa', `Tras ${offline} días`, 'Nunca'],
        ['Se elimina si no se usa', `Tras ${removed} días`, 'Nunca'],
        ['Instancia', 'Compartida', 'Propia'],
        ['Dónde se ejecuta', 'En nuestra nube', 'Nube o servidores propios'],
        ['Qué gestionas tú', 'Nada', 'Un solo servicio'],
        ['Dominios propios', false, true],
      ],
    },
    {
      title: 'Control',
      rows: [
        ['Inicio de sesión', 'No hace falta', 'Tu propio SSO'],
        ['Tus reglas de gobernanza y cumplimiento para agentes', false, true],
        ['Consola de administración', false, true],
        ['Datos separados de otros clientes', false, true],
        ['Soporte', 'Comunidad', 'Prioritario'],
      ],
    },
  ],

  questionsTitle: 'Preguntas',
  questions: [
    ['¿Necesito una cuenta para empezar?', 'No. Una lambda gratuita solo necesita el enlace de edición que recibes al crearla.'],
    [
      '¿Quién cuenta como usuario en Enterprise?',
      'Todas las personas que inician sesión con tu SSO, ya sea para crear en el editor o para usar una app desplegada en tu instalación. Quien abre una app sin iniciar sesión no cuenta.',
    ],
    [
      '¿El agente integrado está incluido en Enterprise?',
      'No. Tu equipo trae su propio agente (Claude, Claude Code o cualquier otro que hable MCP) y lo conecta a tu instalación, con el plan que ya tenga con su proveedor.',
    ],
    [
      '¿Cómo aprenden los agentes nuestras reglas de cumplimiento?',
      'Integramos tus reglas de gobernanza y cumplimiento en lo que la plataforma les dice a los agentes por MCP. Cada agente que conecta tu equipo las recibe mientras escribe código, así que las apps ya salen cumpliendo tus reglas, sin que todos tengan que sabérselas de memoria.',
    ],
    [
      '¿Necesitamos Kubernetes o un clúster?',
      'No. Todas las apps se ejecutan dentro de un solo servicio, así que no hay pods que repartir ni nada que orquestar por app. Gestionar la instalación es gestionar ese único servicio.',
    ],
    [
      '¿Dónde se ejecuta una instalación Enterprise?',
      'Donde tú elijas. Podemos alojarla en nuestra nube, o puede ejecutarse en una cuenta de nube tuya o en tus propios servidores: en cualquier lugar donde se ejecuten contenedores. En ambos casos te ayudamos a ponerla en marcha y a mantenerla actualizada.',
    ],
  ],
  anythingElse: (mail) => <>¿Algo más? Escríbenos a {mail}.</>,
};
