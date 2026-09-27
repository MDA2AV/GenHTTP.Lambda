import type { Messages } from '../en';

export const enterprise: Messages['enterprise'] = {
  eyebrow: 'Entreprise',
  title: 'Essai gratuit, exploitation en propre',
  intro:
    'Tout ce qui est proposé ici est gratuit et sans compte. Lorsque votre organisation a besoin d’applications disponibles durablement, derrière sa propre authentification, optez pour une installation dédiée – dans le cloud ou sur site.',

  free: 'Gratuit',
  freeTagline: 'Pour découvrir',
  forever: 'sans limite de durée',
  buildOne: 'Créer une application',
  freeFeatures: (offline, removed) => [
    'Lambdas illimités, sans compte',
    'L’agent intégré, ou le vôtre via MCP',
    'En ligne tant que l’application est utilisée',
    `Hors ligne après ${offline} jours sans visite, supprimé après ${removed} jours`,
    'Accessible via un chemin sur l’hôte partagé',
  ],
  freeNote: 'Ni carte bancaire, ni inscription. Créez un lambda : il vous appartient.',

  name: 'Enterprise',
  tagline: 'Pour les organisations souhaitant leur propre instance',
  perUser: 'par utilisateur et par mois',
  contact: 'Nous contacter',
  features: [
    'Instance dédiée, dans le cloud ou sur site',
    'Un seul service exécute toutes les applications',
    'Authentification via votre propre SSO',
    'Vos règles de gouvernance et de conformité intégrées',
    'Applications disponibles durablement, jamais supprimées',
    'Vos propres agents via MCP',
    'Support prioritaire',
  ],
  users: (count) => <>{count} utilisateurs</>,
  perMonth: ' / mois',
  price: (amount) => `${amount} $`,
  perUserPrice: (amount) => `${amount} $ par utilisateur et par mois`,

  compareTitle: 'Comparer les offres',
  compareText:
    'Les deux offres reposent sur la même plateforme. Elles diffèrent par la durée de conservation de votre application et son lieu d’exécution.',
  included: 'Inclus',
  notIncluded: 'Non inclus',
  groups: (offline, removed) => [
    {
      title: 'Développement',
      rows: [
        ['Lambdas', 'Illimités', 'Illimités'],
        ['Agent intégré', true, false],
        ['Votre propre agent via MCP', true, true],
        ['Éditeur, versions et journaux', true, true],
        ['Vitrine', true, 'Dédiée'],
      ],
    },
    {
      title: 'Hébergement',
      rows: [
        ['Mise hors ligne en cas d’inactivité', `Après ${offline} jours`, 'Jamais'],
        ['Suppression en cas d’inactivité', `Après ${removed} jours`, 'Jamais'],
        ['Instance', 'Partagée', 'Dédiée'],
        ['Exécution', 'Dans notre cloud', 'Cloud ou sur site'],
        ['Ce que vous exploitez', 'Rien', 'Un seul service'],
        ['Noms de domaine personnalisés', false, true],
      ],
    },
    {
      title: 'Contrôle',
      rows: [
        ['Authentification', 'Non requise', 'Votre propre SSO'],
        ['Vos règles de gouvernance et de conformité pour les agents', false, true],
        ['Console d’administration', false, true],
        ['Données isolées des autres clients', false, true],
        ['Support', 'Communauté', 'Prioritaire'],
      ],
    },
  ],

  questionsTitle: 'Questions fréquentes',
  questions: [
    [
      'Faut-il un compte pour commencer ?',
      'Non. Un lambda gratuit ne nécessite que le lien d’édition que vous recevez lors de sa création.',
    ],
    [
      'Qui est considéré comme utilisateur dans l’offre Enterprise ?',
      'Toute personne qui se connecte via votre SSO, que ce soit pour développer dans l’éditeur ou pour utiliser une application déployée sur votre installation. Les personnes accédant à une application sans se connecter ne sont pas comptabilisées.',
    ],
    [
      'L’agent intégré est-il inclus dans l’offre Enterprise ?',
      'Non. Vos équipes utilisent leur propre agent – Claude, Claude Code ou tout autre client compatible MCP – et le connectent à votre installation, dans le cadre de l’abonnement dont vous disposez déjà auprès de son éditeur.',
    ],
    [
      'Comment les agents prennent-ils connaissance de nos règles de conformité ?',
      'Nous intégrons vos règles de gouvernance et de conformité aux informations que la plateforme transmet aux agents via MCP. Chaque agent connecté les reçoit lorsqu’il écrit du code : les applications respectent ainsi vos règles sans que chacun ait à les connaître en détail.',
    ],
    [
      'Avons-nous besoin de Kubernetes ou d’un cluster ?',
      'Non. Toutes les applications s’exécutent au sein d’un seul service : aucun pod à répartir, aucune orchestration par application. Exploiter l’installation revient à exploiter ce seul service.',
    ],
    [
      'Où une installation Enterprise est-elle exécutée ?',
      'Là où vous le souhaitez. Nous pouvons l’héberger dans notre cloud, ou elle peut fonctionner dans votre propre compte cloud ou sur vos serveurs – partout où des conteneurs peuvent s’exécuter. Dans tous les cas, nous vous accompagnons pour la mise en place et assurons sa mise à jour.',
    ],
  ],
  anythingElse: (mail) => <>Une autre question ? Écrivez à {mail}.</>,
};
