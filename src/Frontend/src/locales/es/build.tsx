import type { Messages } from '../en';

export const build: Messages['build'] = {
  title: 'Crear una página web con IA.',
  intro:
    'Describa con sus propias palabras la página web o la app que tiene en mente. La IA la crea por usted, nosotros la alojamos y queda en línea en minutos, con un enlace que puede enviar a quien quiera. Gratis, sin programar y sin registrarse.',
  placeholder: 'Quiero una página web que…',
  working: 'trabajando…',
  shortcut: 'ctrl + enter',
  building: 'Creando',
  buildIt: 'Crear mi web',
  builtBy: 'Creado con',
  password: 'contraseña',
  fable:
    'Fable está protegido con contraseña mientras se prueba. No tiene límite de tiempo, así que sigue trabajando hasta que su web esté terminada, no hasta que se acabe el reloj.',
  onlyNew:
    'Aquí se crean webs nuevas. Para cambiar una que ya tiene, abra su enlace de edición y describa en «Cambiar» qué debe ser diferente.',
  ideas: [
    'una web para nuestro club donde los socios se apuntan a eventos',
    'una lista para nuestra comida compartida, para que nadie lleve el mismo plato',
    'un libro de visitas para nuestra boda',
    'una encuesta donde la gente vota y ve los resultados',
    'un ranking para nuestra noche de preguntas semanal',
    'una página de cumpleaños donde los amigos dejan sus felicitaciones',
  ],
  ahead: (waiting) =>
    waiting === 1 ? 'Hay una web por delante de la suya: después le toca a usted.' : `Hay ${waiting} webs por delante de la suya.`,
  starting: 'Iniciando…',
  steps: {
    guide: 'Preparándose',
    examples: 'Mirando ejemplos',
    create: 'Eligiendo una dirección para su web',
    write: 'Creando su web',
    improve: 'Mejorando su web',
    check: 'Buscando errores',
    online: 'Poniéndola en línea',
    trying: 'Probándola',
    looking: 'Revisando su web',
    forRecords: 'Preparando espacio para sus registros',
    forKeys: 'Preparando espacio para claves y contraseñas',
    forFiles: 'Preparando espacio para lo que guarde',
    records: 'Mirando sus registros',
    keys: 'Comprobando qué claves y contraseñas necesita',
    addFile: 'Añadiendo un archivo',
    removeFile: 'Quitando un archivo',
    files: 'Mirando lo que ha guardado',
    isOnline: 'en línea',
  },

  points: [
    {
      title: 'Sin saber programar',
      text: 'Explique con sus propias palabras qué debe hacer su web, como se lo contaría a un amigo. La IA la crea por usted, sin necesidad de conocimientos técnicos.',
    },
    {
      title: 'Hosting gratis incluido',
      text: 'Su web funciona en nuestros servidores. Sin plan de hosting, sin servidor ni dominio que comprar y sin nada que instalar: nos ocupamos de la seguridad y las actualizaciones.',
    },
    {
      title: 'En línea en minutos',
      text: 'Recibe al instante un enlace para compartir. La web recuerda lo que la gente envía (inscripciones, votos, mensajes, puntuaciones), para que todos vean lo mismo.',
    },
  ],

  questionsTitle: 'Antes de empezar',
  questions: (offline, removed) => [
    [
      '¿De verdad la IA puede crear mi página web gratis?',
      `Sí. Descríbala con sus propias palabras y la IA la crea, la pone en línea y le da el enlace. Sin registro, sin tarjeta, sin periodo de prueba. Sigue en línea mientras se use: tras ${offline} días sin visitas ni cambios se desconecta, y tras ${removed} días se elimina.`,
    ],
    [
      '¿Necesito hosting, un servidor o un dominio?',
      'No. Su web funciona en nuestros servidores, con hosting, seguridad y actualizaciones incluidos. Recibe un enlace al instante, así que tampoco tiene que comprar un dominio.',
    ],
    [
      '¿Puedo crear una app sin saber programar?',
      'Sí. Nunca verá código. Diga qué debe hacer, como se lo contaría a un amigo, y la IA hace el resto: una web, una pequeña app o un juego.',
    ],
    [
      '¿La gente puede apuntarse, votar o dejar mensajes?',
      'Sí. Su web recuerda lo que la gente envía, así que todos los que abren el enlace ven las mismas inscripciones, votos y puntuaciones.',
    ],
    [
      '¿Cómo la abren los demás?',
      'Con el enlace, en cualquier navegador, desde el teléfono o desde el PC. No hay nada que instalar ni tiendas de apps de por medio.',
    ],
    [
      '¿Cómo la cambio más adelante?',
      'Abra el enlace de edición que recibe con su web y describa qué debe ser diferente, igual que aquí. Si un cambio no le gusta, puede volver a como estaba antes.',
    ],
  ],

  yourApp: 'Su web',
  further: 'Para cambiarla más adelante',
  keep:
    'Guarde ese enlace. Es la única forma de volver a entrar y no se puede recuperar, ni siquiera nosotros podemos. Añádalo a sus marcadores antes de cerrar esta pestaña.',
  change:
    'Para cambiar su web, abra el enlace de edición y describa en «Cambiar» qué debe ser diferente, igual que aquí. También puede hacerlo su propio asistente de IA, como se explica más abajo.',
  copyLink: 'Copiar el enlace de edición',
  lifetime: (offline, removed) =>
    `La mantenemos en línea mientras se use: tras ${offline} días sin visitas ni cambios se desconecta, y tras ${removed} días se elimina. Abra el editor para volver a ponerla en línea.`,
  openEditor: 'Abrir el editor',
  another: 'Crear otra web',

  keepGoing: 'Continúe con su propio asistente de IA',
  orOwn: 'O use su propio asistente de IA',
  ownText:
    '¿Ya trabaja con Claude u otro asistente de IA? Conéctelo aquí y creará y cambiará webs para usted de la misma manera. Nosotros las alojamos, así que sigue sin tener nada que configurar. No hay límite diario.',
  ownTitle: 'Cree su web con su asistente de IA',
  ownOnly:
    'Conecte Claude u otro asistente de IA a la dirección de abajo y describa la web que desea. El asistente la crea, nosotros la alojamos en nuestros servidores y queda en línea al instante, con un enlace para compartir.',
  thenAsk:
    'Después, dígale lo que desea, por ejemplo: «Crea una web para nuestro coro con un calendario de nuestros conciertos».',
  howToChange:
    'Así también se cambia una web más adelante: dé a su asistente el enlace de edición y dígale qué debe ser diferente.',

  failedToStart: 'No se ha podido enviar.',
  noAnswer: 'Terminó sin indicar qué ha pasado.',
  failed: 'No ha funcionado.',
};
