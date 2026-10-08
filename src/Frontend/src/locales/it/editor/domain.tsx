import type { EditorMessages } from '../../en/editor';

export const domain: EditorMessages['domain'] = {
  readFailed: 'Impossibile leggere il dominio.',
  reaching: (domain) => `Ora le richieste a ${domain} arrivano a questa lambda.`,
  saveFailed: 'Impossibile salvare il dominio.',
  removed: 'Dominio rimosso. La lambda risponde di nuovo al suo indirizzo qui.',
  removeFailed: 'Impossibile rimuovere il dominio.',
  hint:
    'Una lambda Premium può rispondere su un dominio tutto suo (per intero, dalla radice in giù). Punta prima il dominio su questo server, poi inseriscilo qui: da quel momento le richieste al dominio arrivano alla lambda e il suo indirizzo qui manda i visitatori al dominio.',
  loading: 'Caricamento…',
  example: 'il-tuo-dominio.it',
  open: (domain) => `Apri ${domain}`,
  label: 'Il dominio su cui risponde',
  serving: (domain) => <>Ora serve {domain}. Il suo indirizzo qui porta i visitatori lì.</>,
  none: 'Ancora nessuno. Un sottodominio come shop.example.com, o un dominio intero come example.com.',
  change: 'Cambia',
  use: 'Usa questo dominio',
  remove: 'Rimuovi',
  confirm: 'Rimuovere il dominio?',
  keep: 'Tienilo',
  confirmText: (domain) => (
    <>
      Le richieste a {domain} smettono subito di arrivare a questa lambda e il suo indirizzo qui torna a rispondere
      invece di portare i visitatori altrove. Il DNS del dominio resta com’è.
    </>
  ),
  point: 'Punta il dominio su questo server',
  check: 'Controlla di nuovo',
  records:
    'Aggiungi questi due record dove gestisci il DNS del dominio. Salta il record AAAA se preferisci non essere raggiungibile via IPv6.',
  type: 'Tipo',
  name: 'Nome',
  value: 'Valore',
  pointsHere: (domain) => <>{domain} punta qui.</>,
  alsoElsewhere: (addresses) =>
    ` Risolve anche su ${addresses}, che non è questo server: i visitatori mandati lì non raggiungeranno la lambda.`,
  elsewhere: (addresses) => `Risolve su ${addresses}, che non è ancora questo server.`,
  wait: 'Una modifica può metterci un po’ a propagarsi ovunque, fino al TTL del vecchio record.',
  cname: 'Usare un record CNAME',
  cnameText: (target) => (
    <>
      Un sottodominio può invece puntare a {target} con un record CNAME, e così segue questo server se un giorno i suoi
      indirizzi cambiano. Ha però degli svantaggi:
    </>
  ),
  cnameRoot: (example) => (
    <>
      Non si può usare per un dominio intero ({example} stesso): lo standard non ammette un CNAME accanto ai record che
      ogni dominio ha alla radice. Alcuni provider offrono un record ALIAS, ANAME o «flattened» che lì funziona.
    </>
  ),
  cnameAlone: 'Sullo stesso nome non può esserci nient’altro: niente record MX per la posta, niente record TXT per le verifiche.',
  cnameLookup: 'I resolver dei visitatori fanno una ricerca DNS in più prima di arrivare.',
  copy: 'Copia',
  copyValue: (value) => `Copia ${value}`,
};
