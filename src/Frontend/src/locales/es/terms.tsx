import type { Messages } from '../en';

export const terms: Messages['terms'] = {
  title: 'Condiciones del servicio',
  binding: (english) => (
    <>Esta traducción se ofrece únicamente a título informativo. La versión vinculante es la {english('versión en inglés')}.</>
  ),
  intro:
    'Este es un servicio gratuito para realizar pruebas. Ejecuta código escrito por terceros en una infraestructura compartida, lo cual solo es viable si todos respetan unas pocas normas.',
  sections: {
    forbiddenTitle: 'Contenidos no permitidos',
    forbidden: [
      'Ni malware, ni phishing, ni mineros de criptomonedas. Nada que ataque, analice, sature o interfiera de otro modo con otros sistemas, aquí o en cualquier otro lugar. Nada que acose a otras personas. Nada que no tenga derecho a publicar, incluidos código, textos, imágenes y marcas de terceros.',
      'No utilice un lambda para almacenar ni reenviar datos personales de otras personas. Una dirección pública no tiene nada de privado, y esta plataforma no ofrece ningún medio para proteger dichos datos.',
    ],
    actionTitle: 'Medidas que podemos adoptar',
    action:
      'Cualquier contenido desplegado aquí puede desconectarse o eliminarse en cualquier momento, sin previo aviso y sin obligación de justificarlo. En la práctica, esto ocurre cuando se infringen las normas anteriores, cuando se pone en riesgo el servidor compartido o cuando alguien lo denuncia con fundamento.',
    lastingTitle: 'Duración',
    lasting: (hours, days) =>
      `Un despliegue permanece accesible aproximadamente ${hours} horas. Un lambda que no haya abierto se elimina, junto con todas las versiones de su código, unos ${days} días después de su última modificación. Guardar o desplegar cuenta como modificación, por lo que todo aquello en lo que esté trabajando se conserva. Nada de esto constituye una copia de seguridad: conserve su propia copia del código que le importe.`,
    keyTitle: 'Su enlace de edición es su contraseña',
    key: 'Cualquier persona que disponga del enlace de edición puede leer y modificar ese lambda; no hay ninguna cuenta ni contraseña asociada. Publicar el enlace equivale a permitir que otros lo modifiquen. Un enlace perdido no puede recuperarse.',
    warrantyTitle: 'Sin garantía',
    warranty:
      'El servicio se ofrece tal cual, sin garantía de que funcione, siga funcionando o conserve lo que usted almacene en él. Puede reiniciarse, modificarse o interrumpirse en cualquier momento. No base en él nada que sea importante para usted o para terceros.',
    reportTitle: 'Denuncias',
    report: (mailbox, front) => (
      <>
        Si un lambda alojado aquí hace algo indebido, escriba a {mailbox} indicando su dirección. En la{' '}
        {front('página de inicio')} encontrará qué información incluir.
      </>
    ),
  },
  change: 'Estas condiciones pueden cambiar. La versión aplicable es la publicada en esta página.',

  short:
    'Los lambdas se ejecutan en una infraestructura compartida. Al crear uno, usted se compromete a no desplegar malware, páginas de phishing, mineros de criptomonedas ni nada que ataque, analice o sature otros sistemas, y a no publicar contenidos sobre los que no tenga derechos. Cualquiera que conozca el enlace de edición puede modificar su lambda, por lo que debe tratarlo como una contraseña. Los lambdas del plan gratuito permanecen en línea mientras se utilizan: uno que nadie visite ni modifique durante un mes se desconecta y se elimina si no hay actividad durante los dos meses siguientes. Cualquier contenido desplegado puede eliminarse en cualquier momento.',
};
