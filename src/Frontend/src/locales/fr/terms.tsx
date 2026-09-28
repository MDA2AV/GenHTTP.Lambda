import type { Messages } from '../en';

export const terms: Messages['terms'] = {
  title: 'Conditions d’utilisation',
  binding: (english) => (
    <>Cette traduction est fournie à titre d’information. Seule la {english('version anglaise')} fait foi.</>
  ),
  intro:
    'Ce service gratuit sert à tester des idées. Il exécute du code écrit par des inconnus sur une infrastructure partagée. Cela ne fonctionne que si chacun respecte quelques règles.',
  sections: {
    forbiddenTitle: 'Ce qui est interdit ici',
    forbidden: [
      'Pas de logiciels malveillants, pas de phishing, pas de mineurs de cryptomonnaie. Rien qui attaque, scanne, inonde de requêtes ou perturbe d’une autre façon d’autres systèmes, ici ou ailleurs. Rien qui harcèle qui que ce soit. Rien que vous n’ayez pas le droit de publier, y compris le code, les textes, les images et les marques d’autrui.',
      'N’utilisez pas une lambda pour stocker ou transmettre des données personnelles sur d’autres personnes. Une adresse publique n’a rien de privé, et cette plateforme ne vous offre aucun moyen de protéger ces données.',
    ],
    actionTitle: 'Ce que nous pouvons faire',
    action:
      'Tout ce qui est déployé ici peut être mis hors ligne ou supprimé à tout moment, sans préavis et sans obligation de s’expliquer. En pratique, cela arrive quand quelque chose enfreint les règles ci-dessus, menace la machine que tout le monde partage, ou quand quelqu’un le signale et qu’il a raison.',
    lastingTitle: 'Durée de vie',
    lasting: (hours, days) =>
      `Un déploiement reste accessible environ ${hours} heures. Une lambda que vous n’avez pas ouverte est supprimée, avec toutes les versions de son code, environ ${days} jours après la dernière fois que vous y avez touché. Enregistrer ou déployer, c’est y toucher : ce sur quoi vous travaillez reste donc en place. Rien ici ne fait office de sauvegarde : gardez votre propre copie du code auquel vous tenez.`,
    keyTitle: 'Votre lien d’édition est votre mot de passe',
    key: 'Toute personne qui a le lien d’édition peut lire et modifier la lambda. Il n’y a ni compte ni mot de passe derrière. Publier le lien, c’est publier le droit de la modifier. Un lien perdu ne peut pas être récupéré.',
    warrantyTitle: 'Aucune garantie',
    warranty:
      'Le service est fourni tel quel, sans garantie qu’il fonctionne, qu’il continue de fonctionner ou qu’il conserve ce que vous y mettez. Il peut être redémarré, modifié ou arrêté à tout moment. N’y construisez rien d’important, pour vous ou pour quelqu’un d’autre.',
    reportTitle: 'Signaler un problème',
    report: (mailbox, front) => (
      <>
        Si une lambda hébergée ici fait ce qu’elle ne devrait pas, écrivez à {mailbox} en donnant son adresse. La{' '}
        {front('page d’accueil')} explique quoi indiquer.
      </>
    ),
  },
  change: 'Ces conditions peuvent changer. C’est la version de cette page qui s’applique.',

  short:
    'Les lambdas tournent sur une infrastructure partagée. En créant une lambda, vous vous engagez à ne pas déployer de logiciels malveillants, de pages de phishing, de mineurs de cryptomonnaie, ni rien qui attaque, scanne ou inonde de requêtes d’autres systèmes, et à ne pas publier de contenu que vous n’avez pas le droit de publier. Toute personne qui connaît le lien d’édition peut modifier votre lambda : traitez-le comme un mot de passe. Dans l’offre gratuite, une lambda reste en ligne tant qu’elle est utilisée. Sans visite ni modification pendant un mois, elle est mise hors ligne, puis supprimée s’il ne se passe rien pendant les deux mois suivants. Tout ce que vous déployez peut être supprimé à tout moment.',
};
