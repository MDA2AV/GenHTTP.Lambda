import { useCallback, useEffect, useRef, useState } from 'react';
import { Link, useLocation, useNavigate, useParams } from 'react-router-dom';

import { absoluteAddress, isDomain, platformPath } from '../address';
import { ApiError, allowsDomain, api, isActive, isDemo, type ChangeJob, type Lambda, type LambdaSummary, type VersionInfo } from '../api';
import { CopyField } from '../components/CopyField';
import { Diagnostics } from '../components/Diagnostics';
import { Dialog } from '../components/Dialog';
import { IconAlert, IconCheck, IconCopy, IconExternal, IconPlay, IconSpark, IconSpinner } from '../components/Icons';
import { useToast } from '../components/Toast';
import { useAgent } from '../control/agent';
import { ChangeTab } from '../control/ChangeTab';
import type { Busy, Control, Rejection } from '../control/context';
import { DeploymentsTab } from '../control/DeploymentsTab';
import { DomainTab } from '../control/DomainTab';
import { FilesTab } from '../control/FilesTab';
import { LogsTab } from '../control/LogsTab';
import { StatsTab } from '../control/StatsTab';
import { ShowcaseTab } from '../control/ShowcaseTab';
import { SummaryTab } from '../control/SummaryTab';
import { VersionsTab } from '../control/VersionsTab';
import { Workbench } from '../control/Workbench';
import { Menu, StatusBadge, TierBadge, menuItem, menuRule } from '../control/ui';
import { SharedWordsContext } from '../control/words';
import { useEditorT } from '../i18n';
import { registerCompletions, registerResolver, registerSemantics } from '../monaco';
import type { Theme } from '../theme';
import { usePageMeta } from '../meta';

interface Props {
  theme: Theme;
}

type SectionId = 'overview' | 'change' | 'files' | 'versions' | 'deployments' | 'stats' | 'logs' | 'code' | 'showcase' | 'domain';

