import type { EditorMessages } from '../../en/editor';

export const code: EditorMessages['code'] = {
  title: 'Código',
  version: (version) => `versión ${version}`,
  edited: ', editada',
  online: ', en línea',
  loadFailed: 'No se pudo cargar esa versión.',
  compiles: 'Compila.',
  notYet: 'Todavía no compila.',
  checkFailed: 'No se pudo comprobar el código.',
  saved: (version) => `Guardado como versión ${version}.`,
  featureSaved: 'Guardado en el borrador. Despliega su vista previa para probarlo.',
  featureLoadFailed: 'No se pudo cargar el borrador.',
  previewOnline: 'La vista previa está en línea.',
  previewRefused: 'La vista previa no cambió. Mira abajo lo que dijo el compilador.',
  isOnline: (version) => `La versión ${version} está en línea.`,
  notOnline: 'No se puso en línea. Mira abajo lo que dijo el compilador.',
  failed: 'No funcionó.',
  unchanged: 'No hay cambios desde la última vez que guardaste.',
  demo: 'Es una demo, así que todo es de solo lectura. Para cambiarla, crea tu propia lambda a partir de ella.',
  hint: (b) => (
    <>
      Los archivos de una versión. Su {b('código')} es el programa y todo lo que se guarda con él: los archivos .cs de la
      raíz se compilan, y todos los demás (su documentación, sus pruebas, aquello a partir de lo que se genera un front
      end) se guardan con la versión y nunca se compilan ni se sirven. Sus {b('recursos')} (páginas, scripts, estilos,
      imágenes, las migraciones de la base de datos) se leen y se sirven mientras se ejecuta, y son públicos donde el
      código los sirve. Al guardar se crea una versión nueva y lo que está en línea no se toca; para probar antes un
      cambio, empieza un borrador. Ctrl-S guarda, F12 va a una declaración.
    </>
  ),
  hintFeature: (b) => (
    <>
      Los archivos de este borrador: su {b('código')} (los archivos .cs de la raíz se compilan, el resto se guarda con
      él) y sus {b('recursos')}, que se leen y se sirven mientras se ejecuta. Al guardar se quedan en el borrador y se
      muestran en la dirección propia del borrador; tus visitantes no ven nada hasta que lo pongas en línea.
    </>
  ),
  inFeature: (name) => `en «${name}»`,
  changedElsewhere: 'El borrador se guardó en otro sitio desde que lo abriste (quizá lo hizo el agente). Carga lo guardado antes de guardar aquí; tus cambios no se guardarían encima.',
  readAgain: 'Cargar lo guardado',
  newer: (version) => `La versión ${version} es más nueva que la que tienes abierta aquí.`,
  check: 'Comprobar',
  save: 'Guardar',
  deploy: 'Desplegar',
  deployPreviewTitle: 'Guardar y poner en línea el borrador en su propia dirección para probarlo',
  binary: (size) => `No es texto, así que no hay nada que editar aquí. Pesa ${size}.`,
  saveAndDeploy: 'Guardar y desplegar',
  saveVersion: 'Guardar una versión nueva',
  fromOlder: (version, newest) =>
    `Esto parte de la versión ${version}, y la versión ${newest} es más nueva. Al guardar pasa a ser la versión más nueva, sin lo que vino después de la versión ${version}.`,
  featureInstead: (start) => (
    <>
      ¿Vas a probar algo? {start('Mejor, ponlo en un borrador nuevo')}: tiene su propia dirección, y no se guarda ninguna
      versión hasta que esté bien.
    </>
  ),
  cancel: 'Cancelar',
  what: '¿Qué cambia? Es opcional y se muestra en el historial.',
  placeholder: 'Añade un formulario de contacto',
  goToDefinition: 'Ir a la definición',
  versionLabel: 'Versión',
  shown: (version, online, newest) =>
    `Versión ${version}${online ? ', en línea' : newest ? ', la más reciente' : ''}`,
  optionOnline: ' (en línea)',
  switchUnsaved: 'Lo que cambiaste aquí no está guardado. ¿Abrir la otra versión de todos modos?',
  noVersion: 'Todavía no hay ninguna versión que mostrar.',
  label: 'Archivos',
  codeGroup: 'Código',
  codeWhy: 'Nunca se sirve. Los archivos .cs de la raíz se compilan; el resto se guarda con la versión.',
  resources: 'Recursos',
  resourcesPublic: 'Públicos: esta versión los sirve con Resources.',
  resourcesPrivate: 'Van con la versión, pero esta versión no los sirve.',
  noResources: 'Ninguno en esta versión.',
  count: (files) => (files === 1 ? '1 archivo' : `${files} archivos`),
  groupUsage: (files, size) => `${files}, ${size}`,
  usage: (used, of) => `Esta versión ocupa ${used} de los ${of} que puede tener una versión, entre su código y sus recursos.`,
  scope: (data) => (
    <>Lo que la lambda guarda mientras se ejecuta es igual para todas las versiones y está en {data('Datos')}.</>
  ),
  download: 'Descargar',
  newIn: (group) => `Archivo nuevo en ${group}`,
  uploadIn: (group) => `Subir a ${group}`,
  pick: 'Elige un archivo para ver qué contiene.',
};
