import type { KitMessages } from '../en';

export const licenses: KitMessages['licenses'] = {
  MIT: 'Cualquiera puede usarlo, modificarlo y redistribuirlo, para lo que sea, siempre que la licencia y el aviso de copyright lo acompañen.',
  'Apache-2.0': 'Como MIT, con una licencia de patentes de todos los que han contribuido, y los cambios marcados como tales.',
  'BSD-3-Clause': 'Como MIT, y nadie puede usar el nombre de los autores para promocionar lo que haya hecho con él.',
  'MPL-2.0': 'Los cambios en estos archivos siguen bajo la misma licencia; se pueden combinar con código bajo cualquier otra.',
  'GPL-3.0-or-later': 'Quien lo distribuya, modificado o no, debe distribuir también su código fuente bajo la misma licencia.',
  'AGPL-3.0-or-later': 'Como la GPL, y ofrecer una copia modificada a otras personas a través de la red cuenta como distribuirla.',
  Unlicense: 'Cedido al dominio público: cualquiera puede hacer lo que quiera con él, sin condiciones.',
};

export const kinds: KitMessages['kinds'] = {
  Permissive: 'Permisiva',
  Copyleft: 'Copyleft',
  PublicDomain: 'Dominio público',
};
