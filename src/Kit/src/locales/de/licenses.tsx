import type { KitMessages } from '../en';

export const licenses: KitMessages['licenses'] = {
  MIT: 'Jeder darf den Code nutzen, ändern und weitergeben, für jeden Zweck, solange Lizenz und Copyright-Vermerk dabeibleiben.',
  'Apache-2.0': 'Wie MIT, dazu eine Patentlizenz von allen, die beigetragen haben; Änderungen müssen als solche gekennzeichnet sein.',
  'BSD-3-Clause': 'Wie MIT, und niemand darf mit den Namen der Autoren für das werben, was er daraus gemacht hat.',
  'MPL-2.0': 'Änderungen an diesen Dateien bleiben unter derselben Lizenz; die Dateien dürfen mit Code unter jeder anderen Lizenz kombiniert werden.',
  'GPL-3.0-or-later': 'Wer den Code weitergibt, ob geändert oder nicht, gibt auch seinen Quelltext unter derselben Lizenz weiter.',
  'AGPL-3.0-or-later': 'Wie die GPL – und eine geänderte Fassung anderen über das Netzwerk anzubieten, gilt als Weitergabe.',
  Unlicense: 'Gemeinfrei: Jeder darf ohne Bedingungen alles damit tun.',
};

export const kinds: KitMessages['kinds'] = {
  Permissive: 'Freizügig',
  Copyleft: 'Copyleft',
  PublicDomain: 'Gemeinfrei',
};
