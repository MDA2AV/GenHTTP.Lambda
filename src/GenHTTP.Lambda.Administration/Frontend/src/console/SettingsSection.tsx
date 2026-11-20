import { useCallback, useEffect, useState } from 'react';

import { ApiError, refusedToken, api, type AdminSettings, type OptimizationOverview } from '@genhttp-lambda/kit/api';
import { IconSpinner } from '@genhttp-lambda/kit/components/Icons';
import { useToast } from '@genhttp-lambda/kit/components/Toast';
import { Ago, Section } from '@genhttp-lambda/kit/ui/ui';
import type { Access } from './context';
import { bytes, plural, Setting } from './parts';

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
    <Section title="Settings" hint="Applies at once - to the next run of the platform's jobs. No restart needed. Whether the build agent builds and changes apps is under Build agent.">
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
