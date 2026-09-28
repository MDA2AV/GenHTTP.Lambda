import { absoluteAddress } from '../address';
import { IconDraft, IconExternal, IconPlus, IconSpark } from '../components/Icons';
import { useEditorT } from '../i18n';
import type { Control } from './context';
import { AgentMark, Ago, LiveDot, Section } from './ui';

/**
 * The features being worked on beside the lambda - drafts, to the owner.
 *
 * A version never changes once it is saved; a feature is where a change is
 * made instead. It starts as a copy of a version and of the lambda's data,
 * can be tried at an address of its own while visitors keep getting what is
 * online, and becomes the next version when it is put online. This lists
 * them and starts new ones; which version each began from is left out, and
 * only one that has fallen behind the newest says so.
 */
export function FeaturesTab({ control }: { control: Control }) {
  const t = useEditorT();
  const said = t.features;
  const { features, lambda } = control;

  const limit = control.summary?.limits.features ?? null;
  const full = limit != null && features.length >= limit;
  const agent = control.agent.state?.available ?? false;

  return (
    <Section
      title={t.frame.sections.features}
      hint={said.hint}
      actions={
        <button
          type="button"
          onClick={() => control.startFeature()}
          disabled={full || lambda.latestVersion == null}
          className="btn-primary !px-4 !py-1.5 text-[13px]"
          title={full ? said.full(limit!) : undefined}
        >
          <IconPlus className="h-3.5 w-3.5" />
          {said.newFeature}
        </button>
      }
    >
      {features.length === 0 ? (
        <div className="surface mx-auto max-w-xl p-8 text-center">
          <span className="mx-auto flex h-10 w-10 items-center justify-center rounded-full bg-accent-500/10 text-accent-600 dark:text-accent-400">
            <IconDraft className="h-5 w-5" />
          </span>
          <h2 className="mt-4 text-[15px] font-medium">{said.emptyTitle}</h2>
          <p className="mx-auto mt-2 max-w-md text-sm text-slate-600 dark:text-slate-400">{said.emptyText}</p>

          <div className="mt-6 flex flex-wrap justify-center gap-2">
            {agent && (
              <button type="button" onClick={() => control.askAgent()} className="btn-primary !px-4 !py-1.5 text-[13px]">
                <IconSpark className="h-3.5 w-3.5" />
                {said.askAgentNew}
              </button>
            )}
            <button
              type="button"
              onClick={() => control.startFeature()}
              disabled={lambda.latestVersion == null}
              className={`${agent ? 'btn-ghost !px-3' : 'btn-primary !px-4'} !py-1.5 text-[13px]`}
            >
              <IconPlus className="h-3.5 w-3.5" />
              {said.start}
            </button>
          </div>
        </div>
      ) : (
        <>
          <ul className="divide-y divide-slate-200 border-y border-slate-200 dark:divide-ink-800 dark:border-ink-800">
            {features.map((feature) => (
              <li key={feature.key} className="group flex items-start gap-3 py-3">
                <IconDraft className="mt-1 h-4 w-4 shrink-0 text-slate-400" />

                <button type="button" onClick={() => control.openFeature(feature.key)} className="min-w-0 flex-1 text-left">
                  <span className="flex items-center gap-2">
                    <span className="truncate text-[15px] font-medium group-hover:text-accent-600 dark:group-hover:text-accent-400">{feature.name}</span>
                    <AgentMark origin={feature.origin} />
                  </span>
                  <span className="mt-0.5 block truncate text-[13px] text-slate-500">
                    {feature.change ?? feature.specification ?? said.noChange}
                  </span>
                  <span className="mt-1.5 flex flex-wrap items-center gap-x-3 gap-y-1 text-xs text-slate-500">
                    <span className="flex items-center gap-1.5">
                      <LiveDot live={feature.online} />
                      {feature.online ? (feature.current ? said.previewOnline : said.previewOutdated) : said.previewOffline}
                    </span>
                    {!feature.mergeable && (
                      <span className="text-amber-600 dark:text-amber-400" title={said.behindTitle}>
                        {said.behind(feature.newest ?? feature.base)}
                      </span>
                    )}
                    <span>
                      {said.changed} <Ago at={feature.modified} />
                    </span>
                  </span>
                </button>

                {feature.online && (
                  <a
                    href={absoluteAddress(feature.previewPath)}
                    target="_blank"
                    rel="noreferrer"
                    className="mt-0.5 inline-flex shrink-0 items-center gap-1.5 text-[13px] text-accent-500 hover:underline"
                    title={said.openPreviewTitle}
                  >
                    <IconExternal className="h-3.5 w-3.5" />
                    <span className="hidden sm:inline">{said.openPreview}</span>
                  </a>
                )}
              </li>
            ))}
          </ul>

          {limit != null && <p className="mt-3 text-xs text-slate-500">{said.count(features.length, limit)}</p>}
        </>
      )}
    </Section>
  );
}
