import type { EditorMessages } from '../../en/editor';

export const clone: EditorMessages['clone'] = {
  button: 'Cloner',
  title: 'Cloner avec git',
  intro:
    'Travaillez dessus avec vos propres outils et votre agent de code : le dépôt est le projet tel qu’il tourne, chaque version un commit de main et chaque brouillon une branche.',
  keyWarning:
    'L’adresse contient le lien d’édition : qui la possède peut modifier l’app. Ne l’incluez pas dans ce que vous partagez.',
  draft: (branch) => <>Ce brouillon est la branche {branch}.</>,
  pushing: 'Pousser',
  toMain: (deploy) => (
    <>
      Un commit poussé sur main devient la prochaine version, pas encore en ligne : {deploy} la met en ligne avec le
      push.
    </>
  ),
  toBranch: 'Une branche poussée devient un brouillon, dont l’aperçu est en ligne à sa propre adresse.',
  agents: (file) => <>{file} dans le dépôt explique le reste à un agent de code.</>,
  readOnly:
    'Une démo est en lecture seule : clonez-la pour la lire, et créez-en une lambda à vous pour la modifier.',
  copy: 'Copier',
  copied: 'Copié',
};
