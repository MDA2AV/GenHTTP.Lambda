import type { EditorMessages } from '../../en/editor';
import { many } from './language';

export const files: EditorMessages['files'] = {
  hint: (b) => (
    <>
      De bestanden van één versie: het programma. {b('Code')} wordt gecompileerd en nooit geserveerd.{' '}
      {b('Assets')}, zoals pagina's, scripts, stylesheets en afbeeldingen, worden met de code opgeslagen, samen ermee
      gedeployd en teruggezet, en zijn openbaar als de code ze serveert. Wat de lambda bewaart terwijl hij draait,
      staat hier niet: dat is zijn {b('Data')}.
    </>
  ),
  scope: (version, data) => (
    <>
      Deze bestanden horen bij versie {version} en veranderen mee. Wat de lambda bewaart terwijl hij draait, is voor
      elke versie hetzelfde en staat onder {data('Data')}.
    </>
  ),
  edit: 'Deze versie bewerken',
  version: 'Versie',
  shown: (version, online, newest) => `Versie ${version}${online ? ', online' : newest ? ', nieuwste' : ''}`,
  optionOnline: ' (online)',
  readFailed: 'Die versie kon niet worden gelezen.',
  noVersion: 'Er is nog geen versie om te tonen.',
  label: 'Bestanden',
  code: 'Code',
  codeWhy: 'Gecompileerd in de lambda, nooit geserveerd.',
  count: (files) => many(files, 'bestand', 'bestanden'),
  codeUsage: (files, used, of) => `${files}, ${used} van ${of} tekens`,
  usage: (files, used, of) => `${files}, ${used} van ${of}`,
  noCode: 'Geen code in deze versie.',
  assets: 'Assets',
  assetsPublic: 'Openbaar: deze versie serveert ze met Assets.',
  assetsPrivate: 'Opgeslagen met de code, maar deze versie serveert ze niet.',
  noAssets: 'Geen assets in deze versie.',
  context: 'Documentatie en tests',
  contextWhy: 'Nooit gecompileerd en nooit geserveerd: wat er over deze versie geschreven is, voor wie hem leest of aanpast.',
  contextUsage: (files, size) => `${files}, ${size} – meegeteld bij de assets`,
  noContext: 'Er is nog niets over deze versie geschreven.',
  data: 'Data',
  dataPublic: 'Openbaar: de code die online staat, serveert de data met Workspace.',
  dataPrivate: 'Alleen voor de lambda zelf. Hoort bij geen enkele versie.',
  uploadFailed: (path) => `${path} kon niet worden geüpload.`,
  deleteFolder: (path, held) =>
    held > 0
      ? `${path} verwijderen, met ${held === 1 ? 'het bestand' : `de ${held} bestanden`} erin?`
      : `De map ${path} verwijderen?`,
  deleteFile: (path) => `${path} verwijderen? De lambda kan het dan niet meer vinden.`,
  deleteFailed: 'Verwijderen is niet gelukt.',
  full: 'De data-opslag is vol',
  uploadInto: (folder) => `Uploaden naar ${folder}`,
  upload: 'Uploaden',
  reading: 'Lezen…',
  noData: 'Nog niets. Wat de lambda opslaat terwijl hij draait, verschijnt hier.',
  delete: (path) => `${path} verwijderen`,
  deleteShort: 'Verwijderen',
  fileFailed: 'Het bestand kon niet worden gelezen.',
  pick: 'Kies een bestand om te zien wat erin staat.',
  tooLarge: (name, size) => (
    <>
      {name} is {size}, te groot om hier te tonen.
    </>
  ),
  download: 'Downloaden',
  readingFile: (name) => `${name} lezen…`,
  missing: (name) => `Deze versie heeft geen bestand met de naam ${name}.`,
  saved: 'opgeslagen',
  notText: 'Geen tekst. Download het om erin te kijken.',
};
