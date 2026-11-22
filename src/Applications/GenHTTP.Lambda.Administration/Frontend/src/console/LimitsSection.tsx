import { Fragment, useCallback, useEffect, useMemo, useState, type ReactNode } from 'react';

import { ApiError, refusedToken, api, type AdminLimits, type TierLimits } from '@genhttp-lambda/kit/api';
import { IconSpinner } from '@genhttp-lambda/kit/components/Icons';
import { useToast } from '@genhttp-lambda/kit/components/Toast';
import { Section } from '@genhttp-lambda/kit/ui/ui';
import type { Access } from './context';

type Tier = 'sandbox' | 'production' | 'system';

/** The tiers the operator sets, a column each - the demos are held to the sandbox's. */
const TIERS: { key: Tier; name: string }[] = [
  { key: 'sandbox', name: 'Sandbox' },
  { key: 'production', name: 'Production' },
  { key: 'system', name: 'System' },
];

const MB = 1024 * 1024;

/** One limit as the form shows it. Bytes are edited in megabytes and sent as bytes. */
interface Limit {
  key: string;
  title: string;
  note: string;
  unit: string;
  bytes?: boolean;
  /** A yes or a no rather than a number. */
  flag?: boolean;
  /** Words rather than a number, which may be left empty. */
  text?: boolean;
}

/** What each tier has, the same for every tier: a column each. */
const TIER: (Limit & { key: keyof TierLimits })[] = [
  { key: 'label', title: 'Called', unit: '', text: true,
    note: 'What the pages call the tier, as written, in every language. Empty for its own name, which the pages translate. The API and the code use the name only.' },
  { key: 'buildBytes', title: 'Build', unit: 'MB', bytes: true,
    note: 'A version: its code - the C#, and the documentation, the tests and whatever else is kept with it - and its resources, together. Kept in every version. The C# is compiled, so this also bounds what the compiler spends, which the whole server shares.' },
  { key: 'dataBytes', title: 'Data', unit: 'MB', bytes: true,
    note: 'What a lambda keeps while it runs: its database and its workspace, together. Holds from the next write; nothing already stored is removed.' },
  { key: 'analyticsDays', title: 'Analytics shown', unit: 'days',
    note: 'How far back the owner sees what was counted of their visitors. A year is kept of every lambda, so at most 365; moved to a tier that shows more, a lambda shows what was counted before.' },
  { key: 'connectivity', title: 'Connects by default', unit: '', flag: true,
    note: 'Whether its code may connect to other servers while nothing was decided about the lambda. What is allowed or declined in Connectivity comes first. Switched off, the lambdas of the tier that connect are built again.' },
];

/** Only the sandbox's: production and system lambdas are kept online and kept. */
const LIFETIME: Limit[] = [
  { key: 'offlineAfterHours', title: 'Offline after', unit: 'hours',
    note: 'Without visits or edits. Its address stops answering until it is used or deployed again. A lambda moved into the sandbox gets the whole time from the move.' },
  { key: 'removedAfterHours', title: 'Removed after', unit: 'hours',
    note: 'Without visits or edits: versions, features and data, all of it.' },
];

/** The same for every lambda, whatever its tier. */
const EVERY_LAMBDA: Limit[] = [
  { key: 'versions', title: 'Versions kept', unit: '',
    note: 'Older ones are removed when a new one is saved, never the one online.' },
  { key: 'features', title: 'Features open at once', unit: '',
    note: 'Each holds a copy of the files, the workspace and the database.' },
];

/** Counted per caller rather than per lambda, so there is no tier to them. */
const EVERYBODY: Limit[] = [
  { key: 'showcaseImageBytes', title: 'Showcase picture', unit: 'MB', bytes: true,
    note: 'Every visitor of the showcase downloads it.' },
  { key: 'requestsPerSecond', title: 'Requests per second', unit: '',
    note: 'Per client, across every lambda it calls.' },
  { key: 'buildsPerDay', title: 'Builds per day', unit: '',
    note: 'Builds and changes the agent of this installation makes for one address. Counted in memory.' },
];

/** The tiers a new lambda may start in: the installation's own are not among them. */
const STARTS = ['Sandbox', 'Production'];

type Draft = Record<string, string>;

const show = (value: number, bytes?: boolean) =>
  bytes ? String(Math.round((value / MB) * 100) / 100) : String(value);

