import type { EditorMessages } from '../../en/editor';

export const versions: EditorMessages['versions'] = {
  hint: (limit) =>
    `Una versión es el programa (su código y sus recursos) y nunca cambia una vez guardada, así que cualquiera de ellas se puede comparar y volver a poner en línea exactamente como estaba. Cada una guarda lo que se pidió y lo que cambió. Para cambiar la lambda, empieza un borrador: pasa a ser la siguiente versión cuando esté bien. Cuando hay más de ${limit}, se eliminan las más antiguas; la que está en línea, nunca.`,
  none: 'Todavía no hay versiones.',
  noDescription: 'Sin descripción',
  online: 'en línea',
  putOnline: 'Poner esta versión en línea',
  rollBackTitle: 'Volver a poner en línea esta versión anterior',
  deploy: 'Desplegar',
  rollBack: 'Restaurar',
  readFailed: 'No se pudo leer esta versión.',
  comparing: 'Comparando…',
  unchanged: 'Nada cambió respecto a la versión anterior.',
  first: 'La primera versión.',
  status: { added: 'añadido', removed: 'eliminado', changed: 'modificado', same: 'igual' },
  browse: 'Ver sus archivos',
  docs: 'Leer su documentación',
  edit: 'Editar desde aquí',
  feature: 'Empezar un borrador desde aquí',
  featureTitle:
    'Trabajar en un cambio de esta versión al lado de la lambda, y fusionarlo en la siguiente versión cuando esté bien',
  binary: 'No es texto, así que no hay líneas que comparar.',
  tooLarge: 'Es demasiado grande para compararlo línea a línea.',
};
