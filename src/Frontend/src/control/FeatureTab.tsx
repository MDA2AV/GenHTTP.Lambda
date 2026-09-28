import { useEffect, useState } from 'react';

import { absoluteAddress } from '../address';
import { ApiError, api, type LambdaFile } from '../api';
import { CopyField } from '../components/CopyField';
import { IconAlert, IconBranch, IconChevronDown, IconMerge, IconPencil, IconPlay, IconSpark, IconSpinner } from '../components/Icons';
import { useEditorT } from '../i18n';
import { ChangeList } from './Changes';
import type { Control } from './context';
import { AgentMark, Ago, LiveDot, Quote, Section } from './ui';

/**
 * One feature: what it is for, where it can be tried, whether it can be
 * merged, and what it changes against the version it is based on.
 *
 * The actions that change it - trying it, merging it, renaming or deleting
 * it - are the frame's, in the sidebar beside every view of the feature, so
 * this page is the one that explains it.
 */
export function FeatureTab({ control, onNotes, onRebase }: {
  control: Control;
  /** Opens the dialog for its name and notes. */
  onNotes: () => void;
  /** Opens the dialog that moves its base. */
  onRebase: () => void;
}) {
  const t = useEditorT();
  const said = t.features;
  const feature = control.feature!.info;

  const [files, setFiles] = useState<LambdaFile[] | null>(null);
  const [base, setBase] = useState<LambdaFile[] | null>(null);
  const [failure, setFailure] = useState<string | null>(null);

  // what it holds, and what the version it is based on holds - read again
  // whenever either changes
  useEffect(() => {
    let alive = true;

    Promise.all([api.feature.get(control.privateKey, feature.key), api.version(control.privateKey, feature.base)])
      .then(([content, version]) => {
        if (alive) {
          setFiles(content.files);
          setBase(version.files);
          setFailure(null);
        }
      })
      .catch((error) => alive && setFailure(error instanceof ApiError ? error.message : said.readFailed));

    return () => {
      alive = false;
    };
  }, [control.privateKey, feature.key, feature.base, feature.revision, said]);

  const behind = !feature.mergeable && feature.newest != null;
  const agent = control.agent.state?.available ?? false;

  // from the preview, a full path to the lambda is the live lambda - and its real data
  const own = `/lambda/${control.lambda.publicKey}/`;
  const leaks = (files ?? []).filter((file) => file.encoding !== 'base64' && file.code.includes(own)).map((file) => file.name);

  return (
    <Section
      title={
        <span className="flex items-center gap-2">
          <IconBranch className="h-4 w-4 text-slate-400" />
          {feature.name}
        </span>
      }
      hint={said.featureHint}
      actions={
        <>
          {agent && (
            <button type="button" onClick={() => control.askAgent(feature.key)} className="btn-ghost !px-3 !py-1.5 text-[13px]">
              <IconSpark className="h-3.5 w-3.5" />
              {said.askAgent}
            </button>
          )}
          <button type="button" onClick={() => control.edit()} className="btn-ghost !px-3 !py-1.5 text-[13px]">
            <IconPencil className="h-3.5 w-3.5" />
            {said.editCode}
          </button>
        </>
      }
    >
      <div className="surface grid gap-5 p-5 sm:grid-cols-3">
        <div>
          <div className="text-xs font-medium uppercase tracking-wide text-slate-500">{said.preview}</div>
          <p className="mt-1.5 flex items-center gap-2 text-[15px]">
            <LiveDot live={feature.online} />
            {feature.online ? (feature.current ? said.state.online : said.state.outdated) : said.state.offline}
          </p>
          {feature.online && feature.previewed && (
            <p className="mt-0.5 text-xs text-slate-500">
              {said.deployed} <Ago at={feature.previewed} />
            </p>
          )}
          {(!feature.online || !feature.current) && (
            <button
              type="button"
              onClick={() => control.feature?.preview()}
              disabled={control.feature?.previewing}
              className="mt-2 inline-flex items-center gap-1.5 text-[13px] font-medium text-accent-500 hover:underline disabled:opacity-50"
            >
              {control.feature?.previewing ? <IconSpinner className="h-3.5 w-3.5" /> : <IconPlay className="h-3.5 w-3.5" />}
              {feature.online ? said.updatePreview : said.deployPreview}
            </button>
          )}
        </div>

        <div>
          <div className="text-xs font-medium uppercase tracking-wide text-slate-500">{said.basedOn}</div>
          <p className="mt-1.5 text-[15px]">{said.version(feature.base, feature.mergeable, feature.base === control.lambda.activeVersion)}</p>
          <p className={`mt-0.5 text-xs ${feature.mergeable ? 'text-emerald-600 dark:text-emerald-400' : 'text-amber-600 dark:text-amber-400'}`}>
            {feature.mergeable ? said.mergeableLong : said.behind(feature.newest ?? feature.base)}
          </p>
        </div>

        <div>
          <div className="text-xs font-medium uppercase tracking-wide text-slate-500">{said.started}</div>
          <p className="mt-1.5 flex items-center gap-2 text-[15px]">
            <Ago at={feature.created} />
            <AgentMark origin={feature.origin} />
          </p>
          <p className="mt-0.5 text-xs text-slate-500">
            {said.changed} <Ago at={feature.modified} />
          </p>
        </div>
      </div>

      {feature.online && (
        <div className="mt-4 max-w-2xl">
          <CopyField label={said.previewAddress} value={absoluteAddress(feature.previewPath)} />
          <p className="mt-1.5 text-xs text-slate-500">{said.previewNote}</p>
        </div>
      )}

      {leaks.length > 0 && (
        <p className="mt-6 flex max-w-3xl gap-2 text-[13px] text-amber-700 dark:text-amber-400">
          <IconAlert className="mt-0.5 h-4 w-4 shrink-0" />
          <span>{said.leaks(own, leaks.join(', '))}</span>
        </p>
      )}

      {behind && (
        <div className="mt-6 border-l-2 border-amber-500 pl-4">
          <h2 className="flex items-center gap-2 text-sm font-medium">
            <IconAlert className="h-4 w-4 text-amber-500" />
            {said.behindTitle}
          </h2>
          <p className="mt-1.5 max-w-3xl text-[13px] text-slate-600 dark:text-slate-400">{said.behindText(feature.base, feature.newest!)}</p>
          <div className="mt-3 flex flex-wrap gap-2">
            {agent && (
              <button type="button" onClick={() => control.askAgent(feature.key)} className="btn-ghost !px-3 !py-1.5 text-[13px]">
                <IconSpark className="h-3.5 w-3.5" />
                {said.askAgent}
              </button>
            )}
            <button type="button" onClick={onRebase} className="btn-ghost !px-3 !py-1.5 text-[13px]">
              {said.moveBase}
            </button>
          </div>
          <Missed control={control} from={feature.base} to={feature.newest!} />
        </div>
      )}

      <div className="mt-8 grid gap-8 lg:grid-cols-5">
        <section className="lg:col-span-3">
          <div className="flex items-center justify-between gap-3">
            <h2 className="text-sm font-medium">{said.changes(feature.base)}</h2>
          </div>

          <div className="mt-3">
            {failure ? (
              <p className="text-sm text-red-500">{failure}</p>
            ) : files === null || base === null ? (
              <div className="flex items-center gap-2 text-sm text-slate-500">
                <IconSpinner /> {t.versions.comparing}
              </div>
            ) : (
              <ChangeList before={base} after={files} theme={control.theme} empty={said.noChanges(feature.base)} />
            )}
          </div>
        </section>

        <section className="lg:col-span-2">
          <div className="flex items-center justify-between gap-3">
            <h2 className="text-sm font-medium">{said.notes}</h2>
            <button type="button" onClick={onNotes} className="text-[13px] text-accent-500 hover:underline">{said.editNotes}</button>
          </div>

          <dl className="mt-3 space-y-4 text-[13px]">
            <div>
              <dt className="text-slate-500">{said.what}</dt>
              <dd className="mt-1 text-[15px]">{feature.change ?? <span className="text-slate-400">{said.noChange}</span>}</dd>
            </div>
            <div>
              <dt className="text-slate-500">{said.wanted}</dt>
              <dd className="mt-1">{feature.specification ? <Quote>{feature.specification}</Quote> : <span className="text-slate-400">{said.noWanted}</span>}</dd>
            </div>
          </dl>

          <p className="mt-6 flex items-start gap-2 text-[13px] text-slate-500">
            <IconMerge className="mt-0.5 h-3.5 w-3.5 shrink-0" />
            {said.mergeNote((feature.newest ?? feature.base) + 1)}
          </p>
        </section>
      </div>
    </Section>
  );
}

