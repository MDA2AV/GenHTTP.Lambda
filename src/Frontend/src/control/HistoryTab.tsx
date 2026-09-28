import { useState } from 'react';

import { isDemo, type VersionInfo } from '../api';
import { Dialog } from '../components/Dialog';
import { useEditorT } from '../i18n';
import type { Control } from './context';
import { OnlineMark } from './SimpleOverview';
import { AgentMark, Ago, Section } from './ui';

/**
 * What the simple view has instead of the versions: every version, as the
 * change it was - what it did and when, which one visitors get, and a way
 * back to any other.
 *
 * Going back is deploying an older version, which is all it ever was, but
 * said as what it does. There is nothing to compare and no files to browse:
 * somebody who wants those wants the full view.
 */
export function HistoryTab({ control }: { control: Control }) {
  const t = useEditorT();
  const said = t.simple;
  const { versions, lambda, busy } = control;
  const [asking, setAsking] = useState<VersionInfo | null>(null);

  const demo = isDemo(lambda.tier);
  const active = lambda.activeVersion;

  // newer than what is online - or nothing is online - is putting it online;
  // older is going back
  const forward = (version: number) => active == null || version > active;

  return (
    <Section title={t.frame.sections.history} hint={said.historyHint}>
      {/* a lambda starts with a version, so there is always one to show */}
      {versions.length > 0 && (
        <ol className="max-w-3xl">
          {versions.map((version, index) => {
            const online = version.version === active;
            const last = index === versions.length - 1;

            return (
              <li key={version.version} className="relative flex gap-4 pb-6 last:pb-0">
                {/* the line that ties one change to the one before it */}
                {!last && <span className="absolute left-[5px] top-4 h-full w-px bg-slate-200 dark:bg-ink-800" aria-hidden="true" />}

                <span
                  className={`relative mt-1.5 h-[11px] w-[11px] shrink-0 rounded-full border-2 ${
                    online ? 'border-emerald-500 bg-emerald-500' : 'border-slate-300 bg-white dark:border-ink-700 dark:bg-ink-950'
                  }`}
                  aria-hidden="true"
                />

                <div className="min-w-0 flex-1">
                  <p className={`break-words text-[15px] leading-snug ${version.change ? '' : 'text-slate-500'}`}>
                    {version.change ?? (version.origin === 'template' ? said.created : said.noNote)}
                  </p>
                  <p className="mt-1 flex flex-wrap items-center gap-2 text-[13px] text-slate-500">
                    <Ago at={version.created} />
                    <AgentMark origin={version.origin} />
                    {online && <OnlineMark />}
                  </p>
                </div>

                {!online && !demo && (
                  <button
                    type="button"
                    onClick={() => setAsking(version)}
                    disabled={busy !== null}
                    className="mt-0.5 shrink-0 self-start text-[13px] font-medium text-accent-600 hover:underline disabled:opacity-50 dark:text-accent-400"
                  >
                    {forward(version.version) ? said.putThisOnline : said.goBack}
                  </button>
                )}
              </li>
            );
          })}
        </ol>
      )}

      <Dialog
        title={asking && forward(asking.version) ? said.putOnlineTitle : said.goBackTitle}
        open={asking !== null}
        onClose={() => setAsking(null)}
        footer={
          <>
            <button type="button" onClick={() => setAsking(null)} className="btn-ghost">
              {said.cancel}
            </button>
            <button
              type="button"
              className="btn-primary"
              onClick={() => {
                const version = asking?.version;

                setAsking(null);

                if (version != null) {
                  void control.deploy(version);
                }
              }}
            >
              {asking && forward(asking.version) ? said.putOnlineConfirm : said.goBackConfirm}
            </button>
          </>
        }
      >
        {asking?.change && <p className="border-l-2 border-slate-300 pl-3 text-slate-700 dark:border-ink-700 dark:text-slate-300">{asking.change}</p>}
        <p className="text-slate-600 dark:text-slate-400">{asking && forward(asking.version) ? said.putOnlineText : said.goBackText}</p>
      </Dialog>
    </Section>
  );
}
