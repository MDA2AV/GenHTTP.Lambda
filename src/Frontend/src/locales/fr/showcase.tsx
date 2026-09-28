import type { Messages } from '../en';

export const showcase: Messages['showcase'] = {
  eyebrow: 'Vitrine',
  title: 'Créées ici, en ligne en ce moment',
  intro:
    'Des lambdas que leurs auteurs ont choisi de montrer. Toutes sont en ligne : chaque carte ouvre la vraie app. Les plus utilisées récemment viennent en premier.',
  counted: (total) => (total === 1 ? '1 lambda' : `${total} lambdas`),
  failed: 'Impossible de charger la vitrine.',
  loadingMore: 'Chargement…',
  showMore: 'Afficher plus',
  nothingTitle: 'Rien en vitrine pour l’instant',
  nothing: (tab) => (
    <>
      Vous avez créé quelque chose qui marche ? Ouvrez son tableau de bord, choisissez {tab('Vitrine')}, puis ajoutez
      un titre, quelques mots et une image. Votre app apparaîtra ici tant qu’elle est en ligne.
    </>
  ),
  buildOne: 'Créer une app',
  yoursTitle: 'Et la vôtre ?',
  yours: (tab) => (
    <>
      Ouvrez le tableau de bord de votre lambda et choisissez {tab('Vitrine')}, ou demandez à l’agent qui l’a créée de
      l’y ajouter. Il faut pour cela la clé d’édition, et vous pouvez retirer votre lambda de la vitrine à tout moment.
    </>
  ),
  buildSomething: 'Créer une app',
};

export const card: Messages['card'] = {
  noPicture: 'Pas encore d’image',
  title: 'Titre',
  description: 'Ce qu’un visiteur peut en faire.',
  opens: (title, address) => `${title}, ouvre ${address} dans un nouvel onglet`,
};