const SECTIONS: SectionId[] = ['overview', 'change', 'showcase', 'domain', 'files', 'versions', 'deployments', 'stats', 'logs', 'code'];

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
  const t = useEditorT();
  const said = t.frame;

  usePageMeta({ title: said.title, index: false });

  const { privateKey = '', '*': rest = '' } = useParams();
  const navigate = useNavigate();
  const location = useLocation();
  const toast = useToast();

  const segment = rest.split('/')[0];
  const section: SectionId = segment === 'edit' ? 'code' : (SECTIONS.find((id) => id === segment) ?? 'overview');

  const [lambda, setLambda] = useState<Lambda | null>(null);
  const [summary, setSummary] = useState<LambdaSummary | null>(null);
  const [versions, setVersions] = useState<VersionInfo[]>([]);
  const [failure, setFailure] = useState<string | null>(null);
  const [busy, setBusy] = useState<Busy>(null);
  const [rejection, setRejection] = useState<Rejection | null>(null);
  const [renaming, setRenaming] = useState(false);
  const [removing, setRemoving] = useState(false);

  /** Whether the code view holds something unsaved, so leaving it can ask first. */
  const dirty = useRef(false);

  // the creation page says so in the router state
  const fresh = (location.state as { created?: boolean } | null)?.created === true;
  const [welcome, setWelcome] = useState(fresh);

  const base = `/editor/${privateKey}`;

  // completions come from the server, so they always match what compiles
  useEffect(() => {
    api
      .platform()
      .then((platform) => {
        registerCompletions(platform.completions);
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
        setFailure(error instanceof ApiError ? error.message : said.loadFailed);
      }
    });

    return () => {
      alive = false;
    };
  }, [refresh, said]);

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

        toast(said.online(result.lambda?.activeVersion ?? version ?? ''), 'success');
        return true;
      } catch (error) {
        toast(error instanceof ApiError ? error.message : said.deployFailed, 'error');
        return false;
      } finally {
        setBusy(null);
      }
    },
    [privateKey, refresh, toast, said],
  );

  const undeploy = useCallback(async () => {
    setBusy('undeploy');

    try {
      setLambda(await api.undeploy(privateKey));
      await refresh();
      toast(said.offline);
    } catch (error) {
      toast(error instanceof ApiError ? error.message : said.offlineFailed, 'error');
    } finally {
      setBusy(null);
    }
  }, [privateKey, refresh, toast, said]);

  /** Said once when a change this page watched comes to an end, wherever the owner is. */
  const finished = useCallback(
    (job: ChangeJob) => {
      const words = t.change.toast;
      const result = job.result;

      if (job.state === 'cancelled') {
        toast(words.stopped);
      } else if (result?.ok && result.online != null) {
        toast(words.online(result.online));
      } else if (result?.ok && result.version != null) {
        toast(words.saved(result.version), result.compiles === false ? 'error' : 'success');
      } else if (result?.unchanged && job.state === 'done') {
        toast(words.unchanged);
      } else {
        toast(words.failed, 'error');
      }
    },
    [t, toast],
  );

  const agent = useAgent({
    privateKey,
    // a demo is changed by nobody, the agent included
    enabled: lambda != null && !isDemo(lambda.tier),
    watching: section === 'change',
    refresh,
    onFinished: finished,
    failed: t.change.readFailed,
  });

  const go = useCallback(
    (to: string) => {
      if (section === 'code' && dirty.current && !to.startsWith(`${base}/code`)
          && !window.confirm(said.leave)) {
        return;
      }

      dirty.current = false;
      navigate(to);
    },
    [base, navigate, section, said],
  );

  /*
   * A domain of its own is part of the premium tier, so the section is not
   * there for any other: a link to it, or a lambda moved out of the tier
   * while it was open, lands on the overview instead.
   */
  const hidden = lambda != null && !allowsDomain(lambda.tier);

  // a demo is read by anybody holding its announced key, and changed by nobody
  const demo = lambda != null && isDemo(lambda.tier);

  // sections that only change the lambda, which a demo does not have
  const absent = (id: SectionId) => (hidden && id === 'domain') || (demo && (id === 'showcase' || id === 'change'));

  const away = absent(section);

  useEffect(() => {
    if (away) {
      navigate(base, { replace: true });
    }
  }, [away, base, navigate]);

  if (failure) {
    return (
      <div className="mx-auto flex w-full max-w-xl flex-col items-start px-5 py-24">
        <div className="flex h-11 w-11 items-center justify-center bg-red-500/10 text-red-500">
          <IconAlert className="h-5 w-5" />
        </div>
        <h1 className="mt-5 text-2xl font-bold tracking-tight">{said.nothingTitle}</h1>
        <p className="mt-3 text-[15px] text-slate-600 dark:text-slate-400">{failure}</p>
        <button type="button" onClick={() => navigate('/editor/create')} className="btn-primary mt-8 px-5 py-2.5">
          {said.createNew}
        </button>
      </div>
    );
  }

  if (!lambda) {
    return (
      <div className="flex flex-1 items-center justify-center gap-2 text-sm text-slate-500">
        <IconSpinner />
        {said.loading}
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
    agent,
  };

  const change = agent.state?.job;
  const changing = isActive(change);

  const live = lambda.activeVersion != null;
  const publicUrl = absoluteAddress(lambda.publicPath);
  const domainUrl = isDomain(lambda.address) ? lambda.address : null;
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
    <SharedWordsContext.Provider value={t.shared}>
    <div className={code ? 'flex min-h-0 flex-1 flex-col' : 'min-h-0 flex-1 overflow-y-auto'}>
    <div className={`mx-auto flex w-full max-w-[max(80rem,90%)] flex-col md:flex-row md:gap-6 md:px-6 ${code ? 'min-h-0 flex-1' : ''}`}>
      <aside className="shrink-0 border-b border-slate-200 dark:border-ink-800 md:sticky md:top-0 md:flex md:w-56 md:flex-col md:self-start md:border-b-0">
        <div className="px-4 pb-3 pt-4 md:px-3 md:pt-6">
          <div className="flex items-start gap-2">
            <div className="min-w-0 flex-1">
              <span className="block truncate font-mono text-[15px] font-semibold" title={lambda.publicKey}>{lambda.publicKey}</span>
              {/* pulled left by the padding of a badge, so what they say lines
                  up with the name above and the addresses below */}
              <div className="-ml-2 mt-2 flex flex-wrap items-center gap-1.5">
                <StatusBadge version={lambda.activeVersion} />
                <TierBadge tier={lambda.tier} />
              </div>
            </div>

            <Menu label={said.moreActions} align="left">
              {(close) => (
                <>
                  {live && !demo && (
                    <button type="button" role="menuitem" className={menuItem} disabled={busy !== null}
                            onClick={() => { close(); deploy(lambda.activeVersion!); }}>
                      {said.redeploy(lambda.activeVersion!)}
                    </button>
                  )}
                  {live && !demo && (
                    <button type="button" role="menuitem" className={menuItem} disabled={busy !== null}
                            onClick={() => { close(); undeploy(); }}>
                      {said.takeOffline}
                    </button>
                  )}
                  {live && !demo && menuRule}
                  <CopyItem value={editorUrl} label={demo ? said.copyLink : said.copyPrivate} title={said.privateLink} onDone={close} />
                  {!demo && (
                    <button type="button" role="menuitem" className={menuItem} onClick={() => { close(); setRenaming(true); }}>
                      {said.rename}
                    </button>
                  )}
                  <a role="menuitem" href={api.exportUrl(privateKey)} className={menuItem} onClick={close}>
                    {said.download}
                  </a>
                  {!demo && menuRule}
                  {!demo && (
                    <button type="button" role="menuitem" className={`${menuItem} text-red-500`}
                            onClick={() => { close(); setRemoving(true); }}>
                      {said.delete}
                    </button>
                  )}
                </>
              )}
            </Menu>
          </div>

          {/* where it answers: its own domain first when it has one, since
              that is the address its visitors know */}
          <div className="mt-3 space-y-0.5">
            {domainUrl && <Address url={domainUrl} live={live} primary />}
            <Address url={publicUrl} live={live} primary={!domainUrl} />
          </div>

          {/* not while the agent is at work: what it saved last may be a
              version it is still fixing, and it deploys what it finishes */}
          {ahead && latest != null && !demo && !changing && (
            <button
              type="button"
              onClick={() => deploy(latest)}
              disabled={busy !== null}
              className="btn-primary mt-4 w-full"
              title={versions[0]?.change ?? undefined}
            >
              {busy === 'deploy' ? <IconSpinner /> : <IconPlay />}
              {said.deploy(latest)}
            </button>
          )}
        </div>

        <nav aria-label={said.sectionsLabel} className="flex gap-1 overflow-x-auto [scrollbar-width:none] px-3 pb-2 md:mt-2 md:flex-col md:gap-0.5 md:overflow-visible md:px-0">
          {SECTIONS.filter((id) => !absent(id)).map((id) => {
            const to = id === 'overview' ? base : `${base}/${id}`;
            const current = section === id;

            return (
              <Link
                key={id}
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
                {said.sections[id]}
                {id === 'versions' && versions.length > 0 && (
                  <span className="ml-auto text-xs tabular-nums text-slate-400">{versions.length}</span>
                )}
                {id === 'logs' && problems && (
                  <span className="ml-auto h-1.5 w-1.5 rounded-full bg-red-500" title={said.problems} />
                )}
                {id === 'change' && changing && (
                  <span className="ml-auto" title={said.changeRunning}>
                    <IconSpinner className="h-3.5 w-3.5 text-accent-500 dark:text-accent-400" />
                  </span>
                )}
                {id === 'change' && !changing && agent.unseen && (
                  <span
                    className={`ml-auto h-1.5 w-1.5 rounded-full ${change?.result?.ok ? 'bg-emerald-500' : 'bg-amber-500'}`}
                    title={said.changeEnded}
                  />
                )}
              </Link>
            );
          })}
        </nav>
      </aside>

      <main className={`flex min-w-0 flex-1 flex-col ${code ? 'min-h-0' : ''}`}>
        {demo && (
          <div className="mx-4 mt-6 border border-accent-500/30 bg-accent-500/5 px-4 py-3 text-sm md:mx-0">
            <p className="font-medium">{said.demoTitle}</p>
            <p className="mt-1 text-slate-600 dark:text-slate-400">
              {said.demo((text) => (
                <Link to={`/editor/create?from=${encodeURIComponent(lambda.publicKey)}`} className="text-accent-500 hover:underline">
                  {text}
                </Link>
              ))}
            </p>
          </div>
        )}
        {welcome && !demo && (
          <div className="mx-4 mt-6 border border-accent-500/30 bg-accent-500/5 px-4 py-3 md:mx-0">
            <div className="flex items-start justify-between gap-4">
              <p className="text-sm font-medium">{said.keep}</p>
              <button type="button" onClick={() => setWelcome(false)} className="text-xs text-slate-500 hover:underline">
                {said.gotIt}
              </button>
            </div>
            <div className="mt-2 max-w-xl">
              <CopyField value={editorUrl} tone="accent" />
            </div>

          </div>
        )}

        {changing && section !== 'change' && change && (
          <div className="mx-4 mt-6 flex items-center gap-3 border border-accent-500/30 bg-accent-500/5 px-4 py-2.5 text-sm md:mx-0">
            {change.state === 'running'
              ? <IconSpinner className="h-4 w-4 shrink-0 text-accent-500 dark:text-accent-400" />
              : <IconSpark className="h-4 w-4 shrink-0 text-accent-500 dark:text-accent-400" />}
            <p className="min-w-0 flex-1 truncate">
              <span className="font-medium">{change.state === 'running' ? said.changing : said.waiting}</span>
              <span className="text-slate-500"> - {change.prompt}</span>
            </p>
            <Link
              to={`${base}/change`}
              onClick={(event) => { event.preventDefault(); go(`${base}/change`); }}
              className="shrink-0 text-[13px] font-medium text-accent-500 hover:underline dark:text-accent-400"
            >
              {said.follow}
            </Link>
          </div>
        )}

        {section === 'code' ? (
          <Workbench control={control} onDirty={(value) => { dirty.current = value; }} />
        ) : section === 'change' && !demo ? (
          <ChangeTab control={control} />
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
        ) : section === 'showcase' && !demo ? (
          <ShowcaseTab control={control} />
        ) : section === 'domain' && !hidden ? (
          <DomainTab control={control} />
        ) : (
          <SummaryTab control={control} />
        )}
      </main>
    </div>

      <Dialog
        title={rejection?.version != null ? said.rejected(rejection.version) : said.refused}
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
                {said.openCode}
              </button>
            )}
            <button type="button" onClick={() => setRejection(null)} className="btn-primary">
              {said.close}
            </button>
          </>
        }
      >
        <p className="text-slate-600 dark:text-slate-400">{said.notCompiling}</p>
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
          toast(said.moved(platformPath(updated.publicKey)));
        }}
        privateKey={privateKey}
      />

      <Dialog
        title={said.deleteTitle}
        open={removing}
        onClose={() => setRemoving(false)}
        footer={
          <>
            <button type="button" onClick={() => setRemoving(false)} className="btn-ghost">
              {said.cancel}
            </button>
            <button
              type="button"
              className="btn-danger"
              onClick={async () => {
                try {
                  await api.remove(privateKey);
                  navigate('/', { replace: true });
                } catch (error) {
                  toast(error instanceof ApiError ? error.message : said.deleteFailed, 'error');
                }
              }}
            >
              {said.deleteForGood}
            </button>
          </>
        }
      >
        <p className="text-slate-600 dark:text-slate-400">
          {said.deleteText(<code className="font-mono">{lambda.publicKey}</code>)}
        </p>
      </Dialog>
    </div>
    </SharedWordsContext.Provider>
  );
}

