import type { EditorMessages } from '../../en/editor';

export const domain: EditorMessages['domain'] = {
  readFailed: 'Não foi possível ler o domínio.',
  reaching: (domain) => `As requisições para ${domain} agora chegam a esta lambda.`,
  saveFailed: 'Não foi possível salvar o domínio.',
  removed: 'Domínio removido. A lambda volta a responder no endereço daqui.',
  removeFailed: 'Não foi possível remover o domínio.',
  hint:
    'Uma lambda premium pode responder em um domínio próprio (o domínio inteiro, a partir da raiz). Aponte primeiro o domínio para este servidor e informe-o aqui: a partir daí, as requisições para ele chegam à lambda e o endereço daqui encaminha os visitantes para ele.',
  loading: 'Carregando…',
  example: 'seu-dominio.com.br',
  open: (domain) => `Abrir ${domain}`,
  label: 'Domínio em que ela responde',
  serving: (domain) => <>Servindo {domain} agora. O endereço daqui encaminha os visitantes para ele.</>,
  none: 'Nenhum ainda. Um subdomínio como loja.example.com, ou um domínio inteiro como example.com.',
  change: 'Mudar',
  use: 'Usar este domínio',
  remove: 'Remover',
  confirm: 'Remover o domínio?',
  keep: 'Manter',
  confirmText: (domain) => (
    <>
      As requisições para {domain} param de chegar a esta lambda na hora, e o endereço daqui volta a responder em vez de
      encaminhar os visitantes. O que o DNS do domínio diz continua como está.
    </>
  ),
  point: 'Aponte o domínio para este servidor',
  check: 'Verificar de novo',
  records:
    'No provedor que gerencia o DNS do domínio, adicione estes dois registros. Deixe de fora o registro AAAA se preferir não ficar acessível via IPv6.',
  type: 'Tipo',
  name: 'Nome',
  value: 'Valor',
  pointsHere: (domain) => <>{domain} aponta para cá.</>,
  alsoElsewhere: (addresses) =>
    ` Ele também resolve para ${addresses}, que não é este servidor: visitantes mandados para lá não chegam à lambda.`,
  elsewhere: (addresses) => `Ele resolve para ${addresses}, que ainda não é este servidor.`,
  wait: 'Uma mudança pode levar um tempo para valer em todo lugar, até o TTL do registro antigo.',
  cname: 'Usar um registro CNAME',
  cnameText: (target) => (
    <>
      Um subdomínio pode apontar para {target} com um registro CNAME e, assim, acompanhar este servidor se os
      endereços dele mudarem. Isso tem desvantagens:
    </>
  ),
  cnameRoot: (example) => (
    <>
      Não serve para um domínio inteiro ({example} em si): o padrão não permite um CNAME junto dos registros que todo
      domínio tem na raiz. Alguns provedores oferecem um registro ALIAS, ANAME ou “flattened” que funciona nesse caso.
    </>
  ),
  cnameAlone: 'Nada mais pode ficar no mesmo nome: nenhum registro MX para e-mail, nenhum TXT para verificações.',
  cnameLookup: 'Os resolvedores dos visitantes fazem uma consulta a mais antes de chegar.',
  copy: 'Copiar',
  copyValue: (value) => `Copiar ${value}`,
};
