import type { EditorMessages } from '../../en/editor';

export const development: EditorMessages['development'] = {
  title: 'Ontwikkelruimte',
  hint: 'Waaruit de assets van een versie worden gebouwd, waar een toolchain ze bouwt: het project van de frontend, met de bronnen, de configuratie en het lockbestand. Het wordt bij elke versie bewaard en nooit gecompileerd of geserveerd. Wie het wijzigt – jouw agent, in een kloon – bouwt het op de plek waar hij werkt en slaat het samen met het resultaat op: dit platform bouwt niets. Het wordt hier dus gelezen, niet bewerkt.',
  overview: 'Overzicht',
  files: 'Bestanden',
  scope: (version) =>
    `Waaruit de assets van versie ${version} worden gebouwd – bij die versie bewaard, nooit gecompileerd of geserveerd, en gebouwd door wie het wijzigt, nooit hier.`,
  scopeDraft: 'Waaruit de assets van dit concept worden gebouwd – bij het concept bewaard, nooit gecompileerd of geserveerd, en gebouwd door wie het wijzigt, nooit hier.',
  reading: 'De ontwikkelruimte wordt gelezen…',
  readFailed: 'De ontwikkelruimte kon niet worden gelezen.',

  emptyTitle: (version) => `Geen ontwikkelruimte in versie ${version}`,
  emptyTitleDraft: 'Geen ontwikkelruimte in dit concept',
  emptyText: (code) => (
    <>
      Waar een frontend met een toolchain wordt gebouwd – React, Vue of Svelte met Vite, TypeScript, Tailwind – wordt
      het project hier bewaard, bij elke versie: waaruit de assets worden gebouwd. Jouw agent bouwt het op de plek waar
      hij werkt en slaat de bronnen samen met het resultaat op – in een kloon is het de map {code('dev/')}. Een
      frontend van gewone HTML, CSS en JavaScript heeft dat niet nodig.
    </>
  ),
  emptyHow: (code) => (
    <>{code('AGENTS.md')} in een kloon vertelt een coding agent hoe je er een opzet.</>
  ),

  projects: 'Projecten',
  atTheTop: 'de ontwikkelruimte zelf',
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
  builtWith: 'Gebouwd met',
  build: 'Build',
  noBuild: 'Geen buildscript in de package.json.',
  into: 'Bouwt naar',
  intoAssets: (folder, files, size) => (
    <>
      {folder} van de assets – {files === 1 ? '1 bestand' : `${files} bestanden`}, {size} in deze versie
    </>
  ),
  intoNothing: (folder) => <>{folder} van de assets – die in deze versie niets bevat</>,
  packages: 'Pakketten',
  packagesCount: (runtime, tooling) =>
    `${runtime === 1 ? '1 om te draaien' : `${runtime} om te draaien`}, ${tooling === 1 ? '1 om te bouwen' : `${tooling} om te bouwen`}`,
  showPackages: 'Toon ze',
  hidePackages: 'Verberg ze',
  runtime: 'Om te draaien',
  tooling: 'Om te bouwen',
  missing: (page, files) => (
    <>
      {page} verwijst naar {files.length === 1 ? 'een bestand' : `${files.length} bestanden`} dat niet bij de assets {files.length === 1 ? 'zit' : 'zitten'}{' '}
      ({files.slice(0, 3).join(', ')}{files.length > 3 ? ', …' : ''}): wat de build schreef is niet volledig
      opgeslagen, en de pagina laadt niet.
    </>
  ),
  noLock: 'Geen lockbestand: de volgende build kan andere versies van de pakketten installeren dan de vorige.',
  noIgnore: 'Geen .gitignore: wat de toolchain installeert en bouwt kan in een versie terechtkomen.',

  inVersion: (version) => `In versie ${version}`,
  inDraft: 'In dit concept',
  comparedWith: (version) => `ten opzichte van versie ${version}`,
  first: 'De eerste versie die er een heeft.',
  both: (here, assets) =>
    `${here === 1 ? '1 bestand' : `${here} bestanden`} hier gewijzigd, en ${assets === 1 ? '1 bestand' : `${assets} bestanden`} van de assets.`,
  hereOnly: (here) =>
    `${here === 1 ? '1 bestand' : `${here} bestanden`} hier gewijzigd, en geen van de assets: tenzij de wijziging geen build nodig had, krijgen bezoekers wat ze eerder kregen.`,
  builtOnly: (folder) => (
    <>Wat in {folder} is gebouwd is gewijzigd, en hier niets: een wijziging in wat de build schreef wordt door de volgende build ongedaan gemaakt.</>
  ),
  assetsOnly: 'Hier is niets gewijzigd.',
  unchanged: 'Hier en in de assets is niets gewijzigd.',
  showChanges: 'Toon de wijzigingen',
  hideChanges: 'Verberg de wijzigingen',
  noChanges: 'Hier is niets gewijzigd.',

  readme: 'Hoe het is gebouwd',
  noReadme: (code) => (
    <>
      Nergens staat hoe het is gebouwd. Een {code('README.md')} bovenaan de ontwikkelruimte – met de commando’s en
      waar de build naartoe gaat – is wat de volgende agent gebruikt om te bouwen.
    </>
  ),
  readOnly: 'Alleen lezen: het wordt gewijzigd waar het wordt gebouwd.',
  noFiles: 'Geen bestanden.',
};
