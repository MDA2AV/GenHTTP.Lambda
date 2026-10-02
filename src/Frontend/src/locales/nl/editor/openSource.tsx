import type { EditorMessages } from '../../en/editor';

export const openSource: EditorMessages['openSource'] = {
  loading: 'Laden…',
  loadFailed: 'Er kon niet worden nagegaan of de code gepubliceerd is.',
  hint: (tool) => (
    <>
      Iedereen kan een gepubliceerde lambda lezen, een ster geven en downloaden op zijn eigen pagina onder Open
      source – elke versie ervan, onder de licentie die je kiest, en nooit de data die hij bewaart. Alleen wie de
      editorsleutel heeft, kan hem publiceren of de publicatie weer intrekken. Een agent kan hetzelfde met de tool{' '}
      {tool}.
    </>
  ),
  hintSimple:
    'Iedereen kan op een eigen pagina lezen hoe je app gemaakt is, en erop verderbouwen onder de licentie die je kiest – nooit met wat hij bewaart. Alleen jij kunt hem publiceren of de publicatie weer intrekken.',
  open: 'Broncodepagina openen',
  switch: 'De code van deze app publiceren',
  publishedNow: (license) => `Gepubliceerd onder ${license}. Iedereen kan hem lezen en downloaden.`,
  off: 'Uit. Niemand ziet de code tot je hem publiceert.',
  keptStars: (stars) =>
    stars === 1
      ? 'De ster blijft bewaard voor als je hem opnieuw publiceert.'
      : `De ${stars} sterren blijven bewaard voor als je hem opnieuw publiceert.`,
  published: 'Gepubliceerd. Iedereen kan de code nu lezen.',
  saved: 'Opgeslagen.',
  saveFailed: 'De code kon niet worden gepubliceerd.',
  withdrawn: 'Ingetrokken. De pagina is weg.',
  withdrawFailed: 'De publicatie kon niet worden ingetrokken.',
  whatTitle: 'Wat er gepubliceerd wordt',
  what: [
    'De code – zoals hij nu is, en elke eerdere stand ervan',
    "Alles wat de app toont: zijn pagina's, stijlen en afbeeldingen",
    'Wat erover geschreven is: waar hij voor is, en hoe hij getest wordt',
    'Elke wijziging die hij onderging, elk in één regel',
  ],
  neverTitle: 'Wat nooit gepubliceerd wordt',
  never: [
    'Wat de app bewaart: zijn items, wat hij opsloeg, zijn sleutels en wachtwoorden',
    'Wat je vroeg, in je eigen woorden',
    'Wie hem gebruikt: zijn bezoekers en wat ze deden',
    'De editorlink',
  ],
  careful:
    'Alles in de code wordt openbaar, ook de eerdere standen ervan. Een wachtwoord of sleutel hoort nooit in de code – die hoort bij de sleutels en wachtwoorden onder Data, en die worden nooit gepubliceerd.',
  licenseLabel: 'Licentie',
  licenseHint:
    'Wat anderen met de code mogen doen. MIT, de meest gebruikte, laat iedereen er bijna alles mee doen, zolang je naam erbij blijft staan.',
  readLicense: 'Licentie lezen',
  authorLabel: 'Naam in de licentie',
  optional: 'optioneel',
  authorPlaceholder: (key) => `De auteurs van ${key}`,
  authorHint:
    'Je naam of die van je organisatie, getoond op de broncodepagina en in de licentie. Laat je het leeg, dan noemt de licentie de auteurs van deze app.',
  publish: 'Publiceren',
  save: 'Wijzigingen opslaan',
  allSaved: 'Alles is opgeslagen.',
  takeDown: 'Publicatie intrekken',
  confirm: 'De publicatie van de code intrekken?',
  confirmText:
    'De pagina en de downloads verdwijnen meteen. Wie de code al gedownload heeft, houdt hem onder de licentie waarmee hij kwam. De sterren blijven bewaard voor als je hem opnieuw publiceert.',
  keep: 'Gepubliceerd laten',
  stars: (count) => (count === 1 ? '1 ster' : `${count} sterren`),
  sidebar: 'Broncode',
};
