import type { EditorMessages } from '../../en/editor';
import { counted } from '../plural';

export const features: EditorMessages['features'] = {
  hint:
    'Szkic to kopia twojej aplikacji, na której wypróbowujesz zmianę, zanim ktokolwiek ją zobaczy, z własnym adresem i danymi testowymi. Udostępnij go, gdy wszystko będzie w porządku; do tego czasu odwiedzający dostają to, co jest teraz online.',
  newFeature: 'Nowy szkic',
  full: (limit) =>
    `Masz już ${counted(limit, 'szkic', 'szkice', 'szkiców')} – więcej być nie może. Najpierw udostępnij któryś albo go usuń.`,
  emptyTitle: 'Brak szkiców',
  emptyText:
    'Szkic to kopia twojej aplikacji, na której wypróbowujesz zmianę, zanim trafi online. Gdy agent zostawi zmianę do wypróbowania, znajdziesz ją tutaj.',
  start: 'Nowy szkic',
  askAgentNew: 'Poproś agenta o zmianę',
  noChange: 'Jeszcze nie opisano, co zmienia',
  behindTitle: 'Twoja aplikacja zmieniła się od początku tego szkicu',
  behind: () => 'nieaktualny',
  previewOnline: 'podgląd działa',
  previewOutdated: 'podgląd pokazuje wcześniejszy zapis',
  previewOffline: 'podgląd nie działa',
  changed: 'zmieniono',
  openPreview: 'Wypróbuj',
  openPreviewTitle: 'Otwórz podgląd w nowej karcie',
  count: (open, limit) => `${open} z ${limit} szkiców`,
  loading: 'Ładowanie szkicu…',
  readFailed: 'Nie udało się odczytać szkicu.',

  newTitle: 'Nowy szkic',
  newText:
    'Kopia twojej aplikacji i jej danych, z własnym adresem. Zmieniaj ją i wypróbowuj tam – odwiedzający nic z tego nie zobaczą, dopóki jej nie udostępnisz.',
  newTextFiles:
    'To, co wpisano, trafi do szkicu zamiast stać się wersją, więc możesz to wypróbować pod jego własnym adresem, zanim trafi online.',
  name: 'Nazwa',
  namePlaceholder: 'Ranking',
  wanted: 'Co ma robić?',
  wantedPlaceholder: 'Opcjonalnie. Zapamiętuj dziesięć najlepszych wyników i pokazuj je po każdej grze.',
  olderBase: (newest) =>
    `Ten szkic zaczyna się od starszej wersji, więc od początku jest nieaktualny: zanim trafi online, trzeba do niego wprowadzić to, co zmieniło się do wersji ${newest} włącznie.`,
  create: 'Utwórz szkic',
  createFailed: 'Nie udało się utworzyć szkicu.',
  retry: 'Spróbuj ponownie',
  madeNotSaved: (name) =>
    `Utworzono szkic „${name}”, ale tego, co jest wpisane, nie udało się jeszcze do niego zapisać. Spróbuj ponownie albo zamknij to okno i znajdź szkic w sekcji Szkice.`,
  created: (name) => `Utworzono szkic „${name}”.`,
  cancel: 'Anuluj',

  featureHint:
    'Kopia twojej aplikacji, na której wypróbowujesz tę zmianę. Jej podgląd ma własny adres i dane testowe, więc odwiedzający nic z tego nie zobaczą, dopóki jej nie udostępnisz.',
  askAgent: 'Poproś agenta',
  askCatchUp: 'Poproś agenta o zaktualizowanie go',
  catchUp: 'Zaktualizuj ten szkic do najnowszej wersji aplikacji i zachowaj to, co w nim zmieniono.',
  editCode: 'Edytuj kod',
  deployPreview: 'Uruchom podgląd',
  updatePreview: 'Zaktualizuj podgląd',
  previewDeployed: 'Podgląd działa.',
  previewFailed: 'Nie udało się uruchomić podglądu.',
  previewStopped: 'Podgląd został zatrzymany.',
  previewRejected: 'Podgląd się nie zmienił',
  previewNotCompiling: 'Kod się nie kompiluje. Podgląd nadal pokazuje to, co wcześniej.',
  started: 'Utworzono',
  changes: () => 'Zmienione pliki',
  noChanges: () => 'Na razie nic się nie zmieniło.',
  editNotes: 'Nazwa i notatki',
  what: 'Co zmienia?',
  whatPlaceholder: 'Dodaje ranking z dziesięcioma najlepszymi wynikami',
  missed: () => 'Co zmieniło się w twojej aplikacji od rozpoczęcia szkicu',
  missedNothing: 'Nic w plikach.',

  behindText: (_base, newest) =>
    `Wersja ${newest} twojej aplikacji została zapisana po utworzeniu tego szkicu. Udostępnienie go teraz cofnęłoby to, co ta wersja zmieniła, więc najpierw trzeba go zaktualizować – agent może to zrobić za ciebie.`,
  moveBase: 'Oznacz jako aktualny',
  close: 'Zamknij',
  mergeTitle: (name) => `Udostępnij „${name}”`,
  mergeTitleShort: 'Zrób z niego nową wersję aplikacji i udostępnij ją',
  leaks: (path, files) =>
    `W ${files} linki prowadzą do ${path}, czyli do twojej działającej aplikacji. Z podglądu te linki odczytują i zmieniają jej prawdziwe dane zamiast danych testowych. Poproś agenta, aby linkował bez tej części („api/items”).`,
  mergeButton: 'Udostępnij',
  saveFirst: 'Najpierw zapisz zmiany: podgląd i udostępnienie używają tego, co jest zapisane.',
  mergeAndDeploy: () => 'Udostępnij',
  mergeText: (version) =>
    `Szkic stanie się wersją ${version} twojej aplikacji i trafi online. Dane aplikacji pozostaną bez zmian.`,
  deployTooNote: (active) => `Do wersji ${active} wrócisz jednym kliknięciem w sekcji wersji.`,
  deployTooOffline: 'Twoja aplikacja jest teraz offline; w ten sposób ją udostępnisz.',
  notCompiling: 'Kod się nie kompiluje, więc nie został udostępniony. Najpierw popraw go w szkicu.',
  mergeFailed: 'Nie udało się udostępnić szkicu.',
  merged: (version) => `Zapisano jako wersję ${version}.`,
  mergedOnline: (version) => `Wersja ${version} jest online.`,

  notesTitle: 'Nazwa i notatki',
  save: 'Zapisz',
  saveFailed: 'Nie udało się tego zapisać.',

  baseTitle: 'Oznaczyć jako aktualny?',
  baseText: () =>
    'Tylko szkic, który zawiera zmiany z najnowszej wersji, może zostać udostępniony bez ich cofnięcia. Jeśli te zmiany są już w tym szkicu – wprowadzone przez ciebie lub przez agenta – oznacz go jako aktualny.',
  moveTo: () => 'Oznacz jako aktualny',
  baseWarning: 'Nic tego nie sprawdza. Jeśli zmian nie ma w szkicu, udostępnienie go je cofnie.',

  deleteTitle: (name) => `Odrzucić „${name}”?`,
  deleteText: 'Jego kod, podgląd i dane testowe zostaną usunięte na zawsze. Twoja aplikacja i jej wersje pozostaną bez zmian.',
  keep: 'Zostaw',
  deleteForGood: 'Odrzuć',
  deleteFailed: 'Nie udało się odrzucić szkicu.',
  deleted: (name) => `Szkic „${name}” został odrzucony.`,

  all: 'Wszystkie szkice',
  actions: 'Więcej opcji szkicu',
  download: 'Pobierz jako zip',
  stopPreview: 'Wyłącz podgląd',
  delete: 'Odrzuć ten szkic',
  viewsLabel: 'Szkic',
  views: {
    overview: 'Szkic',
    docs: 'Dokumentacja',
    code: 'Kod',
    tests: 'Testy',
    data: 'Dane testowe',
    logs: 'Logi',
  },
  missingTitle: 'Tego szkicu już nie ma',
  missingText: 'Został udostępniony albo odrzucony. W wersjach widać, co się z nim stało.',
};
