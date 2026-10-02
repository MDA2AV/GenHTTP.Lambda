import type { EditorMessages } from '../../en/editor';

export const versions: EditorMessages['versions'] = {
  hint: (limit) =>
    `Wersja to program – jej kod i zasoby – i po zapisaniu nigdy się nie zmienia, więc każdą można porównać i przywrócić online dokładnie taką, jaka była. Każda pamięta, o co proszono i co zmieniła. Żeby zmienić lambdę, utwórz szkic: stanie się kolejną wersją, gdy wszystko będzie gotowe. Gdy wersji jest więcej niż ${limit}, najstarsze są usuwane; ta, która jest online, nigdy.`,
  none: 'Jeszcze nie ma wersji.',
  noDescription: 'Bez opisu',
  online: 'online',
  putOnline: 'Wdróż tę wersję',
  rollBackTitle: 'Przywróć tę starszą wersję online',
  deploy: 'Wdróż',
  rollBack: 'Przywróć',
  readFailed: 'Nie udało się odczytać tej wersji.',
  comparing: 'Porównywanie…',
  unchanged: 'Nic się nie zmieniło względem poprzedniej wersji.',
  first: 'Pierwsza wersja.',
  status: { added: 'dodany', removed: 'usunięty', changed: 'zmieniony', same: 'bez zmian' },
  browse: 'Przeglądaj pliki',
  docs: 'Przeczytaj dokumentację',
  edit: 'Edytuj od tej wersji',
  feature: 'Utwórz szkic od tej wersji',
  featureTitle: 'Pracuj nad zmianą tej wersji obok lambdy i scal ją w kolejną wersję, gdy wszystko będzie gotowe',
  binary: 'To nie jest tekst, więc nie ma linii do porównania.',
  tooLarge: 'Za duży, żeby porównać go linia po linii.',
};
