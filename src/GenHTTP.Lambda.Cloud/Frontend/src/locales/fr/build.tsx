import type { Messages } from '../en';

export const build: Messages['build'] = {
  title: 'Créer un site web avec l’IA.',
  intro:
    'Décrivez avec vos propres mots le site web ou l’application que vous avez en tête. L’IA le crée pour vous, nous l’hébergeons, et il est en ligne en quelques minutes, avec un lien à envoyer à qui vous voulez. Gratuit, sans coder, sans inscription.',
  points: [
    {
      title: 'Sans savoir coder',
      text: 'Dites avec vos propres mots ce que votre site doit faire, comme vous l’expliqueriez à un ami. L’IA le crée pour vous, sans aucune compétence technique.',
    },
    {
      title: 'Hébergement gratuit inclus',
      text: 'Votre site tourne sur nos serveurs. Pas d’hébergeur ni de serveur à payer, pas de nom de domaine à acheter, rien à installer : la sécurité et les mises à jour sont prises en charge.',
    },
    {
      title: 'En ligne en quelques minutes',
      text: 'Vous recevez tout de suite un lien à partager. Le site retient ce que les gens y saisissent (inscriptions, votes, messages, scores), pour que tout le monde voie la même chose.',
    },
  ],

  questionsTitle: 'Avant de commencer',
  questions: (offline, removed) => [
    [
      'L’IA peut-elle vraiment créer mon site web gratuitement ?',
      `Oui. Décrivez-le avec vos propres mots : l’IA le crée, le met en ligne et vous donne le lien. Sans inscription, sans carte bancaire, sans période d’essai. Il reste en ligne tant qu’il est utilisé : après ${offline} jours sans visite ni modification, il est mis hors ligne, puis supprimé après ${removed} jours.`,
    ],
    [
      'Faut-il un hébergeur, un serveur ou un nom de domaine ?',
      'Non. Votre site tourne sur nos serveurs, hébergement, sécurité et mises à jour compris. Vous recevez un lien tout de suite : pas de nom de domaine à acheter non plus.',
    ],
    [
      'Peut-on créer une application sans savoir coder ?',
      'Oui. Vous ne voyez jamais de code. Dites ce qu’elle doit faire, comme vous l’expliqueriez à un ami, et l’IA s’occupe du reste : un site web, une petite application ou un jeu.',
    ],
    [
      'Les gens peuvent-ils s’inscrire, voter, laisser un message ?',
      'Oui. Votre site retient ce que les gens y saisissent : tous ceux qui ouvrent le lien voient les mêmes inscriptions, votes et scores.',
    ],
    [
      'Comment les autres l’ouvrent-ils ?',
      'Avec le lien, dans n’importe quel navigateur, sur téléphone comme sur ordinateur. Rien à installer, et pas de boutique d’applications à passer.',
    ],
    [
      'Comment le modifier plus tard ?',
      'Ouvrez le lien d’édition reçu avec votre site et décrivez ce qui doit changer, comme ici. Si une modification ne vous plaît pas, vous pouvez revenir à ce qu’il y avait avant.',
    ],
  ],

};
