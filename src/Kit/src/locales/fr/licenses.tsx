import type { KitMessages } from '../en';

export const licenses: KitMessages['licenses'] = {
  MIT: 'Chacun peut l’utiliser, le modifier et le diffuser, dans n’importe quel projet, tant que la licence et la mention de copyright l’accompagnent.',
  'Apache-2.0': 'Comme MIT, avec en plus une licence de brevet de chaque contributeur, et les modifications signalées comme telles.',
  'BSD-3-Clause': 'Comme MIT, et personne ne peut utiliser le nom des auteurs pour promouvoir ce qu’il en a fait.',
  'MPL-2.0': 'Les modifications de ces fichiers restent sous la même licence ; ils peuvent être combinés avec du code sous n’importe quelle autre.',
  'GPL-3.0-or-later': 'Quiconque le diffuse, modifié ou non, diffuse aussi son code source sous la même licence.',
  'AGPL-3.0-or-later': 'Comme la GPL, et proposer une copie modifiée à d’autres via le réseau compte comme une diffusion.',
  Unlicense: 'Versé dans le domaine public : chacun peut en faire ce qu’il veut, sans condition.',
};

export const kinds: KitMessages['kinds'] = {
  Permissive: 'Permissive',
  Copyleft: 'Copyleft',
  PublicDomain: 'Domaine public',
};
