import type { EditorMessages } from '../../en/editor';

export const build: EditorMessages['build'] = {
  title: 'Build',
  hint: 'Waaruit de assets of de code van een versie worden gebouwd: bestanden waarop wie de app wijzigt – jouw agent, in een kloon – een buildtool draait, bij elke versie bewaard en nooit gecompileerd of geserveerd. Dit platform bouwt niets, dus ze worden hier gelezen, niet bewerkt.',
  overview: 'Overzicht',
  files: 'Bestanden',
  scope: (version) =>
    `Waaruit versie ${version} is gebouwd – bij die versie bewaard, nooit gecompileerd of geserveerd, en gebouwd door wie haar wijzigt, nooit hier.`,
  scopeDraft: 'Waaruit dit concept is gebouwd – erbij bewaard, nooit gecompileerd of geserveerd, en gebouwd door wie het wijzigt, nooit hier.',
  reading: 'Lezen waaruit het is gebouwd…',
  readFailed: 'Waaruit het is gebouwd kon niet worden gelezen.',

  emptyTitle: (version) => `Versie ${version} bewaart niets waaruit ze is gebouwd`,
  emptyTitleDraft: 'Dit concept bewaart niets waaruit het is gebouwd',
  emptyText: (code) => (
    <>
      Waar de assets of de code van een versie door een buildtool worden gemaakt – gecompileerd, gebundeld of
      gegenereerd – worden de bestanden waaruit ze zijn gemaakt hier bewaard, bij elke versie: de map {code('build/')} in
      een kloon. Wie de app wijzigt, draait de build op de plek waar hij werkt en slaat beide samen op; dit platform
      bouwt niets. Wat wordt geschreven zoals het wordt geserveerd of gecompileerd, heeft dat niet nodig.
    </>
  ),
  emptyHow: (code) => (
    <>{code('AGENTS.md')} in een kloon vertelt een codeeragent hoe het wordt gebruikt.</>
  ),

  inVersion: (version) => `In versie ${version}`,
  inDraft: 'In dit concept',
  comparedWith: (version) => `ten opzichte van versie ${version}`,
  first: 'De eerste versie die het bewaart.',
  both: (here, program) =>
    `${here === 1 ? '1 bestand' : `${here} bestanden`} hier gewijzigd, en ${program === 1 ? '1 bestand' : `${program} bestanden`} van de code en de assets.`,
  hereOnly: (here) =>
    `${here === 1 ? '1 bestand' : `${here} bestanden`} hier gewijzigd, en niets van de code of de assets: als wat is gewijzigd daarin wordt ingebouwd, is het niet gebouwd.`,
  programOnly: 'Hier is niets gewijzigd.',
  unchanged: 'Hier is niets gewijzigd, en ook niet in de code of de assets.',
  showChanges: 'Wijzigingen tonen',
  hideChanges: 'Wijzigingen verbergen',
  noChanges: 'Hier is niets gewijzigd.',

  readme: 'Hoe het wordt gebouwd',
  noReadme: (code) => (
    <>
      Niets zegt hoe het wordt gebouwd. Een {code('README.md')} bovenaan – met de commando’s en de plek waar de build
      terechtkomt – is waarmee de volgende agent bouwt.
    </>
  ),
  readOnly: 'Alleen lezen: het wordt gewijzigd op de plek waar het wordt gebouwd.',
  noFiles: 'Geen bestanden.',
};
