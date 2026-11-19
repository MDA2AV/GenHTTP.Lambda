import { useCallback, useEffect, useRef, useState, type ReactNode } from 'react';
import { useLocation, useNavigate, useParams } from 'react-router-dom';

import { useAdminToken } from '../admin';
import { ApiError, api, refusedToken, type AdminOverview } from '@genhttp-lambda/kit/api';
import { IconLock, IconSpinner } from '@genhttp-lambda/kit/components/Icons';
import { PhoneNav, SideNav, type NavEntry, type NavGroup } from '@genhttp-lambda/kit/components/Navigation';
import { BlocksSection } from '../console/BlocksSection';
import { BrandingSection } from '../console/BrandingSection';
import { diskState, type Access } from '../console/context';
import { ConnectivitySection } from '../console/ConnectivitySection';
import { DashboardSection } from '../console/DashboardSection';
import { GuideSection } from '../console/GuideSection';
import { IntegrationsSection } from '../console/IntegrationsSection';
import { LambdaDetail } from '../console/LambdaDetail';
import { LambdasSection } from '../console/LambdasSection';
import { LimitsSection } from '../console/LimitsSection';
import { LogSection } from '../console/LogSection';
import { ServerSection } from '../console/ServerSection';
import { SettingsSection } from '../console/SettingsSection';
import { StatisticsSection } from '../console/StatisticsSection';
import { StorageSection } from '../console/StorageSection';
import { UsageReportSection } from '../console/UsageReportSection';
import { usePageMeta } from '@genhttp-lambda/kit/meta';
import { askPlatform } from '@genhttp-lambda/kit/site';
import type { Theme } from '@genhttp-lambda/kit/theme';

type SectionId = 'dashboard' | 'statistics' | 'lambdas' | 'connectivity' | 'blocks' | 'server' | 'storage' | 'log' | 'settings' | 'limits' | 'branding'
  | 'integrations' | 'usage' | 'guide';

/*
 * The sections in groups, as the editor has them, because a dozen in one
 * list is a list nobody reads. The dashboard first, under no heading: what
 * waits and how the installation is doing, where the operator starts. Then
 * the lambdas - counted, and every one of them. Then what keeps the
 * installation safe from what runs on it: the lambdas that ask to connect
 * out, and the addresses refused. Then how the machine is doing: the
 * process, the room on its volume, and what it logs. Then how the operator
 * set the installation up: the site's switches, the name and logo it shows -
 * where its host lets them give it some - and what it keeps its data in, as
 * its host chose. Then what the platform
 * holds the lambdas to and tells the agents that build them: the limits,
 * and the guide with the operator's own rules in it. Last, what fits no
 * group: the report the installation sends about itself.
 */
const GROUPS: { id: string; label: string | null; sections: { id: SectionId; label: string; note: string }[] }[] = [
  { id: 'start', label: null, sections: [
    { id: 'dashboard', label: 'Dashboard', note: 'What waits for you, and how the installation is doing' },
  ] },
  { id: 'lambdas', label: 'Lambdas', sections: [
    { id: 'statistics', label: 'Statistics', note: 'The lambdas counted: their states, tiers and what they use' },
    { id: 'lambdas', label: 'All lambdas', note: 'Every lambda on the installation, to narrow and open' },
  ] },
  { id: 'security', label: 'Security', sections: [
    { id: 'connectivity', label: 'Connectivity', note: 'Lambdas asking to connect to other servers' },
    { id: 'blocks', label: 'Blocked addresses', note: 'Addresses refused before anything else, for two weeks' },
  ] },
  { id: 'performance', label: 'Performance', sections: [
    { id: 'server', label: 'Server', note: 'Memory, connections and what the engine is doing' },
    { id: 'storage', label: 'Storage', note: 'The room left, and what takes it' },
    { id: 'log', label: 'Log', note: 'What the server and the lambdas are printing, live' },
  ] },
  { id: 'configuration', label: 'Configuration', sections: [
    { id: 'settings', label: 'Settings', note: 'What the site offers, switched without a restart' },
    { id: 'branding', label: 'Branding', note: 'The name and the logo the pages show in place of GenHTTP Lambda' },
    { id: 'integrations', label: 'Integrations', note: 'What the database, the files and the secrets are kept in' },
  ] },
  { id: 'platform', label: 'Platform', sections: [
    { id: 'limits', label: 'Limits', note: 'What a lambda may have in its tier, and for every lambda' },
    { id: 'guide', label: 'Guide', note: 'What agents read first, with rules of your own' },
  ] },
  { id: 'misc', label: 'Misc', sections: [
    { id: 'usage', label: 'Usage report', note: 'What the installation reports about itself once a day' },
  ] },
];

