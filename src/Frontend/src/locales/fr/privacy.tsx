import type { Messages } from '../en';

export const privacy: Messages['privacy'] = {
  title: 'Politique de confidentialité',
  binding: (english) => (
    <>
      Cette traduction est fournie à titre d’information. Seule la {english('version anglaise')} de cette politique
      fait foi.
    </>
  ),
  intro:
    'Ce que ce site apprend sur vous, ce qu’il en fait, combien de temps il le garde et qui d’autre peut le voir. En bref : pas de compte, pas de publicité, aucun suivi. Le serveur note qui lui a demandé quoi, pour que le site continue de tourner et que les abus puissent être retrouvés, et ce que vous demandez à l’agent de création est envoyé à Anthropic, dont le modèle écrit l’app.',
  sections: {
    whoTitle: 'Qui est responsable',
    who: 'Ce site est exploité par la personne ci-dessous, qui est responsable du traitement des données personnelles au sens du règlement général sur la protection des données (RGPD) de l’UE. Pour toute question sur cette page, écrivez à cette adresse :',

    requestsTitle: 'Ce que le serveur note à chaque requête',
    requests: [
      'Chaque requête adressée à ce site, et à chaque lambda qu’il héberge, est inscrite dans le journal (les logs) du serveur : l’adresse IP d’où elle vient et, si elle a été relayée, l’adresse d’origine qu’elle indique, le navigateur ou le programme qui l’a envoyée, l’adresse demandée, le moment, et la réponse donnée. Le serveur cherche aussi à quel pays, quelle ville et quel réseau appartient l’adresse IP, dans une base de données qu’il gère lui-même, sans interroger personne d’autre.',
      'C’est ce qui permet de trouver les pannes, de remonter jusqu’à ce qui surcharge le serveur et de retrouver les abus qui nous sont signalés. L’adresse IP sert aussi, uniquement en mémoire, à limiter le nombre de requêtes et de créations d’un même visiteur. Sans ces informations, impossible de répondre à une requête. La base légale est notre intérêt légitime à faire fonctionner le service et à le sécuriser (art. 6.1.f du RGPD).',
      'Les administrateurs peuvent tout consulter. Le propriétaire d’une lambda voit le pays et le navigateur de chaque requête vers sa lambda, mais pas l’adresse IP.',
    ],

    logsTitle: 'Combien de temps le journal est conservé',
    logs: 'Le journal est conservé à deux endroits : dans la mémoire du serveur, qui est vidée à chaque redémarrage, et dans la sortie console du serveur, qui est effacée à chaque mise à jour. Les deux ont une taille fixe : chaque nouvelle ligne chasse la plus ancienne, et la durée de vie d’une ligne dépend de l’activité du site. Rien n’en est archivé.',

    contentTitle: 'Ce que vous mettez ici',
    content: (days) =>
      `Une lambda, c’est son code, ses fichiers, ses réglages et les notes enregistrées avec ses versions sur ce qui a été demandé et ce qui a changé. Tout cela est stocké sur le serveur pour qu’elle puisse être exécutée et modifiée. Une lambda gratuite est supprimée avec toutes ses versions environ ${days} jours après sa dernière modification ou visite, et immédiatement si la personne qui a son lien d’édition la supprime. Toute personne qui a le lien d’édition peut tout lire, ce que vous mettez dans la vitrine est visible par tous, et les administrateurs consultent une lambda quand c’est nécessaire, pour traiter un signalement ou protéger le serveur. La base légale est la fourniture du service que vous avez demandé (art. 6.1.b du RGPD).`,

    agentTitle: 'Ce que vous demandez à l’agent de création',
    agent: (policy) => (
      <>
        Ce que vous tapez dans le champ de création, ou dans la section Modifier de l’éditeur d’une lambda, est envoyé à
        Anthropic PBC, aux États-Unis, qui exploite Claude, le modèle qui écrit l’app. Pour faire une modification,
        l’agent lit aussi la lambda (son code, les notes de ses versions et son journal, qui contient ses requêtes et ce
        qu’elle a affiché, mais pas les adresses IP de ses visiteurs), et ce qu’il lit y est lui aussi envoyé. Ce
        qu’Anthropic en fait relève de {policy('sa propre politique de confidentialité')}. Les États-Unis ne protègent
        pas les données personnelles comme l’UE. Votre demande y est envoyée parce qu’elle est nécessaire pour créer ou
        modifier ce que vous voulez (art. 6.1.b et art. 49.1.b du RGPD) : n’y mettez donc rien que vous ne
        voudriez pas partager.
      </>
    ),
    agentKept:
      'L’agent enregistre votre demande, souvent reformulée avec ses propres mots, comme note de la version qu’il écrit. La demande elle-même est écrite en entier dans le journal du serveur, et ses premières centaines de caractères dans celui du service de création ; tous deux ont eux aussi une taille fixe. Si vous utilisez plutôt votre propre agent, comme Claude ou Claude Code, ce que vous lui dites va chez le fournisseur de cet agent, pas chez nous : nous recevons seulement le code et les notes qu’il envoie ici.',

    lambdasTitle: 'Ce que fait une lambda dépend de son propriétaire',
    lambdas:
      'Une lambda est écrite par la personne qui a son lien d’édition, pas par nous. Ce qu’elle demande à ses visiteurs et ce qu’elle en fait ne dépend que de cette personne, et cette page ne le couvre pas, à part le journal des requêtes décrit plus haut, que le serveur tient pour chaque lambda. Les conditions d’utilisation interdisent d’utiliser une lambda pour collecter des données personnelles sur d’autres personnes. Si vous en trouvez une qui le fait, merci de la signaler.',

    mailTitle: 'Quand vous nous écrivez',
    mail: 'Si vous nous écrivez, pour signaler un abus ou pour autre chose, nous utilisons votre adresse e-mail et votre message pour vous répondre et traiter ce que vous nous écrivez, puis nous les supprimons dès qu’ils ne sont plus nécessaires pour cela (art. 6.1.f du RGPD).',

    storageTitle: 'Cookies et navigateur',
    storage:
      'Il y a un seul cookie, appelé lang. Il retient la langue que vous avez choisie, pour que les adresses qui n’en indiquent pas s’ouvrent dans cette langue, et il dure un an. Le stockage local du navigateur retient le mode clair ou sombre, quelques réglages des pages que vous utilisez et, pour les administrateurs, leur jeton. Rien de tout cela ne sert à vous suivre et rien n’est transmis à qui que ce soit : pas de mesure d’audience, pas de publicité, et rien n’est chargé depuis d’autres sites, pas même les polices. Comme tout cela sert uniquement à ce que vous avez demandé, aucun consentement n’est nécessaire (§ 25, al. 2, n° 2 de la loi allemande TDDDG).',

    hostingTitle: 'Où tout cela est stocké',
    hosting:
      'Le serveur sur lequel tout cela tourne est loué à un hébergeur situé dans l’Union européenne, et c’est là que sont stockées les données décrites sur cette page.',

    rightsTitle: 'Vos droits',
    rights: (mailbox) => (
      <>
        Vous pouvez savoir ce que nous avons enregistré sur vous et en demander une copie, le faire rectifier ou effacer,
        en faire limiter l’utilisation, et vous opposer à tout ce que nous faisons au titre de notre intérêt légitime
        (art. 15 à 21 du RGPD). Écrivez à {mailbox}. Comme il n’y a pas de compte, nous ne pouvons retrouver ce qui vous
        concerne que si vous nous dites comment : l’adresse IP utilisée et le moment approximatif, ou l’adresse de votre
        lambda. Aucune décision ayant des effets juridiques sur vous, ou des effets tout aussi importants, n’est prise de
        façon automatique (art. 22 du RGPD).
      </>
    ),
    complaint:
      'Vous pouvez aussi adresser une réclamation à une autorité de protection des données, là où vous vivez ou là où nous sommes. Pour nous, c’est le commissaire à la protection des données et à la liberté d’information du Land de Bade-Wurtemberg, en Allemagne (LfDI Baden-Württemberg).',
  },
  change: 'Cette politique change quand le site change. C’est la version de cette page qui s’applique.',
  updated: 'Dernière modification : 30 septembre 2026.',
};
