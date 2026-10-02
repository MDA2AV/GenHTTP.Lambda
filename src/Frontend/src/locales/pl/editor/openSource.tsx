import type { EditorMessages } from '../../en/editor';
import { counted } from '../plural';

export const openSource: EditorMessages['openSource'] = {
  loading: 'Ładowanie…',
  loadFailed: 'Nie udało się sprawdzić, czy kod jest opublikowany.',
  hint: (tool) => (
    <>
      Opublikowaną lambdę każdy może czytać, oznaczać gwiazdką i pobierać na jej własnej stronie w dziale Open source
      – każdą jej wersję, na wybranej przez ciebie licencji, i nigdy z danymi, które przechowuje. Opublikować ją albo
      wycofać publikację może tylko osoba z kluczem edytora. Agent może zrobić to samo narzędziem {tool}.
    </>
  ),
  hintSimple:
    'Każdy może na osobnej stronie przeczytać, jak zbudowana jest twoja aplikacja, i rozwijać ją na wybranej przez ciebie licencji – ale nigdy z tym, co przechowuje. Opublikować ją albo wycofać publikację możesz tylko ty.',
  open: 'Otwórz stronę z kodem',
  switch: 'Opublikuj kod tej aplikacji',
  publishedNow: (license) => `Opublikowany na licencji ${license}. Każdy może go czytać i pobierać.`,
  off: 'Wyłączone. Nikt nie widzi kodu, dopóki go nie opublikujesz.',
  keptStars: (stars) =>
    stars === 1
      ? 'Zdobyta gwiazdka zostanie zachowana na wypadek ponownej publikacji.'
      : `Zdobyte gwiazdki (${stars}) zostaną zachowane na wypadek ponownej publikacji.`,
  published: 'Opublikowano. Każdy może teraz czytać kod.',
  saved: 'Zapisano.',
  saveFailed: 'Nie udało się opublikować kodu.',
  withdrawn: 'Publikacja wycofana. Strony już nie ma.',
  withdrawFailed: 'Nie udało się wycofać publikacji.',
  whatTitle: 'Co jest publikowane',
  what: [
    'Kod – taki, jaki jest teraz, i każdy jego wcześniejszy stan',
    'Wszystko, co aplikacja pokazuje: strony, style i obrazki',
    'To, co o niej napisano: do czego służy i jak jest testowana',
    'Każda zmiana, przez którą przeszła – każda w jednej linijce',
  ],
  neverTitle: 'Czego nigdy się nie publikuje',
  never: [
    'To, co aplikacja przechowuje: jej wpisy, zapisane pliki, klucze i hasła',
    'Twoje prośby, sformułowane twoimi słowami',
    'Kto z niej korzysta: odwiedzający i to, co robili',
    'Link do edytora',
  ],
  careful:
    'Wszystko, co jest w kodzie, staje się publiczne – także jego wcześniejsze stany. Hasłom i kluczom nie miejsce w kodzie: ich miejsce jest wśród kluczy i haseł w sekcji Dane, których nigdy się nie publikuje.',
  licenseLabel: 'Licencja',
  licenseHint:
    'Co inni mogą robić z kodem. MIT, najpopularniejsza, pozwala każdemu zrobić z nim prawie wszystko, o ile przy kodzie zostanie twoje nazwisko.',
  readLicense: 'Przeczytaj licencję',
  authorLabel: 'Nazwa w licencji',
  optional: 'opcjonalnie',
  authorPlaceholder: (key) => `Autorzy aplikacji ${key}`,
  authorHint:
    'Twoje imię i nazwisko albo nazwa organizacji, widoczne na stronie z kodem i w licencji. Jeśli zostawisz to pole puste, licencja wskaże autorów tej aplikacji.',
  publish: 'Opublikuj',
  save: 'Zapisz zmiany',
  allSaved: 'Wszystko zapisane.',
  takeDown: 'Wycofaj publikację',
  confirm: 'Wycofać publikację kodu?',
  confirmText:
    'Strona i pobrania znikną od razu. Kto już pobrał kod, zachowuje go na licencji, na której go dostał. Zdobyte gwiazdki zostaną zachowane na wypadek ponownej publikacji.',
  keep: 'Zostaw opublikowany',
  stars: (count) => counted(count, 'gwiazdka', 'gwiazdki', 'gwiazdek'),
  sidebar: 'Kod źródłowy',
};