function toDraft(limits: AdminLimits): Draft {
  const draft: Draft = { defaultTier: limits.defaultTier };

  for (const { key: tier } of TIERS) {
    for (const limit of TIER) {
      const value = limits[tier][limit.key];

      draft[`${tier}.${limit.key}`] = limit.flag ? String(value === true) : limit.text ? String(value ?? '') : show(value as number, limit.bytes);
    }
  }

  for (const limit of [...LIFETIME, ...EVERY_LAMBDA, ...EVERYBODY]) {
    draft[limit.key] = show(limits[limit.key as keyof AdminLimits] as number, limit.bytes);
  }

  return draft;
}

/** What a field says, as the number to send, or nothing when it is no number the server takes. */
function parse(text: string, bytes?: boolean): number | null {
  const value = Number(text.trim());

  if (text.trim() === '' || !Number.isFinite(value) || value <= 0) {
    return null;
  }

  if (bytes) {
    return Math.round(value * MB);
  }

  return Number.isInteger(value) ? value : null;
}

/** Whether a field holds what the server takes for it. */
const valid = (limit: Limit, text: string) =>
  limit.flag ? true : limit.text ? text.trim().length <= 24 : parse(text, limit.bytes) !== null;

function fromDraft(draft: Draft): AdminLimits | null {
  const read = (key: string, bytes?: boolean) => parse(draft[key] ?? '', bytes);

  const tier = (name: Tier): TierLimits | null => {
    if (TIER.some((limit) => !valid(limit, draft[`${name}.${limit.key}`] ?? ''))) return null;

    const value = (key: string) => draft[`${name}.${key}`] ?? '';

    return {
      buildBytes: read(`${name}.buildBytes`, true)!,
      dataBytes: read(`${name}.dataBytes`, true)!,
      analyticsDays: read(`${name}.analyticsDays`)!,
      connectivity: value('connectivity') === 'true',
      label: value('label').trim() === '' ? null : value('label').trim(),
    };
  };

  const tiers = TIERS.map(({ key }) => tier(key));
  const others = [...LIFETIME, ...EVERY_LAMBDA, ...EVERYBODY];
  const rest = others.map((limit) => read(limit.key, limit.bytes));

  if (tiers.some((t) => t === null) || rest.some((v) => v === null) || !STARTS.includes(draft.defaultTier ?? '')) {
    return null;
  }

  return {
    defaultTier: draft.defaultTier,
    sandbox: tiers[0]!,
    production: tiers[1]!,
    system: tiers[2]!,
    ...Object.fromEntries(others.map((limit, i) => [limit.key, rest[i]])),
  } as AdminLimits;
}

/**
 * What a lambda may have: the limits of the tiers as a table with a column per
 * tier, and the limits that have no tier apart from it, in one column.
 */
