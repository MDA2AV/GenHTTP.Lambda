import type { KitMessages } from '../en';

export const licenses: KitMessages['licenses'] = {
  MIT: 'Siapa pun boleh memakai, mengubah, dan membagikannya, untuk apa pun, selama lisensi dan pemberitahuan hak ciptanya tetap disertakan.',
  'Apache-2.0': 'Seperti MIT, ditambah lisensi paten dari semua kontributornya, dan perubahan harus ditandai sebagai perubahan.',
  'BSD-3-Clause': 'Seperti MIT, dan tidak seorang pun boleh memakai nama para penulisnya untuk mempromosikan hasil turunannya.',
  'MPL-2.0': 'Perubahan pada file-file ini tetap berada di bawah lisensi yang sama; file-file ini boleh digabungkan dengan kode berlisensi apa pun.',
  'GPL-3.0-or-later': 'Siapa pun yang membagikannya, diubah atau tidak, wajib membagikan kode sumbernya dengan lisensi yang sama.',
  'AGPL-3.0-or-later': 'Seperti GPL, dan menyediakan salinan yang diubah kepada orang lain lewat jaringan juga dihitung sebagai membagikannya.',
  Unlicense: 'Diserahkan ke domain publik: siapa pun boleh melakukan apa saja dengannya, tanpa syarat.',
};

export const kinds: KitMessages['kinds'] = {
  Permissive: 'Permisif',
  Copyleft: 'Copyleft',
  PublicDomain: 'Domain publik',
};