const SECTIONS = GROUPS.flatMap((group) => group.sections);

const pathOf = (id: SectionId) => (id === 'dashboard' ? '/admin' : `/admin/${id}`);

/**
 * Everything an operator can see, laid out the way the editor is: the
 * sections down a sidebar in their groups - a bar that opens them all on a
 * phone - and one of them beside it.
 *
 * Nothing is shown before the token is. Every section asks the server for it
 * as well, so this is a door rather than a disguise - hiding the sections
 * while the API answered anyone who asked would be worth nothing.
 */
export function Admin({ theme }: { theme: Theme }) {
  usePageMeta({ title: 'Admin', index: false });

  const { '*': rest = '' } = useParams();
  const navigate = useNavigate();
  const location = useLocation();

  const [segment, detail] = rest.split('/');
  // the server's figures were what the panel opened on, before there was a dashboard
  const section: SectionId = SECTIONS.find((s) => s.id === segment)?.id ?? 'dashboard';

  const [token, setToken] = useAdminToken();
  const [refused, setRefused] = useState(false);

  const scroller = useRef<HTMLDivElement>(null);

  /*
   * The installation at a glance, read again whenever the operator goes
   * elsewhere in the panel or decides something, rather than on a timer: the
   * operator is the one who changes most of it, and what owners and visitors
   * change the next look shows. It marks the sections that want a look.
   */
  const [overview, setOverview] = useState<AdminOverview | null>(null);
  const [asked, setAsked] = useState(0);

  // whether the host of this installation lets its operator brand it, which
  // decides whether there is a section for it
  const [brandable, setBrandable] = useState(false);

  useEffect(() => {
    askPlatform()
      .then((platform) => setBrandable(platform.brand.changeable))
      .catch(() => undefined);
  }, []);

  useEffect(() => {
    if (token === '') {
      return;
    }

    let current = true;

    api.admin.overview(token)
      .then((read) => current && setOverview(read))
      .catch(() => undefined);

    return () => {
      current = false;
    };
  }, [token, location.pathname, asked]);

  useEffect(() => {
    scroller.current?.scrollTo({ top: 0 });
  }, [segment, detail]);

  // a token that stops working - the host restarted with another one - locks
  // the whole panel again rather than leaving each section to say so
  const deny = useCallback(() => {
    setRefused(true);
    setToken('');
  }, [setToken]);

  const changed = useCallback(() => setAsked((n) => n + 1), []);

  if (token === '') {
    return (
      <Unlock
        refused={refused}
        onUnlocked={(entered) => {
          setRefused(false);
          setToken(entered);
        }}
      />
    );
  }

  const access: Access = { token, deny, dark: theme === 'dark', overview, changed };

  const groups: NavGroup[] = GROUPS.map((group) => ({
    id: group.id,
    label: group.label,
    entries: group.sections
      .filter((item) => item.id !== 'branding' || brandable)
      .map((item) => ({ id: item.id, to: pathOf(item.id), label: item.label, ...mark(item.id, overview) })),
  }));

  const opening = (entry: NavEntry) => {
    if (entry.id === section && !detail) {
      // the section already open is read again from the top
      changed();
      scroller.current?.scrollTo({ top: 0 });
    }

    navigate(entry.to);
  };

  return (
    <div ref={scroller} className="min-h-0 flex-1 overflow-y-auto">
      <div className="mx-auto flex w-full max-w-[max(80rem,90%)] flex-col md:flex-row md:gap-6 md:px-6">
        <aside className="shrink-0 border-b border-slate-200 dark:border-ink-800 md:sticky md:top-0 md:flex md:max-h-[calc(100dvh-3.75rem)] md:w-56 md:flex-col md:self-start md:border-b-0">
          <div className="px-4 pb-3 pt-4 md:px-3 md:pt-6">
            <div className="flex items-start gap-2">
              <div className="min-w-0 flex-1">
                <span className="text-[15px] font-semibold">Administration</span>
                <p className="mt-1 text-[13px] text-slate-500">Unlocked in this browser</p>
              </div>

              <button
                type="button"
                onClick={() => setToken('')}
                className="flex h-8 w-8 items-center justify-center rounded-full text-slate-500 hover:bg-slate-100 hover:text-slate-900 dark:hover:bg-ink-850 dark:hover:text-slate-100"
                aria-label="Lock again"
                title="Lock again"
              >
                <IconLock className="h-4 w-4" />
              </button>
            </div>
          </div>

          <div className="hidden min-h-0 flex-1 overflow-y-auto pb-6 [scrollbar-width:thin] md:block">
            <SideNav label="Sections" groups={groups} current={section} onOpen={opening} />
          </div>
        </aside>

        {/* on a phone the sections are a bar of their own, which stays at the
            top of the page as it moves - outside the sidebar, which does not */}
        <PhoneNav
          label="Sections"
          groups={groups}
          current={section}
          title={SECTIONS.find((s) => s.id === section)?.label ?? 'Dashboard'}
          onOpen={opening}
          scroller={scroller}
        />

        <main className="flex min-w-0 flex-1 flex-col">
          {section === 'lambdas' && detail ? (
            <LambdaDetail key={detail} access={access} publicKey={decodeURIComponent(detail)} />
          ) : section === 'lambdas' ? (
            <LambdasSection access={access} />
          ) : section === 'statistics' ? (
            <StatisticsSection access={access} />
          ) : section === 'connectivity' ? (
            <ConnectivitySection access={access} />
          ) : section === 'blocks' ? (
            <BlocksSection access={access} />
          ) : section === 'server' ? (
            <ServerSection access={access} />
          ) : section === 'storage' ? (
            <StorageSection access={access} />
          ) : section === 'log' ? (
            <LogSection access={access} />
          ) : section === 'settings' ? (
            <SettingsSection access={access} />
          ) : section === 'limits' ? (
            <LimitsSection access={access} />
          ) : section === 'branding' ? (
            <BrandingSection access={access} />
          ) : section === 'integrations' ? (
            <IntegrationsSection access={access} />
          ) : section === 'usage' ? (
            <UsageReportSection access={access} />
          ) : section === 'guide' ? (
            <GuideSection access={access} />
          ) : (
            <DashboardSection access={access} />
          )}
        </main>
      </div>
    </div>
  );
}

