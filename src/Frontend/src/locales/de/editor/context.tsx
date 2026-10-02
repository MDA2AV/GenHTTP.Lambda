import type { EditorMessages } from '../../en/editor';

export const context: EditorMessages['context'] = {
  docs: {
    title: 'Dokumentation',
    titleSimple: 'Über Ihre App',
    hint: 'Was diese App ist, für wen sie gedacht ist und warum – und warum sie so gebaut ist, wie sie ist. Agenten schreiben sie mit jeder Änderung, und sie wird mit jeder Version gespeichert. Eine ältere Version kommt also mit der Dokumentation zurück, die für sie galt.',
    hintSimple: 'Wofür Ihre App da ist und warum – so, wie der Agent es aus Ihren Wünschen verstanden hat. Er hält das mit jeder Änderung aktuell.',
    inDraft: 'Die Dokumentation dieses Entwurfs. Sie wird zur Dokumentation Ihrer App, wenn der Entwurf online geht.',
    pages: { product: 'Produkt', decisions: 'Entscheidungen' },
    emptyTitle: 'Noch nichts geschrieben',
    emptyText: (code) => (
      <>
        Agenten schreiben die Dokumentation mit ihren Änderungen: was die App ist, für wen sie gedacht ist und warum,
        in {code('.lambda/docs/product.md')}, und warum sie so gebaut ist, wie sie ist, in {code('decisions.md')}. Sie
        gehört zur Version, neben dem Code.
      </>
    ),
    emptySimpleTitle: 'Über Ihre App ist noch nichts geschrieben',
    emptySimple: 'Der Agent kann beschreiben, wofür Ihre App da ist und warum – ausgehend von dem, worum Sie gebeten haben. Von da an hält er die Beschreibung aktuell.',
    ask: 'Den Agenten bitten, sie zu schreiben',
    describe: 'Den Agenten um eine Beschreibung bitten',
    writePrompt: 'Schreibe die Dokumentation dieser App: was sie ist, für wen sie gedacht ist und warum, und die technischen Entscheidungen dahinter.',
    describePrompt: 'Beschreibe, wofür diese App da ist und warum, damit ich es unter „Über die App“ lesen kann.',
    decisionsPrompt: 'Halte die technischen Entscheidungen hinter dieser App fest und warum sie getroffen wurden.',
    missingProduct: 'Noch keine Produktseite',
    missingProductText: 'Was die App ist, für wen sie gedacht ist, was Leute damit tun und warum – in den Worten derer, die darum gebeten haben.',
    missingDecisions: 'Noch keine Entscheidungen festgehalten',
    missingDecisionsText: 'Wie die App gebaut ist und warum: wie sie ihre Daten aufbewahrt, wovon sie abhängt, was weggelassen wurde. Was wissen muss, wer sie als Nächstes ändert.',
    correctText: 'Der Agent schreibt das aus dem, worum Sie gebeten haben, und hält es mit jeder Änderung aktuell. Ist etwas falsch oder fehlt etwas? Sagen Sie es ihm.',
    correct: 'Dem Agenten sagen',
    correctPrompt: 'Korrigiere die Beschreibung der App: ',
    placeholder: 'Erklärt, warum Einträge ein Jahr lang aufbewahrt werden',
  },
  tests: {
    title: 'Tests',
    hint: 'Wie diese App automatisch getestet wird, und die Scripts und Daten, die die Tests verwenden. Agenten halten das aktuell und führen die Tests aus, bevor sie eine Änderung für fertig erklären. Es wird mit jeder Version gespeichert.',
    inDraft: 'Die Tests dieses Entwurfs. Sie werden zu den Tests Ihrer App, wenn der Entwurf online geht – führen Sie sie vorher gegen seine Vorschau aus.',
    pages: { testing: 'Wie getestet wird' },
    emptyTitle: 'Noch keine Tests',
    emptyText: (code) => (
      <>
        Wie die App getestet wird – was weiter funktionieren muss, wie man es prüft und wie die Scripts dafür
        ausgeführt werden –, schreiben Agenten in {code('.lambda/tests/README.md')}, mit den Scripts und den Testdaten
        daneben.
      </>
    ),
    ask: 'Den Agenten bitten, Tests zu schreiben',
    writePrompt: 'Schreibe die Tests dieser App: was weiter funktionieren muss und wie es sich automatisch prüfen lässt, mit einem Script, das gegen ihre Vorschau läuft.',
    missing: 'Noch nicht beschrieben, wie getestet wird',
    missingText: 'Was weiter funktionieren muss, wie jeder Punkt davon geprüft wird und wie die Scripts daneben ausgeführt werden.',
    placeholder: 'Prüft, dass eine volle Liste keine neuen Einträge annimmt',
  },
  files: 'Dateien',
  noFiles: 'Keine Dateien neben den Seiten.',
  none: 'fehlt',
  missingPill: 'Noch nicht geschrieben',
  changedIn: (version) => `In Version ${version} geändert`,
  changedInDraft: 'In diesem Entwurf geändert',
  showChanges: 'Änderungen zeigen',
  hideChanges: 'Änderungen ausblenden',
  noChanges: 'Nichts geändert.',
  edit: 'Bearbeiten',
  olderVersion: 'Eine Version ändert sich nie: Bearbeitet wird eine Seite in der neuesten Version oder in einem Entwurf.',
  writeIt: 'Selbst schreiben',
  askPage: 'Den Agenten bitten, sie zu schreiben',
  editInCode: 'Im Code öffnen',
  cancel: 'Abbrechen',
  save: 'Speichern',
  write: 'Schreiben',
  preview: 'Vorschau',
  writeOrPreview: 'Schreiben oder Vorschau',
  discard: 'Ihre Änderungen an dieser Seite gehen verloren. Verwerfen?',
  reading: 'Wird gelesen …',
  readFailed: 'Das konnte nicht gelesen werden.',
  saveFailed: 'Das konnte nicht gespeichert werden.',
  savedDraft: 'Im Entwurf gespeichert.',
  savedVersion: (version) => `Als Version ${version} gespeichert.`,
  savedOnline: (version) => `Als Version ${version} gespeichert und online.`,
  savedNotOnline: (version) => `Als Version ${version} gespeichert, aber nicht online gegangen.`,
  saveTitle: 'Als neue Version speichern',
  saveText: (newest) =>
    `Eine Version ändert sich nie, deshalb wird diese Seite als nächste gespeichert – auf Grundlage von Version ${newest}, alles andere bleibt, wie es ist.`,
  clash: (version) => `Version ${version} wurde gespeichert, seit Sie begonnen haben, und hat diese Seite ebenfalls geändert. Speichern ersetzt das.`,
  alsoOnline: 'Auch online stellen',
  alsoOnlineNote: 'Nur die Dokumentation ändert sich, Besucher sehen also nichts Neues – aber was online ist, bleibt die neueste Version.',
  skeleton: {
    product: '# Name der App\n\nWas sie ist, in ein, zwei Sätzen.\n\n## Für wen sie gedacht ist\n\n## Was Leute damit tun\n\n## Funktionen und warum es sie gibt\n\n## Was sie nicht tut\n',
    decisions: '# Entscheidungen\n\n## Eine Entscheidung\n\nWas entschieden wurde, warum, und was eine Änderung beachten muss.\n',
    testing: '# Wie getestet wird\n\nWie die Tests ausgeführt werden und gegen welche Adresse.\n\n## Was weiter funktionieren muss\n\n| Verhalten | Request | Erwartet |\n|---|---|---|\n| | | |\n',
  },
};
