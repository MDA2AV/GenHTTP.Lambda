import type { EditorMessages } from '../../en/editor';

export const showcase: EditorMessages['showcase'] = {
  loadFailed: 'No se pudo cargar la galería.',
  loading: 'Cargando…',
  title: 'un título',
  description: 'una descripción',
  picture: 'una imagen',
  updated: 'Se actualizó la entrada de la galería.',
  listed: 'Ya aparece en la galería.',
  waiting: 'Guardado. Aparecerá en la galería cuando la lambda esté en línea.',
  saveFailed: 'No se pudo guardar la entrada de la galería.',
  removed: 'Retirada de la galería.',
  removeFailed: 'No se pudo retirar la entrada de la galería.',
  wrongType: 'No es una imagen PNG, JPEG, GIF ni WebP.',
  tooLarge: (size, limit) => `Ocupa ${size}; una imagen puede ocupar ${limit} como máximo.`,
  unreadable: 'No se pudo leer ese archivo.',
  hint: (tool) => (
    <>
      La galería muestra las lambdas que sus dueños quisieron mostrar, primero las que más se han usado últimamente. Solo
      quien tiene la clave de edición puede poner una lambda ahí o retirarla, y solo aparece mientras está en línea. Un
      agente puede hacer lo mismo con su herramienta {tool}.
    </>
  ),
  open: 'Abrir la galería',
  switch: 'Mostrar esta lambda en la galería',
  listedNow: 'Ya aparece. Cualquiera que visite la galería puede abrirla.',
  notListed: 'Guardado, pero no aparece: la lambda está desconectada. Volverá a aparecer cuando la despliegues de nuevo.',
  off: 'Desactivado. Nada de esta lambda se muestra en ninguna parte hasta que actives esto y guardes.',
  offline: 'La lambda está desconectada, así que la entrada esperará a que la despliegues. Solo aparecen las lambdas que responden.',
  titleLabel: 'Título',
  titlePlaceholder: 'Marcador de la trivia del bar',
  descriptionLabel: 'Descripción',
  descriptionPlaceholder:
    'Los equipos escriben sus respuestas en el teléfono, el presentador las corrige y el marcador se actualiza para toda la sala.',
  save: 'Guardar cambios',
  add: 'Añadir a la galería',
  takeOff: 'Retirar',
  needs: (missing) =>
    missing.length > 1
      ? `Todavía faltan ${missing.slice(0, -1).join(', ')} y ${missing[missing.length - 1]}.`
      : `Todavía falta ${missing[0]}.`,
  tooLong: 'Hay campos demasiado largos.',
  allSaved: 'Todo está guardado.',
  preview: 'Vista previa',
  card: (address) => <>Esta es la tarjeta que ven los visitantes. Abre {address}.</>,
  confirm: '¿Retirarla de la galería?',
  keep: 'Mantenerla',
  confirmText: 'Se eliminan el título, la descripción y la imagen. La lambda queda exactamente como está.',
  pictureLabel: 'Imagen',
  formats: (limit) => `PNG, JPEG, GIF o WebP, hasta ${limit}`,
  notSaved: 'sin guardar',
  replace: 'Suelta una nueva aquí para reemplazarla.',
  drop: 'Suelta una imagen aquí.',
  advice: 'Una captura de pantalla, o un GIF corto de la app en uso, queda mejor en 16:10.',
  another: 'Elegir otra',
  choose: 'Elegir un archivo',
  keepSaved: 'Mantener la guardada',
  clear: 'Quitar',
};
