import type { Messages } from '../en';

export const ship: Messages['ship'] = {
  title: 'De su equipo a todas las pantallas.',
  intro:
    'Ha creado algo con su agente de programación y solo funciona en su equipo. Pida al agente que lo publique aquí. Pocos minutos después tendrá un enlace público que cualquiera puede abrir, y la aplicación podrá guardar datos para que varias personas jueguen, conversen y publiquen en ella de forma conjunta.',
  facts: ['Gratis', 'Sin cuenta', 'Sin instalaciones'],
  connect: 'Conectar su agente',
  seeOthers: 'Ver aplicaciones publicadas',

  stepsTitle: 'Tres pasos, y uno de ellos es una frase',
  step: (n) => `Paso ${n}`,
  steps: [
    {
      title: 'Conexión única',
      body: 'Añada una dirección a Claude, Cursor o al agente con el que trabaja. Lleva menos de un minuto y solo es necesario una vez.',
    },
    {
      title: 'Solicitud de publicación',
      body: 'Pida al agente que publique la aplicación aquí. La empaqueta, la publica y comprueba que responde.',
    },
    {
      title: 'Compartir el enlace',
      body: 'Recibirá una dirección pública y un enlace de edición privado. La primera puede compartirla con cualquiera; conserve el segundo, ya que con él modificará la aplicación más adelante.',
    },
  ],

  togetherTitle: 'Más que una página: un espacio compartido.',
  together:
    'La mayoría de los servicios de alojamiento entregan a cada visitante su propia copia de la aplicación, y cada uno la utiliza por separado. Aquí, cada aplicación tiene su propia memoria y una conexión en tiempo real con todas las personas que la tienen abierta. Lo que hace una persona aparece de inmediato para las demás, y lo que se publica se conserva.',
  together2:
    'Sin bases de datos que contratar ni servicios adicionales que integrar. Basta con describir la funcionalidad deseada.',
  kinds: [
    { name: 'Juegos multijugador', ask: 'Permite que hasta ocho personas se unan a la misma partida y vean los movimientos de los demás en tiempo real.' },
    { name: 'Salas de chat', ask: 'Añade una sala en la que todos los que tengan el enlace puedan conversar y guarda los últimos cien mensajes.' },
    { name: 'Listas compartidas', ask: 'Convierte la lista de equipaje en una lista que todo el equipo pueda editar a la vez.' },
    { name: 'Puntuaciones y récords', ask: 'Lleva una clasificación con el mejor tiempo de cada persona y muestra los diez primeros en la pantalla de inicio.' },
    { name: 'Pequeñas comunidades', ask: 'Permite que los invitados a la boda publiquen fotos en un muro común y valoren las de los demás.' },
  ],
  quote: (text) => `«${text}»`,

  connectTitle: 'Conecte su agente una sola vez',
  connectText:
    'Indique esta dirección a su agente. A partir de ese momento sabrá publicar aquí, sin claves ni inicio de sesión.',
  sayLike: 'Después, en su proyecto, basta con pedir algo como',
  asks: [
    'Publica esta aplicación en GenHTTP Lambda y envíame el enlace.',
    'Haz que las puntuaciones sean compartidas, para que todos vean la misma clasificación.',
  ],

  domainChip: 'Cuando su aplicación crece',
  domainTitle: 'Un dominio propio',
  domainText:
    'La misma aplicación y el mismo enlace de edición, pero en una dirección de su propiedad: más fácil de comunicar, de recordar y con una imagen más profesional cuando se comparte.',
  domainSubject: 'Un dominio para mi aplicación',
  domainAsk: 'Solicitar un dominio propio',

  questionsTitle: 'Preguntas frecuentes',
  questions: (offline, removed, showcase, terms) => [
    [
      '¿Es realmente gratuito?',
      <>
        Sí. Sin tarjeta, sin periodo de prueba y sin cuenta. Su aplicación permanece en línea mientras se utiliza. Tras{' '}
        {offline} días sin visitas ni cambios se desconecta, y tras {removed} días se elimina.
      </>,
    ],
    [
      '¿Mi aplicación debe estar construida de una forma determinada?',
      'No, de eso se encarga su agente. Las páginas, imágenes y estilos se publican tal cual, y el agente adapta a esta plataforma todo lo que debe ejecutarse en el servidor. Usted describe lo que debe hacer la aplicación y el agente se ocupa de la implementación.',
    ],
    [
      '¿Cómo la modifico más adelante?',
      'Con el enlace de edición que recibió al publicarla. Entrégueselo a su agente junto con el siguiente cambio o ábralo en su navegador. Cada cambio se convierte en una nueva versión en la misma dirección, y puede volver a una versión anterior en cualquier momento.',
    ],
    [
      '¿Quién puede ver mi aplicación?',
      <>
        Cualquier persona a la que usted facilite el enlace. No figura en ningún listado, salvo que decida añadirla a la{' '}
        {showcase('galería')}.
      </>,
    ],
    [
      '¿Hay contenidos que no puedo publicar?',
      <>
        Sí, por ejemplo cualquier cosa que perjudique o engañe a las personas. Las {terms('condiciones del servicio')} son
        breves y están redactadas con claridad.
      </>,
    ],
  ],

  closeTitle: 'Funciona en su equipo.',
  closeAccent: 'Haga que funcione para todos.',
  noAgent: '¿Sin agente? Créela aquí',
  closeFacts: 'Gratis. Sin cuenta. Sin instalaciones.',

  scene: {
    label:
      'Se pide a un agente que publique una aplicación. La dirección pasa de localhost a un enlace público y se unen otras personas.',
    ask: 'Publica mi juego de preguntas para que mis amigos puedan participar.',
    live: 'Ya está en línea. Este es su enlace.',
    publishing: 'Publicando…',
    public: 'Pública',
    onlyYou: 'Local',
    app: 'Noche de preguntas del viernes',
    playing: 'en línea',
    you: 'Usted',
  },

  compareTitle: 'El camino más corto de «funciona» a «pruébelo»',
  compareText:
    'Vercel, Cloudflare y Lovable son excelentes plataformas para ejecutar aplicaciones. Sin embargo, requieren registrarse y, en cuanto su aplicación necesita compartir datos entre visitantes, configurar un servicio adicional. Así se compara cuando se parte de cero.',
  rows: [
    'Empezar sin cuenta',
    'Publicar desde el agente que ya utiliza',
    'Datos compartidos en tiempo real: chat, multijugador, récords',
    'Coste hasta el primer enlace',
  ],
  us: ['Sí', 'Una conexión y después basta con pedirlo', 'Incluido en cada aplicación', 'Gratis'],
  rivals: [
    ['Requiere registro', 'Con sus propias herramientas, tras iniciar sesión', 'Hay que añadir un servicio de base de datos', 'Plan gratuito'],
    ['Requiere registro', 'Con sus propias herramientas, tras iniciar sesión', 'Posible, con configuración', 'Plan gratuito'],
    ['Requiere registro', 'En su propio editor', 'A través de un backend conectado', 'Plan gratuito, créditos limitados'],
  ],
  compareNote:
    'Datos a septiembre de 2026, para alguien sin cuenta en ningún servicio. Los planes y funciones de otros servicios cambian; consulte los detalles con cada proveedor.',

  yourAgent: 'Su agente',
  terminal: 'Terminal',
  setups: {
    claudeCode: 'Ejecute esto una vez en un terminal. Cualquier proyecto que abra después podrá publicar aquí.',
    claude: (strong) => (
      <>
        En Claude en la web o en el escritorio, abra la {strong('Configuración')}, luego {strong('Connectors')}, y elija{' '}
        {strong('Add custom connector')}. Pegue la dirección anterior y guarde. No se necesita nada más.
      </>
    ),
    cursor: 'Añada esto a la configuración MCP de Cursor, o al archivo indicado a continuación, y recargue.',
    vscode: 'Guarde este archivo en su proyecto y, a continuación, inicie el servidor desde la vista MCP de Copilot Chat.',
  },
  elsewhere:
    '¿Utiliza otra herramienta? Windsurf, Codex, Zed y la mayoría de los agentes permiten añadir un servidor MCP remoto en su configuración. Indíqueles la dirección anterior.',
};
