import type { EditorMessages } from '../../en/editor';

export const openSource: EditorMessages['openSource'] = {
  loading: 'Carregando…',
  loadFailed: 'Não foi possível verificar se o código está publicado.',
  hint: (tool) => (
    <>
      Uma lambda publicada pode ser lida, receber estrelas e ser baixada por qualquer pessoa na página dela em Código
      aberto: todas as versões, sob a licença que você escolher, e nunca os dados que ela guarda. Só quem tem a chave
      de edição pode publicá-la ou retirá-la. Um agente pode fazer o mesmo com a ferramenta {tool}.
    </>
  ),
  hintSimple:
    'Qualquer pessoa pode ler como o seu app é feito, numa página própria, e aproveitá-lo sob a licença que você escolher – mas nunca o que ele guarda. Só você pode publicá-lo ou retirá-lo.',
  open: 'Abrir a página do código',
  switch: 'Publicar o código deste app',
  publishedNow: (license) => `Publicado sob ${license}. Qualquer pessoa pode ler e baixar o código.`,
  off: 'Desligado. Ninguém vê o código até você publicá-lo.',
  keptStars: (stars) =>
    stars === 1
      ? 'A estrela dele fica guardada para quando você publicar de novo.'
      : `As ${stars} estrelas dele ficam guardadas para quando você publicar de novo.`,
  published: 'Publicado. Qualquer pessoa já pode ler o código.',
  saved: 'Salvo.',
  saveFailed: 'Não foi possível publicar o código.',
  withdrawn: 'Publicação retirada. A página dele não existe mais.',
  withdrawFailed: 'Não foi possível retirar a publicação.',
  whatTitle: 'O que é publicado',
  what: [
    'O código dele – como está agora, e cada estado anterior',
    'Tudo o que ele mostra: as páginas, os estilos e as imagens',
    'O que está escrito sobre ele: para que serve e como é testado',
    'Cada mudança pela qual ele passou, numa linha cada',
  ],
  neverTitle: 'O que nunca é publicado',
  never: [
    'O que ele guarda: os registros, o que ele salvou, as chaves e senhas',
    'O que você pediu, com as suas próprias palavras',
    'Quem usa o app: os visitantes e o que eles fizeram',
    'O link de edição',
  ],
  careful:
    'Tudo o que está no código fica público, inclusive os estados anteriores. Uma senha ou uma chave nunca deve ir no código: o lugar dela é junto das chaves e senhas em Dados, que nunca são publicadas.',
  licenseLabel: 'Licença',
  licenseHint: 'O que outras pessoas podem fazer com o código. A MIT, a mais comum, deixa qualquer pessoa fazer quase tudo com ele, desde que o seu nome continue nele.',
  readLicense: 'Ler a licença',
  authorLabel: 'Nome na licença',
  optional: 'opcional',
  authorPlaceholder: (key) => `Os autores de ${key}`,
  authorHint: 'O seu nome ou o da sua organização, mostrado na página do código e na licença. Se ficar vazio, a licença cita os autores deste app.',
  publish: 'Publicar',
  save: 'Salvar mudanças',
  allSaved: 'Tudo salvo.',
  takeDown: 'Retirar a publicação',
  confirm: 'Retirar a publicação do código?',
  confirmText:
    'A página e os downloads deixam de existir na hora. Quem já baixou continua com o código, sob a licença com que ele veio. As estrelas ficam guardadas para quando você publicar de novo.',
  keep: 'Manter publicado',
  stars: (count) => (count === 1 ? '1 estrela' : `${count} estrelas`),
  sidebar: 'Código-fonte',
};
