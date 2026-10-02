import type { EditorMessages } from '../../en/editor';
import { count } from './language';

export const logs: EditorMessages['logs'] = {
  readFailed: 'Impossible de lire les logs.',
  hint: (capturing) =>
    'Les requêtes, ce que la lambda affiche et ce qui plante, en direct.' +
    (capturing ? '' : ' Cette installation ne garde pas ce que les lambdas affichent : seules les requêtes et les erreurs apparaissent.') +
    ' Les logs sont gardés en mémoire et partagés par toutes les lambdas de l’installation : ils remontent de quelques minutes à quelques heures, et repartent de zéro à chaque redémarrage. Les adresses des visiteurs ne sont pas affichées.',
  featureHint: (capturing) =>
    'Les réponses de l’aperçu de ce brouillon, ce qu’il affiche et ce qui plante, en direct.' +
    (capturing ? '' : ' Cette installation ne garde pas ce que les lambdas affichent : seules les requêtes et les erreurs apparaissent.') +
    ' Ces logs sont séparés de ceux de la lambda, qui ne montrent jamais l’aperçu. Ils sont gardés en mémoire : ils remontent de quelques minutes à quelques heures.',
  nothingPreview: 'Rien pour l’instant. Ouvrez l’aperçu du brouillon : ses requêtes apparaîtront ici.',
  search: 'Rechercher',
  searchLabel: 'Rechercher dans les logs',
  resume: 'Afficher les nouvelles lignes au fur et à mesure',
  pause: 'Ne plus ajouter de lignes pendant que vous lisez',
  paused: 'En pause',
  live: 'En direct',
  show: 'Afficher',
  all: 'Tout',
  requests: 'Requêtes',
  output: 'Sortie',
  problems: 'Erreurs',
  reading: 'Chargement des logs…',
  noProblems: 'Aucune erreur, du moins dans ce que les logs ont gardé.',
  nothing: 'Rien pour l’instant. Ouvrez l’adresse de la lambda : ses requêtes apparaîtront ici.',
  noMatch: 'Aucun résultat.',
  identical: (value) => count(value, 'ligne identique', 'lignes identiques'),
  at: (domain) => `, sur ${domain}`,
  from: (country) => `, pays : ${country}`,
};
