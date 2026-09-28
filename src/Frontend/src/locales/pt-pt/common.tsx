import type { Messages } from '../en';

export const shell: Messages['shell'] = {
  main: 'Principal',
  build: 'Criar',
  ship: 'Publicar',
  showcase: 'Montra',
  enterprise: 'Empresas',
  docs: 'Docs',
  admin: 'Admin',
  lightMode: 'Mudar para o modo claro',
  darkMode: 'Mudar para o modo escuro',
  openMenu: 'Abrir o menu',
  closeMenu: 'Fechar o menu',
  language: 'Idioma',
  terms: 'Termos de utilização',
  privacy: 'Política de privacidade',
  imprint: 'Aviso legal',
  writeCode: 'Escrever o código à mão',
  contact: 'Contacto',
};

export const common: Messages['common'] = {
  loading: 'A carregar…',
  loadingEditor: 'A carregar o editor…',
  editorFailed: 'Não foi possível carregar o editor',
  editorFailedWhy: 'Normalmente, isto quer dizer que o site foi atualizado com este separador aberto.',
  reload: 'Recarregar a página',
  backToStart: 'Voltar ao início',
  tryAgain: 'Tentar novamente',
  copy: 'Copiar',
  copied: 'Copiado',
  copyToClipboard: 'Copiar para a área de transferência',
  openInNewTab: 'Abrir num novo separador',
  close: 'Fechar',
  operatorCountry: 'Alemanha',
};

export const notFound: Messages['notFound'] = {
  title: 'Página não encontrada',
  heading: 'Esta página não existe',
  text: 'O link pode estar desatualizado, ou a lambda para onde apontava foi eliminada.',
};

export const missing: Messages['missing'] = {
  title: 'Não há nada a correr aqui',
  heading: 'Não há nada a correr aqui',
  notDeployed: (key) => (
    <>
      Há uma lambda em {key}, mas neste momento não está online. No plano gratuito, os deploys ficam online enquanto
      são usados e são postos offline ao fim de um mês sem visitas nem alterações. Quem tiver o link de edição pode
      voltar a pô-la online.
    </>
  ),
  unknown: (key) => (
    <>Não há nenhuma lambda em {key}. A chave pode nunca ter existido, ou a lambda foi eliminada.</>
  ),
  create: 'Criar uma lambda aqui',
};

export const abuse: Messages['abuse'] = {
  report: 'Denunciar abuso',
  title: 'Denunciar uma lambda',
  write: 'Escreve-nos',
  subject: 'Denúncia de abuso',
  intro:
    'Qualquer pessoa pode pôr código online aqui, e por isso, às vezes, alguém publica o que não devia. Se uma página alojada aqui está a tentar enganar pessoas, a atacar alguma coisa ou a usar material sem ter direito a isso, avisa-nos e nós retiramo-la.',
  how: (mailbox, strong, path) => (
    <>
      Escreve para {mailbox} com {strong('o endereço da página')} (algo como {path}) e uma frase sobre o que está mal.
      Uma captura de ecrã ajuda. Não precisas de ter conta nem de ser utilizador deste site.
    </>
  ),
  next: (strong, terms) => (
    <>
      {strong('O que acontece a seguir.')} Uma pessoa lê a denúncia. Se a lambda violar os{' '}
      {terms('termos de utilização')}, é posta offline, normalmente no prazo de um dia. Não te diremos quem a
      publicou, e não podemos prometer responder a todas as denúncias, mas lemos todas.
    </>
  ),
  danger:
    'Se alguém estiver em perigo imediato, ou se estiver a ser cometido um crime, contacta também as autoridades locais. Nós podemos remover uma página, mas não podemos fazer mais nada.',
};
