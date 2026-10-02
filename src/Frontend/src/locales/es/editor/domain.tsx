import type { EditorMessages } from '../../en/editor';

export const domain: EditorMessages['domain'] = {
  readFailed: 'No se pudo leer el dominio.',
  reaching: (domain) => `Las peticiones a ${domain} ya llegan a esta lambda.`,
  saveFailed: 'No se pudo guardar el dominio.',
  removed: 'Se quitó el dominio. La lambda sigue respondiendo en su dirección de aquí.',
  removeFailed: 'No se pudo quitar el dominio.',
  hint:
    'Una lambda premium puede responder en un dominio propio (entero, desde la raíz) además de en su dirección de aquí. Apunta el dominio a este servidor, escríbelo aquí y las peticiones que le lleguen irán a la lambda.',
  loading: 'Cargando…',
  example: 'tu-dominio.com',
  open: (domain) => `Abrir ${domain}`,
  label: 'El dominio en el que responde',
  serving: (domain) => <>Ya sirve {domain}, además de su dirección de aquí.</>,
  none: 'Ninguno todavía. Puede ser un subdominio como shop.example.com o un dominio entero como example.com.',
  change: 'Cambiar',
  use: 'Usar este dominio',
  remove: 'Quitar',
  confirm: '¿Quitar el dominio?',
  keep: 'Mantenerlo',
  confirmText: (domain) => (
    <>
      Las peticiones a {domain} dejan de llegar a esta lambda al instante. Su dirección de aquí no cambia, y tampoco lo
      que diga el DNS del dominio.
    </>
  ),
  point: 'Apunta el dominio a este servidor',
  check: 'Volver a comprobar',
  records:
    'En el proveedor que gestiona el DNS del dominio, añade estos dos registros. Omite el registro AAAA si prefieres no ser accesible por IPv6.',
  type: 'Tipo',
  name: 'Nombre',
  value: 'Valor',
  pointsHere: (domain) => <>{domain} apunta aquí.</>,
  alsoElsewhere: (addresses) =>
    ` También resuelve a ${addresses}, que no es este servidor: los visitantes que lleguen por ahí no verán la lambda.`,
  elsewhere: (addresses) => `Resuelve a ${addresses}, que todavía no es este servidor.`,
  wait: 'Un cambio puede tardar en verse en todas partes: como mucho, lo que dure el TTL del registro anterior.',
  cname: 'Usar un registro CNAME en su lugar',
  cnameText: (target) => (
    <>
      Un subdominio también puede apuntar a {target} con un registro CNAME, y así sigue a este servidor si alguna vez
      cambian sus direcciones. Tiene inconvenientes:
    </>
  ),
  cnameRoot: (example) => (
    <>
      No sirve para un dominio entero ({example} en sí): el estándar no permite un CNAME junto a los registros que todo
      dominio tiene en su raíz. Algunos proveedores ofrecen un registro ALIAS, ANAME o «aplanado» que sí funciona ahí.
    </>
  ),
  cnameAlone: 'No puede haber nada más con el mismo nombre: ni un registro MX para el correo ni un TXT para verificaciones.',
  cnameLookup: 'Los resolvers DNS de los visitantes hacen una consulta más antes de llegar.',
  copy: 'Copiar',
  copyValue: (value) => `Copiar ${value}`,
};
