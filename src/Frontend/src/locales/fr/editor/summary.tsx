import type { EditorMessages } from '../../en/editor';
import { count } from './language';

export const summary: EditorMessages['summary'] = {
  reading: 'Chargement de l’état…',
  readDocs: 'Lire la documentation',
  hint: (since, kept, retention, tier) =>
    `Le trafic est compté depuis le dernier démarrage du serveur (${since}). ` +
    (kept
      ? `Une lambda reste en ligne tant qu’on l’utilise, et elle est supprimée après ${retention} jours sans visite ni modification.`
      : `Cette lambda est dans l’offre ${tier}, qui la garde en ligne et stockée, même sans aucune activité.`),
  onlineFor: (duration, version) => (
    <>
      En ligne depuis {duration('un moment')}, sert la version {version}.
    </>
  ),
  offline: 'Hors ligne. Rien n’est servi tant qu’aucune version n’est déployée.',
  nothing: 'Rien n’a encore été écrit.',
  requestsToday: 'requêtes aujourd’hui',
  lastHour: (count) => `${count} dans la dernière heure`,
  hourly: 'Requêtes par heure sur les dernières 24 heures',
  failed: 'en échec',
  failedTitle: (failed, rejected) =>
    `${count(failed, 'erreur serveur', 'erreurs serveur')}, ${count(rejected, 'requête introuvable ou refusée', 'requêtes introuvables ou refusées')}, sur les dernières 24 heures`,
  average: 'temps de réponse moyen',
  noneYet: 'pas encore',
  lastVisit: 'dernière visite',
  problems: 'Il y a eu des erreurs récemment',
  openLog: 'Ouvrir les logs',
  latest: 'Dernière modification',
  allVersions: 'Toutes les versions',
  noDescription: 'Sans description',
  version: (version) => `Version ${version}`,
  notOnline: 'pas encore en ligne',
  wanted: 'Ce qui était demandé',
  noVersions: 'Aucune version pour l’instant.',
  inProgress: 'En cours',
  allFeatures: 'Tous les brouillons',
  previewOnline: 'Son aperçu est en ligne',
  previewOffline: 'Son aperçu est hors ligne',
  behind: 'pas à jour',
  storage: 'Stockage',
  versionAllowance: 'Code et ressources',
  data: 'Données',
};
