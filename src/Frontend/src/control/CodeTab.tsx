import { useCallback, useEffect, useMemo, useRef, useState, type ReactNode } from 'react';
import { useSearchParams } from 'react-router-dom';

import { ApiError, api, isDemo, type Diagnostic, type LambdaFile } from '../api';
import { decode, download, encodeBytes, readable } from '../bytes';
import { CodeEditor } from '../components/CodeEditor';
import { Diagnostics } from '../components/Diagnostics';
import { Dialog } from '../components/Dialog';
import { IconAlert, IconChevronDown, IconDownload, IconLayers, IconPlay, IconPlus, IconSpinner, IconTrash, IconUpload } from '../components/Icons';
import { useToast } from '../components/Toast';
import { useEditorT } from '../i18n';
import type { EditorMessages } from '../locales/en/editor';
import { languageFor } from '../monaco';
import { CloneMenu } from './CloneMenu';
import type { Control } from './context';
import { GroupList, Tree, sizeOf } from './FileBrowser';
import { bytes, servesResources } from './format';
import { Exposure } from './SummaryTab';
import { Section, pill } from './ui';
import { RESOURCES, isCompiled, isResource } from './written';

type Busy = 'save' | 'check' | 'deploy' | null;

type Said = EditorMessages['tabs'];

/** The file the snippet lives in, which cannot be renamed or removed. */
const ENTRY = 'lambda.cs';

/** Where a new file or an upload goes: the code, or the resources. */
type Group = 'code' | 'resources';

/**
 * The files of a version - or of a draft - and the place to change them by
 * hand.
 *
 * A version is its code and its resources, and both are any number of files
 * in any folders, so they are a tree rather than a strip of tabs: the code
 * first - the C# at the top compiled, everything else kept with it - and the
 * resources the code reads and serves after it, each with what it comes to
 * and whether anybody on the internet can reach it. The file picked is open
 * beside them, to be read or changed; checking, saving and deploying are the
 * section's actions. Saving asks what changed - the same note an agent leaves
 * - so a version written by hand reads as well in the history as one that was
 * not.
 *
 * The newest version is what is open, since that is what a change starts
 * from; an older one is picked from the pill under the title, and saving it
 * makes it the newest. Opened on a draft, it edits the draft instead: saving
 * replaces what the draft holds and puts it online at its preview address in
 * the same step, so there is one button rather than two that only make sense
 * to somebody who knows the difference.
 */
