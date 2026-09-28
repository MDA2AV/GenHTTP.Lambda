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
    files: 'Más de un archivo',
    page: 'Servir una página',
    spa: 'Un frontend, paso a paso',
    storage: 'Los dos lugares donde viven los archivos',
    keeping: 'Guardar datos',
    sockets: 'WebSockets',
    limits: 'Lo que no te deja hacer',
    away: 'Llévate tu código',
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
      su dirección y un botón cuando hay una versión más nueva esperando para publicarse) y sus secciones. Lo que se hace
      pocas veces, como cambiar la dirección o eliminarla, está en el menú {k.b('⋯')} de esa barra.
    </>
  ),
  bits: [
    ['Resumen', () => <>Si está en línea, cuántas peticiones tuvo hoy y cuántas fallaron, el último cambio y cuánto espacio le queda.</>],
    ['Archivos', () => <>Los archivos de una versión y sus datos, es decir, lo que la lambda guarda mientras se ejecuta. Un candado o un globo indica si el público puede acceder a ellos.</>],
    ['Versiones', () => <>Qué cambió cada versión, qué se pidió y la diferencia con la anterior. Desde aquí despliegas o vuelves atrás.</>],
    ['Despliegues', () => <>Qué estuvo en línea y cuándo, y qué lo desconectó.</>],
    ['Estadísticas', () => <>Peticiones, fallos, tiempos de respuesta y las rutas más pedidas, en la última hora o el último día.</>],
    ['Logs', () => <>Sus peticiones, lo que imprimió y el stack trace de cualquier error, en tiempo real.</>],
    [
      'Código',
      (k) => (
        <>
          Para escribirlo a mano. {k.b('Comprobar')} compila, {k.b('Guardar')} crea una versión y {k.b('Desplegar')} la
          pone en línea. {k.code('Ctrl-S')} guarda; {k.code('F12')} va a una declaración.
        </>
      ),
    ],
  ],
  sections: (k) => (
    <>
      Todas las secciones funcionan igual: su título, un {k.b('ⓘ')} que la explica, sus acciones a la derecha y, si
      tiene más de una vista, una fila de pestañas debajo. En el código, las pestañas son sus archivos.
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
    change: 'Guarda las entradas en el workspace para que sobrevivan a un reinicio',
  },
  why2: (k) => (
    <>
      Los agentes pasan esos mismos dos campos a {k.code('write_code')}. En {k.b('Código')}, al guardar se te pide el
      cambio. Los dos son opcionales; una especificación larga se recorta a 4000 caracteres y un cambio a 500, en vez de
      rechazarse.
    </>
  ),

  files: (k) => (
    <>
      Los tipos no tienen por qué estar debajo del código que los usa. En {k.b('Código')}, haz clic en {k.b('+')} junto a
      los archivos: el archivo nuevo se compila junto al fragmento, en el mismo namespace, así que no hay que importar
      nada para usarlo. Un nombre sin extensión se toma como C#.
    </>
  ),

  page: 'Hay tres formas, y la que te conviene depende de dónde vive la página.',
  inlineTitle: 'Una página escrita en el código',
  inline: 'Sirve para algo pequeño. La página forma parte del fragmento.',
  folderTitle: 'Una carpeta de archivos reales',
  folder:
    'Lo que necesitas para cualquier cosa con hoja de estilos y script. Los archivos se añaden igual que un archivo C# y se sirven tal como están escritos. Nada los compila.',
  workspaceTitle: 'Desde el workspace',
  workspace: 'Cuando la página se sube en vez de escribirse, y quieres poder cambiarla sin volver a desplegar.',

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
      La sección {k.b('Archivos')} muestra los dos (los archivos de una versión, y el workspace como {k.b('Datos')}) e
      indica a cuáles puede acceder el público. Los archivos del código se cambian en {k.b('Código')}; los datos se
      pueden subir y eliminar en {k.b('Archivos')}. Pero no son lo mismo, y la diferencia está en{' '}
      {k.em('cuándo cambia cada uno')}.
    </>
  ),
  savedWithCode: 'Se guarda con tu código',
  workspaceColumn: 'Workspace',
  table: [
    ['qué contiene', 'todos los archivos de tu lambda, incluido el C#', 'lo que se haya escrito o subido'],
    ['cuándo cambia', 'cuando haces clic en Guardar o Desplegar', 'en cuanto se escribe algo en él'],
    ['un despliegue', 'lo reemplaza todo', 'nunca lo toca'],
    ['volver a una versión', 'trae de vuelta los archivos anteriores', 'no le afecta'],
    ['clonar la lambda', 'se copia', 'no se copia'],
  ],
  reachedAs: 'cómo se accede desde el código',
  storageAside:
    'No pueden ser un solo directorio. Si lo fueran, un despliegue borraría todo lo que tu lambda haya escrito desde entonces, o nunca se podría quitar nada de lo que publica. Un juego con ranking quiere lo segundo; la página que sirve, lo primero.',

  keeping: (k) => (
    <>
      {k.code('Workspace')} es un directorio privado que tu lambda puede leer y escribir. Es el lugar para todo lo que
      tenga que sobrevivir a una petición o a un despliegue.
    </>
  ),
  keeping2: (k) => (
    <>
      También tienes {k.code('ReadBytes')}, {k.code('WriteBytes')}, {k.code('Delete')}, {k.code('List')},{' '}
      {k.code('CreateFolder')} y {k.code('Tree')}/{k.code('Files')}/{k.code('App')} para servirlo. No se puede acceder a
      nada más del sistema de archivos.
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
    'Tu código se ejecuta en un servidor compartido, así que parte de C# se rechaza antes de compilar: iniciar procesos, abrir tus propios sockets, cargar ensamblados, acceder al sistema de archivos fuera de tu workspace y usar reflexión para saltarte cualquiera de esas reglas.',
  limits2:
    'Todo lo demás está disponible, incluida toda la API de módulos de GenHTTP. Si algo se rechaza, te decimos en qué línea y por qué, no solo que falló.',

  away: (k) => (
    <>
      En el editor, {k.b('Descargar como proyecto .NET')} te da todo listo para llevártelo: una solución que puedes
      abrir, ejecutar con {k.code('dotnet run')} y conservar. Tiene una sola referencia a un paquete y ningún rastro de
      esta plataforma.
    </>
  ),
  away2: (k) => (
    <>
      Tu fragmento pasa a ser el cuerpo de {k.code('Program.cs')}, dentro de un host que sirve lo que devuelve. Tus otros
      archivos llegan exactamente como los escribiste. {k.code('Workspace')} y {k.code('Assets')} se convierten en dos
      carpetas junto al código, con los mismos métodos, así que no tienes que cambiar nada de tu código.
    </>
  ),
  awayAside:
    'Conviene saberlo antes de crear nada aquí: lo que escribes es tuyo y te lo llevas entero. Ejecutarlo en esta máquina no te ata a esta máquina.',

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
      mismo en el centro de control.
    </>
  ),
  more: 'Más sobre esto →',
  make: 'Crea una lambda',
};
