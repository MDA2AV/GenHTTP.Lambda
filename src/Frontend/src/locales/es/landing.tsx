import type { Messages } from '../en';

export const landing: Messages['landing'] = {
  eyebrow: 'Una plataforma de programación agéntica',
  headline: 'Describa una aplicación.',
  headlineAccent: 'Su agente la publica.',
  intro:
    'Encuestas, libros de visitas, clasificaciones, pequeñas tiendas: describa lo que necesita a nuestro agente o al que ya utiliza, y recibirá una aplicación funcional con un enlace para compartir. La aplicación sigue siendo editable, por lo que puede perfeccionarla mucho después de la primera versión.',
  build: 'Crear una aplicación',
  ownAgent: 'Usar su propio agente',
  free: 'Gratis. Sin cuenta y sin instalaciones.',
  seeIt: 'Ver una demostración',

  videoTitle: 'De una frase a una aplicación en línea',
  videoText:
    'Una ventana de navegación privada, sin cuenta y con una única solicitud en la página Crear; a continuación, la aplicación terminada, abierta desde su enlace tal como la verá cualquier visitante.',
  videoNote: 'La creación se muestra acelerada. Todo lo demás, en tiempo real.',
  tryIt: 'Pruébelo usted mismo',

  oneShotTitle: 'Mucho más que un resultado único',
  oneShotText:
    'La mayoría de los generadores entregan un resultado y ahí termina todo. Aquí la aplicación sigue funcionando donde se creó, de modo que usted y su agente pueden seguir desarrollándola.',
  steps: [
    {
      title: 'Describa lo que necesita',
      body: 'Explíquelo con sus propias palabras, al agente de este sitio o al que ya utiliza. Sin código, sin configuración y sin cuenta.',
      alt: 'La página Crear con una solicitud para una encuesta sobre el almuerzo',
    },
    {
      title: 'Una aplicación funcional y un enlace',
      body: 'La aplicación se crea, se despliega y se le entrega como una dirección pública que puede compartir. Conserva sus datos – votos, puntuaciones, mensajes – para que todos vean el mismo estado.',
      alt: 'La encuesta terminada, abierta en un navegador',
    },
    {
      title: 'Mejórela de forma continua',
      body: 'Cada aplicación incluye un enlace de edición privado. Entrégueselo a su agente junto con el siguiente cambio o ábralo usted mismo. Cada cambio se convierte en una nueva versión y la dirección se mantiene.',
      alt: 'El centro de control de la encuesta: sus versiones, cada una con la solicitud, el cambio y la diferencia con la anterior',
    },
  ],
  weekLater: 'Una semana después',
  weekAsk:
    'Este es el enlace de edición de mi encuesta. Por favor, cierra la votación a las 11 los viernes y muestra el resultado en la parte superior.',
  weekAnswer:
    'Hecho. La versión 4 está en línea en la misma dirección y la versión 3 sigue disponible si desea volver atrás.',

  agentsTitle: 'Utilice el agente de su preferencia',
  agentsText:
    '¿Ya trabaja con Claude u otro asistente? Conéctelo a esta dirección y podrá crear, desplegar y actualizar aplicaciones aquí, directamente desde la conversación que ya tiene abierta.',
  agents: [
    {
      name: 'Claude en la web o en el escritorio',
      how: 'Abra la Configuración, luego «Connectors», y elija «Add custom connector». Pegue la dirección anterior; no se requiere clave de API ni inicio de sesión.',
    },
    {
      name: 'Claude Code',
      how: 'Ejecute una vez en un terminal:',
    },
    {
      name: 'Otros clientes MCP',
      how: 'Cursor, VS Code, Codex y otros clientes MCP admiten servidores remotos. Configúrelos con la misma dirección.',
    },
  ],
  thenAsk: (em) => (
    <>A continuación, basta con pedir: {em('crea una lista de inscripción para nuestro evento de equipo y publícala')}.</>
  ),

  contactTitle: 'Contacto',
  contactText:
    '¿Necesita ayuda, planea un proyecto de mayor envergadura o busca una solución a medida? Será un placer atenderle.',
  mailTitle: 'Por correo electrónico',
  mailText: 'Para proyectos, consultas y cualquier asunto que prefiera tratar de forma privada.',
  discordTitle: 'Únase al Discord',
  discordText: 'Comparta lo que ha creado, obtenga ayuda y converse directamente con el equipo.',
  discordLink: 'El Discord de GenHTTP',

  terms: 'Condiciones del servicio',
  writeCode: 'Escribir el código usted mismo',
  contact: 'Contacto',
};
