import type { EditorMessages } from '../../en/editor';

export const features: EditorMessages['features'] = {
  hint:
    'Un borrador es una copia de tu app para probar un cambio antes de que lo vea nadie, con una dirección y datos de prueba propios. Ponlo en línea cuando esté bien; hasta entonces, tus visitantes siguen recibiendo lo que está en línea ahora.',
  newFeature: 'Borrador nuevo',
  full: (limit) => `Ya hay ${limit} borradores, que es el máximo. Pon uno en línea o descártalo antes.`,
  emptyTitle: 'No hay borradores',
  emptyText:
    'Un borrador es una copia de tu app para probar un cambio antes de que se ponga en línea. Cuando el agente te deje un cambio para probar, lo encontrarás aquí.',
  start: 'Borrador nuevo',
  askAgentNew: 'Pedirle un cambio al agente',
  noChange: 'Todavía no dice qué cambia',
  behindTitle: 'Tu app cambió desde que empezó este borrador',
  behind: () => 'desactualizado',
  branchTitle: 'La rama de este borrador en el repositorio git de la app',
  previewOnline: 'vista previa en marcha',
  previewOutdated: 'la vista previa muestra un guardado anterior',
  previewOffline: 'vista previa detenida',
  changed: 'modificado',
  openPreview: 'Probarlo',
  openPreviewTitle: 'Abrir la vista previa en una pestaña nueva',
  count: (open, limit) => `${open} de ${limit} borradores`,
  loading: 'Cargando el borrador…',
  readFailed: 'No se pudo leer el borrador.',

  newTitle: 'Borrador nuevo',
  newText:
    'Una copia de tu app y de sus datos, con una dirección propia. Cámbiala y pruébala ahí: tus visitantes no ven nada hasta que la pongas en línea.',
  newTextFiles:
    'Lo que escribiste va al borrador en vez de convertirse en una versión, así que puedes probarlo en su propia dirección antes de ponerlo en línea.',
  name: 'Nombre',
  namePlaceholder: 'Ranking',
  wanted: '¿Qué debería hacer?',
  wantedPlaceholder: 'Opcional. Guarda las diez mejores puntuaciones y muéstralas después de cada partida.',
  olderBase: (newest) =>
    `Este parte de una versión anterior, así que está desactualizado desde el principio: antes de poder ponerse en línea, hay que incorporar lo que cambió hasta la versión ${newest}.`,
  create: 'Empezar el borrador',
  createFailed: 'No se pudo empezar el borrador.',
  retry: 'Volver a intentarlo',
  madeNotSaved: (name) =>
    `El borrador «${name}» ya está en marcha, pero lo que escribiste todavía no se pudo guardar en él. Vuelve a intentarlo, o cierra esto y busca el borrador en Borradores.`,
  created: (name) => `El borrador «${name}» ya está en marcha.`,
  cancel: 'Cancelar',

  featureHint:
    'Una copia de tu app para probar este cambio. Su vista previa tiene una dirección y datos de prueba propios, así que tus visitantes no ven nada de esto hasta que lo pongas en línea.',
  askAgent: 'Pedírselo al agente',
  askCatchUp: 'Pedirle al agente que lo ponga al día',
  catchUp: 'Pon este borrador al día con la versión más nueva de la app, y conserva lo que cambia.',
  editCode: 'Editar el código',
  deployPreview: 'Iniciar la vista previa',
  updatePreview: 'Actualizar la vista previa',
  previewDeployed: 'La vista previa está en marcha.',
  previewFailed: 'No se pudo iniciar la vista previa.',
  previewStopped: 'La vista previa está detenida.',
  previewRejected: 'La vista previa no cambió',
  previewNotCompiling: 'No compila, así que la vista previa sigue mostrando la última versión que sí compilaba.',
  started: 'Empezó',
  changes: () => 'Archivos modificados',
  noChanges: () => 'Todavía no hay cambios.',
  editNotes: 'Nombre y notas',
  what: '¿Qué cambia?',
  whatPlaceholder: 'Añade un ranking que guarda las diez mejores puntuaciones',
  missed: () => 'Lo que cambió en tu app desde que empezó',
  missedNothing: 'Nada en los archivos.',

  behindText: (_base, newest) =>
    `La versión ${newest} de tu app se guardó después de que empezara este borrador. Si lo pones en línea ahora, se desharía lo que esa versión cambió, así que antes hay que ponerlo al día: el agente puede hacerlo por ti.`,
  moveBase: 'Marcar como al día',
  close: 'Cerrar',
  mergeTitle: (name) => `Poner «${name}» en línea`,
  mergeTitleShort: 'Conviértelo en la nueva versión de tu app y ponlo en línea',
  leaks: (path, files) =>
    `${files} enlaza a ${path}, que es tu app en línea. Desde la vista previa, esos enlaces leen y cambian sus datos reales en lugar de los datos de prueba. Pide al agente que enlace sin esa parte («api/items»).`,
  mergeButton: 'Poner en línea',
  saveFirst: 'Guarda los cambios primero: la vista previa y ponerlo en línea usan lo que está guardado.',
  mergeAndDeploy: () => 'Poner en línea',
  mergeText: (version) =>
    `Pasa a ser la versión ${version} de tu app y se pone en línea. Los datos de tu app se quedan como están.`,
  deployTooNote: (active) => `La versión ${active} sigue a un clic en las versiones.`,
  deployTooOffline: 'Tu app está fuera de línea ahora; esto la pone en línea.',
  notCompiling: 'No compila, así que no se puso en línea. Corrígelo antes en el borrador.',
  mergeFailed: 'No se pudo poner el borrador en línea.',
  merged: (version) => `Guardado como versión ${version}.`,
  mergedOnline: (version) => `La versión ${version} está en línea.`,

  notesTitle: 'Nombre y notas',
  save: 'Guardar',
  saveFailed: 'No se pudo guardar.',

  baseTitle: '¿Marcarlo como al día?',
  baseText: () =>
    'Solo un borrador que contiene lo que cambió la versión más nueva puede ponerse en línea sin deshacerlo. Si esos cambios ya están en este borrador, traídos por ti o por el agente, márcalo como al día.',
  moveTo: () => 'Marcar como al día',
  baseWarning: 'Nada lo comprueba. Si los cambios no están en el borrador, al ponerlo en línea se deshacen.',

  deleteTitle: (name) => `¿Descartar «${name}»?`,
  deleteText:
    'Su código, su vista previa y sus datos de prueba se eliminan para siempre. Tu app y sus versiones no se tocan.',
  keep: 'Mantenerlo',
  deleteForGood: 'Descartar',
  deleteFailed: 'No se pudo descartar el borrador.',
  deleted: (name) => `Se descartó el borrador «${name}».`,

  all: 'Todos los borradores',
  actions: 'Más opciones de este borrador',
  download: 'Descargar como zip',
  stopPreview: 'Detener la vista previa',
  delete: 'Descartar este borrador',
  viewsLabel: 'El borrador',
  views: {
    overview: 'Borrador',
    docs: 'Documentación',
    code: 'Código',
    tests: 'Pruebas',
    data: 'Datos de prueba',
    logs: 'Logs',
  },
  missingTitle: 'Este borrador ya no existe',
  missingText: 'Se puso en línea o se descartó. Las versiones muestran qué fue de él.',
};
