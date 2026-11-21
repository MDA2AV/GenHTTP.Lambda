import { useCallback, useEffect, useState } from 'react';

import { ApiError, refusedToken, api, type AdminSettings, type AdminSettingsChange, type OptimizationOverview } from '@genhttp-lambda/kit/api';
import { IconSpinner } from '@genhttp-lambda/kit/components/Icons';
import { useToast } from '@genhttp-lambda/kit/components/Toast';
import { Ago, Section } from '@genhttp-lambda/kit/ui/ui';
import type { Access } from './context';
import { bytes, plural, Setting } from './parts';

/**
 * Who each profile is for, and what the pages and agents make of it - in the
 * order they are offered.
 */
const PROFILES: Record<string, { label: string; says: string }> = {
  cloud: {
    label: 'Cloud',
    says: 'A public platform anybody builds on. Examples: a club, a guestbook, a quiz night.',
  },
  home: {
    label: 'Home',
    says: 'For you, your household and friends. Examples: a club, a guestbook, a quiz night.',
  },
  enterprise: {
    label: 'Enterprise',
    says: 'For an organization and its people. Examples: time off, room bookings, onboarding; what is built is called an app.',
  },
};

/** What the operator can switch on or off, and choose, without restarting the host. */
export function SettingsSection({ access }: { access: Access }) {
  const { token, deny } = access;

  const toast = useToast();

  const [settings, setSettings] = useState<AdminSettings | null>(null);
  const [saving, setSaving] = useState<keyof AdminSettingsChange | null>(null);
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

  async function save(change: AdminSettingsChange, said: (saved: AdminSettings) => string) {
    if (settings === null || saving !== null) return;

    setSaving(Object.keys(change)[0] as keyof AdminSettingsChange);

    try {
      const saved = await api.admin.saveSettings(token, change);

      setSettings(saved);

      toast(said(saved), 'success');
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
    <Section title="Settings" hint="Applies at once - to the next page served and the next run of the platform's jobs. No restart needed. Whether the build agent builds and changes apps is under Build agent.">
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
          {/* only where the host lets the operator choose: genhttp.dev is the cloud */}
          {settings.profiles.length > 1 && (
            <Setting
              id="profile-choice"
              title="Who it is for"
              saving={saving === 'profile'}
              field={
                <select
                  aria-labelledby="profile-choice"
                  value={settings.profile}
                  disabled={saving !== null}
                  onChange={(event) => void save({ profile: event.target.value }, (saved) =>
                    `Who it is for: ${PROFILES[saved.profile]?.label ?? saved.profile}, from the next page served.`)}
                  className="field w-auto py-1.5 text-[13px] disabled:opacity-60"
                >
                  {settings.profiles.map((profile) => (
                    <option key={profile} value={profile}>{PROFILES[profile]?.label ?? profile}</option>
                  ))}
                </select>
              }
            >
              {PROFILES[settings.profile]?.says}{' '}
              It changes the examples and words of the pages and what agents read about the apps they build here (platform_guide) - nothing the platform allows, refuses or keeps.
            </Setting>
          )}

          <Setting
            id="optimization-switch"
            title="Optimize the databases in their quiet hours"
            on={settings.optimizeDatabases}
            saving={saving === 'optimizeDatabases'}
            onToggle={() => void save({ optimizeDatabases: !settings.optimizeDatabases }, (saved) =>
              saved.optimizeDatabases ? 'The databases are optimized again, each in its quiet hours.' : 'The platform leaves the databases to their own tools.')}
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
