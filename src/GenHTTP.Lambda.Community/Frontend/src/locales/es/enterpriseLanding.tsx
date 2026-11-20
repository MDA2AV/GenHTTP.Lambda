import type { Messages } from '../en';

export const enterpriseLanding: Messages['enterpriseLanding'] = {
  eyebrow: 'Apps para su trabajo',
  headline: 'Describa la app que necesita su trabajo.',
  headlineAccent: 'Un agente de IA la crea y la pone en línea aquí.',
  intro:
    'Un formulario, un calendario de reservas, una lista para su equipo: diga con sus propias palabras qué debe hacer. En pocos minutos tendrá un enlace para compartir con sus compañeros, y más adelante cambiará la app de la misma manera, sin escribir código.',
  describe: 'Describir una app',
  own: 'Usar su propio agente de IA',

  examplesTitle: 'Qué crea la gente aquí',
  examples: [
    { title: 'Formularios y solicitudes', text: 'Vacaciones, compras, comentarios: las personas lo rellenan y quien decide lo aprueba.' },
    { title: 'Reservas', text: 'Salas de reuniones, puestos, equipos: quién tiene qué y cuándo.' },
    { title: 'Listas y seguimientos', text: 'Solicitudes de soporte, inventario, incorporaciones: qué está abierto y quién se ocupa.' },
    { title: 'Páginas para un equipo', text: 'Un horario, un manual, noticias: un único enlace que todos abren.' },
  ],

  stepsTitle: 'Cómo funciona',
  steps: [
    {
      title: 'Descríbala',
      text: 'Diga qué debe hacer la app, como se lo explicaría a un compañero. Las ideas bajo el cuadro son un punto de partida.',
    },
    {
      title: 'Comparta el enlace',
      text: 'Unos minutos después está en línea aquí. Envíe su enlace a quienes la usan y conserve su enlace de edición: es la forma de cambiar la app.',
    },
    {
      title: 'Pida más',
      text: 'En el mismo cuadro: algo nuevo, algo distinto o cómo le va: quién la usó y qué se introdujo. Las versiones anteriores se conservan, así que la anterior puede volver a ponerse en línea.',
    },
  ],

  questionsTitle: 'Conviene saber',
  questions: (showcase) => [
    ['¿Necesito saber programar?', 'No. Usted describe qué debe hacer la app y el agente la escribe. Nunca tiene que mirar el código.'],
    ['¿Quién puede abrir mi app?', 'Quien tenga su enlace y pueda acceder a este sitio. Envíelo a las personas que deban usarla.'],
    ['¿Dónde se guarda lo que introduce la gente?', 'Cada app guarda sus propios registros en esta instalación, que gestiona su organización.'],
    [
      '¿Cómo vuelvo a mi app?',
      'Este navegador recuerda las apps que creó aquí, junto al cuadro donde las creó. En otro dispositivo, abra el enlace de edición que recibió con la app; guárdelo en un lugar seguro, porque nadie puede recuperarlo.',
    ],
    [
      'Algo no funciona. ¿Qué hago?',
      'Cuente al agente qué falla, igual que le pidió la app. Si un cambio empeoró las cosas, ponga de nuevo en línea la versión anterior en el editor de la app.',
    ],
    ['¿Qué han creado los compañeros?', <>Las apps que decidieron mostrar están en la {showcase('galería')}.</>],
  ],
};
