import type { EditorMessages } from '../../en/editor';

export const context: EditorMessages['context'] = {
  docs: {
    title: 'Documentación',
    titleSimple: 'Acerca de tu app',
    hint: 'Qué es esta app, para quién es y por qué, y por qué está hecha como está. Los agentes la escriben con cada cambio y se guarda con cada versión, así que una versión anterior vuelve con la documentación que era cierta para ella.',
    hintSimple: 'Para qué sirve tu app y por qué, tal como lo entendió el agente a partir de lo que pediste. El agente mantiene esta descripción al día con cada cambio.',
    inDraft: 'La documentación de este borrador. Pasa a ser la de tu app cuando el borrador se pone en línea.',
    pages: { product: 'Producto', decisions: 'Decisiones' },
    emptyTitle: 'Todavía no hay nada escrito',
    emptyText: (code) => (
      <>
        Los agentes escriben la documentación con sus cambios: qué es la app, para quién es y por qué en{' '}
        {code('.lambda/docs/product.md')}, y por qué está hecha como está en {code('decisions.md')}. Forma parte de la
        versión, junto al código.
      </>
    ),
    emptySimpleTitle: 'Todavía no hay nada escrito sobre tu app',
    emptySimple: 'El agente puede describir para qué sirve tu app y por qué, a partir de lo que pediste, y desde entonces mantiene la descripción al día.',
    ask: 'Pedirle al agente que la escriba',
    describe: 'Pedirle al agente que la describa',
    writePrompt: 'Escribe la documentación de esta app: qué es, para quién es y por qué, y las decisiones técnicas que hay detrás.',
    describePrompt: 'Describe para qué sirve esta app y por qué, para que yo lo lea en «Acerca de».',
    decisionsPrompt: 'Deja por escrito las decisiones técnicas detrás de esta app, y por qué se tomaron.',
    missingProduct: 'Todavía no hay página de producto',
    missingProductText: 'Qué es la app, para quién es, qué hace la gente con ella y por qué, con las palabras de quien la pidió.',
    missingDecisions: 'Todavía no hay decisiones por escrito',
    missingDecisionsText: 'Cómo está hecha la app y por qué: cómo guarda sus datos, de qué depende, qué se dejó fuera. Lo que necesita saber quien la cambie después.',
    correctText: 'El agente escribe esto a partir de lo que pediste, y lo mantiene al día con cada cambio. ¿Hay algo mal o falta algo? Díselo.',
    correct: 'Decírselo al agente',
    correctPrompt: 'Corrige la descripción de la app: ',
    placeholder: 'Explica por qué las entradas se guardan durante un año',
  },
  tests: {
    title: 'Pruebas',
    hint: 'Cómo se prueba esta app automáticamente, y los scripts y datos que usan las pruebas. Los agentes las mantienen al día y las ejecutan antes de dar un cambio por terminado. Se guardan con cada versión.',
    inDraft: 'Las pruebas de este borrador. Pasan a ser las de tu app cuando el borrador se pone en línea: ejecútalas antes contra su vista previa.',
    pages: { testing: 'Cómo se prueba' },
    emptyTitle: 'Todavía no hay pruebas',
    emptyText: (code) => (
      <>
        Cómo se prueba la app (qué tiene que seguir funcionando, cómo comprobarlo y cómo ejecutar los scripts para ello)
        lo escriben los agentes en {code('.lambda/tests/README.md')}, con los scripts y los datos de prueba al lado.
      </>
    ),
    ask: 'Pedirle al agente que escriba pruebas',
    writePrompt: 'Escribe las pruebas de esta app: qué tiene que seguir funcionando y cómo comprobarlo automáticamente, con un script para ejecutar contra su vista previa.',
    missing: 'Todavía no dice cómo se prueba',
    missingText: 'Qué tiene que seguir funcionando, cómo se comprueba cada cosa y cómo ejecutar los scripts que hay al lado.',
    placeholder: 'Comprueba que una lista llena rechaza entradas nuevas',
  },
  files: 'Archivos',
  noFiles: 'No hay archivos aparte de las páginas.',
  none: 'falta',
  missingPill: 'Todavía sin escribir',
  changedIn: (version) => `Cambió en la versión ${version}`,
  changedInDraft: 'Cambió en este borrador',
  showChanges: 'Ver qué cambió',
  hideChanges: 'Ocultar qué cambió',
  noChanges: 'No cambió nada.',
  edit: 'Editar',
  olderVersion: 'Una versión nunca cambia: una página se edita en la versión más nueva o en un borrador.',
  writeIt: 'Escribirla tú',
  askPage: 'Pedirle al agente que la escriba',
  editInCode: 'Abrir en el código',
  cancel: 'Cancelar',
  save: 'Guardar',
  write: 'Escribir',
  preview: 'Vista previa',
  writeOrPreview: 'Escribir o ver la vista previa',
  discard: 'Vas a perder los cambios de esta página. ¿Descartarlos?',
  reading: 'Leyendo…',
  readFailed: 'No se pudo leer.',
  saveFailed: 'No se pudo guardar.',
  savedDraft: 'Guardado en el borrador.',
  savedVersion: (version) => `Guardado como versión ${version}.`,
  savedOnline: (version) => `Guardado como versión ${version}, y en línea.`,
  savedNotOnline: (version) => `Guardado como versión ${version}, pero no se puso en línea.`,
  saveTitle: 'Guardar como versión nueva',
  saveText: (newest) =>
    `Una versión nunca cambia, así que esta página se guarda como la siguiente: sobre la versión ${newest}, con todo lo demás como está.`,
  clash: (version) => `Desde que empezaste se guardó la versión ${version}, que también cambió esta página. Al guardar, la reemplazas.`,
  alsoOnline: 'Ponerla también en línea',
  alsoOnlineNote: 'Solo cambia la documentación, así que los visitantes no ven nada nuevo, pero lo que está en línea sigue siendo la versión más nueva.',
  skeleton: {
    product: '# Nombre de la app\n\nQué es, en una o dos frases.\n\n## Para quién es\n\n## Qué hace la gente con ella\n\n## Funciones, y por qué están ahí\n\n## Qué no hace\n',
    decisions: '# Decisiones\n\n## Una decisión\n\nQué se decidió, por qué y qué tiene que tener en cuenta un cambio.\n',
    testing: '# Cómo se prueba\n\nCómo ejecutar las pruebas, y contra qué dirección.\n\n## Qué tiene que seguir funcionando\n\n| Comportamiento | Petición | Resultado esperado |\n|---|---|---|\n| | | |\n',
  },
};
