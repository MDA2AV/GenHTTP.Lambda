import { ABUSE_MAILBOX } from '../abuse';
import { LEGAL_LINK, LegalPage, LegalSection } from '../components/Legal';
import { useT } from '../i18n';
import { Link } from '../i18n/links';
import { usePublicPage } from '../meta';
import { useLifetimes } from '../site';

/**
 * The terms in full, as their own page so they can be linked to.
 *
 * The editor shows the short version beside the checkbox, because a wall of
 * text above a button is not read by anyone. This is what that short version
 * is short for, and the two have to agree - the limits here are read from the
 * installation rather than written down twice.
 *
 * Every language has them, and every translation says that the English ones
 * are the ones that apply.
 */
export function Terms() {
  usePublicPage('/terms');

  const said = useT().terms;
  const part = said.sections;

  const { lifetimeHours: hours, retentionDays: days } = useLifetimes();

  return (
    <LegalPage title={said.title} binding={said.binding} english="/en/terms" intro={said.intro}>
      <LegalSection title={part.forbiddenTitle}>
        {part.forbidden.map((paragraph) => (
          <p key={paragraph}>{paragraph}</p>
        ))}
      </LegalSection>

      <LegalSection title={part.actionTitle}>
        <p>{part.action}</p>
      </LegalSection>

      <LegalSection title={part.lastingTitle}>
        <p>{part.lasting(hours, days)}</p>
      </LegalSection>

      <LegalSection title={part.keyTitle}>
        <p>{part.key}</p>
      </LegalSection>

      <LegalSection title={part.warrantyTitle}>
        <p>{part.warranty}</p>
      </LegalSection>

      <LegalSection title={part.reportTitle}>
        <p>
          {part.report(
            <a className={LEGAL_LINK} href={`mailto:${ABUSE_MAILBOX}`}>
              {ABUSE_MAILBOX}
            </a>,
            (text) => (
              <Link className={LEGAL_LINK} to="/">
                {text}
              </Link>
            ),
          )}
        </p>
      </LegalSection>

      <p className="mt-10 text-xs text-slate-500">{said.change}</p>
    </LegalPage>
  );
}
