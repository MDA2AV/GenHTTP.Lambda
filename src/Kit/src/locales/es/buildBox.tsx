import type { KitMessages } from '../en';

export const buildBox: KitMessages['buildBox'] = {
  placeholder: 'Quiero una página web que…',
  shortcut: 'ctrl + enter',
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
  asked: 'Lo que pidió',
  leaveOpen: 'Mantenga esta página abierta: el enlace para cambiar su web más adelante solo aparece aquí, cuando esté lista.',
  log: 'Lo que hizo la IA',
  online: 'Su web está en línea',
  notOnline: 'Su web se ha creado, pero no se ha puesto en línea.',
  open: 'Abrir su web',
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
  },

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
