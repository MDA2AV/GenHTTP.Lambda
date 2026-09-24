import { useCallback, useEffect, useRef, useState } from 'react';
import { Link, useLocation, useNavigate, useParams } from 'react-router-dom';

import { ApiError, api, type Lambda, type LambdaSummary, type VersionInfo } from '../api';
import { CopyField } from '../components/CopyField';
import { Diagnostics } from '../components/Diagnostics';
import { Dialog } from '../components/Dialog';
import { IconAlert, IconCheck, IconCopy, IconExternal, IconPlay, IconSpinner } from '../components/Icons';
import { useToast } from '../components/Toast';
import type { Busy, Control, Rejection } from '../control/context';
import { DeploymentsTab } from '../control/DeploymentsTab';
import { FilesTab } from '../control/FilesTab';
import { LogsTab } from '../control/LogsTab';
import { StatsTab } from '../control/StatsTab';
import { ShowcaseTab } from '../control/ShowcaseTab';
import { SummaryTab } from '../control/SummaryTab';
import { VersionsTab } from '../control/VersionsTab';
import { Workbench } from '../control/Workbench';
import { LiveDot, Menu, menuItem, menuRule } from '../control/ui';
import { registerCompletions, registerResolver, registerSemantics } from '../monaco';
import type { Theme } from '../theme';
import { usePageMeta } from '../meta';

interface Props {
  theme: Theme;
}

type SectionId = 'overview' | 'files' | 'versions' | 'deployments' | 'stats' | 'logs' | 'code' | 'showcase';

const SECTIONS: { id: SectionId; title: string }[] = [
  { id: 'overview', title: 'Overview' },
  { id: 'showcase', title: 'Showcase' },
  { id: 'files', title: 'Files' },
  { id: 'versions', title: 'Versions' },
  { id: 'deployments', title: 'Deployments' },
  { id: 'stats', title: 'Stats' },
  { id: 'logs', title: 'Logs' },
  { id: 'code', title: 'Code' },
];

/**
 * The control center of one lambda.
 *
 * Laid out for the person who owns it rather than the one typing it: most of
 * the code here is written by an agent. The sidebar is the lambda - whether
 * it is online, where, and its sections; the page beside it is one section at
 * a time. Everything done rarely sits behind one menu, so what is left on the
 * screen is what is worth looking at.
 */