/**
 * One address the lambda answers at, with copying and opening beside it. The
 * main one is the link to follow; any other is there to be found, not to
 * compete with it.
 */
function Address({ url, live, primary }: { url: string; live: boolean; primary: boolean }) {
  const said = useEditorT().frame;
  const shown = url.replace(/^https?:\/\//, '').replace(/\/$/, '');

  return (
    <div className={`flex items-center gap-1 ${primary ? 'text-[13px]' : 'text-xs'}`}>
      <a
        href={live ? url : undefined}
        target="_blank"
        rel="noreferrer"
        title={url}
        className={`min-w-0 flex-1 truncate ${
          !live
            ? 'text-slate-400'
            : primary
              ? 'font-medium text-accent-600 hover:underline dark:text-accent-400'
              : 'text-slate-500 hover:text-accent-600 hover:underline dark:hover:text-accent-400'
        }`}
      >
        {shown}
      </a>
      <CopyButton value={url} />
      {live && (
        <a href={url} target="_blank" rel="noreferrer" className="p-1 text-slate-400 hover:text-slate-700 dark:hover:text-slate-200"
           title={said.openInTab} aria-label={said.open(shown)}>
          <IconExternal className="h-3.5 w-3.5" />
        </a>
      )}
    </div>
  );
}

function CopyButton({ value }: { value: string }) {
  const said = useEditorT().frame;
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
      title={said.copyAddress}
      aria-label={said.copyAddress}
    >
      {copied ? <IconCheck className="h-3.5 w-3.5 text-emerald-500" /> : <IconCopy className="h-3.5 w-3.5" />}
    </button>
  );
}

function CopyItem({ value, label, title, onDone }: { value: string; label: string; title: string; onDone: () => void }) {
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
      title={title}
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
  const said = useEditorT().frame;
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
      setError(caught instanceof ApiError ? caught.message : said.renameFailed);
    } finally {
      setWorking(false);
    }
  }

  return (
    <Dialog
      title={said.rename}
      open={open}
      onClose={onClose}
      footer={
        <>
          <button type="button" onClick={onClose} className="btn-ghost">
            {said.cancel}
          </button>
          <button type="button" onClick={submit} disabled={working} className="btn-primary">
            {working && <IconSpinner />}
            {said.moveIt}
          </button>
        </>
      }
    >
      <p className="text-slate-600 dark:text-slate-400">{said.renameText}</p>

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
