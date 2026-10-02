import type { EditorMessages } from '../../en/editor';
import { count } from './language';

export const stats: EditorMessages['stats'] = {
  readFailed: 'Impossible de lire les chiffres.',
  range: 'Période',
  lastHour: 'Dernière heure',
  lastDay: 'Dernières 24 heures',
  hint: (since) =>
    `Chiffres gardés en mémoire depuis le dernier démarrage du serveur (${since}). Un redémarrage les remet à zéro.`,
  reading: 'Chargement des chiffres…',
  requests: 'requêtes',
  websockets: (value) => `et ${count(value, 'connexion WebSocket', 'connexions WebSocket')}`,
  failed: 'en échec',
  serverErrors: (value) => count(value, 'erreur serveur', 'erreurs serveur'),
  rejected: 'introuvables ou refusées',
  average: 'temps de réponse moyen',
  sent: (amount) => `${amount} envoyés`,
  nobody: (hour) => (hour ? 'Aucune requête dans la dernière heure.' : 'Aucune requête ces dernières 24 heures.'),
  requestsTitle: 'Requêtes',
  per: (hour) => (hour ? 'Par minute.' : 'Par tranche de 15 minutes.'),
  answered: 'Traitées',
  rejectedSeries: 'Introuvables ou refusées',
  failedSeries: 'En échec',
  timeTitle: 'Temps de réponse',
  averagePer: (hour) => (hour ? 'Moyenne par minute.' : 'Moyenne par tranche de 15 minutes.'),
  averageSeries: 'Moyenne',
  mostAsked: 'Les plus demandés',
  path: 'Chemin',
  requestsColumn: 'Requêtes',
  failedColumn: 'Échecs',
  averageColumn: 'Moyenne',
  since: 'Depuis le démarrage du serveur.',
};
