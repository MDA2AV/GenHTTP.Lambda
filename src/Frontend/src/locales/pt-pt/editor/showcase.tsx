import type { EditorMessages } from '../../en/editor';

export const showcase: EditorMessages['showcase'] = {
  loadFailed: 'Não foi possível carregar a montra.',
  loading: 'A carregar…',
  title: 'um título',
  description: 'uma descrição',
  picture: 'uma imagem',
  updated: 'A entrada na montra foi atualizada.',
  listed: 'Já está na página da montra.',
  waiting: 'Guardado. Aparece na montra assim que a lambda estiver online.',
  saveFailed: 'Não foi possível guardar a entrada na montra.',
  removed: 'Retirada da montra.',
  removeFailed: 'Não foi possível retirar a entrada da montra.',
  wrongType: 'Isso não é uma imagem PNG, JPEG, GIF ou WebP.',
  tooLarge: (size, limit) => `Essa imagem tem ${size}; o máximo é ${limit}.`,
  unreadable: 'Não foi possível ler esse ficheiro.',
  hint: (tool) => (
    <>
      A montra lista as lambdas que os donos quiseram mostrar, primeiro as usadas há menos tempo. Só quem tem a chave
      de edição pode lá pôr uma lambda ou retirá-la, e ela só aparece enquanto estiver online. Um agente pode fazer o
      mesmo com a ferramenta {tool}.
    </>
  ),
  open: 'Abrir a montra',
  switch: 'Mostrar esta lambda na montra',
  listedNow: 'Está na montra. Quem passar por lá pode abri-la.',
  notListed: 'Guardado, mas não aparece: a lambda está offline. Volta a aparecer quando fizeres deploy outra vez.',
  off: 'Desligado. Nada sobre esta lambda aparece em lado nenhum até ligares isto e guardares.',
  offline: 'A lambda está offline, por isso a entrada fica à espera do próximo deploy. Só aparecem lambdas que respondem.',
  titleLabel: 'Título',
  titlePlaceholder: 'Pontuações do quiz do bar',
  descriptionLabel: 'Descrição',
  descriptionPlaceholder:
    'As equipas enviam as respostas pelo telemóvel, o apresentador corrige-as e a tabela atualiza-se para toda a sala.',
  save: 'Guardar alterações',
  add: 'Adicionar à montra',
  takeOff: 'Retirar da montra',
  needs: (missing) =>
    missing.length > 1
      ? `Ainda faltam ${missing.slice(0, -1).join(', ')} e ${missing[missing.length - 1]}.`
      : `Ainda falta ${missing[0]}.`,
  tooLong: 'Há campos demasiado longos.',
  allSaved: 'Está tudo guardado.',
  preview: 'Pré-visualização',
  card: (address) => <>Este é o cartão que os visitantes veem. Abre {address}.</>,
  confirm: 'Retirar da montra?',
  keep: 'Manter',
  confirmText: 'O título, a descrição e a imagem são eliminados. A lambda fica exatamente como está.',
  pictureLabel: 'Imagem',
  formats: (limit) => `PNG, JPEG, GIF ou WebP, até ${limit}`,
  notSaved: 'ainda não guardada',
  replace: 'Arrasta uma nova para aqui para a substituir.',
  drop: 'Arrasta uma imagem para aqui.',
  advice: 'Uma captura de ecrã, ou um GIF curto da app a ser usada, fica melhor em 16:10.',
  another: 'Escolher outra',
  choose: 'Escolher um ficheiro',
  keepSaved: 'Manter a guardada',
  clear: 'Limpar',
};
