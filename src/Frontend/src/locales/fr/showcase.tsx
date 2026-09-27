import type { Messages } from '../en';

export const showcase: Messages['showcase'] = {
  eyebrow: 'Vitrine',
  title: 'Créées ici, en ligne maintenant',
  intro:
    'Des lambdas que leurs propriétaires ont choisi de présenter. Tous sont en ligne : chaque carte ouvre l’application réelle. Les plus utilisés récemment apparaissent en premier.',
  counted: (total) => (total === 1 ? '1 lambda' : `${total} lambdas`),
  failed: 'La vitrine n’a pas pu être chargée.',
  loadingMore: 'Chargement…',
  showMore: 'Afficher davantage',
  nothingTitle: 'Aucune application pour le moment',
  nothing: (tab) => (
    <>
      Vous avez créé une application fonctionnelle ? Ouvrez son centre de contrôle, choisissez {tab('Vitrine')}, puis
      ajoutez un titre, une courte description et une image. Elle apparaîtra ici tant qu’elle est en ligne.
    </>
  ),
  buildOne: 'Créer une application',
  yoursTitle: 'Présenter votre application',
  yours: (tab) => (
    <>
      Ouvrez le centre de contrôle de votre lambda et choisissez {tab('Vitrine')}, ou demandez à l’agent qui l’a créé de
      l’y ajouter. Seul le détenteur de la clé d’édition peut le faire, et l’application peut être retirée à tout moment.
    </>
  ),
  buildSomething: 'Créer une application',
};

export const card: Messages['card'] = {
  noPicture: 'Pas encore d’image',
  title: 'Titre',
  description: 'Ce que les visiteurs peuvent en faire.',
  opens: (title, address) => `${title}, ouvre ${address} dans un nouvel onglet`,
};
