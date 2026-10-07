import type { EditorMessages } from '../../en/editor';
import { count } from './language';

export const summary: EditorMessages['summary'] = {
  reading: 'Chargement de l’état…',
  readDocs: 'Lire la documentation',
  written: 'Documentation et tests',
  writtenWhy: 'Dans docs/ et tests/ du code : conservés avec chaque version, jamais compilés ni servis.',
  writtenMissing: 'Pas encore rédigé',
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
  inVersion: (version) => `Dans la version ${version}`,
  noVersion: 'Dans la version',
  inData: 'Dans les données',
  sharedByAll: 'Partagées par toutes les versions',
  browse: 'Parcourir',
  versionAllowance: 'Code et ressources',
  dataAllowance: 'Base de données et workspace',
  files: (files, size) => (files === 1 ? `1 fichier, ${size}` : `${files} fichiers, ${size}`),
  code: 'Code',
  codeWhy: 'Jamais servi. Les fichiers .cs à la racine sont compilés ; le reste (documentation, tests, ce à partir de quoi il est construit) est conservé avec la version.',
  resources: 'Ressources',
  resourcesPublic: 'Publiques : le code les sert.',
  resourcesPrivate: 'Non servies par le code.',
  data: 'Données',
  workspace: 'Workspace',
  workspaceOff: 'désactivé',
  dataPublic: 'Publiques : le code sert le workspace.',
  dataPrivate: 'Privées, réservées à la lambda.',
  secrets: 'Secrets',
  secretsOff: 'désactivés',
  secretsCount: (count) => (count === 1 ? '1 secret' : `${count} secrets`),
  secretsMissing: (count) => `${count} manquant${count === 1 ? '' : 's'}`,
  secretsMissingTitle: 'Le code lit des secrets qui ne sont pas définis, et échoue à ces endroits.',
  database: 'Base de données',
  databaseOff: 'désactivée',
  databaseHolds: (tables, size) => (tables === 1 ? `1 table, ${size}` : `${tables} tables, ${size}`),
  databaseOffUsed: 'Le code se connecte à la base de données, qui est désactivée.',
};
