import { useEffect, useState } from 'react';

import { ApiError, api, type Diagnostic, type Feature, type FeatureMerge, type LambdaFile } from '../api';
import { Diagnostics } from '../components/Diagnostics';
import { Dialog } from '../components/Dialog';
import { IconAlert, IconSpinner } from '../components/Icons';
import { useEditorT } from '../i18n';
import type { Control } from './context';

/** How long a feature's name may be; the server says the same. */
const MAX_NAME = 80;

/**
 * Starting a feature - a draft, to the owner: what it is called and what it
 * should do. It starts from the newest version, unless the owner came from
 * an older one in the versions; which one is not asked, since starting from
 * anything else only makes a draft that cannot go online as it is.
 */
export function NewFeatureDialog({ control, open, base, files, onClose, onCreated }: {
  control: Control;
  open: boolean;
  /** The version to start from; the newest when left out. */
  base?: number;
  /** What to put into it instead of that version's files - what was typed in the code view. */
  files?: LambdaFile[];
  onClose: () => void;
  onCreated: (feature: Feature) => void;
}) {
  const said = useEditorT().features;
  const { versions, features } = control;

  const [name, setName] = useState('');
  const [specification, setSpecification] = useState('');
  const [working, setWorking] = useState(false);
  const [error, setError] = useState<string | null>(null);

  /** The feature, once it is made - so a save of the files that failed is tried again rather than a second feature made. */
  const [made, setMade] = useState<Feature | null>(null);

  // what was typed is kept while the page reads the lambda again underneath;
  // it starts over only when the dialog is opened again
  useEffect(() => {
    if (open) {
      setName('');
      setSpecification('');
      setError(null);
      setMade(null);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open]);

  const newest = versions[0]?.version;
  const from = base ?? newest;

  const limit = control.summary?.limits.features ?? Infinity;
  const full = features.length >= limit && made == null;
  const ready = (made != null || (name.trim() !== '' && !full && from != null)) && !working;

  async function create() {
    if (!ready) {
      return;
    }

    setWorking(true);
    setError(null);

    try {
      const feature = made ?? (await api.feature.create(control.privateKey, name.trim(), specification.trim() || undefined, from));

      setMade(feature);

      // what was typed goes in as the feature's first save
      const holding = files ? (await api.feature.save(control.privateKey, feature.key, files)).feature : feature;

      onCreated(holding);
    } catch (caught) {
      setError(caught instanceof ApiError ? caught.message : said.createFailed);
    } finally {
      setWorking(false);
    }
  }

  const close = () => {
    // made, but what was typed could not be put in: it is there all the same
    if (made) {
      control.refresh().catch(() => undefined);
    }

    onClose();
  };

  return (
    <Dialog
      title={said.newTitle}
      open={open}
      onClose={close}
      footer={
        <>
          <button type="button" onClick={close} className="btn-ghost">{said.cancel}</button>
          <button type="button" onClick={create} disabled={!ready} className="btn-primary">
            {working && <IconSpinner />}
            {made ? said.retry : said.create}
          </button>
        </>
      }
    >
      {full ? (
        <p className="flex gap-2 text-amber-700 dark:text-amber-400">
          <IconAlert className="mt-0.5 h-4 w-4 shrink-0" />
          {said.full(limit)}
        </p>
      ) : (
        <p className="text-slate-600 dark:text-slate-400">{files ? said.newTextFiles : said.newText}</p>
      )}

      {made && <p className="text-[13px] text-amber-700 dark:text-amber-400">{said.madeNotSaved(made.name)}</p>}

      <label className="block">
        <span className="text-slate-600 dark:text-slate-400">{said.name}</span>
        <input
          autoFocus
          disabled={made != null}
          value={name}
          onChange={(event) => setName(event.target.value)}
          onKeyDown={(event) => event.key === 'Enter' && create()}
          maxLength={MAX_NAME}
          placeholder={said.namePlaceholder}
          className="field mt-1.5"
        />
      </label>

      <label className="block">
        <span className="text-slate-600 dark:text-slate-400">{said.wanted}</span>
        <textarea
          value={specification}
          onChange={(event) => setSpecification(event.target.value)}
          rows={3}
          maxLength={4000}
          placeholder={said.wantedPlaceholder}
          className="field mt-1.5 resize-y"
        />
      </label>

      {from != null && newest != null && from !== newest && (
        <p className="flex gap-2 text-[13px] text-amber-700 dark:text-amber-400">
          <IconAlert className="mt-0.5 h-4 w-4 shrink-0" />
          {said.olderBase(newest)}
        </p>
      )}

      {error && <p className="text-xs text-red-500">{error}</p>}
    </Dialog>
  );
}

/**
 * Putting a feature online: it is merged into the next version, with a line
 * on what it changes, and that version is deployed - one step to the owner,
 * who wants the draft online rather than a version to deploy afterwards.
 * Only offered for a feature based on the newest version; one that is not is
 * told how to get there instead, since merging it would undo what was saved
 * after it began.
 */
export function MergeDialog({ control, feature, open, onClose, onMerged, onRebase }: {
  control: Control;
  feature: Feature;
  open: boolean;
  onClose: () => void;
  onMerged: (result: FeatureMerge) => void;
  /** Opens the dialog that moves its base, for a feature behind the newest version. */
  onRebase: () => void;
}) {
  const t = useEditorT();
  const said = t.features;
  const plain = t.simple;

  const [change, setChange] = useState('');
  const [working, setWorking] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [refused, setRefused] = useState<Diagnostic[] | null>(null);

  useEffect(() => {
    if (open) {
      setChange(feature.change ?? '');
      setError(null);
      setRefused(null);
    }
    // a feature changed while the dialog is open keeps what was typed
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open]);

  const next = (feature.newest ?? feature.base) + 1;
  const active = control.lambda.activeVersion;
  const agent = control.agent.state?.available ?? false;
  const { simple } = control;

  async function merge() {
    setWorking(true);
    setError(null);
    setRefused(null);

    try {
      const result = await api.feature.merge(control.privateKey, feature.key, { deploy: true, change: change.trim() || undefined });

      if (!result.merged) {
        setRefused(result.diagnostics);
        return;
      }

      onMerged(result);
    } catch (caught) {
      setError(caught instanceof ApiError ? caught.message : said.mergeFailed);

      // saved over since the page last looked: what it shows is stale
      await control.feature?.refresh();
    } finally {
      setWorking(false);
    }
  }

  if (!feature.mergeable) {
    return (
      <Dialog
        title={said.behindTitle}
        open={open}
        onClose={onClose}
        footer={
          <>
            <button type="button" onClick={onClose} className="btn-ghost">{said.close}</button>
            {agent ? (
              <button type="button" onClick={() => { onClose(); control.askAgent(feature.key, said.catchUp); }} className="btn-primary">
                {said.askAgent}
              </button>
            ) : (
              <button type="button" onClick={() => { onClose(); onRebase(); }} className="btn-primary">
                {said.moveBase}
              </button>
            )}
          </>
        }
      >
        <p className="text-slate-600 dark:text-slate-400">
          {simple ? plain.behindText : said.behindText(feature.base, feature.newest ?? feature.base)}
        </p>

        {/* for changes brought in by hand: said, but not offered as the way -
            and not at all to somebody who does not bring any in by hand */}
        {agent && !simple && (
          <button type="button" onClick={() => { onClose(); onRebase(); }} className="text-[13px] text-accent-500 hover:underline">
            {said.moveBase}
          </button>
        )}
      </Dialog>
    );
  }

  return (
    <Dialog
      title={said.mergeTitle(feature.name)}
      open={open}
      onClose={onClose}
      footer={
        <>
          <button type="button" onClick={onClose} className="btn-ghost">{said.cancel}</button>
          <button type="button" onClick={merge} disabled={working} className="btn-primary">
            {working && <IconSpinner />}
            {said.mergeAndDeploy(next)}
          </button>
        </>
      }
    >
      <p className="text-slate-600 dark:text-slate-400">
        {simple
          ? `${plain.mergeText} ${active != null ? plain.mergeUndo : said.deployTooOffline}`
          : `${said.mergeText(next)} ${active != null ? said.deployTooNote(active) : said.deployTooOffline}`}
      </p>

      <label className="block">
        <span className="text-slate-600 dark:text-slate-400">{said.what}</span>
        <input
          value={change}
          onChange={(event) => setChange(event.target.value)}
          maxLength={500}
          placeholder={said.whatPlaceholder}
          className="field mt-1.5"
        />
      </label>

      {refused && (
        <>
          <p className="flex gap-2 text-red-600 dark:text-red-400">
            <IconAlert className="mt-0.5 h-4 w-4 shrink-0" />
            {simple ? plain.refusedText : said.notCompiling}
          </p>
          {!simple && (
            <div className="max-h-48 overflow-y-auto border border-slate-200 dark:border-ink-800">
              <Diagnostics diagnostics={refused} state="idle" onSelect={() => undefined} />
            </div>
          )}
        </>
      )}

      {error && <p className="text-xs text-red-500">{error}</p>}
    </Dialog>
  );
}

/** What a feature is called, what it changes, and what was wanted - the notes the version it becomes keeps. */
export function NotesDialog({ control, feature, open, onClose }: {
  control: Control;
  feature: Feature;
  open: boolean;
  onClose: () => void;
}) {
  const said = useEditorT().features;

  const [name, setName] = useState(feature.name);
  const [change, setChange] = useState(feature.change ?? '');
  const [specification, setSpecification] = useState(feature.specification ?? '');
  const [working, setWorking] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (open) {
      setName(feature.name);
      setChange(feature.change ?? '');
      setSpecification(feature.specification ?? '');
      setError(null);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open]);

  async function save() {
    setWorking(true);
    setError(null);

    try {
      await api.feature.update(control.privateKey, feature.key, { name: name.trim(), change: change.trim(), specification: specification.trim() });
      await control.feature?.refresh();
      onClose();
    } catch (caught) {
      setError(caught instanceof ApiError ? caught.message : said.saveFailed);
    } finally {
      setWorking(false);
    }
  }

  return (
    <Dialog
      title={said.notesTitle}
      open={open}
      onClose={onClose}
      footer={
        <>
          <button type="button" onClick={onClose} className="btn-ghost">{said.cancel}</button>
          <button type="button" onClick={save} disabled={working || name.trim() === ''} className="btn-primary">
            {working && <IconSpinner />}
            {said.save}
          </button>
        </>
      }
    >
      <label className="block">
        <span className="text-slate-600 dark:text-slate-400">{said.name}</span>
        <input autoFocus value={name} onChange={(event) => setName(event.target.value)} maxLength={MAX_NAME} className="field mt-1.5" />
      </label>

      <label className="block">
        <span className="text-slate-600 dark:text-slate-400">{said.what}</span>
        <input value={change} onChange={(event) => setChange(event.target.value)} maxLength={500} placeholder={said.whatPlaceholder} className="field mt-1.5" />
      </label>

      <label className="block">
        <span className="text-slate-600 dark:text-slate-400">{said.wanted}</span>
        <textarea value={specification} onChange={(event) => setSpecification(event.target.value)} rows={4} maxLength={4000} className="field mt-1.5 resize-y" />
      </label>

      {error && <p className="text-xs text-red-500">{error}</p>}
    </Dialog>
  );
}

