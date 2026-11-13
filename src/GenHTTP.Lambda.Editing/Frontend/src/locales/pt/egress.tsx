import type { EditorMessages } from '../en';

export const egress: EditorMessages['egress'] = {
  hint:
    'Se o código pode se conectar a outros servidores: chamar uma API, abrir um socket, enviar e-mail. As lambdas compartilham um único servidor, por isso isso é permitido lambda por lambda - uma premium pode, qualquer outra depois que o operador deste site permitir.',

  included: 'Incluído no premium',
  includedText: 'O código pode se conectar a outros servidores: o premium inclui isso.',
  allowed: 'Permitido',
  allowedText: (ago) => <>O operador deste site permitiu {ago}. O código pode se conectar a outros servidores.</>,
  requested: 'Aguardando o operador',
  requestedText: (ago) => (
    <>Pedido feito {ago}. Quando o operador deste site permitir, o código que se conecta compila - até lá, ele é recusado.</>
  ),
  declined: 'Recusado',
  declinedText: (ago) => <>O operador deste site recusou {ago}. Você pode pedir de novo, explicando melhor para que serve.</>,
  none: 'Não permitido',
  noneText:
    'O código não pode se conectar a outros servidores: código que chama uma API, abre um socket ou envia e-mail não compila. Peça abaixo se a lambda precisar disso.',
  operatorSaid: 'O operador disse',
  purposeSaid: 'Para que serve',

  connects: 'O código se conecta a outros servidores, o que esta lambda não pode fazer: ele não compila até que isso seja permitido.',
  connectsWaiting: 'O código se conecta a outros servidores: ele compila assim que o operador permitir.',

  askTitle: 'Pedir permissão',
  askAgainTitle: 'Pedir de novo',
  editTitle: 'Para que serve',
  purpose: 'A que ele se conecta e por quê?',
  purposeHint: 'Informe os servidores ou serviços e o que a lambda faz com eles. O operador deste site lê isso para decidir.',
  purposeExample: 'api.open-meteo.com, para a previsão do tempo na página inicial',
  send: 'Enviar o pedido',
  save: 'Salvar',
  cancel: 'Cancelar',
  edit: 'Alterar para que serve',
  asked: 'Pedido enviado. O operador deste site decide.',
  askFailed: 'Não foi possível enviar o pedido.',
  withdraw: 'Retirar o pedido',
  withdrawn: 'O pedido foi retirado.',
  giveUp: 'Abrir mão',
  givenUp: 'O código não pode mais se conectar a outros servidores.',
  giveUpTitle: 'Abrir mão de conectar a outros servidores?',
  giveUpText:
    'A partir da próxima requisição, o código não poderá se conectar a outros servidores, e a versão no ar deixa de funcionar onde isso acontece. Para recuperar, peça de novo.',
  keep: 'Manter',
  failed: 'Não funcionou.',

  coversTitle: 'O que conta',
  covers: ['HttpClient e WebRequest', 'Sockets e clientes WebSocket', 'E-mail (SmtpClient)', 'DNS e ping', 'XML carregado de um endereço'],
  notCovered:
    'Não vale para o que acontece no navegador: as páginas da lambda podem buscar de outros sites por conta própria, e visitantes, webhooks e websockets chegam à lambda como sempre chegaram. Uma API pública que os navegadores podem chamar é melhor chamada a partir da página - isso não precisa de permissão.',

  refused: 'O código se conecta a outros servidores',
  refusedText: 'Esta lambda ainda não pode fazer isso. Peça permissão, e o operador deste site decide.',
  openEgress: 'Pedir permissão',
};
