import type { EditorMessages } from '../../en/editor';
import { count } from './language';

export const features: EditorMessages['features'] = {
  hint:
    'Un brouillon est une copie de votre app sur laquelle essayer une modification avant que quiconque la voie, avec sa propre adresse et ses propres données de test. Mettez-le en ligne quand il est au point ; d’ici là, vos visiteurs continuent de recevoir ce qui est en ligne actuellement.',
  newFeature: 'Nouveau brouillon',
  full: (limit) =>
    `Il y a déjà ${count(limit, 'brouillon', 'brouillons')}, le maximum possible. Mettez-en un en ligne ou supprimez-le d’abord.`,
  emptyTitle: 'Aucun brouillon',
  emptyText:
    'Un brouillon est une copie de votre app pour essayer une modification avant sa mise en ligne. Quand l’agent vous laisse une modification à essayer, vous la trouvez ici.',
  start: 'Nouveau brouillon',
  askAgentNew: 'Demander une modification à l’agent',
  noChange: 'Rien n’indique encore ce qu’il change',
  behindTitle: 'Votre app a changé depuis le début de ce brouillon',
  behind: () => 'pas à jour',
  branchTitle: 'La branche de ce brouillon dans le dépôt git de l’app',
  previewOnline: 'aperçu actif',
  previewOutdated: 'l’aperçu montre un enregistrement antérieur',
  previewOffline: 'aperçu inactif',
  changed: 'modifié',
  openPreview: 'Essayer',
  openPreviewTitle: 'Ouvrir l’aperçu dans un nouvel onglet',
  count: (open, limit) => `${open} sur ${limit} brouillons`,
  loading: 'Chargement du brouillon…',
  readFailed: 'Impossible de lire le brouillon.',

  newTitle: 'Nouveau brouillon',
  newText:
    'Une copie de votre app et de ses données, avec sa propre adresse. Modifiez-la et essayez-la là : vos visiteurs n’en voient rien tant que vous ne l’avez pas mise en ligne.',
  newTextFiles:
    'Ce que vous avez tapé va dans le brouillon au lieu de devenir une version : vous pouvez l’essayer à sa propre adresse avant sa mise en ligne.',
  name: 'Nom',
  namePlaceholder: 'Classement',
  wanted: 'Que doit-il faire ?',
  wantedPlaceholder: 'Facultatif. Garder les dix meilleurs scores et les afficher après chaque partie.',
  olderBase: (newest) =>
    `Ce brouillon part d’une version plus ancienne : il n’est donc pas à jour dès le départ. Avant de pouvoir le mettre en ligne, il faut y reporter ce qui a changé jusqu’à la version ${newest}.`,
  create: 'Démarrer le brouillon',
  createFailed: 'Impossible de démarrer le brouillon.',
  retry: 'Réessayer',
  madeNotSaved: (name) =>
    `Le brouillon « ${name} » est démarré, mais ce que vous avez tapé n’a pas encore pu y être mis. Réessayez, ou fermez cette fenêtre et retrouvez le brouillon dans Brouillons.`,
  created: (name) => `Le brouillon « ${name} » est démarré.`,
  cancel: 'Annuler',

  featureHint:
    'Une copie de votre app pour essayer cette modification. Son aperçu a sa propre adresse et ses propres données de test : vos visiteurs n’en voient rien tant que vous ne mettez pas le brouillon en ligne.',
  askAgent: 'Demander à l’agent',
  askCatchUp: 'Demander à l’agent de le mettre à jour',
  catchUp: 'Mets ce brouillon à jour avec la version la plus récente de l’app, en gardant ce qu’il change.',
  editCode: 'Modifier le code',
  deployPreview: 'Démarrer l’aperçu',
  updatePreview: 'Mettre à jour l’aperçu',
  previewDeployed: 'L’aperçu est démarré.',
  previewFailed: 'Impossible de démarrer l’aperçu.',
  previewStopped: 'L’aperçu est arrêté.',
  previewRejected: 'L’aperçu n’a pas changé',
  previewNotCompiling: 'Le code ne compile pas : l’aperçu montre donc toujours la dernière version qui fonctionnait.',
  started: 'Démarré',
  changes: () => 'Fichiers modifiés',
  noChanges: () => 'Rien n’a encore été modifié.',
  editNotes: 'Nom et notes',
  what: 'Qu’est-ce qu’il change ?',
  whatPlaceholder: 'Ajoute un classement qui garde les dix meilleurs scores',
  missed: () => 'Ce qui a changé dans votre app depuis son début',
  missedNothing: 'Rien dans les fichiers.',

  behindText: (_base, newest) =>
    `La version ${newest} de votre app a été enregistrée après le début de ce brouillon. Le mettre en ligne maintenant annulerait ce qu’elle a changé : il faut d’abord le mettre à jour, et l’agent peut s’en charger pour vous.`,
  moveBase: 'Marquer comme à jour',
  close: 'Fermer',
  mergeTitle: (name) => `Mettre « ${name} » en ligne`,
  mergeTitleShort: 'En faire la nouvelle version de votre app et la mettre en ligne',
  leaks: (path, files) =>
    `Dans ${files}, des liens mènent vers ${path}, c’est-à-dire votre app en ligne. Depuis l’aperçu, ces liens lisent et modifient ses vraies données au lieu des données de test. Demandez à l’agent de lier sans cette partie (« api/items »).`,
  mergeButton: 'Mettre en ligne',
  saveFirst: 'Enregistrez d’abord vos modifications : l’aperçu et la mise en ligne utilisent ce qui est enregistré.',
  mergeAndDeploy: () => 'Mettre en ligne',
  mergeText: (version) =>
    `Il devient la version ${version} de votre app et passe en ligne. Les données de votre app restent telles quelles.`,
  deployTooNote: (active) => `La version ${active} reste à un clic, dans les versions.`,
  deployTooOffline: 'Votre app est hors ligne pour l’instant ; ceci la met en ligne.',
  notCompiling: 'Le code ne compile pas, donc rien n’est passé en ligne. Corrigez-le d’abord dans le brouillon.',
  mergeFailed: 'Impossible de mettre le brouillon en ligne.',
  merged: (version) => `Enregistré en version ${version}.`,
  mergedOnline: (version) => `La version ${version} est en ligne.`,

  notesTitle: 'Nom et notes',
  save: 'Enregistrer',
  saveFailed: 'Impossible d’enregistrer.',

  baseTitle: 'Marquer comme à jour ?',
  baseText: () =>
    'Seul un brouillon qui contient ce que la version la plus récente a changé peut être mis en ligne sans l’annuler. Si ces modifications sont maintenant dans ce brouillon, que vous les ayez reportées vous-même ou que l’agent l’ait fait, marquez-le comme à jour.',
  moveTo: () => 'Marquer comme à jour',
  baseWarning:
    'Rien ne le vérifie. Si les modifications ne sont pas dans le brouillon, le mettre en ligne les annule.',

  deleteTitle: (name) => `Supprimer « ${name} » ?`,
  deleteText:
    'Son code, son aperçu et ses données de test sont supprimés définitivement. Votre app et ses versions ne sont pas touchées.',
  keep: 'Le garder',
  deleteForGood: 'Supprimer définitivement',
  deleteFailed: 'Impossible de supprimer le brouillon.',
  deleted: (name) => `Le brouillon « ${name} » est supprimé.`,

  all: 'Tous les brouillons',
  actions: 'Plus d’actions pour ce brouillon',
  download: 'Télécharger en zip',
  stopPreview: 'Arrêter l’aperçu',
  delete: 'Supprimer ce brouillon',
  viewsLabel: 'Le brouillon',
  views: {
    overview: 'Brouillon',
    docs: 'Documentation',
    code: 'Code',
    build: 'Build',
    tests: 'Tests',
    data: 'Données de test',
    logs: 'Logs',
  },
  missingTitle: 'Ce brouillon n’existe plus',
  missingText: 'Il a été mis en ligne ou supprimé. Les versions montrent ce qu’il est devenu.',
};
