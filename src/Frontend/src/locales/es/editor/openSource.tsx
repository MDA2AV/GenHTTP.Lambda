import type { EditorMessages } from '../../en/editor';

export const openSource: EditorMessages['openSource'] = {
  loading: 'Cargando…',
  loadFailed: 'No se pudo saber si el código está publicado.',
  hint: (tool) => (
    <>
      Una lambda publicada tiene su propia página entre las apps de código abierto, donde cualquiera puede leerla,
      darle una estrella y descargarla: cada una de sus versiones, bajo la licencia que elijas, y nunca los datos que
      conserva. Solo quien tiene la clave de edición puede publicarla o retirarla. Un agente puede hacer lo mismo con
      su herramienta {tool}.
    </>
  ),
  hintSimple:
    'Cualquiera puede leer en su propia página cómo está hecha tu app y construir a partir de ella bajo la licencia que elijas, nunca con lo que conserva. Solo tú puedes publicar el código o retirarlo.',
  open: 'Abrir la página del código',
  switch: 'Publicar el código de esta app',
  publishedNow: (license) => `Publicado bajo la licencia ${license}. Cualquiera puede leerlo y descargarlo.`,
  off: 'Desactivado. Nadie ve el código hasta que lo publiques.',
  keptStars: (stars) =>
    stars === 1
      ? 'Su estrella se conserva para cuando vuelvas a publicarlo.'
      : `Sus ${stars} estrellas se conservan para cuando vuelvas a publicarlo.`,
  published: 'Publicado. Ya cualquiera puede leer el código.',
  saved: 'Guardado.',
  saveFailed: 'No se pudo publicar el código.',
  withdrawn: 'Retirado. Su página ya no existe.',
  withdrawFailed: 'No se pudo retirar el código.',
  whatTitle: 'Qué se publica',
  what: [
    'Su código: tal como es ahora y en cada estado anterior',
    'Todo lo que muestra: sus páginas, estilos e imágenes',
    'Lo que está escrito sobre ella: para qué sirve y cómo se prueba',
    'Cada cambio por el que pasó, en una línea cada uno',
  ],
  neverTitle: 'Qué no se publica nunca',
  never: [
    'Lo que conserva: sus registros, lo que ha guardado, sus claves y contraseñas',
    'Lo que pediste, con tus propias palabras',
    'Quién la usa: sus visitantes y lo que hicieron',
    'El enlace de edición',
  ],
  careful:
    'Todo lo que hay en el código se hace público, también sus estados anteriores. Una contraseña o una clave nunca va en el código: su lugar está en Claves y contraseñas, dentro de Datos, que nunca se publican.',
  licenseLabel: 'Licencia',
  licenseHint:
    'Lo que otros pueden hacer con el código. MIT, la más común, permite a cualquiera hacer casi cualquier cosa con él siempre que tu nombre siga figurando.',
  readLicense: 'Leer la licencia',
  authorLabel: 'Nombre en la licencia',
  optional: 'opcional',
  authorPlaceholder: (key) => `Los autores de ${key}`,
  authorHint:
    'Tu nombre o el de tu organización, que se muestra en la página del código y en la licencia. Si lo dejas vacío, la licencia nombra a los autores de esta app.',
  publish: 'Publicar',
  save: 'Guardar cambios',
  allSaved: 'Todo está guardado.',
  takeDown: 'Retirar el código',
  confirm: '¿Retirar el código?',
  confirmText:
    'Su página y sus descargas desaparecen al instante. Quien ya lo descargó lo conserva bajo la licencia con la que lo recibió. Sus estrellas se conservan para cuando vuelvas a publicarlo.',
  keep: 'Mantenerlo publicado',
  stars: (count) => (count === 1 ? '1 estrella' : `${count} estrellas`),
  sidebar: 'Código fuente',
};
