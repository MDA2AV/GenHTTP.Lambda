import type { EditorMessages } from '../../en/editor';

export const deployments: EditorMessages['deployments'] = {
  hint: (until) =>
    `Un déploiement reste en ligne tant qu’on l’utilise${until ? ` (sans visite, fin prévue : ${until})` : ''}. Un nouveau déploiement, ou la moindre visite, relance le compteur.`,
  takeOffline: 'Mettre hors ligne',
  readFailed: 'Impossible de lire l’historique.',
  reading: 'Chargement de l’historique…',
  none: 'Rien n’a encore été déployé.',
  noDescription: 'Sans description',
  deployed: (when, by) => `Déployé : ${when} (${by})`,
  duration: 'Durée en ligne',
  online: 'en ligne',
  short: {
    replaced: 'remplacé',
    stopped: 'mis hors ligne',
    expired: 'expiré',
    admin: 'par l’opérateur',
    ended: 'terminé',
  },
  putBack: (version) => `Remettre la version ${version} en ligne`,
  timeline: 'Ce qui était en ligne ces sept derniers jours',
  block: (version, from, to) => `Version ${version} : ${from} – ${to ?? 'maintenant'}`,
  weekAgo: 'il y a une semaine',
  now: 'maintenant',
};
