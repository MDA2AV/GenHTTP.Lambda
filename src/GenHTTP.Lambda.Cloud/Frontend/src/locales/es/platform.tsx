import type { Messages } from '../en';

export const platform: Messages['platform'] = {

  eyebrow: 'Creador de apps con IA, hosting incluido',
  title: 'Crearla lleva un minuto.',
  titleAccent: 'Todo lo demás ya viene incluido.',
  intro:
    'Describe una app a nuestro agente de IA, o a Claude Code, Codex o Cursor, y en minutos está en línea, alojada por nosotros y con un enlace para compartir. Ya incluye todo lo que la mantiene útil: sus registros, sus versiones y para qué sirve, quién la visita y un dominio propio cuando despega.',
  build: 'Crea algo',
  ownAgent: 'Usa tu propio agente',
  free: 'Gratis. Sin registro, sin tarjeta.',
  heroAlt: 'El editor de una app de reserva de pistas: en línea en el dominio propio del club, cuánta gente la usó hoy y su último cambio',
  example: 'Todas las imágenes de esta página muestran la misma app de ejemplo: la reserva de pistas de un club de tenis.',

  contents: 'En esta página',

  makeTitle: 'Créala, cámbiala',
  makeText: 'Di lo que necesitas y sigue diciéndolo. Nadie tiene que escribir ni leer código para ninguna de las dos cosas.',

  behindTitle: 'Base de datos, historial de versiones y analítica para cada app',
  behindText:
    'Lo que mantiene útil una app es todo lo que rodea a su código, y está ahí para quien la creó, no solo para quien sabe leer código.',

  yoursTitle: 'Tuyo para llevarlo más lejos',
  yoursText: 'Para desarrolladores y para quien quiera que su app sobreviva a este sitio.',

  features: {
    build: {
      name: 'Crear',
      title: 'La describes y ya está en línea',
      text: 'Cuéntale a nuestro agente lo que necesitas, con tus propias palabras, o pídeselo a Claude Code, Codex o Cursor. Unos minutos después la app funciona en una dirección propia, lista para compartir. No hay que configurar servidor, base de datos ni cuenta.',
      points: ['Nuestro agente en la página de creación, o el tuyo por MCP', 'Un enlace para compartir en minutos', 'Gratis, sin registrarte'],
      alt: 'La página de reserva de pistas: cuatro pistas, hora por hora, con las horas reservadas a nombre de cada persona',
    },
    change: {
      name: 'Cambiar',
      title: 'Pregunta por ella, cámbiala',
      text: 'Tus apps quedan en la lista de la página de creación, cada una con su conversación. Pregunta cómo va una (quién la visitó, qué escribió la gente, si algo falla) o di qué debe ser distinto, y el agente responde o hace el cambio. O pásale su enlace de edición a tu propio agente.',
      points: ['Respuestas sobre visitantes, registros y errores', 'En línea por sí sola cuando funciona, o guardada como borrador para que la pruebes antes', 'El código queda fuera de la vista salvo que lo quieras'],
      alt: 'La página de creación: el club preguntó cuántas pistas se reservaron esta semana, el agente respondió y las clases del entrenador están listas para probar en un borrador',
    },
    drafts: {
      name: 'Borradores',
      title: 'Prueba un cambio antes de que lo vea nadie',
      text: 'Un cambio más grande se hace en un borrador: la app tal como quedará, en una dirección propia y con una copia de los registros para probarla. Nada de lo que hagas ahí toca los reales. Cuando está bien, un clic la pone en línea.',
      points: ['Una dirección propia, para probarlo y enseñarlo', 'Datos de prueba: una copia de los registros, no los registros', 'Ponlo en línea cuando esté bien, o descártalo'],
      alt: 'Un borrador de la página de reservas, donde las clases del entrenador se ven en morado, sobre una copia de las reservas',
    },
    history: {
      name: 'Historial',
      title: 'Cada cambio, una versión, con vuelta atrás',
      text: 'Cada cambio se convierte en una versión nueva, descrita por lo que cambió. Si una sale mal, la anterior está a un clic: en la misma dirección y con los mismos registros.',
      points: ['Lo que se pidió y lo que cambió', 'Vuelta a una versión anterior con un clic', 'Los registros son comunes a todas las versiones, así que volver atrás no pierde ninguno'],
      alt: 'El historial de la app: sus cambios, uno debajo de otro, con el más reciente en línea',
    },
    about: {
      name: 'Acerca de',
      title: 'Sabe lo que es',
      text: 'Con cada cambio el agente anota para qué sirve la app, quién la usa y por qué funciona como funciona. Tú lo lees con palabras sencillas, y el siguiente agente lo lee antes de cambiar nada, de modo que cada cambio se apoya en lo que hay en lugar de adivinar.',
      points: ['Qué hace y para quién, con palabras sencillas', 'Las decisiones que hay detrás, para quien la cambie después', 'Cómo se prueba, guardado con cada versión'],
      alt: 'Acerca de tu app: la reserva de pistas descrita con palabras sencillas, quién la usa y qué hace',
    },
    records: {
      name: 'Registros',
      title: 'Registros en una base de datos propia',
      text: 'Una app que guarda cosas (reservas, inscripciones, puntuaciones) las guarda en una base de datos propia, y se conservan cuando la app cambia. Léelas tabla por tabla en el editor, junto a los archivos que guardó, como la lista semanal del tablón de anuncios.',
      points: ['Una base de datos para cada app que la necesite', 'Todas las tablas, legibles en el editor', 'Los archivos que guarda la app, listados a su lado'],
      alt: 'Los registros de la app: sus reservas y sus socios, leídos tabla por tabla',
    },
    keys: {
      name: 'Claves y contraseñas',
      title: 'Las claves y contraseñas siguen siendo secretas',
      text: 'Cuando una app necesita una contraseña o la clave de otro servicio, el agente solo la pide por su nombre. Tú escribes el valor en el editor y desde entonces nadie lo ve: ni tú ni el agente. Solo lo lee la app.',
      points: ['El agente nunca ve el valor', 'Se guarda cifrado y no se vuelve a mostrar', 'Se cambia cuando quieras'],
      alt: 'Claves y contraseñas: la contraseña de recepción está definida y su valor está oculto',
    },
    analytics: {
      name: 'Analítica',
      title: 'Mira quién te visita, sin cookies',
      text: 'Activa la analítica y comprueba cuánta gente abrió la app, de dónde venía y con qué dispositivos, todo contado en nuestro servidor, sin cookies y sin ningún script en tus páginas.',
      points: ['Visitantes, páginas vistas y de dónde vinieron', 'Sin cookies y sin nada que identifique a un visitante', 'Desactivada hasta que la actives'],
      alt: 'Analítica: los visitantes de los últimos treinta días, día a día',
    },
    domain: {
      name: 'Dominio',
      title: 'Un dominio propio',
      text: 'Cada app empieza en una dirección nuestra. Cuando tiene éxito, puede pasar a un dominio tuyo: la misma app y el mismo editor, en una dirección que la gente recuerda. Los enlaces a la dirección antigua siguen funcionando y llevan a la nueva.',
      points: ['Para apps Premium: escríbenos', 'La misma app, el mismo editor y los mismos registros', 'La dirección antigua redirige a los visitantes'],
      alt: 'La dirección de la app pasando de su dirección en genhttp.run al dominio propio del club',
    },
    code: {
      name: 'Código y git',
      title: 'El código, cuando lo quieras',
      text: 'Cambia el editor a su vista completa y ahí está el código de cada versión, con sus registros de actividad y sus cifras. Clona la app con git y haz push de un cambio, o descárgala como un proyecto .NET que funciona en cualquier sitio. Es tu código.',
      points: ['Cada archivo, comparado de una versión a otra', 'Git: un commit en main es una versión, una rama es un borrador', 'Una descarga que funciona por sí sola, registros incluidos'],
      alt: 'La vista completa del editor: los archivos de la app y su código',
    },
    publish: {
      name: 'Galería y código fuente',
      title: 'Enséñala o publica su código',
      text: 'Pon la app en la galería para que otros la encuentren, o publica su código con una licencia de código abierto para que cualquiera lo lea, le dé una estrella y lo descargue. Las dos cosas son decisión tuya, y ninguna comparte sus registros.',
      points: ['En la galería, con imagen y descripción', 'Su código con licencia MIT u otra', 'Nunca sus registros, claves ni visitantes'],
      alt: 'El código publicado de la reserva de pistas: sus archivos, su documentación y sus versiones, con licencia MIT',
    },
  },

  draftBadge: 'Borrador',
  domainOwn: 'Premium',
  domainSubject: 'Un dominio para mi app',
  domainAsk: 'Consultar por un dominio',
  zoom: 'Ampliar',
  close: 'Cerrar',

  closeTitle: 'Pruébalo gratis.',
  closeAccent: 'Sin registro.',
  closeBuild: 'Crea algo',
  closeShip: 'Publica lo que hiciste',
  openSource: (selfHosting, github) => (
    <>
      GenHTTP Lambda es de código abierto. Úsalo aquí o {selfHosting('ejecútalo en tu propio servidor')}: el código está en{' '}
      {github('GitHub')}.
    </>
  ),
};
