import type { Messages } from '../en';

export const selfHosting: Messages['selfHosting'] = {
  menu: 'Auto-hébergement',

  eyebrow: 'Open source · cloud ou auto-hébergé',
  title: 'Utilisez-la ici,',
  titleAccent: 'ou hébergez-la vous-même.',
  intro:
    'GenHTTP Lambda est open source sous licence MIT. genhttp.dev la fait tourner pour tous, et l’édition Community est la même plateforme sous forme d’image Docker, pour ceux qui préfèrent l’exploiter eux-mêmes. Les deux créent et hébergent les mêmes apps ; ce qui change, c’est où elles vivent et qui s’en occupe.',

  cloud: {
    kicker: 'Cloud',
    name: 'genhttp.dev',
    summary: 'Ouvrez ce site et lancez-vous. Le serveur, ses mises à jour et ses limites, c’est à nous de nous en occuper.',
    points: ['Rien à installer ni à exploiter', 'Les apps répondent à une adresse sur genhttp.run', 'L’agent de création inclus'],
    action: 'Créer quelque chose',
  },
  community: {
    kicker: 'Auto-hébergé',
    name: 'Édition Community',
    summary: 'Un conteneur Docker sur une machine à vous. Le serveur, ses mises à jour et ses limites, c’est à vous de vous en occuper.',
    points: ['Un conteneur, un volume, pas de serveur de base de données', 'Les apps répondent à votre adresse', 'L’agent de création dans un conteneur supplémentaire'],
    action: 'L’exploiter vous-même',
  },

  sameTitle: 'Identique des deux côtés',
  sameText: 'L’édition Community n’est pas une version réduite. Chaque app reçoit les mêmes choses, où qu’elle tourne :',
  same: [
    'L’agent de création',
    'Votre propre agent via MCP',
    'L’API REST',
    'L’éditeur, en vue simple et complète',
    'Des brouillons avec des données de test',
    'Des versions, et un retour possible',
    'Entrées, fichiers, clés et mots de passe',
    'L’audience sans cookies',
    'Des domaines à elles',
    'Git et téléchargements',
    'La vitrine et le code publié',
  ],

  differTitle: 'Ce qui diffère',
  aspect: 'Question',
  rows: (offline) => [
    ['Où tournent les apps', 'Sur notre serveur', 'Sur le vôtre, dans un conteneur avec un volume'],
    ['Pour commencer', 'Ouvrir la page de création, sans compte', 'Démarrer l’image, puis ouvrir son adresse'],
    ['Qui s’occupe du serveur', 'Nous', 'Vous'],
    ['Ce que ça coûte', 'Rien ; apps Premium sur demande', 'Rien ; le serveur est à votre charge'],
    ['Où répond une app', 'À un sous-domaine de genhttp.run', 'Sous votre adresse, ou à des sous-domaines d’un domaine que vous indiquez'],
    ['Un domaine à elle', 'Pour les apps Premium', 'Pour toute app que vous passez en production'],
    ['Apps que personne n’utilise', `Hors ligne après ${offline} jours sans visite ni modification, sauf Premium`, 'Selon votre réglage'],
    ['Limites et règles', 'Les nôtres, et nos conditions d’utilisation', 'Les vôtres, réglées dans le panneau d’administration'],
    ['L’agent de création', 'Inclus, avec un nombre de créations par jour', 'Facultatif, avec votre propre abonnement Claude'],
    ['Rapports d’utilisation', 'Aucun', 'Une fois par jour : la version, la machine, les options et des totaux sur les apps – jamais une app ni une personne. Vous pouvez les désactiver.'],
  ],

  moveTitle: 'Passer de l’un à l’autre',
  move:
    'Les mêmes apps tournent des deux côtés, et votre agent en déplace une avec git : il crée l’app de l’autre côté, y copie les fichiers de l’ancienne et les pousse. Un téléchargement est un projet .NET qui tourne partout, avec ses entrées dans un fichier SQLite.',

  pickTitle: 'Ce qui vous convient',
  pickCloud: {
    title: 'genhttp.dev, si',
    points: [
      'vous voulez une app en ligne sans rien exploiter',
      'les personnes qui l’utilisent sont n’importe où',
      'vous préférez ne pas vous occuper d’un serveur',
    ],
  },
  pickCommunity: {
    title: 'L’édition Community, si',
    points: [
      'vos apps et leurs entrées doivent rester sur une machine à vous',
      'ce sont des outils pour votre équipe, dans votre propre réseau',
      'vous voulez régler vous-même les limites et les règles',
    ],
  },

  startTitle: 'Pour commencer',
  startCloud: {
    title: 'Sur genhttp.dev',
    text: 'Décrivez ce dont vous avez besoin sur la page de création, ou connectez l’agent que vous utilisez déjà. Aucun compte à créer.',
  },
  startCommunity: {
    title: 'Sur votre serveur',
    text: 'Une commande suffit à le démarrer. Ouvrez son adresse, connectez votre agent et créez : le README dit le reste.',
  },
  connect: 'Connecter votre agent',
  readme: 'Lire le README',
  dockerHub: 'L’image sur Docker Hub',

  sourceTitle: 'Open source',
  source: (github) => (
    <>
      GenHTTP Lambda est publiée sous licence MIT. Son code, ses tickets et ses versions sont sur {github('GitHub')}.
    </>
  ),
};
