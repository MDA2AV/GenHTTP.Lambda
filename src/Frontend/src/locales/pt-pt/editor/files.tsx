import type { EditorMessages } from '../../en/editor';

export const files: EditorMessages['files'] = {
  hint: (b) => (
    <>
      Os ficheiros de uma versão: o programa. O {b('Código')} é compilado e nunca é servido. Os {b('Assets')} (páginas,
      scripts, estilos, imagens) são guardados com o código, seguem com ele em cada deploy e em cada reversão, e são
      públicos se o código os servir. O que a lambda guarda enquanto corre não está aqui: são os {b('Dados')} dela.
    </>
  ),
  scope: (version, data) => (
    <>
      Estes ficheiros pertencem à versão {version} e mudam com ela. O que a lambda guarda enquanto corre é igual para
      todas as versões, e está em {data('Dados')}.
    </>
  ),
  edit: 'Editar esta versão',
  version: 'Versão',
  shown: (version, online, newest) =>
    `Versão ${version}${online ? ', online' : newest ? ', a mais recente' : ''}`,
  optionOnline: ' (online)',
  readFailed: 'Não foi possível ler essa versão.',
  noVersion: 'Ainda não há nenhuma versão para mostrar.',
  label: 'Ficheiros',
  code: 'Código',
  codeWhy: 'Compilado na lambda, nunca servido.',
  count: (files) => (files === 1 ? '1 ficheiro' : `${files} ficheiros`),
  codeUsage: (files, used, of) => `${files}, ${used} de ${of} caracteres`,
  usage: (files, used, of) => `${files}, ${used} de ${of}`,
  noCode: 'Não há código nesta versão.',
  assets: 'Assets',
  assetsPublic: 'Públicos: esta versão serve-os com Assets.',
  assetsPrivate: 'Guardados com o código, mas esta versão não os serve.',
  noAssets: 'Nenhum nesta versão.',
  context: 'Documentação e testes',
  contextWhy: 'Nunca compilados nem servidos: o que está escrito sobre esta versão, para quem a lê ou altera.',
  contextUsage: (files, size) => `${files}, ${size} - contados com os assets`,
  noContext: 'Ainda não há nada escrito sobre esta versão.',
  build: 'Compilação',
  buildWhy: 'Nunca compilado nem servido: aquilo a partir do qual o código ou os assets são compilados, por quem os altera.',
  noBuild: 'Nada - onde uma ferramenta de compilação faz o código ou os assets, aquilo a partir do qual os faz fica guardado aqui.',
  data: 'Dados',
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
