import type { Messages } from '../en';

export const create: Messages['create'] = {
  title: 'Créer un lambda',
  whatTitle: 'Que souhaitez-vous créer ?',
  whatText:
    'Choisissez le modèle le plus proche : vous partez d’une copie d’une application fonctionnelle, entièrement modifiable. Vous pouvez également partir de zéro.',
  seeIt: 'Voir en fonctionnement',
  startFrom: 'Utiliser ce modèle',
  starters: {
    'demo-crud': {
      title: 'Gérer des éléments',
      description: 'Une liste que l’on complète, modifie et coche – tâches, notes, favoris ou petit inventaire.',
    },
    'demo-registration': {
      title: 'Inscription et connexion',
      description: 'Des comptes permettant de s’inscrire et de se connecter, et des pages réservées aux membres.',
    },
    'demo-game': {
      title: 'Un jeu multijoueur',
      description: 'Une application utilisée simultanément par plusieurs personnes, en direct dans leur navigateur.',
    },
    'demo-files': {
      title: 'Partager des fichiers et des images',
      description: 'Des images ou documents téléversés, visibles ensuite par tous.',
    },
    'demo-live': {
      title: 'Mises à jour en direct',
      description: 'Une page qui se met à jour automatiquement à chaque changement – votes, scores, tableau de bord.',
    },
    empty: {
      title: 'Lambda vide',
      description: 'Partez d’un lambda vide et réalisez votre propre projet.',
    },
  },

  addressTitle: 'Choisir une adresse',
  fromDemo: (title) => <>{title} – votre lambda démarre comme une copie de la démo, entièrement modifiable.</>,
  fromNothing: 'Votre lambda démarre vide, prêt pour votre projet.',
  pickAgain: 'Choisir un autre modèle',
  publicKey: 'Clé publique',
  free: (key) => `« ${key} » est disponible.`,
  keyHint: 'Lettres minuscules, chiffres et tirets, trois caractères minimum. Laissez vide pour une clé aléatoire.',
  accept: 'J’accepte les conditions d’utilisation',
  fullTerms: 'Lire les conditions d’utilisation complètes',
  back: 'Retour',
  creating: 'Création…',
  submit: 'Créer le lambda',
  keepLink: 'L’écran suivant affiche votre lien d’édition. C’est le seul moyen d’accès : conservez-le précieusement.',
  failed: 'Le lambda n’a pas pu être créé.',
};
