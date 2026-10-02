import type { EditorMessages } from '../../en/editor';

export const tabs: EditorMessages['tabs'] = {
  codeName: 'Letras, números, hífens e underscores, terminando em .cs',
  slashes: 'Sem barra no início ou no fim, e com menos de 120 caracteres.',
  deep: 'No máximo seis pastas de profundidade.',
  characters: 'Letras, números, hífens, underscores e pontos, separados por barras.',
  extension: 'Precisa de uma extensão, para ser servido do jeito certo.',
  context: 'Em .lambda/, só docs/ e tests/: letras, números, hífens, underscores e pontos, separados por barras.',
  contextFiles: 'Documentação e testes: fazem parte da versão, nunca são compilados nem servidos',
  exists: 'Já existe um arquivo com esse nome.',
  remove: (name) => `Remover ${name}? O conteúdo vai junto.`,
  there: (name) => `${name} já existe.`,
  entry: 'O trecho principal: o que ele retorna é o que é servido',
  errors: 'tem erros',
  removeFile: (name) => `Remover ${name}`,
  removeTitle: 'Remover este arquivo',
  placeholder: 'Types.cs, site/index.html ou .lambda/docs/api.md',
  newFile: 'Novo arquivo',
  uploadTitle: 'Enviar um arquivo: uma imagem, uma fonte, uma página',
  upload: 'Enviar um arquivo',
};
