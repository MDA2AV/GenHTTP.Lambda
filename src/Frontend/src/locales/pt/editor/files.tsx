import type { EditorMessages } from '../../en/editor';

export const files: EditorMessages['files'] = {
  version: 'Versão',
  shown: (version, online, newest) => `Versão ${version}${online ? ', no ar' : newest ? ', mais recente' : ''}`,
  optionOnline: ' (no ar)',
  count: (files) => (files === 1 ? '1 arquivo' : `${files} arquivos`),
  usage: (files, used, of) => `${files}, ${used} de ${of}`,
  dataPublic: 'Públicos: o código no ar serve os dados com Workspace.',
  dataPrivate: 'Privados: só a lambda acessa. Não fazem parte de nenhuma versão.',
  uploadFailed: (path) => `Não foi possível enviar ${path}.`,
  deleteFolder: (path, held) =>
    held > 0
      ? `Excluir a pasta ${path} e ${held === 1 ? 'o arquivo' : `os ${held} arquivos`} dentro dela?`
      : `Excluir a pasta ${path}?`,
  deleteFile: (path) => `Excluir ${path}? A lambda não vai mais encontrar esse arquivo.`,
  deleteFailed: 'Não foi possível excluir.',
  full: 'O espaço de dados está cheio',
  uploadInto: (folder) => `Enviar para ${folder}`,
  upload: 'Enviar',
  reading: 'Lendo…',
  noData: 'Nada ainda. O que a lambda salvar enquanto roda aparece aqui.',
  delete: (path) => `Excluir ${path}`,
  deleteShort: 'Excluir',
  fileFailed: 'Não foi possível ler o arquivo.',
  pick: 'Escolha um arquivo para ver o que tem nele.',
  tooLarge: (name, size) => (
    <>
      {name} tem {size}, grande demais para mostrar aqui.
    </>
  ),
  download: 'Baixar',
  readingFile: (name) => `Lendo ${name}…`,
  missing: (name) => `Esta versão não tem nenhum arquivo chamado ${name}.`,
  saved: 'salvo',
  notText: 'Não é texto. Baixe para ver o conteúdo.',
};
