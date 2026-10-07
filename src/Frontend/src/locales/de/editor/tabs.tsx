import type { EditorMessages } from '../../en/editor';

export const tabs: EditorMessages['tabs'] = {
  codeName: 'Der Name einer C#-Datei besteht aus Buchstaben, Ziffern, Binde- und Unterstrichen und Punkten, beginnt mit einem Buchstaben und endet auf .cs, höchstens 40 Zeichen.',
  name: 'Buchstaben, Ziffern und - _ . + @ ( ) [ ] { } $ ~, Ordner durch Schrägstriche getrennt, keine Leerzeichen und kein Name, der auf einen Punkt endet.',
  taken: 'Ein exportiertes oder geklontes Lambda hat ganz oben bereits eine Datei oder einen Ordner mit diesem Namen. Legen Sie sie in einen Ordner oder wählen Sie einen anderen Namen.',
  lambda: 'Ein Lambda hat keinen Ordner .lambda/ mehr: Seine Dokumentation liegt in docs/, seine Tests in tests/.',
  assets: 'Was ein Lambda ausliefert, liegt jetzt in seinen Ressourcen – fügen Sie es dort hinzu.',
  resourceName: 'Buchstaben, Ziffern, Bindestriche, Unterstriche und Punkte, durch Schrägstriche getrennt, höchstens sechs Ordner tief – und eine Endung, damit die Datei als das Richtige ausgeliefert wird.',
  exists: 'Es gibt schon eine Datei mit diesem Namen.',
  remove: (name) => `${name} entfernen? Der Inhalt wird mitgelöscht.`,
  removeFolder: (name, files) => `${name} und ${files === 1 ? '1 Datei' : `${files} Dateien`} darin entfernen?`,
  there: (name) => `${name} gibt es schon.`,
  entry: 'Das Snippet: Was es zurückgibt, wird ausgeliefert',
  errors: 'hat Fehler',
  removeFile: (name) => `${name} entfernen`,
  removeTitle: 'Entfernen',
  codePlaceholder: 'Store.cs, models/Item.cs oder docs/notes.md',
  resourcePlaceholder: 'web/index.html',
  upload: 'Datei hochladen',
};
