import type { Messages } from '../en';

export const shell: Messages['shell'] = {
  main: 'Navegación principal',
  build: 'Crear una web',
  ship: 'Publicar',
  showcase: 'Galería',
  enterprise: 'Empresas',
  docs: 'Docs',
  admin: 'Admin',
  lightMode: 'Cambiar a modo claro',
  darkMode: 'Cambiar a modo oscuro',
  openMenu: 'Abrir el menú',
  closeMenu: 'Cerrar el menú',
  language: 'Idioma',
  terms: 'Condiciones del servicio',
  privacy: 'Política de privacidad',
  imprint: 'Aviso legal',
  writeCode: 'Escribe tú el código',
  contact: 'Contacto',
};

export const common: Messages['common'] = {
  loading: 'Cargando…',
  loadingEditor: 'Cargando el editor…',
  editorFailed: 'No se pudo cargar el editor',
  pageFailed: 'No se pudo cargar la página',
  editorFailedWhy: 'Suele pasar cuando el sitio se actualiza mientras tienes esta pestaña abierta.',
  reload: 'Recargar la página',
  backToStart: 'Volver al inicio',
  tryAgain: 'Volver a intentarlo',
  copy: 'Copiar',
  copied: 'Copiado',
  copyToClipboard: 'Copiar al portapapeles',
  openInNewTab: 'Abrir en una pestaña nueva',
  close: 'Cerrar',
  operatorCountry: 'Alemania',
};

export const notFound: Messages['notFound'] = {
  title: 'Página no encontrada',
  heading: 'Esta página no existe',
  text: 'Puede que el enlace sea antiguo o que la lambda a la que apuntaba se haya eliminado.',
};

export const abuse: Messages['abuse'] = {
  report: 'Denunciar abuso',
  title: 'Denunciar una lambda',
  write: 'Escríbenos',
  subject: 'Denuncia de abuso',
  intro:
    'Aquí cualquiera puede publicar código, así que a veces alguien publica lo que no debe. Si una página alojada aquí intenta engañar a la gente, ataca algo o usa material sin tener derecho a ello, avísanos y la retiramos.',
  how: (mailbox, strong, path) => (
    <>
      Escribe a {mailbox} e incluye {strong('la dirección de la página')} (tiene esta forma: {path}) y una frase sobre
      qué tiene de malo. Una captura de pantalla ayuda. No necesitas cuenta ni ser usuario de este sitio.
    </>
  ),
  next: (strong, terms) => (
    <>
      {strong('Qué pasa después.')} Lo lee una persona. Si la lambda incumple las {terms('condiciones del servicio')},
      la desconectamos, por lo general en un día. No te diremos quién la publicó y no podemos prometer responder a cada
      denuncia, pero las leemos todas.
    </>
  ),
  danger:
    'Si alguien está en peligro inmediato o se está cometiendo un delito, avisa también a las autoridades locales. Nosotros podemos retirar una página, pero nada más.',
};
