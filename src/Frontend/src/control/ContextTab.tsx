import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react';
import { useSearchParams } from 'react-router-dom';

import { ApiError, api, isActive, isDemo, type LambdaFile } from '../api';
import { CodeEditor } from '../components/CodeEditor';
import { Dialog } from '../components/Dialog';
import { IconAlert, IconBeaker, IconBook, IconChevronDown, IconPencil, IconSpark, IconSpinner } from '../components/Icons';
import { Markdown } from '../components/Markdown';
import { useToast } from '../components/Toast';
import { useEditorT } from '../i18n';
import { ChangeList } from './Changes';
import type { Control } from './context';
import { Tree, Viewer, sizeOf } from './FileBrowser';
import { Section, pill } from './ui';
import { AREAS, DECISIONS, PRODUCT, TESTING, othersOf, pagesOf, titleOf, within, type Area } from './written';

/** A page being written: which file, what it was when writing began, and what it is now. */
interface Writing {
  name: string;
  original: string;
  text: string;
}

/**
 * What a version says about itself, beside its program: its documentation,
 * or its tests - one component for both, because both are the same thing on
 * disk (a folder of .lambda/ with markdown pages and files beside them) and
 * should behave the same way on the screen.
 *
 * Written by agents, read here: the pages are rendered to be read, each one
 * a pill under the title, with the files beside them - pictures, scripts,
 * test data - behind one more. Each version has its own, so the version is
 * picked like the files of one, and a page says when that version changed
 * it and can show how. Opened on a draft, it is the draft's, compared with
 * the version the draft began from.
 *
 * Changing a page is possible in the full view, on the newest version or in
 * a draft, and changes that page and nothing else: a version never changes,
 * so the change is saved as the next version - with the same note every save
 * asks for, and put online with it when the newest version was online, so a
 * change of the documentation does not leave the app looking out of date.
 *
 * The simple view shows the documentation only, and of it only what the app
 * is and why - its product page - with a way to have the agent correct it
 * rather than a way to edit it: the owner says, the agent writes.
 */
