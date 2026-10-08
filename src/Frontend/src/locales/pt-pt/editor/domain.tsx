import type { EditorMessages } from '../../en/editor';

export const domain: EditorMessages['domain'] = {
  readFailed: 'Não foi possível ler o domínio.',
  reaching: (domain) => `Os pedidos para ${domain} já chegam a esta lambda.`,
  saveFailed: 'Não foi possível guardar o domínio.',
  removed: 'O domínio foi removido. A lambda volta a responder no endereço daqui.',
  removeFailed: 'Não foi possível remover o domínio.',
  hint:
    'Uma lambda premium pode responder num domínio próprio (o domínio inteiro, a partir da raiz). Aponta primeiro o domínio para este servidor e indica-o depois aqui: a partir daí, os pedidos para ele chegam à lambda e o endereço daqui reencaminha os visitantes para ele.',
  loading: 'A carregar…',
  example: 'o-teu-dominio.pt',
  open: (domain) => `Abrir ${domain}`,
  label: 'O domínio em que responde',
  serving: (domain) => <>A servir {domain}. O endereço daqui reencaminha os visitantes para ele.</>,
  none: 'Ainda nenhum. Um subdomínio como loja.example.com, ou um domínio inteiro como example.com.',
  change: 'Mudar',
  use: 'Usar este domínio',
  remove: 'Remover',
  confirm: 'Remover o domínio?',
  keep: 'Manter',
  confirmText: (domain) => (
    <>
      Os pedidos para {domain} deixam logo de chegar a esta lambda, e o endereço daqui volta a responder em vez de
      reencaminhar os visitantes. O que o DNS do domínio indica fica como está.
    </>
  ),
  point: 'Aponta o domínio para este servidor',
  check: 'Verificar outra vez',
  records:
    'No serviço que gere o DNS do domínio, adiciona estes dois registos. Deixa de fora o registo AAAA se preferires não estar acessível por IPv6.',
  type: 'Tipo',
  name: 'Nome',
  value: 'Valor',
  pointsHere: (domain) => <>{domain} aponta para aqui.</>,
  alsoElsewhere: (addresses) =>
    ` Também resolve para ${addresses}, que não é este servidor: os visitantes enviados para lá não chegam à lambda.`,
  elsewhere: (addresses) => `Resolve para ${addresses}, que ainda não é este servidor.`,
  wait: 'Uma alteração pode demorar a ser vista em todo o lado, até ao TTL do registo antigo.',
  cname: 'Usar antes um registo CNAME',
  cnameText: (target) => (
    <>
      Em vez disso, um subdomínio pode apontar para {target} com um registo CNAME, e assim acompanha este servidor se os
      endereços dele mudarem. Tem desvantagens:
    </>
  ),
  cnameRoot: (example) => (
    <>
      Não serve para um domínio inteiro ({example} em si): a norma não permite um CNAME ao lado dos registos que todos
      os domínios têm na raiz. Alguns fornecedores oferecem um registo ALIAS, ANAME ou «flattened» que funciona aí.
    </>
  ),
  cnameAlone: 'Mais nada pode ficar no mesmo nome: nenhum registo MX para email, nenhum registo TXT para verificações.',
  cnameLookup: 'Os resolvers de DNS dos visitantes fazem mais uma consulta antes de chegarem.',
  copy: 'Copiar',
  copyValue: (value) => `Copiar ${value}`,
};
