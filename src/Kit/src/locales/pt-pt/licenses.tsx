import type { KitMessages } from '../en';

export const licenses: KitMessages['licenses'] = {
  MIT: 'Qualquer pessoa o pode usar, alterar e redistribuir, em qualquer coisa, desde que a licença e o aviso de copyright o acompanhem.',
  'Apache-2.0': 'Como a MIT, com uma licença de patentes de todos os que contribuíram, e as alterações assinaladas como tal.',
  'BSD-3-Clause': 'Como a MIT, e ninguém pode usar o nome dos autores para promover o que fez a partir do código.',
  'MPL-2.0': 'As alterações a estes ficheiros ficam sob a mesma licença; podem ser combinados com código sob qualquer outra.',
  'GPL-3.0-or-later': 'Quem o redistribuir, alterado ou não, redistribui também o código-fonte sob a mesma licença.',
  'AGPL-3.0-or-later': 'Como a GPL, e disponibilizar uma cópia alterada a pessoas através da rede conta como redistribuí-la.',
  Unlicense: 'Cedido ao domínio público: qualquer pessoa pode fazer o que quiser com ele, sem condições.',
};

export const kinds: KitMessages['kinds'] = {
  Permissive: 'Permissiva',
  Copyleft: 'Copyleft',
  PublicDomain: 'Domínio público',
};
