import type { EditorMessages } from '../../en/editor';

export const clone: EditorMessages['clone'] = {
  button: 'Klonen',
  title: 'Klonen met git',
  intro:
    'Werk eraan met je eigen tools en coding agent: de repository is het project zoals het draait, elke versie een commit van main en elk concept een branch.',
  keyWarning: 'Het adres bevat de editorsleutel: wie het heeft, kan de app wijzigen. Deel het dus nergens.',
  draft: (branch) => <>Dit concept is de branch {branch}.</>,
  pushing: 'Pushen',
  toMain: (deploy) => <>Een commit die je naar main pusht is de volgende versie, nog niet online - {deploy} zet hem met de push online.</>,
  toBranch: 'Een gepushte branch is een concept, met de voorvertoning online op een eigen adres.',
  agents: (file) => <>{file} in de repository vertelt een coding agent de rest.</>,
  readOnly: 'Een demo is alleen-lezen: kloon hem om hem te lezen en begin er een eigen lambda mee om iets te wijzigen.',
  copy: 'Kopiëren',
  copied: 'Gekopieerd',
};
