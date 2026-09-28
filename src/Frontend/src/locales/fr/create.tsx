import type { Messages } from '../en';

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
};
