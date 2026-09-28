import type { EditorMessages } from '../en/editor';

/** Los textos del editor en español. */
export const editor: EditorMessages = {
  shared: {
    units: { s: 's', min: 'min', h: 'h', d: 'd' },
    amount: (value, unit) => `${value} ${unit}`,
    pair: (larger, smaller) => `${larger} ${smaller}`,
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
      replaced: 'reemplazado por un despliegue más nuevo',
      stopped: 'desconectado',
      expired: 'caducado por falta de uso',
      admin: 'desconectado por el operador',
      ended: 'finalizado',
    },
    whatThisIs: 'Qué es esto',
    byAgent: 'por un agente',
    writtenByAgent: 'Escrito por un agente',
    more: 'Más',
    of: (used, total) => `${used} de ${total}`,
    online: (version) => `En línea · v${version}`,
    onlineTitle: (version) => `En línea, sirviendo la versión ${version}`,
    offline: 'Fuera de línea',
    offlineTitle: 'Fuera de línea: no se sirve nada',
    premium:
      'Premium: puede responder en un dominio propio, tiene más espacio para código, recursos y datos, y sigue en línea aunque nadie la use',
    demo: 'Demo: esta instalación la mantiene en línea y es de solo lectura',
    tier: (tier) => `Plan ${tier}`,
    entrances: {
      title: 'Por dónde llegan',
      note: 'Desde que arrancó el servidor, incluidas las conexiones websocket.',
    },
    chart: {
      showChart: 'Ver gráfico',
      showValues: 'Ver valores',
      none: 'Todavía no hay mediciones.',
      time: 'Hora',
    },
    diagnostics: {
      compiles: 'El código compila.',
      none: 'Todavía no hay mensajes. Comprueba o despliega tu código para compilarlo.',
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
      logs: 'Logs',
      code: 'Código',
    },
    sectionsLabel: 'Secciones',
    loadFailed: 'No se pudo cargar esta lambda.',
    online: (version) => `La versión ${version} está en línea.`,
    deployFailed: 'No se pudo desplegar la lambda.',
    offline: 'Desconectada. El código sigue aquí.',
    offlineFailed: 'No se pudo desconectar la lambda.',
    leave: 'Vas a perder los cambios sin guardar del código. ¿Salir de todos modos?',
    nothingTitle: 'Este enlace no abre nada',
    createNew: 'Crear una lambda nueva',
    loading: 'Cargando tu lambda…',
    moreActions: 'Más acciones',
    redeploy: (version) => `Volver a desplegar la versión ${version}`,
    takeOffline: 'Desconectar',
    copyLink: 'Copiar el enlace',
    copyPrivate: 'Copiar el enlace privado',
    privateLink: 'Cualquiera con este enlace puede cambiar la lambda. No lo compartas.',
    rename: 'Cambiar la dirección',
    download: 'Descargar como proyecto .NET',
    delete: 'Eliminar esta lambda',
    deploy: (version) => `Desplegar la versión ${version}`,
    problems: 'Algo falló hace poco',
    demoTitle: 'Una demo: esta instalación la mantiene en línea y es de solo lectura.',
    demo: (start) => (
      <>
        Lee su código, su historial, lo que guarda y sus logs: para eso está. Para cambiarla,{' '}
        {start('crea tu propia lambda a partir de ella')}.
      </>
    ),
    keep: 'Guarda este enlace. Es la única forma de volver a esta lambda.',
    gotIt: 'Entendido',
    rejected: (version) => `La versión ${version} no se puso en línea`,
    refused: 'Se rechazó el despliegue',
    openCode: 'Abrir el código',
    close: 'Cerrar',
    notCompiling: 'No compila. Lo que estaba en línea sigue en línea.',
    moved: (path) => `Ahora está en ${path}.`,
    deleteTitle: '¿Eliminar esta lambda?',
    cancel: 'Cancelar',
    deleteForGood: 'Eliminar para siempre',
    deleteFailed: 'No se pudo eliminar la lambda.',
    deleteText: (key) => (
      <>Se van con ella todas las versiones, sus archivos, su historial y la dirección {key}. No se puede deshacer.</>
    ),
    openInTab: 'Abrir en una pestaña nueva',
    open: (address) => `Abrir ${address} en una pestaña nueva`,
    copyAddress: 'Copiar la dirección',
    renameFailed: 'No se pudo cambiar la dirección.',
    moveIt: 'Cambiarla',
    renameText: 'La dirección anterior deja de funcionar al instante, así que actualiza todo lo que enlace a ella.',
  },

  summary: {
    reading: 'Consultando su estado…',
    hint: (since, kept, retention, tier) =>
      `El tráfico se cuenta desde el último arranque del servidor (${since}). ` +
      (kept
        ? `Una lambda sigue en línea mientras la gente la usa, y se elimina tras ${retention} días sin visitas ni cambios.`
        : `Esta lambda está en el plan ${tier}, que la mantiene en línea y guardada aunque nadie la use.`),
    onlineFor: (duration, version) => (
      <>
        En línea desde hace {duration('un rato')}, sirviendo la versión {version}.
      </>
    ),
    offline: 'Fuera de línea. No se sirve nada hasta que despliegues una versión.',
    nothing: 'Todavía no hay nada escrito.',
    requestsToday: 'peticiones hoy',
    lastHour: (count) => `${count} en la última hora`,
    hourly: 'Peticiones por hora en el último día',
    failed: 'fallidas',
    failedTitle: (failed, rejected) =>
      `${failed} errores del servidor, ${rejected} no encontradas o rechazadas, en el último día`,
    average: 'tiempo medio de respuesta',
    noneYet: 'ninguna todavía',
    lastVisit: 'última visita',
    problems: 'Algo falló hace poco',
    openLog: 'Abrir los logs',
    latest: 'Último cambio',
    allVersions: 'Todas las versiones',
    noDescription: 'Sin descripción',
    version: (version) => `Versión ${version}`,
    notOnline: 'todavía no está en línea',
    wanted: 'Lo que se pidió',
    noVersions: 'Todavía no hay versiones.',
    storage: 'Almacenamiento',
    browse: 'Explorar',
    code: 'Código',
    codeWhy: 'El C# se compila, nunca se sirve.',
    characters: 'caracteres',
    assets: 'Recursos',
    assetsPublic: 'Públicos: el código los sirve.',
    assetsPrivate: 'El código no los sirve.',
    data: 'Datos',
    dataPublic: 'Públicos: el código sirve el workspace.',
    dataPrivate: 'Privados: solo para la lambda.',
  },

  files: {
    hint: (b) => (
      <>
        El {b('Código')} se compila y nunca se sirve. Los {b('Recursos')} (páginas, estilos, imágenes) se guardan con cada
        versión y son públicos si el código los sirve. Los {b('Datos')} son lo que la lambda escribe mientras se ejecuta;
        no forman parte de ninguna versión y solo son públicos si el código los sirve.
      </>
    ),
    edit: 'Editar esta versión',
    version: 'Versión',
    shown: (version, online, newest) => `Versión ${version}${online ? ', en línea' : newest ? ', la más nueva' : ''}`,
    optionOnline: ' (en línea)',
    readFailed: 'No se pudo leer esa versión.',
    dataFailed: 'No se pudieron leer los datos.',
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
    data: 'Datos',
    dataPublic: 'Públicos: esta versión los sirve con Workspace.',
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
  },

  versions: {
    hint: (limit) =>
      `Cada versión guarda lo que se pidió y lo que cambió, si quien la escribió lo indicó. Cuando hay más de ${limit}, se eliminan las más antiguas; la que está en línea, nunca.`,
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
    edit: 'Editar desde aquí',
    binary: 'No es texto, así que no hay líneas que comparar.',
    tooLarge: 'Es demasiado grande para compararlo línea a línea.',
  },

  deployments: {
    hint: (until) =>
      `Un despliegue sigue en línea mientras la gente lo usa${until ? ` (si nadie lo usa, hasta ${until})` : ''}. Volver a desplegar, o cualquier visita, reinicia ese plazo.`,
    takeOffline: 'Desconectar',
    readFailed: 'No se pudo leer el historial.',
    reading: 'Leyendo el historial…',
    none: 'Todavía no se ha desplegado nada.',
    noDescription: 'Sin descripción',
    deployed: (when, by) => `Desplegado ${when} (${by})`,
    duration: 'Tiempo en línea',
    online: 'en línea',
    short: {
      replaced: 'reemplazado',
      stopped: 'desconectado',
      expired: 'caducado',
      admin: 'por el operador',
      ended: 'finalizado',
    },
    putBack: (version) => `Volver a poner en línea la versión ${version}`,
    timeline: 'Lo que estuvo en línea en los últimos siete días',
    block: (version, from, to) => `Versión ${version}: ${from} – ${to ?? 'ahora'}`,
    weekAgo: 'hace una semana',
    now: 'ahora',
  },

  stats: {
    readFailed: 'No se pudieron leer las cifras.',
    range: 'Periodo',
    lastHour: 'Última hora',
    lastDay: 'Último día',
    hint: (since) =>
      `Se cuentan en memoria desde el último arranque del servidor (${since}). Si se reinicia, estas cifras empiezan de cero.`,
    reading: 'Leyendo las cifras…',
    requests: 'peticiones',
    websockets: (count) => (count === 1 ? 'y 1 conexión websocket' : `y ${count} conexiones websocket`),
    failed: 'fallidas',
    serverErrors: (count) => (count === 1 ? '1 error del servidor' : `${count} errores del servidor`),
    rejected: 'no encontradas o rechazadas',
    average: 'tiempo medio de respuesta',
    sent: (amount) => `${amount} enviados`,
    nobody: (hour) => (hour ? 'Ninguna petición en la última hora.' : 'Ninguna petición en el último día.'),
    requestsTitle: 'Peticiones',
    per: (hour) => (hour ? 'Por minuto.' : 'Cada 15 minutos.'),
    answered: 'Respondidas',
    rejectedSeries: 'No encontradas o rechazadas',
    failedSeries: 'Fallidas',
    timeTitle: 'Tiempo de respuesta',
    averagePer: (hour) => (hour ? 'Media por minuto.' : 'Media cada 15 minutos.'),
    averageSeries: 'Media',
    mostAsked: 'Rutas más pedidas',
    path: 'Ruta',
    requestsColumn: 'Peticiones',
    failedColumn: 'Fallidas',
    averageColumn: 'Media',
    since: 'Desde que arrancó el servidor.',
  },

  logs: {
    readFailed: 'No se pudieron leer los logs.',
    hint: (capturing) =>
      'Peticiones, lo que imprime la lambda y lo que sale mal, en tiempo real.' +
      (capturing ? '' : ' Esta instalación no guarda lo que imprimen las lambdas, así que solo aparecen peticiones y errores.') +
      ' Se guardan en memoria y se comparten con todas las lambdas de aquí, así que abarcan de minutos a horas y se vacían tras un reinicio. No se muestran las direcciones de los visitantes.',
    search: 'Buscar',
    searchLabel: 'Buscar en los logs',
    resume: 'Mostrar las líneas nuevas según llegan',
    pause: 'Dejar de añadir líneas mientras lees',
    paused: 'En pausa',
    live: 'En vivo',
    show: 'Mostrar',
    all: 'Todo',
    requests: 'Peticiones',
    output: 'Salida',
    problems: 'Problemas',
    reading: 'Leyendo los logs…',
    noProblems: 'No hay ningún fallo que los logs recuerden.',
    nothing: 'Nada todavía. Abre la dirección de la lambda y sus peticiones aparecerán aquí.',
    noMatch: 'No hay coincidencias.',
    identical: (count) => `${count} líneas idénticas`,
    at: (domain) => `, en ${domain}`,
    from: (country) => `, desde ${country}`,
  },

  showcase: {
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
  },

  domain: {
    readFailed: 'No se pudo leer el dominio.',
    reaching: (domain) => `Las peticiones a ${domain} ya llegan a esta lambda.`,
    saveFailed: 'No se pudo guardar el dominio.',
    removed: 'Se quitó el dominio. La lambda sigue respondiendo en su dirección de aquí.',
    removeFailed: 'No se pudo quitar el dominio.',
    hint:
      'Una lambda premium puede responder en un dominio propio (entero, desde la raíz) además de en su dirección de aquí. Apunta el dominio a este servidor, escríbelo aquí y las peticiones que le lleguen irán a la lambda.',
    loading: 'Cargando…',
    example: 'tu-dominio.com',
    open: (domain) => `Abrir ${domain}`,
    label: 'El dominio en el que responde',
    serving: (domain) => <>Ya sirve {domain}, además de su dirección de aquí.</>,
    none: 'Ninguno todavía. Puede ser un subdominio como shop.example.com o un dominio entero como example.com.',
    change: 'Cambiar',
    use: 'Usar este dominio',
    remove: 'Quitar',
    confirm: '¿Quitar el dominio?',
    keep: 'Mantenerlo',
    confirmText: (domain) => (
      <>
        Las peticiones a {domain} dejan de llegar a esta lambda al instante. Su dirección de aquí no cambia, y tampoco lo
        que diga el DNS del dominio.
      </>
    ),
    point: 'Apunta el dominio a este servidor',
    check: 'Volver a comprobar',
    records:
      'En el proveedor que gestiona el DNS del dominio, añade estos dos registros. Omite el registro AAAA si prefieres no ser accesible por IPv6.',
    type: 'Tipo',
    name: 'Nombre',
    value: 'Valor',
    pointsHere: (domain) => <>{domain} apunta aquí.</>,
    alsoElsewhere: (addresses) =>
      ` También resuelve a ${addresses}, que no es este servidor: los visitantes que lleguen por ahí no verán la lambda.`,
    elsewhere: (addresses) => `Resuelve a ${addresses}, que todavía no es este servidor.`,
    wait: 'Un cambio puede tardar en verse en todas partes: como mucho, lo que dure el TTL del registro anterior.',
    cname: 'Usar un registro CNAME en su lugar',
    cnameText: (target) => (
      <>
        Un subdominio también puede apuntar a {target} con un registro CNAME, y así sigue a este servidor si alguna vez
        cambian sus direcciones. Tiene inconvenientes:
      </>
    ),
    cnameRoot: (example) => (
      <>
        No sirve para un dominio entero ({example} en sí): el estándar no permite un CNAME junto a los registros que todo
        dominio tiene en su raíz. Algunos proveedores ofrecen un registro ALIAS, ANAME o «aplanado» que sí funciona ahí.
      </>
    ),
    cnameAlone: 'No puede haber nada más con el mismo nombre: ni un registro MX para el correo ni un TXT para verificaciones.',
    cnameLookup: 'Los resolvers DNS de los visitantes hacen una consulta más antes de llegar.',
    copy: 'Copiar',
    copyValue: (value) => `Copiar ${value}`,
  },

  code: {
    title: 'Código',
    version: (version) => `versión ${version}`,
    edited: ', editada',
    online: ', en línea',
    loadFailed: 'No se pudo cargar esa versión.',
    compiles: 'Compila.',
    notYet: 'Todavía no compila.',
    checkFailed: 'No se pudo comprobar el código.',
    saved: (version) => `Guardado como versión ${version}.`,
    isOnline: (version) => `La versión ${version} está en línea.`,
    notOnline: 'No se puso en línea. Mira abajo lo que dijo el compilador.',
    failed: 'No funcionó.',
    unchanged: 'No hay cambios desde la última vez que guardaste.',
    demo: 'Es una demo, así que todo es de solo lectura. Para cambiarla, crea tu propia lambda a partir de ella. ',
    edit: 'Edita el código a mano. Guardar crea una versión nueva sin tocar lo que está en línea; desplegar la pone en línea. ',
    files: (entry, cs) => (
      <>
        {entry} devuelve lo que se sirve, los demás archivos {cs} contienen tipos y cualquier otro archivo se sirve tal
        cual. Ctrl-S guarda; F12 va a una declaración.
      </>
    ),
    newer: (version) => ` La versión ${version} es más nueva que la que tienes abierta.`,
    check: 'Comprobar',
    save: 'Guardar',
    deploy: 'Desplegar',
    binary: (size) => `No es texto, así que no hay nada que editar. Se sirve tal cual y pesa ${size} kB.`,
    saveAndDeploy: 'Guardar y desplegar',
    saveVersion: 'Guardar una versión nueva',
    cancel: 'Cancelar',
    what: '¿Qué cambia? Es opcional y se muestra en el historial.',
    placeholder: 'Añade un formulario de contacto',
    goToDefinition: 'Ir a la definición',
  },

  tabs: {
    codeName: 'Letras, números, guiones y guiones bajos, y al final .cs',
    slashes: 'Sin barra al principio ni al final, y con menos de 120 caracteres.',
    deep: 'Como máximo seis carpetas de profundidad.',
    characters: 'Letras, números, guiones, guiones bajos y puntos, separados por barras.',
    extension: 'Necesita una extensión para servirse como lo que es.',
    exists: 'Ya hay un archivo con ese nombre.',
    remove: (name) => `¿Quitar ${name}? Se borrará su contenido.`,
    there: (name) => `${name} ya existe.`,
    entry: 'El fragmento principal: lo que devuelve es lo que se sirve',
    errors: 'tiene errores',
    removeFile: (name) => `Quitar ${name}`,
    removeTitle: 'Quitar este archivo',
    placeholder: 'Types.cs o site/index.html',
    newFile: 'Archivo nuevo',
    uploadTitle: 'Sube un archivo: una imagen, una fuente, una página',
    upload: 'Subir un archivo',
  },
};
