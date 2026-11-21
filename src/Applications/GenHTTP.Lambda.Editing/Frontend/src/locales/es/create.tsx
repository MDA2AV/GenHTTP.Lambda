import type { Messages } from '../en/site';

export const create: Messages['create'] = {
  title: 'Crear una lambda',
  whatTitle: '¿Qué quieres crear?',
  whatText:
    'Elige lo más parecido y empiezas con una copia de algo que ya funciona, lista para cambiarla a tu gusto. O empieza desde cero.',
  seeIt: 'Verla en marcha',
  startFrom: 'Empezar con esta',
  starters: {
    'demo-crud': {
      title: 'Lleva el control de tus cosas',
      description: 'Una lista donde la gente añade, cambia y marca cosas: tareas, notas, marcadores o un pequeño inventario.',
    },
    'demo-registration': {
      title: 'Deja que la gente se registre',
      description: 'Cuentas con las que la gente se registra e inicia sesión, y páginas que solo ellos pueden ver.',
    },
    'demo-game': {
      title: 'Un juego en grupo',
      description: 'Varias personas juegan a la vez, en vivo, cada una en su navegador.',
    },
    'demo-files': {
      title: 'Comparte archivos y fotos',
      description: 'La gente sube fotos o documentos, y todos los demás pueden verlos.',
    },
    'demo-live': {
      title: 'Muestra lo que pasa al instante',
      description: 'Una página que se actualiza sola en cuanto algo cambia: votos, puntuaciones, un panel.',
    },
    empty: {
      title: 'Otra cosa',
      description: 'Empieza con una lambda vacía y crea lo que tengas en mente.',
    },
  },

  addressTitle: 'Dale una dirección',
  fromDemo: (title) => (
    <>{title}: tu lambda empieza como una copia de la demo, y puedes cambiar todo lo que quieras.</>
  ),
  fromNothing: 'Tu lambda empieza vacía, lista para lo que tengas en mente.',
  pickAgain: 'Elegir otra cosa',
  publicKey: 'Clave pública',
  free: (key) => `«${key}» está disponible.`,
  keyHint: 'Minúsculas, números y guiones. Tres caracteres como mínimo. Déjalo vacío y te damos una al azar.',
  accept: 'Acepto las condiciones del servicio',
  fullTerms: 'Leer las condiciones completas',
  back: 'Atrás',
  creating: 'Creando…',
  submit: 'Crear mi lambda',
  keepLink: 'En la siguiente pantalla verás tu enlace de edición. Es la única forma de volver a entrar, así que guárdalo.',
  failed: 'No se pudo crear la lambda.',
  termsShort:
    'Las lambdas se ejecutan en una infraestructura compartida. Al crear una, aceptas no desplegar malware, páginas de phishing, mineros de criptomonedas ni nada que ataque, escanee o sature otros sistemas, y no publicar contenido que no tengas derecho a publicar. Cualquiera que conozca el enlace de edición puede cambiar tu lambda, así que trátalo como una contraseña. Las lambdas del plan gratuito siguen en línea mientras se usen: si nadie visita ni edita una durante un mes, se desconecta, y se elimina si no pasa nada en los dos meses siguientes. Todo lo que despliegues puede eliminarse en cualquier momento.',
};
