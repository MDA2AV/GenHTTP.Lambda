import type { EditorMessages } from '../../en/editor';

export const build: EditorMessages['build'] = {
  title: 'Budowanie',
  hint: 'Z czego zbudowane są zasoby lub kod wersji: pliki, na których narzędzie do budowania uruchamia ten, kto zmienia aplikację – twój agent, w sklonowanym repozytorium. Są przechowywane z każdą wersją i nigdy nie są kompilowane ani serwowane. Ta platforma niczego nie buduje, więc pliki można tu tylko czytać, nie edytować.',
  overview: 'Przegląd',
  files: 'Pliki',
  scope: (version) =>
    `Z czego zbudowana jest wersja ${version} – przechowywane razem z nią, nigdy nie kompilowane ani serwowane, budowane przez tego, kto ją zmienia, nigdy tutaj.`,
  scopeDraft: 'Z czego zbudowany jest ten szkic – przechowywane razem z nim, nigdy nie kompilowane ani serwowane, budowane przez tego, kto go zmienia, nigdy tutaj.',
  reading: 'Wczytywanie tego, z czego jest zbudowane…',
  readFailed: 'Nie udało się wczytać tego, z czego jest zbudowane.',

  emptyTitle: (version) => `Wersja ${version} nie przechowuje niczego, z czego jest zbudowana`,
  emptyTitleDraft: 'Ten szkic nie przechowuje niczego, z czego jest zbudowany',
  emptyText: (code) => (
    <>
      Jeśli zasoby lub kod wersji powstają w narzędziu do budowania – są kompilowane, pakowane lub generowane – pliki,
      z których powstają, są przechowywane tutaj, z każdą wersją: to folder {code('build/')} w sklonowanym repozytorium.
      Kto zmienia aplikację, uruchamia build u siebie i zapisuje jedno i drugie razem; ta platforma niczego nie
      buduje. To, co jest pisane tak, jak jest serwowane lub kompilowane, żadnych takich plików nie potrzebuje.
    </>
  ),
  emptyHow: (code) => (
    <>{code('AGENTS.md')} w sklonowanym repozytorium mówi agentowi programującemu, jak z niego korzystać.</>
  ),

  inVersion: (version) => `W wersji ${version}`,
  inDraft: 'W tym szkicu',
  comparedWith: (version) => `w porównaniu z wersją ${version}`,
  first: 'Pierwsza wersja, która to przechowuje.',
  both: (here, program) => {
    const files = (count: number) =>
      count === 1
        ? '1 plik'
        : `${count} ${count % 10 >= 2 && count % 10 <= 4 && (count % 100 < 12 || count % 100 > 14) ? 'pliki' : 'plików'}`
    return `Zmiana tutaj: ${files(here)}, a w kodzie i zasobach: ${files(program)}.`
  },
  hereOnly: (here) => {
    const files =
      here === 1
        ? '1 plik'
        : `${here} ${here % 10 >= 2 && here % 10 <= 4 && (here % 100 < 12 || here % 100 > 14) ? 'pliki' : 'plików'}`
    return `Zmiana tutaj: ${files}, a w kodzie i zasobach nic: jeśli to, co się zmieniło, jest w nie wbudowane, nie zostało zbudowane.`
  },
  programOnly: 'Nic się tutaj nie zmieniło.',
  unchanged: 'Nic się nie zmieniło ani tutaj, ani w kodzie czy zasobach.',
  showChanges: 'Pokaż zmiany',
  hideChanges: 'Ukryj zmiany',
  noChanges: 'Nic się tutaj nie zmieniło.',

  readme: 'Jak jest budowane',
  noReadme: (code) => (
    <>
      Nic nie mówi, jak to jest budowane. Plik {code('README.md')} na samej górze – z poleceniami i miejscem, dokąd
      trafia wynik budowania – jest tym, z czego zbuduje następny agent.
    </>
  ),
  readOnly: 'Tylko do odczytu: zmienia się to tam, gdzie jest budowane.',
  noFiles: 'Brak plików.',
};
