import type { EditorMessages } from '../../en/editor';

export const showcase: EditorMessages['showcase'] = {
  loadFailed: 'Não foi possível carregar a vitrine.',
  loading: 'Carregando…',
  title: 'um título',
  description: 'uma descrição',
  picture: 'uma imagem',
  updated: 'Vitrine atualizada.',
  listed: 'Já está na vitrine.',
  waiting: 'Salvo. Aparece na vitrine assim que a lambda estiver no ar.',
  saveFailed: 'Não foi possível salvar na vitrine.',
  removed: 'Tirado da vitrine.',
  removeFailed: 'Não foi possível tirar da vitrine.',
  wrongType: 'Isso não é uma imagem PNG, JPEG, GIF ou WebP.',
  tooLarge: (size, limit) => `Essa imagem tem ${size}; o máximo é ${limit}.`,
  unreadable: 'Não foi possível ler esse arquivo.',
  hint: (tool) => (
    <>
      A vitrine lista as lambdas que os donos escolheram mostrar, as mais usadas ultimamente primeiro. Só quem tem a
      chave de edição pode colocar ou tirar uma lambda de lá, e ela só aparece enquanto está no ar. Um agente pode
      fazer o mesmo com a ferramenta {tool}.
    </>
  ),
  open: 'Abrir a vitrine',
  switch: 'Mostrar esta lambda na vitrine',
  listedNow: 'Na vitrine. Qualquer pessoa que passar por lá pode abrir.',
  notListed: 'Salvo, mas fora da vitrine: a lambda está fora do ar. Ela volta a aparecer depois do próximo deploy.',
  off: 'Desligado. Nada sobre esta lambda aparece em lugar nenhum até você ligar isto e salvar.',
  offline: 'A lambda está fora do ar, então só vai aparecer na vitrine depois do próximo deploy. Só entram lambdas que respondem.',
  titleLabel: 'Título',
  titlePlaceholder: 'Placar do quiz do bar',
  descriptionLabel: 'Descrição',
  descriptionPlaceholder:
    'As equipes mandam as respostas pelo celular, o apresentador corrige e o placar atualiza para todo mundo na sala.',
  save: 'Salvar mudanças',
  add: 'Adicionar à vitrine',
  takeOff: 'Tirar da vitrine',
  needs: (missing) =>
    missing.length > 1
      ? `Ainda faltam ${missing.slice(0, -1).join(', ')} e ${missing[missing.length - 1]}.`
      : `Ainda falta ${missing[0]}.`,
  tooLong: 'Algum campo está longo demais.',
  allSaved: 'Tudo salvo.',
  preview: 'Prévia',
  card: (address) => <>Este é o card que os visitantes veem. Ele abre {address}.</>,
  confirm: 'Tirar da vitrine?',
  keep: 'Manter',
  confirmText: 'O título, a descrição e a imagem são apagados. A lambda em si continua exatamente como está.',
  pictureLabel: 'Imagem',
  formats: (limit) => `PNG, JPEG, GIF ou WebP, até ${limit}`,
  notSaved: 'ainda não salva',
  replace: 'Solte uma nova aqui para substituir.',
  drop: 'Solte uma imagem aqui.',
  advice: 'Uma captura de tela, ou um GIF curto do app em uso, fica melhor em 16:10.',
  another: 'Escolher outra',
  choose: 'Escolher um arquivo',
  keepSaved: 'Manter a salva',
  clear: 'Limpar',
};
