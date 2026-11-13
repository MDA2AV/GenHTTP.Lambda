import type { EditorMessages } from '../en';

export const egress: EditorMessages['egress'] = {
  hint:
    'Si el código puede conectarse a otros servidores: llamar a una API, abrir un socket, enviar correo. Las lambdas comparten un servidor, así que se permite lambda por lambda: una premium puede, cualquier otra cuando el operador de este sitio lo haya permitido.',

  included: 'Incluido con premium',
  includedText: 'El código puede conectarse a otros servidores: premium lo incluye.',
  allowed: 'Permitido',
  allowedText: (ago) => <>El operador de este sitio lo permitió {ago}. El código puede conectarse a otros servidores.</>,
  requested: 'Esperando al operador',
  requestedText: (ago) => (
    <>Solicitado {ago}. Cuando el operador de este sitio lo permita, el código que se conecta se compilará; hasta entonces se rechaza.</>
  ),
  declined: 'Rechazado',
  declinedText: (ago) => <>El operador de este sitio lo rechazó {ago}. Puedes volver a pedirlo, explicando mejor para qué sirve.</>,
  none: 'No permitido',
  noneText:
    'El código no puede conectarse a otros servidores: el código que llama a una API, abre un socket o envía correo no se compila. Pídelo abajo si la lambda lo necesita.',
  operatorSaid: 'El operador dijo',
  purposeSaid: 'Para qué sirve',

  connects: 'El código se conecta a otros servidores, algo que esta lambda no puede hacer: no se compila hasta que se permita.',
  connectsWaiting: 'El código se conecta a otros servidores: se compilará cuando el operador lo permita.',

  askTitle: 'Pedirlo',
  askAgainTitle: 'Volver a pedirlo',
  editTitle: 'Para qué sirve',
  purpose: '¿A qué se conecta y por qué?',
  purposeHint: 'Indica los servidores o servicios y qué hace la lambda con ellos. El operador de este sitio lo lee para decidir.',
  purposeExample: 'api.open-meteo.com, para la previsión del tiempo de la página principal',
  send: 'Enviar la solicitud',
  save: 'Guardar',
  cancel: 'Cancelar',
  edit: 'Cambiar para qué sirve',
  asked: 'Solicitado. El operador de este sitio decide.',
  askFailed: 'No se pudo enviar la solicitud.',
  withdraw: 'Retirar la solicitud',
  withdrawn: 'La solicitud se ha retirado.',
  giveUp: 'Renunciar',
  givenUp: 'El código ya no puede conectarse a otros servidores.',
  giveUpTitle: '¿Renunciar a conectarse a otros servidores?',
  giveUpText:
    'A partir de su próxima petición, el código no podrá conectarse a otros servidores, y la versión en línea dejará de funcionar donde lo haga. Para recuperarlo, vuelve a pedirlo.',
  keep: 'Mantenerlo',
  failed: 'No ha funcionado.',

  coversTitle: 'Qué cuenta',
  covers: ['HttpClient y WebRequest', 'Sockets y clientes WebSocket', 'Correo (SmtpClient)', 'DNS y ping', 'XML cargado desde una dirección'],
  notCovered:
    'No lo que ocurre en el navegador: las páginas de la lambda pueden obtener datos de otros sitios por sí mismas, y los visitantes, los webhooks y los websockets llegan a la lambda como siempre. Una API pública que los navegadores pueden llamar se llama mejor desde la página: eso no necesita permiso.',

  refused: 'El código se conecta a otros servidores',
  refusedText: 'Esta lambda aún no puede hacerlo. Pídelo y el operador de este sitio decide.',
  openEgress: 'Pedirlo',
};
