import type { KitMessages } from '../en';

export const buildBox: KitMessages['buildBox'] = {
  placeholder: 'Quiero una página web que…',
  password: 'contraseña',
  unlimited:
    'Este modelo necesita la contraseña y entonces se ejecuta sin límite de tiempo: sigue hasta terminar su página web en lugar de hasta que se agote el reloj.',
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
    services: 'Pidiendo usar otros servicios',
  },
  further: 'Su enlace de edición',
  keep:
    'Guarde ese enlace en un lugar seguro. Es la única forma de entrar en su sitio web desde otro dispositivo y no se puede recuperar, ni siquiera nosotros podemos.',
  copyLink: 'Copiar el enlace de edición',
  lifetime: (offline, removed) =>
    `La mantenemos en línea mientras se use: tras ${offline} días sin visitas ni cambios se desconecta, y tras ${removed} días se elimina. Puede volver a ponerla en línea aquí o en su editor.`,

  failedToStart: 'No se ha podido enviar.',
  noAnswer: 'Terminó sin indicar qué ha pasado.',
  failed: 'No ha funcionado.',
  profiles: {
    enterprise: {
      placeholder: 'Necesitamos una app que…',
      unlimited:
        'Este modelo necesita la contraseña y entonces se ejecuta sin límite de tiempo: sigue hasta terminar su app en lugar de hasta que se agote el reloj.',
      ideas: [
        'un formulario para solicitar días libres, aprobado por el responsable del equipo',
        'un calendario de reservas para nuestras salas de reuniones',
        'una lista de comprobación para incorporar a los nuevos compañeros',
        'un inventario de nuestros equipos y de quién los tiene',
        'una lista de solicitudes de soporte abiertas y de quién atiende cada una',
        'un formulario de opinión para nuestras formaciones',
      ],
      ahead: (waiting) => (waiting === 1 ? 'Hay una app por delante de la suya: después le toca a usted.' : `Hay ${waiting} apps por delante de la suya.`),
      online: 'Su app está en línea',
      notOnline: 'Su app se ha creado, pero no se ha puesto en línea.',
      open: 'Abrir su app',
      steps: {
        create: 'Eligiendo una dirección para su app',
        write: 'Creando su app',
        improve: 'Mejorando su app',
        looking: 'Revisando su app',
      },
      keep: 'Guarde ese enlace en un lugar seguro. Es la única forma de entrar en su app desde otro dispositivo, y nadie puede recuperarlo, ni siquiera quienes gestionan este sitio.',
    },
  },
};
