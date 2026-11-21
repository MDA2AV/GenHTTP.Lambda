import type { Messages } from '../en';

export const ship: Messages['ship'] = {
  eyebrow: 'Hosting gratis para apps de vibe coding',
  title: 'De localhost a la pantalla de todos.',
  intro:
    'Creaste una app con Claude Code, Codex o Cursor y solo funciona en tu máquina. Pídele a tu agente que la publique aquí. En unos minutos tendrá un enlace público que cualquiera puede abrir, su propia base de datos y una conexión en vivo con todos los que la tienen abierta, para que la gente juegue, chatee y comparta cosas en ella.',
  facts: ['Gratis', 'Sin registro', 'Sin tarjeta', 'Nada que instalar'],

  stepsTitle: 'Sube tu app web a internet en tres pasos',
  step: (n) => `Paso ${n}`,
  steps: [
    {
      title: 'Conecta una vez',
      body: 'Añade una dirección, la de un servidor MCP remoto, a Claude Code, Codex, Cursor o el agente con el que trabajes. Tardas menos de un minuto y solo lo haces una vez.',
    },
    {
      title: 'Pídele que la publique',
      body: 'Dile que ponga la app en línea aquí. La empaqueta, la despliega y comprueba que responde, sin repositorio de GitHub, sin pipeline de despliegue y sin Docker.',
    },
    {
      title: 'Comparte el enlace',
      body: 'Recibes una dirección pública y un enlace de edición privado. Manda la primera a quien quieras. Guarda el segundo: con él cambias la app más adelante.',
    },
  ],

  togetherTitle: 'No es solo hosting. Base de datos y multijugador incluidos.',
  together:
    'La mayoría de los servicios de hosting le dan a cada visitante su propia copia de la app, y cada uno juega solo: lo que un navegador guarda en localStorage, el siguiente nunca lo ve. Aquí cada app tiene su propia base de datos y una conexión en vivo con todos los que la tienen abierta. Lo que hace una persona lo ven los demás al instante, y lo que publican sigue ahí mañana.',
  sayLike: 'Después, en tu proyecto, pídele algo como',
  asks: [
    'Publica esta app en GenHTTP Lambda y pásame el enlace.',
    'Haz que los récords se compartan, así todos ven el mismo ranking.',
  ],

  questionsTitle: 'Antes de publicar',
  questions: (offline, removed, showcase, terms, selfHosting) => [
    [
      '¿De verdad es gratis?',
      <>
        Sí. Sin registro, sin tarjeta y sin periodo de prueba. Tu app sigue en línea mientras la gente la use. Después de{' '}
        {offline} días sin una sola visita ni cambio, se desconecta. A los {removed} días, se elimina.
      </>,
    ],
    [
      '¿Puedo desplegar mi app aquí con Claude Code, Codex o Cursor?',
      'Sí, y con cualquier otro agente que pueda añadir un servidor MCP remoto. Conéctalo una vez con la dirección de arriba y pídele que la publique: despliega la app, comprueba que responde y te manda el enlace.',
    ],
    [
      '¿Por qué mis amigos no pueden abrir mi enlace de localhost?',
      'Porque localhost es tu propia máquina: la dirección solo funciona ahí, y solo mientras la app esté en marcha. Un túnel le presta una dirección pública mientras tu equipo siga encendido. Publicada aquí, la app funciona en nuestros servidores, con un enlace que sigue funcionando aunque apagues el tuyo.',
    ],
    [
      '¿Necesito un servidor, un backend o Supabase?',
      'No. Cada app tiene su propia base de datos, almacenamiento de archivos y una conexión en vivo con todos los que la tienen abierta. No hay servidor que alquilar ni otro servicio que configurar, y tampoco nada que mantener en marcha de tu lado.',
    ],
    [
      '¿Puedo hacer mi juego multijugador sin tener un servidor?',
      'Sí. Lo que un navegador guarda en localStorage, el siguiente nunca lo ve, así que la parte compartida tiene que estar en un servidor: aquí, el nuestro. Pídele a tu agente que haga el juego multijugador y cada jugada llegará a todos los que lo tienen abierto.',
    ],
    [
      '¿Mi app tiene que estar hecha de alguna forma concreta?',
      'No, de eso se encarga tu agente. Las páginas, imágenes y estilos se suben tal cual, y lo que tenga que ejecutarse en el servidor lo adapta el agente a esta plataforma. Tú dices qué debe hacer la app y él lo traduce.',
    ],
    [
      '¿Cómo la cambio más adelante?',
      'Con el enlace de edición que recibiste al publicarla. Pásaselo a tu agente con el siguiente cambio o ábrelo en el navegador. Cada cambio es una versión nueva en la misma dirección, y puedes volver a una anterior cuando quieras.',
    ],
    [
      '¿Puedo hacer push con git?',
      'Sí. Cada app es también un repositorio git: clónalo desde la dirección que aparece en Clonar, en el editor, cámbialo con tus propias herramientas o tu agente y haz push. Cada commit enviado a main es la siguiente versión, y una rama que envíes se convierte en un borrador con su propia vista previa.',
    ],
    [
      '¿Dónde van mis claves de API?',
      'En el código no. Tu agente pide una clave por su nombre y tú escribes el valor en el editor. Nadie puede volver a leerlo: ni el editor ni el agente.',
    ],
    [
      '¿Puedo llevarme mi código?',
      'Sí, es tuyo. Descárgalo desde el editor cuando quieras, como un proyecto que funciona por sí solo, base de datos incluida, o clónalo con git, con todas sus versiones.',
    ],
    [
      '¿Puedo alojarlo en mi propio servidor?',
      <>
        Sí. GenHTTP Lambda es de código abierto, y su edición Community es la misma plataforma en un contenedor Docker, para
        tu propio servidor y tu propio agente. {selfHosting('En la nube o autoalojado')} explica qué cambia.
      </>,
    ],
    [
      '¿Quién puede ver mi app?',
      <>Quien tenga el enlace. No aparece en ningún listado, a menos que decidas añadirla a la {showcase('galería')}.</>,
    ],
    [
      '¿Hay algo que no pueda publicar?',
      <>
        Algunas cosas, como todo lo que haga daño o engañe a la gente. Las {terms('condiciones')} son cortas y fáciles
        de leer.
      </>,
    ],
  ],
  noAgent: '¿No tienes agente? Créala aquí',

};
