import type { Messages } from '../en';

export const enterprise: Messages['enterprise'] = {
  eyebrow: 'Entreprise',
  title: 'Gratuit pour essayer, à vous pour de bon',
  intro:
    'Ici, tout est gratuit et sans compte. Votre équipe a besoin d’apps en ligne en permanence, avec sa propre connexion ? Passez à votre propre installation, dans le cloud ou sur site.',

  free: 'Gratuit',
  freeTagline: 'Pour essayer',
  forever: 'pour toujours',
  buildOne: 'Créer une app',
  freeFeatures: (offline, removed) => [
    'Lambdas illimitées, sans compte',
    'L’agent intégré, ou le vôtre via MCP',
    'En ligne tant qu’on l’utilise',
    `Hors ligne après ${offline} jours sans visite, supprimée après ${removed} jours`,
    'Accessible sur un sous-domaine du domaine partagé',
  ],
  freeNote: 'Pas de carte, pas d’inscription. Créez une lambda : elle est à vous.',

  name: 'Entreprise',
  tagline: 'Pour les équipes qui veulent leur propre instance',
  perUser: 'par utilisateur / mois',
  contact: 'Nous contacter',
  features: [
    'Votre propre instance, dans le cloud ou sur site',
    'Un seul service fait tourner toutes les apps',
    'Connexion via votre propre SSO',
    'Vos règles de gouvernance et de conformité intégrées',
    'Les apps restent en ligne, rien n’est jamais supprimé',
    'Votre propre agent via MCP',
    'Support prioritaire',
  ],
  users: (count) => <>{count} utilisateurs</>,
  perMonth: ' / mois',
  price: (amount) => `${amount.toLocaleString('fr-FR')} $`,
  perUserPrice: (amount) => `${amount.toLocaleString('fr-FR')} $ par utilisateur / mois`,

  compareTitle: 'Comparer les offres',
  compareText: 'Les deux reposent sur la même plateforme. Ce qui change : combien de temps votre app est gardée, et où.',
  included: 'Inclus',
  notIncluded: 'Non inclus',
  groups: (offline, removed) => [
    {
      title: 'Création',
      rows: [
        ['Lambdas', 'Illimitées', 'Illimitées'],
        ['Agent intégré', true, false],
        ['Votre propre agent via MCP', true, true],
        ['Éditeur, versions et logs', true, true],
        ['Vitrine', true, 'La vôtre'],
      ],
    },
    {
      title: 'Hébergement',
      rows: [
        ['Mise hors ligne sans activité', `Après ${offline} jours`, 'Jamais'],
        ['Suppression sans activité', `Après ${removed} jours`, 'Jamais'],
        ['Instance', 'Partagée', 'La vôtre'],
        ['Exécution', 'Dans notre cloud', 'Cloud ou sur site'],
        ['Ce que vous gérez', 'Rien', 'Un seul service'],
        ['Domaines personnalisés', false, true],
      ],
    },
    {
      title: 'Contrôle',
      rows: [
        ['Connexion', 'Aucune', 'Votre propre SSO'],
        ['Vos règles de gouvernance et de conformité pour les agents', false, true],
        ['Console d’administration', false, true],
        ['Données isolées des autres clients', false, true],
        ['Support', 'Communauté', 'Prioritaire'],
      ],
    },
  ],

  questionsTitle: 'Questions',
  questions: [
    [
      'Faut-il un compte pour commencer ?',
      'Non. Une lambda gratuite ne demande rien d’autre que le lien d’édition reçu à sa création.',
    ],
    [
      'Qui compte comme utilisateur dans l’offre Entreprise ?',
      'Toute personne qui se connecte via votre SSO, que ce soit pour créer dans l’éditeur ou pour utiliser une app déployée sur votre installation. Ceux qui ouvrent une app sans se connecter ne sont pas comptés.',
    ],
    [
      'L’agent intégré est-il inclus dans l’offre Entreprise ?',
      'Non. Votre équipe apporte son propre agent (Claude, Claude Code ou tout autre outil qui parle MCP) et le connecte à votre installation, avec l’abonnement qu’elle a déjà chez son fournisseur.',
    ],
    [
      'Comment les agents apprennent-ils nos règles de conformité ?',
      'Nous intégrons vos règles de gouvernance et de conformité à ce que la plateforme transmet aux agents via MCP. Chaque agent connecté par votre équipe les reçoit pendant qu’il écrit du code. Les apps respectent donc vos règles, sans que chacun doive les connaître par cœur.',
    ],
    [
      'Faut-il Kubernetes ou un cluster ?',
      'Non. Toutes les apps tournent dans un seul service : pas de pods à répartir, rien à orchestrer app par app. Faire tourner l’installation, c’est faire tourner ce service.',
    ],
    [
      'Où tourne une installation Entreprise ?',
      'Où vous voulez. Nous pouvons l’héberger pour vous dans notre cloud, ou elle peut tourner dans votre propre compte cloud ou sur vos serveurs, partout où l’on peut lancer des conteneurs. Dans tous les cas, nous vous aidons à la mettre en place et à la tenir à jour.',
    ],
  ],
  anythingElse: (mail) => <>Autre chose ? Écrivez à {mail}.</>,
};
