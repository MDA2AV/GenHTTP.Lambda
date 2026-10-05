import type { SourceMessages } from '../en/source';

/** Los textos de las páginas del código publicado, /source y cada proyecto, en español. */
export const source: SourceMessages = {
  shell: {
    section: 'Código abierto',
    home: 'GenHTTP Lambda, la página de inicio',
  },

  lambda: {
    label: '¿Qué es una lambda?',
    text: 'Una app web en GenHTTP Lambda: alguien dice lo que quiere, un agente de IA la escribe en C# y en pocos minutos está en línea en su propia dirección, con cada versión guardada junto con lo que cambió.',
    build: 'Crea la tuya',
  },

  catalog: {
    eyebrow: 'Código abierto',
    title: 'Mira cómo están hechas las apps de aquí',
    intro:
      'Lambdas cuyos dueños publicaron su código: cada versión, lo que cambió, su documentación y sus pruebas. Léelo aquí o descarga un proyecto que funciona en cualquier sitio donde funcione .NET.',
    searchLabel: 'Buscar en los proyectos',
    searchPlaceholder: 'Busca por nombre o por lo que hace',
    orderLabel: 'Orden',
    orders: {
      stars: 'Más estrellas',
      updated: 'Actualizados recientemente',
      published: 'Publicados recientemente',
    },
    counted: (total) => (total === 1 ? '1 proyecto' : `${total} proyectos`),
    failed: 'No se pudieron cargar los proyectos.',
    loadingMore: 'Cargando más…',
    showMore: 'Ver más',
    nothingTitle: 'Todavía no se ha publicado nada',
    nothing: (tab) => (
      <>
        ¿Hiciste algo de lo que otros podrían aprender? Abre su centro de control, elige {tab('Código abierto')},
        escoge una licencia y su código aparecerá aquí.
      </>
    ),
    noMatchTitle: 'No hay coincidencias',
    noMatch: (query) => `Ningún proyecto publicado menciona «${query}».`,
    clear: 'Ver todos los proyectos',
    yoursTitle: 'Publica el tuyo',
    yours: (tab) => (
      <>
        Abre el centro de control de tu lambda y elige {tab('Código abierto')}, o pídele al agente que la creó que la
        publique. Solo puede hacerlo quien tenga la clave de edición, bajo la licencia que elija, y lo que conserva la app
        (sus registros, archivos y claves) nunca forma parte de ello.
      </>
    ),
    build: 'Crea algo',
    online: 'En línea',
    offline: 'Fuera de línea',
    changed: (ago) => `actualizado ${ago}`,
    stars: (count) => (count === 1 ? '1 estrella' : `${count} estrellas`),
  },

  project: {
    loading: 'Cargando el código…',
    failed: 'No se pudo cargar el código.',
    missingTitle: 'Aquí no hay código publicado',
    missing: 'Puede que su dueño lo haya retirado, o que nunca haya habido una lambda en esta dirección.',
    all: 'Todos los proyectos',
    by: (name) => `por ${name}`,
    versions: (count) => (count === 1 ? '1 versión' : `${count} versiones`),
    onlineAt: (address) => <>En línea en {address}</>,
    offline: 'Fuera de línea ahora mismo',
    openApp: 'Abrir la app',
    opens: (address) => `Abre ${address} en una pestaña nueva`,
    published: (ago) => `Publicado ${ago}`,
    changed: (ago) => `Actualizado ${ago}`,
    picture: (name) => `${name}, tal como se ve`,
    tabsLabel: 'Qué leer',
    tabs: {
      code: 'Código',
      docs: 'Documentación',
      tests: 'Pruebas',
      changes: 'Cambios',
    },
  },

  versions: {
    label: 'Versión',
    choose: 'Leer otra versión',
    newest: 'más reciente',
    online: 'en línea',
    older: (version, ago, newest) =>
      `Estás leyendo la versión ${version}, guardada ${ago}. La más reciente es la versión ${newest}.`,
    toNewest: 'Leer la más reciente',
    noChange: 'Sin nota sobre lo que cambió',
  },

  star: {
    star: 'Estrella',
    add: 'Dar una estrella a este proyecto',
    remove: 'Quitar tu estrella',
    count: (count) => (count === 1 ? '1 estrella' : `${count} estrellas`),
    failed: 'No se pudo guardar la estrella.',
  },
  clone: {
    button: 'Código',
    title: 'Clonar con git',
    what: (oldest, newest) =>
      oldest === newest
        ? `Su versión es el commit de main, etiquetado v${newest}.`
        : `Cada versión viene como un commit de main, etiquetado de v${oldest} a v${newest}; main es la más reciente.`,
    readOnly:
      'Solo lectura. Para construir sobre ella, crea una lambda propia y trae estos archivos: AGENTS.md en el clon explica cómo, y su licencia, qué puedes hacer.',
  },

  download: {
    title: (version) => `La versión ${version} como proyecto`,
    what:
      'Un proyecto .NET 10 con un Dockerfile, su documentación, sus pruebas y su licencia. Lo que conserva la app (sus registros, los archivos que guardó, sus claves) no forma parte de él.',
    zip: 'Descargar ZIP',
    preparing: 'Preparando el proyecto…',
    slow: 'La primera vez que se descarga una versión, se empaqueta mientras esperas.',
    failed: 'No se pudo preparar el proyecto. Vuelve a intentarlo en un momento.',
    run: 'Ejecutarlo',
    local: 'Con el SDK de .NET 10:',
    container: 'O en un contenedor:',
    agent: 'O pásale la carpeta a tu agente de programación y construye a partir de ella, respetando su licencia.',
    copy: 'Copiar',
    copied: 'Copiado',
  },

  tree: {
    label: 'Archivos',
    files: (count) => (count === 1 ? '1 archivo' : `${count} archivos`),
    packing: 'Empaquetando esta versión…',
    packingSlow: 'Una versión se empaqueta la primera vez que alguien la lee, y si es grande tarda un momento.',
    failed: 'No se pudieron cargar los archivos de esta versión.',
    legend: 'Qué es cada cosa',
    kinds: {
      code: 'El código propio de la lambda',
      asset: 'Lo que se sirve: páginas, scripts, estilos, imágenes, y sus migraciones de la base de datos',
      docs: 'Qué es y por qué está hecha así',
      tests: 'Cómo se prueba',
      dev: 'Aquello a partir de lo que se compilan sus recursos: el proyecto de su frontend',
      platform: 'Lo que sustituye a la plataforma',
      project: 'El host, la compilación, el contenedor y la licencia',
    },
    short: {
      code: 'Código',
      asset: 'Servido',
      docs: 'Docs',
      tests: 'Pruebas',
      dev: 'Dev',
      platform: 'Plataforma',
      project: 'Proyecto',
    },
  },

  file: {
    loading: 'Cargando…',
    failed: 'No se pudo cargar este archivo.',
    missing: (path) => `No hay ningún archivo ${path} en esta versión.`,
    binary: 'Este archivo no es texto.',
    tooLarge: 'Este archivo es demasiado largo para mostrarlo aquí.',
    download: 'Descargar',
    raw: 'Raw',
    rawTitle: 'Abrir el archivo tal cual',
    copy: 'Copiar',
    copied: 'Copiado',
    lines: (count) => (count === 1 ? '1 línea' : `${count} líneas`),
    plain: 'Se muestra sin colores: es largo.',
    line: (line) => `Línea ${line}`,
  },

  docs: {
    pages: 'Páginas',
    product: 'Qué es',
    decisions: 'Decisiones',
    loading: 'Cargando…',
    failed: 'No se pudo cargar esta página.',
    noneTitle: 'No hay nada escrito sobre esta versión',
    none: 'Su documentación estaría en docs/: qué es la app, para quién es y por qué está hecha como está.',
  },

  tests: {
    files: 'Scripts y datos',
    noneTitle: 'Esta versión no dice nada sobre sus pruebas',
    none: 'Cómo se prueba estaría en tests/README.md, con los scripts que ejecuta al lado.',
  },

  changes: {
    title: 'Todas las versiones, de la más reciente a la más antigua',
    intro: 'Una versión nunca cambia una vez guardada. Cada una dice en una línea qué cambió.',
    agent: 'Escrito por un agente',
    online: 'en línea',
    browse: 'Leer el código',
    noChange: 'Sin nota',
  },

  licenses: {
    MIT: 'Cualquiera puede usarlo, modificarlo y redistribuirlo, para lo que sea, siempre que la licencia y el aviso de copyright lo acompañen.',
    'Apache-2.0': 'Como MIT, con una licencia de patentes de todos los que han contribuido, y los cambios marcados como tales.',
    'BSD-3-Clause': 'Como MIT, y nadie puede usar el nombre de los autores para promocionar lo que haya hecho con él.',
    'MPL-2.0': 'Los cambios en estos archivos siguen bajo la misma licencia; se pueden combinar con código bajo cualquier otra.',
    'GPL-3.0-or-later': 'Quien lo distribuya, modificado o no, debe distribuir también su código fuente bajo la misma licencia.',
    'AGPL-3.0-or-later': 'Como la GPL, y ofrecer una copia modificada a otras personas a través de la red cuenta como distribuirla.',
    Unlicense: 'Cedido al dominio público: cualquiera puede hacer lo que quiera con él, sin condiciones.',
  },

  kinds: {
    Permissive: 'Permisiva',
    Copyleft: 'Copyleft',
    PublicDomain: 'Dominio público',
  },
};
