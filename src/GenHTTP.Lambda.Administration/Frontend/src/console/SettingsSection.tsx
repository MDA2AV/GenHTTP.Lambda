import { useCallback, useEffect, useState, type ReactNode } from 'react';

import { ApiError, refusedToken, api, type AdminSettings, type OptimizationOverview } from '@genhttp-lambda/kit/api';
import { IconExternal, IconSpinner } from '@genhttp-lambda/kit/components/Icons';
import { useToast } from '@genhttp-lambda/kit/components/Toast';
import { Ago, Section, Switch } from '@genhttp-lambda/kit/ui/ui';
import type { Access } from './context';
import { bytes, plural } from './parts';

/** What the operator can switch on or off without restarting the host. */
export function SettingsSection({ access }: { access: Access }) {
  const { token, deny } = access;

  const toast = useToast();

  const [settings, setSettings] = useState<AdminSettings | null>(null);
  const [saving, setSaving] = useState<keyof AdminSettings | null>(null);
  const [error, setError] = useState<string | null>(null);

  // how the databases fared lately, beside the switch that decides whether
  // the platform looks after them - nothing where it cannot be read
  const [optimization, setOptimization] = useState<OptimizationOverview | null>(null);

  const fail = useCallback((problem: unknown, fallback: string) => {
    if (refusedToken(problem)) {
      deny();
    } else {
      setError(fallback);
    }
  }, [deny]);

  useEffect(() => {
    api.admin.settings(token)
      .then(setSettings)
      .catch((problem) => fail(problem, 'The settings could not be read.'));

    api.admin.optimization(token)
      .then(setOptimization)
      .catch(() => undefined);
  }, [token, fail]);

  async function toggle(key: keyof AdminSettings, said: (on: boolean) => string) {
    if (settings === null || saving !== null) return;

    setSaving(key);

    try {
      const saved = await api.admin.saveSettings(token, { ...settings, [key]: !settings[key] });

      setSettings(saved);

      toast(said(saved[key]), 'success');
    } catch (problem) {
      if (refusedToken(problem)) {
        deny();
      } else {
        toast(problem instanceof ApiError ? problem.message : 'The settings could not be saved.', 'error');
      }
    } finally {
      setSaving(null);
    }
  }

  return (
    <Section title="Settings" hint="Applies at once - to the next page anybody opens, the next run of the platform's jobs. No restart needed.">
      {settings === null ? (
        error !== null ? (
          <p className="text-sm text-red-500">{error}</p>
        ) : (
          <div className="flex items-center gap-2 text-sm text-slate-500">
            <IconSpinner /> Reading the settings…
          </div>
        )
      ) : (
        <div className="space-y-3">
          <Setting
            id="build-switch"
            title="Build new apps in the workspace on /build"
            on={settings.buildBox}
            saving={saving === 'buildBox'}
            onToggle={() => toggle('buildBox', (on) =>
              on ? 'New apps are built again.' : 'No new apps are built; the workspace leads to connecting an agent.')}
          >
            {settings.buildBox
              ? 'Visitors describe what they want and the agent of this installation creates it, as long as there is one.'
              : 'The workspace only leads to connecting an agent of one’s own over MCP. Nothing new is built from here.'}
          </Setting>

          <Setting
            id="change-switch"
            title="Let owners ask the agent about their apps"
            on={settings.changeBox}
            saving={saving === 'changeBox'}
            onToggle={() => toggle('changeBox', (on) =>
              on ? 'Owners can ask the agent again.' : 'Owners can no longer ask the agent; the workspace and the Change section explain how to connect one.')}
          >
            {settings.changeBox
              ? 'In the workspace on /build and the Change section of the editor, owners ask questions about an app, change its settings or say what should be different, and the agent of this installation does it, as long as there is one.'
              : 'The workspace and the Change section only explain how to connect an agent of one’s own over MCP.'}
          </Setting>

          <Setting
            id="optimization-switch"
            title="Optimize the databases in their quiet hours"
            on={settings.optimizeDatabases}
            saving={saving === 'optimizeDatabases'}
            onToggle={() => toggle('optimizeDatabases', (on) =>
              on ? 'The databases are optimized again, each in its quiet hours.' : 'The platform leaves the databases to their own tools.')}
            status={settings.optimizeDatabases && optimization ? <Optimized overview={optimization} /> : undefined}
          >
            {settings.optimizeDatabases
              ? 'The platform’s records and every lambda’s database, each at most once a day in its quiet hours (see Quiet hours), and a lambda’s only where it was visited since. SQLite: PRAGMA optimize, VACUUM where a quarter of a file is free, a checkpoint of the journal. PostgreSQL: VACUUM (ANALYZE) of every table.'
              : 'Nothing is done to the databases: they are left to their own tools, such as a PostgreSQL server’s own maintenance.'}
          </Setting>
        </div>
      )}
    </Section>
  );
}

function Setting({ id, title, on, saving, onToggle, link, status, children }: {
  id: string;
  title: string;
  on: boolean;
  saving: boolean;
  onToggle: () => void;
  link?: string;
  /** What the setting came to lately, under what it does. */
  status?: ReactNode;
  children: ReactNode;
}) {
  return (
    <div className="surface flex items-start gap-4 p-4">
      <div className="min-w-0 flex-1">
        <p id={id} className="text-[15px] font-medium">{title}</p>
        <p className="mt-1 text-[13px] text-slate-500">
          {children}
          {link && (
            <>
              {' '}
              {/* a page of the site, which is a bundle of its own: loaded, not routed to */}
              <a href={link} target="_blank" rel="noreferrer" className="inline-flex items-center gap-1 text-accent-600 hover:underline dark:text-accent-400">
                Open it
                <IconExternal className="h-3 w-3" />
              </a>
            </>
          )}
        </p>
        {status && <p className="mt-2 text-xs text-slate-500">{status}</p>}
      </div>

      {saving && <IconSpinner />}

      <Switch on={on} onToggle={onToggle} labelledBy={id} />
    </div>
  );
}

/** What optimizing the databases came to lately: the platform's own, and the lambdas' of the last day. */
function Optimized({ overview }: { overview: OptimizationOverview }) {
  const { platform, lambdas, failed, freed } = overview;

  return (
    <>
      {platform ? (
        <>
          The platform’s records <Ago at={platform.optimized} />
          {platform.failure ? <span className="text-red-500"> - failed: {platform.failure}</span> : <>, {bytes(Math.max(0, platform.before - platform.after))} given back</>}
        </>
      ) : (
        'Not done yet'
      )}
      {' · '}
      {plural(lambdas, 'lambda’s database', 'lambdas’ databases')} in the last day{lambdas > 0 && <>, {bytes(freed)} given back</>}
      {failed > 0 && <span className="text-red-500">, {plural(failed, 'failure')}</span>}
    </>
  );
}
