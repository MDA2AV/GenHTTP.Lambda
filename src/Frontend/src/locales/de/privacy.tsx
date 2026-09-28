import type { Messages } from '../en';

export const privacy: Messages['privacy'] = {
  title: 'Datenschutzerklärung',
  binding: (english) => (
    <>Diese Übersetzung dient nur zur Information. Verbindlich ist die {english('englische Fassung')}.</>
  ),
  intro:
    'Was diese Seite über Sie erfährt, was sie damit macht, wie lange sie es behält und wer es sonst noch sehen kann. Kurz gesagt: Es gibt keine Konten, keine Werbung und kein Tracking. Der Server notiert, wer was bei ihm abgerufen hat. So bleibt er am Laufen, und Missbrauch lässt sich zurückverfolgen. Was Sie beim Build-Agenten in Auftrag geben, geht an Anthropic – dessen Modell schreibt die App.',
  sections: {
    whoTitle: 'Wer verantwortlich ist',
    who: 'Die unten genannte Person betreibt diese Seite und ist nach der Datenschutz-Grundverordnung der EU (DSGVO) für die personenbezogenen Daten verantwortlich, die dabei verarbeitet werden. Schreiben Sie bei allen Fragen zu dieser Seite an diese Adresse:',

    requestsTitle: 'Was der Server bei jeder Anfrage notiert',
    requests: [
      'Jede Anfrage an diese Seite und an jedes hier gehostete Lambda landet im Log des Servers: die IP-Adresse, von der sie kam, die Adresse, für die sie nach eigener Angabe weitergeleitet wurde, der Browser oder das Programm, das sie geschickt hat, die aufgerufene Adresse, der Zeitpunkt und die Antwort des Servers. Außerdem schlägt der Server nach, zu welchem Land, welchem Ort und welchem Netz die IP-Adresse gehört – in einer Datenbank, die er selbst vorhält. Niemand sonst wird dafür gefragt.',
      'So finden wir Fehler, so führen wir einen überlasteten Server auf die Ursache zurück, und so gehen wir Missbrauch nach, der uns gemeldet wird. Außerdem nutzt der Server die IP-Adresse, nur im Arbeitsspeicher, um zu begrenzen, wie viele Anfragen und Builds eine Person auslösen kann. Ohne diese Angaben lässt sich eine Anfrage nicht beantworten. Rechtsgrundlage ist unser berechtigtes Interesse daran, den Dienst zu betreiben und sicher zu halten (Art. 6 Abs. 1 lit. f DSGVO).',
      'Administratoren können alles davon lesen. Wer ein Lambda besitzt, sieht bei jeder Anfrage an dieses Lambda das Land und den Browser, aber nicht die IP-Adresse.',
    ],

    logsTitle: 'Wie lange das Log aufbewahrt wird',
    logs: 'Das Log liegt an zwei Stellen: im Arbeitsspeicher des Servers, der bei jedem Neustart geleert wird, und in der Konsolenausgabe des Servers, die bei jedem Update gelöscht wird. Beide haben eine feste Größe. Jede neue Zeile verdrängt also die älteste, und wie lange eine Zeile erhalten bleibt, hängt davon ab, wie viel auf der Seite los ist. Nichts aus dem Log wird archiviert.',

    contentTitle: 'Was Sie hier ablegen',
    content: (days) =>
      `Ein Lambda besteht aus seinem Code, seinen Dateien, seinen Einstellungen und den Notizen, die mit seinen Versionen gespeichert werden: was gewünscht war und was sich geändert hat. All das liegt auf dem Server, damit das Lambda laufen und bearbeitet werden kann. Ein kostenloses Lambda wird samt allen Versionen etwa ${days} Tage nach der letzten Änderung oder dem letzten Besuch gelöscht – und sofort, wenn jemand mit dem Editor-Link es löscht. Wer den Editor-Link hat, kann alles davon lesen. Was Sie in den Showcase stellen, sehen alle. Und Administratoren sehen sich ein Lambda an, wenn es nötig ist, etwa um einer Meldung nachzugehen oder den Server zu schützen. Rechtsgrundlage ist, dass wir den Dienst erbringen, den Sie angefordert haben (Art. 6 Abs. 1 lit. b DSGVO).`,

    agentTitle: 'Was Sie dem Build-Agenten sagen',
    agent: (policy) => (
      <>
        Was Sie auf der Seite „App bauen“ in das Eingabefeld tippen, geht an Anthropic PBC in den USA. Anthropic
        betreibt Claude, das Modell, das die App schreibt. Was Anthropic damit macht, steht in{' '}
        {policy('der Datenschutzerklärung von Anthropic')}. Die USA schützen personenbezogene Daten nicht so wie die EU.
        Ihre Anfrage wird dorthin geschickt, weil das nötig ist, um zu bauen, was Sie möchten
        (Art. 6 Abs. 1 lit. b und Art. 49 Abs. 1 lit. b DSGVO). Schreiben Sie also nichts hinein, was Sie nicht
        weitergeben möchten.
      </>
    ),
    agentKept:
      'Der Agent speichert Ihre Anfrage – oft in eigenen Worten – als Notiz zu der Version, die er schreibt. Die ersten paar hundert Zeichen davon landen im Log des Build-Dienstes, das ebenfalls eine feste Größe hat. Nutzen Sie stattdessen Ihren eigenen Agenten, etwa Claude oder Claude Code, geht das, was Sie ihm sagen, an dessen Anbieter und nicht an uns. Wir bekommen nur den Code und die Notizen, die er hierher schickt.',

    lambdasTitle: 'Was ein Lambda tut, entscheidet sein Besitzer',
    lambdas:
      'Ein Lambda wird von der Person geschrieben, die den Editor-Link hat – nicht von uns. Was es von seinen Besuchern wissen will und was es damit macht, entscheidet diese Person. Diese Erklärung gilt dafür nicht, mit Ausnahme des Anfrage-Logs oben, das der Server für jedes Lambda führt. Die Nutzungsbedingungen verbieten, mit einem Lambda personenbezogene Daten anderer Menschen zu sammeln. Wenn Sie auf eines stoßen, das es trotzdem tut, melden Sie es bitte.',

    mailTitle: 'Wenn Sie uns schreiben',
    mail: 'Wenn Sie uns schreiben – um Missbrauch zu melden oder aus einem anderen Grund –, nutzen wir Ihre Adresse und Ihre Nachricht, um Ihnen zu antworten und Ihr Anliegen zu bearbeiten. Sobald wir sie dafür nicht mehr brauchen, löschen wir sie (Art. 6 Abs. 1 lit. f DSGVO).',

    storageTitle: 'Cookies und Ihr Browser',
    storage:
      'Es gibt ein einziges Cookie. Es heißt lang, merkt sich die Sprache, die Sie gewählt haben, damit sich Adressen ohne Sprachangabe in dieser Sprache öffnen, und hält ein Jahr. Im Speicher Ihres Browsers liegen außerdem das helle oder dunkle Design, ein paar Einstellungen der Seiten, die Sie nutzen, und bei Administratoren ihr Token. Nichts davon dient dazu, Sie zu verfolgen, und nichts davon geht an andere: Es gibt keine Analyse-Tools, keine Werbung, und nichts wird von fremden Seiten geladen, nicht einmal Schriftarten. Weil all das nur tut, was Sie selbst wollen, ist keine Einwilligung nötig (§ 25 Abs. 2 Nr. 2 TDDDG).',

    hostingTitle: 'Wo die Daten liegen',
    hosting:
      'Der Server, auf dem all das läuft, ist bei einem Hosting-Anbieter in der Europäischen Union gemietet. Dort wird gespeichert, was diese Seite beschreibt.',

    rightsTitle: 'Ihre Rechte',
    rights: (mailbox) => (
      <>
        Sie können fragen, was wir über Sie gespeichert haben, und eine Kopie davon verlangen. Sie können verlangen, dass
        es berichtigt, gelöscht oder nur noch eingeschränkt genutzt wird, und allem widersprechen, was wir aufgrund
        unseres berechtigten Interesses tun (Art. 15 bis 21 DSGVO). Schreiben Sie dazu an {mailbox}. Da es keine Konten
        gibt, finden wir Ihre Daten nur, wenn Sie uns sagen, wie: mit der IP-Adresse, die Sie genutzt haben, und dem
        ungefähren Zeitpunkt, oder mit der Adresse Ihres Lambdas. Keine Entscheidung, die rechtliche oder ähnlich
        erhebliche Folgen für Sie hat, wird automatisch getroffen (Art. 22 DSGVO).
      </>
    ),
    complaint:
      'Sie können sich auch bei einer Datenschutz-Aufsichtsbehörde beschweren – dort, wo Sie wohnen, oder dort, wo wir sitzen. Für uns zuständig ist der Landesbeauftragte für den Datenschutz und die Informationsfreiheit Baden-Württemberg (LfDI Baden-Württemberg).',
  },
  change: 'Diese Erklärung ändert sich, wenn sich die Seite ändert. Es gilt die Fassung auf dieser Seite.',
  updated: 'Zuletzt geändert am 28. September 2026.',
};
