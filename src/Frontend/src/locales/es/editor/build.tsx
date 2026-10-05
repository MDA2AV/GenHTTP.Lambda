import type { EditorMessages } from '../../en/editor';

export const build: EditorMessages['build'] = {
  title: 'Compilación',
  hint: 'Aquello a partir de lo que se compilan los recursos o el código de una versión: archivos sobre los que quien cambia la app (tu agente, en un clon) ejecuta una herramienta de compilación, guardados con cada versión y que nunca se compilan ni se sirven. Esta plataforma no compila nada, así que aquí se leen, no se editan.',
  overview: 'Resumen',
  files: 'Archivos',
  scope: (version) =>
    `Aquello a partir de lo que se compila la versión ${version}: se guarda con ella, nunca se compila ni se sirve, y lo compila quien la cambia, nunca esta plataforma.`,
  scopeDraft: 'Aquello a partir de lo que se compila este borrador: se guarda con él, nunca se compila ni se sirve, y lo compila quien lo cambia, nunca esta plataforma.',
  reading: 'Leyendo aquello a partir de lo que se compila…',
  readFailed: 'No se pudo leer aquello a partir de lo que se compila.',

  emptyTitle: (version) => `La versión ${version} no guarda nada a partir de lo que se compile`,
  emptyTitleDraft: 'Este borrador no guarda nada a partir de lo que se compile',
  emptyText: (code) => (
    <>
      Cuando los recursos o el código de una versión los genera una herramienta de compilación (compilados, empaquetados
      o generados), los archivos a partir de los que se generan se guardan aquí, con cada versión: la carpeta{' '}
      {code('build/')} en un clon. Quien cambia la app ejecuta la compilación donde trabaja y guarda ambas cosas juntas;
      esta plataforma no compila nada. Lo que se escribe tal como se sirve o se compila no necesita nada de esto.
    </>
  ),
  emptyHow: (code) => (
    <>{code('AGENTS.md')} en un clon le explica a un agente de programación cómo se usa.</>
  ),

  inVersion: (version) => `En la versión ${version}`,
  inDraft: 'En este borrador',
  comparedWith: (version) => `respecto a la versión ${version}`,
  first: 'La primera versión que lo guarda.',
  both: (here, program) =>
    `${here === 1 ? '1 archivo cambiado' : `${here} archivos cambiados`} aquí y ${program === 1 ? '1 archivo' : `${program} archivos`} del código y los recursos.`,
  hereOnly: (here) =>
    `${here === 1 ? '1 archivo cambiado' : `${here} archivos cambiados`} aquí y nada del código ni de los recursos: si lo que cambió se compila en ellos, no se compiló.`,
  programOnly: 'Aquí no ha cambiado nada.',
  unchanged: 'Aquí no ha cambiado nada, ni en el código ni en los recursos.',
  showChanges: 'Mostrar los cambios',
  hideChanges: 'Ocultar los cambios',
  noChanges: 'Aquí no ha cambiado nada.',

  readme: 'Cómo se compila',
  noReadme: (code) => (
    <>
      Nada explica cómo se compila. Un {code('README.md')} al principio (los comandos y dónde va el resultado) es lo
      que usará el próximo agente para compilar.
    </>
  ),
  readOnly: 'Solo lectura: se cambia donde se compila.',
  noFiles: 'No hay archivos.',
};
