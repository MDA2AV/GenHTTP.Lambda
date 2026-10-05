import type { EditorMessages } from '../../en/editor';

export const build: EditorMessages['build'] = {
  title: 'Build',
  hint: 'Woraus die Assets oder der Code einer Version gebaut werden: Dateien, auf denen wer die App ändert – Ihr Agent, in einem Klon – ein Build-Werkzeug ausführt. Sie werden mit jeder Version gespeichert und nie kompiliert oder ausgeliefert. Diese Plattform baut nichts, deshalb werden sie hier gelesen, nicht bearbeitet.',
  overview: 'Übersicht',
  files: 'Dateien',
  scope: (version) =>
    `Woraus Version ${version} gebaut wird – mit ihr gespeichert, nie kompiliert oder ausgeliefert und gebaut von dem, der sie ändert, nie hier.`,
  scopeDraft: 'Woraus dieser Entwurf gebaut wird – mit ihm gespeichert, nie kompiliert oder ausgeliefert und gebaut von dem, der ihn ändert, nie hier.',
  reading: 'Lese, woraus es gebaut wird …',
  readFailed: 'Es konnte nicht gelesen werden, woraus es gebaut wird.',

  emptyTitle: (version) => `Version ${version} speichert nichts, woraus sie gebaut wird`,
  emptyTitleDraft: 'Dieser Entwurf speichert nichts, woraus er gebaut wird',
  emptyText: (code) => (
    <>
      Wo die Assets oder der Code einer Version von einem Build-Werkzeug erzeugt werden – kompiliert, gebündelt oder
      generiert –, werden die Dateien, aus denen sie entstehen, hier mit jeder Version gespeichert: der Ordner {code('build/')}
      in einem Klon. Wer die App ändert, führt den Build dort aus, wo er arbeitet, und speichert beides zusammen; diese
      Plattform baut nichts. Was so geschrieben ist, wie es ausgeliefert oder kompiliert wird, braucht nichts davon.
    </>
  ),
  emptyHow: (code) => (
    <>{code('AGENTS.md')} in einem Klon sagt einem Coding-Agent, wie er verwendet wird.</>
  ),

  inVersion: (version) => `In Version ${version}`,
  inDraft: 'In diesem Entwurf',
  comparedWith: (version) => `gegenüber Version ${version}`,
  first: 'Die erste Version, die es speichert.',
  both: (here, program) =>
    `${here === 1 ? '1 Datei' : `${here} Dateien`} hier geändert und ${program === 1 ? '1 Datei' : `${program} Dateien`} des Codes und der Assets.`,
  hereOnly: (here) =>
    `${here === 1 ? '1 Datei' : `${here} Dateien`} hier geändert, aber nichts am Code oder an den Assets: Wenn das Geänderte darin eingebaut wird, wurde nicht gebaut.`,
  programOnly: 'Hier hat sich nichts geändert.',
  unchanged: 'Weder hier noch am Code oder an den Assets hat sich etwas geändert.',
  showChanges: 'Änderungen anzeigen',
  hideChanges: 'Änderungen ausblenden',
  noChanges: 'Hier hat sich nichts geändert.',

  readme: 'Wie gebaut wird',
  noReadme: (code) => (
    <>
      Nichts sagt, wie gebaut wird. Eine {code('README.md')} ganz oben – die Befehle und wohin der Build geht – ist das,
      woraus der nächste Agent baut.
    </>
  ),
  readOnly: 'Nur lesbar: Es wird dort geändert, wo es gebaut wird.',
  noFiles: 'Keine Dateien.',
};