export function LimitsSection({ access }: { access: Access }) {
  const { token, deny } = access;

  const toast = useToast();

  const [saved, setSaved] = useState<AdminLimits | null>(null);
  const [draft, setDraft] = useState<Draft>({});
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fail = useCallback((problem: unknown, fallback: string) => {
    if (refusedToken(problem)) {
      deny();
    } else {
      setError(fallback);
    }
  }, [deny]);

  useEffect(() => {
    api.admin.limits(token)
      .then((limits) => {
        setSaved(limits);
        setDraft(toDraft(limits));
      })
      .catch((problem) => fail(problem, 'The limits could not be read.'));
  }, [token, fail]);

  const original = useMemo(() => (saved === null ? {} : toDraft(saved)), [saved]);

  const changed = (key: string) => saved !== null && draft[key] !== original[key];
  const dirty = Object.keys(draft).some(changed);

  const parsed = fromDraft(draft);

  async function save() {
    if (parsed === null || saving) return;

    setSaving(true);

    try {
      const result = await api.admin.saveLimits(token, parsed);

      setSaved(result);
      setDraft(toDraft(result));

      toast('The limits are saved. They apply to the next save, upload, write, connection or sweep.', 'success');
    } catch (problem) {
      if (refusedToken(problem)) {
        deny();
      } else {
        toast(problem instanceof ApiError ? problem.message : 'The limits could not be saved.', 'error');
      }
    } finally {
      setSaving(false);
    }
  }

  const input = (limit: Limit, key: string, column?: string) => (
    <Input
      id={`limit-${key.replace('.', '-')}`}
      label={column ? `${limit.title}, ${column}` : limit.title}
      column={column}
      limit={limit}
      value={draft[key] ?? ''}
      changed={changed(key)}
      valid={valid(limit, draft[key] ?? '')}
      onChange={(value) => setDraft((current) => ({ ...current, [key]: value }))}
    />
  );

  return (
    <Section
      title="Limits"
      hint="What a lambda may have, per tier and for every lambda. A change applies to what is checked next - a save, an upload, a write, a connection to a database, the next sweep - and takes nothing away that a lambda already has. No restart needed."
    >
      {saved === null ? (
        error !== null ? (
          <p className="text-sm text-red-500">{error}</p>
        ) : (
          <div className="flex items-center gap-2 text-sm text-slate-500">
            <IconSpinner /> Reading the limits…
          </div>
        )
      ) : (
        <form
          onSubmit={(event) => {
            event.preventDefault();
            void save();
          }}
          className="space-y-10"
        >
          <section>
            <h2 className="text-sm font-medium">Per tier</h2>
            <p className="mt-1 text-[13px] text-slate-500">
              What one lambda may have, by what it is for: the sandbox to try something, production for an app people use, system
              for what this installation depends on - never blocked, never in the showcase, not deleted with its editor key. A
              demo is held to the sandbox; production never to less than the sandbox.
            </p>

            <div className="surface mt-3">
              <div className="hidden border-b border-grey-300 px-4 py-2.5 text-xs font-medium uppercase tracking-wide text-slate-500 dark:border-ink-800 sm:grid sm:grid-cols-[1fr_9rem_9rem_9rem] sm:gap-4">
                <span>Limit</span>
                {TIERS.map((tier) => <span key={tier.key}>{tier.name}</span>)}
              </div>

              <div className="divide-y divide-grey-300 dark:divide-ink-800">
                {TIER.map((limit) => (
                  <Row key={limit.key} limit={limit}>
                    {TIERS.map((tier) => <Fragment key={tier.key}>{input(limit, `${tier.key}.${limit.key}`, tier.name)}</Fragment>)}
                  </Row>
                ))}

                {LIFETIME.map((limit) => (
                  <Row key={limit.key} limit={limit}>
                    {input(limit, limit.key, 'Sandbox')}
                    <Kept tier="Production" />
                    <Kept tier="System" />
                  </Row>
                ))}
              </div>
            </div>
          </section>

          <section>
            <h2 className="text-sm font-medium">For every lambda</h2>
            <p className="mt-1 text-[13px] text-slate-500">
              The same whatever its tier.
            </p>

            <div className="surface mt-3 divide-y divide-grey-300 dark:divide-ink-800">
              <Row limit={{ key: 'defaultTier', title: 'New lambdas start in', unit: '', note: 'The sandbox where strangers create them; production on an installation run for a team, where nothing should go offline by itself.' }} single>
                <div className="min-w-0">
                  <div className="relative">
                    <select
                      id="limit-defaultTier"
                      aria-label="New lambdas start in"
                      value={draft.defaultTier ?? ''}
                      onChange={(event) => setDraft((current) => ({ ...current, defaultTier: event.target.value }))}
                      className="field py-1.5 text-sm"
                    >
                      {STARTS.map((tier) => <option key={tier} value={tier}>{tier}</option>)}
                    </select>
                    {changed('defaultTier') && <span className="pointer-events-none absolute inset-y-0 right-8 flex items-center"><span className="h-1.5 w-1.5 rounded-full bg-amber-500" title="Changed, not saved yet" /></span>}
                  </div>
                </div>
              </Row>

              {EVERY_LAMBDA.map((limit) => (
                <Row key={limit.key} limit={limit} single>
                  {input(limit, limit.key)}
                </Row>
              ))}
            </div>
          </section>

          <section>
            <h2 className="text-sm font-medium">For everybody</h2>
            <p className="mt-1 text-[13px] text-slate-500">
              Counted per visitor rather than per lambda, so there is no tier to them.
            </p>

            <div className="surface mt-3 divide-y divide-grey-300 dark:divide-ink-800">
              {EVERYBODY.map((limit) => (
                <Row key={limit.key} limit={limit} single>
                  {input(limit, limit.key)}
                </Row>
              ))}
            </div>
          </section>

          <div className="sticky bottom-0 -mx-4 flex flex-wrap items-center gap-3 border-t border-grey-300 bg-white/95 px-4 py-3 backdrop-blur dark:border-ink-800 dark:bg-ink-950/95 md:mx-0 md:px-0">
            <button type="submit" disabled={!dirty || parsed === null || saving} className="btn-primary">
              {saving && <IconSpinner />}
              Save limits
            </button>

            {dirty && (
              <button type="button" onClick={() => setDraft(original)} disabled={saving} className="btn-ghost">
                Discard changes
              </button>
            )}

            <span className="text-[13px] text-slate-500">
              {parsed === null
                ? 'Every limit is a whole number greater than zero; sizes may have decimals, and a label is at most 24 characters.'
                : dirty
                  ? 'Not saved yet.'
                  : 'Everything is saved.'}
            </span>
          </div>
        </form>
      )}
    </Section>
  );
}

