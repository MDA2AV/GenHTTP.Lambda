import type { EditorMessages } from '../../en/editor';

export const tabs: EditorMessages['tabs'] = {
  codeName: 'Un archivo C# se nombra con letras, números, guiones, guiones bajos y puntos, empieza por una letra y termina en .cs, de 40 caracteres como máximo.',
  name: 'Letras, números y - _ . + @ ( ) [ ] { } $ ~, con las carpetas separadas por barras, sin espacios y sin que el nombre termine en punto.',
  taken: 'Una lambda exportada o clonada ya tiene en la raíz un archivo o una carpeta con ese nombre. Ponlo en una carpeta o elige otro nombre.',
  lambda: 'Una lambda ya no guarda una carpeta .lambda/: su documentación va en docs/ y sus pruebas en tests/.',
  assets: 'Lo que sirve una lambda está ahora en sus recursos: añádelo allí.',
  resourceName: 'Letras, números, guiones, guiones bajos y puntos, separados por barras, con como máximo seis carpetas de profundidad, y una extensión para que se sirva como lo que es.',
  exists: 'Ya hay un archivo con ese nombre.',
  remove: (name) => `¿Quitar ${name}? Se borrará su contenido.`,
  removeFolder: (name, files) => `¿Quitar ${name} y ${files === 1 ? 'el archivo' : `los ${files} archivos`} que contiene?`,
  there: (name) => `${name} ya existe.`,
  entry: 'El fragmento principal: lo que devuelve es lo que se sirve',
  errors: 'tiene errores',
  removeFile: (name) => `Quitar ${name}`,
  removeTitle: 'Quitar',
  codePlaceholder: 'Store.cs, models/Item.cs o docs/notas.md',
  resourcePlaceholder: 'web/index.html',
  upload: 'Subir un archivo',
};
