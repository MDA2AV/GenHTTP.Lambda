import { useCallback, useEffect, useState } from 'react';
import { useSearchParams } from 'react-router-dom';

import { ApiError, api, isDemo, type Diagnostic, type LambdaFile } from '../api';
import { CodeEditor } from '../components/CodeEditor';
import { Diagnostics } from '../components/Diagnostics';
import { Dialog } from '../components/Dialog';
import { ENTRY, FileTabs } from '../components/FileTabs';
import { IconPlay, IconSpinner } from '../components/Icons';
import { useToast } from '../components/Toast';
import { useEditorT } from '../i18n';
import { languageFor } from '../monaco';
import type { Control } from './context';
import { Section } from './ui';

type Busy = 'save' | 'check' | 'deploy' | null;

/**
 * Writing the code by hand.
 *
 * Built like every other section: the files are its views, so they are the
 * pills under the title, and checking, saving and deploying are its actions.
 *
 * The newest version is the one being worked on, so saving changes it where
 * it is - as often as it takes, without a question every time - and what is
 * online only follows when it is deployed. Keeping it as it is and carrying
 * on in a new one is a decision rather than a side effect of saving, and asks
 * what the new version is for: the same note an agent leaves, so a version
 * written by hand reads as well in the history as one that was not. A version
 * that is not the newest is history, and saving it can only make a new one.
 */
