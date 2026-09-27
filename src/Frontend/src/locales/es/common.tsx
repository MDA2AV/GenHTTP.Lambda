import type { Messages } from '../en';

export const shell: Messages['shell'] = {
  main: 'Navegación principal',
  build: 'Crear',
  ship: 'Publicar',
  showcase: 'Galería',
  enterprise: 'Empresas',
  docs: 'Documentación',
  admin: 'Admin',
  lightMode: 'Cambiar al tema claro',
  darkMode: 'Cambiar al tema oscuro',
  openMenu: 'Abrir el menú',
  closeMenu: 'Cerrar el menú',
  language: 'Idioma',
};

export const common: Messages['common'] = {
  loading: 'Cargando…',
  loadingEditor: 'Cargando el editor…',
  editorFailed: 'No se pudo cargar el editor',
  editorFailedWhy: 'Por lo general, esto significa que el sitio se actualizó mientras esta pestaña estaba abierta.',
  reload: 'Recargar la página',
  backToStart: 'Volver al inicio',
  tryAgain: 'Reintentar',
  copy: 'Copiar',
  copied: 'Copiado',
  copyToClipboard: 'Copiar al portapapeles',
  openInNewTab: 'Abrir en una pestaña nueva',
  close: 'Cerrar',
};

export const notFound: Messages['notFound'] = {
  title: 'Página no encontrada',
  heading: 'Esta página no existe',
  text: 'Es posible que el enlace esté desactualizado o que el lambda al que apuntaba se haya eliminado.',
};

export const missing: Messages['missing'] = {
  title: 'No hay nada en funcionamiento aquí',
  heading: 'No hay nada en funcionamiento aquí',
  notDeployed: (key) => (
    <>
      Existe un lambda en {key}, pero en este momento no está desplegado. En el plan gratuito, los despliegues permanecen
      en línea mientras se utilizan y se retiran tras un mes sin visitas ni cambios. Quien disponga del enlace de edición
      puede volver a ponerlo en línea.
    </>
  ),
  unknown: (key) => (
    <>
      No hay ningún lambda alojado en {key}. Es posible que la clave nunca haya existido o que el lambda correspondiente
      se haya eliminado.
    </>
  ),
  create: 'Crear un lambda',
};

export const abuse: Messages['abuse'] = {
  report: 'Denunciar un abuso',
  title: 'Denunciar un lambda',
  write: 'Escribirnos',
  subject: 'Denuncia de abuso',
  intro:
    'En esta plataforma cualquier persona puede publicar código, por lo que en ocasiones se publican contenidos indebidos. Si una página alojada aquí intenta engañar, ataca otros sistemas o utiliza material sin tener los derechos correspondientes, le rogamos que nos lo comunique: la retiraremos.',
  how: (mailbox, strong, path) => (
    <>
      Escriba a {mailbox} indicando {strong('la dirección de la página')} – con el formato {path} – y una breve
      descripción del problema. Una captura de pantalla es de gran ayuda. No necesita una cuenta ni ser usuario de este
      sitio.
    </>
  ),
  next: (strong, terms) => (
    <>
      {strong('Próximos pasos.')} Cada denuncia es revisada por una persona. Si el lambda infringe las{' '}
      {terms('condiciones del servicio')}, se retira, normalmente en el plazo de un día. No revelamos la identidad de
      quien lo publicó y no podemos responder a cada denuncia, pero todas se leen.
    </>
  ),
  danger:
    'Si alguien se encuentra en peligro inminente o se está cometiendo un delito, le rogamos que contacte también con las autoridades competentes. Podemos retirar una página, pero no podemos tomar otras medidas.',
};
