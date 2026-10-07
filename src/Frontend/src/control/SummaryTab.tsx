import type { ReactNode } from 'react';
import { Link } from 'react-router-dom';

import { isDemo } from '../api';
import { IconAlert, IconCheck, IconDraft, IconGlobe, IconLock, IconSpinner } from '../components/Icons';
import { useEditorT } from '../i18n';
import { CloneMenu } from './CloneMenu';
import type { Control } from './context';
import { ago, bytes, count, local, millis, percent, span } from './format';
import { AgentMark, Ago, Figure, LiveDot, Meter, Quote, Section, Sparkline } from './ui';

/**
 * Whether it is working, in the order somebody asks: is it up, is anybody
 * using it, is it failing, what changed last, and is there room left.
 *
 * Above all of that, what the app is - the paragraph its documentation opens
 * with, the way a repository is described in a line under its name - since
 * whoever opens a lambda they did not write, a demo included, asks that
 * first.
 */
export function SummaryTab({ control }: { control: Control }) {
  const t = useEditorT();
  const said = t.summary;
  const title = t.frame.sections.overview;

  const { lambda, summary, features } = control;
  const base = `/editor/${control.privateKey}`;
  const demo = isDemo(lambda.tier);

  if (!summary) {
    return (
      <Section title={title}>
        <div className="flex items-center gap-2 py-10 text-sm text-slate-500">
          <IconSpinner /> {said.reading}
        </div>
      </Section>
    );
  }

  const { traffic, storage, limits, activation, latest, documentation } = summary;

  const live = lambda.activeVersion != null;
  const errorTone = traffic.dayFailed === 0 ? (traffic.dayRequests > 0 ? 'good' : 'default') : traffic.dayFailed / traffic.dayRequests > 0.05 ? 'bad' : 'warn';

  return (
    <Section
      title={title}
      hint={said.hint(ago(traffic.since, t.shared), !!lambda.keptUntil, limits.retentionDays, lambda.tier)}
      actions={<CloneMenu lambda={lambda} />}
    >
      {documentation.about && (
        <p className="mb-5 max-w-3xl text-[15px] leading-relaxed text-slate-700 dark:text-slate-300">
          {documentation.about}{' '}
          <Link to={`${base}/docs`} className="whitespace-nowrap text-[13px] text-accent-500 hover:underline">{said.readDocs}</Link>
        </p>
      )}

      <p className="text-[15px]">
        {live
          ? said.onlineFor(
              (fallback) => <strong className="font-semibold">{activation ? span(activation.seconds, t.shared) : fallback}</strong>,
              lambda.activeVersion!,
            )
          : latest
            ? said.offline
            : said.nothing}
      </p>

      <div className="surface mt-5 grid grid-cols-2 gap-6 p-5 lg:grid-cols-4">
        <div>
          <Figure value={count(traffic.dayRequests)} label={said.requestsToday} title={said.lastHour(traffic.hourRequests)} />
          <div className="mt-2"><Sparkline values={traffic.hourly} label={said.hourly} /></div>
        </div>
        <Figure
          value={traffic.dayRequests > 0 ? percent(traffic.dayFailed, traffic.dayRequests) : '-'}
          label={said.failed}
          tone={errorTone}
          title={said.failedTitle(traffic.dayFailed, traffic.dayRejected)}
        />
        <Figure value={traffic.dayRequests > 0 ? millis(traffic.averageMillis) : '-'} label={said.average} />
        <Figure value={traffic.lastSeen ? ago(traffic.lastSeen, t.shared) : said.noneYet} label={said.lastVisit} />
      </div>

      {summary.recentProblems.length > 0 && (
        <div className="mt-6 border-l-2 border-red-500 pl-4">
          <div className="flex items-center justify-between gap-3">
            <h2 className="flex items-center gap-2 text-sm font-medium">
              <IconAlert className="h-4 w-4 text-red-500" />
              {said.problems}
            </h2>
            <Link to={`${base}/logs`} className="text-[13px] text-accent-500 hover:underline">{said.openLog}</Link>
          </div>
          <ul className="mt-2 space-y-1.5">
            {distinct(summary.recentProblems).slice(0, 3).map((problem) => (
              <li key={problem.seq} className="flex gap-3 text-[13px]">
                <span className="min-w-0 flex-1 truncate" title={problem.text}>{local(problem.text, lambda.publicKey)}</span>
                <Ago at={problem.at} className="shrink-0 text-slate-500" />
              </li>
            ))}
          </ul>
        </div>
      )}

      <div className="mt-8 grid gap-8 lg:grid-cols-5">
        <section className="lg:col-span-3">
          <div className="flex items-center justify-between">
            <h2 className="text-sm font-medium">{said.latest}</h2>
            <Link to={`${base}/versions`} className="text-[13px] text-accent-500 hover:underline">{said.allVersions}</Link>
          </div>

          {latest ? (
            <div className="mt-3 space-y-2">
              <p className="text-[15px]">
                {latest.change ?? <span className="text-slate-500">{said.noDescription}</span>}
              </p>
              <p className="flex flex-wrap items-center gap-2 text-[13px] text-slate-500">
                <span>{said.version(latest.version)}</span>
                <AgentMark origin={latest.origin} git />
                <Ago at={latest.created} />
                {latest.version !== lambda.activeVersion && <span className="text-amber-600 dark:text-amber-400">{said.notOnline}</span>}
              </p>
              {latest.specification && (
                <details className="text-[13px]">
                  <summary className="cursor-pointer select-none text-slate-500 hover:text-slate-700 dark:hover:text-slate-300">
                    {said.wanted}
                  </summary>
                  <div className="mt-2"><Quote>{latest.specification}</Quote></div>
                </details>
              )}
            </div>
          ) : (
            <p className="mt-3 text-sm text-slate-500">{said.noVersions}</p>
          )}

          {/* the drafts being worked on beside it, while there are any */}
          {!demo && features.length > 0 && (
            <div className="mt-8">
              <div className="flex items-center justify-between">
                <h2 className="text-sm font-medium">{said.inProgress}</h2>
                <Link to={`${base}/features`} className="text-[13px] text-accent-500 hover:underline">{said.allFeatures}</Link>
              </div>
              <ul className="mt-3 space-y-2">
                {features.slice(0, 3).map((feature) => (
                  <li key={feature.key} className="flex items-center gap-3 text-[13px]">
                    <IconDraft className="h-3.5 w-3.5 shrink-0 text-slate-400" />
                    <button
                      type="button"
                      onClick={() => control.openFeature(feature.key)}
                      className="min-w-0 flex-1 truncate text-left text-[15px] hover:text-accent-600 dark:hover:text-accent-400"
                      title={feature.change ?? undefined}
                    >
                      {feature.name}
                    </button>
                    <AgentMark origin={feature.origin} />
                    <span className="flex shrink-0 items-center gap-1.5 text-slate-500" title={feature.online ? said.previewOnline : said.previewOffline}>
                      <LiveDot live={feature.online} />
                      <span className="sr-only">{feature.online ? said.previewOnline : said.previewOffline}</span>
                      {!feature.mergeable && <span className="text-amber-600 dark:text-amber-400">{said.behind}</span>}
                    </span>
                    <Ago at={feature.modified} className="hidden shrink-0 text-slate-500 sm:inline" />
                  </li>
                ))}
              </ul>
            </div>
          )}
        </section>

        {/* the two halves of what a lambda keeps, apart: what belongs to a
            version, and what belongs to the lambda whichever version runs -
            each one allowance, which what is under it shares */}
        <section className="lg:col-span-2">
          <h2 className="text-sm font-medium">{said.storage}</h2>

          <div className="mt-3 space-y-5">
            <div>
              <div className="flex items-center justify-between gap-3">
                <h3 className="text-xs font-medium uppercase tracking-wide text-slate-500">
                  {storage.version != null ? said.inVersion(storage.version) : said.noVersion}
                </h3>
                <Link to={`${base}/code`} className="text-[13px] text-accent-500 hover:underline">{said.browse}</Link>
              </div>

              <div className="mt-2 space-y-2">
                <Meter label={said.versionAllowance} used={storage.codeBytes + storage.resourceBytes} of={limits.buildBytes} format={bytes} />

                <Part label={said.code} exposure={<Exposure open={false} why={said.codeWhy} />}>
                  {said.files(storage.codeFiles, bytes(storage.codeBytes))}
                </Part>
                <Part
                  label={said.resources}
                  exposure={<Exposure open={storage.servesResources} why={storage.servesResources ? said.resourcesPublic : said.resourcesPrivate} />}
                >
                  {said.files(storage.resourceFiles, bytes(storage.resourceBytes))}
                </Part>

                {/* counted in pages rather than room: what matters is whether one is missing */}
                {storage.version != null && (
                  <div className="flex flex-wrap items-center justify-between gap-x-3 gap-y-1.5 text-[13px]">
                    <span className="flex items-center gap-1.5 text-slate-700 dark:text-slate-300">
                      {said.written}
                      <Exposure open={false} why={said.writtenWhy} />
                    </span>
                    <span className="flex flex-wrap items-center gap-x-3 gap-y-1">
                      <Written to={`${base}/docs`} done={documentation.product} label={t.context.docs.pages.product} missing={said.writtenMissing} />
                      <Written to={`${base}/docs?page=decisions.md`} done={documentation.decisions} label={t.context.docs.pages.decisions} missing={said.writtenMissing} />
                      <Written to={`${base}/tests`} done={documentation.tests} label={t.frame.sections.tests} missing={said.writtenMissing} />
                    </span>
                  </div>
                )}
              </div>
            </div>

            <div>
              <div className="flex items-center justify-between gap-3">
                <h3 className="text-xs font-medium uppercase tracking-wide text-slate-500" title={said.sharedByAll}>
                  {said.inData}
                </h3>
                <Link to={`${base}/data`} className="text-[13px] text-accent-500 hover:underline">{said.browse}</Link>
              </div>

              <div className="mt-2 space-y-2">
                <Meter
                  label={said.dataAllowance}
                  used={(storage.databaseEnabled ? storage.databaseBytes : 0) + (storage.workspaceEnabled ? storage.workspaceBytes : 0)}
                  of={limits.dataBytes}
                  format={bytes}
                />

                {/* its records first: that is what most lambdas keep */}
                <Part label={said.database} exposure={<Exposure open={false} why={said.dataPrivate} />}>
                  {storage.databaseEnabled ? (
                    said.databaseHolds(storage.databaseTables, bytes(storage.databaseBytes))
                  ) : storage.usesDatabase ? (
                    <Link
                      to={`${base}/data/database`}
                      className="inline-flex items-center gap-1 text-amber-600 hover:underline dark:text-amber-400"
                      title={said.databaseOffUsed}
                    >
                      <IconAlert className="h-3.5 w-3.5" />
                      {said.databaseOff}
                    </Link>
                  ) : (
                    said.databaseOff
                  )}
                </Part>

                <Part
                  label={said.workspace}
                  exposure={<Exposure open={storage.workspaceEnabled && storage.servesWorkspace} why={storage.servesWorkspace ? said.dataPublic : said.dataPrivate} />}
                >
                  {storage.workspaceEnabled ? said.files(storage.workspaceFiles, bytes(storage.workspaceBytes)) : said.workspaceOff}
                </Part>

                {/* counted in names rather than room: a secret is small, and what matters is whether one is missing */}
                <p className="flex items-center justify-between gap-3 text-[13px]">
                  <span className="flex items-center gap-1.5 text-slate-700 dark:text-slate-300">
                    {said.secrets}
                    <Exposure open={false} why={said.dataPrivate} />
                  </span>
                  <span className="flex items-center gap-2 text-slate-500">
                    {(storage.missingSecrets?.length ?? 0) > 0 && (
                      <Link
                        to={`${base}/data/secrets`}
                        className="inline-flex items-center gap-1 text-amber-600 hover:underline dark:text-amber-400"
                        title={said.secretsMissingTitle}
                      >
                        <IconAlert className="h-3.5 w-3.5" />
                        {said.secretsMissing(storage.missingSecrets!.length)}
                      </Link>
                    )}
                    <span className="tabular-nums">{storage.secretsEnabled ? said.secretsCount(storage.secrets) : said.secretsOff}</span>
                  </span>
                </p>
              </div>
            </div>
          </div>
        </section>
      </div>
    </Section>
  );
}