/**
 * Moving the version a feature is based on to the newest - marking the draft
 * as up to date, to the owner. Nothing is merged or rebased: the owner - or
 * the agent - brings a newer version's changes into the feature, and this is
 * where that is said, so the feature can be put online. Only ever to the
 * newest, since anything older would leave it just as far from going online.
 */
export function BaseDialog({ control, feature, open, onClose }: {
  control: Control;
  feature: Feature;
  open: boolean;
  onClose: () => void;
}) {
  const said = useEditorT().features;

  const to = feature.newest ?? feature.base;
  const [working, setWorking] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (open) {
      setError(null);
    }
  }, [open]);

  async function move() {
    setWorking(true);
    setError(null);

    try {
      await api.feature.update(control.privateKey, feature.key, { base: to });
      await control.feature?.refresh();
      onClose();
    } catch (caught) {
      setError(caught instanceof ApiError ? caught.message : said.saveFailed);
    } finally {
      setWorking(false);
    }
  }

  return (
    <Dialog
      title={said.baseTitle}
      open={open}
      onClose={onClose}
      footer={
        <>
          <button type="button" onClick={onClose} className="btn-ghost">{said.cancel}</button>
          <button type="button" onClick={move} disabled={working || to === feature.base} className="btn-primary">
            {working && <IconSpinner />}
            {said.moveTo(to)}
          </button>
        </>
      }
    >
      <p className="text-slate-600 dark:text-slate-400">{said.baseText(feature.base)}</p>

      <p className="flex gap-2 text-[13px] text-amber-700 dark:text-amber-400">
        <IconAlert className="mt-0.5 h-4 w-4 shrink-0" />
        {said.baseWarning}
      </p>

      {error && <p className="text-xs text-red-500">{error}</p>}
    </Dialog>
  );
}

/** Deleting a feature nobody wants any more - discarding the draft. */
export function DeleteFeatureDialog({ control, feature, open, onClose, onDeleted }: {
  control: Control;
  feature: Feature;
  open: boolean;
  onClose: () => void;
  onDeleted: () => void;
}) {
  const said = useEditorT().features;
  const [working, setWorking] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (open) {
      setError(null);
    }
  }, [open]);

  async function remove() {
    setWorking(true);

    try {
      await api.feature.remove(control.privateKey, feature.key);
      onDeleted();
    } catch (caught) {
      setError(caught instanceof ApiError ? caught.message : said.deleteFailed);
    } finally {
      setWorking(false);
    }
  }

  return (
    <Dialog
      title={said.deleteTitle(feature.name)}
      open={open}
      onClose={onClose}
      footer={
        <>
          <button type="button" onClick={onClose} className="btn-ghost">{said.keep}</button>
          <button type="button" onClick={remove} disabled={working} className="btn-danger">
            {working && <IconSpinner />}
            {said.deleteForGood}
          </button>
        </>
      }
    >
      <p className="text-slate-600 dark:text-slate-400">{said.deleteText}</p>
      {error && <p className="text-xs text-red-500">{error}</p>}
    </Dialog>
  );
}