/**
 * What the versions saved after the feature began changed - what has to be
 * brought into the feature before it can be merged. Folded away, since it is
 * only read while doing exactly that.
 */
function Missed({ control, from, to }: { control: Control; from: number; to: number }) {
  const t = useEditorT();
  const said = t.features;
  const [open, setOpen] = useState(false);
  const [sides, setSides] = useState<{ before: LambdaFile[]; after: LambdaFile[] } | null>(null);
  const [failure, setFailure] = useState<string | null>(null);

  useEffect(() => {
    if (!open) {
      return;
    }

    let alive = true;

    setSides(null);
    setFailure(null);

    Promise.all([api.version(control.privateKey, from), api.version(control.privateKey, to)])
      .then(([before, after]) => alive && setSides({ before: before.files, after: after.files }))
      .catch((error) => alive && setFailure(error instanceof ApiError ? error.message : said.readFailed));

    return () => {
      alive = false;
    };
  }, [open, control.privateKey, from, to, said]);

  return (
    <div className="mt-4">
      <button
        type="button"
        onClick={() => setOpen((was) => !was)}
        aria-expanded={open}
        className="flex items-center gap-1.5 text-[13px] text-slate-600 hover:text-slate-900 dark:text-slate-400 dark:hover:text-slate-200"
      >
        <IconChevronDown className={`h-4 w-4 transition-transform ${open ? '' : '-rotate-90'}`} />
        {said.missed(from, to)}
      </button>

      {open && (
        <div className="mt-3">
          {failure ? (
            <p className="text-sm text-red-500">{failure}</p>
          ) : sides === null ? (
            <div className="flex items-center gap-2 text-sm text-slate-500"><IconSpinner /> {t.versions.comparing}</div>
          ) : (
            <ChangeList before={sides.before} after={sides.after} theme={control.theme} empty={said.missedNothing} />
          )}
        </div>
      )}
    </div>
  );
}