export function Workbench({ control, onDirty }: { control: Control; onDirty: (dirty: boolean) => void }) {
  const { privateKey, lambda } = control;

  const said = useEditorT().code;
  const toast = useToast();
  const [params, setParams] = useSearchParams();

  // what an agent works on is the newest, so that is where editing starts,
  // unless a particular version was asked for
  const requested = Number(params.get('version')) || lambda.latestVersion || lambda.activeVersion;

  const [loaded, setLoaded] = useState<number | null>(null);
  const [files, setFiles] = useState<LambdaFile[]>([{ name: ENTRY, code: '' }]);
  const [saved, setSaved] = useState('');
  const [active, setActive] = useState(ENTRY);
  const [diagnostics, setDiagnostics] = useState<Diagnostic[]>([]);
  const [built, setBuilt] = useState<'idle' | 'clean'>('idle');
  const [busy, setBusy] = useState<Busy>(null);
  const [reveal, setReveal] = useState<{ line: number; column: number; nonce: number }>();

  /** Whether the dialog for a new version is open, and whether it deploys afterwards. */
  const [saving, setSaving] = useState<'save' | 'deploy' | null>(null);
  const [change, setChange] = useState('');

  const current = files.find((file) => file.name === active) ?? files[0];
  const code = current?.code ?? '';
  const dirty = saved !== '' && JSON.stringify(files) !== saved;

  // only the newest version is saved over; anything older is history, and
  // with nothing saved yet there is nothing to save over either
  const newest = loaded != null && loaded === lambda.latestVersion;

  useEffect(() => {
    onDirty(dirty);
  }, [dirty, onDirty]);

  const setCode = useCallback((next: string) => {
    setFiles((all) => all.map((file) => (file.name === active ? { ...file, code: next } : file)));
  }, [active]);

  const adopt = useCallback((incoming: LambdaFile[]) => {
    const usable = incoming.length > 0 ? incoming : [{ name: ENTRY, code: '' }];

    setFiles(usable);
    setSaved(JSON.stringify(usable));
    setActive((was) => (usable.some((file) => file.name === was) ? was : usable[0].name));
  }, []);

  useEffect(() => {
    if (requested == null) {
      setSaved(JSON.stringify(files));
      return;
    }

    // a version this view just made is already what it shows
    if (requested === loaded) {
      return;
    }

    let alive = true;

    api
      .version(privateKey, requested)
      .then((content) => {
        if (alive) {
          adopt(content.files);
          setLoaded(requested);
          setDiagnostics([]);
          setBuilt('idle');
        }
      })
      .catch((error) => toast(error instanceof ApiError ? error.message : said.loadFailed, 'error'));

    return () => {
      alive = false;
    };
    // only a different version is a reason to reload what is being edited
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [privateKey, requested, adopt]);

  useEffect(() => {
    if (!dirty) {
      return;
    }

    const warn = (event: BeforeUnloadEvent) => event.preventDefault();

    window.addEventListener('beforeunload', warn);

    return () => window.removeEventListener('beforeunload', warn);
  }, [dirty]);

  /** Where a name was declared, and going there - only ever to a file of this lambda. */
  const goToDefinition = useCallback(
    async (line: number, column: number) => {
      if (!active.endsWith('.cs')) {
        return;
      }

      try {
        const at = await api.definition(privateKey, files, active, line, column);

        if (!at.file) {
          return;
        }

        if (at.file !== active) {
          setActive(at.file);
        }

        setReveal({ line: at.line + 1, column: at.column + 1, nonce: Date.now() });
      } catch {
        // nowhere to go is not worth interrupting anybody over
      }
    },
    [privateKey, files, active],
  );

  async function check() {
    setBusy('check');

    try {
      const result = await api.check(privateKey, files);

      setDiagnostics(result.diagnostics);
      setBuilt(result.success ? 'clean' : 'idle');

      toast(result.success ? said.compiles : said.notYet, result.success ? 'success' : 'error');
    } catch (error) {
      toast(error instanceof ApiError ? error.message : said.checkFailed, 'error');
    } finally {
      setBusy(null);
    }
  }

  /** Makes the version just saved the one the address shows, so a reload opens it. */
  function follow(version: number) {
    setLoaded(version);

    if (Number(params.get('version')) !== version) {
      setParams({ version: String(version) }, { replace: true });
    }
  }

  /**
   * Stores what is in the editor - over the newest version, or as a new one
   * with the note - and puts it online if asked to.
   */
  async function commit(asNew: boolean, thenDeploy: boolean) {
    setSaving(null);
    setBusy(thenDeploy ? 'deploy' : 'save');

    try {
      let target = loaded ?? undefined;

      if (dirty || asNew) {
        const version = asNew || !newest
          ? await api.save(privateKey, files, change.trim() || undefined)
          : await api.update(privateKey, loaded!, files);

        setSaved(JSON.stringify(files));
        setChange('');
        follow(version.version);

        target = version.version;

        if (!thenDeploy) {
          await control.refresh();

          toast(asNew || !newest
            ? said.saved(version.version)
            : version.version === lambda.activeVersion
              ? said.savedOverOnline(version.version)
              : said.savedOver(version.version));

          return;
        }
      }

      const result = await api.deploy(privateKey, target);

      setDiagnostics(result.diagnostics);
      setBuilt(result.success ? 'clean' : 'idle');

      await control.refresh();

      toast(result.success ? said.isOnline(result.lambda?.activeVersion ?? target) : said.notOnline,
            result.success ? 'success' : 'error');
    } catch (error) {
      toast(error instanceof ApiError ? error.message : said.failed, 'error');
    } finally {
      setBusy(null);
    }
  }

  /** Saving: over the newest version at once, or - for one that is history - as a new version. */
  const save = useCallback(() => {
    if (busy) {
      return;
    }

    if (!dirty) {
      toast(said.unchanged);
      return;
    }

    if (newest) {
      commit(false, false);
    } else {
      setSaving('save');
    }
    // commit reads the state of this render, which is what it should save
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [busy, dirty, newest, toast, said, files]);

  function deploy() {
    if (dirty && !newest) {
      setSaving('deploy');
    } else {
      commit(false, true);
    }
  }

  const online = loaded != null && loaded === lambda.activeVersion;
  // a demo is there to be read: its files open, nothing in them changes
  const demo = isDemo(lambda.tier);
  const showDiagnostics = diagnostics.length > 0 || built === 'clean';

  // nothing to put online: what is open is exactly what is online already
  const settled = !dirty && online && !lambda.activeChanged;

  return (
    <Section
      flush
      title={
        <>
          {said.title}
          <span className="ml-2 text-sm font-normal text-slate-500">
            {loaded != null ? said.version(loaded) : ''}
            {loaded != null ? (newest ? said.newestTag : said.older) : ''}
            {dirty ? said.edited : online ? (lambda.activeChanged ? said.unpublished : said.online) : ''}
          </span>
        </>
      }
      hint={
        <>
          {demo ? said.demo : said.edit}
          {!demo && loaded != null && !newest && lambda.latestVersion != null && said.history(loaded, lambda.latestVersion)}
          {said.files(<code className="font-mono">lambda.cs</code>, <code className="font-mono">.cs</code>)}
        </>
      }
      actions={
        <>
          <button type="button" onClick={check} disabled={busy !== null} className="btn-ghost !px-3 !py-1.5 text-[13px]">
            {busy === 'check' && <IconSpinner />}
            {said.check}
          </button>
          {!demo && (
            <>
              {loaded != null && (
                <button
                  type="button"
                  onClick={() => setSaving('save')}
                  disabled={busy !== null}
                  className="btn-ghost !px-3 !py-1.5 text-[13px]"
                  title={said.saveNewTitle}
                >
                  {said.saveNew}
                </button>
              )}
              {(newest || loaded == null) && (
                <button
                  type="button"
                  onClick={save}
                  disabled={busy !== null || !dirty}
                  className="btn-ghost !px-3 !py-1.5 text-[13px]"
                  title={loaded != null ? said.saveTitle(loaded) : 'Ctrl+S'}
                >
                  {busy === 'save' && <IconSpinner />}
                  {said.save}
                </button>
              )}
              <button type="button" onClick={deploy} disabled={busy !== null || settled} className="btn-primary !px-4 !py-1.5 text-[13px]">
                {busy === 'deploy' ? <IconSpinner /> : <IconPlay className="h-3.5 w-3.5" />}
                {said.deploy}
              </button>
            </>
          )}
        </>
      }
      pills={
        <FileTabs
          files={files}
          active={active}
          onSelect={setActive}
          onChange={demo ? undefined : setFiles}
          faulty={new Set(diagnostics.filter((d) => d.file).map((d) => d.file!))}
        />
      }
    >
      <div className="relative mx-4 min-h-[18rem] flex-1 border border-slate-200 dark:border-ink-800 md:mx-0">
        {/* one editor for every file, so switching swaps what it shows
            rather than building it again - which is what made it jump */}
        <CodeEditor
          path={current?.encoding === 'base64' ? '\u0000binary' : active}
          value={current?.encoding === 'base64' ? '' : code}
          language={languageFor(active)}
          theme={control.theme}
          diagnostics={diagnostics.filter((d) => (d.file ?? ENTRY) === active)}
          reveal={reveal}
          onChange={current?.encoding === 'base64' || demo ? undefined : setCode}
          onSave={save}
          readOnly={demo}
          onDefinition={goToDefinition}
        />

        {current?.encoding === 'base64' && (
          <div className="absolute inset-0 flex flex-col items-center justify-center gap-2 bg-white px-6 text-center dark:bg-ink-900">
            <p className="font-mono text-sm">{active}</p>
            <p className="text-sm text-slate-500">{said.binary(Math.round((code.length * 3) / 4 / 1024) || 1)}</p>
          </div>
        )}
      </div>

      {showDiagnostics && (
        <div className="mx-4 max-h-52 shrink-0 overflow-y-auto border-x border-b border-slate-200 dark:border-ink-800 md:mx-0">
          <Diagnostics
            diagnostics={diagnostics}
            state={built}
            onSelect={(diagnostic) => {
              if (diagnostic.file && diagnostic.file !== active) {
                setActive(diagnostic.file);
              }

              setReveal({ line: diagnostic.line, column: diagnostic.column, nonce: Date.now() });
            }}
          />
        </div>
      )}

      <Dialog
        title={saving === 'deploy' ? said.saveNewAndDeploy : said.saveVersion}
        open={saving !== null}
        onClose={() => setSaving(null)}
        footer={
          <>
            <button type="button" onClick={() => setSaving(null)} className="btn-ghost">
              {said.cancel}
            </button>
            <button type="button" onClick={() => commit(true, saving === 'deploy')} className="btn-primary">
              {saving === 'deploy' ? said.saveNewAndDeploy : said.saveNew}
            </button>
          </>
        }
      >
        <label className="block text-sm">
          <span className="text-slate-600 dark:text-slate-400">{said.what}</span>
          <input
            autoFocus
            value={change}
            onChange={(event) => setChange(event.target.value)}
            onKeyDown={(event) => event.key === 'Enter' && commit(true, saving === 'deploy')}
            maxLength={500}
            placeholder={said.placeholder}
            className="field mt-2"
          />
        </label>
      </Dialog>
    </Section>
  );
}
