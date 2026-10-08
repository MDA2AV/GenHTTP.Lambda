import type { Messages } from '../en';

export const shell: Messages['shell'] = {
  main: 'Nawigacja główna',
  build: 'Stwórz stronę',
  ship: 'Opublikuj',
  showcase: 'Galeria',
  enterprise: 'Dla firm',
  docs: 'Dokumentacja',
  admin: 'Admin',
  lightMode: 'Włącz jasny motyw',
  darkMode: 'Włącz ciemny motyw',
  openMenu: 'Otwórz menu',
  closeMenu: 'Zamknij menu',
  language: 'Język',
  terms: 'Regulamin',
  privacy: 'Polityka prywatności',
  imprint: 'Nota prawna',
  writeCode: 'Napisz kod samodzielnie',
  contact: 'Kontakt',
};

export const common: Messages['common'] = {
  loading: 'Ładowanie…',
  loadingEditor: 'Ładowanie edytora…',
  editorFailed: 'Nie udało się załadować edytora',
  pageFailed: 'Nie udało się załadować strony',
  editorFailedWhy: 'Zwykle oznacza to, że strona została zaktualizowana, gdy ta karta była otwarta.',
  reload: 'Odśwież stronę',
  backToStart: 'Wróć na stronę główną',
  tryAgain: 'Spróbuj ponownie',
  copy: 'Kopiuj',
  copied: 'Skopiowano',
  copyToClipboard: 'Kopiuj do schowka',
  openInNewTab: 'Otwórz w nowej karcie',
  close: 'Zamknij',
  operatorCountry: 'Niemcy',
};

export const notFound: Messages['notFound'] = {
  title: 'Nie znaleziono strony',
  heading: 'Ta strona nie istnieje',
  text: 'Link może być nieaktualny albo lambda, do której prowadził, została usunięta.',
};

export const abuse: Messages['abuse'] = {
  report: 'Zgłoś nadużycie',
  title: 'Zgłoś lambdę',
  write: 'Napisz do nas',
  subject: 'Zgłoszenie nadużycia',
  intro:
    'Każdy może tu wrzucić kod, więc czasem ktoś wrzuca coś, czego nie powinien. Jeśli hostowana tu strona próbuje oszukiwać ludzi, coś atakuje albo używa materiałów, do których nie ma praw, daj nam znać – usuniemy ją.',
  how: (mailbox, strong, path) => (
    <>
      Napisz na {mailbox} i podaj {strong('adres strony')} – wygląda tak: {path} – oraz jedno zdanie o tym, co jest nie
      tak. Zrzut ekranu też się przyda. Nie potrzebujesz konta ani nie musisz korzystać z tej strony.
    </>
  ),
  next: (strong, terms) => (
    <>
      {strong('Co dalej.')} Twoje zgłoszenie przeczyta człowiek. Jeśli lambda łamie {terms('regulamin')}, wyłączamy ją,
      zwykle w ciągu doby. Nie zdradzimy, kto ją tu umieścił, i nie obiecujemy odpowiedzi na każde zgłoszenie – ale
      czytamy każde.
    </>
  ),
  danger:
    'Jeśli komuś grozi bezpośrednie niebezpieczeństwo albo dochodzi do przestępstwa, zawiadom też odpowiednie służby. My możemy usunąć stronę – nic więcej nie jesteśmy w stanie zrobić.',
};
