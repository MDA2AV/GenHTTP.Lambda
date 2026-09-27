import type { Messages } from '../en';

export const create: Messages['create'] = {
  title: 'Crear un lambda',
  whatTitle: '¿Qué desea crear?',
  whatText:
    'Elija la opción más parecida y empezará con una copia de algo que ya funciona, totalmente modificable. También puede empezar desde cero.',
  seeIt: 'Ver en funcionamiento',
  startFrom: 'Usar esta plantilla',
  starters: {
    'demo-crud': {
      title: 'Gestionar elementos',
      description: 'Una lista que se puede ampliar, modificar y marcar: tareas, notas, marcadores o un pequeño inventario.',
    },
    'demo-registration': {
      title: 'Registro e inicio de sesión',
      description: 'Cuentas con registro e inicio de sesión, y páginas visibles solo para los usuarios registrados.',
    },
    'demo-game': {
      title: 'Un juego multijugador',
      description: 'Algo que varias personas utilizan al mismo tiempo, en tiempo real en sus navegadores.',
    },
    'demo-files': {
      title: 'Compartir archivos e imágenes',
      description: 'Las personas suben imágenes o documentos y los demás pueden verlos.',
    },
    'demo-live': {
      title: 'Actualizaciones en tiempo real',
      description: 'Una página que se actualiza sola en cuanto algo cambia: votos, puntuaciones, un panel.',
    },
    empty: {
      title: 'Lambda vacío',
      description: 'Empiece con un lambda vacío y desarrolle su propia idea.',
    },
  },

  addressTitle: 'Asignar una dirección',
  fromDemo: (title) => <>{title}: su lambda comienza como una copia de la demo, totalmente modificable.</>,
  fromNothing: 'Su lambda comienza vacío, listo para lo que tenga en mente.',
  pickAgain: 'Elegir otra opción',
  publicKey: 'Clave pública',
  free: (key) => `«${key}» está disponible.`,
  keyHint: 'Letras minúsculas, dígitos y guiones; tres caracteres como mínimo. Déjelo vacío para obtener una clave aleatoria.',
  accept: 'Acepto las condiciones del servicio',
  fullTerms: 'Leer las condiciones del servicio completas',
  back: 'Atrás',
  creating: 'Creando…',
  submit: 'Crear lambda',
  keepLink: 'En la siguiente pantalla verá su enlace de edición. Es el único acceso, así que consérvelo.',
  failed: 'No se pudo crear el lambda.',
};