export function CodeTab({ control, onDirty }: { control: Control; onDirty: (dirty: boolean) => void }) {
  const { privateKey, lambda, versions, summary } = control;
  const feature = control.feature?.info ?? null;

  const t = useEditorT();
  const said = t.code;
  const toast = useToast();
  const [params, setParams] = useSearchParams();

  // what an agent works on is the newest, so that is where reading starts,
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
  const text = current?.code ?? '';
  const dirty = saved !== '' && JSON.stringify(files) !== saved;

  useEffect(() => {
    onDirty(dirty);
  }, [dirty, onDirty]);

  const setCode = useCallback((next: string) => {
    setFiles((all) => all.map((file) => (file.name === active ? { ...file, code: next } : file)));
  }, [active]);

  // a file asked for by name - from the documentation or the tests - is where reading starts
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
      if (!isCompiled(active)) {
        return;
      }

      try {
        const at = await api.definition(privateKey, files.filter((file) => isCompiled(file.name)), active, line, column);

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
      // the whole version, since what is checked is whether it could be
      // saved and deployed: its size, its names and what it serves as well
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

        // what is open is the version just saved, from now on
        if (params.get('version')) {
          setParams((was) => {
            const next = new URLSearchParams(was);
            next.delete('version');
            return next;
          }, { replace: true });
        }
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
        setDiagnostics(result.preview.diagnostics);
        setBuilt(result.preview.success ? 'clean' : 'idle');

        toast(result.preview.success ? said.previewOnline : said.previewRefused, result.preview.success ? 'success' : 'error');
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

  function pickVersion(version: number) {
    if (dirty && !window.confirm(said.switchUnsaved)) {
      return;
    }

    setParams({ version: String(version) }, { replace: true });
  }

  const online = loaded != null && loaded === lambda.activeVersion;
  // a demo is there to be read: its files open, nothing in them changes
  const demo = isDemo(lambda.tier);
  const editable = !demo;
  const newer = !feature && lambda.latestVersion != null && loaded != null && lambda.latestVersion > loaded && !dirty;
  const showDiagnostics = diagnostics.length > 0 || built === 'clean';

  const code = files.filter((file) => !isResource(file.name));
  const resources = files.filter((file) => isResource(file.name));

  const source = code.filter((file) => isCompiled(file.name)).map((file) => file.code).join('\n');
  const serves = servesResources(source);

  const total = files.reduce((sum, file) => sum + sizeOf(file), 0);
  const allowance = summary?.limits.buildBytes;

  const faulty = new Set(diagnostics.filter((d) => d.file).map((d) => d.file!));

  const binary = current?.encoding === 'base64';
  const image = binary ? imageType(active) : null;

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
          {demo && <>{said.demo} </>}
          {feature ? said.hintFeature((text) => <b>{text}</b>) : said.hint((text) => <b>{text}</b>)}
          {newer && <> {said.newer(lambda.latestVersion!)}</>}
        </>
      }
      actions={
        <>
          <CloneMenu lambda={lambda} feature={feature} />
          <button type="button" onClick={check} disabled={busy !== null} className="btn-ghost !px-3 !py-1.5 text-[13px]">
            {busy === 'check' && <IconSpinner />}
            {said.check}
          </button>
          {editable && feature && (
            <button type="button" onClick={save} disabled={busy !== null || !dirty} className="btn-primary !px-4 !py-1.5 text-[13px]" title={said.deployPreviewTitle}>
              {busy === 'deploy' && <IconSpinner />}
              {said.save}
            </button>
          )}
          {editable && !feature && (
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
        !feature && versions.length > 0 && loaded != null && (
          <label className={`${pill(true)} relative cursor-pointer pr-7`}>
            <span className="sr-only">{said.versionLabel}</span>
            <span>{said.shown(loaded, loaded === lambda.activeVersion, loaded === lambda.latestVersion)}</span>
            <IconChevronDown className="pointer-events-none absolute right-2.5 h-3.5 w-3.5" />
            <select
              value={loaded}
              onChange={(event) => pickVersion(Number(event.target.value))}
              className="absolute inset-0 cursor-pointer opacity-0"
            >
              {versions.map((v) => (
                <option key={v.version} value={v.version}>
                  {v.version}
                  {v.version === lambda.activeVersion ? said.optionOnline : ''}
                  {v.change ? ` - ${v.change.slice(0, 60)}` : ''}
                </option>
              ))}
            </select>
          </label>
        )
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

      {!feature && requested == null ? (
        <p className="mx-4 text-sm text-slate-500 md:mx-0">{said.noVersion}</p>
      ) : (
        <div className="mx-4 flex min-h-0 flex-1 flex-col gap-4 md:mx-0 lg:flex-row">
          <nav aria-label={said.label} className="flex shrink-0 flex-col gap-5 lg:w-72 lg:overflow-y-auto">
            <FileGroup
              group="code"
              title={said.codeGroup}
              exposure={<Exposure open={false} why={said.codeWhy} />}
              files={code}
              active={active}
              faulty={faulty}
              editable={editable}
              onSelect={setActive}
              onChange={setFiles}
              all={files}
            />

            <FileGroup
              group="resources"
              title={said.resources}
              exposure={<Exposure open={serves} why={serves ? said.resourcesPublic : said.resourcesPrivate} />}
              files={resources}
              active={active}
              faulty={faulty}
              editable={editable}
              onSelect={setActive}
              onChange={setFiles}
              all={files}
            />

            <div className="space-y-2 border-t border-slate-200 pt-3 text-[12.5px] leading-relaxed text-slate-500 dark:border-ink-800">
              {allowance != null && <p>{said.usage(bytes(total), bytes(allowance))}</p>}
              <p className="flex items-start gap-2">
                <IconLayers className="mt-0.5 h-3.5 w-3.5 shrink-0 text-slate-400" />
                <span>
                  {said.scope((text) => (
                    <button type="button" onClick={() => control.openData()} className="text-accent-500 hover:underline">
                      {text}
                    </button>
                  ))}
                </span>
              </p>
            </div>
          </nav>

          <div className="flex min-h-[24rem] min-w-0 flex-1 flex-col">
            <div className="surface flex min-h-0 flex-1 flex-col">
              <header className="flex items-center gap-3 border-b border-slate-200 px-3 py-2 dark:border-ink-800">
                <span className="min-w-0 truncate font-mono text-[13px]" title={active}>{active}</span>
                {current && <span className="text-xs text-slate-400">{bytes(sizeOf(current))}</span>}
                {current && (
                  <button
                    type="button"
                    onClick={() => download(active.split('/').pop()!, binary ? new Uint8Array(decode(current.code)) : new TextEncoder().encode(current.code))}
                    className="ml-auto rounded-full p-1 text-slate-400 hover:bg-accent-500/10 hover:text-accent-500"
                    title={said.download}
                    aria-label={said.download}
                  >
                    <IconDownload className="h-3.5 w-3.5" />
                  </button>
                )}
              </header>

              <div className="relative min-h-0 flex-1">
                {/* one editor for every file, so switching swaps what it shows
                    rather than building it again - which is what made it jump */}
                <CodeEditor
                  path={binary ? '\u0000binary' : active}
                  value={binary ? '' : text}
                  language={languageFor(active)}
                  theme={control.theme}
                  diagnostics={diagnostics.filter((d) => (d.file ?? ENTRY) === active)}
                  reveal={reveal}
                  onChange={binary || !editable ? undefined : setCode}
                  onSave={save}
                  readOnly={!editable}
                  onDefinition={goToDefinition}
                />

                {binary && (
                  <div className="absolute inset-0 flex flex-col items-center justify-center gap-3 bg-white px-6 text-center dark:bg-ink-900">
                    {image ? (
                      <div className="flex max-h-full items-center justify-center bg-[repeating-conic-gradient(#8881_0%_25%,transparent_0%_50%)] bg-[length:16px_16px] p-6">
                        <img src={`data:${image};base64,${current.code}`} alt={active} className="max-h-[24rem] max-w-full" />
                      </div>
                    ) : (
                      <p className="text-sm text-slate-500">{said.binary(bytes(sizeOf(current)))}</p>
                    )}
                  </div>
                )}
              </div>
            </div>

            {showDiagnostics && (
              <div className="max-h-52 shrink-0 overflow-y-auto border-x border-b border-slate-200 dark:border-ink-800">
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
          </div>
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

/**
 * One group of the files - the code, or the resources - as a tree, with
 * adding, uploading and removing files where they may be changed.
 *
 * The resources are named in the tree as they are below resources/, which
 * the group is: a front end in resources/web/ shows as web/.
 */
function FileGroup({ group, title, exposure, files, all, active, faulty, editable, onSelect, onChange }: {
  group: Group;
  title: string;
  exposure: ReactNode;
  files: LambdaFile[];
  /** Every file of the version, which a new one may not share a name with. */
  all: LambdaFile[];
  active: string;
  faulty: Set<string>;
  editable: boolean;
  onSelect: (name: string) => void;
  onChange: (files: LambdaFile[]) => void;
}) {
  const t = useEditorT();
  const said = t.tabs;
  const words = t.code;

  const prefix = group === 'resources' ? RESOURCES : '';

  const [adding, setAdding] = useState(false);
  const [name, setName] = useState('');
  const [problem, setProblem] = useState<string | null>(null);
  const picker = useRef<HTMLInputElement>(null);

  const entries = useMemo(() => files.map((file) => ({ path: file.name.slice(prefix.length), size: sizeOf(file) })), [files, prefix]);

  const marked = useMemo(() => new Set([...faulty].filter((name) => name.startsWith(prefix)).map((name) => name.slice(prefix.length))), [faulty, prefix]);

  const selected = active.startsWith(prefix) && (group === 'code' ? !isResource(active) : true) ? active.slice(prefix.length) : null;

  function add(event: React.FormEvent) {
    event.preventDefault();

    const typed = name.trim().replace(/^\/+/, '');

    /*
     * A name of the code with no extension and no folder is taken to be C#,
     * which is what it almost always was. Anything else is the file it says
     * - and it may name a folder, as frontend/app.ts or web/index.html.
     */
    const wanted = prefix + (group === 'code' && !typed.includes('.') && !typed.includes('/') ? `${typed}.cs` : typed);

    const wrong = complaintOf(wanted, said);

    if (wrong) {
      setProblem(wrong);
      return;
    }

    if (all.some((file) => file.name.toLowerCase() === wanted.toLowerCase())) {
      setProblem(said.exists);
      return;
    }

    // a new file starts as a comment rather than empty, because an empty file
    // compiles and therefore says nothing about what it is for
    onChange([...all, { name: wanted, code: starterFor(wanted) }]);
    onSelect(wanted);

    setName('');
    setProblem(null);
    setAdding(false);
  }

  /*
   * Anything that is not text - an image, a font - cannot be typed into the
   * editor, so it comes in here. It lands beside the file that is open where
   * that is in this group and in a folder, which is where a picture for a
   * page goes, and at the top of the group otherwise.
   */
  async function upload(chosen: FileList | null) {
    if (!chosen) {
      return;
    }

    const folder = selected != null && selected.includes('/') ? selected.slice(0, selected.lastIndexOf('/') + 1) : '';

    const added: LambdaFile[] = [];

    for (const file of Array.from(chosen)) {
      const wanted = `${prefix}${folder}${file.name}`;

      if ([...all, ...added].some((one) => one.name.toLowerCase() === wanted.toLowerCase())) {
        setProblem(said.there(wanted));
        continue;
      }

      const wrong = complaintOf(wanted, said);

      if (wrong) {
        setProblem(`${wanted}: ${wrong}`);
        continue;
      }

      const content = new Uint8Array(await file.arrayBuffer());

      added.push(readable(content)
        ? { name: wanted, code: new TextDecoder().decode(content) }
        : { name: wanted, code: encodeBytes(content), encoding: 'base64' });
    }

    if (picker.current) {
      picker.current.value = '';
    }

    if (added.length > 0) {
      onChange([...all, ...added]);
      onSelect(added[0].name);
    }
  }

  function remove(path: string, folder: boolean) {
    const target = prefix + path;

    if (target === ENTRY) {
      return;
    }

    const gone = folder ? all.filter((file) => file.name.startsWith(`${target}/`)) : all.filter((file) => file.name === target);

    if (gone.length === 0 || !window.confirm(folder ? said.removeFolder(path, gone.length) : said.remove(path))) {
      return;
    }

    onChange(all.filter((file) => !gone.includes(file)));

    if (gone.some((file) => file.name === active)) {
      onSelect(ENTRY);
    }
  }

  const size = files.reduce((sum, file) => sum + sizeOf(file), 0);

  return (
    <GroupList
      title={title}
      exposure={exposure}
      usage={words.groupUsage(words.count(files.length), bytes(size))}
      action={
        editable && (
          <span className="flex items-center gap-0.5">
            <button
              type="button"
              onClick={() => { setAdding((was) => !was); setProblem(null); }}
              className="rounded-full p-1 text-slate-400 hover:bg-accent-500/10 hover:text-accent-500"
              title={words.newIn(title)}
              aria-label={words.newIn(title)}
            >
              <IconPlus className="h-3.5 w-3.5" />
            </button>
            <input ref={picker} type="file" multiple className="hidden" onChange={(event) => upload(event.target.files)} />
            <button
              type="button"
              onClick={() => picker.current?.click()}
              className="rounded-full p-1 text-slate-400 hover:bg-accent-500/10 hover:text-accent-500"
              title={words.uploadIn(title)}
              aria-label={words.uploadIn(title)}
            >
              <IconUpload className="h-3.5 w-3.5" />
            </button>
          </span>
        )
      }
    >
      {adding && (
        <form onSubmit={add} className="mb-2 px-1">
          <div className="flex items-center gap-1">
            {prefix && <span className="font-mono text-[12px] text-slate-400">{prefix}</span>}
            <input
              autoFocus
              value={name}
              onChange={(event) => setName(event.target.value)}
              onKeyDown={(event) => event.key === 'Escape' && setAdding(false)}
              placeholder={group === 'code' ? said.codePlaceholder : said.resourcePlaceholder}
              className="w-full rounded-full border border-slate-300 bg-white px-3 py-1 font-mono text-xs dark:border-ink-700 dark:bg-ink-900"
            />
          </div>
        </form>
      )}

      {problem && <p className="mb-2 px-1 text-xs text-red-500">{problem}</p>}

      <Tree
        entries={entries}
        selected={selected}
        onSelect={(path) => onSelect(prefix + path)}
        empty={group === 'resources' ? words.noResources : words.count(0)}
        marked={marked}
        markedLabel={said.errors}
        action={editable
          ? (node) => (prefix + node.path === ENTRY ? null : (
              <button
                type="button"
                onClick={() => remove(node.path, node.folder)}
                className="rounded-full p-0.5 text-slate-400 hover:text-red-500"
                aria-label={said.removeFile(node.path)}
                title={said.removeTitle}
              >
                <IconTrash className="h-3 w-3" />
              </button>
            ))
          : undefined}
      />
    </GroupList>
  );
}

/**
 * What is wrong with the name of a new file, if anything - the rules the
 * server applies, so a name is refused where it is typed.
 */
function complaintOf(name: string, said: Said): string | null {
  if (isResource(name)) {
    const rest = name.slice(RESOURCES.length);
    const parts = rest.split('/');

    const usable = rest.length > 0 && rest.length <= 120 && !rest.endsWith('/') && parts.length <= 6
      && parts.every((part) => part.length > 0 && part.length <= 60 && !part.startsWith('.') && /^[A-Za-z0-9._-]+$/.test(part))
      && /\.[A-Za-z0-9]+$/.test(rest);

    return usable ? null : said.resourceName;
  }

  if (isCompiled(name)) {
    return name.length <= 40 && /^[A-Za-z][A-Za-z0-9_-]*\.cs$/.test(name) ? null : said.codeName;
  }

  const parts = name.split('/');
  const top = parts[0].toLowerCase();

  if (top === '.lambda') {
    return said.lambda;
  }

  if (parts.length > 1 && top === 'assets') {
    return said.assets;
  }

  if ((parts.length === 1 && (TAKEN.includes(top) || top.endsWith('.csproj'))) || (parts.length > 1 && FOLDERS.includes(top))) {
    return said.taken;
  }

  const usable = !name.endsWith('/') && name.length <= 240 && parts.length <= 16
    && parts.every((part) => part.length > 0 && part.length <= 100 && part !== '.' && part !== '..' && !part.endsWith('.')
                             && part.toLowerCase() !== '.git' && /^[A-Za-z0-9\-_.+@()[\]{}$~]+$/.test(part));

  return usable ? null : said.name;
}

/** The names at the top that an exported or cloned project has for its own. */
const TAKEN = ['dockerfile', '.gitignore', '.dockerignore', 'license', 'agents.md', 'claude.md'];

/** The folders at the top that an exported or cloned project has for its own - and the resources, in lowercase. */
const FOLDERS = ['platform', 'bin', 'obj', 'workspace', 'database', 'resources'];

/**
 * What a new file starts as. Never empty: an empty file is valid and says
 * nothing about what it is for.
 */
function starterFor(name: string): string {
  const file = name.slice(name.lastIndexOf('/') + 1);

  if (name.endsWith('.md')) {
    const stem = file.slice(0, -3);

    return `# ${stem.charAt(0).toUpperCase()}${stem.slice(1)}\n`;
  }

  if (isCompiled(name)) {
    return `// Types for ${file.slice(0, -3)}.\n// Everything here is compiled beside the snippet and needs no using.\n`;
  }

  if (isResource(name) && (name.endsWith('.html') || name.endsWith('.htm'))) {
    const within = name.slice(RESOURCES.length);
    const folder = within.includes('/') ? within.slice(0, within.lastIndexOf('/')) : null;

    return [
      '<!doctype html>',
      '<html lang="en">',
      '<meta charset="utf-8">',
      '<meta name="viewport" content="width=device-width,initial-scale=1">',
      '<title>My app</title>',
      '',
      '<h1>It is served</h1>',
      '',
      folder
        ? `<!-- Serve this folder from lambda.cs with:\n     return Layout.Create().Add(Resources.App("${folder}")); -->`
        : '<!-- Serve these files from lambda.cs with:\n     return Layout.Create().Add(Resources.App()); -->',
      '',
    ].join('\n');
  }

  if (name.endsWith('.css')) {
    return 'body {\n  font: 16px/1.5 system-ui, sans-serif;\n  margin: 3rem auto;\n  max-width: 40rem;\n}\n';
  }

  return '';
}

function imageType(name: string): string | null {
  const extension = name.split('.').pop()?.toLowerCase();

  return (
    {
      png: 'image/png',
      jpg: 'image/jpeg',
      jpeg: 'image/jpeg',
      gif: 'image/gif',
      webp: 'image/webp',
      svg: 'image/svg+xml',
      ico: 'image/x-icon',
      avif: 'image/avif',
    }[extension ?? ''] ?? null
  );
}
