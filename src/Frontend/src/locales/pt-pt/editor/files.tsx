import type { EditorMessages } from '../../en/editor';

export const files: EditorMessages['files'] = {
  version: 'Versão',
  shown: (version, online, newest) =>
    `Versão ${version}${online ? ', online' : newest ? ', a mais recente' : ''}`,
  optionOnline: ' (online)',
  count: (files) => (files === 1 ? '1 ficheiro' : `${files} ficheiros`),
  usage: (files, used, of) => `${files}, ${used} de ${of}`,
  dataPublic: 'Públicos: o código online serve-os com Workspace.',
  dataPrivate: 'Privados: só a lambda os usa. Não fazem parte de nenhuma versão.',
  uploadFailed: (path) => `Não foi possível carregar ${path}.`,
  deleteFolder: (path, held) =>
    held > 0
      ? `Eliminar a pasta ${path} e ${held === 1 ? 'o ficheiro' : `os ${held} ficheiros`} que contém?`
      : `Eliminar a pasta ${path}?`,
  deleteFile: (path) => `Eliminar ${path}? A lambda deixa de o encontrar.`,
  deleteFailed: 'Não foi possível eliminar.',
  full: 'O espaço para dados está cheio',
  uploadInto: (folder) => `Carregar para ${folder}`,
  upload: 'Carregar',
  reading: 'A ler…',
  noData: 'Ainda nada. O que a lambda guardar enquanto corre aparece aqui.',
  delete: (path) => `Eliminar ${path}`,
  deleteShort: 'Eliminar',
  fileFailed: 'Não foi possível ler o ficheiro.',
  pick: 'Escolhe um ficheiro para ver o que tem.',
  tooLarge: (name, size) => (
    <>
      {name} tem {size}, é demasiado grande para mostrar aqui.
    </>
  ),
  download: 'Transferir',
  readingFile: (name) => `A ler ${name}…`,
  missing: (name) => `Esta versão não tem nenhum ficheiro chamado ${name}.`,
  saved: 'guardado',
  notText: 'Não é texto. Transfere-o para ver o conteúdo.',
};
