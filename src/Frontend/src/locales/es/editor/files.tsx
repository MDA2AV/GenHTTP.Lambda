import type { EditorMessages } from '../../en/editor';

export const files: EditorMessages['files'] = {
  hint: (b) => (
    <>
      Los archivos de una versión: el programa. El {b('Código')} se compila y nunca se sirve. Los {b('Recursos')}
      (páginas, scripts, estilos, imágenes) se guardan con el código, se despliegan y se restauran con él, y son públicos
      si el código los sirve. Lo que la lambda guarda mientras se ejecuta no está aquí: son sus {b('Datos')}.
    </>
  ),
  scope: (version, data) => (
    <>
      Pertenecen a la versión {version} y cambian con ella. Lo que la lambda guarda mientras se ejecuta es lo mismo para
      todas las versiones, y está en {data('Datos')}.
    </>
  ),
  edit: 'Editar esta versión',
  version: 'Versión',
  shown: (version, online, newest) => `Versión ${version}${online ? ', en línea' : newest ? ', la más nueva' : ''}`,
  optionOnline: ' (en línea)',
  readFailed: 'No se pudo leer esa versión.',
  noVersion: 'Todavía no hay ninguna versión que mostrar.',
  label: 'Archivos',
  code: 'Código',
  codeWhy: 'Se compila en la lambda, nunca se sirve.',
  count: (files) => (files === 1 ? '1 archivo' : `${files} archivos`),
  codeUsage: (files, used, of) => `${files}, ${used} de ${of} caracteres`,
  usage: (files, used, of) => `${files}, ${used} de ${of}`,
  noCode: 'No hay código en esta versión.',
  assets: 'Recursos',
  assetsPublic: 'Públicos: esta versión los sirve con Assets.',
  assetsPrivate: 'Se guardan con el código, pero esta versión no los sirve.',
  noAssets: 'Ninguno en esta versión.',
  context: 'Documentación y pruebas',
  contextWhy: 'Nunca se compilan ni se sirven: lo que está escrito sobre esta versión, para quien la lea o la cambie.',
  contextUsage: (files, size) => `${files}, ${size} - cuentan junto con los recursos`,
  noContext: 'Todavía no hay nada escrito sobre esta versión.',
  data: 'Datos',
  dataPublic: 'Públicos: el código en línea los sirve con Workspace.',
  dataPrivate: 'Privados: solo para la lambda. No forman parte de ninguna versión.',
  uploadFailed: (path) => `No se pudo subir ${path}.`,
  deleteFolder: (path, held) =>
    held > 0
      ? `¿Eliminar ${path} y ${held === 1 ? 'el archivo que contiene' : `los ${held} archivos que contiene`}?`
      : `¿Eliminar la carpeta ${path}?`,
  deleteFile: (path) => `¿Eliminar ${path}? La lambda ya no lo va a encontrar.`,
  deleteFailed: 'No se pudo eliminar.',
  full: 'El espacio para datos está lleno',
  uploadInto: (folder) => `Subir a ${folder}`,
  upload: 'Subir',
  reading: 'Leyendo…',
  noData: 'Nada todavía. Aquí aparece lo que la lambda guarda mientras se ejecuta.',
  delete: (path) => `Eliminar ${path}`,
  deleteShort: 'Eliminar',
  fileFailed: 'No se pudo leer el archivo.',
  pick: 'Elige un archivo para ver qué contiene.',
  tooLarge: (name, size) => (
    <>
      {name} ocupa {size}, demasiado para mostrarlo aquí.
    </>
  ),
  download: 'Descargar',
  readingFile: (name) => `Leyendo ${name}…`,
  missing: (name) => `Esta versión no tiene ningún archivo llamado ${name}.`,
  saved: 'guardado',
  notText: 'No es texto. Descárgalo para ver qué contiene.',
};
