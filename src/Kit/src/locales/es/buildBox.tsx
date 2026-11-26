import type { KitMessages } from '../en';

export const buildBox: KitMessages['buildBox'] = {
  placeholder: 'Quiero una app que…',
  password: 'contraseña',
  unlimited:
    'Esta opción necesita la contraseña del sitio y entonces se le dan el tiempo y los pasos fijados para ella, en lugar de los habituales.',
  ideas: [
    'una app para nuestro club donde los socios se apuntan a eventos',
    'una lista para nuestra comida compartida, para que nadie lleve el mismo plato',
    'un libro de visitas para nuestra boda',
    'una encuesta donde la gente vota y ve los resultados',
    'un ranking para nuestra noche de preguntas semanal',
    'una página de cumpleaños donde los amigos dejan sus felicitaciones',
  ],
  ahead: (waiting) =>
    waiting === 1 ? 'Hay una app por delante de la suya: después le toca a usted.' : `Hay ${waiting} apps por delante de la suya.`,
  starting: 'Iniciando…',
  online: 'Su app está en línea',
  notOnline: 'Su app se ha creado, pero no se ha puesto en línea.',
  open: 'Abrir su app',
  steps: {
    guide: 'Preparándose',
    examples: 'Mirando ejemplos',
    create: 'Eligiendo una dirección para su app',
    write: 'Creando su app',
    improve: 'Mejorando su app',
    check: 'Buscando errores',
    online: 'Poniéndola en línea',
    trying: 'Probándola',
    looking: 'Revisando su app',
    forRecords: 'Preparando espacio para sus registros',
    forKeys: 'Preparando espacio para claves y contraseñas',
    forFiles: 'Preparando espacio para lo que guarde',
    records: 'Mirando sus registros',
    keys: 'Comprobando qué claves y contraseñas necesita',
    addFile: 'Añadiendo un archivo',
    removeFile: 'Quitando un archivo',
    files: 'Mirando lo que ha guardado',
    services: 'Pidiendo usar otros servicios',
    site: (site) => `Mirando ${site}`,
    search: 'Buscando algo en la web',
    attachment: 'Mirando el archivo que envió',
    showcase: 'Poniéndola en la galería',
    file: 'Escribiendo un archivo para usted',
  },
  further: 'Su enlace de edición',
  keep:
    'Guarde ese enlace en un lugar seguro. Es la única forma de entrar en su app desde otro dispositivo y no se puede recuperar, ni siquiera nosotros podemos.',
  copyLink: 'Copiar el enlace de edición',
  lifetime: (offline, removed) =>
    `La mantenemos en línea mientras se use: tras ${offline} días sin visitas ni cambios se desconecta, y tras ${removed} días se elimina. Puede volver a ponerla en línea aquí o en su editor.`,

  failedToStart: 'No se ha podido enviar.',
  noAnswer: 'Terminó sin indicar qué ha pasado.',
  failed: 'No ha funcionado.',
  profiles: {
    enterprise: {
      placeholder: 'Necesitamos una app que…',
      ideas: [
        'un formulario para solicitar días libres, aprobado por el responsable del equipo',
        'un calendario de reservas para nuestras salas de reuniones',
        'una lista de comprobación para incorporar a los nuevos compañeros',
        'un inventario de nuestros equipos y de quién los tiene',
        'una lista de solicitudes de soporte abiertas y de quién atiende cada una',
        'un formulario de opinión para nuestras formaciones',
      ],
    },
  },
};
