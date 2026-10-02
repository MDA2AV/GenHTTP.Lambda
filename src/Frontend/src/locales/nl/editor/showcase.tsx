import type { EditorMessages } from '../../en/editor';

export const showcase: EditorMessages['showcase'] = {
  loadFailed: 'De showcase kon niet worden geladen.',
  loading: 'Laden…',
  title: 'een titel',
  description: 'een beschrijving',
  picture: 'een afbeelding',
  updated: 'Je vermelding in de showcase is bijgewerkt.',
  listed: 'Hij staat nu op de showcasepagina.',
  waiting: 'Opgeslagen. Hij verschijnt op de showcasepagina zodra de lambda online is.',
  saveFailed: 'Je vermelding in de showcase kon niet worden opgeslagen.',
  removed: 'Van de showcasepagina gehaald.',
  removeFailed: 'Je vermelding kon niet uit de showcase worden gehaald.',
  wrongType: 'Dat is geen PNG-, JPEG-, GIF- of WebP-afbeelding.',
  tooLarge: (size, limit) => `Dat is ${size}; een afbeelding mag maximaal ${limit} zijn.`,
  unreadable: 'Dat bestand kon niet worden gelezen.',
  hint: (tool) => (
    <>
      De showcasepagina toont lambda's die hun eigenaars willen laten zien, recent gebruikte eerst. Alleen wie de
      editorsleutel heeft, kan een lambda erop zetten of eraf halen, en hij staat er alleen zolang hij online is. Een
      agent kan hetzelfde met de tool {tool}.
    </>
  ),
  open: 'Showcase openen',
  switch: 'Deze lambda op de showcasepagina tonen',
  listedNow: 'Staat erop. Iedereen die door de showcase bladert, kan hem openen.',
  notListed: 'Opgeslagen, maar niet zichtbaar: de lambda is offline. Hij verschijnt weer zodra hij opnieuw is gedeployd.',
  off: 'Uit. Er wordt nergens iets van deze lambda getoond tot je dit aanzet en opslaat.',
  offline: "De lambda is offline, dus de vermelding wacht tot hij gedeployd is. Alleen lambda's die reageren, staan erop.",
  titleLabel: 'Titel',
  titlePlaceholder: 'Scorebord voor de pubquiz',
  descriptionLabel: 'Beschrijving',
  descriptionPlaceholder:
    'Teams vullen hun antwoorden in op hun telefoon, de quizmaster kijkt ze na en het scorebord werkt voor iedereen in de zaal bij.',
  save: 'Wijzigingen opslaan',
  add: 'Toevoegen aan de showcase',
  takeOff: 'Eraf halen',
  needs: (missing) =>
    `Nog nodig: ${missing.length > 1 ? `${missing.slice(0, -1).join(', ')} en ${missing[missing.length - 1]}` : missing[0]}.`,
  tooLong: 'Sommige velden zijn te lang.',
  allSaved: 'Alles is opgeslagen.',
  preview: 'Voorbeeld',
  card: (address) => <>Dit is de kaart die bezoekers zien. Hij opent {address}.</>,
  confirm: 'Uit de showcase halen?',
  keep: 'Laten staan',
  confirmText: 'De titel, beschrijving en afbeelding worden verwijderd. De lambda zelf blijft precies zoals hij is.',
  pictureLabel: 'Afbeelding',
  formats: (limit) => `PNG, JPEG, GIF of WebP, tot ${limit}`,
  notSaved: 'nog niet opgeslagen',
  replace: 'Sleep hier een nieuwe heen om hem te vervangen.',
  drop: 'Sleep hier een afbeelding heen.',
  advice: 'Een screenshot, of een korte GIF van de app in gebruik, werkt het best in 16:10.',
  another: 'Andere kiezen',
  choose: 'Bestand kiezen',
  keepSaved: 'Opgeslagen afbeelding houden',
  clear: 'Wissen',
};
