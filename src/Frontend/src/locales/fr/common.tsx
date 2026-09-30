import type { Messages } from '../en';

export const shell: Messages['shell'] = {
  main: 'Navigation principale',
  build: 'Créer un site',
  ship: 'Publier',
  showcase: 'Vitrine',
  enterprise: 'Entreprise',
  docs: 'Docs',
  admin: 'Admin',
  lightMode: 'Passer en mode clair',
  darkMode: 'Passer en mode sombre',
  openMenu: 'Ouvrir le menu',
  closeMenu: 'Fermer le menu',
  language: 'Langue',
  terms: 'Conditions d’utilisation',
  privacy: 'Politique de confidentialité',
  imprint: 'Mentions légales',
  writeCode: 'Écrire le code vous-même',
  contact: 'Contact',
};

export const common: Messages['common'] = {
  loading: 'Chargement…',
  loadingEditor: 'Chargement de l’éditeur…',
  editorFailed: 'Impossible de charger l’éditeur',
  pageFailed: 'Impossible de charger la page',
  editorFailedWhy: 'En général, c’est que le site a été mis à jour pendant que cet onglet était ouvert.',
  reload: 'Recharger la page',
  backToStart: 'Retour à l’accueil',
  tryAgain: 'Réessayer',
  copy: 'Copier',
  copied: 'Copié',
  copyToClipboard: 'Copier dans le presse-papiers',
  openInNewTab: 'Ouvrir dans un nouvel onglet',
  close: 'Fermer',
  operatorCountry: 'Allemagne',
};

export const notFound: Messages['notFound'] = {
  title: 'Page introuvable',
  heading: 'Cette page n’existe pas',
  text: 'Le lien est peut-être périmé, ou la lambda vers laquelle il menait a été supprimée.',
};

export const missing: Messages['missing'] = {
  title: 'Rien n’est en ligne ici',
  heading: 'Rien n’est en ligne ici',
  notDeployed: (key) => (
    <>
      Il y a bien une lambda à l’adresse {key}, mais elle n’est pas déployée en ce moment. Dans l’offre gratuite, une
      lambda reste en ligne tant qu’on l’utilise, et elle est mise hors ligne après un mois sans visite ni
      modification. Avec le lien d’édition, on peut la remettre en ligne.
    </>
  ),
  unknown: (key) => (
    <>
      Aucune lambda n’est hébergée à l’adresse {key}. Cette clé n’a peut-être jamais existé, ou la lambda a été
      supprimée.
    </>
  ),
  create: 'Créer une lambda ici',
};

export const abuse: Messages['abuse'] = {
  report: 'Signaler un abus',
  title: 'Signaler une lambda',
  write: 'Nous écrire',
  subject: 'Signalement d’abus',
  intro:
    'Ici, n’importe qui peut mettre du code en ligne. Il arrive donc que quelqu’un publie ce qu’il ne devrait pas. Si une page hébergée ici cherche à piéger des gens, attaque quelque chose ou utilise du contenu sans en avoir le droit, dites-le-nous : nous la retirerons.',
  how: (mailbox, strong, path) => (
    <>
      Écrivez à {mailbox} en indiquant {strong('l’adresse de la page')} (elle ressemble à {path}) et une phrase sur le
      problème. Une capture d’écran aide. Pas besoin de compte, ni d’utiliser ce site.
    </>
  ),
  next: (strong, terms) => (
    <>
      {strong('Et ensuite ?')} Une vraie personne lit votre message. Si la lambda enfreint les{' '}
      {terms('conditions d’utilisation')}, elle est mise hors ligne, en général dans la journée. Nous ne vous dirons pas
      qui l’a publiée, et nous ne pouvons pas promettre de répondre à chaque signalement. Mais nous les lisons tous.
    </>
  ),
  danger:
    'Si quelqu’un est en danger immédiat, ou si une infraction est en train d’être commise, contactez aussi les autorités. Nous pouvons retirer une page, rien de plus.',
};
