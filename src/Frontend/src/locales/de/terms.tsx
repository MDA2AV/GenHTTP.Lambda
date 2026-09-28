import type { Messages } from '../en';

export const terms: Messages['terms'] = {
  title: 'Nutzungsbedingungen',
  binding: (english) => (
    <>Diese Übersetzung dient nur zur Information. Verbindlich ist die {english('englische Fassung')}.</>
  ),
  intro:
    'Dies ist ein kostenloser Dienst zum Ausprobieren. Er führt Code von Fremden auf gemeinsam genutzter Infrastruktur aus. Das funktioniert nur, wenn sich alle an ein paar Regeln halten.',
  sections: {
    forbiddenTitle: 'Was hier nicht erlaubt ist',
    forbidden: [
      'Keine Malware, kein Phishing, keine Krypto-Miner. Nichts, was andere Systeme angreift, scannt, überlastet oder anderweitig stört – hier oder anderswo. Nichts, was Menschen belästigt. Nichts, was Sie nicht veröffentlichen dürfen – dazu gehören fremder Code, fremde Texte, Bilder und Marken.',
      'Speichern oder übermitteln Sie mit einem Lambda keine personenbezogenen Daten anderer Menschen. Eine öffentliche Adresse ist nicht privat, und diese Plattform bietet keine Möglichkeit, solche Daten zu schützen.',
    ],
    actionTitle: 'Was wir dagegen tun dürfen',
    action:
      'Alles, was hier deployt ist, kann jederzeit offline genommen oder gelöscht werden – ohne Ankündigung und ohne dass wir es begründen müssen. In der Praxis passiert das, wenn etwas gegen die Regeln oben verstößt, wenn es den gemeinsam genutzten Server gefährdet oder wenn jemand es meldet und recht hat.',
    lastingTitle: 'Wie lange etwas bleibt',
    lasting: (hours, days) =>
      `Ein Deployment bleibt etwa ${hours} Stunden erreichbar. Ein Lambda, das Sie nicht mehr öffnen, wird samt allen Versionen seines Codes etwa ${days} Tage nach Ihrer letzten Bearbeitung gelöscht. Speichern und Deployen zählen als Bearbeitung – woran Sie arbeiten, bleibt also erhalten. Nichts hier ist ein Backup: Bewahren Sie eine eigene Kopie von Code auf, der Ihnen wichtig ist.`,
    keyTitle: 'Ihr Editor-Link ist Ihr Passwort',
    key: 'Wer den Editor-Link hat, kann das Lambda lesen und ändern – ein Konto oder Passwort gibt es dahinter nicht. Veröffentlichen Sie den Link, kann also jeder das Lambda ändern. Ein verlorener Link lässt sich nicht wiederherstellen.',
    warrantyTitle: 'Keine Gewährleistung',
    warranty:
      'Der Dienst wird bereitgestellt, wie er ist – ohne Garantie, dass er funktioniert, weiter funktioniert oder behält, was Sie darin ablegen. Er kann jederzeit neu gestartet, geändert oder abgeschaltet werden. Bauen Sie darauf nichts, was Ihnen oder anderen wichtig ist.',
    reportTitle: 'Etwas melden',
    report: (mailbox, front) => (
      <>
        Tut ein hier gehostetes Lambda etwas, was es nicht soll, schreiben Sie mit seiner Adresse an {mailbox}. Was die
        Meldung enthalten sollte, steht auf der {front('Startseite')}.
      </>
    ),
  },
  change: 'Diese Bedingungen können sich ändern. Es gilt die Fassung auf dieser Seite.',

  short:
    'Lambdas laufen auf gemeinsam genutzter Infrastruktur. Mit dem Erstellen stimmen Sie zu, keine Malware, Phishing-Seiten, Krypto-Miner oder sonst etwas zu deployen, das andere Systeme angreift, scannt oder überlastet – und nichts zu veröffentlichen, wozu Ihnen die Rechte fehlen. Wer den Editor-Link kennt, kann Ihr Lambda ändern. Behandeln Sie ihn wie ein Passwort. Lambdas im Free-Tarif bleiben online, solange sie genutzt werden: Wird eines einen Monat lang weder besucht noch bearbeitet, geht es offline. Passiert danach zwei weitere Monate nichts, wird es gelöscht. Alles, was Sie deployen, kann jederzeit entfernt werden.',
};
