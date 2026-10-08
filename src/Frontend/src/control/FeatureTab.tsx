import { useEffect, useState } from 'react';

import { shownAddress } from '../address';
import { ApiError, api, type LambdaFile } from '../api';
import { IconAlert, IconBranch, IconChevronDown, IconPencil, IconSpark, IconSpinner } from '../components/Icons';
import { useEditorT } from '../i18n';
import { ChangeList } from './Changes';
import { CloneMenu } from './CloneMenu';
import type { Control } from './context';
import { AgentMark, Ago, Quote, Section } from './ui';
import { isCompiled, isResource } from './written';

/** Where a page names an address rather than linking to it: its meta tags and its canonical link. */
const NAMED = /<meta\b[^>]*>|<link\b[^>]*\brel\s*=\s*["']?canonical\b[^>]*>/gi;

/**
 * One feature - a draft, to the owner: what it changes, what was asked for,
 * and the files it changes.
 *
 * Trying it and putting it online are the frame's, in the sidebar beside
 * every view of the feature, so this page says what the draft is rather than
 * repeating them. Which version it began from is not said at all: it matters
 * only once a newer version means the draft cannot go online as it is, and
 * then the page says that, and what to do about it.
 */
export function FeatureTab({ control, onNotes, onRebase }: {
  control: Control;
  /** Opens the dialog for its name and notes. */
  onNotes: () => void;
  /** Opens the dialog that marks it as up to date. */
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

  // what a draft is to somebody who does not read code: what it does, what
  // was asked for, and whether it can go online - not the files it changes
  const { simple } = control;

  // from the preview, the full address of the lambda is the live lambda - and its real data;
  // so is its old path below /lambda/, which leads there
  const own = shownAddress(control.lambda.publicUrl).toLowerCase();
  const old = `/lambda/${control.lambda.publicKey}/`;
  // what is compiled or served only: documentation naming the address says where it is, and
  // links nowhere - and what a front end is built from says it again in what it builds, if anywhere.
  // Meta tags and the canonical link name the page and are never followed, as the server sees it too.
  // A host is matched in any case, as the server matches it
  const linked = (code: string) => {
    const followed = code.replace(NAMED, '');
    return followed.toLowerCase().includes(`//${own}`) ? own : followed.includes(old) ? old : null;
  };
  const found = (files ?? []).filter((file) => file.encoding !== 'base64' && (isCompiled(file.name) || isResource(file.name)))
                             .map((file) => ({ name: file.name, address: linked(file.code) }))
                             .filter((file) => file.address !== null);
  const leaks = found.map((file) => file.name);
  // said as the files write it: the address, or the old path that leads there
  const leaked = found.some((file) => file.address === own) ? own : old;

  return (
    <Section
      title={feature.name}
      hint={said.featureHint}
      actions={
        <>
          {agent && (
            <button type="button" onClick={() => control.askAgent(feature.key)} className="btn-ghost !px-3 !py-1.5 text-[13px]">
              <IconSpark className="h-3.5 w-3.5" />
              {said.askAgent}
            </button>
          )}
          {!simple && (
            <button type="button" onClick={() => control.edit()} className="btn-ghost !px-3 !py-1.5 text-[13px]">
              <IconPencil className="h-3.5 w-3.5" />
              {said.editCode}
            </button>
          )}
          {!simple && <CloneMenu lambda={control.lambda} feature={feature} />}
        </>
      }
    >
      <div className="max-w-3xl">
        <p className="-mt-2 flex flex-wrap items-center gap-x-2 gap-y-1 text-[13px] text-slate-500">
          <span>
            {said.started} <Ago at={feature.created} />
          </span>
          <AgentMark origin={feature.origin} git={!simple} />
          <span aria-hidden="true">·</span>
          <span>
            {said.changed} <Ago at={feature.modified} />
          </span>
          {/* where a clone has it, for whoever works on it with git */}
          {!simple && (
            <>
              <span aria-hidden="true">·</span>
              <span className="inline-flex items-center gap-1" title={said.branchTitle}>
                <IconBranch className="h-3.5 w-3.5" />
                <code className="font-mono text-[12px]">{feature.branch}</code>
              </span>
            </>
          )}
        </p>

        {behind && (
          <div className="mt-5 border-l-2 border-amber-500 pl-4">
            <h2 className="flex items-center gap-2 text-sm font-medium">
              <IconAlert className="h-4 w-4 text-amber-500" />
              {said.behindTitle}
            </h2>
            <p className="mt-1.5 text-[13px] text-slate-600 dark:text-slate-400">
              {simple ? t.simple.behindText : said.behindText(feature.base, feature.newest!)}
            </p>
            <div className="mt-3 flex flex-wrap gap-2">
              {agent && (
                <button type="button" onClick={() => control.askAgent(feature.key, said.catchUp)} className="btn-primary !px-4 !py-1.5 text-[13px]">
                  <IconSpark className="h-3.5 w-3.5" />
                  {said.askCatchUp}
                </button>
              )}
              {(!simple || !agent) && (
                <button type="button" onClick={onRebase} className="btn-ghost !px-3 !py-1.5 text-[13px]">
                  {said.moveBase}
                </button>
              )}
            </div>
            {!simple && <Missed control={control} from={feature.base} to={feature.newest!} />}
          </div>
        )}

        {leaks.length > 0 && !simple && (
          <p className="mt-5 flex gap-2 text-[13px] text-amber-700 dark:text-amber-400">
            <IconAlert className="mt-0.5 h-4 w-4 shrink-0" />
            <span>{said.leaks(leaked, leaks.join(', '))}</span>
          </p>
        )}

        {/* what it is for: the line the version it becomes will carry, and what was asked for */}
        <div className="mt-6">
          <div className="flex items-start justify-between gap-4">
            <p className="text-[17px] leading-relaxed">
              {feature.change ?? <span className="text-slate-400">{said.noChange}</span>}
            </p>
            <button type="button" onClick={onNotes} className="mt-1 shrink-0 text-[13px] text-accent-500 hover:underline">
              {said.editNotes}
            </button>
          </div>
          {feature.specification && (
            <div className="mt-3">
              <Quote>{feature.specification}</Quote>
            </div>
          )}
        </div>

        {!simple && (
        <section className="mt-8">
          <h2 className="text-sm font-medium">{said.changes(feature.base)}</h2>

          <div className="mt-3">
            {failure ? (
              <p className="text-sm text-red-500">{failure}</p>
            ) : files === null || base === null ? (
              <div className="flex items-center gap-2 text-sm text-slate-500">
                <IconSpinner /> {t.versions.comparing}
              </div>
            ) : (
              <ChangeList before={base} after={files} theme={control.theme} empty={said.noChanges(feature.base)} folded />
            )}
          </div>
        </section>
        )}
      </div>
    </Section>
  );
}

/**
 * What the versions saved after the feature began changed - what has to be
 * brought into the feature before it can go online. Folded away, since it is
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