export function ContextTab({ control, area, onDirty }: {
  control: Control;
  area: Area;
  /** Says whether a page holds something unsaved, so leaving it can ask first. */
  onDirty: (dirty: boolean) => void;
}) {
  const t = useEditorT();
  const said = t.context;
  const words = area === 'docs' ? said.docs : said.tests;
  const toast = useToast();
  const [params, setParams] = useSearchParams();

  const { lambda, versions, simple } = control;
  const feature = control.feature?.info ?? null;
  const demo = isDemo(lambda.tier);
  const { folder } = AREAS[area];

  // the newest, where agents work and a change of a page is saved
  const wanted = simple || feature ? lambda.latestVersion : Number(params.get('version')) || lambda.latestVersion || versions[0]?.version;
  const newest = feature != null || wanted === lambda.latestVersion;

  // what this version is compared with to say what it changed: the version
  // before it, or what a draft began from
  const index = versions.findIndex((v) => v.version === wanted);
  const previous = feature ? feature.base : index >= 0 ? versions[index + 1]?.version : undefined;

  const [files, setFiles] = useState<LambdaFile[] | null>(null);
  const [before, setBefore] = useState<LambdaFile[] | null>(null);
  const [failure, setFailure] = useState<string | null>(null);

  const [writing, setWriting] = useState<Writing | null>(null);
  const [shown, setShown] = useState<'write' | 'preview'>('write');
  const [comparing, setComparing] = useState(false);

  const [asking, setAsking] = useState(false);
  const [note, setNote] = useState('');
  const [online, setOnline] = useState(true);
  const [clash, setClash] = useState<number | null>(null);
  const [busy, setBusy] = useState(false);

  const load = useCallback(async () => {
    try {
      // its own folder only: the development space beside it can be a project of hundreds of files
      const [mine, theirs] = await Promise.all([
        feature
          ? api.feature.get(control.privateKey, feature.key, folder).then((content) => content.files)
          : wanted != null
            ? api.version(control.privateKey, wanted, folder).then((content) => content.files)
            : Promise.resolve([] as LambdaFile[]),
        previous != null
          ? api.version(control.privateKey, previous, folder).then((content) => content.files).catch(() => null)
          : Promise.resolve(null),
      ]);

      setFiles(mine);
      setBefore(theirs);
      setFailure(null);
    } catch (error) {
      setFailure(error instanceof ApiError ? error.message : said.readFailed);
    }
  }, [control.privateKey, feature?.key, feature?.revision, wanted, previous, folder, said]);

  useEffect(() => {
    setFiles(null);
    load();
    // a save elsewhere - the agent's, most likely - is read again, while nothing here is unsaved
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [control.privateKey, feature?.key, wanted, previous]);

  useEffect(() => {
    if (feature && !writing) {
      load();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [feature?.revision]);

  const dirty = writing != null && writing.text !== writing.original;

  useEffect(() => {
    onDirty(dirty);
  }, [dirty, onDirty]);

  useEffect(() => {
    if (!dirty) {
      return;
    }

    const warn = (event: BeforeUnloadEvent) => event.preventDefault();

    window.addEventListener('beforeunload', warn);

    return () => window.removeEventListener('beforeunload', warn);
  }, [dirty]);

  const all = files ?? [];

  /*
   * The pages there are, and the ones every version is meant to have even
   * where this one does not: a missing page is a pill of its own, which is
   * how it is noticed. The simple view has the product page only.
   */
  const slots = useMemo(() => {
    const pages = pagesOf(all, area);
    const expected = simple ? [PRODUCT] : AREAS[area].pages;

    const names = [...expected, ...pages.map((p) => p.name).filter((name) => !expected.includes(name) && !simple)];

    return names.map((name) => ({ name, file: pages.find((p) => p.name === name) ?? null }));
  }, [all, area, simple]);

  const others = simple ? [] : othersOf(all, area);

  const chosenFile = params.get('file');
  const chosenPage = params.get('page');

  const browsing = !simple && chosenFile != null && others.length > 0;
  const current = slots.find((slot) => slot.name === `${folder}${chosenPage}`) ?? slots[0];
  const selectedFile = browsing ? (others.find((f) => f.name === `${folder}${chosenFile}`) ?? others[0]) : null;

  const agent = !demo && (control.agent.state?.available ?? false);
  const working = isActive(control.agent.state?.job);

  const editable = !simple && !demo;

  /** Goes somewhere else within the section, asking first when a page holds something unsaved. */
  function go(next: Record<string, string>) {
    if (dirty && !window.confirm(said.discard)) {
      return;
    }

    setWriting(null);
    setComparing(false);

    const query = new URLSearchParams();

    if (params.get('version') && !feature) {
      query.set('version', params.get('version')!);
    }

    Object.entries(next).forEach(([key, value]) => query.set(key, value));

    setParams(query, { replace: true });
  }

  function write(name: string, text: string) {
    setWriting({ name, original: text, text });
    setShown('write');
  }

  function stop() {
    if (dirty && !window.confirm(said.discard)) {
      return;
    }

    setWriting(null);
  }

  /** Saves a page: into the draft at once, or - after the note - as the next version. */
  async function save() {
    if (!writing || !dirty) {
      return;
    }

    if (feature) {
      setBusy(true);

      try {
        await api.feature.change(control.privateKey, feature.key, { files: [{ name: writing.name, code: writing.text }] });

        toast(said.savedDraft, 'success');
        setWriting(null);

        await control.feature?.refresh();
        await load();
      } catch (error) {
        toast(error instanceof ApiError ? error.message : said.saveFailed, 'error');
      } finally {
        setBusy(false);
      }

      return;
    }

    // a version saved meanwhile is saved on top of; only a change of this very page is in the way
    setClash(null);

    if (lambda.latestVersion != null && wanted != null && lambda.latestVersion !== wanted) {
      try {
        const latest = await api.version(control.privateKey, lambda.latestVersion, folder);
        const theirs = latest.files.find((f) => f.name === writing.name)?.code ?? '';

        if (theirs !== writing.original) {
          setClash(lambda.latestVersion);
        }
      } catch {
        // said nothing about, and saved on top of all the same
      }
    }

    setNote('');
    setOnline(lambda.activeVersion != null && lambda.activeVersion === lambda.latestVersion);
    setAsking(true);
  }

  async function commit() {
    if (!writing) {
      return;
    }

    setAsking(false);
    setBusy(true);

    try {
      const deploy = online && lambda.activeVersion != null;

      const saved = await api.changeVersion(control.privateKey, {
        files: [{ name: writing.name, code: writing.text }],
        change: note.trim() || null,
      }, deploy);

      setWriting(null);

      await control.refresh();

      if (deploy && saved.deployment && !saved.deployment.success) {
        toast(said.savedNotOnline(saved.version), 'error');
      } else {
        toast(deploy ? said.savedOnline(saved.version) : said.savedVersion(saved.version), 'success');
      }

      go(params.get('page') ? { page: params.get('page')!, version: String(saved.version) } : { version: String(saved.version) });
    } catch (error) {
      toast(error instanceof ApiError ? error.message : said.saveFailed, 'error');
    } finally {
      setBusy(false);
    }
  }

  const title = area === 'docs' ? (simple ? said.docs.titleSimple : said.docs.title) : said.tests.title;

  const label = (name: string, file: LambdaFile | null) =>
    name === PRODUCT ? said.docs.pages.product
      : name === DECISIONS ? said.docs.pages.decisions
        : name === TESTING ? said.tests.pages.testing
          : (file && titleOf(file)) || within(name, area);

  const changed = (name: string, file: LambdaFile | null) =>
    before != null && (before.find((f) => f.name === name)?.code ?? null) !== (file?.code ?? null);

  const empty = files != null && slots.every((slot) => !slot.file) && others.length === 0;

  return (
    <Section
      title={title}
      hint={simple ? said.docs.hintSimple : words.hint}
      actions={
        writing ? (
          <>
            <button type="button" onClick={stop} disabled={busy} className="btn-ghost !px-3 !py-1.5 text-[13px]">
              {said.cancel}
            </button>
            <button type="button" onClick={save} disabled={busy || !dirty} className="btn-primary !px-4 !py-1.5 text-[13px]" title="Ctrl+S">
              {busy && <IconSpinner />}
              {said.save}
            </button>
          </>
        ) : (
          editable && !browsing && current?.file && (
            <button
              type="button"
              onClick={() => write(current.name, current.file!.code)}
              disabled={!newest}
              title={newest ? undefined : said.olderVersion}
              className="btn-ghost !px-3 !py-1.5 text-[13px]"
            >
              <IconPencil className="h-3.5 w-3.5" />
              {said.edit}
            </button>
          )
        )
      }
      pills={!simple && (
        <>
          {/* nothing written at all is said once, below, rather than once per page */}
          {!empty && slots.map(({ name, file }) => (
            <button
              key={name}
              type="button"
              aria-pressed={!browsing && current?.name === name}
              onClick={() => go({ page: within(name, area) })}
              className={`${pill(!browsing && current?.name === name)} ${file ? '' : 'opacity-60'}`}
              title={file ? name : said.missingPill}
            >
              {label(name, file)}
              {!file && <span className="text-[11px] uppercase tracking-wide text-slate-400">{said.none}</span>}
              {file && changed(name, file) && (
                <span className="h-1.5 w-1.5 rounded-full bg-accent-500 dark:bg-accent-400" title={feature ? said.changedInDraft : said.changedIn(wanted ?? 0)} />
              )}
            </button>
          ))}

          {others.length > 0 && (
            <button
              type="button"
              aria-pressed={browsing}
              onClick={() => go({ file: within(others[0].name, area) })}
              className={pill(browsing)}
            >
              {said.files}
              <span className="tabular-nums text-slate-400">{others.length}</span>
            </button>
          )}

          {!feature && versions.length > 0 && wanted != null && (
            <label className={`${pill(false)} relative cursor-pointer pr-7 sm:ml-auto`}>
              <span className="sr-only">{t.files.version}</span>
              <span>{t.files.shown(wanted, wanted === lambda.activeVersion, wanted === lambda.latestVersion)}</span>
              <IconChevronDown className="pointer-events-none absolute right-2.5 h-3.5 w-3.5" />
              <select
                value={wanted}
                onChange={(event) => go({ ...(chosenPage ? { page: chosenPage } : {}), version: event.target.value })}
                className="absolute inset-0 cursor-pointer opacity-0"
              >
                {versions.map((v) => (
                  <option key={v.version} value={v.version}>
                    {v.version}
                    {v.version === lambda.activeVersion ? t.files.optionOnline : ''}
                    {v.change ? ` - ${v.change.slice(0, 60)}` : ''}
                  </option>
                ))}
              </select>
            </label>
          )}
        </>
      )}
    >
      {feature && <p className="-mt-1 mb-4 max-w-2xl text-[13px] text-slate-600 dark:text-slate-400">{words.inDraft}</p>}

      {failure ? (
        <p className="text-sm text-red-500">{failure}</p>
      ) : files === null ? (
        <div className="flex items-center gap-2 py-10 text-sm text-slate-500"><IconSpinner /> {said.reading}</div>
      ) : writing ? (
        <Writer control={control} files={all} writing={writing} shown={shown} onShown={setShown}
                onChange={(text) => setWriting({ ...writing, text })} onSave={save} />
      ) : empty ? (
        <Empty
          area={area}
          simple={simple}
          agent={agent && !working}
          editable={editable && newest}
          onAsk={() => control.askAgent(feature?.key, simple ? said.docs.describePrompt : words.writePrompt)}
          onWrite={() => write(area === 'docs' ? PRODUCT : TESTING, area === 'docs' ? said.skeleton.product : said.skeleton.testing)}
        />
      ) : browsing ? (
        <div className="grid gap-5 lg:grid-cols-[17rem,1fr]">
          <nav aria-label={said.files} className="lg:max-h-[40rem] lg:overflow-y-auto">
            <Tree
              entries={others.map((f) => ({ path: within(f.name, area), size: sizeOf(f) }))}
              selected={selectedFile ? within(selectedFile.name, area) : null}
              onSelect={(path) => go({ file: path })}
              empty={said.noFiles}
            />
            {editable && selectedFile && (
              <button type="button" onClick={() => control.edit(feature ? undefined : wanted ?? undefined, selectedFile.name)}
                      className="mt-4 px-1 text-[13px] text-accent-500 hover:underline">
                {said.editInCode}
              </button>
            )}
          </nav>
          <Viewer control={control} selection={selectedFile ? { group: 'context', path: selectedFile.name } : null} files={all} listing={null} />
        </div>
      ) : current?.file ? (
        <article>
          {/* the simple view does not name versions, and has the history for what changed */}
          {!simple && changed(current.name, current.file) && (
            <p className="-mt-1 mb-5 flex flex-wrap items-center gap-x-3 gap-y-1 text-[13px] text-slate-500">
              <span className="flex items-center gap-1.5">
                <span className="h-1.5 w-1.5 rounded-full bg-accent-500 dark:bg-accent-400" />
                {feature ? said.changedInDraft : said.changedIn(wanted ?? 0)}
              </span>
              {!simple && (
                <button type="button" onClick={() => setComparing((was) => !was)} className="text-accent-500 hover:underline" aria-expanded={comparing}>
                  {comparing ? said.hideChanges : said.showChanges}
                </button>
              )}
            </p>
          )}

          {comparing && !simple && (
            <div className="mb-8 max-w-4xl">
              <ChangeList
                before={(before ?? []).filter((f) => f.name === current.name)}
                after={[current.file]}
                theme={control.theme}
                empty={said.noChanges}
              />
            </div>
          )}

          <Markdown source={current.file.code} name={current.name} files={all} theme={control.theme}
                    onOpen={(name) => go(name.startsWith(folder) ? { page: within(name, area) } : {})} />

          {simple && area === 'docs' && (
            <Correction agent={agent && !working} onAsk={() => control.askAgent(undefined, said.docs.correctPrompt)} />
          )}
        </article>
      ) : current ? (
        <Missing
          name={current.name}
          agent={agent && !working}
          editable={editable && newest}
          onAsk={() => control.askAgent(feature?.key, missingPrompt(current.name))}
          onWrite={() => write(current.name, skeletonOf(current.name))}
        />
      ) : null}

      <Dialog
        title={said.saveTitle}
        open={asking}
        onClose={() => setAsking(false)}
        footer={
          <>
            <button type="button" onClick={() => setAsking(false)} className="btn-ghost">{said.cancel}</button>
            <button type="button" onClick={commit} className="btn-primary">{said.save}</button>
          </>
        }
      >
        <p className="text-slate-600 dark:text-slate-400">{said.saveText(lambda.latestVersion ?? 0)}</p>

        {clash != null && (
          <p className="flex gap-2 text-[13px] text-amber-700 dark:text-amber-400">
            <IconAlert className="mt-0.5 h-4 w-4 shrink-0" />
            {said.clash(clash)}
          </p>
        )}

        <label className="block text-sm">
          <span className="text-slate-600 dark:text-slate-400">{t.code.what}</span>
          <input
            autoFocus
            value={note}
            onChange={(event) => setNote(event.target.value)}
            onKeyDown={(event) => event.key === 'Enter' && commit()}
            maxLength={500}
            placeholder={words.placeholder}
            className="field mt-2"
          />
        </label>

        {lambda.activeVersion != null && (
          <label className="flex items-start gap-2.5 text-[13px]">
            <input type="checkbox" checked={online} onChange={(event) => setOnline(event.target.checked)} className="mt-0.5 accent-accent-500" />
            <span>
              <span className="text-slate-800 dark:text-slate-200">{said.alsoOnline}</span>
              <span className="block text-slate-500">{said.alsoOnlineNote}</span>
            </span>
          </label>
        )}
      </Dialog>
    </Section>
  );

  function missingPrompt(name: string): string {
    return name === DECISIONS ? said.docs.decisionsPrompt : name === TESTING ? said.tests.writePrompt : said.docs.writePrompt;
  }

  function skeletonOf(name: string): string {
    return name === PRODUCT ? said.skeleton.product : name === DECISIONS ? said.skeleton.decisions : name === TESTING ? said.skeleton.testing : '';
  }
}

/**
 * Writing a page: the markdown beside what it will look like on a wide
 * screen, and one or the other on a narrow one.
 */
function Writer({ control, files, writing, shown, onShown, onChange, onSave }: {
  control: Control;
  files: LambdaFile[];
  writing: Writing;
  shown: 'write' | 'preview';
  onShown: (shown: 'write' | 'preview') => void;
  onChange: (text: string) => void;
  onSave: () => void;
}) {
  const said = useEditorT().context;

  return (
    <div>
      <div className="mb-3 flex items-center gap-3 lg:hidden">
        <div role="radiogroup" aria-label={said.writeOrPreview} className="flex gap-1.5">
          {(['write', 'preview'] as const).map((view) => (
            <button key={view} type="button" role="radio" aria-checked={shown === view} onClick={() => onShown(view)} className={pill(shown === view)}>
              {view === 'write' ? said.write : said.preview}
            </button>
          ))}
        </div>
      </div>

      <p className="mb-2 font-mono text-xs text-slate-500">{writing.name}</p>

      <div className="grid gap-5 lg:grid-cols-2">
        <div className={`h-[36rem] min-h-0 border border-slate-200 dark:border-ink-800 ${shown === 'write' ? '' : 'hidden lg:block'}`}>
          <CodeEditor path={`context:${writing.name}`} value={writing.text} language="markdown" theme={control.theme} diagnostics={[]}
                      onChange={onChange} onSave={onSave} wrap />
        </div>
        <div className={`h-[36rem] overflow-y-auto border border-dashed border-slate-200 p-5 dark:border-ink-800 ${shown === 'preview' ? '' : 'hidden lg:block'}`}>
          <Markdown source={writing.text} name={writing.name} files={files} theme={control.theme} />
        </div>
      </div>
    </div>
  );
}

/** Nothing written at all in this part of the version. */
function Empty({ area, simple, agent, editable, onAsk, onWrite }: {
  area: Area;
  simple: boolean;
  agent: boolean;
  editable: boolean;
  onAsk: () => void;
  onWrite: () => void;
}) {
  const said = useEditorT().context;
  const words = area === 'docs' ? said.docs : said.tests;
  const Icon = area === 'docs' ? IconBook : IconBeaker;

  return (
    <div className="max-w-xl py-6">
      <div className="flex h-11 w-11 items-center justify-center bg-accent-500/10 text-accent-500 dark:text-accent-400">
        <Icon className="h-5 w-5" />
      </div>
      <h2 className="mt-5 text-base font-semibold">{simple ? said.docs.emptySimpleTitle : words.emptyTitle}</h2>
      <p className="mt-2 text-sm text-slate-600 dark:text-slate-400">
        {simple ? said.docs.emptySimple : words.emptyText((text) => <code className="font-mono text-[13px]">{text}</code>)}
      </p>
      <Buttons agent={agent} editable={editable && !simple} onAsk={onAsk} onWrite={onWrite}
               ask={simple ? said.docs.describe : words.ask} />
    </div>
  );
}

/** One page every version is meant to have, which this one does not. */
function Missing({ name, agent, editable, onAsk, onWrite }: {
  name: string;
  agent: boolean;
  editable: boolean;
  onAsk: () => void;
  onWrite: () => void;
}) {
  const said = useEditorT().context;

  const [title, text] = name === DECISIONS
    ? [said.docs.missingDecisions, said.docs.missingDecisionsText]
    : name === TESTING
      ? [said.tests.missing, said.tests.missingText]
      : [said.docs.missingProduct, said.docs.missingProductText];

  return (
    <div className="max-w-xl border-l-2 border-slate-300 py-1 pl-4 dark:border-ink-700">
      <h2 className="text-sm font-medium">{title}</h2>
      <p className="mt-1.5 text-[13px] text-slate-600 dark:text-slate-400">{text}</p>
      <Buttons agent={agent} editable={editable} onAsk={onAsk} onWrite={onWrite} ask={said.askPage} />
    </div>
  );
}

function Buttons({ agent, editable, ask, onAsk, onWrite }: {
  agent: boolean;
  editable: boolean;
  ask: ReactNode;
  onAsk: () => void;
  onWrite: () => void;
}) {
  const said = useEditorT().context;

  if (!agent && !editable) {
    return null;
  }

  return (
    <div className="mt-5 flex flex-wrap gap-2">
      {agent && (
        <button type="button" onClick={onAsk} className="btn-primary !px-4 !py-1.5 text-[13px]">
          <IconSpark className="h-3.5 w-3.5" />
          {ask}
        </button>
      )}
      {editable && (
        <button type="button" onClick={onWrite} className="btn-ghost !px-3 !py-1.5 text-[13px]">
          <IconPencil className="h-3.5 w-3.5" />
          {said.writeIt}
        </button>
      )}
    </div>
  );
}

/**
 * The simple view's way to change what is written about the app: telling
 * the agent, which keeps it up to date anyway, rather than editing it.
 */
function Correction({ agent, onAsk }: { agent: boolean; onAsk: () => void }) {
  const said = useEditorT().context.docs;

  return (
    <div className="mt-10 flex max-w-[46rem] flex-wrap items-center gap-x-4 gap-y-3 border-t border-slate-200 pt-5 dark:border-ink-800">
      <p className="min-w-0 flex-1 text-[13px] text-slate-500">{said.correctText}</p>
      {agent && (
        <button type="button" onClick={onAsk} className="btn-ghost !px-3 !py-1.5 text-[13px]">
          <IconSpark className="h-3.5 w-3.5" />
          {said.correct}
        </button>
      )}
    </div>
  );
}
