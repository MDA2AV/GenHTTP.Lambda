import type { Messages } from '../en';

export const showcase: Messages['showcase'] = {
  eyebrow: 'Galeria',
  title: 'Powstały tutaj. Działają teraz.',
  intro:
    'Lambdy, które ich właściciele postanowili pokazać. Wszystkie są online, więc każda karta otwiera prawdziwą aplikację. Najpierw te, z których ktoś ostatnio korzystał.',
  // prerendered with a placeholder in place of the number: only 1 may have words of its own
  counted: (total) => (total === 1 ? '1 lambda w galerii' : `Lambdy w galerii: ${total}`),
  failed: 'Nie udało się załadować galerii.',
  loadingMore: 'Ładowanie kolejnych…',
  showMore: 'Pokaż więcej',
  nothingTitle: 'Na razie pusto',
  nothing: (tab) => (
    <>
      Masz coś, co działa? Otwórz centrum sterowania, wybierz {tab('Galeria')} i dodaj tytuł, kilka słów opisu i
      obrazek. Aplikacja będzie tu widoczna, dopóki jest online.
    </>
  ),
  buildOne: 'Zbuduj aplikację',
  yoursTitle: 'Chcesz tu pokazać swoją?',
  yours: (tab) => (
    <>
      Otwórz centrum sterowania swojej lambdy i wybierz {tab('Galeria')} albo poproś agenta, który ją zbudował, żeby
      dodał ją do galerii. Może to zrobić tylko osoba z kluczem edytora, a wpis da się w każdej chwili usunąć.
    </>
  ),
  buildSomething: 'Zbuduj coś',
};

export const card: Messages['card'] = {
  noPicture: 'Jeszcze bez obrazka',
  title: 'Tytuł',
  description: 'Co można tu zrobić.',
  opens: (title, address) => `${title} – otwiera ${address} w nowej karcie`,
};
