import type { KitMessages } from '../en';

export const licenses: KitMessages['licenses'] = {
  MIT: 'Qualquer pessoa pode usar, modificar e redistribuir, em qualquer coisa, desde que a licença e o aviso de copyright acompanhem o código.',
  'Apache-2.0': 'Como a MIT, com uma licença de patentes de todos que contribuíram, e as mudanças marcadas como mudanças.',
  'BSD-3-Clause': 'Como a MIT, e ninguém pode usar o nome dos autores para promover o que fez a partir do código.',
  'MPL-2.0': 'As mudanças nestes arquivos continuam sob a mesma licença; eles podem ser combinados com código sob qualquer outra.',
  'GPL-3.0-or-later': 'Quem repassar o código, modificado ou não, repassa também o código-fonte sob a mesma licença.',
  'AGPL-3.0-or-later': 'Como a GPL, e oferecer uma cópia modificada a pessoas pela rede conta como repassá-la.',
  Unlicense: 'Entregue ao domínio público: qualquer pessoa pode fazer o que quiser com ele, sem condições.',
};

export const kinds: KitMessages['kinds'] = {
  Permissive: 'Permissiva',
  Copyleft: 'Copyleft',
  PublicDomain: 'Domínio público',
};
