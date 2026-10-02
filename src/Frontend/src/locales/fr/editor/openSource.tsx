import type { EditorMessages } from '../../en/editor';
import { count } from './language';

export const openSource: EditorMessages['openSource'] = {
  loading: 'Chargement…',
  loadFailed: 'Impossible de savoir si le code est publié.',
  hint: (tool) => (
    <>
      N’importe qui peut lire une lambda publiée sur sa page, dans Open source, lui donner une étoile et la
      télécharger : toutes ses versions, sous la licence de votre choix, et jamais les données qu’elle garde. Seule la
      personne qui a la clé d’édition peut la publier ou la retirer. Un agent peut faire de même avec son outil {tool}.
    </>
  ),
  hintSimple:
    'N’importe qui peut lire comment votre application est faite, sur sa propre page, et s’en servir comme base sous la licence de votre choix – jamais avec ce qu’elle garde. Personne d’autre que vous ne peut la publier ou la retirer.',
  open: 'Ouvrir la page du code',
  switch: 'Publier le code de cette application',
  publishedNow: (license) => `Publié sous ${license}. N’importe qui peut le lire et le télécharger.`,
  off: 'Désactivé. Personne ne voit le code tant que vous ne le publiez pas.',
  keptStars: (stars) =>
    stars < 2
      ? 'Son étoile est conservée pour le jour où vous le publierez à nouveau.'
      : `Ses ${stars} étoiles sont conservées pour le jour où vous le publierez à nouveau.`,
  published: 'Publié. N’importe qui peut maintenant lire le code.',
  saved: 'Enregistré.',
  saveFailed: 'Impossible de publier le code.',
  withdrawn: 'Retiré. Sa page n’existe plus.',
  withdrawFailed: 'Impossible de le retirer.',
  whatTitle: 'Ce qui est publié',
  what: [
    'Son code – tel qu’il est aujourd’hui, et chacun de ses états précédents',
    'Tout ce qu’elle affiche : ses pages, sa mise en forme et ses images',
    'Ce qui est écrit à son sujet : à quoi elle sert, et comment elle est testée',
    'Chaque modification qu’elle a connue, en une ligne chacune',
  ],
  neverTitle: 'Ce qui n’est jamais publié',
  never: [
    'Ce qu’elle garde : ses entrées, ce qu’elle a enregistré, ses clés et mots de passe',
    'Ce que vous avez demandé, avec vos propres mots',
    'Qui l’utilise : ses visiteurs et ce qu’ils ont fait',
    'Le lien d’édition',
  ],
  careful:
    'Tout ce qui est dans le code devient public, ses états précédents aussi. Un mot de passe ou une clé n’a jamais sa place dans le code : sa place est avec les clés et mots de passe, sous Données, qui ne sont jamais publiés.',
  licenseLabel: 'Licence',
  licenseHint: 'Ce que les autres peuvent faire du code. MIT, la plus courante, permet à chacun d’en faire presque tout, tant que votre nom y reste attaché.',
  readLicense: 'Lire la licence',
  authorLabel: 'Nom dans la licence',
  optional: 'facultatif',
  authorPlaceholder: (key) => `Les auteurs de ${key}`,
  authorHint: 'Votre nom ou celui de votre organisation, affiché sur la page du code et dans la licence. Si le champ reste vide, la licence nomme les auteurs de cette application.',
  publish: 'Publier',
  save: 'Enregistrer les modifications',
  allSaved: 'Tout est enregistré.',
  takeDown: 'Le retirer',
  confirm: 'Retirer le code ?',
  confirmText:
    'Sa page et ses téléchargements disparaissent immédiatement. Ceux qui l’ont déjà téléchargé le gardent, sous la licence avec laquelle ils l’ont reçu. Ses étoiles sont conservées pour le jour où vous le publierez à nouveau.',
  keep: 'Le laisser publié',
  stars: (value) => count(value, 'étoile', 'étoiles'),
  sidebar: 'Code source',
};
