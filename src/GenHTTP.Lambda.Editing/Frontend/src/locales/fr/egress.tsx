import type { EditorMessages } from '../en';

export const egress: EditorMessages['egress'] = {
  hint:
    'Indique si le code peut se connecter à d’autres serveurs : appeler une API, ouvrir un socket, envoyer un e-mail. Les lambdas partagent un même serveur, c’est donc autorisé lambda par lambda - une lambda premium le peut, toute autre une fois que l’opérateur de ce site l’a autorisé.',

  included: 'Inclus avec premium',
  includedText: 'Le code peut se connecter à d’autres serveurs : premium l’inclut.',
  allowed: 'Autorisé',
  allowedText: (ago) => <>L’opérateur de ce site l’a autorisé {ago}. Le code peut se connecter à d’autres serveurs.</>,
  requested: 'En attente de l’opérateur',
  requestedText: (ago) => (
    <>Demandé {ago}. Dès que l’opérateur de ce site l’autorise, le code qui se connecte compile - d’ici là, il est refusé.</>
  ),
  declined: 'Refusé',
  declinedText: (ago) => <>L’opérateur de ce site l’a refusé {ago}. Vous pouvez refaire la demande en précisant à quoi cela sert.</>,
  none: 'Non autorisé',
  noneText:
    'Le code ne peut pas se connecter à d’autres serveurs : le code qui appelle une API, ouvre un socket ou envoie un e-mail ne compile pas. Faites la demande ci-dessous si la lambda en a besoin.',
  operatorSaid: 'L’opérateur a répondu',
  purposeSaid: 'À quoi cela sert',

  connects: 'Le code se connecte à d’autres serveurs, ce que cette lambda n’a pas le droit de faire : il ne compile pas tant que ce n’est pas autorisé.',
  connectsWaiting: 'Le code se connecte à d’autres serveurs : il compilera dès que l’opérateur l’autorisera.',

  askTitle: 'Faire la demande',
  askAgainTitle: 'Refaire la demande',
  editTitle: 'À quoi cela sert',
  purpose: 'À quoi se connecte-t-elle, et pourquoi ?',
  purposeHint: 'Indiquez les serveurs ou services, et ce que la lambda en fait. L’opérateur de ce site le lit pour décider.',
  purposeExample: 'api.open-meteo.com, pour les prévisions de la page d’accueil',
  send: 'Envoyer la demande',
  save: 'Enregistrer',
  cancel: 'Annuler',
  edit: 'Modifier à quoi cela sert',
  asked: 'Demande envoyée. L’opérateur de ce site décide.',
  askFailed: 'La demande n’a pas pu être envoyée.',
  withdraw: 'Retirer la demande',
  withdrawn: 'La demande est retirée.',
  giveUp: 'Y renoncer',
  givenUp: 'Le code ne peut plus se connecter à d’autres serveurs.',
  giveUpTitle: 'Renoncer à la connexion à d’autres serveurs ?',
  giveUpText:
    'Dès sa prochaine requête, le code ne pourra plus se connecter à d’autres serveurs, et la version en ligne cessera de fonctionner là où elle le fait. Pour le récupérer, refaites la demande.',
  keep: 'Le garder',
  failed: 'Cela n’a pas fonctionné.',

  coversTitle: 'Ce qui compte',
  covers: ['HttpClient et WebRequest', 'Sockets et clients WebSocket', 'E-mail (SmtpClient)', 'DNS et ping', 'XML chargé depuis une adresse'],
  notCovered:
    'Ce qui se passe dans le navigateur n’est pas concerné : les pages de la lambda peuvent elles-mêmes récupérer des données d’autres sites, et les visiteurs, webhooks et websockets atteignent la lambda comme toujours. Une API publique que les navigateurs peuvent appeler est mieux appelée depuis la page - cela ne demande aucune autorisation.',

  refused: 'Le code se connecte à d’autres serveurs',
  refusedText: 'Cette lambda n’a pas encore le droit de le faire. Faites la demande, et l’opérateur de ce site décide.',
  openEgress: 'Faire la demande',
};
