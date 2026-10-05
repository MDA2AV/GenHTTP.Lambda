import { useCallback, useEffect, useState } from 'react';
import { useSearchParams } from 'react-router-dom';

import { ApiError, api, isDemo, type Diagnostic, type LambdaFile } from '../api';
import { CodeEditor } from '../components/CodeEditor';
import { Diagnostics } from '../components/Diagnostics';
import { Dialog } from '../components/Dialog';
import { ENTRY, FileTabs } from '../components/FileTabs';
import { IconAlert, IconPlay, IconSpinner } from '../components/Icons';
import { useToast } from '../components/Toast';
import { useEditorT } from '../i18n';
import { languageFor } from '../monaco';
import { CloneMenu } from './CloneMenu';
import type { Control } from './context';
import { Section } from './ui';
import { CONTEXT, isCode } from './written';

type Busy = 'save' | 'check' | 'deploy' | null;

/**
 * Writing the code by hand.
 *
 * Built like every other section: the files are its views, so they are the
 * pills under the title, and checking, saving and deploying are its actions.
 * Saving asks what changed - the same note an agent leaves - so a version
 * written by hand reads as well in the history as one that was not.
 *
 * Opened on a feature - a draft, to the owner - it edits the feature
 * instead: saving replaces what the feature holds, without a note - the
 * feature says what it changes as a whole, on its overview - and puts it
 * online at its preview address in the same step, so there is one button
 * rather than two that only make sense to somebody who knows the difference.
 * Nothing a visitor of the lambda gets changes either way.
 *
 * The documentation and the tests of the version are its files too, and are
 * here after the program's, so a change and what it means for them can be
 * made and saved in one. Reading them is what their own sections are for.
 */
