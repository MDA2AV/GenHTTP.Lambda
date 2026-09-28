import { ABUSE_MAILBOX } from '../abuse';
import { LEGAL_LINK, LegalPage, LegalSection, OperatorAddress } from '../components/Legal';
import { CONTACT_MAIL } from '../contact';
import { useT } from '../i18n';
import { usePublicPage } from '../meta';

/**
 * Who runs this site - the Impressum German law asks of every site run from
 * Germany (§ 5 DDG). Every language has it, but only the German footer links
 * to it (see Footer.tsx).
 *
 * Facts rather than rules, so no translation carries the note that the
 * English one applies. The address is the privacy policy's, from the same
 * constant.
 */
export function Imprint() {
  usePublicPage('/imprint');

  const said = useT().imprint;
  const part = said.sections;

  const mail = (
    <a className={LEGAL_LINK} href={`mailto:${CONTACT_MAIL}`}>
      {CONTACT_MAIL}
    </a>
  );

  const abuse = (
    <a className={LEGAL_LINK} href={`mailto:${ABUSE_MAILBOX}`}>
      {ABUSE_MAILBOX}
    </a>
  );

  return (
    <LegalPage title={said.title} binding={null} english="/en/imprint" intro={said.intro}>
      <LegalSection title={part.providerTitle}>
        <OperatorAddress />
      </LegalSection>

      <LegalSection title={part.contactTitle}>
        <p>{part.contact(mail, abuse)}</p>
      </LegalSection>

      <LegalSection title={part.editorialTitle}>
        <p>{part.editorial}</p>
        <OperatorAddress />
      </LegalSection>

      <LegalSection title={part.dsaTitle}>
        <p>{part.dsa(mail)}</p>
      </LegalSection>
    </LegalPage>
  );
}
