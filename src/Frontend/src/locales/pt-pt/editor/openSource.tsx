import type { EditorMessages } from '../../en/editor';

export const openSource: EditorMessages['openSource'] = {
  loading: 'A carregar…',
  loadFailed: 'Não foi possível saber se o código está publicado.',
  hint: (tool) => (
    <>
      Uma lambda publicada pode ser lida, receber estrelas e ser transferida por qualquer pessoa, na página dela em
      Código aberto: todas as versões, sob a licença que escolheres, e nunca os dados que ela guarda. Só quem tem a
      chave de edição a pode publicar ou retirar. Um agente pode fazer o mesmo com a ferramenta {tool}.
    </>
  ),
  hintSimple:
    'Qualquer pessoa pode ler como a tua app é feita, numa página própria, e aproveitá-la sob a licença que escolheres – mas nunca o que ela guarda. Só tu a podes publicar ou retirar.',
  open: 'Abrir a página do código',
  switch: 'Publicar o código desta app',
  publishedNow: (license) => `Publicado sob ${license}. Qualquer pessoa o pode ler e transferir.`,
  off: 'Desligado. Ninguém vê o código até o publicares.',
  keptStars: (stars) =>
    stars === 1
      ? 'A estrela fica guardada para quando o voltares a publicar.'
      : `As ${stars} estrelas ficam guardadas para quando o voltares a publicar.`,
  published: 'Publicado. Qualquer pessoa já pode ler o código.',
  saved: 'Guardado.',
  saveFailed: 'Não foi possível publicar o código.',
  withdrawn: 'Publicação retirada. A página já não existe.',
  withdrawFailed: 'Não foi possível retirar a publicação.',
  whatTitle: 'O que é publicado',
  what: [
    'O código – tal como está agora, e cada estado anterior',
    'Tudo o que mostra: as páginas, os estilos e as imagens',
    'O que está escrito sobre ela: para que serve e como é testada',
    'Cada alteração por que passou, numa linha cada uma',
  ],
  neverTitle: 'O que nunca é publicado',
  never: [
    'O que guarda: os registos, o que guardou, as chaves e palavras-passe',
    'O que pediste, pelas tuas próprias palavras',
    'Quem a usa: os visitantes e o que fizeram',
    'O link de edição',
  ],
  careful:
    'Tudo o que está no código fica público, incluindo os estados anteriores. Uma palavra-passe ou uma chave nunca deve ir para o código: o lugar dela é junto das chaves e palavras-passe em Dados, que nunca são publicadas.',
  licenseLabel: 'Licença',
  licenseHint: 'O que os outros podem fazer com o código. A MIT, a mais comum, deixa qualquer pessoa fazer quase tudo com ele, desde que o teu nome se mantenha.',
  readLicense: 'Ler a licença',
  authorLabel: 'Nome na licença',
  optional: 'opcional',
  authorPlaceholder: (key) => `Os autores de ${key}`,
  authorHint: 'O teu nome ou o da tua organização, mostrado na página do código e na licença. Se ficar vazio, a licença refere os autores desta app.',
  publish: 'Publicar',
  save: 'Guardar alterações',
  allSaved: 'Está tudo guardado.',
  takeDown: 'Retirar a publicação',
  confirm: 'Retirar a publicação do código?',
  confirmText:
    'A página e as transferências desaparecem de imediato. Quem já o transferiu fica com ele, sob a licença com que veio. As estrelas ficam guardadas para quando o voltares a publicar.',
  keep: 'Manter publicado',
  stars: (count) => (count === 1 ? '1 estrela' : `${count} estrelas`),
  sidebar: 'Código-fonte',
};
