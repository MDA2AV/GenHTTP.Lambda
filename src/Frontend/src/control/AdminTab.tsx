import { IconExternal } from '../components/Icons';
import { useT } from '../i18n';
import type { Control } from './context';
import { Section } from './ui';

/**
 * What only the operator decides about a lambda, in its editor - nothing at
 * the moment, since the switch for the sitemap went with the lambdas' move to
 * hosts of their own: a sitemap names pages of the host it is served from.
 * The section stays, with the way to the lambda's page in the panel, for what
 * comes next.
 *
 * Shown to a browser holding the admin token - the one entered in the panel -
 * and to nobody else, in either view; the owner never sees it. The panel asks
 * for the token again behind the link.
 *
 * In English, like the panel it is a part of (CLAUDE.md, Translations). Only
 * its name is the header's word for the panel, which every language has.
 */
export function AdminTab({ control }: { control: Control }) {
  const publicKey = control.lambda.publicKey;

  const heading = useT().shell.admin;

  const hint = 'Only you see this section: the admin token in this browser unlocks it. The owner of the lambda never does.';

  const panel = (
    <a
      href={`/admin/lambdas/${encodeURIComponent(publicKey)}`}
      target="_blank"
      rel="noreferrer"
      className="btn-ghost !px-3 !py-1.5 text-[13px]"
      title="Its page in the panel: the tier, the domain, the traffic and the log"
    >
      Open in the panel
      <IconExternal className="h-3.5 w-3.5" />
    </a>
  );

  return (
    <Section title={heading} hint={hint} actions={panel}>
      <p className="py-10 text-sm text-slate-500">Nothing to decide here at the moment.</p>
    </Section>
  );
}