/**
 * What is worth noticing about a section beside its name: what waits there
 * for the operator, as a count or a dot - the colour of how urgent it is.
 */
function mark(id: SectionId, overview: AdminOverview | null): Pick<NavEntry, 'marker' | 'calls'> {
  if (overview === null) {
    return {};
  }

  const { attention, failing } = overview;

  switch (id) {
    case 'connectivity':
      return attention.connectivityWaiting > 0
        ? { calls: 'wanted', marker: <Count tone="wanted" title={`${attention.connectivityWaiting} waiting for a decision`}>{attention.connectivityWaiting}</Count> }
        : {};
    case 'blocks':
      return attention.youAreBlocked
        ? { calls: 'problem', marker: <Dot tone="problem" title="Your own address is blocked" /> }
        : {};
    case 'storage': {
      const state = diskState(overview.disk);

      return state ? { calls: state === 'bad' ? 'problem' : 'wanted', marker: <Dot tone={state === 'bad' ? 'problem' : 'wanted'} title="The volume is running out of room" /> } : {};
    }
    case 'usage':
      return attention.usageReportFailure ? { calls: 'wanted', marker: <Dot tone="wanted" title="The last report did not arrive" /> } : {};
    case 'log':
      return failing.length > 0 ? { calls: 'problem', marker: <Dot tone="problem" title="Lambdas answered with server errors in the last hour" /> } : {};
    default:
      return {};
  }
}

