import type { Messages } from '../en';

export const build: Messages['build'] = {
  offTitle: 'No disponible en esta instalación',
  off: (write, mcp) => (
    <>
      Esta instalación no dispone de un agente de creación. No obstante, puede {write('escribir el código usted mismo')}{' '}
      o conectar su propio Claude a {mcp}.
    </>
  ),

  title: 'Describa lo que necesita.',
  intro:
    'La aplicación se crea y se publica, y usted recibe un enlace que puede enviar a cualquier persona. Sin cuenta ni instalaciones, y con capacidad para guardar datos – puntuaciones, mensajes, entradas – de modo que todos vean lo mismo.',
  placeholder: 'crea un…',
  working: 'en curso…',
  shortcut: 'Ctrl + Intro',
  building: 'Creando',
  buildIt: 'Crear',
  builtBy: 'Creado con',
  password: 'contraseña',
  fable:
    'Fable está protegido con contraseña mientras se encuentra en fase de prueba. Funciona sin límite de tiempo y continúa hasta que la aplicación está terminada.',
  onlyNew:
    'Aquí solo se crean aplicaciones nuevas. Para seguir desarrollando una existente, entregue su enlace de edición a su propio agente de programación; consulte más abajo.',
  ideas: [
    'un muro donde cualquiera pueda dejar un mensaje breve',
    'una tabla de puntuaciones para un juego de dados',
    'una encuesta con resultados visibles',
    'un libro de visitas para nuestra boda',
    'una cuenta atrás compartida hasta una fecha',
  ],
  ahead: (waiting) =>
    waiting === 1 ? 'Hay una creación antes de la suya; usted es el siguiente.' : `Hay ${waiting} creaciones antes de la suya.`,
  starting: 'Iniciando…',

  yourApp: 'Su aplicación',
  further: 'Para seguir desarrollándola',
  keep: 'Conserve este enlace. Es el único acceso y no se puede recuperar, ni siquiera por nuestra parte. Guárdelo en sus marcadores antes de cerrar esta pestaña.',
  change:
    'Esta página solo crea aplicaciones nuevas. Para modificar esta, conecte su propio agente de programación como se describe a continuación, entréguele el enlace de edición y describa el cambio deseado.',
  copyLink: 'Copiar el enlace de edición',
  lifetime: (offline, removed) =>
    `La aplicación permanece en línea mientras se utiliza: tras ${offline} días sin visitas ni cambios se desconecta, y tras ${removed} días se elimina. Para volver a publicarla, abra el editor y pulse Desplegar.`,
  openEditor: 'Abrir el editor',
  another: 'Crear otra aplicación',

  keepGoing: 'Continuar con su propio agente',
  orOwn: 'O utilice su propio agente',
  ownText:
    'El campo anterior utiliza un Claude que se ejecuta en este servidor. Si ya dispone de su propio agente, puede conectarlo aquí: tendrá las mismas capacidades – crear un lambda, escribir el código, publicarlo – sin límite diario y sin pasar por esta página.',
  thenAsk: 'A continuación, describa lo que necesita, igual que aquí.',
  claudeWeb: 'Claude en la web',
  claudeWebHow:
    'Configuración, luego «Connectors» y después «Add custom connector». Pegue la dirección anterior como URL del servidor MCP remoto. No se necesita clave ni inicio de sesión.',
  howToChange:
    'Así también se modifica una aplicación ya creada: entregue el enlace de edición a su agente y describa el cambio.',
  more: 'Más información sobre el uso de un agente',

  failedToStart: 'No se pudo iniciar la solicitud.',
  noAnswer: 'El proceso finalizó sin indicar el resultado.',
  failed: 'La operación no se completó.',
};
