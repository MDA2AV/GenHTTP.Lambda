import type { Messages } from '../en';

export const guide: Messages['guide'] = {
  title: 'Cómo funciona',
  intro:
    'Usted escribe un fragmento de C#. Lo que devuelva queda alojado en una dirección pública, mediante HTTPS, en pocos segundos. Esta página describe toda la plataforma en el orden en que la irá conociendo.',
  contents: 'Contenido',

  parts: {
    what: '¿Qué es un lambda?',
    first: 'Su primer lambda',
    editor: 'El centro de control',
    why: 'Documentar los cambios',
    files: 'Varios archivos',
    page: 'Servir una página',
    spa: 'Un frontend, paso a paso',
    storage: 'Los dos lugares donde se guardan los archivos',
    keeping: 'Conservar datos',
    sockets: 'WebSockets',
    limits: 'Restricciones',
    away: 'Exportar el código',
    agents: 'Trabajar con un agente',
  },

  what: [
    (k) => (
      <>
        Un lambda es un fragmento de código que devuelve un handler de GenHTTP. La plataforma lo compila, lo carga y
        publica el resultado en su propia dirección. No se necesita proyecto, archivo de compilación ni instrucción{' '}
        {k.code('using')}: todos los módulos de GenHTTP ya están importados.
      </>
    ),
    (k) => (
      <>
        Esto ya es un lambda completo. Desplegado en {k.code('/lambda/your-key/')}, responde a cada solicitud con la
        palabra «hello».
      </>
    ),
  ],
  whatAside: (k) => (
    <>
      El fragmento está formado por {k.em('instrucciones')}, no por una clase. Lo último que hace es devolver algo capaz de
      atender solicitudes: un handler o un builder de un handler.
    </>
  ),

  first: [
    (k) => (
      <>
        Pulse {k.b('Crear lambda')}. Recibirá una dirección pública y una clave de edición. La clave es el único acceso y
        nadie puede recuperarla por usted, así que consérvela.
      </>
    ),
    () => (
      <>
        Accederá a su centro de control, con un pequeño servicio REST ya escrito como primera versión. Se trata solo de un
        punto de partida.
      </>
    ),
    (k) => (
      <>
        Entregue la clave de edición a un agente y describa lo que debe crear: escribirá nuevas versiones mediante{' '}
        {k.link('/#agents', 'MCP')}. También puede abrir {k.b('Código')} y escribirlo usted mismo: {k.b('Comprobar')}{' '}
        compila sin guardar nada y muestra los mensajes del compilador, con archivo y línea.
      </>
    ),
    (k) => (
      <>
        Pulse {k.b('Desplegar')}. Ahora está en línea; antes no es accesible. Cada nuevo despliegue prolonga el tiempo que
        permanece en línea.
      </>
    ),
  ],

  editor: (k) => (
    <>
      El enlace de edición abre un centro de control en lugar de un simple editor de texto: la mayor parte del código la
      escriben agentes, por lo que lo primero que se muestra es el estado de su lambda. La barra lateral indica si está en
      línea, su dirección, un botón cuando hay una versión más reciente pendiente de desplegar y sus secciones. Las
      acciones poco frecuentes, como cambiar la dirección o eliminarlo, se encuentran en el menú {k.b('⋯')}.
    </>
  ),
  bits: [
    ['Resumen', () => <>Si está en línea, cuántas solicitudes ha recibido hoy y cuántas han fallado, el último cambio y el espacio disponible.</>],
    ['Archivos', () => <>Los archivos de una versión y sus datos, es decir, lo que el lambda guarda mientras se ejecuta. Un candado o un globo indica si son accesibles públicamente.</>],
    ['Versiones', () => <>Qué cambió cada versión, qué se solicitó y la diferencia con la anterior. Desde aquí se despliega o se restaura.</>],
    ['Despliegues', () => <>Qué estuvo en línea, cuándo y qué lo desconectó.</>],
    ['Estadísticas', () => <>Solicitudes, errores, tiempos de respuesta y rutas más solicitadas, en la última hora o las últimas 24 horas.</>],
    ['Registros', () => <>Sus solicitudes, lo que imprime y la traza de pila de cualquier error, en tiempo real.</>],
    [
      'Código',
      (k) => (
        <>
          Edición manual. {k.b('Comprobar')} compila, {k.b('Guardar')} crea una versión y {k.b('Desplegar')} la publica.{' '}
          {k.code('Ctrl+S')} guarda; {k.code('F12')} va a una declaración.
        </>
      ),
    ],
  ],
  sections: (k) => (
    <>
      Todas las secciones funcionan igual: su título, un {k.b('ⓘ')} que la explica, sus acciones a la derecha y, cuando
      hay varias vistas, una fila de selectores debajo. En el código, esos selectores son los archivos.
    </>
  ),
  editorAside:
    'El tráfico y el registro se mantienen en memoria, para supervisar y no para archivar: un reinicio del servidor los pone a cero. Las versiones y el historial de despliegues se guardan de forma permanente.',

  why: (k) => (
    <>
      Una versión es el código y, opcionalmente, dos notas: {k.b('la especificación')}, es decir, lo que desea el usuario
      y por qué, a ser posible con sus propias palabras, y {k.b('el cambio')}, una línea sobre lo que hace la versión. Se
      muestran junto al diff en el historial de versiones, de modo que el {k.em('porqué')} se conserva junto al{' '}
      {k.em('qué')}, tanto para usted como para el próximo agente que consulte el historial antes de hacer cambios.
    </>
  ),
  whySample: {
    specification: 'Un libro de visitas que la gente pueda firmar; las entradas deben conservarse tras un reinicio',
    change: 'Guarda las entradas en el workspace para que se conserven tras un reinicio',
  },
  why2: (k) => (
    <>
      Los agentes envían los mismos dos campos a {k.code('write_code')}. En {k.b('Código')}, al guardar se solicita el
      cambio. Ambos son opcionales; una especificación extensa se recorta a 4000 caracteres y un cambio a 500, en lugar de
      rechazarse.
    </>
  ),

  files: (k) => (
    <>
      Los tipos no tienen por qué estar debajo del código que los utiliza. En {k.b('Código')}, pulse {k.b('+')} junto a
      los archivos: el nuevo archivo se compila junto al fragmento, en el mismo espacio de nombres, por lo que no hace falta
      importar nada. Un nombre sin extensión se considera C#.
    </>
  ),

  page: 'Existen tres formas; la adecuada depende de dónde se encuentre la página.',
  inlineTitle: 'Una página escrita en el código',
  inline: 'Adecuada para algo pequeño. La página forma parte del fragmento.',
  folderTitle: 'Una carpeta de archivos',
  folder:
    'Lo indicado para cualquier cosa con hoja de estilos y script. Los archivos se añaden igual que un archivo C# y se sirven tal como están escritos, sin compilarse.',
  workspaceTitle: 'Desde el workspace',
  workspace: 'Cuando la página se sube en lugar de escribirse y debe poder cambiar sin un nuevo despliegue.',

  spa: (k) => (
    <>
      La segunda opción, en detalle. Todas las demos sirven su página de este modo desde una carpeta llamada{' '}
      {k.code('web')}; abra {k.link('/editor/demo-crud', 'demo-crud')} para ver un ejemplo. Las demos son de solo lectura;
      su clave de edición coincide con su nombre.
    </>
  ),
  spaSteps: [
    (k) => (
      <>
        En {k.b('Código')}, pulse {k.b('+')} junto a los archivos y escriba {k.code('site/index.html')}. Un nombre con
        barra coloca el archivo en una carpeta; un nombre con extensión se trata como el tipo de archivo que indica.
      </>
    ),
    (k) => (
      <>
        Añada {k.code('site/app.css')} y {k.code('site/app.js')} de la misma manera. Su página los referencia por su
        nombre, por ejemplo {k.code('href="app.css"')}, ya que la carpeta es la raíz de lo que se sirve y no forma parte de
        la dirección.
      </>
    ),
    (k) => (
      <>
        Para archivos que no son texto, como una imagen o una fuente, abra un archivo de {k.code('site')} y use el botón de
        carga situado junto a los archivos: se guardará en la misma carpeta.
      </>
    ),
    (k) => <>En {k.code('lambda.cs')}, sirva la carpeta:</>,
    (k) => (
      <>
        Pulse {k.b('Desplegar')}. {k.code('site/index.html')} responde en {k.code('/')}, {k.code('site/app.css')} en{' '}
        {k.code('/app.css')}, y cualquier dirección que no corresponda a un archivo se responde con la página, de modo que
        un frontend con enrutamiento propio sigue funcionando al recargar un enlace profundo.
      </>
    ),
    () => <>Añada una API junto a ella para que la página tenga con qué comunicarse:</>,
  ],

  storage: (k) => (
    <>
      La sección {k.b('Archivos')} muestra ambos – los archivos de una versión y el workspace como {k.b('Datos')} – e
      indica cuáles son accesibles públicamente. Los archivos del código se modifican en {k.b('Código')}; los datos pueden
      subirse y eliminarse en {k.b('Archivos')}. Sin embargo, no son lo mismo, y la diferencia está en{' '}
      {k.em('cuándo cambia cada uno')}.
    </>
  ),
  savedWithCode: 'Guardado con su código',
  workspaceColumn: 'Workspace',
  table: [
    ['qué contiene', 'todos los archivos de su lambda, incluido el C#', 'todo lo que se ha escrito o subido'],
    ['cuándo cambia', 'al pulsar Guardar o Desplegar', 'en el momento en que se escribe algo'],
    ['un despliegue', 'lo reemplaza todo', 'no lo modifica'],
    ['restaurar una versión', 'recupera los archivos anteriores', 'sin efecto'],
    ['clonar el lambda', 'se copia', 'no se copia'],
  ],
  reachedAs: 'accesible desde el código como',
  storageAside:
    'No pueden ser un único directorio. Si lo fueran, un despliegue borraría todo lo que el lambda ha escrito desde entonces, o bien nunca podría eliminarse nada de lo publicado. Un juego con clasificación necesita lo segundo; la página que lo sirve, lo primero.',

  keeping: (k) => (
    <>
      {k.code('Workspace')} es un directorio privado que su lambda puede leer y escribir. Es el lugar para todo lo que
      deba perdurar más allá de una solicitud o de un despliegue.
    </>
  ),
  keeping2: (k) => (
    <>
      También están disponibles {k.code('ReadBytes')}, {k.code('WriteBytes')}, {k.code('Delete')}, {k.code('List')},{' '}
      {k.code('CreateFolder')} y {k.code('Tree')}/{k.code('Files')}/{k.code('App')} para servirlo. No hay acceso al resto
      del sistema de archivos.
    </>
  ),

  sockets: (k) => (
    <>
      Totalmente compatibles. La demo {k.link('/editor/demo-game', 'demo-game')} empareja jugadores y ejecuta cada partida
      en el servidor. La forma más sencilla consta de tres callbacks:
    </>
  ),
  socketsAside: (k) => (
    <>
      Un detalle que conviene tener en cuenta: un navegador no puede establecer cabeceras en el handshake de un WebSocket.
      Pase lo que necesite el handler en la query, donde lo lee desde {k.code('connection.Request.Header.Query')}, o envíe
      la información confidencial en el primer mensaje.
    </>
  ),

  limits:
    'Su código se ejecuta en un servidor compartido, por lo que parte de C# se rechaza antes de compilar: iniciar procesos, abrir sus propios sockets, cargar ensamblados, acceder al sistema de archivos fuera de su workspace y la reflexión empleada para eludir estas restricciones.',
  limits2:
    'Todo lo demás está disponible, incluida toda la API de módulos de GenHTTP. Si algo se rechaza, se le indica en qué línea y por qué.',

  away: (k) => (
    <>
      {k.b('Descargar como proyecto .NET')}, en el editor, le entrega el conjunto como proyecto .NET: una solución que
      puede abrir, ejecutar con {k.code('dotnet run')} y conservar. Contiene una sola referencia de paquete y ninguna
      dependencia de esta plataforma.
    </>
  ),
  away2: (k) => (
    <>
      Su fragmento se convierte en el cuerpo de {k.code('Program.cs')}, dentro de un host que sirve lo que devuelve. Sus
      demás archivos se incluyen tal como los escribió. {k.code('Workspace')} y {k.code('Assets')} pasan a ser dos
      carpetas junto al código, con los mismos métodos, por lo que no es necesario modificar el código.
    </>
  ),
  awayAside:
    'Conviene saberlo antes de empezar: el código que escribe le pertenece y puede exportarse íntegramente. Ejecutarlo en esta plataforma no lo vincula a ella.',

  agents: (k) => (
    <>
      Hay un endpoint MCP en {k.code('/mcp')}. Un agente conectado a él puede hacer todo lo que permite el editor: leer la
      guía, consultar una demo completa, escribir archivos, compilarlos y desplegarlos. Ambos utilizan la misma API.
    </>
  ),
  agents2: (k) => (
    <>
      El agente deja constancia de sus motivos – {k.code('write_code')} recibe la especificación y el cambio – y puede
      revisar lo desplegado: {k.code('read_logs')} devuelve las solicitudes recientes del lambda, lo que ha impreso y la
      traza de pila de cualquier excepción. Así un agente comprueba que su código funciona en lugar de suponerlo. Usted
      puede consultar la misma información en el centro de control.
    </>
  ),
  more: 'Más información →',
  make: 'Crear un lambda',
};
