import { useCallback, useEffect, useState } from 'react';
import { Link } from 'react-router-dom';

import { ApiError, api, type AdminSettings } from '../api';
import { IconExternal, IconSpinner } from '../components/Icons';
import { useToast } from '../components/Toast';
import { Section, Switch } from '../control/ui';
import { publishFeatures } from '../features';
import type { Access } from './context';

/** What the operator can switch on or off without restarting the host. */
export function SettingsSection({ access }: { access: Access }) {
  const { token, deny } = access;

  const toast = useToast();

  const [settings, setSettings] = useState<AdminSettings | null>(null);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fail = useCallback((problem: unknown, fallback: string) => {
    if (problem instanceof ApiError && problem.status === 404) {
      deny();
    } else {
      setError(fallback);
    }
  }, [deny]);

  useEffect(() => {
    api.admin.settings(token)
      .then(setSettings)
      .catch((problem) => fail(problem, 'The settings could not be read.'));
  }, [token, fail]);

  async function save(changed: AdminSettings) {
    setSaving(true);

    try {
      const saved = await api.admin.saveSettings(token, changed);

      setSettings(saved);
      // the header of this very tab follows along
      publishFeatures({ enterprise: saved.enterprisePage });

      toast(saved.enterprisePage ? 'The enterprise page is in the menu again.' : 'The enterprise page is no longer in the menu.', 'success');
    } catch (problem) {
      if (problem instanceof ApiError && problem.status === 404) {
        deny();
      } else {
        toast(problem instanceof ApiError ? problem.message : 'The settings could not be saved.', 'error');
      }
    } finally {
      setSaving(false);
    }
  }

  return (
    <Section title="Settings" hint="Applies to the next page anybody opens. No restart needed.">
      {settings === null ? (
        error !== null ? (
          <p className="text-sm text-red-500">{error}</p>
        ) : (
          <div className="flex items-center gap-2 text-sm text-slate-500">
            <IconSpinner /> Reading the settings…
          </div>
        )
      ) : (
        <div className="surface flex items-start gap-4 p-4">
          <div className="min-w-0 flex-1">
            <p id="enterprise-switch" className="text-[15px] font-medium">Show the enterprise page in the menu</p>
            <p className="mt-1 text-[13px] text-slate-500">
              {settings.enterprisePage
                ? 'Linked from the header on every page.'
                : 'Not linked from the header. The page is still there for anyone who has the address.'}{' '}
              <Link to="/enterprise" target="_blank" className="inline-flex items-center gap-1 text-accent-600 hover:underline dark:text-accent-400">
                Open it
                <IconExternal className="h-3 w-3" />
              </Link>
            </p>
          </div>

          {saving && <IconSpinner />}

          <Switch
            on={settings.enterprisePage}
            onToggle={() => !saving && save({ ...settings, enterprisePage: !settings.enterprisePage })}
            labelledBy="enterprise-switch"
          />
        </div>
      )}
    </Section>
  );
}
