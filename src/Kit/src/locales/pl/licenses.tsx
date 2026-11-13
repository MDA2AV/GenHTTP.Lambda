import type { KitMessages } from '../en';

export const licenses: KitMessages['licenses'] = {
  MIT: 'Każdy może używać kodu, zmieniać go i przekazywać dalej, w czymkolwiek, o ile licencja i informacja o prawach autorskich pozostaną przy nim.',
  'Apache-2.0': 'Jak MIT, z licencją patentową od każdego, kto coś wniósł, i ze zmianami oznaczonymi jako zmiany.',
  'BSD-3-Clause': 'Jak MIT, a do tego nikt nie może używać nazwisk autorów, żeby promować to, co z niego zrobił.',
  'MPL-2.0': 'Zmiany w tych plikach pozostają na tej samej licencji; można je łączyć z kodem na dowolnej innej.',
  'GPL-3.0-or-later': 'Kto przekazuje kod dalej, zmieniony czy nie, przekazuje też jego źródła na tej samej licencji.',
  'AGPL-3.0-or-later': 'Jak GPL, a udostępnianie zmienionej kopii ludziom przez sieć też liczy się jako przekazanie dalej.',
  Unlicense: 'Przekazany do domeny publicznej: każdy może z nim zrobić wszystko, bez żadnych warunków.',
};

export const kinds: KitMessages['kinds'] = {
  Permissive: 'Liberalna',
  Copyleft: 'Copyleft',
  PublicDomain: 'Domena publiczna',
};
