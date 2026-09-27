import type { Messages } from '../en';

export const shell: Messages['shell'] = {
  main: 'Navigation principale',
  build: 'Créer',
  ship: 'Publier',
  showcase: 'Vitrine',
  enterprise: 'Entreprise',
  docs: 'Documentation',
  admin: 'Admin',
  lightMode: 'Passer au thème clair',
  darkMode: 'Passer au thème sombre',
  openMenu: 'Ouvrir le menu',
  closeMenu: 'Fermer le menu',
  language: 'Langue',
};

export const common: Messages['common'] = {
  loading: 'Chargement…',
  loadingEditor: 'Chargement de l’éditeur…',
  editorFailed: 'L’éditeur n’a pas pu être chargé',
  editorFailedWhy: 'Le site a généralement été mis à jour pendant que cet onglet était ouvert.',
  reload: 'Recharger la page',
  backToStart: 'Retour à l’accueil',
  tryAgain: 'Réessayer',
  copy: 'Copier',
  copied: 'Copié',
  copyToClipboard: 'Copier dans le presse-papiers',
  openInNewTab: 'Ouvrir dans un nouvel onglet',
  close: 'Fermer',
};

export const notFound: Messages['notFound'] = {
  title: 'Page introuvable',
  heading: 'Cette page n’existe pas',
  text: 'Le lien est peut-être obsolète, ou le lambda vers lequel il pointait a été supprimé.',
};

export const missing: Messages['missing'] = {
  title: 'Aucune application à cette adresse',
  heading: 'Aucune application à cette adresse',
  notDeployed: (key) => (
    <>
      Un lambda existe à l’adresse {key}, mais il n’est pas déployé actuellement. Dans l’offre gratuite, les déploiements
      restent en ligne tant qu’ils sont utilisés et sont retirés après un mois sans visite ni modification. La personne
      disposant du lien d’édition peut le remettre en ligne.
    </>
  ),
  unknown: (key) => (
    <>
      Aucun lambda n’est hébergé à l’adresse {key}. Cette clé n’a peut-être jamais existé, ou le lambda correspondant a
      été supprimé.
    </>
  ),
  create: 'Créer un lambda',
};

export const abuse: Messages['abuse'] = {
  report: 'Signaler un abus',
  title: 'Signaler un lambda',
  write: 'Nous écrire',
  subject: 'Signalement d’abus',
  intro:
    'Toute personne peut publier du code sur cette plateforme, et il arrive que des contenus inappropriés y soient mis en ligne. Si une page hébergée ici cherche à tromper, attaque d’autres systèmes ou utilise des contenus sans en détenir les droits, merci de nous en informer : nous la retirerons.',
  how: (mailbox, strong, path) => (
    <>
      Écrivez à {mailbox} en indiquant {strong('l’adresse de la page')} – de la forme {path} – ainsi qu’une brève
      description du problème. Une capture d’écran est utile. Aucun compte n’est nécessaire, et vous n’avez pas besoin
      d’utiliser cette plateforme.
    </>
  ),
  next: (strong, terms) => (
    <>
      {strong('Suite donnée.')} Chaque signalement est examiné par une personne. Si le lambda enfreint les{' '}
      {terms('conditions d’utilisation')}, il est généralement retiré dans la journée. Nous ne communiquons pas
      l’identité de son auteur et ne pouvons pas répondre à chaque signalement, mais tous sont lus.
    </>
  ),
  danger:
    'Si une personne est en danger immédiat ou si une infraction est en cours, veuillez également contacter les autorités compétentes. Nous pouvons retirer une page, mais ne pouvons prendre aucune autre mesure.',
};
