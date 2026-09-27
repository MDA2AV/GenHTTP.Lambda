import type { Messages } from '../en';

export const terms: Messages['terms'] = {
  title: 'Nutzungsbedingungen',
  binding: (english) => (
    <>Diese Übersetzung dient ausschließlich der Information. Rechtlich maßgeblich ist die {english('englische Fassung')}.</>
  ),
  intro:
    'Dieser kostenlose Dienst dient zum Ausprobieren. Er führt Code unterschiedlicher Personen auf gemeinsam genutzter Infrastruktur aus. Das ist nur möglich, wenn sich alle Nutzer an einige Regeln halten.',
  sections: {
    forbiddenTitle: 'Unzulässige Inhalte',
    forbidden: [
      'Keine Schadsoftware, kein Phishing, keine Krypto-Miner. Keine Inhalte, die andere Systeme – hier oder anderswo – angreifen, scannen, überlasten oder anderweitig beeinträchtigen. Keine Inhalte, die andere Personen belästigen. Keine Inhalte, zu deren Veröffentlichung Sie nicht berechtigt sind; dazu zählen fremder Code, fremde Texte, Bilder und Marken.',
      'Verwenden Sie ein Lambda nicht, um personenbezogene Daten Dritter zu speichern oder weiterzuleiten. Eine öffentliche Adresse ist nicht privat, und diese Plattform bietet keine Möglichkeit, solche Daten angemessen zu schützen.',
    ],
    actionTitle: 'Unsere Maßnahmen',
    action:
      'Alle hier bereitgestellten Inhalte können jederzeit ohne Vorankündigung und ohne Angabe von Gründen offline genommen oder entfernt werden. Dies geschieht insbesondere bei Verstößen gegen die obigen Regeln, bei einer Gefährdung des gemeinsam genutzten Servers oder nach einer begründeten Meldung.',
    lastingTitle: 'Aufbewahrungsdauer',
    lasting: (hours, days) =>
      `Eine Bereitstellung bleibt etwa ${hours} Stunden erreichbar. Ein nicht geöffnetes Lambda wird einschließlich aller Versionen seines Codes etwa ${days} Tage nach der letzten Bearbeitung entfernt. Speichern und Bereitstellen gelten als Bearbeitung; aktiv genutzte Lambdas bleiben also erhalten. Dieser Dienst ersetzt keine Datensicherung: Bitte bewahren Sie eine eigene Kopie wichtigen Codes auf.`,
    keyTitle: 'Der Editor-Link als Zugangsdaten',
    key: 'Wer über den Editor-Link verfügt, kann das zugehörige Lambda lesen und ändern; ein Konto oder Passwort ist nicht damit verbunden. Die Veröffentlichung des Links gewährt anderen diese Möglichkeit. Ein verlorener Link kann nicht wiederhergestellt werden.',
    warrantyTitle: 'Keine Gewährleistung',
    warranty:
      'Der Dienst wird ohne Gewähr bereitgestellt – ohne Zusicherung, dass er funktioniert, dauerhaft verfügbar bleibt oder gespeicherte Inhalte erhält. Er kann jederzeit neu gestartet, geändert oder eingestellt werden. Nutzen Sie ihn nicht für Anwendungen, die für Sie oder Dritte von Bedeutung sind.',
    reportTitle: 'Meldungen',
    report: (mailbox, front) => (
      <>
        Wenn ein hier gehostetes Lambda unzulässige Inhalte verbreitet, schreiben Sie bitte unter Angabe seiner Adresse an{' '}
        {mailbox}. Welche Angaben hilfreich sind, finden Sie auf der {front('Startseite')}.
      </>
    ),
  },
  change: 'Diese Bedingungen können sich ändern. Maßgeblich ist die auf dieser Seite veröffentlichte Fassung.',

  short:
    'Lambdas laufen auf gemeinsam genutzter Infrastruktur. Mit dem Erstellen erklären Sie sich einverstanden, keine Schadsoftware, Phishing-Seiten, Krypto-Miner oder Inhalte bereitzustellen, die andere Systeme angreifen, scannen oder überlasten, und keine Inhalte ohne entsprechende Rechte zu veröffentlichen. Wer den Editor-Link kennt, kann Ihr Lambda ändern – behandeln Sie ihn daher wie ein Passwort. Lambdas im kostenlosen Tarif bleiben online, solange sie genutzt werden: Ein Lambda ohne Aufrufe und Änderungen während eines Monats wird offline genommen und nach zwei weiteren Monaten ohne Aktivität entfernt. Bereitgestellte Inhalte können jederzeit entfernt werden.',
};
