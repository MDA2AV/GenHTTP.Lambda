import { LEGAL_LINK, LegalPage, LegalSection, OperatorAddress } from '../components/Legal';
import { CONTACT_MAIL } from '../contact';
import { useT } from '../i18n';
import { usePublicPage } from '../meta';
import { useLifetimes } from '../site';

const ANTHROPIC_PRIVACY = 'https://www.anthropic.com/legal/privacy';

/**
 * What this site records about the people who use it, and about the people
 * who visit what is hosted on it.
 *
 * It describes the code, so it has to change with it: the request log
 * (Services/Diagnostics/CallerConcern.cs), the build agent and what it logs
 * (docker/agent/builder.mjs), the cookie and the browser storage (i18n,
 * theme.ts, features.ts, admin.ts), and the size of the logs in the compose
 * files. How long a lambda is kept is read from the installation, the same
 * way the terms read it.
 *
 * Every language has it, and every translation says that the English one is
 * the one that applies.
 */
export function Privacy() {
  usePublicPage('/privacy');

  const said = useT().privacy;
  const part = said.sections;

  const { retentionDays: days } = useLifetimes();

  const mailbox = (
    <a className={LEGAL_LINK} href={`mailto:${CONTACT_MAIL}`}>
      {CONTACT_MAIL}
    </a>
  );

  return (
    <LegalPage title={said.title} binding={said.binding} english="/en/privacy" intro={said.intro}>
      <LegalSection title={part.whoTitle}>
        <p>{part.who}</p>
        <OperatorAddress>{mailbox}</OperatorAddress>
      </LegalSection>

      <LegalSection title={part.requestsTitle}>
        {part.requests.map((paragraph) => (
          <p key={paragraph}>{paragraph}</p>
        ))}
      </LegalSection>

      <LegalSection title={part.logsTitle}>
        <p>{part.logs}</p>
      </LegalSection>

      <LegalSection title={part.contentTitle}>
        <p>{part.content(days)}</p>
      </LegalSection>

      <LegalSection title={part.agentTitle}>
        <p>
          {part.agent((text) => (
            <a className={LEGAL_LINK} href={ANTHROPIC_PRIVACY} target="_blank" rel="noreferrer">
              {text}
            </a>
          ))}
        </p>
        <p>{part.agentKept}</p>
      </LegalSection>

      <LegalSection title={part.lambdasTitle}>
        <p>{part.lambdas}</p>
      </LegalSection>

      <LegalSection title={part.mailTitle}>
        <p>{part.mail}</p>
      </LegalSection>

      <LegalSection title={part.storageTitle}>
        <p>{part.storage}</p>
      </LegalSection>

      <LegalSection title={part.hostingTitle}>
        <p>{part.hosting}</p>
      </LegalSection>

      <LegalSection title={part.rightsTitle}>
        <p>{part.rights(mailbox)}</p>
        <p>{part.complaint}</p>
      </LegalSection>

      {/* on lines of their own: a space between two sentences is wrong in Japanese and Korean */}
      <p className="mt-10 text-xs text-slate-500">
        {said.change}
        <br />
        {said.updated}
      </p>
    </LegalPage>
  );
}
