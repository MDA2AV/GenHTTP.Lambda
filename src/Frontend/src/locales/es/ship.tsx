import type { Messages } from '../en';

export const ship: Messages['ship'] = {
  eyebrow: 'Hosting gratis para apps de vibe coding',
  title: 'De localhost a la pantalla de todos.',
  intro:
    'Creaste una app con Claude Code, Codex o Cursor y solo funciona en tu máquina. Pídele a tu agente que la publique aquí. En unos minutos tendrá un enlace público que cualquiera puede abrir, su propia base de datos y una conexión en vivo con todos los que la tienen abierta, para que la gente juegue, chatee y comparta cosas en ella.',
  facts: ['Gratis', 'Sin registro', 'Sin tarjeta', 'Nada que instalar'],
  connect: 'Conecta tu agente',
  seeOthers: 'Mira lo que publicaron otros',

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
  together2:
    'Sin Supabase ni Firebase en los que registrarte, sin backend que conectar, sin servidor que alquilar. Pídelo como se lo explicarías a un amigo.',
  kinds: [
    { name: 'Juegos multijugador', ask: 'Haz que hasta ocho amigos puedan unirse a la misma partida y ver en vivo las jugadas de los demás.' },
    { name: 'Salas de chat', ask: 'Añade una sala donde todos los que tengan el enlace puedan hablar, y guarda los últimos cien mensajes.' },
    { name: 'Listas compartidas', ask: 'Convierte la lista del viaje en una que todo el equipo pueda editar a la vez.' },
    { name: 'Rankings', ask: 'Guarda un ranking con el mejor tiempo de cada uno y muestra el top 10 en la pantalla de inicio.' },
    { name: 'Pequeñas redes sociales', ask: 'Deja que los invitados de la boda suban fotos a un muro y den like a las de los demás.' },
  ],
  quote: (text) => `«${text}»`,

  connectTitle: 'Conecta Claude Code, Codex o Cursor una sola vez',
  connectText:
    'Dale a tu agente esta dirección, la de nuestro servidor MCP. A partir de ahí ya sabe publicar aquí, sin clave y sin iniciar sesión.',
  sayLike: 'Después, en tu proyecto, pídele algo como',
  asks: [
    'Publica esta app en GenHTTP Lambda y pásame el enlace.',
    'Haz que los récords se compartan, así todos ven el mismo ranking.',
  ],

  domainChip: 'Cuando despegue',
  domainTitle: 'Dale un dominio propio',
  domainText:
    'La misma app y el mismo enlace de edición, pero en una dirección que es tuya. Más fácil de decir y de recordar, y se ve más profesional cuando la gente empieza a compartirla.',
  domainSubject: 'Un dominio para mi app',
  domainAsk: 'Pregúntanos por tu dominio',

  questionsTitle: 'Antes de publicar',
  questions: (offline, removed, showcase, terms) => [
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
      '¿Dónde van mis claves de API?',
      'En el código no. Tu agente pide una clave por su nombre y tú escribes el valor en el editor. Nadie puede volver a leerlo: ni el editor ni el agente.',
    ],
    [
      '¿Puedo llevarme mi código?',
      'Sí, es tuyo. Descárgalo desde el editor cuando quieras, como un proyecto que funciona por sí solo, base de datos incluida.',
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

  closeTitle: 'En tu máquina funciona.',
  closeAccent: 'Que funcione en la de todos.',
  noAgent: '¿No tienes agente? Créala aquí',
  closeFacts: 'Gratis. Sin registro. Nada que instalar.',

  scene: {
    label: 'Alguien le pide a un agente que publique una app. La dirección pasa de localhost a un enlace público y empieza a llegar gente.',
    ask: 'Publica mi juego de trivia para que mis amigos puedan unirse.',
    live: 'Ya está en línea. Aquí tienes tu enlace.',
    publishing: 'Publicando…',
    public: 'Público',
    onlyYou: 'Solo tú',
    app: 'Trivia del viernes',
    playing: (count) => <>{count} jugando</>,
    you: 'Tú',
  },

  compareTitle: 'El camino más corto de «funciona» a «pruébalo»',
  compareText:
    'Vercel, Cloudflare y Lovable son muy buenas opciones para ejecutar cosas. Pero empiezan con un formulario de registro y, en cuanto tu app necesita compartir algo entre visitantes, añaden otro servicio que configurar. Así se ve si empiezas desde cero.',
  rows: [
    'Empezar sin cuenta',
    'Publicar desde el agente que ya usas',
    'Base de datos y tiempo real: chat, multijugador, récords',
    'Lo que cuesta tu primer enlace',
  ],
  us: ['Sí', 'Conecta una vez y pide', 'Incluido en cada app', 'Gratis'],
  rivals: [
    ['Requiere registro', 'Tras iniciar sesión en sus herramientas', 'Añadir un servicio de base de datos', 'Plan gratuito'],
    ['Requiere registro', 'Tras iniciar sesión en sus herramientas', 'Posible, con configuración', 'Plan gratuito'],
    ['Requiere registro', 'Desde su propio editor', 'Con un backend conectado', 'Plan gratuito, créditos limitados'],
  ],
  compareNote:
    'Datos de septiembre de 2026, para alguien sin cuenta en ningún servicio. Los planes y funciones de otros servicios cambian; consulta los detalles con cada uno.',

};
