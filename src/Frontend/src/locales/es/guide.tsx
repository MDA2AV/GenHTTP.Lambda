import type { Messages } from '../en';

export const guide: Messages['guide'] = {
  title: 'Cómo funciona',
  intro:
    'Escribes un fragmento de C#. Lo que devuelva queda alojado en una dirección pública, con HTTPS, en unos segundos. Aquí tienes todo, en el orden en que te lo vas a encontrar.',
  contents: 'Contenido',

  parts: {
    what: 'Qué es una lambda',
    first: 'Tu primera lambda',
    editor: 'El centro de control',
    why: 'Explicar el porqué',
    written: 'Documentación y pruebas',
    features: 'Cambiarla sin riesgo',
    files: 'Más de un archivo',
    page: 'Servir una página',
    spa: 'Un frontend, paso a paso',
    storage: 'Los dos lugares donde viven los archivos',
    database: 'Guardar registros',
    keeping: 'Guardar archivos',
    secrets: 'Claves y contraseñas',
    sockets: 'WebSockets',
    limits: 'Lo que no te deja hacer',
    away: 'Llévate tu código',
    open: 'Publicar el código',
    agents: 'Que lo haga un agente',
  },

  what: [
    (k) => (
      <>
        Una lambda es un fragmento de código que devuelve un handler de GenHTTP. La plataforma lo compila, lo carga y
        monta lo que devuelve en tu propia dirección. No hay proyecto, ni archivo de build, ni sentencias{' '}
        {k.code('using')}. Todos los módulos de GenHTTP ya vienen importados.
      </>
    ),
    (k) => (
      <>
        Eso ya es una lambda completa. Desplegada en {k.code('/lambda/your-key/')}, responde a cada petición con la
        palabra «hello».
      </>
    ),
  ],
  whatAside: (k) => (
    <>
      El fragmento son {k.em('sentencias')}, no una clase. Lo último que hace es devolver algo que pueda atender
      peticiones: un handler o un builder que cree uno.
    </>
  ),

  first: [
    (k) => (
      <>
        Haz clic en {k.b('Crear mi lambda')}. Recibes una dirección pública y una clave de edición. La clave es la única
        forma de volver a entrar, así que guárdala. Nadie puede recuperarla por ti.
      </>
    ),
    () => (
      <>
        Llegas a su centro de control, con un pequeño servicio REST ya escrito como primera versión. Es solo un punto de
        partida.
      </>
    ),
    (k) => (
      <>
        Pásale la clave de edición a un agente y dile qué construir: escribe versiones nuevas a través de{' '}
        {k.link('/#agents', 'MCP')}. O abre {k.b('Código')} y escríbelo tú: {k.b('Comprobar')} compila sin guardar nada
        y te dice qué opina el compilador, con archivo y línea.
      </>
    ),
    (k) => (
      <>
        Haz clic en {k.b('Desplegar')}. Ya está en línea. Antes de eso no se puede acceder a nada, y cada vez que vuelves
        a desplegar alargas el tiempo que sigue en línea.
      </>
    ),
  ],

  editor: (k) => (
    <>
      El enlace de edición abre un centro de control, no un cuadro de texto: aquí casi todo el código lo escriben
      agentes, así que lo primero que ves es cómo está tu lambda. La barra lateral muestra la lambda (si está en línea,
      su dirección y un botón cuando hay una versión más nueva esperando para ponerse en línea) y sus secciones. Lo
      que se hace pocas veces, como cambiar la dirección o eliminarla, está en el menú {k.b('⋯')} de esa barra.
    </>
  ),
  bits: [
    ['Resumen', () => <>Qué es la app, si está en línea, cuántas peticiones tuvo hoy y cuántas fallaron, el último cambio y cuánto espacio le queda.</>],
    ['Documentación', () => <>Qué es la app, para quién es y por qué, y por qué está hecha como está: la escriben los agentes y se guarda con cada versión.</>],
    [
      'Cambiar',
      (k) => (
        <>
          Di qué debería ser distinto y el agente de este servidor lo hace mientras miras. Trabaja en un borrador, lo
          prueba ahí y lo fusiona en la siguiente versión cuando funciona. Desactiva{' '}
          {k.b('Ponerlo en línea al terminar')} para probar tú el borrador antes.
          Solo trabaja en tu app: si lo que pides no tiene que ver con ella o busca hacer daño, lo rechaza y te dice por qué.
        </>
      ),
    ],
    ['Borradores', () => <>Cambios en los que se trabaja al lado de la lambda: cada uno se prueba en su propia dirección y se fusiona en la siguiente versión cuando está bien. Al abrirlo, un borrador tiene su propio código, sus datos y sus logs.</>],
    ['Archivos', () => <>Los archivos de una versión: su código y sus recursos, el programa en sí. Un candado o un globo indica si el público puede acceder a ellos.</>],
    ['Datos', () => <>Lo que la lambda guarda mientras se ejecuta, compartido por todas las versiones: la base de datos, el workspace y los secretos, cada uno en su pestaña. Mira las tablas y los archivos, sube archivos, define secretos o activa y desactiva un tipo. La vista simple lo muestra en cuanto la app guarda algo.</>],
    ['Versiones', () => <>Qué cambió cada versión, qué se pidió y la diferencia con la anterior. Desde aquí despliegas o vuelves atrás, o empiezas un borrador a partir de cualquiera de ellas.</>],
    ['Despliegues', () => <>Qué estuvo en línea y cuándo, y qué lo desconectó.</>],
    ['Estadísticas', () => <>Peticiones, fallos, tiempos de respuesta y las rutas más pedidas, en la última hora o el último día.</>],
    ['Logs', () => <>Sus peticiones, lo que imprimió y el stack trace de cualquier error, en tiempo real.</>],
    [
      'Código',
      (k) => (
        <>
          Para escribirlo a mano. {k.b('Comprobar')} compila, {k.b('Guardar')} crea una versión y {k.b('Desplegar')} la
          pone en línea. En un borrador, {k.b('Guardar')} lo guarda en el borrador y {k.b('Desplegar la vista previa')}{' '}
          lo pone en línea en la dirección del borrador. {k.code('Ctrl-S')} guarda; {k.code('F12')} va a una declaración.
        </>
      ),
    ],
    ['Pruebas', () => <>Cómo se prueba la app automáticamente, con los scripts y los datos de prueba necesarios. Solo en la vista completa.</>],
  ],
  sections: (k) => (
    <>
      Todas las secciones funcionan igual: su título, un {k.b('ⓘ')} que la explica, sus acciones a la derecha y, si
      tiene más de una vista, una fila de pestañas debajo. En el código, las pestañas son sus archivos. La vista
      completa agrupa las secciones: cómo la encuentra la gente, dónde se hace un cambio, el programa y sus datos, y
      cómo se ejecuta.
    </>
  ),
  editorAside:
    'El tráfico y los logs se guardan en memoria, para mirarlos, no para conservarlos: si el servidor se reinicia, empiezan de cero. Las versiones y el historial de despliegues sí se guardan.',

  why: (k) => (
    <>
      Una versión es el código y, si quieres, dos notas sobre él: {k.b('la especificación')}, qué quiere el usuario y
      por qué, con sus palabras si se puede, y {k.b('el cambio')}, una línea sobre lo que hace la versión. Se muestran
      junto al diff en el historial de versiones, así que el {k.em('porqué')} se queda junto al {k.em('qué')}: para ti y
      para el próximo agente que lea el historial antes de tocar nada.
    </>
  ),
  whySample: {
    specification: 'Un libro de visitas que la gente pueda firmar; las entradas tienen que sobrevivir a un reinicio',
    change: 'Guarda las entradas en la base de datos para que sobrevivan a un reinicio',
  },
  why2: (k) => (
    <>
      Los agentes pasan esos mismos dos campos a {k.code('write_code')}. En {k.b('Código')}, al guardar se te pide el
      cambio. Los dos son opcionales; una especificación larga se recorta a 4000 caracteres y un cambio a 500, en vez de
      rechazarse. Un borrador tiene sus propios dos campos, y la versión en la que se fusiona se queda con ellos.
    </>
  ),

  written: (k) => (
    <>
      Cada versión guarda, junto a su programa, lo que está escrito sobre ella: su {k.b('documentación')} (qué es la
      app, para quién es y por qué, y por qué está hecha como está) y sus {k.b('pruebas')}: cómo comprobar
      automáticamente que funciona, con los scripts y los datos de prueba necesarios. Los agentes las escriben con una
      lambda nueva y las mantienen al día con cada cambio. El próximo agente que cambie la lambda las lee primero, así
      que sabe para qué sirve la app y qué tiene que seguir funcionando, algo que el código por sí solo no dice.
    </>
  ),
  writtenFiles: [
    ['.lambda/docs/product.md', 'qué es la app, para quién es, qué hace la gente con ella y por qué'],
    ['.lambda/docs/decisions.md', 'las decisiones técnicas, y por qué se tomaron'],
    ['.lambda/tests/README.md', 'cómo se prueba la app automáticamente, y cómo ejecutar las pruebas'],
    ['.lambda/tests/…', 'los scripts y los datos de prueba que usan las pruebas'],
  ],
  written2: (k) => (
    <>
      Son archivos de la versión como cualquier otro, en la carpeta {k.code('.lambda')}: el historial muestra qué
      cambió en ellos una versión, volver atrás trae de vuelta la documentación que era cierta para esa versión, y un
      borrador tiene su propia copia, que se pone en línea con él. Nunca se compilan ni se sirven, y cuentan para el
      límite de los recursos de una versión.
    </>
  ),
  written3: (k) => (
    <>
      En el centro de control, {k.b('Documentación')} muestra las páginas para leer, y {k.b('Pruebas')} cómo se
      prueba la app y los archivos que la acompañan; la versión se elige igual que para sus archivos. Ahí también se
      puede editar una página, lo que guarda la siguiente versión. La vista sencilla llama a la documentación{' '}
      {k.b('Acerca de')} y solo muestra para qué sirve la app: para corregirla, díselo al agente.
    </>
  ),
  writtenAside:
    'Se escriben en el idioma que usas con el agente, para quien cambie la app después, sea una persona o un agente. No son una copia del código: dicen para qué sirve la app, y por qué.',

  features: (k) => (
    <>
      Una versión nunca cambia una vez guardada, y eso es lo que hace que valga la pena conservarlas todas: cualquiera de
      ellas se puede comparar y volver a poner en línea exactamente como estaba. Para cambiar una lambda que la gente
      usa, empieza un {k.b('borrador')}.
    </>
  ),
  featureSteps: [
    (k) => (
      <>
        Empiézalo en {k.b('Borradores')} o desde cualquier versión. Es una copia del código, los recursos, la
        documentación y las pruebas de esa versión, y de los datos de la lambda.
      </>
    ),
    (k) => (
      <>
        Cámbialo tantas veces como haga falta, en {k.b('Código')} o pidiéndoselo al agente.{' '}
        {k.b('Desplegar la vista previa')} lo pone en línea en su propia dirección, {k.code('/features/…/')}, con su
        propia copia de los datos. Los visitantes de la lambda no ven nada de esto, y nada de lo que escribe llega a los
        datos de la lambda.
      </>
    ),
    (k) => (
      <>
        Cuando esté bien, {k.b('Fusionar')} lo convierte en la siguiente versión, con sus notas, y lo pone en línea
        enseguida si quieres. El borrador desaparece con ello, junto con su vista previa y su copia de los datos.
      </>
    ),
  ],
  featureSample: 'Ranking',
  featuresAside: () => (
    <>
      Se puede trabajar en varios borradores a la vez. Solo se puede fusionar uno basado en la versión más nueva, para
      que una fusión nunca deshaga una versión guardada después de que empezara el borrador. Si antes se fusionó otro,
      lleva sus cambios a este (o pídeselo al agente) y después basa el borrador en la versión más nueva. Nada se
      fusiona solo; es a propósito.
    </>
  ),

  files: (k) => (
    <>
      Los tipos no tienen por qué estar debajo del código que los usa. En {k.b('Código')}, haz clic en {k.b('+')} junto a
      los archivos: el archivo nuevo se compila junto al fragmento, en el mismo namespace, así que no hay que importar
      nada para usarlo. Un nombre sin extensión se toma como C#.
    </>
  ),

  page: 'Hay dos formas de servir una página, y una más para lo que la gente sube junto a ella.',
  inlineTitle: 'Una página escrita en el código',
  inline: 'Sirve para algo pequeño. La página forma parte del fragmento.',
  folderTitle: 'Una carpeta de archivos reales',
  folder:
    'Lo que necesitas para cualquier cosa con hoja de estilos y script. Los archivos se añaden igual que un archivo C# y se sirven tal como están escritos. Nada los compila.',
  workspaceTitle: 'Archivos subidos, desde los datos',
  workspace:
    'Para lo que la gente sube o la lambda crea (fotos, documentos), servido junto a la app. No para las páginas de la propia app: esas van en una carpeta de archivos, donde se versionan junto con el código que las necesita.',

  spa: (k) => (
    <>
      La segunda forma, completa. Todas las demos sirven su página así, desde una carpeta llamada {k.code('web')}: abre{' '}
      {k.link('/editor/demo-crud', 'demo-crud')} para ver una. Las demos son de solo lectura; su clave de edición es su
      nombre.
    </>
  ),
  spaSteps: [
    (k) => (
      <>
        En {k.b('Código')}, haz clic en {k.b('+')} junto a los archivos y escribe {k.code('site/index.html')}. Un nombre
        con barra pone el archivo en una carpeta; un nombre con extensión se toma como el tipo de archivo que indica.
      </>
    ),
    (k) => (
      <>
        Añade {k.code('site/app.css')} y {k.code('site/app.js')} de la misma forma. Tu página los enlaza por su nombre,
        como en {k.code('href="app.css"')}, porque la carpeta es la raíz de lo que se sirve, no parte de la dirección.
      </>
    ),
    (k) => (
      <>
        Para todo lo que no sea texto, como una imagen o una fuente, abre un archivo de {k.code('site')} y usa el botón de
        subir que está junto a los archivos: va a parar a la misma carpeta. Un PNG no se puede escribir en un editor de
        texto, así que esa es la forma de meterlo.
      </>
    ),
    (k) => <>En {k.code('lambda.cs')}, sirve la carpeta:</>,
    (k) => (
      <>
        Haz clic en {k.b('Desplegar')}. {k.code('site/index.html')} responde en {k.code('/')}, {k.code('site/app.css')}{' '}
        en {k.code('/app.css')}, y cualquier dirección que no coincida con un archivo se responde con la página. Así, un
        frontend con su propio enrutamiento sigue funcionando cuando alguien recarga en un enlace profundo.
      </>
    ),
    () => <>Añade una API al lado y la página tendrá con quién hablar:</>,
  ],

  storage: (k) => (
    <>
      Una lambda guarda archivos en dos lugares, y el editor los muestra por separado: {k.b('Archivos')} contiene los
      archivos de una versión (el programa) y {k.b('Datos')} contiene el workspace (lo que el programa guarda). La
      diferencia está en {k.em('de quién son')}. Los archivos de una versión pertenecen a esa versión; los datos
      pertenecen a la lambda, y todas las versiones los comparten.
    </>
  ),
  savedWithCode: 'En una versión',
  workspaceColumn: 'En los datos',
  table: [
    ['qué contiene', 'el código y los recursos: el programa, frontend incluido, y su documentación y sus pruebas', 'lo que escribe la lambda o sube alguien'],
    ['cuándo cambia', 'nunca: un cambio es una versión nueva', 'en cuanto se escribe algo en ellos'],
    ['un despliegue', 'pone en línea exactamente estos archivos', 'nunca los toca'],
    ['volver atrás', 'trae de vuelta los archivos anteriores', 'no les afecta: todas las versiones los comparten'],
    ['un borrador', 'empieza como una copia de ellos', 'trabaja con una copia de ellos'],
    ['cuándo desaparece', 'con las versiones antiguas, al pasar el límite', 'con la lambda, o cuando desactivas el workspace'],
  ],
  reachedAs: 'cómo se accede desde el código',
  storageAside:
    'No pueden estar en un solo lugar. Si lo estuvieran, un despliegue borraría todo lo que tu lambda haya escrito desde entonces, o nunca se podría quitar nada de lo que publica. Un juego con ranking quiere lo segundo; la página que sirve, lo primero. Por eso la página va en la versión y el ranking, en los datos.',

  database: (k) => (
    <>
      Los registros (entradas, cuentas, pedidos, votos) van en la {k.b('base de datos')}: una base de datos SQLite
      propia de la lambda, que se activa en {k.b('Datos')}. El código abre una conexión con{' '}
      {k.code('Database.GetConnection()')} y lee y escribe los datos mediante{' '}
      {k.link('https://learn.microsoft.com/ef/core/', 'Entity Framework Core')}, con un contexto propio que mapea las
      tablas:
    </>
  ),
  database2: (k) => (
    <>
      Sus tablas las crean las {k.b('migraciones')}: archivos SQL que van con la versión en {k.code('migrations/')} y
      que {k.link('https://evolve-db.netlify.app/', 'Evolve')} aplica en orden cuando arranca la lambda, cada uno una
      sola vez, así que una versión nueva solo ejecuta lo que es nuevo. Nunca cambies una migración que ya se aplicó;
      un cambio en una tabla es el siguiente archivo.
    </>
  ),
  database3: (k) => (
    <>
      Como todos los datos, la base de datos la comparten todas las versiones, desplegar o volver atrás no la toca, y un
      borrador trabaja con una copia. En {k.b('Datos')} ves sus tablas y sus filas, que la vista simple llama
      registros. {k.b('Descargar como proyecto .NET')} la incluye como un archivo SQLite normal.
    </>
  ),
  databaseAside: (k) => (
    <>
      Crea un contexto donde lo necesites y libéralo después, y úsalo de forma síncrona: {k.code('ToList')} y{' '}
      {k.code('SaveChanges')}, no {k.code('ToListAsync')} y {k.code('SaveChangesAsync')}. Las tablas las crean las
      migraciones, nunca Entity Framework. La demo {k.link('/editor/demo-crud', 'demo-crud')} hace todo esto.
    </>
  ),

  keeping: (k) => (
    <>
      {k.code('Workspace')} es un directorio privado que tu lambda puede leer y escribir: el lugar para archivos, como
      las fotos que sube alguien, un documento que genera o un modelo que carga. Los registros van en la base de datos,
      y lo que se sabe de un archivo (quién lo subió, cuándo) también es un registro.
    </>
  ),
  keeping2: (k) => (
    <>
      También tienes {k.code('ReadBytes')}, {k.code('WriteBytes')}, {k.code('Delete')}, {k.code('List')},{' '}
      {k.code('CreateFolder')} y {k.code('Tree')}/{k.code('Files')}/{k.code('App')} para servirlo. No se puede acceder a
      nada más del sistema de archivos.
    </>
  ),

  secrets: (k) => (
    <>
      Una clave de API, una contraseña o un token va en los {k.b('secretos')}, no en el código, donde lo tendrían cada
      versión, cada descarga y cualquiera que lea el historial. El código lee un secreto por su nombre:
    </>
  ),
  secrets2: (k) => (
    <>
      Activa los secretos en {k.b('Datos')} y define ahí el valor. Una vez guardado, no se vuelve a mostrar, ni a ti ni
      a un agente: solo puedes reemplazarlo. La lista dice qué nombres lee el código sin que haya todavía un valor, y el
      resumen los pide. {k.code('Secret.Exists')} dice si un secreto está definido, para el código que funciona sin
      él. Como todos los datos, los secretos los comparten todas las versiones, y un borrador trabaja con una copia.
    </>
  ),
  secretsAside: (k) => (
    <>
      Se guardan cifrados, con una clave que no está en la base de datos. En un proyecto descargado,{' '}
      {k.code('Secret.Read("NAME")')} lee la variable de entorno {k.code('NAME')}: los valores se quedan aquí.
    </>
  ),

  sockets: (k) => (
    <>
      Funcionan, y no son un añadido de última hora. La demo {k.link('/editor/demo-game', 'demo-game')} empareja
      jugadores y ejecuta cada partida en el servidor. La forma más simple son tres callbacks:
    </>
  ),
  socketsAside: (k) => (
    <>
      Hay un detalle que sorprende a todo el mundo: un navegador no puede poner cabeceras en el handshake de un websocket.
      Pasa lo que necesite el handler en la query, donde lo lee desde {k.code('connection.Request.Header.Query')}, o
      envía los secretos en el primer mensaje.
    </>
  ),

  limits:
    'Tu código se ejecuta en un servidor compartido, así que parte de C# se rechaza antes de compilar: iniciar procesos, abrir tus propios sockets, cargar ensamblados, acceder al sistema de archivos fuera de tu workspace y usar reflexión para saltarte cualquiera de esas reglas. Lo mismo ocurre con esperar una tarea con .Result o .Wait() en lugar de usar await: las peticiones se ejecutan en un hilo por núcleo, y la tarea tendría que terminar en el mismo hilo que la está esperando.',
  limits2:
    'Todo lo demás está disponible, incluida toda la API de módulos de GenHTTP. Si algo se rechaza, te decimos en qué línea y por qué, no solo que falló.',

  away: (k) => (
    <>
      En el editor, {k.b('Descargar como proyecto .NET')} te da todo listo para llevártelo: una solución que puedes
      abrir, ejecutar con {k.code('dotnet run')} y conservar. Solo necesita el paquete de GenHTTP e incluye un{' '}
      {k.code('Dockerfile')} para compilarla y ejecutarla como contenedor.
    </>
  ),
  away2: (k) => (
    <>
      Tu fragmento pasa a ser {k.code('Project.cs')}, y {k.code('Program.cs')} sirve lo que devuelve. Tus otros
      archivos llegan exactamente como los escribiste. {k.code('Workspace')} y {k.code('Assets')} se convierten en dos
      carpetas junto al programa, con los mismos métodos, aparte en una carpeta {k.code('Platform')}, así que no tienes
      que cambiar nada de tu código.
      {' '}{k.code('Secret')} lee allí las variables de entorno con el mismo nombre; los valores se quedan aquí. La
      documentación y las pruebas también se van contigo, en {k.code('docs')} y {k.code('tests')}.
      {' '}{k.code('Database')} abre {k.code('database/database.db')}, que la descarga incluye con los registros que
      guardó tu app.
    </>
  ),
  awayAside:
    'Conviene saberlo antes de crear nada aquí: lo que escribes es tuyo y te lo llevas entero. Ejecutarlo en esta máquina no te ata a esta máquina.',

  open: (k) => (
    <>
      Si lo que creaste puede ayudar a otras personas, publica su código: abre {k.b('Código abierto')} en el centro de
      control, elige una licencia (MIT, salvo que quieras otra) y actívalo. Su código tendrá su propia página entre las{' '}
      {k.link('/source', 'apps de código abierto')}, donde cualquiera puede leerlo, darle una estrella y descargar
      cualquier versión como el mismo proyecto que te da {k.b('Descargar como proyecto .NET')}, con la licencia al lado.
    </>
  ),
  open2: () => (
    <>
      Se publican todas las versiones, también las anteriores, con su documentación, sus pruebas y el cambio que hizo
      cada una. Lo que conserva la app nunca se publica (sus registros, los archivos que guardó, los valores de sus
      claves y contraseñas), como tampoco lo que pediste con tus propias palabras ni quién usa la app. Si lo
      desactivas, la página desaparece; sus estrellas se conservan para cuando vuelvas a publicarlo.
    </>
  ),
  openAside:
    'Todo lo que hay en el código se hace público, incluidas las versiones anteriores. Una clave o una contraseña va en Claves y contraseñas, dentro de Datos, nunca en el código, esté publicado o no.',

  agents: (k) => (
    <>
      Hay un endpoint MCP en {k.code('/mcp')}. Conecta un agente y podrá hacer todo lo que hace el editor: leer la guía,
      leer una demo completa, escribir archivos, compilarlos y desplegar. Por debajo es la misma API.
    </>
  ),
  agents2: (k) => (
    <>
      El agente explica el porqué sobre la marcha ({k.code('write_code')} recibe la especificación y el cambio) y puede
      revisar lo que desplegó: {k.code('read_logs')} devuelve las peticiones recientes de la lambda, lo que imprimió y el
      stack trace de cualquier excepción. Así un agente comprueba que su código funciona en vez de suponerlo. Tú ves lo
      mismo en el centro de control. Escribe la documentación y las pruebas a medida que trabaja, las lee antes de
      cambiar nada y ejecuta las pruebas contra la dirección de un borrador antes de ponerlo en línea. A una página
      pensada para que la encuentren le pone un título, una descripción, un icono y una vista previa para cuando
      alguien comparte su enlace.
    </>
  ),
  more: 'Más sobre esto →',
  make: 'Crea una lambda',
};
