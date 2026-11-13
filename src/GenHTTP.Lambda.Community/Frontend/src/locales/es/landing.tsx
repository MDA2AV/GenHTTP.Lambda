import type { Messages } from '../en';

export const landing: Messages['landing'] = {
  eyebrow: 'GenHTTP Lambda · Community edition',
  headline: 'La plataforma donde tus agentes crean apps.',
  headlineAccent: 'Código abierto, en un servidor propio.',
  intro:
    'Describe una app y el agente de creación la escribe, o deja que Claude Code, Codex o Cursor lo hagan por MCP. Cada app es un handler de GenHTTP en C#, compilado en este servidor y en línea en segundos, con base de datos, claves y contraseñas, versiones y git.',
  describe: 'Describe una app',
  connect: 'Conecta tu agente',
  write: 'Escribe tú el código',

  buildTitle: 'Descríbela',
  buildText: 'Basta con una frase. El agente de creación escribe la app, la pone en línea y te da el enlace para cambiarla más adelante.',
  noAgent: 'Esta instalación no tiene ningún agente de creación conectado. Conecta tu propio agente más abajo: hace lo mismo.',

  connectTitle: 'Conecta tu agente',
  connectText:
    'Tu agente de programación recibe la plataforma como herramientas: crear una app, escribir y comprobar su código, probar un cambio en un borrador con una copia de los datos, ponerlo en línea y leer sus logs.',
  connectThen: 'Luego pídele algo, por ejemplo: «Crea un libro de visitas, con una página donde yo apruebe las entradas».',

  featuresTitle: 'Lo que recibe cada app',
  features: [
    { title: 'Una dirección', text: 'Una dirección propia en este servidor, con el nombre de su clave, y un dominio propio cuando pase a premium.' },
    { title: 'Registros', text: 'Una base de datos SQLite propia, con tablas creadas por migraciones, que se lee y se escribe con Entity Framework Core.' },
    { title: 'Archivos', text: 'Una carpeta para lo que la app guarda: subidas, exportaciones, todo lo que no es un registro.' },
    { title: 'Claves y contraseñas', text: 'Se guardan cifradas en el servidor. El agente dice cuáles necesita, tú las introduces y nadie puede volver a leerlas.' },
    { title: 'Borradores y versiones', text: 'Un cambio se prueba en un borrador con una copia de los datos y luego se pone en línea. Se conservan las versiones anteriores, y volver atrás está a un clic.' },
    { title: 'Git', text: 'Cada app es un repositorio: clónala, haz push de un commit para guardar una versión (con -o deploy para ponerla en línea) o de una rama para empezar un borrador.' },
    { title: 'Visitantes', text: 'Páginas vistas, de dónde llegaron los visitantes y sus países: se cuentan en el servidor, sin cookies, cuando lo activas.' },
    { title: 'Tu código', text: 'Descarga cualquier app como un proyecto .NET normal y ejecútala donde quieras, o publica su código fuente para que otros lo lean.' },
  ],

  runsTitle: 'Cómo funciona',
  runs: [
    { title: 'Un proceso', text: 'El servidor, el editor, la API y todas las apps se ejecutan en un solo proceso y guardan todo en una sola carpeta. El agente de compilación, si lo hay, se ejecuta a su lado.' },
    {
      title: 'Por ruta o por host',
      text: 'Las apps responden bajo /lambda/ en la dirección de la instalación o, si se le da un dominio, cada una en un subdominio propio.',
    },
    {
      title: 'Código de confianza',
      text: 'Las apps se compilan dentro del servidor y se comprueba que su código no haga lo que no debe, pero comparten su proceso. Ejecuta solo lo que ejecutarías tú mismo.',
    },
    { title: 'Código abierto', text: 'Con licencia MIT: léelo, cámbialo y ejecútalo donde quieras.' },
  ],
  repository: 'El código fuente en GitHub',

  aroundTitle: 'Echa un vistazo',
  around: {
    demos: { title: 'Empieza desde una demo', text: 'Una API sobre registros, un inicio de sesión, actualizaciones en vivo, un juego: cada una es una app terminada que puedes copiar y cambiar.' },
    showcase: { title: 'Galería', text: 'Las apps que sus propietarios quisieron mostrar aquí.' },
    guide: { title: 'Guía', text: 'Qué puede hacer una app y cómo, en el orden en que lo necesitas.' },
    api: { title: 'API REST', text: 'Lo mismo que las herramientas, por HTTP y descrito en OpenAPI.' },
    sources: { title: 'Código publicado', text: 'Apps cuyos propietarios publicaron su código, para leerlo y descargarlo.' },
    admin: { title: 'Administración', text: 'Las apps, los límites, los bloqueos y el log, tras el token de administración.' },
  },
};