/**
 * One line per problem rather than per occurrence: the same request failing
 * three times is one thing that is wrong. The timing on a request line is a
 * measurement rather than part of what went wrong, so it is left out of the
 * comparison.
 */
function distinct<T extends { text: string }>(lines: T[]): T[] {
  const seen = new Set<string>();

  return lines.filter((line) => {
    const key = line.text.replace(/ — \d{3} ·.*$/, '');

    if (seen.has(key)) {
      return false;
    }

    seen.add(key);
    return true;
  });
}

/** One of the things an allowance is shared by, and what it holds. */
function Part({ label, exposure, children }: { label: string; exposure: ReactNode; children: ReactNode }) {
  return (
    <p className="flex items-center justify-between gap-3 text-[13px]">
      <span className="flex items-center gap-1.5 text-slate-700 dark:text-slate-300">
        {label}
        {exposure}
      </span>
      <span className="tabular-nums text-slate-500">{children}</span>
    </p>
  );
}

/** One page of the documentation or the tests, and whether the version has it. */
function Written({ to, done, label, missing }: { to: string; done: boolean; label: string; missing: string }) {
  return (
    <Link
      to={to}
      title={done ? undefined : missing}
      className={`inline-flex items-center gap-1 hover:underline ${done ? 'text-slate-600 dark:text-slate-400' : 'text-slate-400 dark:text-slate-500'}`}
    >
      {done ? (
        <IconCheck className="h-3.5 w-3.5 text-emerald-500" />
      ) : (
        <span aria-hidden="true" className="inline-block h-3 w-3 rounded-full border border-dashed border-slate-400" />
      )}
      {label}
      {!done && <span className="sr-only">: {missing}</span>}
    </Link>
  );
}

/** Whether the public can reach it, as an icon, with the reason on hover. */
export function Exposure({ open, why }: { open: boolean; why: string }) {
  return open ? (
    <span title={why} className="inline-flex text-accent-500"><IconGlobe className="h-3.5 w-3.5" /><span className="sr-only">{why}</span></span>
  ) : (
    <span title={why} className="inline-flex text-slate-400"><IconLock className="h-3.5 w-3.5" /><span className="sr-only">{why}</span></span>
  );
}
