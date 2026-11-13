import { useCallback, useEffect, useState, type ReactNode } from 'react';

import { ApiError, refusedToken, api, type AdminSettings } from '@genhttp-lambda/kit/api';
import { IconExternal, IconSpinner } from '@genhttp-lambda/kit/components/Icons';
import { useToast } from '@genhttp-lambda/kit/components/Toast';
import { Section, Switch } from '@genhttp-lambda/kit/ui/ui';
import type { Access } from './context';

/** What the operator can switch on or off without restarting the host. */
export function SettingsSection({ access }: { access: Access }) {
  const { token, deny } = access;

  const toast = useToast();

  const [settings, setSettings] = useState<AdminSettings | null>(null);
  const [saving, setSaving] = useState<keyof AdminSettings | null>(null);
  const [error, setError] = useState<string | null>(null);

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
        <div className="space-y-3">
          <Setting
            id="build-switch"
            title="Offer the text box that has an app built"
            on={settings.buildBox}
            saving={saving === 'buildBox'}
            onToggle={() => toggle('buildBox', (on) =>
              on ? 'The box is back.' : 'The box is gone; the page explains how to connect an agent.')}
          >
            {settings.buildBox
              ? 'Visitors describe what they want and the agent of this installation creates it, as long as there is one.'
              : 'The page only explains how to connect an agent of one’s own over MCP. Nothing is built from here.'}
          </Setting>

          <Setting
            id="change-switch"
            title="Offer the text box in the Change section of the editor"
            on={settings.changeBox}
            saving={saving === 'changeBox'}
            onToggle={() => toggle('changeBox', (on) =>
              on ? 'The box in the Change section is back.' : 'The box in the Change section is gone; it explains how to connect an agent.')}
          >
            {settings.changeBox
              ? 'Owners say what should be different and the agent of this installation changes it, as long as there is one.'
              : 'The section only explains how to connect an agent of one’s own over MCP.'}
          </Setting>
        </div>
      )}
    </Section>
  );
}

function Setting({ id, title, on, saving, onToggle, link, children }: {
  id: string;
  title: string;
  on: boolean;
  saving: boolean;
  onToggle: () => void;
  link?: string;
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
      </div>

      {saving && <IconSpinner />}

      <Switch on={on} onToggle={onToggle} labelledBy={id} />
    </div>
  );
}
