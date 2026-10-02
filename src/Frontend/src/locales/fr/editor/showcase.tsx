import type { EditorMessages } from '../../en/editor';

export const showcase: EditorMessages['showcase'] = {
  loadFailed: 'Impossible de charger la fiche vitrine.',
  loading: 'Chargement…',
  title: 'un titre',
  description: 'une description',
  picture: 'une image',
  updated: 'La fiche vitrine est à jour.',
  listed: 'Elle est maintenant sur la page Vitrine.',
  waiting: 'Enregistré. La fiche apparaîtra dans la vitrine dès que la lambda sera en ligne.',
  saveFailed: 'Impossible d’enregistrer la fiche vitrine.',
  removed: 'Retirée de la vitrine.',
  removeFailed: 'Impossible de retirer la fiche vitrine.',
  wrongType: 'Ce n’est pas une image PNG, JPEG, GIF ou WebP.',
  tooLarge: (size, limit) => `Ce fichier fait ${size} ; une image ne peut pas dépasser ${limit}.`,
  unreadable: 'Impossible de lire ce fichier.',
  hint: (tool) => (
    <>
      La vitrine liste les lambdas que leurs auteurs ont choisi de montrer, les plus utilisées récemment en premier.
      Seule la personne qui a la clé d’édition peut y ajouter une lambda ou l’en retirer, et la lambda n’y figure que
      tant qu’elle est en ligne. Un agent peut faire de même avec son outil {tool}.
    </>
  ),
  open: 'Ouvrir la vitrine',
  switch: 'Montrer cette lambda dans la vitrine',
  listedNow: 'Publiée dans la vitrine. N’importe quel visiteur peut l’ouvrir.',
  notListed: 'Enregistré, mais pas en vitrine : la lambda est hors ligne. Elle réapparaîtra dès qu’elle sera redéployée.',
  off: 'Désactivé. Cette lambda n’apparaît nulle part tant que vous n’activez pas cette option et n’enregistrez pas.',
  offline: 'La lambda est hors ligne : la fiche attendra son déploiement. Seules les lambdas qui répondent sont listées.',
  titleLabel: 'Titre',
  titlePlaceholder: 'Scores du quiz au bar',
  descriptionLabel: 'Description',
  descriptionPlaceholder:
    'Les équipes entrent leurs réponses sur leur téléphone, l’animateur les corrige, et le tableau des scores se met à jour pour toute la salle.',
  save: 'Enregistrer les modifications',
  add: 'Ajouter à la vitrine',
  takeOff: 'Retirer',
  needs: (missing) =>
    `Il manque encore ${missing.length > 1 ? `${missing.slice(0, -1).join(', ')} et ${missing[missing.length - 1]}` : missing[0]}.`,
  tooLong: 'Certains champs sont trop longs.',
  allSaved: 'Tout est enregistré.',
  preview: 'Aperçu',
  card: (address) => <>Voici la carte que voient les visiteurs. Elle ouvre {address}.</>,
  confirm: 'Retirer de la vitrine ?',
  keep: 'La garder',
  confirmText: 'Le titre, la description et l’image seront supprimés. La lambda elle-même reste telle quelle.',
  pictureLabel: 'Image',
  formats: (limit) => `PNG, JPEG, GIF ou WebP, ${limit} max.`,
  notSaved: 'pas encore enregistrée',
  replace: 'Déposez-en une nouvelle ici pour la remplacer.',
  drop: 'Déposez une image ici.',
  advice: 'Une capture d’écran, ou un court GIF de l’app en action, de préférence au format 16:10.',
  another: 'En choisir une autre',
  choose: 'Choisir un fichier',
  keepSaved: 'Garder l’image enregistrée',
  clear: 'Effacer',
};