export function Workbench({ control, onDirty }: { control: Control; onDirty: (dirty: boolean) => void }) {
  const { privateKey, lambda } = control;
  const feature = control.feature?.info ?? null;

  const said = useEditorT().code;
  const toast = useToast();
  const [params] = useSearchParams();

  // what an agent works on is the newest, so that is where editing starts,
  // unless a particular version was asked for
  const requested = feature ? null : Number(params.get('version')) || lambda.latestVersion || lambda.activeVersion;

  const [loaded, setLoaded] = useState<number | null>(null);
  const [files, setFiles] = useState<LambdaFile[]>([{ name: ENTRY, code: '' }]);
  const [saved, setSaved] = useState('');
  const [active, setActive] = useState(ENTRY);
  const [diagnostics, setDiagnostics] = useState<Diagnostic[]>([]);
  const [built, setBuilt] = useState<'idle' | 'clean'>('idle');
  const [busy, setBusy] = useState<Busy>(null);
  const [reveal, setReveal] = useState<{ line: number; column: number; nonce: number }>();

  /** Whether the save dialog is open, and whether it deploys afterwards. */
  const [saving, setSaving] = useState<'save' | 'deploy' | null>(null);
  const [change, setChange] = useState('');

  /** Which save of the feature is open here, as this page knows it - to notice a save made elsewhere. */
  const [held, setHeld] = useState<number | null>(null);

  const current = files.find((file) => file.name === active) ?? files[0];
  const code = current?.code ?? '';
  const dirty = saved !== '' && JSON.stringify(files) !== saved;

  useEffect(() => {
    onDirty(dirty);
  }, [dirty, onDirty]);

  const setCode = useCallback((next: string) => {
    setFiles((all) => all.map((file) => (file.name === active ? { ...file, code: next } : file)));
  }, [active]);

  // a file asked for by name - from the documentation or the tests - is where editing starts
  const asked = params.get('file');

  const adopt = useCallback((incoming: LambdaFile[]) => {
    const usable = incoming.length > 0 ? incoming : [{ name: ENTRY, code: '' }];

    setFiles(usable);
    setSaved(JSON.stringify(usable));
    setActive((was) => (asked && usable.some((file) => file.name === asked)
      ? asked
      : usable.some((file) => file.name === was) ? was : usable[0].name));
  }, [asked]);

  useEffect(() => {
    if (feature || requested == null) {
      if (!feature) {
        setSaved(JSON.stringify(files));
      }
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

  const readFeature = useCallback(async (key: string) => {
    try {
      const content = await api.feature.get(privateKey, key);

      adopt(content.files);
      setHeld(content.feature.revision);
      setDiagnostics([]);
      setBuilt('idle');
    } catch (error) {
      toast(error instanceof ApiError ? error.message : said.featureLoadFailed, 'error');
    }
  }, [privateKey, adopt, toast, said]);

  // a feature is read when it is opened
  useEffect(() => {
    if (feature) {
      readFeature(feature.key);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [feature?.key, readFeature]);

  /*
   * Saved elsewhere since it was read here - by the agent, most likely, or in
   * another tab. Read again while nothing here is unsaved; otherwise said,
   * since saving would replace what was saved there.
   */
  const elsewhere = feature != null && held != null && feature.revision !== held;

  useEffect(() => {
    if (elsewhere && !dirty && busy === null) {
      readFeature(feature.key);
    }
  }, [elsewhere, dirty, busy, feature, readFeature]);

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
      if (!isCode(active)) {
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

  /** Stores what is in the editor, with the note, and puts it online if asked to. */
  async function commit(thenDeploy: boolean) {
    setSaving(null);
    setBusy(thenDeploy ? 'deploy' : 'save');

    try {
      let target = loaded ?? undefined;

      if (dirty) {
        const version = await api.save(privateKey, files, change.trim() || undefined);

        setSaved(JSON.stringify(files));
        setLoaded(version.version);
        setChange('');

        target = version.version;
      }

      if (!thenDeploy) {
        await control.refresh();
        toast(said.saved(target));
        return;
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

  /** Stores what is in the editor as the feature's, and puts its preview online with it. */
  async function commitFeature() {
    if (!feature || !dirty) {
      return;
    }

    setBusy('deploy');

    try {
      // made from the save read here, and refused if another came in between
      const result = await api.feature.save(privateKey, feature.key, files, true, held ?? undefined);

      setSaved(JSON.stringify(files));
      setHeld(result.feature.revision);

      if (result.preview) {
        report(result.preview.success, result.preview.diagnostics);
      } else {
        toast(said.featureSaved);
      }

      await control.feature?.refresh();
    } catch (error) {
      toast(error instanceof ApiError ? error.message : said.failed, 'error');
    } finally {
      setBusy(null);
    }
  }

  function report(success: boolean, found: Diagnostic[]) {
    setDiagnostics(found);
    setBuilt(success ? 'clean' : 'idle');

    toast(success ? said.previewOnline : said.previewRefused, success ? 'success' : 'error');
  }

  // made again with every render, so it always saves what is in the editor
  // now - the editor holds on to the newest one
  function save() {
    if (busy) {
      return;
    }

    if (!dirty) {
      toast(said.unchanged);
      return;
    }

    if (feature) {
      // saved into a draft is saved to be tried, so its preview shows it
      commitFeature();
    } else {
      setSaving('save');
    }
  }

  function deploy() {
    if (dirty) {
      setSaving('deploy');
    } else {
      commit(true);
    }
  }

  const online = loaded != null && loaded === lambda.activeVersion;
  // a demo is there to be read: its files open, nothing in them changes
  const demo = isDemo(lambda.tier);
  const newer = lambda.latestVersion != null && loaded != null && lambda.latestVersion > loaded && !dirty;
  const showDiagnostics = diagnostics.length > 0 || built === 'clean';

  return (
    <Section
      flush
      title={
        <>
          {said.title}
          <span className="ml-2 text-sm font-normal text-slate-500">
            {feature ? said.inFeature(feature.name) : loaded != null ? said.version(loaded) : ''}
            {dirty ? said.edited : !feature && online ? said.online : ''}
          </span>
        </>
      }
      hint={
        <>
          {feature ? said.editFeature : demo ? said.demo : said.edit}
          {said.files(<code className="font-mono">lambda.cs</code>, <code className="font-mono">.cs</code>, <code className="font-mono">{CONTEXT}</code>)}
          {newer && said.newer(lambda.latestVersion!)}
        </>
      }
      actions={
        <>
          <CloneMenu lambda={lambda} feature={feature} />
          <button type="button" onClick={check} disabled={busy !== null} className="btn-ghost !px-3 !py-1.5 text-[13px]">
            {busy === 'check' && <IconSpinner />}
            {said.check}
          </button>
          {!demo && feature && (
            <button type="button" onClick={save} disabled={busy !== null || !dirty} className="btn-primary !px-4 !py-1.5 text-[13px]" title={said.deployPreviewTitle}>
              {busy === 'deploy' && <IconSpinner />}
              {said.save}
            </button>
          )}
          {!demo && !feature && (
            <>
              <button type="button" onClick={save} disabled={busy !== null || !dirty} className="btn-ghost !px-3 !py-1.5 text-[13px]" title="Ctrl+S">
                {busy === 'save' && <IconSpinner />}
                {said.save}
              </button>
              <button
                type="button"
                onClick={deploy}
                disabled={busy !== null || (!dirty && online)}
                className="btn-primary !px-4 !py-1.5 text-[13px]"
              >
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
      {elsewhere && dirty && (
        <p className="mx-4 mb-3 flex items-start gap-2 text-[13px] text-amber-700 dark:text-amber-400 md:mx-0">
          <IconAlert className="mt-0.5 h-4 w-4 shrink-0" />
          <span>
            {said.changedElsewhere}{' '}
            <button type="button" onClick={() => feature && readFeature(feature.key)} className="font-medium underline">
              {said.readAgain}
            </button>
          </span>
        </p>
      )}

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
        title={saving === 'deploy' ? said.saveAndDeploy : said.saveVersion}
        open={saving !== null}
        onClose={() => setSaving(null)}
        footer={
          <>
            <button type="button" onClick={() => setSaving(null)} className="btn-ghost">
              {said.cancel}
            </button>
            <button type="button" onClick={() => commit(saving === 'deploy')} className="btn-primary">
              {saving === 'deploy' ? said.saveAndDeploy : said.save}
            </button>
          </>
        }
      >
        {/* saving from an older version makes it the newest, without what came after it */}
        {loaded != null && lambda.latestVersion != null && loaded < lambda.latestVersion && (
          <p className="flex gap-2 text-[13px] text-amber-700 dark:text-amber-400">
            <IconAlert className="mt-0.5 h-4 w-4 shrink-0" />
            {said.fromOlder(loaded, lambda.latestVersion)}
          </p>
        )}

        <label className="block text-sm">
          <span className="text-slate-600 dark:text-slate-400">{said.what}</span>
          <input
            autoFocus
            value={change}
            onChange={(event) => setChange(event.target.value)}
            onKeyDown={(event) => event.key === 'Enter' && commit(saving === 'deploy')}
            maxLength={500}
            placeholder={said.placeholder}
            className="field mt-2"
          />
        </label>

        {/* a version is saved for good; a feature is where a change is tried */}
        {lambda.activeVersion != null && (
          <p className="text-[13px] text-slate-500">
            {said.featureInstead((text) => (
              <button
                type="button"
                className="text-accent-500 hover:underline"
                onClick={() => {
                  setSaving(null);
                  // what was typed here goes into the feature rather than being lost
                  control.startFeature(loaded ?? undefined, dirty ? files : undefined);
                }}
              >
                {text}
              </button>
            ))}
          </p>
        )}
      </Dialog>
    </Section>
  );
}
