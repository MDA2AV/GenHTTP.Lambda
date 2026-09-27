import type { Messages } from '../en';

export const enterprise: Messages['enterprise'] = {
  eyebrow: 'Empresas',
  title: 'Pruébelo gratis, gestiónelo usted mismo',
  intro:
    'Todo lo que ofrecemos aquí es gratuito y no requiere cuenta. Cuando su organización necesite aplicaciones que permanezcan en línea de forma permanente, con su propio inicio de sesión, puede disponer de una instalación propia, en la nube o en sus instalaciones.',

  free: 'Gratuito',
  freeTagline: 'Para probar',
  forever: 'sin límite de tiempo',
  buildOne: 'Crear una aplicación',
  freeFeatures: (offline, removed) => [
    'Lambdas ilimitados, sin cuenta',
    'El agente integrado o el suyo propio mediante MCP',
    'En línea mientras se utilice',
    `Desconectado tras ${offline} días sin visitas, eliminado tras ${removed} días`,
    'Disponible en una ruta del servidor compartido',
  ],
  freeNote: 'Sin tarjeta ni registro. Cree un lambda y será suyo.',

  name: 'Enterprise',
  tagline: 'Para organizaciones que desean una instancia propia',
  perUser: 'por usuario y mes',
  contact: 'Contactar',
  features: [
    'Instancia propia, en la nube o en sus instalaciones',
    'Un único servicio ejecuta todas las aplicaciones',
    'Inicio de sesión con su propio SSO',
    'Sus normas de gobernanza y cumplimiento integradas',
    'Las aplicaciones permanecen en línea y nunca se eliminan',
    'Sus propios agentes mediante MCP',
    'Soporte prioritario',
  ],
  users: (count) => <>{count} usuarios</>,
  perMonth: ' / mes',
  price: (amount) => `${amount} USD`,
  perUserPrice: (amount) => `${amount} USD por usuario y mes`,

  compareTitle: 'Comparativa de planes',
  compareText:
    'Ambos planes funcionan sobre la misma plataforma. La diferencia está en cuánto tiempo se conserva su aplicación y dónde se ejecuta.',
  included: 'Incluido',
  notIncluded: 'No incluido',
  groups: (offline, removed) => [
    {
      title: 'Desarrollo',
      rows: [
        ['Lambdas', 'Ilimitados', 'Ilimitados'],
        ['Agente integrado', true, false],
        ['Su propio agente mediante MCP', true, true],
        ['Editor, versiones y registros', true, true],
        ['Galería', true, 'Propia'],
      ],
    },
    {
      title: 'Alojamiento',
      rows: [
        ['Desconexión por inactividad', `Tras ${offline} días`, 'Nunca'],
        ['Eliminación por inactividad', `Tras ${removed} días`, 'Nunca'],
        ['Instancia', 'Compartida', 'Propia'],
        ['Ejecución', 'En nuestra nube', 'Nube o instalaciones propias'],
        ['Lo que usted gestiona', 'Nada', 'Un único servicio'],
        ['Dominios propios', false, true],
      ],
    },
    {
      title: 'Control',
      rows: [
        ['Inicio de sesión', 'No necesario', 'Su propio SSO'],
        ['Sus normas de gobernanza y cumplimiento para agentes', false, true],
        ['Consola de administración', false, true],
        ['Datos separados de otros clientes', false, true],
        ['Soporte', 'Comunidad', 'Prioritario'],
      ],
    },
  ],

  questionsTitle: 'Preguntas frecuentes',
  questions: [
    ['¿Necesito una cuenta para empezar?', 'No. Un lambda gratuito solo requiere el enlace de edición que recibe al crearlo.'],
    [
      '¿Quién cuenta como usuario en Enterprise?',
      'Toda persona que inicie sesión mediante su SSO, ya sea para desarrollar en el editor o para utilizar una aplicación desplegada en su instalación. Quien accede a una aplicación sin iniciar sesión no se contabiliza.',
    ],
    [
      '¿El agente integrado está incluido en Enterprise?',
      'No. Su equipo utiliza su propio agente – Claude, Claude Code o cualquier otro compatible con MCP – y lo conecta a su instalación, con el plan que ya tenga contratado con el proveedor.',
    ],
    [
      '¿Cómo conocen los agentes nuestras normas de cumplimiento?',
      'Integramos sus normas de gobernanza y cumplimiento en la información que la plataforma proporciona a los agentes mediante MCP. Cada agente que su equipo conecta las recibe al escribir código, de modo que las aplicaciones cumplen sus normas sin que todos tengan que conocerlas en detalle.',
    ],
    [
      '¿Necesitamos Kubernetes o un clúster?',
      'No. Todas las aplicaciones se ejecutan dentro de un único servicio, por lo que no hay pods que distribuir ni nada que orquestar por aplicación. Gestionar la instalación significa gestionar ese único servicio.',
    ],
    [
      '¿Dónde se ejecuta una instalación Enterprise?',
      'Donde usted decida. Podemos alojarla en nuestra nube, o puede ejecutarse en una cuenta de nube propia o en sus propios servidores, en cualquier entorno que ejecute contenedores. En ambos casos le ayudamos con la puesta en marcha y la mantenemos actualizada.',
    ],
  ],
  anythingElse: (mail) => <>¿Tiene alguna otra pregunta? Escriba a {mail}.</>,
};
