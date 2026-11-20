import type { Messages } from '../en';

export const ship: Messages['ship'] = {
  eyebrow: 'Conecta tu agente',
  title: 'Publica desde el agente que ya usas.',
  intro:
    'Claude Code, Codex, Cursor o cualquier agente que hable MCP recibe esta instalación como herramientas: crea una app, prueba un cambio en un borrador, la pone en línea y lee sus registros. Cada app responde en una dirección propia, con su propia base de datos.',
  stepsTitle: 'Tres pasos hasta una app en línea',
  step: (n) => `Paso ${n}`,
  steps: [
    {
      title: 'Conecta una vez',
      body: 'Añade a tu agente la dirección del servidor MCP de esta instalación. No pide ninguna clave: quien tiene la clave de edición de una app puede cambiar esa app, y nada más.',
    },
    {
      title: 'Pídele que cree o publique',
      body: 'Describe una app nueva, o pídele que publique la de tu proyecto. Escribe el código, comprueba que compila y lo pone en línea.',
    },
    {
      title: 'Guarda el enlace de edición',
      body: 'Recibes la dirección de la app y su enlace de edición. La dirección es para todos; el enlace de edición es la forma en que tú, o tu agente, cambiáis la app más adelante.',
    },
  ],
  sayLike: 'Luego di algo como',
  asks: ['Crea un libro de visitas, con una página donde yo apruebe las entradas.', 'Publica esta app aquí y envíame el enlace.'],
  noAgent: '¿No tienes agente? Usa el agente de creación',

  questionsTitle: 'Conviene saber',
  questions: (create) => [
    [
      '¿Qué agentes pueden conectarse?',
      'Cualquier agente que pueda añadir un servidor MCP remoto por HTTP: Claude Code, Claude, Codex, Cursor, VS Code y otros, en la dirección de arriba.',
    ],
    [
      '¿Cómo cambio una app más adelante?',
      'Dale a tu agente el enlace de edición de la app con el siguiente cambio, o abre tú mismo el enlace: el editor muestra sus versiones, su código, sus registros y sus logs. Un cambio se prueba en un borrador, con una copia de los datos, antes de ponerlo en línea.',
    ],
    [
      '¿Puedo trabajar en una app con git?',
      'Sí. Cada app es un repositorio git: clónala desde su editor, haz push de un commit a main para guardar una versión, o haz push de una rama para iniciar un borrador con su propia vista previa.',
    ],
    [
      '¿Dónde van las claves de API y las contraseñas?',
      'No en el código. El agente las lee por su nombre, y tú introduces los valores en el editor, donde nadie los vuelve a leer, tampoco el agente.',
    ],
    [
      '¿Puedo escribir una app sin agente?',
      <>Sí: {create('escribe el código tú mismo')}, a partir de una de las demos. Una app es un handler de GenHTTP en C#, compilado en este servidor.</>,
    ],
  ],
  profiles: {
    enterprise: {
      asks: [
        'Crea un formulario para solicitar días libres, con una página donde el responsable del equipo apruebe cada solicitud.',
        'Publica esta app aquí y envíame el enlace.',
      ],
    },
  },
};