export function Editor({ theme }: Props) {
  usePageMeta({ title: 'Editor', index: false });

  const { privateKey = '', '*': rest = '' } = useParams();
  const navigate = useNavigate();
  const location = useLocation();
  const toast = useToast();

  const segment = rest.split('/')[0];
  const section: SectionId = segment === 'edit' ? 'code' : (SECTIONS.find((s) => s.id === segment)?.id ?? 'overview');

  const [lambda, setLambda] = useState<Lambda | null>(null);
  const [summary, setSummary] = useState<LambdaSummary | null>(null);
  const [versions, setVersions] = useState<VersionInfo[]>([]);
  const [failure, setFailure] = useState<string | null>(null);
  const [busy, setBusy] = useState<Busy>(null);
  const [rejection, setRejection] = useState<Rejection | null>(null);
  const [renaming, setRenaming] = useState(false);
  const [removing, setRemoving] = useState(false);
  const [terms, setTerms] = useState<string | null>(null);

  /** Whether the code view holds something unsaved, so leaving it can ask first. */
  const dirty = useRef(false);

  // the assistant navigates with router state; a lambda created from a link
  // elsewhere arrives by redirect, which carries none - so the query says so
  const invited = new URLSearchParams(location.search).get('created') === '1';
  const fresh = (location.state as { created?: boolean } | null)?.created === true || invited;
  const [welcome, setWelcome] = useState(fresh);

  const base = `/editor/${privateKey}`;

  // completions come from the server, so they always match what compiles
  useEffect(() => {
    api
      .platform()
      .then((platform) => {
        registerCompletions(platform.completions);
        setTerms(platform.terms);
      })
      .catch(() => undefined);
  }, []);

  // the compiler colours this lambda's code, so the provider needs its key
  useEffect(() => {
    registerSemantics(async (code) => (await api.semantics(privateKey, code)).tokens);

    registerResolver(async (code, line, column) =>
      (await api.completions(privateKey, code, line, column)).completions);
  }, [privateKey]);

  const refresh = useCallback(async () => {
    const [current, history, figures] = await Promise.all([
      api.get(privateKey),
      api.versions(privateKey),
      api.summary(privateKey).catch(() => null),
    ]);

    setLambda(current);
    setVersions(history);

    if (figures) {
      setSummary(figures);
    }
  }, [privateKey]);

  useEffect(() => {
    let alive = true;

    refresh().catch((error) => {
      if (alive) {
        setFailure(error instanceof ApiError ? error.message : 'This lambda could not be loaded.');
      }
    });

    return () => {
      alive = false;
    };
  }, [refresh]);

  /*
   * An agent can deploy while this is open, so the lambda is read again every
   * so often - more often on the overview, not at all while the page is
   * hidden.
   */
  useEffect(() => {
    const every = section === 'overview' ? 10_000 : 30_000;

    const timer = window.setInterval(() => {
      if (document.visibilityState === 'visible') {
        refresh().catch(() => undefined);
      }
    }, every);

    return () => window.clearInterval(timer);
  }, [refresh, section]);

  /*
   * Opening a section is asking how things are now. The sections that load
   * their own figures do so as they open; the ones that show what the frame
   * holds - the overview, the versions - would otherwise show what it held
   * when it was last read, which after a few requests is plainly wrong. The
   * same goes for coming back to the tab after a while somewhere else.
   */
  const opened = useRef(false);

  useEffect(() => {
    if (!opened.current) {
      // the first read is the load above
      opened.current = true;
      return;
    }

    refresh().catch(() => undefined);
  }, [refresh, section]);

  useEffect(() => {
    const back = () => document.visibilityState === 'visible' && refresh().catch(() => undefined);

    document.addEventListener('visibilitychange', back);

    return () => document.removeEventListener('visibilitychange', back);
  }, [refresh]);

  const deploy = useCallback(
    async (version?: number) => {
      setBusy('deploy');

      try {
        const result = await api.deploy(privateKey, version);

        await refresh();

        if (!result.success) {
          setRejection({ version, diagnostics: result.diagnostics });
          return false;
        }

        toast(`Version ${result.lambda?.activeVersion ?? version} is online.`, 'success');
        return true;
      } catch (error) {
        toast(error instanceof ApiError ? error.message : 'The lambda could not be deployed.', 'error');
        return false;
      } finally {
        setBusy(null);
      }
    },
    [privateKey, refresh, toast],
  );

  const undeploy = useCallback(async () => {
    setBusy('undeploy');

    try {
      setLambda(await api.undeploy(privateKey));
      await refresh();
      toast('Taken offline. The code is still here.');
    } catch (error) {
      toast(error instanceof ApiError ? error.message : 'The lambda could not be taken offline.', 'error');
    } finally {
      setBusy(null);
    }
  }, [privateKey, refresh, toast]);

  const go = useCallback(
    (to: string) => {
      if (section === 'code' && dirty.current && !to.startsWith(`${base}/code`)
          && !window.confirm('Your unsaved changes in the code will be lost. Leave anyway?')) {
        return;
      }

      dirty.current = false;
      navigate(to);
    },
    [base, navigate, section],
  );

  if (failure) {
    return (
      <div className="mx-auto flex w-full max-w-xl flex-col items-start px-5 py-24">
        <div className="flex h-11 w-11 items-center justify-center bg-red-500/10 text-red-500">
          <IconAlert className="h-5 w-5" />
        </div>
        <h1 className="mt-5 text-2xl font-bold tracking-tight">This link does not open anything</h1>
        <p className="mt-3 text-[15px] text-slate-600 dark:text-slate-400">{failure}</p>
        <button type="button" onClick={() => navigate('/editor/create')} className="btn-primary mt-8 px-5 py-2.5">
          Create a new lambda
        </button>
      </div>
    );
  }

  if (!lambda) {
    return (
      <div className="flex flex-1 items-center justify-center gap-2 text-sm text-slate-500">
        <IconSpinner />
        Loading your lambda…
      </div>
    );
  }

  const control: Control = {
    privateKey,
    lambda,
    summary,
    versions,
    busy,
    theme,
    refresh,
    deploy,
    undeploy,
    edit: (version) => go(`${base}/code${version != null ? `?version=${version}` : ''}`),
    browse: (version) => go(`${base}/files${version != null ? `?version=${version}` : ''}`),
  };

  const live = lambda.activeVersion != null;
  const publicUrl = `${window.location.origin}${lambda.publicPath}`;
  const editorUrl = `${window.location.origin}${lambda.editorPath}`;
  const latest = lambda.latestVersion;
  const ahead = latest != null && latest !== lambda.activeVersion;
  const problems = (summary?.recentProblems.length ?? 0) > 0;

  const code = section === 'code';

  /*
   * One centred column holding the sidebar and the section beside it, the
   * way a repository page is laid out, rather than two panes pinned to the
   * edges of a wide window. The page scrolls as a whole and the sidebar stays
   * where it is - except in the code, which is an editor and fills the height.
   */
  return (
    <div className={code ? 'flex min-h-0 flex-1 flex-col' : 'min-h-0 flex-1 overflow-y-auto'}>
    <div className={`mx-auto flex w-full max-w-7xl flex-col md:flex-row md:gap-6 md:px-6 ${code ? 'min-h-0 flex-1' : ''}`}>
      <aside className="shrink-0 border-b border-slate-200 dark:border-ink-800 md:sticky md:top-0 md:flex md:w-56 md:flex-col md:self-start md:border-b-0">
        <div className="px-4 pb-3 pt-4 md:px-3 md:pt-6">
          <div className="flex items-start gap-2">
            <div className="min-w-0 flex-1">
              <div className="flex items-center gap-2">
                <LiveDot live={live} />
                <span className="truncate font-mono text-[15px] font-semibold" title={lambda.publicKey}>{lambda.publicKey}</span>
              </div>
              <p className="mt-1 text-[13px] text-slate-500">
                {live ? `Online, version ${lambda.activeVersion}` : 'Offline'}
              </p>
            </div>

            <Menu label="More actions" align="left">
              {(close) => (
                <>
                  {live && (
                    <button type="button" role="menuitem" className={menuItem} disabled={busy !== null}
                            onClick={() => { close(); deploy(lambda.activeVersion!); }}>
                      Redeploy version {lambda.activeVersion}
                    </button>
                  )}
                  {live && (
                    <button type="button" role="menuitem" className={menuItem} disabled={busy !== null}
                            onClick={() => { close(); undeploy(); }}>
                      Take offline
                    </button>
                  )}
                  {live && menuRule}
                  <CopyItem value={editorUrl} label="Copy the private link" onDone={close} />
                  <button type="button" role="menuitem" className={menuItem} onClick={() => { close(); setRenaming(true); }}>
                    Change the address
                  </button>
                  <a role="menuitem" href={api.exportUrl(privateKey)} className={menuItem} onClick={close}>
                    Download as a .NET project
                  </a>
                  {menuRule}
                  <button type="button" role="menuitem" className={`${menuItem} text-red-500`}
                          onClick={() => { close(); setRemoving(true); }}>
                    Delete this lambda
                  </button>
                </>
              )}
            </Menu>
          </div>

          <div className="mt-3 flex items-center gap-1 text-[13px]">
            <a
              href={live ? publicUrl : undefined}
              target="_blank"
              rel="noreferrer"
              title={publicUrl}
              className={`min-w-0 flex-1 truncate ${live ? 'text-accent-600 hover:underline dark:text-accent-400' : 'text-slate-400'}`}
            >
              {publicUrl.replace(/^https?:\/\//, '')}
            </a>
            <CopyButton value={publicUrl} />
            {live && (
              <a href={publicUrl} target="_blank" rel="noreferrer" className="p-1 text-slate-400 hover:text-slate-700 dark:hover:text-slate-200"
                 title="Open in a new tab" aria-label="Open in a new tab">
                <IconExternal className="h-3.5 w-3.5" />
              </a>
            )}
          </div>

          {ahead && latest != null && (
            <button
              type="button"
              onClick={() => deploy(latest)}
              disabled={busy !== null}
              className="btn-primary mt-4 w-full"
              title={versions[0]?.change ?? undefined}
            >
              {busy === 'deploy' ? <IconSpinner /> : <IconPlay />}
              Deploy version {latest}
            </button>
          )}
        </div>

        <nav aria-label="Sections" className="flex gap-1 overflow-x-auto [scrollbar-width:none] px-3 pb-2 md:mt-2 md:flex-col md:gap-0.5 md:overflow-visible md:px-0">
          {SECTIONS.map((item) => {
            const to = item.id === 'overview' ? base : `${base}/${item.id}`;
            const current = section === item.id;

            return (
              <Link
                key={item.id}
                to={to}
                onClick={(event) => {
                  event.preventDefault();

                  // the section already open, asked for again, is asked for
                  // as it is now
                  if (current) {
                    refresh().catch(() => undefined);
                  } else {
                    go(to);
                  }
                }}
                aria-current={current ? 'page' : undefined}
                className={`flex shrink-0 items-center gap-2 whitespace-nowrap border-b-2 px-3 py-2 text-sm md:border-b-0 md:border-l-2 ${
                  current
                    ? 'border-accent-500 font-medium text-ink-900 dark:border-accent-400 dark:text-slate-100 md:bg-slate-100 md:dark:bg-ink-850'
                    : 'border-transparent text-slate-600 hover:text-slate-900 dark:text-slate-400 dark:hover:text-slate-200'
                }`}
              >
                {item.title}
                {item.id === 'versions' && versions.length > 0 && (
                  <span className="ml-auto text-xs tabular-nums text-slate-400">{versions.length}</span>
                )}
                {item.id === 'logs' && problems && (
                  <span className="ml-auto h-1.5 w-1.5 rounded-full bg-red-500" title="Something went wrong recently" />
                )}
              </Link>
            );
          })}
        </nav>
      </aside>

      <main className={`flex min-w-0 flex-1 flex-col ${code ? 'min-h-0' : ''}`}>
        {welcome && (
          <div className="mx-4 mt-6 border border-accent-500/30 bg-accent-500/5 px-4 py-3 md:mx-0">
            <div className="flex items-start justify-between gap-4">
              <p className="text-sm font-medium">Keep this link. It is the only way back into this lambda.</p>
              <button type="button" onClick={() => setWelcome(false)} className="text-xs text-slate-500 hover:underline">
                Got it
              </button>
            </div>
            <div className="mt-2 max-w-xl">
              <CopyField value={editorUrl} tone="accent" />
            </div>

            {/* someone who arrived from a link elsewhere never saw the terms, so
                they are shown here rather than assumed */}
            {invited && (
              <details className="mt-2.5 max-w-xl text-xs text-slate-600 dark:text-slate-400">
                <summary className="cursor-pointer select-none">What you agree to by using it</summary>
                <p className="mt-2 whitespace-pre-line leading-relaxed">{terms ?? 'Loading the terms…'}</p>
              </details>
            )}
          </div>
        )}

        {section === 'code' ? (
          <Workbench control={control} onDirty={(value) => { dirty.current = value; }} />
        ) : section === 'files' ? (
          <FilesTab control={control} />
        ) : section === 'versions' ? (
          <VersionsTab control={control} />
        ) : section === 'deployments' ? (
          <DeploymentsTab control={control} />
        ) : section === 'stats' ? (
          <StatsTab control={control} />
        ) : section === 'logs' ? (
          <LogsTab control={control} />
        ) : section === 'showcase' ? (
          <ShowcaseTab control={control} />
        ) : (
          <SummaryTab control={control} />
        )}
      </main>
    </div>

      <Dialog
        title={rejection?.version != null ? `Version ${rejection.version} did not go online` : 'The deployment was refused'}
        open={rejection !== null}
        onClose={() => setRejection(null)}
        footer={
          <>
            {rejection?.version != null && (
              <button
                type="button"
                className="btn-ghost"
                onClick={() => {
                  const version = rejection.version;
                  setRejection(null);
                  control.edit(version);
                }}
              >
                Open the code
              </button>
            )}
            <button type="button" onClick={() => setRejection(null)} className="btn-primary">
              Close
            </button>
          </>
        }
      >
        <p className="text-slate-600 dark:text-slate-400">
          It does not compile. Whatever was online before is still online.
        </p>
        <div className="max-h-72 overflow-y-auto border border-slate-200 dark:border-ink-800">
          <Diagnostics diagnostics={rejection?.diagnostics ?? []} state="idle" onSelect={() => undefined} />
        </div>
      </Dialog>

      <RenameDialog
        open={renaming}
        current={lambda.publicKey}
        onClose={() => setRenaming(false)}
        onRenamed={(updated) => {
          setLambda(updated);
          setRenaming(false);
          refresh().catch(() => undefined);
          toast(`Now at /lambda/${updated.publicKey}/.`);
        }}
        privateKey={privateKey}
      />

      <Dialog
        title="Delete this lambda?"
        open={removing}
        onClose={() => setRemoving(false)}
        footer={
          <>
            <button type="button" onClick={() => setRemoving(false)} className="btn-ghost">
              Cancel
            </button>
            <button
              type="button"
              className="btn-danger"
              onClick={async () => {
                try {
                  await api.remove(privateKey);
                  navigate('/', { replace: true });
                } catch (error) {
                  toast(error instanceof ApiError ? error.message : 'The lambda could not be deleted.', 'error');
                }
              }}
            >
              Delete for good
            </button>
          </>
        }
      >
        <p className="text-slate-600 dark:text-slate-400">
          Every version, its files, its history and the address <code className="font-mono">{lambda.publicKey}</code> go
          with it. This cannot be undone.
        </p>
      </Dialog>
    </div>
  );
}

function CopyButton({ value }: { value: string }) {
  const [copied, setCopied] = useState(false);

  return (
    <button
      type="button"
      onClick={async () => {
        try {
          await navigator.clipboard.writeText(value);
          setCopied(true);
          window.setTimeout(() => setCopied(false), 1500);
        } catch {
          // a clipboard that refuses is not worth an error
        }
      }}
      className="p-1 text-slate-400 hover:text-slate-700 dark:hover:text-slate-200"
      title="Copy the address"
      aria-label="Copy the address"
    >
      {copied ? <IconCheck className="h-3.5 w-3.5 text-emerald-500" /> : <IconCopy className="h-3.5 w-3.5" />}
    </button>
  );
}

function CopyItem({ value, label, onDone }: { value: string; label: string; onDone: () => void }) {
  return (
    <button
      type="button"
      role="menuitem"
      className={menuItem}
      onClick={async () => {
        try {
          await navigator.clipboard.writeText(value);
        } catch {
          // as above
        }

        onDone();
      }}
      title="Anyone with this link can change the lambda. Keep it private."
    >
      {label}
    </button>
  );
}

function RenameDialog({
  open,
  current,
  privateKey,
  onClose,
  onRenamed,
}: {
  open: boolean;
  current: string;
  privateKey: string;
  onClose: () => void;
  onRenamed: (lambda: Lambda) => void;
}) {
  const [value, setValue] = useState(current);
  const [error, setError] = useState<string | null>(null);
  const [working, setWorking] = useState(false);

  useEffect(() => {
    if (open) {
      setValue(current);
      setError(null);
    }
  }, [open, current]);

  async function submit() {
    setWorking(true);
    setError(null);

    try {
      onRenamed(await api.changeKey(privateKey, value.trim()));
    } catch (caught) {
      setError(caught instanceof ApiError ? caught.message : 'The address could not be changed.');
    } finally {
      setWorking(false);
    }
  }

  return (
    <Dialog
      title="Change the address"
      open={open}
      onClose={onClose}
      footer={
        <>
          <button type="button" onClick={onClose} className="btn-ghost">
            Cancel
          </button>
          <button type="button" onClick={submit} disabled={working} className="btn-primary">
            {working && <IconSpinner />}
            Move it
          </button>
        </>
      }
    >
      <p className="text-slate-600 dark:text-slate-400">
        The old address stops working right away, so update anything that links to it.
      </p>

      <div className="flex items-center gap-2">
        <span className="shrink-0 font-mono text-sm text-slate-500">/lambda/</span>
        <input
          value={value}
          onChange={(event) => setValue(event.target.value)}
          onKeyDown={(event) => event.key === 'Enter' && submit()}
          spellCheck={false}
          autoComplete="off"
          className="field font-mono"
        />
      </div>

      {error && <p className="text-xs text-red-500">{error}</p>}
    </Dialog>
  );
}
