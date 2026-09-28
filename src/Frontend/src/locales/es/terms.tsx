import type { Messages } from '../en';

export const terms: Messages['terms'] = {
  title: 'Condiciones del servicio',
  binding: (english) => (
    <>Esta traducción es solo informativa. La única versión vinculante es la {english('original en inglés')}.</>
  ),
  intro:
    'Este es un servicio gratuito para probar cosas. Ejecuta código escrito por desconocidos en una infraestructura compartida, y eso solo funciona si todos cumplen unas pocas reglas.',
  sections: {
    forbiddenTitle: 'Lo que no puedes publicar aquí',
    forbidden: [
      'Nada de malware, phishing ni mineros de criptomonedas. Nada que ataque, escanee, sature o interfiera de cualquier otra forma con otros sistemas, aquí o en cualquier otro lugar. Nada que acose a nadie. Nada que no tengas derecho a publicar, y eso incluye código, textos, imágenes y marcas de otras personas.',
      'No uses una lambda para guardar ni reenviar datos personales de otras personas. Una dirección pública no tiene nada de privado, y esta plataforma no te ofrece ninguna forma de proteger esos datos.',
    ],
    actionTitle: 'Lo que podemos hacer al respecto',
    action:
      'Cualquier cosa desplegada aquí puede desconectarse o eliminarse en cualquier momento, sin aviso y sin obligación de dar explicaciones. En la práctica, eso pasa cuando algo incumple las reglas de arriba, cuando pone en riesgo la máquina que todos comparten o cuando alguien lo denuncia y resulta que tiene razón.',
    lastingTitle: 'Cuánto dura cada cosa',
    lasting: (hours, days) =>
      `Un despliegue sigue accesible unas ${hours} horas. Una lambda que no hayas abierto se elimina, con todas las versiones de su código, unos ${days} días después de la última vez que la tocaste. Guardar o desplegar cuenta como tocarla, así que lo que tengas entre manos se conserva. Nada de esto es una copia de seguridad: guarda tu propia copia del código que te importe.`,
    keyTitle: 'Tu enlace de edición es tu contraseña',
    key: 'Cualquiera que tenga el enlace de edición puede leer y cambiar esa lambda, y no hay ninguna cuenta ni contraseña detrás. Si publicas el enlace, publicas también la posibilidad de cambiarla. Si lo pierdes, no hay forma de recuperarlo.',
    warrantyTitle: 'Sin garantía',
    warranty:
      'El servicio se ofrece tal cual, sin garantía de que funcione, de que siga funcionando ni de que conserve lo que pongas en él. Puede reiniciarse, cambiarse o apagarse en cualquier momento. No construyas aquí nada que sea importante para ti ni para nadie.',
    reportTitle: 'Cómo denunciar algo',
    report: (mailbox, front) => (
      <>
        Si una lambda alojada aquí hace algo que no debería, escribe a {mailbox} con su dirección. En la{' '}
        {front('página de inicio')} verás qué incluir.
      </>
    ),
  },
  change: 'Estas condiciones pueden cambiar. La versión que se aplica es la de esta página.',

  short:
    'Las lambdas se ejecutan en una infraestructura compartida. Al crear una, aceptas no desplegar malware, páginas de phishing, mineros de criptomonedas ni nada que ataque, escanee o sature otros sistemas, y no publicar contenido que no tengas derecho a publicar. Cualquiera que conozca el enlace de edición puede cambiar tu lambda, así que trátalo como una contraseña. Las lambdas del plan gratuito siguen en línea mientras se usen: si nadie visita ni edita una durante un mes, se desconecta, y se elimina si no pasa nada en los dos meses siguientes. Todo lo que despliegues puede eliminarse en cualquier momento.',
};
