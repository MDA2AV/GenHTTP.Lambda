import type { Messages } from '../en';

export const build: Messages['build'] = {
  title: 'Crear una página web con IA.',
  intro:
    'Describa con sus propias palabras la página web o la app que tiene en mente. La IA la crea por usted, nosotros la alojamos y queda en línea en minutos, con un enlace que puede enviar a quien quiera. Gratis, sin programar y sin registrarse.',
  points: [
    {
      title: 'Sin saber programar',
      text: 'Explique con sus propias palabras qué debe hacer su web, como se lo contaría a un amigo. La IA la crea por usted, sin necesidad de conocimientos técnicos.',
    },
    {
      title: 'Hosting gratis incluido',
      text: 'Su web funciona en nuestros servidores. Sin plan de hosting, sin servidor ni dominio que comprar y sin nada que instalar: nos ocupamos de la seguridad y las actualizaciones.',
    },
    {
      title: 'En línea en minutos',
      text: 'Recibe al instante un enlace para compartir. La web recuerda lo que la gente envía (inscripciones, votos, mensajes, puntuaciones), para que todos vean lo mismo.',
    },
  ],

  questionsTitle: 'Antes de empezar',
  questions: (offline, removed) => [
    [
      '¿De verdad la IA puede crear mi página web gratis?',
      `Sí. Descríbala con sus propias palabras y la IA la crea, la pone en línea y le da el enlace. Sin registro, sin tarjeta, sin periodo de prueba. Sigue en línea mientras se use: tras ${offline} días sin visitas ni cambios se desconecta, y tras ${removed} días se elimina.`,
    ],
    [
      '¿Necesito hosting, un servidor o un dominio?',
      'No. Su web funciona en nuestros servidores, con hosting, seguridad y actualizaciones incluidos. Recibe un enlace al instante, así que tampoco tiene que comprar un dominio.',
    ],
    [
      '¿Puedo crear una app sin saber programar?',
      'Sí. Nunca verá código. Diga qué debe hacer, como se lo contaría a un amigo, y la IA hace el resto: una web, una pequeña app o un juego.',
    ],
    [
      '¿La gente puede apuntarse, votar o dejar mensajes?',
      'Sí. Su web recuerda lo que la gente envía, así que todos los que abren el enlace ven las mismas inscripciones, votos y puntuaciones.',
    ],
    [
      '¿Cómo la abren los demás?',
      'Con el enlace, en cualquier navegador, desde el teléfono o desde el PC. No hay nada que instalar ni tiendas de apps de por medio.',
    ],
    [
      '¿Cómo la cambio más adelante?',
      'Abra el enlace de edición que recibe con su web y describa qué debe ser diferente, igual que aquí. Si un cambio no le gusta, puede volver a como estaba antes.',
    ],
  ],

};
