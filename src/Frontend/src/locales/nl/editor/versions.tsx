import type { EditorMessages } from '../../en/editor';

export const versions: EditorMessages['versions'] = {
  hint: (limit) =>
    `Een versie is het programma (de code en de resources) en verandert nooit meer als hij eenmaal is opgeslagen. Zo kun je elke versie vergelijken en precies zoals hij was weer online zetten. Elke versie bewaart wat er gevraagd werd en wat er veranderde. Wil je de lambda aanpassen, start dan een concept: dat wordt de volgende versie zodra het goed is. Bij meer dan ${limit} versies verdwijnen de oudste. De versie die online staat, verdwijnt nooit.`,
  none: 'Nog geen versies.',
  noDescription: 'Geen beschrijving',
  online: 'online',
  putOnline: 'Deze versie online zetten',
  rollBackTitle: 'Deze oudere versie weer online zetten',
  deploy: 'Deployen',
  rollBack: 'Terugzetten',
  readFailed: 'Deze versie kon niet worden gelezen.',
  comparing: 'Vergelijken…',
  unchanged: 'Niets veranderd ten opzichte van de vorige versie.',
  first: 'De eerste versie.',
  status: { added: 'toegevoegd', removed: 'verwijderd', changed: 'gewijzigd', same: 'gelijk' },
  groups: {
    code: 'Code',
    resources: 'Resources',
  },
  files: 'Bestanden openen',
  docs: 'Documentatie lezen',
  feature: 'Vanaf hier een concept starten',
  featureTitle:
    'Naast de lambda aan een wijziging van deze versie werken, en die samenvoegen tot de volgende versie zodra het goed is',
  binary: 'Geen tekst, dus er zijn geen regels om te vergelijken.',
  tooLarge: 'Te groot om regel voor regel te vergelijken.',
};
