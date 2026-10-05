import type { EditorMessages } from '../../en/editor';

export const clone: EditorMessages['clone'] = {
  button: 'Klonen',
  title: 'Mit git klonen',
  intro:
    'Arbeiten Sie mit Ihren eigenen Werkzeugen und Ihrem eigenen Coding-Agent: Das Repository ist das Projekt, als das die App läuft – jede Version ein Commit auf main, jeder Entwurf ein Branch.',
  keyWarning: 'Die Adresse enthält den Editor-Schlüssel: Wer sie hat, kann die App ändern. Geben Sie sie nicht weiter.',
  draft: (branch) => <>Dieser Entwurf ist der Branch {branch}.</>,
  pushing: 'Pushen',
  toMain: (deploy) => <>Ein Commit, den Sie auf main pushen, ist die nächste Version, noch nicht online – {deploy} stellt sie mit dem Push online.</>,
  toBranch: 'Ein gepushter Branch ist ein Entwurf, dessen Vorschau unter einer eigenen Adresse online ist.',
  agents: (file) => <>{file} im Repository sagt einem Coding-Agent den Rest.</>,
  readOnly: 'Eine Demo ist schreibgeschützt: Klonen Sie sie, um sie zu lesen, und starten Sie damit ein eigenes Lambda, um etwas zu ändern.',
  copy: 'Kopieren',
  copied: 'Kopiert',
};
