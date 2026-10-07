import type { EditorMessages } from '../../en/editor';

export const tabs: EditorMessages['tabs'] = {
  codeName: 'Um ficheiro C# tem um nome com letras, algarismos, hífenes, underscores e pontos, a começar por uma letra e a terminar em .cs, com 40 carateres no máximo.',
  name: 'Letras, algarismos e - _ . + @ ( ) [ ] { } $ ~, pastas separadas por barras, sem espaços e sem nomes a terminar num ponto.',
  taken: 'Uma lambda exportada ou clonada tem no topo um ficheiro ou uma pasta com esse nome. Põe-no numa pasta ou dá-lhe outro nome.',
  lambda: 'Uma lambda já não tem uma pasta .lambda/: a documentação vai para docs/ e os testes para tests/.',
  assets: 'O que uma lambda serve está agora nos seus recursos - adiciona-o aí.',
  resourceName: 'Letras, algarismos, hífenes, underscores e pontos, separados por barras, com no máximo seis pastas de profundidade - e uma extensão, para ser servido como o tipo certo.',
  exists: 'Já existe um ficheiro com esse nome.',
  remove: (name) => `Remover ${name}? O conteúdo vai com ele.`,
  removeFolder: (name, files) => `Remover ${name} e ${files === 1 ? 'o 1 ficheiro' : `os ${files} ficheiros`} que tem?`,
  there: (name) => `${name} já existe.`,
  entry: 'O snippet: o que devolve é o que é servido',
  errors: 'tem erros',
  removeFile: (name) => `Remover ${name}`,
  removeTitle: 'Remover',
  codePlaceholder: 'Store.cs, models/Item.cs ou docs/notes.md',
  resourcePlaceholder: 'web/index.html',
  upload: 'Carregar um ficheiro',
};
