import type { Messages } from '../en';

export const shell: Messages['shell'] = {
  main: 'Navegação principal',
  build: 'Criar',
  ship: 'Publicar',
  showcase: 'Vitrine',
  enterprise: 'Empresas',
  docs: 'Documentação',
  admin: 'Admin',
  lightMode: 'Mudar para o tema claro',
  darkMode: 'Mudar para o tema escuro',
  openMenu: 'Abrir o menu',
  closeMenu: 'Fechar o menu',
  language: 'Idioma',
};

export const common: Messages['common'] = {
  loading: 'Carregando…',
  loadingEditor: 'Carregando o editor…',
  editorFailed: 'Não foi possível carregar o editor',
  editorFailedWhy: 'Normalmente isso significa que o site foi atualizado enquanto esta aba estava aberta.',
  reload: 'Recarregar a página',
  backToStart: 'Voltar ao início',
  tryAgain: 'Tentar novamente',
  copy: 'Copiar',
  copied: 'Copiado',
  copyToClipboard: 'Copiar para a área de transferência',
  openInNewTab: 'Abrir em uma nova aba',
  close: 'Fechar',
};

export const notFound: Messages['notFound'] = {
  title: 'Página não encontrada',
  heading: 'Esta página não existe',
  text: 'O link pode estar desatualizado, ou o lambda para o qual ele apontava foi excluído.',
};

export const missing: Messages['missing'] = {
  title: 'Nada em execução neste endereço',
  heading: 'Nada em execução neste endereço',
  notDeployed: (key) => (
    <>
      Existe um lambda em {key}, mas ele não está implantado no momento. No plano gratuito, as implantações permanecem no
      ar enquanto são utilizadas e são retiradas após um mês sem acessos ou alterações. Quem possui o link de edição pode
      colocá-lo no ar novamente.
    </>
  ),
  unknown: (key) => (
    <>
      Nenhum lambda está hospedado em {key}. A chave pode nunca ter existido, ou o lambda correspondente foi excluído.
    </>
  ),
  create: 'Criar um lambda',
};

export const abuse: Messages['abuse'] = {
  report: 'Denunciar abuso',
  title: 'Denunciar um lambda',
  write: 'Enviar e-mail',
  subject: 'Denúncia de abuso',
  intro:
    'Qualquer pessoa pode publicar código nesta plataforma, e por isso eventualmente surgem conteúdos indevidos. Se uma página hospedada aqui tenta enganar pessoas, ataca outros sistemas ou utiliza material sem os devidos direitos, informe-nos e nós a retiraremos do ar.',
  how: (mailbox, strong, path) => (
    <>
      Escreva para {mailbox} informando {strong('o endereço da página')} – no formato {path} – e uma breve descrição do
      problema. Uma captura de tela é útil. Não é necessário ter uma conta nem ser usuário deste site.
    </>
  ),
  next: (strong, terms) => (
    <>
      {strong('Próximos passos.')} Cada denúncia é analisada por uma pessoa. Se o lambda violar os{' '}
      {terms('termos de serviço')}, ele é retirado do ar, normalmente em até um dia. Não divulgamos quem o publicou e não
      podemos responder a todas as denúncias, mas todas são lidas.
    </>
  ),
  danger:
    'Se alguém estiver em perigo imediato ou um crime estiver sendo cometido, entre em contato também com as autoridades competentes. Podemos remover uma página, mas não podemos tomar outras providências.',
};
