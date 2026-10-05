import type { EditorMessages } from '../../en/editor';

export const clone: EditorMessages['clone'] = {
  button: 'Clonar',
  title: 'Clonar con git',
  intro:
    'Trabaja con tus propias herramientas y tu agente de programación: el repositorio es el proyecto con el que se ejecuta, cada versión un commit de main y cada borrador una rama.',
  keyWarning: 'La dirección contiene la clave de edición: quien la tenga puede cambiar la app. No la incluyas en lo que compartas.',
  draft: (branch) => <>Este borrador es la rama {branch}.</>,
  pushing: 'Hacer push',
  toMain: (deploy) => <>Un commit enviado a main es la siguiente versión, todavía sin poner en línea: {deploy} la pone en línea con el push.</>,
  toBranch: 'Una rama enviada es un borrador, con su vista previa en línea en una dirección propia.',
  agents: (file) => <>{file} en el repositorio le cuenta el resto a un agente de programación.</>,
  readOnly: 'Una demo es de solo lectura: clónala para leerla y crea una lambda propia a partir de ella para cambiarla.',
  copy: 'Copiar',
  copied: 'Copiado',
};
