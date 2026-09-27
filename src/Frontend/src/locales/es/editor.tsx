import type { EditorMessages } from '../en/editor';

/** Los textos del editor en español. */
export const editor: EditorMessages = {
  shared: {
    units: { s: 's', min: 'min', h: 'h', d: 'd' },
    never: 'nunca',
    justNow: 'ahora mismo',
    ago: (span) => `hace ${span}`,
    in: (span) => `en ${span}`,
    origins: {
      agent: 'agente',
      template: 'plantilla',
      admin: 'operador',
      system: 'plataforma',
      api: 'API / editor',
      unknown: 'desconocido',
    },
    endings: {
      replaced: 'sustituido por un despliegue más reciente',
      stopped: 'desconectado',
      expired: 'caducado por falta de uso',
      admin: 'desconectado por el operador',
      ended: 'finalizado',
    },
    whatThisIs: 'Explicación',
    byAgent: 'por un agente',
    writtenByAgent: 'Escrito por un agente',
    more: 'Más',
    of: (used, total) => `${used} de ${total}`,
    online: (version) => `En línea · v${version}`,
    onlineTitle: (version) => `En línea, sirviendo la versión ${version}`,
    offline: 'Desconectado',
    offlineTitle: 'Desconectado: no se sirve nada',
    premium:
      'Premium: puede responder en un dominio propio, dispone de más espacio para código, recursos y datos, y permanece en línea independientemente de su actividad',
    demo: 'Demo: mantenida en línea por esta instalación y de solo lectura',
    tier: (tier) => `Plan ${tier}`,
    entrances: {
      title: 'Accedido mediante',
      note: 'Desde el inicio del servidor, incluidas las conexiones WebSocket.',
    },
    chart: {
      showChart: 'Mostrar gráfico',
      showValues: 'Mostrar valores',
      none: 'Todavía no hay mediciones.',
      time: 'Hora',
    },
    diagnostics: {
      compiles: 'El código compila.',
      none: 'Todavía no hay mensajes. Compruebe o despliegue el código para compilarlo.',
      line: (line) => `línea ${line}`,
    },
  },

  frame: {
    title: 'Editor',
    sections: {
      overview: 'Resumen',
      showcase: 'Galería',
      domain: 'Dominio',
      files: 'Archivos',
      versions: 'Versiones',
      deployments: 'Despliegues',
      stats: 'Estadísticas',
      logs: 'Registros',
      code: 'Código',
    },
    sectionsLabel: 'Secciones',
    loadFailed: 'No se pudo cargar este lambda.',
    online: (version) => `La versión ${version} está en línea.`,
    deployFailed: 'No se pudo desplegar el lambda.',
    offline: 'Desconectado. El código se conserva.',
    offlineFailed: 'No se pudo desconectar el lambda.',
    leave: 'Se perderán los cambios no guardados en el código. ¿Desea salir de todos modos?',
    nothingTitle: 'Este enlace no abre ningún lambda',
    createNew: 'Crear un lambda nuevo',
    loading: 'Cargando su lambda…',
    moreActions: 'Más acciones',
    redeploy: (version) => `Volver a desplegar la versión ${version}`,
    takeOffline: 'Desconectar',
    copyLink: 'Copiar el enlace',
    copyPrivate: 'Copiar el enlace privado',
    privateLink: 'Cualquier persona con este enlace puede modificar el lambda. Manténgalo en privado.',
    rename: 'Cambiar la dirección',
    download: 'Descargar como proyecto .NET',
    delete: 'Eliminar este lambda',
    deploy: (version) => `Desplegar la versión ${version}`,
    problems: 'Se han producido errores recientemente',
    demoTitle: 'Una demo, mantenida en línea por esta instalación y de solo lectura.',
    demo: (start) => (
      <>
        Su código, su historial, sus datos y sus registros están disponibles para consulta. Para modificarla,{' '}
        {start('cree un lambda propio a partir de ella')}.
      </>
    ),
    keep: 'Conserve este enlace. Es el único acceso a este lambda.',
    gotIt: 'Entendido',
    rejected: (version) => `La versión ${version} no se ha puesto en línea`,
    refused: 'El despliegue fue rechazado',
    openCode: 'Abrir el código',
    close: 'Cerrar',
    notCompiling: 'El código no compila. La versión que estaba en línea sigue en línea.',
    moved: (path) => `Ahora disponible en ${path}.`,
    deleteTitle: '¿Eliminar este lambda?',
    cancel: 'Cancelar',
    deleteForGood: 'Eliminar definitivamente',
    deleteFailed: 'No se pudo eliminar el lambda.',
    deleteText: (key) => (
      <>Se eliminarán todas las versiones, sus archivos, su historial y la dirección {key}. Esta acción no se puede deshacer.</>
    ),
    openInTab: 'Abrir en una pestaña nueva',
    open: (address) => `Abrir ${address} en una pestaña nueva`,
    copyAddress: 'Copiar la dirección',
    renameFailed: 'No se pudo cambiar la dirección.',
    moveIt: 'Cambiar la dirección',
    renameText: 'La dirección anterior deja de funcionar de inmediato; actualice cualquier enlace que apunte a ella.',
  },

  summary: {
    reading: 'Consultando el estado…',
    hint: (since, kept, retention, tier) =>
      `El tráfico se contabiliza desde el último inicio del servidor (${since}). ` +
      (kept
        ? `Un lambda permanece en línea mientras se utiliza y se elimina tras ${retention} días sin visitas ni cambios.`
        : `Este lambda pertenece al plan ${tier}, que lo mantiene en línea y almacenado independientemente de su actividad.`),
    onlineFor: (duration, version) => (
      <>
        En línea desde hace {duration('un tiempo')}, sirviendo la versión {version}.
      </>
    ),
    offline: 'Desconectado. No se sirve nada hasta que se despliegue una versión.',
    nothing: 'Todavía no se ha escrito nada.',
    requestsToday: 'solicitudes hoy',
    lastHour: (count) => `${count} en la última hora`,
    hourly: 'Solicitudes por hora en las últimas 24 horas',
    failed: 'con error',
    failedTitle: (failed, rejected) =>
      `${failed} errores del servidor, ${rejected} no encontradas o rechazadas, en las últimas 24 horas`,
    average: 'tiempo medio de respuesta',
    noneYet: 'ninguna todavía',
    lastVisit: 'última visita',
    problems: 'Errores recientes',
    openLog: 'Abrir el registro',
    latest: 'Último cambio',
    allVersions: 'Todas las versiones',
    noDescription: 'Sin descripción',
    version: (version) => `Versión ${version}`,
    notOnline: 'todavía no está en línea',
    wanted: 'Qué se solicitó',
    noVersions: 'Todavía no hay versiones.',
    storage: 'Almacenamiento',
    browse: 'Explorar',
    code: 'Código',
    codeWhy: 'El C# se compila y nunca se sirve.',
    characters: 'caracteres',
    assets: 'Recursos',
    assetsPublic: 'Público: el código los sirve.',
    assetsPrivate: 'El código no los sirve.',
    data: 'Datos',
    dataPublic: 'Público: el código sirve el workspace.',
    dataPrivate: 'Privados del lambda.',
  },

  files: {
    hint: (b) => (
      <>
        El {b('Código')} se compila y nunca se sirve. Los {b('Recursos')} – páginas, estilos, imágenes – se guardan con cada
        versión y son públicos si el código los sirve. Los {b('Datos')} son lo que el lambda escribe mientras se ejecuta; no
        forman parte de ninguna versión y solo son públicos si el código los sirve.
      </>
    ),
    edit: 'Editar esta versión',
    version: 'Versión',
    shown: (version, online, newest) => `Versión ${version}${online ? ', en línea' : newest ? ', la más reciente' : ''}`,
    optionOnline: ' (en línea)',
    readFailed: 'No se pudo leer esa versión.',
    dataFailed: 'No se pudieron leer los datos.',
    noVersion: 'Todavía no hay ninguna versión que mostrar.',
    label: 'Archivos',
    code: 'Código',
    codeWhy: 'Se compila en el lambda y nunca se sirve.',
    count: (files) => (files === 1 ? '1 archivo' : `${files} archivos`),
    codeUsage: (files, used, of) => `${files}, ${used} de ${of} caracteres`,
    usage: (files, used, of) => `${files}, ${used} de ${of}`,
    noCode: 'Esta versión no contiene código.',
    assets: 'Recursos',
    assetsPublic: 'Público: esta versión los sirve con Assets.',
    assetsPrivate: 'Guardados con el código, pero esta versión no los sirve.',
    noAssets: 'Ninguno en esta versión.',
    data: 'Datos',
    dataPublic: 'Público: esta versión los sirve con Workspace.',
    dataPrivate: 'Privados del lambda. No forman parte de ninguna versión.',
    uploadFailed: (path) => `No se pudo subir ${path}.`,
    deleteFolder: (path, held) =>
      held > 0
        ? `¿Eliminar ${path} y ${held === 1 ? 'el archivo que contiene' : `los ${held} archivos que contiene`}?`
        : `¿Eliminar la carpeta ${path}?`,
    deleteFile: (path) => `¿Eliminar ${path}? El lambda ya no podrá encontrarlo.`,
    deleteFailed: 'No se pudo eliminar.',
    full: 'El espacio de datos está lleno',
    uploadInto: (folder) => `Subir a ${folder}`,
    upload: 'Subir',
    reading: 'Leyendo…',
    noData: 'Todavía no hay datos. Lo que el lambda guarde mientras se ejecuta aparecerá aquí.',
    delete: (path) => `Eliminar ${path}`,
    deleteShort: 'Eliminar',
    fileFailed: 'No se pudo leer el archivo.',
    pick: 'Seleccione un archivo para ver su contenido.',
    tooLarge: (name, size) => (
      <>
        {name} ocupa {size}, demasiado para mostrarlo aquí.
      </>
    ),
    download: 'Descargar',
    readingFile: (name) => `Leyendo ${name}…`,
    missing: (name) => `Esta versión no contiene ningún archivo llamado ${name}.`,
    saved: 'guardado',
    notText: 'No es un archivo de texto. Descárguelo para ver su contenido.',
  },

  versions: {
    hint: (limit) =>
      `Cada versión conserva lo solicitado y lo que cambió, cuando su autor lo indicó. Cuando hay más de ${limit}, se eliminan las más antiguas; la versión en línea nunca se elimina.`,
    none: 'Todavía no hay versiones.',
    noDescription: 'Sin descripción',
    online: 'en línea',
    putOnline: 'Poner esta versión en línea',
    rollBackTitle: 'Volver a poner en línea esta versión anterior',
    deploy: 'Desplegar',
    rollBack: 'Restaurar',
    readFailed: 'No se pudo leer esta versión.',
    comparing: 'Comparando…',
    unchanged: 'Sin cambios respecto a la versión anterior.',
    first: 'La primera versión.',
    status: { added: 'añadido', removed: 'eliminado', changed: 'modificado', same: 'sin cambios' },
    browse: 'Explorar sus archivos',
    edit: 'Editar a partir de aquí',
    binary: 'No es un archivo de texto, por lo que no hay líneas que comparar.',
    tooLarge: 'Demasiado grande para compararlo línea a línea.',
  },

  deployments: {
    hint: (until) =>
      `Un despliegue permanece en línea mientras se utiliza${until ? `; si no se utiliza, hasta el ${until}` : ''}. Cada nuevo despliegue o visita reinicia ese plazo.`,
    takeOffline: 'Desconectar',
    readFailed: 'No se pudo leer el historial.',
    reading: 'Leyendo el historial…',
    none: 'Todavía no se ha desplegado nada.',
    noDescription: 'Sin descripción',
    deployed: (when, by) => `Desplegado el ${when} por ${by}`,
    duration: 'Tiempo en línea',
    online: 'en línea',
    short: {
      replaced: 'sustituido',
      stopped: 'desconectado',
      expired: 'caducado',
      admin: 'por el operador',
      ended: 'finalizado',
    },
    putBack: (version) => `Volver a poner en línea la versión ${version}`,
    timeline: 'Lo que estuvo en línea en los últimos siete días',
    block: (version, from, to) => `Versión ${version}, del ${from} ${to ? `al ${to}` : 'hasta ahora'}`,
    weekAgo: 'hace una semana',
    now: 'ahora',
  },

  stats: {
    readFailed: 'No se pudieron leer las cifras.',
    range: 'Periodo',
    lastHour: 'Última hora',
    lastDay: 'Últimas 24 horas',
    hint: (since) =>
      `Contabilizado en memoria desde el último inicio del servidor (${since}). Un reinicio pone estas cifras a cero.`,
    reading: 'Leyendo las cifras…',
    requests: 'solicitudes',
    websockets: (count) => `y ${count} conexiones WebSocket`,
    failed: 'con error',
    serverErrors: (count) => `${count} errores del servidor`,
    rejected: 'no encontradas o rechazadas',
    average: 'tiempo medio de respuesta',
    sent: (amount) => `${amount} enviados`,
    nobody: (hour) => (hour ? 'No ha recibido solicitudes en la última hora.' : 'No ha recibido solicitudes en las últimas 24 horas.'),
    requestsTitle: 'Solicitudes',
    per: (hour) => (hour ? 'Por minuto.' : 'Por intervalo de 15 minutos.'),
    answered: 'Respondidas',
    rejectedSeries: 'No encontradas o rechazadas',
    failedSeries: 'Con error',
    timeTitle: 'Tiempo de respuesta',
    averagePer: (hour) => (hour ? 'Media por minuto.' : 'Media por intervalo de 15 minutos.'),
    averageSeries: 'Media',
    mostAsked: 'Rutas más solicitadas',
    path: 'Ruta',
    requestsColumn: 'Solicitudes',
    failedColumn: 'Errores',
    averageColumn: 'Media',
    since: 'Desde el inicio del servidor.',
  },

  logs: {
    readFailed: 'No se pudo leer el registro.',
    hint: (capturing) =>
      'Solicitudes, lo que imprime el lambda y los errores, en tiempo real.' +
      (capturing ? '' : ' Esta instalación no conserva lo que imprimen los lambdas, por lo que solo aparecen solicitudes y errores.') +
      ' Se mantiene en memoria y se comparte con todos los lambdas de esta instalación, por lo que abarca de minutos a horas y se vacía tras un reinicio. No se muestran las direcciones de los visitantes.',
    search: 'Buscar',
    searchLabel: 'Buscar en el registro',
    resume: 'Mostrar las nuevas líneas a medida que llegan',
    pause: 'Dejar de añadir líneas nuevas mientras lee',
    paused: 'En pausa',
    live: 'En directo',
    show: 'Mostrar',
    all: 'Todo',
    requests: 'Solicitudes',
    output: 'Salida',
    problems: 'Errores',
    reading: 'Leyendo el registro…',
    noProblems: 'El registro actual no contiene errores.',
    nothing: 'Todavía no hay entradas. Abra la dirección del lambda y sus solicitudes aparecerán aquí.',
    noMatch: 'No hay coincidencias.',
    identical: (count) => `${count} líneas idénticas`,
    at: (domain) => `, en ${domain}`,
    from: (country) => `, desde ${country}`,
  },

  showcase: {
    loadFailed: 'No se pudo cargar la entrada de la galería.',
    loading: 'Cargando…',
    title: 'un título',
    description: 'una descripción',
    picture: 'una imagen',
    updated: 'La entrada de la galería se ha actualizado.',
    listed: 'Ya aparece en la galería.',
    waiting: 'Guardado. Aparecerá en la galería en cuanto el lambda esté en línea.',
    saveFailed: 'No se pudo guardar la entrada de la galería.',
    removed: 'Retirado de la galería.',
    removeFailed: 'No se pudo retirar la entrada de la galería.',
    wrongType: 'No es una imagen PNG, JPEG, GIF o WebP.',
    tooLarge: (size, limit) => `El archivo ocupa ${size}; el máximo permitido es ${limit}.`,
    unreadable: 'No se pudo leer el archivo.',
    hint: (tool) => (
      <>
        La galería muestra los lambdas que sus propietarios han decidido mostrar, primero los más utilizados
        recientemente. Solo quien tiene la clave de edición puede añadir o retirar un lambda, que solo aparece mientras
        está en línea. Un agente puede hacer lo mismo con la herramienta {tool}.
      </>
    ),
    open: 'Abrir la galería',
    switch: 'Mostrar este lambda en la galería',
    listedNow: 'Visible ahora. Cualquiera que consulte la galería puede abrirlo.',
    notListed: 'Guardado, pero no visible: el lambda está desconectado. Volverá a aparecer cuando se despliegue de nuevo.',
    off: 'Desactivado. Este lambda no se muestra en ningún lugar hasta que active esta opción y guarde.',
    offline: 'El lambda está desconectado, por lo que la entrada esperará a que se despliegue. Solo se muestran los lambdas que responden.',
    titleLabel: 'Título',
    titlePlaceholder: 'Marcador para la noche de preguntas',
    descriptionLabel: 'Descripción',
    descriptionPlaceholder:
      'Los equipos introducen sus respuestas en el móvil, el presentador las corrige y el marcador se actualiza para toda la sala.',
    save: 'Guardar los cambios',
    add: 'Añadir a la galería',
    takeOff: 'Retirar',
    needs: (missing) =>
      `Todavía falta ${missing.length > 1 ? `${missing.slice(0, -1).join(', ')} y ${missing[missing.length - 1]}` : missing[0]}.`,
    tooLong: 'Algunos campos son demasiado largos.',
    allSaved: 'Todo está guardado.',
    preview: 'Vista previa',
    card: (address) => <>Así verán la tarjeta los visitantes. Abre {address}.</>,
    confirm: '¿Retirar de la galería?',
    keep: 'Mantener',
    confirmText: 'Se eliminarán el título, la descripción y la imagen. El lambda en sí permanece intacto.',
    pictureLabel: 'Imagen',
    formats: (limit) => `PNG, JPEG, GIF o WebP, hasta ${limit}`,
    notSaved: 'sin guardar',
    replace: 'Arrastre aquí una imagen nueva para sustituirla.',
    drop: 'Arrastre una imagen aquí.',
    advice: 'Lo más adecuado es una captura de pantalla o un GIF breve de su uso, en formato 16:10.',
    another: 'Elegir otra',
    choose: 'Elegir un archivo',
    keepSaved: 'Mantener la guardada',
    clear: 'Quitar',
  },

  domain: {
    readFailed: 'No se pudo leer el dominio.',
    reaching: (domain) => `Las solicitudes a ${domain} llegan ahora a este lambda.`,
    saveFailed: 'No se pudo guardar el dominio.',
    removed: 'El dominio se ha eliminado. El lambda sigue respondiendo en su dirección de esta plataforma.',
    removeFailed: 'No se pudo eliminar el dominio.',
    hint:
      'Un lambda Premium puede responder en un dominio propio – completo, desde la raíz – además de en su dirección de esta plataforma. Dirija el dominio a este servidor, introdúzcalo aquí y las solicitudes que reciba llegarán al lambda.',
    loading: 'Cargando…',
    example: 'su-dominio.com',
    open: (domain) => `Abrir ${domain}`,
    label: 'Dominio en el que responde',
    serving: (domain) => <>Sirviendo {domain} actualmente, además de su dirección en esta plataforma.</>,
    none: 'Ninguno todavía. Un subdominio como shop.example.com o un dominio completo como example.com.',
    change: 'Cambiar',
    use: 'Usar este dominio',
    remove: 'Eliminar',
    confirm: '¿Eliminar el dominio?',
    keep: 'Mantener',
    confirmText: (domain) => (
      <>
        Las solicitudes a {domain} dejarán de llegar a este lambda de inmediato. Su dirección en esta plataforma no cambia,
        como tampoco la configuración DNS del dominio.
      </>
    ),
    point: 'Dirigir el dominio a este servidor',
    check: 'Volver a comprobar',
    records:
      'En el proveedor que gestiona el DNS del dominio, añada estos dos registros. Puede omitir el registro AAAA si prefiere que no sea accesible mediante IPv6.',
    type: 'Tipo',
    name: 'Nombre',
    value: 'Valor',
    pointsHere: (domain) => <>{domain} apunta a este servidor.</>,
    alsoElsewhere: (addresses) =>
      ` También se resuelve a ${addresses}, que no es este servidor; los visitantes dirigidos allí no llegarán al lambda.`,
    elsewhere: (addresses) => `Se resuelve a ${addresses}, que todavía no es este servidor.`,
    wait: 'Un cambio puede tardar en verse en todas partes, hasta el tiempo de vida (TTL) del registro anterior.',
    cname: 'Usar un registro CNAME en su lugar',
    cnameText: (target) => (
      <>
        Un subdominio puede apuntar a {target} mediante un registro CNAME y así seguir a este servidor si sus direcciones
        cambian. Esta opción tiene inconvenientes:
      </>
    ),
    cnameRoot: (example) => (
      <>
        No puede utilizarse para un dominio completo ({example} en sí): el estándar no permite un CNAME junto a los
        registros que todo dominio tiene en su raíz. Algunos proveedores ofrecen para ello un registro ALIAS, ANAME o
        «aplanado».
      </>
    ),
    cnameAlone: 'No puede haber ningún otro registro con el mismo nombre: ni MX para el correo ni TXT para verificaciones.',
    cnameLookup: 'Los resolutores de los visitantes realizan una consulta adicional.',
    copy: 'Copiar',
    copyValue: (value) => `Copiar ${value}`,
  },

  code: {
    title: 'Código',
    version: (version) => `versión ${version}`,
    edited: ', modificada',
    online: ', en línea',
    loadFailed: 'No se pudo cargar esa versión.',
    compiles: 'El código compila.',
    notYet: 'El código todavía no compila.',
    checkFailed: 'No se pudo comprobar el código.',
    saved: (version) => `Guardado como versión ${version}.`,
    isOnline: (version) => `La versión ${version} está en línea.`,
    notOnline: 'No se ha puesto en línea. Consulte los mensajes del compilador a continuación.',
    failed: 'La operación no se completó.',
    unchanged: 'No hay cambios desde el último guardado.',
    demo: 'Es una demo, por lo que todo es de solo lectura. Cree un lambda propio a partir de ella para modificarla. ',
    edit: 'Edite el código manualmente. Guardar crea una nueva versión sin modificar la que está en línea; desplegar la pone en línea. ',
    files: (entry, cs) => (
      <>
        {entry} devuelve lo que se sirve, los demás archivos {cs} contienen tipos y cualquier otro archivo se sirve tal
        cual. Ctrl+S guarda; F12 va a una declaración.
      </>
    ),
    newer: (version) => ` La versión ${version} es más reciente que la abierta aquí.`,
    check: 'Comprobar',
    save: 'Guardar',
    deploy: 'Desplegar',
    binary: (size) => `No es un archivo de texto, por lo que no se puede editar. Se sirve tal cual y ocupa ${size} kB.`,
    saveAndDeploy: 'Guardar y desplegar',
    saveVersion: 'Guardar una nueva versión',
    cancel: 'Cancelar',
    what: '¿Qué cambia? Opcional; se muestra en el historial.',
    placeholder: 'Añade un formulario de contacto',
    goToDefinition: 'Ir a la definición',
  },

  tabs: {
    codeName: 'Letras, dígitos, guiones y guiones bajos, con la extensión .cs',
    slashes: 'Sin barra al principio ni al final, y con menos de 120 caracteres.',
    deep: 'Seis niveles de carpetas como máximo.',
    characters: 'Letras, dígitos, guiones, guiones bajos y puntos, separados por barras.',
    extension: 'Necesita una extensión para poder servirse correctamente.',
    exists: 'Ya existe un archivo con ese nombre.',
    remove: (name) => `¿Eliminar ${name}? Su contenido también se eliminará.`,
    there: (name) => `${name} ya existe.`,
    entry: 'El fragmento principal: lo que devuelve es lo que se sirve',
    errors: 'contiene errores',
    removeFile: (name) => `Eliminar ${name}`,
    removeTitle: 'Eliminar este archivo',
    placeholder: 'Types.cs o site/index.html',
    newFile: 'Archivo nuevo',
    uploadTitle: 'Subir un archivo: una imagen, una fuente, una página',
    upload: 'Subir un archivo',
  },
};