function Count({ tone, title, children }: { tone?: 'wanted'; title: string; children: ReactNode }) {
  return (
    <span
      title={title}
      className={`rounded-full px-1.5 text-[11px] font-medium tabular-nums ${tone === 'wanted' ? 'bg-amber-500/15 text-amber-700 dark:text-amber-400' : 'text-slate-400'}`}
    >
      {children}
    </span>
  );
}

function Dot({ tone, title }: { tone: 'problem' | 'wanted'; title: string }) {
  return <span title={title} aria-label={title} className={`h-2 w-2 shrink-0 rounded-full ${tone === 'problem' ? 'bg-red-500' : 'bg-amber-500'}`} />;
}

/**
 * The door. The token is tried against the server before it is kept, so a
 * typo is answered here rather than by every section failing at once.
 */
function Unlock({ refused, onUnlocked }: { refused: boolean; onUnlocked: (token: string) => void }) {
  const [entered, setEntered] = useState('');
  const [working, setWorking] = useState(false);
  const [problem, setProblem] = useState<string | null>(
    refused ? 'That token is no longer accepted. Enter it again.' : null,
  );

  const field = useRef<HTMLInputElement>(null);

  useEffect(() => {
    field.current?.focus();
  }, []);

  async function submit(event: React.FormEvent) {
    event.preventDefault();

    const token = entered.trim();

    if (token === '') {
      return;
    }

    setWorking(true);
    setProblem(null);

    try {
      await api.admin.list(token);
      onUnlocked(token);
    } catch (error) {
      setProblem(
        error instanceof ApiError && error.status === 404
          ? 'This installation has no administration: it was started without an admin token.'
          : refusedToken(error)
            ? 'That token was not accepted.'
            : 'The server could not be asked. Try again in a moment.',
      );
    } finally {
      setWorking(false);
    }
  }

  return (
    <div className="flex min-h-0 flex-1 overflow-y-auto">
      <form onSubmit={submit} className="m-auto w-full max-w-sm px-5 py-16">
        <div className="flex h-11 w-11 items-center justify-center bg-slate-100 text-slate-500 dark:bg-ink-850">
          <IconLock className="h-5 w-5" />
        </div>

        <h1 className="mt-5 text-2xl font-bold tracking-tight">Administration</h1>

        <p className="mt-2 text-sm text-slate-600 dark:text-slate-400">
          The server figures, the log and the lambdas of other people are behind this. It is the token the host
          was started with, and this browser keeps it until it is locked again - for this tab only, where the
          lambdas answer on this address.
        </p>

        <input
          ref={field}
          type="password"
          value={entered}
          onChange={(event) => setEntered(event.target.value)}
          placeholder="Admin token"
          aria-label="Admin token"
          autoComplete="off"
          className="field mt-6 font-mono text-sm"
        />

        {problem && <p className="mt-2 text-xs text-red-500">{problem}</p>}

        <button type="submit" disabled={working || entered.trim() === ''} className="btn-primary mt-4 w-full py-2">
          {working && <IconSpinner />}
          Unlock
        </button>
      </form>
    </div>
  );
}
