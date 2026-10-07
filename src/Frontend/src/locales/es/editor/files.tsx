import type { EditorMessages } from '../../en/editor';

export const files: EditorMessages['files'] = {
  version: 'Versión',
  shown: (version, online, newest) => `Versión ${version}${online ? ', en línea' : newest ? ', la más nueva' : ''}`,
  optionOnline: ' (en línea)',
  count: (files) => (files === 1 ? '1 archivo' : `${files} archivos`),
  usage: (files, used, of) => `${files}, ${used} de ${of}`,
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
