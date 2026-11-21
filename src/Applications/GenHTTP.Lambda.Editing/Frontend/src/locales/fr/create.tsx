import type { Messages } from '../en/site';

export const create: Messages['create'] = {
  title: 'Créer une lambda',
  whatTitle: 'Que voulez-vous créer ?',
  whatText:
    'Choisissez le modèle le plus proche : vous partez d’une copie de quelque chose qui marche déjà, et vous en faites ce que vous voulez. Ou partez de zéro.',
  seeIt: 'Voir la démo',
  startFrom: 'Partir de ce modèle',
  starters: {
    'demo-crud': {
      title: 'Tenir une liste',
      description: 'Une liste que chacun peut compléter, modifier et cocher : tâches, notes, favoris ou petit inventaire.',
    },
    'demo-registration': {
      title: 'Ouvrir les inscriptions',
      description: 'Des comptes pour s’inscrire et se connecter, et des pages réservées aux membres.',
    },
    'demo-game': {
      title: 'Un jeu à plusieurs',
      description: 'Plusieurs personnes jouent en même temps, en direct dans leur navigateur.',
    },
    'demo-files': {
      title: 'Partager fichiers et photos',
      description: 'Chacun envoie des photos ou des documents, et tout le monde peut les voir.',
    },
    'demo-live': {
      title: 'Suivre en direct',
      description: 'Une page qui se met à jour toute seule dès que quelque chose change : votes, scores, tableau de bord.',
    },
    empty: {
      title: 'Autre chose',
      description: 'Partez d’une lambda vide et créez ce que vous avez en tête.',
    },
  },

  addressTitle: 'Choisissez une adresse',
  fromDemo: (title) => <>{title} – votre lambda démarre comme une copie de la démo, et tout y est modifiable.</>,
  fromNothing: 'Votre lambda démarre vide, prête pour ce que vous avez en tête.',
  pickAgain: 'Choisir autre chose',
  publicKey: 'Clé publique',
  free: (key) => `« ${key} » est disponible.`,
  keyHint: 'Minuscules, chiffres et tirets. Au moins trois caractères. Laissez vide pour une clé aléatoire.',
  accept: 'J’accepte les conditions d’utilisation',
  fullTerms: 'Lire les conditions d’utilisation complètes',
  back: 'Retour',
  creating: 'Création…',
  submit: 'Créer ma lambda',
  keepLink: 'L’écran suivant affiche votre lien d’édition. C’est le seul moyen d’y revenir : gardez-le.',
  failed: 'Impossible de créer la lambda.',
  termsShort:
    'Les lambdas tournent sur une infrastructure partagée. En créant une lambda, vous vous engagez à ne pas déployer de logiciels malveillants, de pages de phishing, de mineurs de cryptomonnaie, ni rien qui attaque, scanne ou inonde de requêtes d’autres systèmes, et à ne pas publier de contenu que vous n’avez pas le droit de publier. Toute personne qui connaît le lien d’édition peut modifier votre lambda : traitez-le comme un mot de passe. Dans l’offre gratuite, une lambda reste en ligne tant qu’elle est utilisée. Sans visite ni modification pendant un mois, elle est mise hors ligne, puis supprimée s’il ne se passe rien pendant les deux mois suivants. Tout ce que vous déployez peut être supprimé à tout moment.',
};
