import type { EditorMessages } from '../../en/editor';

export const domain: EditorMessages['domain'] = {
  readFailed: 'Impossible de lire le domaine.',
  reaching: (domain) => `Les requêtes vers ${domain} arrivent désormais sur cette lambda.`,
  saveFailed: 'Impossible d’enregistrer le domaine.',
  removed: 'Domaine retiré. La lambda répond de nouveau à son adresse ici.',
  removeFailed: 'Impossible de retirer le domaine.',
  hint:
    'Une lambda premium peut répondre sur son propre domaine (tout le domaine, depuis la racine). Faites d’abord pointer le domaine vers ce serveur, puis saisissez-le ici : dès lors, ses requêtes arrivent sur la lambda, et son adresse ici renvoie les visiteurs vers lui.',
  loading: 'Chargement…',
  example: 'votre-domaine.fr',
  open: (domain) => `Ouvrir ${domain}`,
  label: 'Le domaine sur lequel elle répond',
  serving: (domain) => <>Répond désormais sur {domain}. Son adresse ici renvoie les visiteurs vers lui.</>,
  none: 'Aucun pour l’instant. Un sous-domaine comme shop.example.com, ou un domaine entier comme example.com.',
  change: 'Modifier',
  use: 'Utiliser ce domaine',
  remove: 'Retirer',
  confirm: 'Retirer le domaine ?',
  keep: 'Le garder',
  confirmText: (domain) => (
    <>
      Dès maintenant, les requêtes vers {domain} n’arriveront plus sur cette lambda, et son adresse ici répondra de
      nouveau au lieu de renvoyer les visiteurs. Ce que dit le DNS du domaine ne change pas.
    </>
  ),
  point: 'Faites pointer le domaine vers ce serveur',
  check: 'Revérifier',
  records:
    'Chez le gestionnaire DNS du domaine, ajoutez ces deux enregistrements. Laissez de côté l’enregistrement AAAA si vous ne voulez pas être joignable en IPv6.',
  type: 'Type',
  name: 'Nom',
  value: 'Valeur',
  pointsHere: (domain) => <>{domain} pointe ici.</>,
  alsoElsewhere: (addresses) =>
    ` Il pointe aussi vers ${addresses}, qui n’est pas ce serveur : les visiteurs envoyés là-bas n’atteindront pas la lambda.`,
  elsewhere: (addresses) => `Il pointe vers ${addresses}, qui n’est pas encore ce serveur.`,
  wait: 'Un changement peut mettre un moment à être visible partout, jusqu’au TTL de l’ancien enregistrement.',
  cname: 'Utiliser plutôt un enregistrement CNAME',
  cnameText: (target) => (
    <>
      Un sous-domaine peut aussi pointer vers {target} avec un enregistrement CNAME : il suit alors ce serveur si ses
      adresses changent un jour. Mais il y a des inconvénients :
    </>
  ),
  cnameRoot: (example) => (
    <>
      Impossible pour un domaine entier ({example} lui-même) : la norme n’autorise pas de CNAME à côté des
      enregistrements que tout domaine a à sa racine. Certains fournisseurs proposent à la place un enregistrement
      ALIAS, ANAME ou « aplati » qui fonctionne dans ce cas.
    </>
  ),
  cnameAlone: 'Rien d’autre ne peut cohabiter sur le même nom : pas d’enregistrement MX pour les e-mails, pas de TXT pour les vérifications.',
  cnameLookup: 'Les résolveurs des visiteurs font une requête de plus avant d’arriver.',
  copy: 'Copier',
  copyValue: (value) => `Copier ${value}`,
};
