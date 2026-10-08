import type { Messages } from '../en';

export const shell: Messages['shell'] = {
  main: 'Principal',
  build: 'Criar site',
  ship: 'Publicar',
  showcase: 'Vitrine',
  enterprise: 'Empresas',
  docs: 'Docs',
  admin: 'Admin',
  lightMode: 'Mudar para o tema claro',
  darkMode: 'Mudar para o tema escuro',
  openMenu: 'Abrir o menu',
  closeMenu: 'Fechar o menu',
  language: 'Idioma',
  terms: 'Termos de uso',
  privacy: 'Política de privacidade',
  imprint: 'Aviso legal',
  writeCode: 'Escrever o código você mesmo',
  contact: 'Contato',
};

export const common: Messages['common'] = {
  loading: 'Carregando…',
  loadingEditor: 'Carregando o editor…',
  editorFailed: 'O editor não carregou',
  pageFailed: 'A página não carregou',
  editorFailedWhy: 'Normalmente é porque o site foi atualizado com esta aba aberta.',
  reload: 'Recarregar a página',
  backToStart: 'Voltar ao início',
  tryAgain: 'Tentar de novo',
  copy: 'Copiar',
  copied: 'Copiado',
  copyToClipboard: 'Copiar para a área de transferência',
  openInNewTab: 'Abrir em nova aba',
  close: 'Fechar',
  operatorCountry: 'Alemanha',
};

export const notFound: Messages['notFound'] = {
  title: 'Página não encontrada',
  heading: 'Esta página não existe',
  text: 'O link pode estar desatualizado, ou a lambda para onde ele apontava foi excluída.',
};

export const abuse: Messages['abuse'] = {
  report: 'Denunciar abuso',
  title: 'Denunciar uma lambda',
  write: 'Escreva para a gente',
  subject: 'Denúncia de abuso',
  intro:
    'Qualquer pessoa pode colocar código no ar aqui, e às vezes alguém publica o que não devia. Se uma página hospedada aqui está tentando enganar pessoas, atacando alguma coisa ou usando material sem autorização, avise a gente e nós tiramos do ar.',
  how: (mailbox, strong, path) => (
    <>
      Escreva para {mailbox} com {strong('o endereço da página')} (algo como {path}) e uma frase sobre o problema. Uma
      captura de tela ajuda. Você não precisa ter conta nem usar este site.
    </>
  ),
  next: (strong, terms) => (
    <>
      {strong('O que acontece depois.')} Uma pessoa lê a denúncia. Se a lambda violar os {terms('termos de uso')}, ela
      sai do ar, normalmente em até um dia. Não contamos quem publicou, e não podemos prometer responder a cada
      denúncia, mas todas são lidas.
    </>
  ),
  danger:
    'Se alguém estiver em perigo imediato, ou se um crime estiver acontecendo, procure também as autoridades locais. Podemos remover a página, mas não podemos fazer mais nada.',
};
