import type { Messages } from '../en';

export const ship: Messages['ship'] = {
  title: 'De tu máquina a la pantalla de todos.',
  intro:
    'Creaste algo con tu agente de programación y solo funciona en tu máquina. Pídele que lo publique aquí. En unos minutos tendrá un enlace público que cualquiera puede abrir. Y como tiene memoria, todos pueden jugar, chatear y compartir cosas juntos.',
  facts: ['Gratis', 'Sin cuenta', 'Nada que instalar'],
  connect: 'Conecta tu agente',
  seeOthers: 'Mira lo que publicaron otros',

  stepsTitle: 'Tres pasos, y uno es una frase',
  step: (n) => `Paso ${n}`,
  steps: [
    {
      title: 'Conecta una vez',
      body: 'Añade una dirección a Claude, Cursor o el agente con el que trabajes. Tardas menos de un minuto y solo lo haces una vez.',
    },
    {
      title: 'Pídele que la publique',
      body: 'Dile que ponga la app en línea aquí. La empaqueta, la publica y comprueba que responde.',
    },
    {
      title: 'Comparte el enlace',
      body: 'Recibes una dirección pública y un enlace de edición privado. Manda la primera a quien quieras. Guarda el segundo: con él cambias la app más adelante.',
    },
  ],

  togetherTitle: 'No es solo una página. Es un punto de encuentro.',
  together:
    'La mayoría de los servicios de hosting le dan a cada visitante su propia copia de la app, y cada uno juega solo. Aquí cada app tiene su propia memoria y una conexión en vivo con todos los que la tienen abierta. Lo que hace una persona lo ven los demás al instante, y lo que publican sigue ahí mañana.',
  together2:
    'Sin base de datos en la que registrarte, sin otro servicio que conectar. Pídelo como se lo explicarías a un amigo.',
  kinds: [
    { name: 'Juegos multijugador', ask: 'Haz que hasta ocho amigos puedan unirse a la misma partida y ver en vivo las jugadas de los demás.' },
    { name: 'Salas de chat', ask: 'Añade una sala donde todos los que tengan el enlace puedan hablar, y guarda los últimos cien mensajes.' },
    { name: 'Listas compartidas', ask: 'Convierte la lista del viaje en una que todo el equipo pueda editar a la vez.' },
    { name: 'Puntuaciones y récords', ask: 'Guarda un ranking con el mejor tiempo de cada uno y muestra el top 10 en la pantalla de inicio.' },
    { name: 'Pequeñas redes sociales', ask: 'Deja que los invitados de la boda suban fotos a un muro y den like a las de los demás.' },
  ],
  quote: (text) => `«${text}»`,

  connectTitle: 'Conecta tu agente una sola vez',
  connectText:
    'Dale esta dirección a tu agente. A partir de ahí ya sabe publicar aquí, sin clave y sin iniciar sesión.',
  sayLike: 'Después, en tu proyecto, pídele algo como',
  asks: [
    'Publica esta app en GenHTTP Lambda y pásame el enlace.',
    'Haz que los récords se compartan, así todos ven el mismo ranking.',
  ],

  domainChip: 'Cuando despegue',
  domainTitle: 'Dale un nombre propio',
  domainText:
    'La misma app y el mismo enlace de edición, pero en una dirección que es tuya. Más fácil de decir y de recordar, y se ve más profesional cuando la gente empieza a compartirla.',
  domainSubject: 'Un dominio para mi app',
  domainAsk: 'Pregúntanos por tu dominio',

  questionsTitle: 'Antes de que preguntes',
  questions: (offline, removed, showcase, terms) => [
    [
      '¿De verdad es gratis?',
      <>
        Sí. Sin tarjeta, sin periodo de prueba y sin cuenta. Tu app sigue en línea mientras la gente la use. Después de{' '}
        {offline} días sin una sola visita ni cambio, se desconecta. A los {removed} días, se elimina.
      </>,
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
  closeFacts: 'Gratis. Sin cuenta. Nada que instalar.',

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
    'Datos compartidos en vivo: chat, multijugador, récords',
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

  yourAgent: 'Tu agente',
  terminal: 'Terminal',
  setups: {
    claudeCode: 'Ejecuta esto una vez en una terminal. Cualquier proyecto que abras después podrá publicar aquí.',
    claude: (strong) => (
      <>
        En Claude (web o escritorio), abre {strong('Configuración')}, luego {strong('Connectors')}, y elige{' '}
        {strong('Add custom connector')}. Pega la dirección de arriba y guarda. Y listo.
      </>
    ),
    cursor: 'Añade esto a la configuración MCP de Cursor, o al archivo de abajo, y recarga.',
    vscode: 'Guarda esto en tu proyecto y luego inicia el servidor desde la vista MCP de Copilot Chat.',
  },
  elsewhere:
    '¿Usas otra cosa? Windsurf, Codex, Zed y la mayoría de los agentes pueden añadir un servidor MCP remoto desde su configuración. Dales la dirección de arriba.',
};
