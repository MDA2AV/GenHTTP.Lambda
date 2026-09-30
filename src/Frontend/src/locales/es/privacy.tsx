import type { Messages } from '../en';

export const privacy: Messages['privacy'] = {
  title: 'Política de privacidad',
  binding: (english) => (
    <>
      Esta traducción es solo informativa. La única versión vinculante de esta política es la{' '}
      {english('original en inglés')}.
    </>
  ),
  intro:
    'Qué sabe este sitio de ti, qué hace con ello, cuánto tiempo lo guarda y quién más puede verlo. En resumen: no hay cuentas, ni publicidad, ni seguimiento. El servidor anota quién le pidió qué, para poder mantenerlo en marcha y rastrear los abusos, y lo que le pides al agente de este sitio se envía a Anthropic, cuyo modelo escribe la app.',
  sections: {
    whoTitle: 'Quién es el responsable',
    who: 'Este sitio lo gestiona la persona que aparece abajo, que es la responsable del tratamiento de los datos personales del sitio según el Reglamento General de Protección de Datos de la UE (RGPD). Para cualquier cosa sobre esta página, escribe a esta dirección:',

    requestsTitle: 'Lo que el servidor anota en cada petición',
    requests: [
      'Cada petición a este sitio, y a cada lambda alojada en él, queda anotada en el registro (log) del servidor: la dirección IP desde la que llega y, si fue reenviada, la IP de origen que declara, el navegador o programa que la envió, la dirección que pidió, cuándo y qué respuesta recibió. El servidor también busca a qué país, ciudad y red pertenece la dirección IP, en una base de datos propia, sin consultar a nadie más.',
      'Así se encuentran los fallos, se averigua qué está sobrecargando el servidor y se localizan los abusos que nos denuncian. La dirección IP también se usa, solo en memoria, para limitar cuántas peticiones puede hacer y cuántas apps puede crear un mismo visitante. Sin estos datos no se puede responder a una petición. La base jurídica es nuestro interés legítimo en prestar el servicio y mantenerlo seguro (art. 6.1.f) RGPD).',
      'Los administradores pueden verlo todo. El dueño de una lambda ve el país y el navegador de cada petición a su lambda, pero no la dirección IP.',
    ],

    logsTitle: 'Cuánto tiempo se guarda el registro',
    logs: 'El registro se guarda en dos sitios: en la memoria del servidor, que se vacía cada vez que el servidor se reinicia, y en la salida de consola del servidor, que se borra cada vez que se actualiza. Los dos tienen un tamaño fijo, así que cada línea nueva desplaza a la más antigua, y cuánto dura una línea depende de cuánto movimiento tenga el sitio. Nada del registro se copia para archivarlo.',

    contentTitle: 'Lo que pones aquí',
    content: (days) =>
      `Una lambda es su código, sus archivos, sus ajustes y las notas que se guardan con sus versiones sobre qué se pidió y qué cambió. Todo eso se guarda en el servidor para poder ejecutarla y editarla. Una lambda gratuita se elimina, con todas sus versiones, unos ${days} días después de la última vez que se cambió o se visitó, y de inmediato si quien tiene su enlace de edición la borra. Cualquiera con el enlace de edición puede leerlo todo, lo que pongas en la galería lo puede ver todo el mundo, y los administradores revisan una lambda cuando hace falta, para atender una denuncia o mantener el servidor seguro. La base jurídica es prestarte el servicio que pediste (art. 6.1.b) RGPD).`,

    agentTitle: 'Lo que le pides al agente de este sitio',
    agent: (policy) => (
      <>
        Lo que escribes en el cuadro para crear apps, o en la sección Cambiar del editor de una lambda, se envía a
        Anthropic PBC, en Estados Unidos, que gestiona Claude, el modelo que escribe la app. Para hacer un cambio, el
        agente también lee la lambda (su código, las notas de sus versiones y su registro, que contiene sus peticiones y
        lo que imprimió, pero no las direcciones IP de sus visitantes), y lo que lee también se envía allí. Lo que Anthropic
        hace con ello se rige por {policy('su propia política de privacidad')}. Estados Unidos no protege los datos
        personales como lo hace la UE. Tu petición se envía allí porque hace falta para crear o cambiar lo que pediste
        (arts. 6.1.b) y 49.1.b) RGPD), así que no escribas nada que no quieras compartir.
      </>
    ),
    agentKept:
      'El agente guarda tu petición, a menudo con sus propias palabras, como nota de la versión que escribe. La petición se escribe completa en el registro del servidor, y sus primeros cientos de caracteres en el del servicio que crea las apps; los dos tienen también un tamaño fijo. Si usas tu propio agente, como Claude o Claude Code, lo que le dices va al proveedor de ese agente, no a nosotros: solo recibimos el código y las notas que envía aquí.',

    lambdasTitle: 'Lo que hace una lambda lo decide su dueño',
    lambdas:
      'Una lambda la escribe quien tiene su enlace de edición, no nosotros. Qué pide a sus visitantes y qué hace con ello lo decide esa persona, y esta página no lo cubre, salvo el registro de peticiones de arriba, que el servidor lleva para todas las lambdas. Las condiciones del servicio no permiten usar una lambda para recoger datos personales de otras personas. Si encuentras una que lo hace, denúnciala, por favor.',

    mailTitle: 'Cuando nos escribes',
    mail: 'Si nos escribes, para denunciar un abuso o por cualquier otro motivo, usamos tu dirección de correo y tu mensaje para responderte y ocuparnos de lo que nos cuentas, y los borramos cuando ya no hacen falta para eso (art. 6.1.f) RGPD).',

    storageTitle: 'Cookies y tu navegador',
    storage:
      'Hay una sola cookie, llamada lang. Recuerda el idioma que elegiste, para que las direcciones que no indican idioma se abran en él, y dura un año. El almacenamiento local del navegador recuerda el modo claro u oscuro, algunos ajustes de las páginas que usas y, en el caso de los administradores, su token. Nada de esto se usa para seguirte y nada llega a nadie más: no hay analítica, ni publicidad, y no se carga nada de otros sitios, ni siquiera las fuentes. Como todo sirve solo para lo que tú pediste, no hace falta tu consentimiento (§ 25, apdo. 2, n.º 2 de la ley alemana TDDDG).',

    hostingTitle: 'Dónde se guarda',
    hosting:
      'El servidor en el que funciona todo esto se alquila a un proveedor de alojamiento de la Unión Europea, y lo que describe esta página se guarda allí.',

    rightsTitle: 'Tus derechos',
    rights: (mailbox) => (
      <>
        Puedes preguntarnos qué datos tuyos tenemos y pedirnos una copia, que los corrijamos, los borremos o limitemos su
        uso, y oponerte a cualquier cosa que hagamos por nuestro interés legítimo (arts. 15 a 21 RGPD). Escribe a{' '}
        {mailbox}. Como no hay cuentas, solo podemos encontrar lo tuyo si nos dices cómo: la dirección IP que usaste y
        más o menos cuándo, o la dirección de tu lambda. No se toma de forma automatizada ninguna decisión que tenga
        efectos jurídicos sobre ti o que te afecte de forma igual de importante (art. 22 RGPD).
      </>
    ),
    complaint:
      'También puedes presentar una reclamación ante una autoridad de protección de datos, la de donde vives o la de donde estamos nosotros. A nosotros nos corresponde el Comisionado para la Protección de Datos y la Libertad de Información del estado alemán de Baden-Wurtemberg (LfDI Baden-Württemberg).',
  },
  change: 'Esta política cambia cuando cambia el sitio. La versión que se aplica es la de esta página.',
  updated: 'Última modificación: 30 de septiembre de 2026.',
};
