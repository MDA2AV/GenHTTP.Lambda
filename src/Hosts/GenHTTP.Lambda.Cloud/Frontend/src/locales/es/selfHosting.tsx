import type { Messages } from '../en';

export const selfHosting: Messages['selfHosting'] = {
  menu: 'Autoalojamiento',

  eyebrow: 'Código abierto · en la nube o autoalojado',
  title: 'Úsalo aquí,',
  titleAccent: 'o autoalójalo.',
  intro:
    'GenHTTP Lambda es de código abierto con licencia MIT. genhttp.dev lo ejecuta para que lo use quien quiera, y la edición Community es la misma plataforma como imagen Docker, para quien prefiera ejecutarla por su cuenta. Las dos crean y alojan las mismas apps; lo que cambia es dónde viven y quién se ocupa de ellas.',

  cloud: {
    kicker: 'Nube',
    name: 'genhttp.dev',
    summary: 'Abre este sitio y empieza. El servidor, sus actualizaciones y sus límites los cuidamos nosotros.',
    points: ['Nada que instalar ni ejecutar', 'Las apps responden en una dirección de genhttp.run', 'El agente de creación incluido'],
    action: 'Crea algo',
  },
  community: {
    kicker: 'Autoalojado',
    name: 'Edición Community',
    summary: 'Un contenedor Docker en una máquina tuya. El servidor, sus actualizaciones y sus límites los cuidas tú.',
    points: ['Un contenedor, un volumen, sin servidor de base de datos', 'Las apps responden en tu dirección', 'El agente de creación como contenedor adicional'],
    action: 'Ejecútalo tú mismo',
  },

  sameTitle: 'Igual en las dos',
  sameText: 'La edición Community no es una versión reducida. Cada app recibe lo mismo allí donde se ejecute:',
  same: [
    'El agente de creación',
    'Tu propio agente por MCP',
    'La API REST',
    'El editor, simple y completo',
    'Borradores con datos de prueba',
    'Versiones, y vuelta atrás',
    'Registros, archivos, claves y contraseñas',
    'Analítica sin cookies',
    'Dominios propios',
    'Git y descargas',
    'La galería y el código publicado',
  ],

  differTitle: 'Qué cambia',
  aspect: 'Pregunta',
  rows: (offline) => [
    ['Dónde se ejecutan las apps', 'En nuestro servidor', 'En el tuyo, en un contenedor con un volumen'],
    ['Cómo empezar', 'Abre la página de creación, sin cuenta', 'Inicia la imagen y abre su dirección'],
    ['Quién cuida el servidor', 'Nosotros', 'Tú'],
    ['Cuánto cuesta', 'Nada; apps Premium bajo consulta', 'Nada; el servidor corre de tu cuenta'],
    ['Dónde responde una app', 'En un subdominio de genhttp.run', 'Bajo tu dirección, o en subdominios de un dominio que le des'],
    ['Un dominio propio', 'Para apps Premium', 'Para cualquier app que pases a producción'],
    ['Apps que nadie usa', `Se desconectan tras ${offline} días sin visitas ni cambios, salvo las Premium`, 'Como tú lo configures'],
    ['Límites y reglas', 'Los nuestros, y nuestras condiciones de uso', 'Los tuyos, definidos en el panel de administración'],
    ['El agente de creación', 'Incluido, con un número de creaciones al día', 'Opcional, con tu propia suscripción de Claude o clave de API'],
    ['Informes de uso', 'Ninguno', 'Una vez al día: la versión, la máquina, las opciones y recuentos de las apps, nunca una app ni una persona. Puedes desactivarlo.'],
  ],

  moveTitle: 'Pasar de una a otra',
  move:
    'Las mismas apps funcionan en las dos, y tu agente pasa una con git: crea la app en el otro lado, copia los archivos de la antigua y los envía con push. Una descarga es un proyecto .NET que funciona en cualquier sitio, con sus registros en un archivo SQLite.',

  pickTitle: 'Cuál te conviene',
  pickCloud: {
    title: 'genhttp.dev, si',
    points: [
      'quieres una app en línea sin ejecutar nada',
      'la gente que la usa está en cualquier parte',
      'prefieres no ocuparte de un servidor',
    ],
  },
  pickCommunity: {
    title: 'La edición Community, si',
    points: [
      'tus apps y sus registros deben quedarse en una máquina tuya',
      'son herramientas para tu equipo, dentro de tu propia red',
      'quieres definir tú los límites y las reglas',
    ],
  },

  startTitle: 'Cómo empezar',
  startCloud: {
    title: 'En genhttp.dev',
    text: 'Describe lo que necesitas en la página de creación, o conecta el agente que ya usas. No hay que crear ninguna cuenta.',
  },
  startCommunity: {
    title: 'En tu servidor',
    text: 'Un comando lo pone en marcha. Abre su dirección, conecta tu agente y crea; el README explica el resto.',
  },
  connect: 'Conecta tu agente',
  readme: 'Lee el README',
  dockerHub: 'La imagen en Docker Hub',

  sourceTitle: 'Código abierto',
  source: (github) => (
    <>
      GenHTTP Lambda se publica con licencia MIT. Su código, sus incidencias y sus versiones están en {github('GitHub')}.
    </>
  ),
};
