import type { EditorMessages } from '../../en/editor';

export const clone: EditorMessages['clone'] = {
  button: 'Clona',
  title: 'Clona con git',
  intro:
    'Lavora con i tuoi strumenti e il tuo agente di programmazione: il repository è il progetto con cui l’app gira, ogni versione è un commit di main e ogni bozza un branch.',
  keyWarning: 'L’indirizzo contiene la chiave di modifica: chi ce l’ha può cambiare l’app. Non condividerlo.',
  draft: (branch) => <>Questa bozza è il branch {branch}.</>,
  pushing: 'Fare push',
  toMain: (deploy) => <>Un commit inviato a main è la versione successiva, non ancora online: {deploy} la mette online con il push.</>,
  toBranch: 'Un branch inviato è una bozza, con la sua anteprima online a un indirizzo tutto suo.',
  agents: (file) => <>{file} nel repository spiega il resto a un agente di programmazione.</>,
  readOnly: 'Una demo è di sola lettura: clonala per leggerla e crea una lambda tua a partire da essa per modificarla.',
  copy: 'Copia',
  copied: 'Copiato',
};
