import type { EditorMessages } from '../../en/editor';

export const files: EditorMessages['files'] = {
  hint: (b) => (
    <>
      Os arquivos de uma versão: o programa. O {b('Código')} é compilado e nunca servido. Os {b('Assets')} (páginas,
      scripts, estilos, imagens) são salvos com o código, vão junto com ele em cada deploy e em cada reversão, e ficam
      públicos se o código os servir. O que a lambda guarda enquanto roda não fica aqui: são os {b('Dados')} dela.
    </>
  ),
  scope: (version, data) => (
    <>
      Estes arquivos pertencem à versão {version} e mudam com ela. O que a lambda guarda enquanto roda é o mesmo em
      todas as versões e fica em {data('Dados')}.
    </>
  ),
  edit: 'Editar esta versão',
  version: 'Versão',
  shown: (version, online, newest) => `Versão ${version}${online ? ', no ar' : newest ? ', mais recente' : ''}`,
  optionOnline: ' (no ar)',
  readFailed: 'Não foi possível ler essa versão.',
  noVersion: 'Ainda não há versão para mostrar.',
  label: 'Arquivos',
  code: 'Código',
  codeWhy: 'Compilado na lambda, nunca servido.',
  count: (files) => (files === 1 ? '1 arquivo' : `${files} arquivos`),
  codeUsage: (files, used, of) => `${files}, ${used} de ${of} caracteres`,
  usage: (files, used, of) => `${files}, ${used} de ${of}`,
  noCode: 'Nenhum código nesta versão.',
  assets: 'Assets',
  assetsPublic: 'Públicos: esta versão serve esses arquivos com Assets.',
  assetsPrivate: 'Salvos com o código, mas esta versão não os serve.',
  noAssets: 'Nenhum nesta versão.',
  context: 'Documentação e testes',
  contextWhy: 'Nunca compilados e nunca servidos: o que está escrito sobre esta versão, para quem a lê ou muda.',
  contextUsage: (files, size) => `${files}, ${size} - contados junto com os assets`,
  noContext: 'Ainda não há nada escrito sobre esta versão.',
  build: 'Build',
  buildWhy: 'Nunca compilado nem servido: aquilo a partir do qual o código ou os assets são construídos, por quem os altera.',
  noBuild: 'Nada: onde uma ferramenta de build faz o código ou os assets, aquilo a partir do qual ela os faz fica guardado aqui.',
  data: 'Dados',
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
