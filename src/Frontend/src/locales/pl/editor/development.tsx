import type { EditorMessages } from '../../en/editor';

export const development: EditorMessages['development'] = {
  title: 'Przestrzeń deweloperska',
  hint: 'To, z czego budowane są zasoby wersji, tam gdzie robi to toolchain: projekt jej frontendu ze źródłami, konfiguracją i plikiem blokady. Jest przechowywana z każdą wersją i nigdy nie jest kompilowana ani serwowana. Kto ją zmienia – twój agent, w sklonowanym repozytorium – buduje ją u siebie i zapisuje razem z tym, co zbudował: ta platforma niczego nie buduje. Dlatego tutaj można ją tylko czytać, nie edytować.',
  overview: 'Przegląd',
  files: 'Pliki',
  scope: (version) =>
    `To, z czego zbudowane są zasoby wersji ${version} – przechowywane razem z nią, nigdy nie kompilowane ani serwowane, a budowane przez tego, kto je zmienia, nigdy tutaj.`,
  scopeDraft: 'To, z czego zbudowane są zasoby tego szkicu – przechowywane razem z nim, nigdy nie kompilowane ani serwowane, a budowane przez tego, kto je zmienia, nigdy tutaj.',
  reading: 'Wczytywanie przestrzeni deweloperskiej…',
  readFailed: 'Nie udało się wczytać przestrzeni deweloperskiej.',

  emptyTitle: (version) => `Brak przestrzeni deweloperskiej w wersji ${version}`,
  emptyTitleDraft: 'Brak przestrzeni deweloperskiej w tym szkicu',
  emptyText: (code) => (
    <>
      Jeśli frontend jest budowany toolchainem – React, Vue lub Svelte z Vite, TypeScript, Tailwind – jego projekt jest
      przechowywany tutaj, z każdą wersją: to, z czego zbudowane są zasoby. Twój agent buduje go u siebie i zapisuje
      źródła razem z tym, co zbudował – w sklonowanym repozytorium to folder {code('dev/')}. Frontend z samego HTML,
      CSS i JavaScriptu nie potrzebuje żadnej.
    </>
  ),
  emptyHow: (code) => (
    <>{code('AGENTS.md')} w sklonowanym repozytorium podpowiada agentowi, jak ją założyć.</>
  ),

  projects: 'Projekty',
  atTheTop: 'sama przestrzeń deweloperska',
  kinds: {
    npm: 'npm',
    deno: 'Deno',
    cargo: 'Rust',
    go: 'Go',
    python: 'Python',
    dotnet: '.NET',
    php: 'PHP',
    ruby: 'Ruby',
    maven: 'Maven',
    gradle: 'Gradle',
    make: 'Make',
  },
  builtWith: 'Zbudowane przy użyciu',
  build: 'Build',
  noBuild: 'Brak skryptu build w jego package.json.',
  into: 'Buduje do',
  intoAssets: (folder, files, size) => {
    const word = files === 1 ? 'plik' : files % 10 >= 2 && files % 10 <= 4 && (files % 100 < 10 || files % 100 >= 20) ? 'pliki' : 'plików'
    return (
      <>
        {folder} zasobów – {files === 1 ? '1 plik' : `${files} ${word}`}, {size} w tej wersji
      </>
    )
  },
  intoNothing: (folder) => <>{folder} zasobów – który w tej wersji jest pusty</>,
  packages: 'Pakiety',
  packagesCount: (runtime, tooling) =>
    `${runtime} do uruchamiania, ${tooling} do budowania`,
  showPackages: 'Pokaż je',
  hidePackages: 'Ukryj je',
  runtime: 'Do uruchamiania',
  tooling: 'Do budowania',
  missing: (page, files) => (
    <>
      {page} odwołuje się do plików, których nie ma wśród zasobów ({files.slice(0, 3).join(', ')}{files.length > 3 ? ', …' : ''}): to, co zapisał build, nie zostało zapisane w całości, więc strona się nie wczytuje.
    </>
  ),
  noLock: 'Brak pliku blokady: następny build może zainstalować inne wersje pakietów niż poprzedni.',
  noIgnore: 'Brak .gitignore: to, co instaluje i buduje jego toolchain, może trafić do wersji.',

  inVersion: (version) => `W wersji ${version}`,
  inDraft: 'W tym szkicu',
  comparedWith: (version) => `w porównaniu z wersją ${version}`,
  first: 'Pierwsza wersja, która ją ma.',
  both: (here, assets) => {
    const word = (n: number) => (n === 1 ? 'plik' : n % 10 >= 2 && n % 10 <= 4 && (n % 100 < 10 || n % 100 >= 20) ? 'pliki' : 'plików')
    return `Zmienione tutaj: ${here === 1 ? '1 plik' : `${here} ${word(here)}`}, oraz w zasobach: ${assets === 1 ? '1 plik' : `${assets} ${word(assets)}`}.`
  },
  hereOnly: (here) => {
    const word = here === 1 ? 'plik' : here % 10 >= 2 && here % 10 <= 4 && (here % 100 < 10 || here % 100 >= 20) ? 'pliki' : 'plików'
    return `Zmienione tutaj: ${here === 1 ? '1 plik' : `${here} ${word}`}, w zasobach nic: o ile zmiana nie wymagała buildu, odwiedzający dostaną to samo co wcześniej.`
  },
  builtOnly: (folder) => (
    <>Zmieniło się to, co zbudowano do {folder}, a nic tutaj: zmiana wprowadzona w tym, co zapisał build, zostanie cofnięta przez następny build.</>
  ),
  assetsOnly: 'Nic tutaj się nie zmieniło.',
  unchanged: 'Nic się nie zmieniło ani tutaj, ani w zasobach.',
  showChanges: 'Pokaż zmiany',
  hideChanges: 'Ukryj zmiany',
  noChanges: 'Nic tutaj się nie zmieniło.',

  readme: 'Jak to jest zbudowane',
  noReadme: (code) => (
    <>
      Nic nie mówi, jak to jest zbudowane. {code('README.md')} na górze przestrzeni deweloperskiej – polecenia i miejsce,
      do którego trafia build – to, z czego skorzysta następny agent.
    </>
  ),
  readOnly: 'Tylko do odczytu: zmienia się ją tam, gdzie jest budowana.',
  noFiles: 'Brak plików.',
};
