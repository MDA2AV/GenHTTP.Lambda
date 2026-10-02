import type { EditorMessages } from '../../en/editor';

export const showcase: EditorMessages['showcase'] = {
  loadFailed: 'Nie udało się załadować wpisu w galerii.',
  loading: 'Ładowanie…',
  // what is still missing, after "Brakuje jeszcze": so in the genitive
  title: 'tytułu',
  description: 'opisu',
  picture: 'obrazka',
  updated: 'Wpis w galerii został zaktualizowany.',
  listed: 'Lambda jest już w galerii.',
  waiting: 'Zapisano. Wpis pojawi się w galerii, gdy tylko lambda będzie online.',
  saveFailed: 'Nie udało się zapisać wpisu w galerii.',
  removed: 'Usunięto z galerii.',
  removeFailed: 'Nie udało się usunąć wpisu z galerii.',
  wrongType: 'To nie jest obrazek PNG, JPEG, GIF ani WebP.',
  tooLarge: (size, limit) => `Ten plik ma ${size}, a obrazek może mieć najwyżej ${limit}.`,
  unreadable: 'Nie udało się odczytać pliku.',
  hint: (tool) => (
    <>
      Galeria pokazuje lambdy, które ich właściciele postanowili pokazać – najpierw te ostatnio używane. Tylko osoba z
      kluczem edytora może dodać lambdę do galerii albo ją z niej usunąć, a lambda jest tam widoczna tylko wtedy, gdy
      jest online. Agent może zrobić to samo narzędziem {tool}.
    </>
  ),
  open: 'Otwórz galerię',
  switch: 'Pokaż tę lambdę w galerii',
  listedNow: 'Widoczna w galerii. Każdy, kto przegląda galerię, może ją otworzyć.',
  notListed: 'Zapisano, ale lambda jest offline, więc nie ma jej w galerii. Wróci tam, gdy znów ją wdrożysz.',
  off: 'Wyłączone. Nic o tej lambdzie nie jest nigdzie pokazywane, dopóki tego nie włączysz i nie zapiszesz.',
  offline: 'Lambda jest offline, więc wpis poczeka, aż zostanie wdrożona. W galerii są tylko lambdy, które odpowiadają.',
  titleLabel: 'Tytuł',
  titlePlaceholder: 'Wyniki quizu w pubie',
  descriptionLabel: 'Opis',
  descriptionPlaceholder:
    'Drużyny wpisują odpowiedzi na telefonach, prowadzący je ocenia, a tablica wyników aktualizuje się u wszystkich na sali.',
  save: 'Zapisz zmiany',
  add: 'Dodaj do galerii',
  takeOff: 'Usuń z galerii',
  needs: (missing) =>
    `Brakuje jeszcze ${missing.length > 1 ? `${missing.slice(0, -1).join(', ')} i ${missing[missing.length - 1]}` : missing[0]}.`,
  tooLong: 'Część tekstu jest za długa.',
  allSaved: 'Wszystko zapisane.',
  preview: 'Podgląd',
  card: (address) => <>Tak odwiedzający zobaczą tę kartę. Prowadzi do {address}.</>,
  confirm: 'Usunąć z galerii?',
  keep: 'Zostaw',
  confirmText: 'Tytuł, opis i obrazek zostaną usunięte. Sama lambda zostaje bez zmian.',
  pictureLabel: 'Obrazek',
  formats: (limit) => `PNG, JPEG, GIF lub WebP, maks. ${limit}`,
  notSaved: 'jeszcze niezapisany',
  replace: 'Przeciągnij tu nowy, żeby go zastąpić.',
  drop: 'Przeciągnij tu obrazek.',
  advice: 'Najlepiej sprawdzi się zrzut ekranu albo krótki GIF z działania aplikacji, w proporcjach 16:10.',
  another: 'Wybierz inny',
  choose: 'Wybierz plik',
  keepSaved: 'Zostaw zapisany',
  clear: 'Wyczyść',
};
