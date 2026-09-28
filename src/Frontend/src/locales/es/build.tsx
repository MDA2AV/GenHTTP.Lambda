import type { Messages } from '../en';

export const build: Messages['build'] = {
  offTitle: 'Aquí no está activado',
  off: (write, mcp) => (
    <>
      Esta instalación no tiene agente para crear apps. Aun así, puedes {write('escribir tú el código')} o conectar tu
      propio Claude a {mcp}.
    </>
  ),

  title: 'Di lo que quieres.',
  intro:
    'Tu app se crea y se publica, y recibes un enlace para mandárselo a quien quieras. Sin cuenta y sin instalar nada. Además, recuerda cosas (puntuaciones, mensajes, entradas), así que todos los que la abren ven lo mismo.',
  placeholder: 'crea un…',
  working: 'trabajando…',
  shortcut: 'ctrl + enter',
  building: 'Creando',
  buildIt: 'Crear',
  builtBy: 'Crear con',
  password: 'contraseña',
  fable:
    'Fable tiene contraseña mientras está en pruebas. No tiene límite de tiempo, así que sigue hasta terminar, no hasta que se acabe el reloj.',
  onlyNew:
    'Aquí solo se crean apps nuevas. Para cambiar algo que ya hiciste, abre su enlace de edición y, en la sección Cambiar, di qué debería ser distinto.',
  ideas: [
    'crea un muro donde cualquiera pueda dejar un mensaje de una línea',
    'haz un ranking para un juego de dados',
    'crea una encuesta donde la gente vote y vea los resultados',
    'haz un libro de visitas para mi boda',
    'crea una cuenta regresiva hasta una fecha, visible para todos',
  ],
  ahead: (waiting) =>
    waiting === 1 ? 'Hay una app en cola antes que la tuya. Después vas tú.' : `Hay ${waiting} apps en cola antes que la tuya.`,
  starting: 'Empezando…',

  yourApp: 'Tu app',
  further: 'Para seguir mejorándola',
  keep: 'No lo pierdas. Es la única forma de volver a entrar y nadie puede recuperarlo, ni siquiera nosotros. Guárdalo en tus marcadores antes de cerrar esta pestaña.',
  change:
    'Para cambiarla, abre el enlace de edición y, en la sección Cambiar, di qué debería ser distinto, igual que aquí. También puede hacerlo tu propio agente de programación, como te explicamos abajo.',
  copyLink: 'Copiar el enlace de edición',
  lifetime: (offline, removed) =>
    `Sigue en línea mientras se use: después de ${offline} días sin visitas ni cambios se desconecta, y a los ${removed} días se elimina. Para volver a publicarla, abre el editor y haz clic en Desplegar.`,
  openEditor: 'Abrir el editor',
  another: 'Crear otra app',

  keepGoing: 'Sigue con tu propio agente',
  orOwn: 'O usa tu propio agente',
  ownText:
    'El cuadro de arriba es un Claude que se ejecuta en este servidor. Si ya tienes uno propio, conéctalo aquí y podrá hacer lo mismo (crear una lambda, escribir el código, publicarla) sin límite diario y sin pasar por esta página.',
  thenAsk: 'Después, pídele lo que quieras, igual que aquí.',
  claudeWeb: 'Claude en la web',
  claudeWebHow:
    'Configuración, luego «Connectors» y después «Add custom connector». Pega la dirección de arriba como URL del servidor MCP remoto. No hay clave ni inicio de sesión.',
  howToChange: 'Así también se cambia algo que ya está creado: dale el enlace de edición a tu agente y dile qué hacer.',
  more: 'Más sobre cómo usar un agente aquí',

  failedToStart: 'No se pudo enviar.',
  noAnswer: 'Terminó sin decir qué pasó.',
  failed: 'No funcionó.',
};
