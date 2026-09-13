import { useCallback, useEffect, useState } from 'react';
import { useLocation, useNavigate, useParams } from 'react-router-dom';

import {
  ApiError,
  api,
  type Diagnostic,
  type Lambda,
  type VersionInfo,
} from '../api';
import { CodeEditor } from '../components/CodeEditor';
import { CopyField } from '../components/CopyField';
import { Storage } from '../components/Storage';
import { Diagnostics } from '../components/Diagnostics';
import { Dialog } from '../components/Dialog';
import { IconAlert, IconFolder, IconHistory, IconPlay, IconSave, IconSpinner, IconStop, IconTrash } from '../components/Icons';
import { useToast } from '../components/Toast';
import { registerCompletions } from '../monaco';
import type { Theme } from '../theme';

type Busy = 'save' | 'check' | 'deploy' | 'undeploy' | null;

interface Props {
  theme: Theme;
}

export function Editor({ theme }: Props) {
  const { privateKey = '' } = useParams();
  const navigate = useNavigate();
  const location = useLocation();
  const toast = useToast();

  const [lambda, setLambda] = useState<Lambda | null>(null);
  const [versions, setVersions] = useState<VersionInfo[]>([]);
  const [loaded, setLoaded] = useState<number | null>(null);
  const [code, setCode] = useState('');
  const [saved, setSaved] = useState('');
  const [diagnostics, setDiagnostics] = useState<Diagnostic[]>([]);
  const [built, setBuilt] = useState<'idle' | 'clean'>('idle');
  const [busy, setBusy] = useState<Busy>(null);
  const [failure, setFailure] = useState<string | null>(null);
  const [reveal, setReveal] = useState<{ line: number; column: number; nonce: number }>();
  const [renaming, setRenaming] = useState(false);
  const [removing, setRemoving] = useState(false);
  const [storage, setStorage] = useState(false);

  const fresh = (location.state as { created?: boolean } | null)?.created === true;
  const dirty = code !== saved;

  // completions come from the server, so they always match what compiles
  useEffect(() => {
    api.platform().then((platform) => registerCompletions(platform.completions)).catch(() => undefined);
  }, []);

  useEffect(() => {
    let active = true;

    async function load() {
      try {
        const current = await api.get(privateKey);
        const history = await api.versions(privateKey);

        if (!active) {
          return;
        }

        setLambda(current);
        setVersions(history);

        const target = current.activeVersion ?? current.latestVersion ?? history[0]?.version;

        if (target != null) {
          const content = await api.version(privateKey, target);

          if (active) {
            setCode(content.code);
            setSaved(content.code);
            setLoaded(target);
          }
        }
      } catch (error) {
        if (active) {
          setFailure(error instanceof ApiError ? error.message : 'This lambda could not be loaded.');
        }
      }
    }

    load();

    return () => {
      active = false;
    };
  }, [privateKey]);

  useEffect(() => {
    if (!dirty) {
      return;
    }

    function warn(event: BeforeUnloadEvent) {
      event.preventDefault();
    }

    window.addEventListener('beforeunload', warn);

    return () => window.removeEventListener('beforeunload', warn);
  }, [dirty]);

  const refresh = useCallback(async () => {
    const [current, history] = await Promise.all([api.get(privateKey), api.versions(privateKey)]);

    setLambda(current);
    setVersions(history);

    return current;
  }, [privateKey]);

  const store = useCallback(async () => {
    const version = await api.save(privateKey, code);

    setSaved(code);
    setLoaded(version.version);

    await refresh();

    return version.version;
  }, [privateKey, code, refresh]);

  const save = useCallback(async () => {
    if (busy) {
      return;
    }

    if (!dirty) {
      toast('There is nothing to save.');
      return;
    }

    setBusy('save');

    try {
      const version = await store();
      toast(`Saved as version ${version}.`);
    } catch (error) {
      toast(error instanceof ApiError ? error.message : 'The code could not be saved.', 'error');
    } finally {
      setBusy(null);
    }
  }, [busy, dirty, store, toast]);

  async function check() {
    setBusy('check');

    try {
      const result = await api.check(privateKey, code);

      setDiagnostics(result.diagnostics);
      setBuilt(result.success ? 'clean' : 'idle');

      toast(result.success ? 'The code compiles.' : 'The code does not compile yet.', result.success ? 'success' : 'error');
    } catch (error) {
      toast(error instanceof ApiError ? error.message : 'The code could not be checked.', 'error');
    } finally {
      setBusy(null);
    }
  }

  async function deploy(version?: number) {
    setBusy('deploy');

    try {
      // deploying always publishes what is in the editor, so an unsaved change
      // becomes a version of its own first
      const target = version ?? (dirty ? await store() : (loaded ?? undefined));

      const result = await api.deploy(privateKey, target);

      setDiagnostics(result.diagnostics);
      setBuilt(result.success ? 'clean' : 'idle');

      if (result.lambda) {
        setLambda(result.lambda);
      }

      await refresh();

      if (result.success) {
        if (version != null) {
          const content = await api.version(privateKey, version);
          setCode(content.code);
          setSaved(content.code);
          setLoaded(version);
        }

        toast(`Version ${result.lambda?.activeVersion ?? target} is live.`);
      } else {
        toast('The deployment was rejected, see the messages below.', 'error');
      }
    } catch (error) {
      toast(error instanceof ApiError ? error.message : 'The lambda could not be deployed.', 'error');
    } finally {
      setBusy(null);
    }
  }

  async function undeploy() {
    setBusy('undeploy');

    try {
      setLambda(await api.undeploy(privateKey));
      toast('The lambda is offline. Its code is still here.');
    } catch (error) {
      toast(error instanceof ApiError ? error.message : 'The lambda could not be taken down.', 'error');
    } finally {
      setBusy(null);
    }
  }

  async function open(version: number) {
    if (dirty && !window.confirm('Your unsaved changes will be replaced. Continue?')) {
      return;
    }

    try {
      const content = await api.version(privateKey, version);

      setCode(content.code);
      setSaved(content.code);
      setLoaded(version);
      setDiagnostics([]);
      setBuilt('idle');
    } catch (error) {
      toast(error instanceof ApiError ? error.message : 'That version could not be loaded.', 'error');
    }
  }

  if (failure) {
    return (
      <div className="mx-auto flex w-full max-w-xl flex-col items-start px-5 py-24">
        <div className="flex h-11 w-11 items-center justify-center rounded-xl bg-red-500/10 text-red-500">
          <IconAlert className="h-5 w-5" />
        </div>
        <h1 className="mt-5 text-2xl font-bold tracking-tight">This editor link does not work</h1>
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

  const publicUrl = `${window.location.origin}${lambda.publicPath}`;
  const editorUrl = `${window.location.origin}${lambda.editorPath}`;
  const live = lambda.activeVersion != null;

  return (
    <div className="flex min-h-0 flex-1 flex-col">
      <Toolbar
        live={live}
        activeVersion={lambda.activeVersion}
        dirty={dirty}
        busy={busy}
        onCheck={check}
        onSave={save}
        onDeploy={() => deploy()}
        onUndeploy={undeploy}
        onFiles={() => setStorage(true)}
      />

      {storage && <Storage privateKey={privateKey!} onClose={() => setStorage(false)} />}

      {fresh && (
        <div className="border-b border-accent-500/30 bg-accent-500/5 px-4 py-3 sm:px-6">
          <p className="text-sm font-medium">Save this link - it is the only way back into this editor.</p>
          <div className="mt-2 max-w-xl">
            <CopyField value={editorUrl} tone="accent" />
          </div>
        </div>
      )}

      <div className="flex min-h-0 flex-1 flex-col lg:flex-row">
        <div className="flex min-h-0 flex-1 flex-col">
          <div className="min-h-[18rem] flex-1">
            <CodeEditor
              value={code}
              theme={theme}
              diagnostics={diagnostics}
              reveal={reveal}
              onChange={setCode}
              onSave={save}
            />
          </div>

          <div className="max-h-52 shrink-0 overflow-y-auto border-t border-slate-200 dark:border-ink-800">
            <Diagnostics
              diagnostics={diagnostics}
              state={built}
              onSelect={(diagnostic) =>
                setReveal({ line: diagnostic.line, column: diagnostic.column, nonce: Date.now() })
              }
            />
          </div>
        </div>

        <aside className="shrink-0 space-y-6 overflow-y-auto border-t border-slate-200 px-4 py-5 dark:border-ink-800 lg:w-80 lg:border-l lg:border-t-0">
          <CopyField label="Public URL" value={publicUrl} href={live ? publicUrl : undefined} />

          <CopyField label="Editor link (keep private)" value={editorUrl} />

          <div>
            <div className="mb-2 flex items-center gap-1.5 text-xs font-medium uppercase tracking-wide text-slate-500">
              <IconHistory className="h-3.5 w-3.5" />
              Versions
            </div>

            <ul className="space-y-1">
              {versions.map((version) => {
                const isLoaded = version.version === loaded;
                const isLive = version.version === lambda.activeVersion;

                return (
                  <li
                    key={version.version}
                    className={`flex items-center gap-2 rounded-lg px-2.5 py-1.5 text-sm ${
                      isLoaded ? 'bg-slate-100 dark:bg-ink-850' : ''
                    }`}
                  >
                    <button
                      type="button"
                      onClick={() => open(version.version)}
                      className="min-w-0 flex-1 text-left"
                      title="Load this version into the editor"
                    >
                      <span className="font-medium">v{version.version}</span>
                      <span className="ml-2 text-xs text-slate-500">
                        {new Date(version.created).toLocaleString()}
                      </span>
                    </button>

                    {isLive ? (
                      <span className="chip bg-emerald-500/10 text-emerald-600 dark:text-emerald-400">live</span>
                    ) : (
                      <button
                        type="button"
                        onClick={() => deploy(version.version)}
                        disabled={busy !== null}
                        className="text-xs font-medium text-accent-500 hover:underline disabled:opacity-50"
                        title="Deploy this version"
                      >
                        deploy
                      </button>
                    )}
                  </li>
                );
              })}
            </ul>
          </div>

          <div className="space-y-2 border-t border-slate-200 pt-5 dark:border-ink-800">
            <button type="button" onClick={() => setRenaming(true)} className="btn-ghost w-full">
              Change public key
            </button>
            <button type="button" onClick={() => setRemoving(true)} className="btn-danger w-full">
              <IconTrash />
              Delete lambda
            </button>
          </div>
        </aside>
      </div>

      <RenameDialog
        open={renaming}
        current={lambda.publicKey}
        onClose={() => setRenaming(false)}
        onRenamed={(updated) => {
          setLambda(updated);
          setRenaming(false);
          toast(`Now hosted at /lambda/${updated.publicKey}/.`);
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
          Every version, the workspace and the key <code className="font-mono">{lambda.publicKey}</code> are
          removed. This cannot be undone.
        </p>
      </Dialog>
    </div>
  );
}

function Toolbar({
  live,
  activeVersion,
  dirty,
  busy,
  onCheck,
  onSave,
  onDeploy,
  onUndeploy,
  onFiles,
}: {
  live: boolean;
  activeVersion?: number;
  dirty: boolean;
  busy: Busy;
  onCheck: () => void;
  onSave: () => void;
  onDeploy: () => void;
  onUndeploy: () => void;
  onFiles: () => void;
}) {
  return (
    <div className="flex flex-wrap items-center gap-2 border-b border-slate-200 px-4 py-2.5 dark:border-ink-800 sm:px-6">
      <span
        className={`chip ${
          live
            ? 'bg-emerald-500/10 text-emerald-600 dark:text-emerald-400'
            : 'bg-slate-400/10 text-slate-500'
        }`}
      >
        <span className={`h-1.5 w-1.5 rounded-full ${live ? 'bg-emerald-500' : 'bg-slate-400'}`} />
        {live ? `live · v${activeVersion}` : 'offline'}
      </span>

      {dirty && <span className="chip bg-amber-500/10 text-amber-600 dark:text-amber-400">unsaved changes</span>}

      <div className="ml-auto flex items-center gap-2">
        <button type="button" onClick={onFiles} className="btn-ghost" title="Workspace files">
          <IconFolder />
          Files
        </button>

        <button type="button" onClick={onCheck} disabled={busy !== null} className="btn-ghost">
          {busy === 'check' ? <IconSpinner /> : null}
          Check
        </button>

        <button type="button" onClick={onSave} disabled={busy !== null || !dirty} className="btn-ghost" title="Ctrl+S">
          {busy === 'save' ? <IconSpinner /> : <IconSave />}
          Save
        </button>

        {live && (
          <button type="button" onClick={onUndeploy} disabled={busy !== null} className="btn-ghost">
            {busy === 'undeploy' ? <IconSpinner /> : <IconStop />}
            Undeploy
          </button>
        )}

        <button type="button" onClick={onDeploy} disabled={busy !== null} className="btn-primary">
          {busy === 'deploy' ? <IconSpinner /> : <IconPlay />}
          Deploy
        </button>
      </div>
    </div>
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
      setError(caught instanceof ApiError ? caught.message : 'The key could not be changed.');
    } finally {
      setWorking(false);
    }
  }

  return (
    <Dialog
      title="Change the public key"
      open={open}
      onClose={onClose}
      footer={
        <>
          <button type="button" onClick={onClose} className="btn-ghost">
            Cancel
          </button>
          <button type="button" onClick={submit} disabled={working} className="btn-primary">
            {working && <IconSpinner />}
            Move the lambda
          </button>
        </>
      }
    >
      <p className="text-slate-600 dark:text-slate-400">
        The old URL stops working right away, so update anything that links to it.
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
