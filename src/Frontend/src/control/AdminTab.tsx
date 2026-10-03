import { useCallback, useEffect, useState, type ReactNode } from 'react';

import { useAdminToken } from '../admin';
import { ApiError, api, type AdminLambdaDetail } from '../api';
import { IconAlert, IconExternal, IconSpinner } from '../components/Icons';
import { useToast } from '../components/Toast';
import { useT } from '../i18n';
import type { Control } from './context';
import { Section, Switch } from './ui';

/**
 * What only the operator decides about a lambda, in its editor: whether the
 * sitemap of the installation names it.
 *
 * Shown to a browser holding the admin token - the one entered in the panel -
 * and to nobody else, in either view; the owner never sees it. The server
 * asks for the token as well, so this is a door rather than a disguise, and a
 * token it turns away is forgotten here as the panel forgets it, which takes
 * the section away with it.
 *
 * In English, like the panel it is a part of (CLAUDE.md, Translations). Only
 * its name is the header's word for the panel, which every language has.
 */
export function AdminTab({ control }: { control: Control }) {
  const { lambda } = control;
  const publicKey = lambda.publicKey;
  const online = lambda.activeVersion != null;

  const heading = useT().shell.admin;
  const toast = useToast();

  const [token, setToken] = useAdminToken();
  const [detail, setDetail] = useState<AdminLambdaDetail | null>(null);
  const [failure, setFailure] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  /** Whether the server turned the token away - the host restarted with another one - which locks the panel again. */
  const refused = useCallback((error: unknown) => {
    if (error instanceof ApiError && (error.status === 401 || error.status === 403)) {
      toast('The admin token is no longer accepted. Unlock the panel again to see this section.', 'error');
      setToken('');
      return true;
    }

    return false;
  }, [toast, setToken]);

  useEffect(() => {
    // forgotten a moment ago, and the section is on its way out
    if (token === '') {
      return;
    }

    let alive = true;

    api.admin
      .lambda(token, publicKey)
      .then((read) => {
        if (alive) {
          setDetail(read);
          setFailure(null);
        }
      })
      .catch((error) => {
        if (alive && !refused(error)) {
          setFailure(error instanceof ApiError ? error.message : 'The panel could not be asked about this lambda.');
        }
      });

    return () => {
      alive = false;
    };
  }, [token, publicKey, refused]);

  async function toggle() {
    if (detail === null || saving) {
      return;
    }

    const listed = !detail.sitemap.listed;

    setSaving(true);

    try {
      setDetail(await api.admin.sitemap(token, publicKey, listed));
      toast(listed ? 'Listed in the sitemap.' : 'Taken out of the sitemap.', 'success');
    } catch (error) {
      if (!refused(error)) {
        toast(error instanceof ApiError ? error.message : 'The sitemap could not be changed.', 'error');
      }
    } finally {
      setSaving(false);
    }
  }

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

  if (detail === null) {
    return (
      <Section title={heading} hint={hint} actions={panel}>
        {failure !== null ? (
          <p className="py-10 text-sm text-slate-500">{failure}</p>
        ) : (
          <div className="flex items-center gap-2 py-10 text-sm text-slate-500">
            <IconSpinner /> Asking the panel…
          </div>
        )}
      </Section>
    );
  }

  const { listed, address } = detail.sitemap;

  const shown = address !== null && <code className="break-words font-mono text-slate-700 dark:text-slate-300">{address}</code>;

  const sitemap = (
    <a href="/sitemap.xml" target="_blank" rel="noreferrer" className="text-accent-600 hover:underline dark:text-accent-400">
      The sitemap
    </a>
  );

  // what the switch does where it is now, and what keeps it from doing it
  let state: ReactNode;
  let warning: string | null = null;

  if (address === null) {
    state = listed ? 'Listed.' : 'Not listed.';
    warning = 'This installation has no public address (LAMBDA_PUBLIC_URL), so it serves no sitemap. What is set here applies once it has one.';
  } else if (!listed) {
    state = <>The sitemap does not name it. Listed, it names {shown} for as long as the lambda is online.</>;
  } else if (online) {
    state = <>{sitemap} names it as {shown}.</>;
  } else {
    state = <>Listed as {shown}.</>;
    warning = 'Left out while it is offline: its address answers with an error, which is no use to a search engine. It is named again once it is online.';
  }

  return (
    <Section title={heading} hint={hint} actions={panel}>
      <div className="surface flex items-start gap-4 p-4">
        <div className="min-w-0 flex-1">
          <p id="sitemap-switch" className="text-[15px] font-medium">List in the sitemap</p>
          <p className="mt-1 text-[13px] text-slate-500">{state}</p>

          {/* a sitemap names pages of its own host, so a domain of its own is never what it names */}
          {address !== null && lambda.domainServed && lambda.domain && (
            <p className="mt-1 text-[13px] text-slate-500">
              It answers at <span className="font-mono">{lambda.domain}</span> as well, which the sitemap of this site cannot name.
            </p>
          )}

          {warning && (
            <p className="mt-2 flex items-start gap-1.5 text-[13px] text-amber-700 dark:text-amber-400">
              <IconAlert className="mt-0.5 h-3.5 w-3.5 shrink-0" />
              <span>{warning}</span>
            </p>
          )}
        </div>

        {saving && <IconSpinner />}

        <Switch on={listed} onToggle={toggle} labelledBy="sitemap-switch" />
      </div>
    </Section>
  );
}
