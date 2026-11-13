import type { EditorMessages } from '../en';

export const connectivity: EditorMessages['connectivity'] = {
  hint:
    'Se o código pode ligar-se a outros servidores: chamar uma API, abrir um socket, enviar e-mail. As lambdas partilham um único servidor, por isso isto é permitido lambda a lambda - uma premium pode, qualquer outra depois de o operador deste site o permitir.',

  included: 'Incluído no premium',
  includedText: 'O código pode ligar-se a outros servidores: o premium inclui isso.',
  allowed: 'Permitido',
  allowedText: (ago) => <>O operador deste site permitiu-o {ago}. O código pode ligar-se a outros servidores.</>,
  requested: 'A aguardar o operador',
  requestedText: (ago) => (
    <>Pedido {ago}. Quando o operador deste site o permitir, o código que se liga compila - até lá, é recusado.</>
  ),
  declined: 'Recusado',
  declinedText: (ago) => <>O operador deste site recusou-o {ago}. Podes pedir de novo, explicando melhor para que serve.</>,
  none: 'Não permitido',
  noneText:
    'O código não pode ligar-se a outros servidores: código que chama uma API, abre um socket ou envia e-mail não compila. Pede-o abaixo se a lambda precisar.',
  operatorSaid: 'O operador disse',
  purposeSaid: 'Para que serve',

  connects: 'O código liga-se a outros servidores, o que esta lambda não pode fazer: não compila enquanto isso não for permitido.',
  connectsWaiting: 'O código liga-se a outros servidores: compila assim que o operador o permitir.',

  askTitle: 'Pedir permissão',
  askAgainTitle: 'Pedir de novo',
  editTitle: 'Para que serve',
  purpose: 'A que se liga e porquê?',
  purposeHint: 'Indica os servidores ou serviços e o que a lambda faz com eles. O operador deste site lê isto para decidir.',
  purposeExample: 'api.open-meteo.com, para a previsão do tempo na página inicial',
  send: 'Enviar o pedido',
  save: 'Guardar',
  cancel: 'Cancelar',
  edit: 'Alterar para que serve',
  asked: 'Pedido feito. O operador deste site decide.',
  askFailed: 'Não foi possível enviar o pedido.',
  withdraw: 'Retirar o pedido',
  withdrawn: 'O pedido foi retirado.',
  giveUp: 'Desistir',
  givenUp: 'O código já não pode ligar-se a outros servidores.',
  giveUpTitle: 'Desistir de ligar a outros servidores?',
  giveUpText:
    'A partir do próximo pedido, o código deixa de poder ligar-se a outros servidores, e a versão online deixa de funcionar onde o faz. Para voltar a ter essa permissão, pede de novo.',
  keep: 'Manter',
  failed: 'Não funcionou.',

  coversTitle: 'O que conta',
  covers: ['HttpClient e WebRequest', 'Sockets e clientes WebSocket', 'E-mail (SmtpClient)', 'DNS e ping', 'XML carregado a partir de um endereço'],
  notCovered:
    'Não conta o que acontece no browser: as páginas da lambda podem obter dados de outros sites por si, e visitantes, webhooks e websockets chegam à lambda como sempre. Uma API pública que os browsers podem chamar é melhor chamada a partir da página - isso não precisa de permissão.',

  refused: 'O código liga-se a outros servidores',
  refusedText: 'Esta lambda ainda não pode fazer isso. Pede permissão e o operador deste site decide.',
  openConnectivity: 'Pedir permissão',
};