/**
 * One limit: what it is on the left, and on the right a value per tier, or a
 * single one in the second column for a limit that has no tier.
 */
function Row({ limit, single = false, children }: { limit: Limit; single?: boolean; children: ReactNode }) {
  return (
    <div className="grid gap-3 p-4 sm:grid-cols-[1fr_9rem_9rem_9rem] sm:items-center sm:gap-4">
      <div className="min-w-0">
        <p className="text-[15px] font-medium">{limit.title}</p>
        <p className="mt-1 text-[13px] text-slate-500">{limit.note}</p>
      </div>

      <div className={`grid gap-3 sm:contents ${single ? 'grid-cols-1' : 'grid-cols-3'}`}>{children}</div>
    </div>
  );
}

/** Where a tier has no such limit: its lambdas stay online and are kept. */
function Kept({ tier }: { tier: string }) {
  return (
    <div>
      <span className="mb-1 block text-xs text-slate-500 sm:hidden">{tier}</span>
      <p className="py-1.5 text-[13px] text-slate-500">never - kept online</p>
    </div>
  );
}

function Input({ id, label, column, limit, value, changed, valid, onChange }: {
  id: string;
  label: string;
  /** The tier the column is for, said above the field where the columns have no header - on a phone. */
  column?: string;
  limit: Limit;
  value: string;
  changed: boolean;
  valid: boolean;
  onChange: (value: string) => void;
}) {
  const unit = limit.unit;
  const hours = unit === 'hours' ? Number(value) : NaN;

  if (limit.flag) {
    return (
      <div className="min-w-0">
        {column && <span className="mb-1 block text-xs text-slate-500 sm:hidden">{column}</span>}

        <label className="inline-flex cursor-pointer items-center gap-2 py-1.5 text-sm">
          <input
            id={id}
            type="checkbox"
            checked={value === 'true'}
            onChange={(event) => onChange(String(event.target.checked))}
            aria-label={label}
            className="h-4 w-4 accent-logo-600"
          />
          {value === 'true' ? 'yes' : 'no'}
          {changed && <span className="h-1.5 w-1.5 rounded-full bg-amber-500" title="Changed, not saved yet" />}
        </label>
      </div>
    );
  }

  return (
    <div className="min-w-0">
      {column && <label htmlFor={id} className="mb-1 block text-xs text-slate-500 sm:hidden">{column}</label>}

      <div className="relative">
        <input
          id={id}
          value={value}
          onChange={(event) => onChange(event.target.value)}
          inputMode={limit.text ? 'text' : unit === 'MB' ? 'decimal' : 'numeric'}
          autoComplete="off"
          aria-label={label}
          aria-invalid={!valid}
          placeholder={limit.text ? column : undefined}
          style={{ paddingRight: `${(unit === '' ? 0.75 : unit.length * 0.5 + 1.4) + (changed ? 0.75 : 0)}rem` }}
          className={`field py-1.5 text-sm ${limit.text ? '' : 'text-right font-mono'} ${valid ? '' : '!border-red-500 !ring-red-500'}`}
        />
        <span className="pointer-events-none absolute inset-y-0 right-3 flex items-center gap-1.5 text-xs text-slate-400">
          {changed && <span className="h-1.5 w-1.5 rounded-full bg-amber-500" title="Changed, not saved yet" />}
          {unit}
        </span>
      </div>

      {Number.isFinite(hours) && hours >= 48 && (
        <span className="mt-1 block text-right text-xs text-slate-400">{Math.round((hours / 24) * 10) / 10} days</span>
      )}
    </div>
  );
}
