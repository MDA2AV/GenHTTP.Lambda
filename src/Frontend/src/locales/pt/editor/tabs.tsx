import type { EditorMessages } from '../../en/editor';

export const tabs: EditorMessages['tabs'] = {
  codeName: 'Um arquivo C# tem nome com letras, números, hífens, underscores e pontos, começando com uma letra e terminando em .cs, com no máximo 40 caracteres.',
  name: 'Letras, números e - _ . + @ ( ) [ ] { } $ ~, pastas separadas por barras, sem espaços e sem nome terminado em ponto.',
  taken: 'Uma lambda exportada ou clonada tem um arquivo ou uma pasta com esse nome no topo. Coloque-o numa pasta ou dê outro nome.',
  lambda: 'Uma lambda não tem mais uma pasta .lambda/: a documentação fica em docs/ e os testes em tests/.',
  assets: 'O que uma lambda serve agora fica nos recursos dela: adicione lá.',
  resourceName: 'Letras, números, hífens, underscores e pontos, separados por barras, com no máximo seis pastas de profundidade, e uma extensão, para que seja servido como o tipo certo.',
  exists: 'Já existe um arquivo com esse nome.',
  remove: (name) => `Remover ${name}? O conteúdo vai junto.`,
  removeFolder: (name, files) => `Remover ${name} e ${files === 1 ? '1 arquivo' : `${files} arquivos`} que há nela?`,
  there: (name) => `${name} já existe.`,
  entry: 'O trecho principal: o que ele retorna é o que é servido',
  errors: 'tem erros',
  removeFile: (name) => `Remover ${name}`,
  removeTitle: 'Remover',
  codePlaceholder: 'Store.cs, models/Item.cs ou docs/notes.md',
  resourcePlaceholder: 'web/index.html',
  upload: 'Enviar um arquivo',
};
