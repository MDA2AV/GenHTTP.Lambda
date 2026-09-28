import type { Messages } from '../en';

export const build: Messages['build'] = {
  title: 'De la idea a la web.',
  intro:
    'Describa la página web o la aplicación que tiene en mente. La IA la crea por usted, nosotros la alojamos en nuestros servidores y queda en línea al instante, con un enlace que puede enviar a quien quiera. Sin programar, sin configurar un alojamiento, sin cuenta.',
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
    'un libro de visitas para nuestra boda',
    'una encuesta donde la gente vota y ve los resultados',
    'un marcador para nuestra noche de preguntas semanal',
    'una cuenta atrás para nuestra inauguración que todos puedan ver',
  ],
  ahead: (waiting) =>
    waiting === 1 ? 'Hay una web por delante de la suya: después le toca a usted.' : `Hay ${waiting} webs por delante de la suya.`,
  starting: 'Iniciando…',

  points: [
    {
      title: 'Descrita, no programada',
      text: 'Explique con sus propias palabras qué debe hacer su web. No hace falta programar ni tener conocimientos técnicos.',
    },
    {
      title: 'Alojamiento incluido',
      text: 'Su web funciona en nuestros servidores. Nos ocupamos del alojamiento, la seguridad y las actualizaciones: usted no tiene nada que configurar ni mantener.',
    },
    {
      title: 'En línea en minutos',
      text: 'Recibe al instante un enlace para compartir. La web también puede guardar datos (inscripciones, votos, puntuaciones) para que todos vean lo mismo.',
    },
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
