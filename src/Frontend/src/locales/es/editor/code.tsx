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
  demo: 'Es una demo, así que todo es de solo lectura. Para cambiarla, crea tu propia lambda a partir de ella. ',
  edit: 'Edita el código a mano. Al guardar se crea una versión nueva y lo que está en línea no cambia; al desplegar, se pone en línea. Para probar un cambio antes, empieza un borrador. ',
  editFeature:
    'El código de este borrador. Al guardar se queda en el borrador: no cambia nada de lo que reciben los visitantes de la lambda. Al desplegar se pone en línea en la dirección propia del borrador, para probarlo; al fusionar el borrador, pasa a ser la siguiente versión. ',
  inFeature: (name) => `en «${name}»`,
  changedElsewhere: 'El borrador se guardó en otro sitio desde que lo abriste (quizá lo hizo el agente). Carga lo guardado antes de guardar aquí; tus cambios no se guardarían encima.',
  readAgain: 'Cargar lo guardado',
  files: (entry, cs, context) => (
    <>
      {entry} devuelve lo que se sirve, los demás archivos {cs} contienen tipos y cualquier otro archivo se sirve tal
      cual, salvo lo que hay en {context}: la documentación, las pruebas y el espacio de desarrollo, que nunca se
      compilan ni se sirven. Ctrl-S guarda; F12 va a una declaración.
    </>
  ),
  newer: (version) => ` La versión ${version} es más nueva que la que tienes abierta aquí.`,
  built: (folder) =>
    `La compilación del espacio de desarrollo escribe ${folder}: la próxima compilación reemplaza lo que cambies aquí. Cambia mejor las fuentes a partir de las que se compila.`,
  check: 'Comprobar',
  save: 'Guardar',
  deploy: 'Desplegar',
  deployPreviewTitle: 'Guardar y poner en línea el borrador en su propia dirección para probarlo',
  binary: (size) => `No es texto, así que no hay nada que editar. Se sirve tal cual y pesa ${size} kB.`,
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
};
