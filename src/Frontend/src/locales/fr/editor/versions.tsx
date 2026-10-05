import type { EditorMessages } from '../../en/editor';

export const versions: EditorMessages['versions'] = {
  hint: (limit) =>
    `Une version, c’est le programme (son code et ses assets), et elle ne change plus une fois enregistrée : on peut donc comparer n’importe laquelle et la remettre en ligne exactement telle qu’elle était. Chacune garde ce qui a été demandé et ce qu’elle a changé. Pour modifier la lambda, démarrez un brouillon : il devient la prochaine version une fois au point. Au-delà de ${limit} versions, les plus anciennes sont supprimées ; celle qui est en ligne, jamais.`,
  none: 'Aucune version pour l’instant.',
  noDescription: 'Sans description',
  online: 'en ligne',
  putOnline: 'Mettre cette version en ligne',
  rollBackTitle: 'Remettre en ligne cette ancienne version',
  deploy: 'Déployer',
  rollBack: 'Restaurer',
  readFailed: 'Impossible de lire cette version.',
  comparing: 'Comparaison…',
  unchanged: 'Rien n’a changé depuis la version précédente.',
  first: 'La première version.',
  status: { added: 'ajouté', removed: 'supprimé', changed: 'modifié', same: 'inchangé' },
  groups: {
    code: 'Code',
    assets: 'Assets',
    build: 'Build',
    context: 'Documentation et tests',
  },
  browse: 'Parcourir ses fichiers',
  docs: 'Lire sa documentation',
  build: 'Voir ce à partir de quoi il est construit',
  edit: 'Modifier à partir d’ici',
  feature: 'Démarrer un brouillon à partir d’ici',
  featureTitle:
    'Préparer une modification de cette version à côté de la lambda, et l’intégrer à la prochaine version une fois au point',
  binary: 'Ce n’est pas du texte : pas de lignes à comparer.',
  tooLarge: 'Trop gros pour une comparaison ligne à ligne.',
};
