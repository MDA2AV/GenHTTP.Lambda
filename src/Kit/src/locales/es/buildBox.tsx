import type { KitMessages } from '../en';

export const buildBox: KitMessages['buildBox'] = {
  placeholder: 'Quiero una página web que…',
  password: 'contraseña',
  fable:
    'Fable está protegido con contraseña mientras se prueba. No tiene límite de tiempo, así que sigue trabajando hasta que su web esté terminada, no hasta que se acabe el reloj.',
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
};
