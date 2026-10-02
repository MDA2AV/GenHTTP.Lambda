import type { EditorMessages } from '../../en/editor';

export const openSource: EditorMessages['openSource'] = {
  loading: 'Caricamento…',
  loadFailed: 'Impossibile sapere se il codice è pubblicato.',
  hint: (tool) => (
    <>
      Una lambda pubblicata ha una pagina tutta sua tra le app open source, dove chiunque può leggerla, darle una
      stella e scaricarla: ogni sua versione, con la licenza che scegli, e mai i dati che conserva. Solo chi ha la
      chiave di modifica può pubblicarla o ritirarla. Un agente può fare lo stesso con lo strumento {tool}.
    </>
  ),
  hintSimple:
    'Chiunque può leggere su una pagina dedicata com’è fatta la tua app e riutilizzarne il codice con la licenza che scegli, mai però quello che conserva. Solo tu puoi pubblicare il codice o ritirarlo.',
  open: 'Apri la pagina del codice',
  switch: 'Pubblica il codice di questa app',
  publishedNow: (license) => `Pubblicato con licenza ${license}. Chiunque può leggerlo e scaricarlo.`,
  off: 'Disattivato. Nessuno vede il codice finché non lo pubblichi.',
  keptStars: (stars) =>
    stars === 1
      ? 'La sua stella resta, per quando lo pubblicherai di nuovo.'
      : `Le sue ${stars} stelle restano, per quando lo pubblicherai di nuovo.`,
  published: 'Pubblicato. Ora chiunque può leggere il codice.',
  saved: 'Salvato.',
  saveFailed: 'Impossibile pubblicare il codice.',
  withdrawn: 'Ritirato. La sua pagina non c’è più.',
  withdrawFailed: 'Impossibile ritirare il codice.',
  whatTitle: 'Cosa viene pubblicato',
  what: [
    'Il suo codice, com’è ora e in ogni suo stato precedente',
    'Tutto quello che mostra: le sue pagine, gli stili e le immagini',
    'Quello che è scritto sull’app: a cosa serve e come viene testata',
    'Ogni modifica che ha avuto, descritta in una riga',
  ],
  neverTitle: 'Cosa non viene mai pubblicato',
  never: [
    'Quello che conserva: le sue voci, quello che ha salvato, le sue chiavi e password',
    'Quello che hai chiesto, con le tue parole',
    'Chi la usa: i suoi visitatori e cosa hanno fatto',
    'Il link di modifica',
  ],
  careful:
    'Tutto quello che c’è nel codice diventa pubblico, anche nei suoi stati precedenti. Una password o una chiave non va mai nel codice: il suo posto è tra le chiavi e password in Dati, che non vengono mai pubblicate.',
  licenseLabel: 'Licenza',
  licenseHint:
    'Cosa possono fare gli altri con il codice. MIT, la più diffusa, permette a chiunque di farci quasi tutto, purché il tuo nome resti indicato.',
  readLicense: 'Leggi la licenza',
  authorLabel: 'Nome nella licenza',
  optional: 'facoltativo',
  authorPlaceholder: (key) => `Gli autori di ${key}`,
  authorHint:
    'Il tuo nome o quello della tua organizzazione, mostrato sulla pagina del codice e nella licenza. Se lo lasci vuoto, la licenza nomina gli autori di questa app.',
  publish: 'Pubblica',
  save: 'Salva le modifiche',
  allSaved: 'Tutto salvato.',
  takeDown: 'Ritira il codice',
  confirm: 'Ritirare il codice?',
  confirmText:
    'La sua pagina e i suoi download spariscono subito. Chi l’ha già scaricato lo tiene, con la licenza con cui l’ha ricevuto. Le sue stelle restano, per quando lo pubblicherai di nuovo.',
  keep: 'Lascialo pubblicato',
  stars: (count) => (count === 1 ? '1 stella' : `${count} stelle`),
  sidebar: 'Codice sorgente',
};
