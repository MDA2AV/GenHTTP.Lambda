import { useCallback, useEffect, useState } from 'react';

import { useAdminToken } from '../admin';
import { api, refusedToken, type AdminLambdaDetail } from '../api';
import { IconExternal, IconSpinner } from '../components/Icons';
import { BlockControl } from '../console/BlockControl';
import { useT } from '../i18n';
import type { Control } from './context';
import { Section } from './ui';

/**
 * What only the operator decides about a lambda, in its editor: whether it
 * is blocked - the same control as on its page in the panel.
 *
 * Shown to a browser holding the admin token - the one entered in the panel -
 * and to nobody else, in either view; the owner never sees it. What it reads
 * and does carries the token, as the panel's calls do; a token the server no
 * longer takes is said here, and is entered again in the panel.
 *
 * In English, like the panel it is a part of (CLAUDE.md, Translations). Only
 * its name is the header's word for the panel, which every language has.
 */
export function AdminTab({ control }: { control: Control }) {
  const publicKey = control.lambda.publicKey;

  const heading = useT().shell.admin;

  const [token] = useAdminToken();
  const [detail, setDetail] = useState<AdminLambdaDetail | null>(null);
  const [problem, setProblem] = useState<string | null>(null);

  const load = useCallback(async () => {
    try {
      setDetail(await api.admin.lambda(token, publicKey));
      setProblem(null);
    } catch (error) {
      setProblem(refusedToken(error) ? 'The server no longer takes the admin token in this browser. Enter it again in the panel.' : 'The lambda could not be read.');
    }
  }, [token, publicKey]);

  useEffect(() => {
    load();
  }, [load]);

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
      {detail ? (
        <BlockControl
          token={token}
          detail={detail}
          onChanged={(next) => {
            setDetail(next);
            // the editor around it shows what the block did
            control.refresh().catch(() => undefined);
          }}
          deny={() => setProblem('The server no longer takes the admin token in this browser. Enter it again in the panel.')}
        />
      ) : problem ? (
        <p className="py-10 text-sm text-slate-500">{problem}</p>
      ) : (
        <div className="flex items-center gap-2 py-10 text-sm text-slate-500"><IconSpinner /> Reading the lambda…</div>
      )}
    </Section>
  );
}
