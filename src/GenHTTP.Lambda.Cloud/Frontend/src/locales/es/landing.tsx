import type { Messages } from '../en';

export const landing: Messages['landing'] = {
  eyebrow: 'Creador de apps con IA, hosting incluido',
  headline: 'Describe una app.',
  headlineAccent: 'Tu agente la publica.',
  intro:
    'Encuestas, libros de visitas, rankings, juegos multijugador. Cuéntale lo que necesitas a nuestro agente de IA o al que ya usas. Recibes una app que funciona, ya alojada, con un enlace para compartirla. Y puedes seguir puliéndola mucho después de la primera versión.',
  build: 'Crea tu app',
  ownAgent: 'Usa tu propio agente',
  free: 'Gratis. Sin registro, sin tarjeta y sin instalar nada.',
  seeIt: 'Míralo en acción',

  videoTitle: 'De una frase a una app en línea',
  videoText:
    'Una ventana privada, sin cuenta y una sola petición en la página Crear. Después, la app terminada, abierta desde su enlace, tal como la vería cualquier visitante.',
  videoNote: 'La creación se muestra acelerada. Todo lo demás, en tiempo real.',
  tryIt: 'Pruébalo tú',

  oneShotTitle: 'No es un generador de apps desechables',
  oneShotText:
    'La mayoría de los generadores te dan un resultado y ahí te dejan. Aquí la app sigue funcionando donde se creó, así que tú y tu agente pueden seguir trabajando en ella.',
  steps: [
    {
      title: 'Di lo que quieres',
      body: 'Descríbelo con tus palabras, al agente de este sitio o al que ya usas. Sin código, sin configurar nada y sin cuenta.',
      alt: 'La página Crear con una petición escrita: una encuesta para elegir dónde almorzar',
    },
    {
      title: 'Recibe tu app y un enlace',
      body: 'La app se crea, se aloja y te llega como una dirección pública para compartir. Sin servidor, plan de hosting, dominio ni base de datos que configurar: de eso nos encargamos nosotros. Guarda sus datos (votos, puntuaciones, mensajes), así que todos los que la abren ven lo mismo.',
      alt: 'La encuesta del almuerzo terminada, abierta en un navegador',
    },
    {
      title: 'Sigue mejorándola',
      body: 'Cada app viene con un enlace de edición privado. Pásaselo a tu agente con el siguiente cambio, o ábrelo tú. Cada cambio es una versión nueva y la dirección no cambia.',
      alt: 'El centro de control de la encuesta: sus versiones, cada una con lo que se pidió, lo que cambió y la diferencia con la anterior',
    },
  ],
  weekLater: 'Una semana después',
  weekAsk:
    'Aquí tienes el enlace de edición de mi encuesta del almuerzo. Cierra la votación los viernes a las 11 y pon el ganador arriba, por favor.',
  weekAnswer:
    'Listo. La versión 4 ya está en línea en la misma dirección, y la 3 sigue disponible si quieres volver atrás.',

  agentsTitle: 'Usa tu propio agente: Claude, Codex, Cursor',
  agentsText:
    '¿Ya haces vibe coding con Claude Code, Codex, Cursor u otro asistente? Conéctalo a esta dirección (un servidor MCP remoto, sin clave) y podrá crear, desplegar y actualizar apps aquí, directamente desde la conversación que ya tienes abierta.',
  thenAsk: (em) => (
    <>Después, solo pídele: {em('crea una lista de inscripción para el evento del equipo y publícala')}.</>
  ),
  hostIt: (link) => (
    <>¿Tu app solo funciona en localhost? {link('Súbela a internet aquí')}.</>
  ),

  contactTitle: 'Hablemos',
  contactText:
    '¿Necesitas ayuda, tienes en mente algo más grande o buscas una solución hecha a tu medida? Nos encantará saber de ti.',
  mailTitle: 'Escríbenos',
  mailText: 'Para proyectos, consultas y todo lo que prefieras hablar en privado.',
  discordTitle: 'Únete al Discord',
  discordText: 'Comparte tus proyectos, pide ayuda con el siguiente paso y habla directamente con el equipo.',
  discordLink: 'El Discord de GenHTTP',
};
