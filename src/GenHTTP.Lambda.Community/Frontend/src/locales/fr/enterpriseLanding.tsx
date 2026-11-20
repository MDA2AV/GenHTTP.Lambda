import type { Messages } from '../en';

export const enterpriseLanding: Messages['enterpriseLanding'] = {
  eyebrow: 'Des applications pour votre travail',
  headline: 'Décrivez l’application dont votre travail a besoin.',
  headlineAccent: 'Un agent IA la crée et la met en ligne ici.',
  intro:
    'Un formulaire, un calendrier de réservation, une liste pour votre équipe : dites avec vos propres mots ce qu’elle doit faire. En quelques minutes, vous obtenez un lien à partager avec vos collègues, et vous modifiez ensuite l’application de la même façon, sans écrire de code.',
  describe: 'Décrire une application',
  own: 'Utiliser votre propre agent IA',

  examplesTitle: 'Ce que les gens créent ici',
  examples: [
    { title: 'Formulaires et demandes', text: 'Congés, achats, retours d’expérience : chacun remplit le formulaire, et la personne qui décide valide.' },
    { title: 'Réservations', text: 'Salles de réunion, bureaux, matériel : qui a quoi, et quand.' },
    { title: 'Listes et suivis', text: 'Demandes d’assistance, inventaire, accueil des nouveaux : ce qui est en cours, et qui s’en occupe.' },
    { title: 'Pages pour une équipe', text: 'Un planning, un guide interne, des actualités : un seul lien que tout le monde ouvre.' },
  ],

  stepsTitle: 'Comment ça marche',
  steps: [
    {
      title: 'La décrire',
      text: 'Dites ce que l’application doit faire, comme vous l’expliqueriez à un collègue. Les idées sous le champ de saisie sont un point de départ.',
    },
    {
      title: 'Partager le lien',
      text: 'Quelques minutes plus tard, elle est en ligne ici. Envoyez son lien aux personnes qui l’utilisent et conservez votre lien d’édition : c’est ainsi que vous modifiez l’application.',
    },
    {
      title: 'En demander plus',
      text: 'Dans le même champ : quelque chose de nouveau, quelque chose de différent, ou un point sur son utilisation : qui l’a utilisée et ce qui a été saisi. Les versions précédentes sont conservées : la précédente peut donc être remise en ligne.',
    },
  ],

  questionsTitle: 'Bon à savoir',
  questions: (showcase) => [
    ['Dois-je savoir programmer ?', 'Non. Vous décrivez ce que l’application doit faire, et l’agent l’écrit. Vous n’avez jamais besoin de regarder le code.'],
    ['Qui peut ouvrir mon application ?', 'Toute personne qui possède son lien et peut accéder à ce site. Envoyez-le aux personnes qui doivent l’utiliser.'],
    ['Où sont conservées les données saisies ?', 'Chaque application garde ses propres entrées sur cette installation, que votre organisation exploite.'],
    [
      'Comment retrouver mon application ?',
      'Ce navigateur se souvient des applications que vous avez créées ici, à côté du champ où vous les avez créées. Sur un autre appareil, ouvrez le lien d’édition reçu avec l’application : conservez-le en lieu sûr, car personne ne peut le récupérer.',
    ],
    [
      'Quelque chose ne fonctionne pas. Que faire ?',
      'Dites à l’agent ce qui ne va pas, comme vous lui avez demandé l’application. Si une modification a aggravé les choses, remettez en ligne la version précédente dans l’éditeur de l’application.',
    ],
    ['Qu’ont créé mes collègues ?', <>Les applications qu’ils ont choisi de montrer se trouvent dans la {showcase('vitrine')}.</>],
  ],
};
