import type { EditorMessages } from '../../en/editor';

export const analytics: EditorMessages['analytics'] = {
  hint: 'Compté pour vous par la plateforme : pas de cookies, pas de code dans votre application, et rien à accepter pour vos visiteurs.',
  loading: 'Lecture des visites…',
  loadFailed: 'Les visites n’ont pas pu être lues.',

  offTitle: 'Voyez qui visite votre application',
  offText:
    'Combien de personnes viennent chaque jour, quelles pages elles ouvrent, quels sites les envoient, de quels pays et sur quels appareils - comptés jour par jour, dès que vous l’activez.',
  offPrivate: 'Aucun cookie, et aucune bannière à faire accepter à vos visiteurs',
  offAddresses: 'Aucune adresse n’est conservée - seulement le nombre de visites de chaque jour',
  offKept: (days) =>
    days >= 365
      ? 'Conservé un an, et tout vous est montré'
      : `Conservé un an - les ${days} derniers jours sont affichés, tout avec Premium`,
  switchOn: 'Activer la mesure d’audience',
  switchedOn: 'La mesure d’audience est activée. Les premières visites apparaissent d’ici quelques minutes.',
  switchOnFailed: 'La mesure d’audience n’a pas pu être activée.',
  demo: 'Les visiteurs d’une démo ne sont pas comptés. Une copie à vous peut compter ses visiteurs.',

  range: 'Période',
  days: (days) => (days >= 365 ? '12 mois' : `${days} jours`),
  premiumRange: 'Premium affiche jusqu’à un an',

  visitors: 'Visiteurs',
  visitorsHint: 'Personnes différentes, chacune comptée une fois par jour.',
  views: 'Pages vues',
  viewsHint: 'Chaque fois que quelqu’un a ouvert une page.',
  hits: 'Requêtes',
  hitsHint: 'Tout ce qui a été demandé à votre application : ses pages, et les images et scripts qu’elles chargent.',
  versus: (days) => `par rapport aux ${days} jours précédents`,
  noChange: 'inchangé',
  inDays: (days) => (days >= 365 ? 'sur les 12 derniers mois' : `sur les ${days} derniers jours`),

  perDay: {
    visitors: 'Visiteurs par jour',
    views: 'Pages vues par jour',
    hits: 'Requêtes par jour',
  },
  perWeek: {
    visitors: 'Visiteurs par semaine',
    views: 'Pages vues par semaine',
    hits: 'Requêtes par semaine',
  },
  showValues: 'Afficher les valeurs',
  showChart: 'Afficher le graphique',
  day: 'Jour',
  week: 'Semaine du',

  nobody: 'Personne n’est venu ces jours-ci.',
  nobodyYet: 'Personne n’est venu depuis l’activation de la mesure d’audience. Les premières visites apparaissent d’ici quelques minutes.',

  pages: 'Pages',
  pagesHint: 'Les pages ouvertes, les plus ouvertes en premier.',
  requested: 'Les plus demandées',
  requestedHint: 'Tout ce qui a été demandé à votre application, par adresse - elle n’a pas de pages que l’on ouvre.',
  sources: 'D’où viennent les visiteurs',
  sourcesHint: 'Les sites qui ont envoyé des gens ici, avec la page où ils se trouvaient. Les moteurs de recherche indiquent seulement que c’était eux, pas ce qui a été cherché.',
  direct: 'Saisie directe, favori ou application',
  countries: 'Pays',
  countriesHint: 'Où sont enregistrées les adresses internet des visiteurs - généralement, mais pas toujours, là où ils se trouvent.',
  unknownCountry: 'Inconnu',
  devices: 'Appareils',
  device: {
    desktop: 'Ordinateur',
    mobile: 'Téléphone',
    tablet: 'Tablette',
    bot: 'Robots',
    other: 'Autre',
  },
  browsers: 'Navigateurs',
  systems: 'Systèmes',
  crawlers: 'Moteurs de recherche et IA',
  crawlersHint: 'Programmes passés par là : moteurs de recherche qui indexent vos pages, assistants IA qui les lisent, messageries qui affichent un aperçu d’un lien.',
  noCrawlers: 'Aucun moteur de recherche ni aucune IA n’est passé ces jours-ci.',
  other: 'Autres',
  otherBots: 'Autres programmes',
  showAll: (count) => `Tout afficher (${count})`,
  showFewer: 'Afficher moins',
  share: (part, whole) => `${part} sur ${whole}`,
  none: 'Rien pour l’instant.',

  moreKept: (since) => `Décompte depuis le ${since} - Premium affiche tout, jusqu’à un an.`,
  premiumAsk: 'Se renseigner sur Premium',
  premiumSubject: 'Premium pour mon application',
  premiumBody: (address) => `Bonjour,\n\nJe voudrais voir toutes les statistiques d’audience de mon application : ${address}\n\n`,

  footer: (minutes) =>
    minutes === 1 ? 'Décompte par jour, en UTC. Actualisé chaque minute.' : `Décompte par jour, en UTC. Actualisé toutes les ${minutes} minutes.`,
  updated: (time) => `Dernière mise à jour à ${time}.`,
  since: (date) => `Décompte depuis le ${date}.`,

  switchOff: 'Désactiver la mesure d’audience',
  switchOffTitle: 'Désactiver la mesure d’audience ?',
  switchOffText: 'Le comptage s’arrête, et tout ce qui a été compté jusqu’ici est supprimé. Cette action est irréversible.',
  keep: 'Continuer à compter',
  switchOffConfirm: 'Désactiver et supprimer',
  switchedOff: 'La mesure d’audience est désactivée, et ce qui avait été compté est supprimé.',
  switchOffFailed: 'La mesure d’audience n’a pas pu être désactivée.',
};
