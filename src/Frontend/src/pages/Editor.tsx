import { Fragment, useCallback, useEffect, useRef, useState } from 'react';
import { Link, useLocation, useNavigate, useParams } from 'react-router-dom';

import { useAdminToken } from '../admin';
import { absoluteAddress, isDomain, platformPath } from '../address';
import {
  ApiError,
  allowsDomain,
  api,
  isActive,
  isDemo,
  type ChangeJob,
  type Feature,
  type FeatureMerge,
  type Lambda,
  type LambdaFile,
  type LambdaSummary,
  type SourceSettings,
  type VersionInfo,
} from '../api';
import { CopyField } from '../components/CopyField';
import { Diagnostics } from '../components/Diagnostics';
import { Dialog } from '../components/Dialog';
import {
  IconAlert, IconCheck, IconCode, IconCopy, IconExternal, IconLock, IconPlay, IconSpark, IconSpinner, IconViewFull, IconViewSimple,
} from '../components/Icons';
import { useToast } from '../components/Toast';
import { AdminTab } from '../control/AdminTab';
import { useAgent } from '../control/agent';
import { ChangeTab } from '../control/ChangeTab';
import { CodeTab } from '../control/CodeTab';
import type { Busy, Control, FeatureControl, FeatureView, Rejection } from '../control/context';
import { ContextTab } from '../control/ContextTab';
import { DataTab } from '../control/DataTab';
import { DeploymentsTab } from '../control/DeploymentsTab';
import { DomainTab } from '../control/DomainTab';
import { BaseDialog, DeleteFeatureDialog, MergeDialog, NewFeatureDialog, NotesDialog } from '../control/FeatureDialogs';
import { FeaturesTab } from '../control/FeaturesTab';
import { FeatureTab } from '../control/FeatureTab';
import { HistoryTab } from '../control/HistoryTab';
import { LogsTab } from '../control/LogsTab';
import { StatsTab } from '../control/StatsTab';
import { ShowcaseTab } from '../control/ShowcaseTab';
import { SimpleOverview } from '../control/SimpleOverview';
import { SourceTab } from '../control/SourceTab';
import { SummaryTab } from '../control/SummaryTab';
import { VersionsTab } from '../control/VersionsTab';
import { Menu, StatusBadge, TierBadge, menuItem, menuRule } from '../control/ui';
import { useView, type View } from '../control/view';
import { SharedWordsContext } from '../control/words';
import { useEditorT, useT } from '../i18n';
import { Link as SiteLink } from '../i18n/links';
import { registerCompletions, registerResolver, registerSemantics } from '../monaco';
import type { Theme } from '../theme';
import { usePageMeta } from '../meta';

interface Props {
  theme: Theme;
}

type SectionId =
  | 'overview' | 'docs' | 'change' | 'features' | 'code' | 'tests' | 'data' | 'versions' | 'deployments' | 'stats'
  | 'logs' | 'showcase' | 'source' | 'domain' | 'history' | 'admin';

/*
 * Where the sections that went into Code used to be - its files, what it was
 * built from - and the old address of the code itself, so a link kept from
 * before still opens what it showed.
 */
const MOVED: Record<string, SectionId> = { edit: 'code', files: 'code', build: 'code' };

type GroupId = 'develop' | 'program' | 'run' | 'sharing';

/*
 * The full view's sections, in groups, because fifteen of them in one list
 * is a list nobody reads: the groups are what somebody scans for, and the
 * section is found inside the one it belongs to. Every section stays one
 * click away - folding some behind pills in others would have made the list
 * shorter by making the log, which developers open all the time, further
 * away.
 *
 * The overview and the documentation first, under no heading: how the
 * lambda is doing, and what it is and why - where anybody starts, a stranger
 * reading a demo included. Then how people find it and what they get to see
 * of it: the showcase, its published source and its domain - right under
 * what the lambda is, since those are what it is to everybody else. Then
 * where a change is made: Change and the drafts, the code - every file of a
 * version, its resources included - and the tests that say whether a change
 * works. Then the program and what it keeps, side by side - the data of the
 * lambda, and the versions that all share it. Then how it runs.
 */
const GROUPS: { id: GroupId | null; sections: SectionId[] }[] = [
  { id: null, sections: ['overview', 'docs'] },
  { id: 'sharing', sections: ['showcase', 'source', 'domain'] },
  { id: 'develop', sections: ['change', 'features', 'code', 'tests'] },
  { id: 'program', sections: ['data', 'versions'] },
  { id: 'run', sections: ['deployments', 'stats', 'logs'] },
];

const SECTIONS: SectionId[] = GROUPS.flatMap((group) => group.sections);

/*
 * What a feature is worked on with: what it is, what is written about it,
 * its code, its tests, its copy of the data and what its preview said.
 * The rest - the showcase, the domain, the figures, the deployments -
 * belongs to the lambda, which the feature leaves alone until it is put
 * online.
 */
const FEATURE_VIEWS: FeatureView[] = ['overview', 'docs', 'code', 'tests', 'data', 'logs'];

/** The views something can be written and left unsaved in. */
const WRITABLE = ['code', 'docs', 'tests'];

/*
 * What the simple view keeps: the app, what it is for, asking for a change,
 * the drafts a change can leave to be tried, what changed so far, what the
 * app keeps, and how people find it. What it is for right after the
 * overview, as in the full view - it is what the agent understood from what
 * was asked, which is worth checking - and Change after that, since asking
 * for one is what the simple view is for. Its tests are not there: they are
 * the agent's to keep and to run. Nothing that is about the code - its
 * files, its deployments, its log - and not the figures either, which the
 * overview sums up in the two that matter. The versions are there, as the
 * history: the changes the app went through and a way back to any of them,
 * without the files to compare. The data is there once the app keeps any -
 * what it saved, and the keys it uses - and not while there is nothing in it.
 * Publishing its code is there, next to the showcase: it is the owner's to
 * decide whoever built the app, and said in the same words in both views.
 */
const SIMPLE_SECTIONS: SectionId[] = ['overview', 'docs', 'change', 'features', 'history', 'data', 'showcase', 'source', 'domain'];

/** A draft, in the simple view, is what it does and where to try it - not its code, its data or its log. */
const SIMPLE_FEATURE_VIEWS: FeatureView[] = ['overview'];

/*
 * What only the operator decides about a lambda - whether the sitemap names
 * it - in both views, set apart after every section of the owner's. There
 * only for a browser holding the admin token the panel asked for, so the
 * owner never sees it; the server asks for the token again.
 */
const ADMIN_SECTIONS: SectionId[] = ['admin'];

/**
 * The control center of one lambda.
 *
 * Laid out for the person who owns it rather than the one typing it: most of
 * the code here is written by an agent. The sidebar is the lambda - whether
 * it is online, where, and its sections; the page beside it is one section at
 * a time. Everything done rarely sits behind one menu, so what is left on the
 * screen is what is worth looking at.
 *
 * Opened on a feature - /features/{key}/{view} - the frame is the feature's:
 * the sidebar holds it, with where it can be tried and the buttons that try
 * it and put it online, and the views are its own code, its copy of the data
 * and what its preview said. The lambda stays at the top, so it is always
 * clear whose feature it is.
 *
 * The owner reads of drafts rather than features: somebody who had an app
 * built knows what a draft is, and does not need to know what merging is.
 * Putting one online merges it and deploys the version it becomes, in one.
 */
export function Editor({ theme }: Props) {
  const t = useEditorT();
  const said = t.frame;
  const site = useT();

  // whoever unlocked the panel in this browser
  const [adminToken] = useAdminToken();
  const operator = adminToken !== '';

  usePageMeta({ title: said.title, index: false });

  const { privateKey = '', '*': rest = '' } = useParams();
  const navigate = useNavigate();
  const location = useLocation();
  const toast = useToast();

  const parts = rest.split('/');
  const segment = parts[0];

  // a feature, opened at one of its views
  const featureKey = segment === 'features' && parts[1] ? parts[1] : null;
  const featureView: FeatureView = parts[2] === 'build' ? 'code' : (FEATURE_VIEWS.find((view) => view === parts[2]) ?? 'overview');

  const section: SectionId = MOVED[segment]
    ?? ([...SECTIONS, ...SIMPLE_SECTIONS, ...ADMIN_SECTIONS].find((id) => id === segment) ?? 'overview');

  // the kind of data open: /data/secrets, or /features/{key}/data/secrets
  const dataKind = featureKey ? (featureView === 'data' ? parts[3] : undefined) : section === 'data' ? parts[1] : undefined;

  const [lambda, setLambda] = useState<Lambda | null>(null);
  const { view, choose } = useView(lambda?.publicKey, lambda?.view);
  const simple = view === 'simple';
  const [summary, setSummary] = useState<LambdaSummary | null>(null);
  const [versions, setVersions] = useState<VersionInfo[]>([]);
  const [features, setFeatures] = useState<Feature[] | null>(null);
  /** Whether its code is published, for the link to where it is read. */
  const [source, setSource] = useState<SourceSettings | null>(null);
  const [failure, setFailure] = useState<string | null>(null);
  const [busy, setBusy] = useState<Busy>(null);
  const [rejection, setRejection] = useState<Rejection | null>(null);
  const [renaming, setRenaming] = useState(false);
  const [removing, setRemoving] = useState(false);

  /** What a new feature is to start from, while the dialog for one is open. */
  const [creating, setCreating] = useState<{ base?: number; files?: LambdaFile[] } | null>(null);
  const [featureDialog, setFeatureDialog] = useState<'notes' | 'base' | 'delete' | null>(null);

  /** The feature being put online, while the dialog for that is open - from its own page, or from the change that left it. */
  const [merging, setMerging] = useState<string | null>(null);
  const [previewing, setPreviewing] = useState(false);

  /** Whether the code view holds something unsaved, so leaving it can ask first. */
  const dirty = useRef(false);

  /** The same, for what is drawn: the sidebar's preview and merge use what is saved, not what is typed. */
  const [unsaved, setUnsaved] = useState(false);

  const onDirty = useCallback((value: boolean) => {
    dirty.current = value;
    setUnsaved(value);
  }, []);

  // the creation page says so in the router state
  const fresh = (location.state as { created?: boolean } | null)?.created === true;
  const [welcome, setWelcome] = useState(fresh);

  const base = `/editor/${privateKey}`;

  // completions come from the server, so they always match what compiles
  useEffect(() => {
    api
      .platform()
      .then((platform) => {
        registerCompletions(platform.completions);
      })
      .catch(() => undefined);
  }, []);

  // the compiler colours this lambda's code, so the provider needs its key
  useEffect(() => {
    registerSemantics(async (code) => (await api.semantics(privateKey, code)).tokens);

    registerResolver(async (code, line, column) =>
      (await api.completions(privateKey, code, line, column)).completions);
  }, [privateKey]);

  /*
   * Read on an interval and after every action, so two reads can be under
   * way at once; only one newer than the last applied is applied, or a slow
   * poll would bring back what an action had just changed.
   */
  const issued = useRef(0);
  const applied = useRef(0);

  const refresh = useCallback(async () => {
    const mine = ++issued.current;

    const [current, history, figures, open, published] = await Promise.all([
      api.get(privateKey),
      api.versions(privateKey),
      api.summary(privateKey).catch(() => null),
      // one that could not be read is not one that is gone: the last list stays
      api.feature.list(privateKey).catch(() => null),
      api.source(privateKey).catch(() => undefined),
    ]);

    if (mine < applied.current) {
      return;
    }

    applied.current = mine;

    setLambda(current);
    setVersions(history);

    if (open) {
      setFeatures(open);
    }

    if (figures) {
      setSummary(figures);
    }

    if (published !== undefined) {
      setSource(published.source ?? null);
    }
  }, [privateKey]);

  useEffect(() => {
    let alive = true;

    refresh().catch((error) => {
      if (alive) {
        setFailure(error instanceof ApiError ? error.message : said.loadFailed);
      }
    });

    return () => {
      alive = false;
    };
  }, [refresh, said]);

  /*
   * An agent can deploy while this is open, so the lambda is read again every
   * so often - more often on the overview, not at all while the page is
   * hidden.
   */
  useEffect(() => {
    // the overviews are where a change by somebody else shows first
    const every = section === 'overview' || (featureKey != null && featureView === 'overview') ? 10_000 : 30_000;

    const timer = window.setInterval(() => {
      if (document.visibilityState === 'visible') {
        refresh().catch(() => undefined);
      }
    }, every);

    return () => window.clearInterval(timer);
  }, [refresh, section, featureKey, featureView]);

  /*
   * Opening a section is asking how things are now. The sections that load
   * their own figures do so as they open; the ones that show what the frame
   * holds - the overview, the versions - would otherwise show what it held
   * when it was last read, which after a few requests is plainly wrong. The
   * same goes for coming back to the tab after a while somewhere else.
   */
  const opened = useRef(false);

  useEffect(() => {
    if (!opened.current) {
      // the first read is the load above
      opened.current = true;
      return;
    }

    refresh().catch(() => undefined);
  }, [refresh, section, featureKey, featureView]);

  useEffect(() => {
    const back = () => document.visibilityState === 'visible' && refresh().catch(() => undefined);

    document.addEventListener('visibilitychange', back);

    return () => document.removeEventListener('visibilitychange', back);
  }, [refresh]);

  const deploy = useCallback(
    async (version?: number) => {
      setBusy('deploy');

      try {
        const result = await api.deploy(privateKey, version);

        await refresh();

        if (!result.success) {
          setRejection({ version, diagnostics: result.diagnostics });
          return false;
        }

        toast(simple ? t.simple.deployed : said.online(result.lambda?.activeVersion ?? version ?? ''), 'success');
        return true;
      } catch (error) {
        toast(error instanceof ApiError ? error.message : said.deployFailed, 'error');
        return false;
      } finally {
        setBusy(null);
      }
    },
    [privateKey, refresh, toast, said, simple, t],
  );

  /** Puts what the open feature holds online at its preview address, saying how it went. */
  const previewFeature = useCallback(async () => {
    if (!featureKey) {
      return false;
    }

    setPreviewing(true);

    try {
      const result = await api.feature.preview(privateKey, featureKey);

      await refresh();

      if (!result.success) {
        setRejection({ feature: featureKey, diagnostics: result.diagnostics });
        return false;
      }

      toast(t.features.previewDeployed, 'success');
      return true;
    } catch (error) {
      toast(error instanceof ApiError ? error.message : t.features.previewFailed, 'error');
      return false;
    } finally {
      setPreviewing(false);
    }
  }, [privateKey, featureKey, refresh, toast, t]);

  const undeploy = useCallback(async () => {
    setBusy('undeploy');

    try {
      setLambda(await api.undeploy(privateKey));
      await refresh();
      toast(said.offline);
    } catch (error) {
      toast(error instanceof ApiError ? error.message : said.offlineFailed, 'error');
    } finally {
      setBusy(null);
    }
  }, [privateKey, refresh, toast, said]);

  /** Said once when a change this page watched comes to an end, wherever the owner is. */
  const finished = useCallback(
    (job: ChangeJob) => {
      // the simple view has no versions to name
      const words = simple
        ? { ...t.change.toast, online: t.simple.results.online, saved: t.simple.results.saved }
        : t.change.toast;
      const result = job.result;

      if (job.state === 'cancelled') {
        toast(words.stopped);
      } else if (result?.ok && result.online != null) {
        toast(words.online(result.online));
      } else if (result?.ok && result.version == null && result.feature) {
        const name = result.featureName ?? result.feature.slice(0, 8);

        toast(result.compiles === false ? t.change.results.featureBroken(name) : words.feature(name), result.compiles === false ? 'error' : 'success');
      } else if (result?.ok && result.version != null) {
        toast(words.saved(result.version), result.compiles === false ? 'error' : 'success');
      } else if (result?.unchanged && job.state === 'done') {
        toast(words.unchanged);
      } else {
        toast(words.failed, 'error');
      }
    },
    [t, toast, simple],
  );

  const agent = useAgent({
    privateKey,
    // a demo is changed by nobody, the agent included
    enabled: lambda != null && !isDemo(lambda.tier),
    watching: section === 'change',
    refresh,
    onFinished: finished,
    failed: t.change.readFailed,
  });

  // where the code being edited is: the lambda's, or the open feature's
  const coding = featureKey ? (featureView === 'code' ? `${base}/features/${featureKey}/code` : null) : section === 'code' ? `${base}/code` : null;

  // where something can be left unsaved: the code, or a page of the documentation or the tests
  const editing = featureKey
    ? (WRITABLE.includes(featureView) ? `${base}/features/${featureKey}/${featureView}` : null)
    : WRITABLE.includes(section) ? `${base}/${section}` : null;

  const go = useCallback(
    (to: string) => {
      if (editing && dirty.current && !to.startsWith(editing) && !window.confirm(said.leave)) {
        return false;
      }

      dirty.current = false;
      setUnsaved(false);
      navigate(to);

      return true;
    },
    [editing, navigate, said],
  );

  /*
   * A domain of its own is part of the premium tier, so the section is not
   * there for any other: a link to it, or a lambda moved out of the tier
   * while it was open, lands on the overview instead.
   */
  const hidden = lambda != null && !allowsDomain(lambda.tier);

  // a demo is read by anybody holding its announced key, and changed by nobody
  const demo = lambda != null && isDemo(lambda.tier);

  // what the lambda keeps as data, and the secrets its code waits for
  const holding = summary?.storage;
  const missingSecrets = holding?.missingSecrets ?? [];
  const keeps = holding != null
    && (holding.databaseTables > 0 || holding.workspaceFiles > 0 || holding.secrets > 0 || missingSecrets.length > 0);

  // the code connects to a database that is switched off - the agent's to
  // switch on, so the simple view is not asked about it
  const databaseWanted = !simple && holding != null && holding.usesDatabase && !holding.databaseEnabled;

  // sections that only change the lambda, which a demo does not have - and
  // the drafts, while there are none: they are met through a change the agent
  // leaves to be tried, or started from a version or the code, and the list
  // of them is nothing to look at until then
  const absent = (id: SectionId) =>
    (hidden && id === 'domain')
    || (demo && (id === 'showcase' || id === 'source' || id === 'change' || id === 'features'))
    || (id === 'features' && section !== 'features' && (features?.length ?? 0) === 0)
    // the simple view has the data once the app keeps any, or waits for a key
    || (id === 'data' && simple && section !== 'data' && !keeps)
    // the full view has the versions for it - known once the lambda is,
    // which says which view it opens in
    || (id === 'history' && lambda != null && !simple)
    // the operator's, for nobody without the admin token
    || (ADMIN_SECTIONS.includes(id) && !operator);

  const away = absent(section);

  // whether the simple view has what is open; when it does not - come to by
  // a link, say - the page says so rather than pretending it is not there
  const kept = featureKey
    ? SIMPLE_FEATURE_VIEWS.includes(featureView)
    : SIMPLE_SECTIONS.includes(section) || ADMIN_SECTIONS.includes(section);
  const outside = simple && !kept;

  /**
   * Switches the view, leaving what the simple view does not have for the
   * overview - or the draft's - and the history for the versions, which are
   * what it is in the full view.
   */
  const switchTo = (next: View) => {
    if (next === 'simple' && !kept && !go(featureKey ? `${base}/features/${featureKey}` : base)) {
      return;
    }

    if (next === 'full' && !featureKey && section === 'history') {
      go(`${base}/versions`);
    }

    choose(next);
  };

  // the feature the page is opened on, as last read; undefined until the list arrives
  const open = featureKey && features ? (features.find((f) => f.key === featureKey) ?? null) : undefined;

  // merged or deleted elsewhere while its code was open: nothing is left to save
  const gone = featureKey != null && open === null;

  useEffect(() => {
    if (gone) {
      dirty.current = false;
      setUnsaved(false);
    }
  }, [gone]);

  useEffect(() => {
    if (away) {
      navigate(base, { replace: true });
    }
  }, [away, base, navigate]);

  /** What scrolls: the page beside the sidebar, unless the code fills it. */
  const scroller = useRef<HTMLDivElement>(null);

  /*
   * A section opened is a page opened, so it starts at the top. Otherwise one
   * opened from the foot of a sidebar taller than the window - the operator's
   * is last - shows the space below it, and a short one looks empty.
   */
  useEffect(() => {
    scroller.current?.scrollTo({ top: 0 });
  }, [section, featureKey, featureView]);

  if (failure) {
    return (
      <div className="mx-auto flex w-full max-w-xl flex-col items-start px-5 py-24">
        <div className="flex h-11 w-11 items-center justify-center bg-red-500/10 text-red-500">
          <IconAlert className="h-5 w-5" />
        </div>
        <h1 className="mt-5 text-2xl font-bold tracking-tight">{said.nothingTitle}</h1>
        <p className="mt-3 text-[15px] text-slate-600 dark:text-slate-400">{failure}</p>
        <button type="button" onClick={() => navigate('/editor/create')} className="btn-primary mt-8 px-5 py-2.5">
          {said.createNew}
        </button>
      </div>
    );
  }

  if (!lambda) {
    return (
      <div className="flex flex-1 items-center justify-center gap-2 text-sm text-slate-500">
        <IconSpinner />
        {said.loading}
      </div>
    );
  }

  const featureControl: FeatureControl | null = open
    ? { info: open, refresh, preview: previewFeature, previewing }
    : null;

  const featurePath = (key: string, view: FeatureView = 'overview') => `${base}/features/${key}${view === 'overview' ? '' : `/${view}`}`;

  const control: Control = {
    privateKey,
    lambda,
    summary,
    versions,
    features: features ?? [],
    busy,
    theme,
    refresh,
    deploy,
    undeploy,
    edit: (version, file) => {
      const query = new URLSearchParams();

      if (version != null && !(open && version == null)) {
        query.set('version', String(version));
      }

      if (file) {
        query.set('file', file);
      }

      const search = query.toString() ? `?${query}` : '';

      go(open && version == null ? `${featurePath(open.key, 'code')}${search}` : `${base}/code${search}`);
    },
    openContext: (area, version) =>
      go(open && version == null ? featurePath(open.key, area) : `${base}/${area}${version != null ? `?version=${version}` : ''}`),
    agent,
    openData: (kind, set) => go(`${featureKey && open ? featurePath(featureKey, 'data') : `${base}/data`}${kind ? `/${kind}` : ''}${set ? `?set=${encodeURIComponent(set)}` : ''}`),
    startFeature: (from, files) => setCreating({ base: from, files }),
    openFeature: (key, view) => go(featurePath(key, view)),
    putOnline: (key) => setMerging(key),
    askAgent: (key, prompt) => {
      const query = new URLSearchParams();

      if (key) {
        query.set('feature', key);
      }

      if (prompt) {
        query.set('ask', prompt);
      }

      const search = query.toString();

      go(`${base}/change${search ? `?${search}` : ''}`);
    },
    feature: featureControl,
    simple,
  };

  /** A new feature is there: open it where work on it starts. */
  const created = (feature: Feature, carried: boolean) => {
    setCreating(null);

    // known at once, so the page it opens does not first say it is missing
    setFeatures((was) => [feature, ...(was ?? []).filter((f) => f.key !== feature.key)]);
    refresh().catch(() => undefined);
    toast(t.features.created(feature.name), 'success');

    // what was typed in the code view is in the feature now, not lost
    onDirty(false);
    navigate(featurePath(feature.key, carried ? 'code' : 'overview'));
  };

  /** A feature became a version: show the version. */
  const merged = (result: FeatureMerge) => {
    setMerging(null);
    refresh().catch(() => undefined);

    const version = result.version?.version;

    if (result.deployment && !result.deployment.success) {
      setRejection({ version, diagnostics: result.deployment.diagnostics });
    } else {
      toast(result.deployment ? t.features.mergedOnline(version ?? '') : t.features.merged(version ?? ''), 'success');
    }

    navigate(`${base}/versions${version != null ? `?version=${version}` : ''}`);
  };

  const change = agent.state?.job;
  const changing = isActive(change);

  // the feature being put online, as last read - gone when it was merged or deleted meanwhile
  const putting = merging ? (features?.find((f) => f.key === merging) ?? null) : null;

  const live = lambda.activeVersion != null;
  const publicUrl = absoluteAddress(lambda.publicPath);
  const domainUrl = isDomain(lambda.address) ? lambda.address : null;
  const editorUrl = `${window.location.origin}${lambda.editorPath}`;
  const latest = lambda.latestVersion;
  const ahead = latest != null && latest !== lambda.activeVersion;
  const problems = (summary?.recentProblems.length ?? 0) > 0;

  const code = coding != null;

  /** One section in the sidebar, with what is worth noticing about it beside its name. */
  const entry = (id: SectionId) => {
    const to = id === 'overview' ? base : `${base}/${id}`;
    const current = section === id;

    return (
      <Link
        key={id}
        to={to}
        onClick={(event) => {
          event.preventDefault();

          // the section already open, asked for again, is asked for
          // as it is now
          if (current) {
            refresh().catch(() => undefined);
          } else {
            go(to);
          }
        }}
        aria-current={current ? 'page' : undefined}
        className={`flex shrink-0 items-center gap-2 whitespace-nowrap border-b-2 px-3 py-2 text-sm md:border-b-0 md:border-l-2 ${
          current
            ? 'border-accent-500 font-medium text-ink-900 dark:border-accent-400 dark:text-slate-100 md:bg-slate-100 md:dark:bg-ink-850'
            : 'border-transparent text-slate-600 hover:text-slate-900 dark:text-slate-400 dark:hover:text-slate-200'
        }`}
      >
        {id === 'admin' && <IconLock className="h-3.5 w-3.5 text-slate-400" />}
        {/* the header's word for the panel, which this belongs to */}
        {id === 'admin' ? site.shell.admin : simple && id === 'docs' ? t.simple.about : said.sections[id]}
        {id === 'versions' && versions.length > 0 && (
          <span className="ml-auto text-xs tabular-nums text-slate-400">{versions.length}</span>
        )}
        {id === 'features' && (features?.length ?? 0) > 0 && (
          <span className="ml-auto text-xs tabular-nums text-slate-400">{features!.length}</span>
        )}
        {id === 'logs' && problems && (
          <span className="ml-auto h-1.5 w-1.5 rounded-full bg-red-500" title={said.problems} />
        )}
        {id === 'data' && (missingSecrets.length > 0 || databaseWanted) && !demo && (
          <span className="ml-auto h-1.5 w-1.5 rounded-full bg-amber-500" title={missingSecrets.length > 0 ? t.data.missingDot : t.data.offDot} />
        )}
        {id === 'change' && changing && (
          <span className="ml-auto" title={said.changeRunning}>
            <IconSpinner className="h-3.5 w-3.5 text-accent-500 dark:text-accent-400" />
          </span>
        )}
        {id === 'change' && !changing && agent.unseen && (
          <span
            className={`ml-auto h-1.5 w-1.5 rounded-full ${change?.result?.ok ? 'bg-emerald-500' : 'bg-amber-500'}`}
            title={said.changeEnded}
          />
        )}
      </Link>
    );
  };

  /*
   * One centred column holding the sidebar and the section beside it, the
   * way a repository page is laid out, rather than two panes pinned to the
   * edges of a wide window. The page scrolls as a whole and the sidebar stays
   * where it is - except in the code, which is an editor and fills the height.
   */
  return (
    <SharedWordsContext.Provider value={t.shared}>
    <div ref={scroller} className={code ? 'flex min-h-0 flex-1 flex-col' : 'min-h-0 flex-1 overflow-y-auto'}>
    <div className={`mx-auto flex w-full max-w-[max(80rem,90%)] flex-col md:flex-row md:gap-6 md:px-6 ${code ? 'min-h-0 flex-1' : ''}`}>
      <aside className="shrink-0 border-b border-slate-200 dark:border-ink-800 md:sticky md:top-0 md:flex md:w-56 md:flex-col md:self-start md:border-b-0">
        <div className="px-4 pb-3 pt-4 md:px-3 md:pt-6">
          <div className="flex items-start gap-2">
            <div className="min-w-0 flex-1">
              <span className="block truncate font-mono text-[15px] font-semibold" title={lambda.publicKey}>{lambda.publicKey}</span>
              {/* pulled left by the padding of a badge, so what they say lines
                  up with the name above and the addresses below */}
              <div className="-ml-2 mt-2 flex flex-wrap items-center gap-1.5">
                <StatusBadge version={lambda.activeVersion} online={simple ? t.simple.badge : undefined} />
                <TierBadge tier={lambda.tier} />
              </div>
            </div>

            <Menu label={said.moreActions} align="start">
              {(close) => (
                <>
                  {live && !demo && !simple && (
                    <button type="button" role="menuitem" className={menuItem} disabled={busy !== null}
                            onClick={() => { close(); deploy(lambda.activeVersion!); }}>
                      {said.redeploy(lambda.activeVersion!)}
                    </button>
                  )}
                  {live && !demo && (
                    <button type="button" role="menuitem" className={menuItem} disabled={busy !== null}
                            onClick={() => { close(); undeploy(); }}>
                      {said.takeOffline}
                    </button>
                  )}
                  {live && !demo && menuRule}
                  <CopyItem value={editorUrl} label={demo ? said.copyLink : said.copyPrivate} title={said.privateLink} onDone={close} />
                  {!demo && (
                    <button type="button" role="menuitem" className={menuItem} onClick={() => { close(); setRenaming(true); }}>
                      {said.rename}
                    </button>
                  )}
                  {!simple && (
                    <a role="menuitem" href={api.exportUrl(privateKey)} className={menuItem} onClick={close}>
                      {said.download}
                    </a>
                  )}
                  {/* on a phone the switch below the sections is not there, so it is here */}
                  <button type="button" role="menuitem" className={`${menuItem} md:hidden`}
                          onClick={() => { close(); switchTo(simple ? 'full' : 'simple'); }}>
                    {simple ? <IconViewFull className="h-4 w-4 text-slate-400" /> : <IconViewSimple className="h-4 w-4 text-slate-400" />}
                    {simple ? t.simple.toFull : t.simple.toSimple}
                  </button>
                  {!demo && menuRule}
                  {!demo && (
                    <button type="button" role="menuitem" className={`${menuItem} text-red-500`}
                            onClick={() => { close(); setRemoving(true); }}>
                      {said.delete}
                    </button>
                  )}
                </>
              )}
            </Menu>
          </div>

          {/* where it answers: its own domain first when it has one, since
              that is the address its visitors know */}
          <div className="mt-3 space-y-0.5">
            {domainUrl && <Address url={domainUrl} live={live} primary />}
            <Address url={publicUrl} live={live} primary={!domainUrl} />
            {/* where anybody reads its code, once its owner published it */}
            {source?.published && (
              <SiteLink
                to={source.path}
                target="_blank"
                className="flex items-center gap-1.5 pt-0.5 text-xs text-slate-500 hover:text-accent-600 hover:underline dark:hover:text-accent-400"
              >
                <IconCode className="h-3.5 w-3.5" />
                {t.openSource.sidebar} · {source.license.id}
              </SiteLink>
            )}
          </div>

          {/* not while the agent is at work: what it saved last may be a
              version it is still fixing, and it deploys what it finishes */}
          {!featureKey && ahead && latest != null && !demo && !changing && (
            <button
              type="button"
              onClick={() => deploy(latest)}
              disabled={busy !== null}
              className="btn-primary mt-4 w-full"
              title={versions[0]?.change ?? undefined}
            >
              {busy === 'deploy' ? <IconSpinner /> : <IconPlay />}
              {simple ? t.simple.publish : said.deploy(latest)}
            </button>
          )}

          {open && (
            <FeatureCard
              feature={open}
              base={base}
              privateKey={privateKey}
              previewing={previewing}
              unsaved={unsaved}
              compact={featureView === 'code'}
              simple={simple}
              onPreview={previewFeature}
              onPutOnline={() => setMerging(open.key)}
              onDialog={setFeatureDialog}
              onStop={async () => {
                try {
                  await api.feature.stop(privateKey, open.key);
                  await refresh();
                  toast(t.features.previewStopped);
                } catch (error) {
                  toast(error instanceof ApiError ? error.message : t.features.previewFailed, 'error');
                }
              }}
              onBack={() => go(`${base}/features`)}
            />
          )}
        </div>

        {featureKey ? (
          // one view is no choice, so the simple view has no row for it
          !simple && (
          <nav aria-label={t.features.viewsLabel} className="flex gap-1 overflow-x-auto [scrollbar-width:none] px-3 pb-2 md:mt-2 md:flex-col md:gap-0.5 md:overflow-visible md:px-0">
            {FEATURE_VIEWS.map((view) => {
              const to = featurePath(featureKey, view);
              const current = featureView === view;

              return (
                <Link
                  key={view}
                  to={to}
                  onClick={(event) => {
                    event.preventDefault();

                    if (current) {
                      refresh().catch(() => undefined);
                    } else {
                      go(to);
                    }
                  }}
                  aria-current={current ? 'page' : undefined}
                  className={`flex shrink-0 items-center gap-2 whitespace-nowrap border-b-2 px-3 py-2 text-sm md:border-b-0 md:border-l-2 ${
                    current
                      ? 'border-accent-500 font-medium text-ink-900 dark:border-accent-400 dark:text-slate-100 md:bg-slate-100 md:dark:bg-ink-850'
                      : 'border-transparent text-slate-600 hover:text-slate-900 dark:text-slate-400 dark:hover:text-slate-200'
                  }`}
                >
                  {t.features.views[view]}
                </Link>
              );
            })}
          </nav>
          )
        ) : (
        <nav aria-label={said.sectionsLabel} className="flex gap-1 overflow-x-auto [scrollbar-width:none] px-3 pb-2 md:mt-2 md:flex-col md:gap-0.5 md:overflow-visible md:px-0">
          {simple
            ? SIMPLE_SECTIONS.filter((id) => !absent(id)).map(entry)
            : GROUPS.map((group) => {
                const shown = group.sections.filter((id) => !absent(id));

                if (shown.length === 0) {
                  return null;
                }

                return (
                  <Fragment key={group.id ?? 'top'}>
                    {group.id && (
                      <>
                        {/* a row along the top on a phone: a rule between the groups rather than their names */}
                        <span aria-hidden="true" className="my-2 w-px shrink-0 self-stretch bg-slate-200 dark:bg-ink-800 md:hidden" />
                        <span className="hidden px-3 pb-1 pt-5 text-[11px] font-medium uppercase tracking-wide text-slate-400 md:block">
                          {said.groups[group.id]}
                        </span>
                      </>
                    )}
                    {shown.map(entry)}
                  </Fragment>
                );
              })}
          {/* the operator's, after everything that is the owner's and apart from it */}
          {ADMIN_SECTIONS.some((id) => !absent(id)) && (
            <>
              <span aria-hidden="true" className="my-2 w-px shrink-0 self-stretch bg-slate-200 dark:bg-ink-800 md:hidden" />
              <span aria-hidden="true" className="mx-3 mb-2 mt-5 hidden h-px bg-slate-200 dark:bg-ink-800 md:block" />
              {ADMIN_SECTIONS.filter((id) => !absent(id)).map(entry)}
            </>
          )}
        </nav>
        )}

        <ViewSwitch view={view} onSwitch={switchTo} />
      </aside>

      <main className={`flex min-w-0 flex-1 flex-col ${code ? 'min-h-0' : ''}`}>
        {demo && (
          <div className="mx-4 mt-6 border border-accent-500/30 bg-accent-500/5 px-4 py-3 text-sm md:mx-0">
            <p className="font-medium">{said.demoTitle}</p>
            <p className="mt-1 text-slate-600 dark:text-slate-400">
              {said.demo((text) => (
                <Link to={`/editor/create?from=${encodeURIComponent(lambda.publicKey)}`} className="text-accent-500 hover:underline">
                  {text}
                </Link>
              ))}
            </p>
          </div>
        )}
        {welcome && !demo && (
          <div className="mx-4 mt-6 border border-accent-500/30 bg-accent-500/5 px-4 py-3 md:mx-0">
            <div className="flex items-start justify-between gap-4">
              <p className="text-sm font-medium">{said.keep}</p>
              <button type="button" onClick={() => setWelcome(false)} className="text-xs text-slate-500 hover:underline">
                {said.gotIt}
              </button>
            </div>
            <div className="mt-2 max-w-xl">
              <CopyField value={editorUrl} tone="accent" />
            </div>

          </div>
        )}

        {changing && section !== 'change' && change && (
          <div className="mx-4 mt-6 flex items-center gap-3 border border-accent-500/30 bg-accent-500/5 px-4 py-2.5 text-sm md:mx-0">
            {change.state === 'running'
              ? <IconSpinner className="h-4 w-4 shrink-0 text-accent-500 dark:text-accent-400" />
              : <IconSpark className="h-4 w-4 shrink-0 text-accent-500 dark:text-accent-400" />}
            <p className="min-w-0 flex-1 truncate">
              <span className="font-medium">{change.state === 'running' ? said.changing : said.waiting}</span>
              <span className="text-slate-500"> - {change.prompt}</span>
            </p>
            <Link
              to={`${base}/change`}
              onClick={(event) => { event.preventDefault(); go(`${base}/change`); }}
              className="shrink-0 text-[13px] font-medium text-accent-500 hover:underline dark:text-accent-400"
            >
              {said.follow}
            </Link>
          </div>
        )}

        {outside && (
          <div className="mx-4 mt-6 flex flex-wrap items-center gap-x-4 gap-y-2 border border-slate-200 bg-slate-50 px-4 py-3 text-sm dark:border-ink-800 dark:bg-ink-900 md:mx-0">
            <IconViewFull className="h-4 w-4 shrink-0 text-slate-400" />
            <div className="min-w-0 flex-1">
              <p className="font-medium">{t.simple.outsideTitle}</p>
              <p className="mt-0.5 text-[13px] text-slate-600 dark:text-slate-400">{t.simple.outsideText}</p>
            </div>
            <div className="flex flex-wrap gap-2">
              <button type="button" onClick={() => go(featureKey ? featurePath(featureKey) : base)} className="btn-ghost !px-3 !py-1.5 text-[13px]">
                {t.simple.back}
              </button>
              <button type="button" onClick={() => choose('full')} className="btn-primary !px-4 !py-1.5 text-[13px]">
                {t.simple.toFull}
              </button>
            </div>
          </div>
        )}

        {featureKey ? (
          open === undefined ? (
            <div className="flex items-center gap-2 px-4 py-10 text-sm text-slate-500 md:px-0"><IconSpinner /> {t.features.loading}</div>
          ) : open === null ? (
            <FeatureMissing onBack={() => go(`${base}/features`)} onVersions={() => go(`${base}/versions`)} />
          ) : featureView === 'code' ? (
            <CodeTab key={open.key} control={control} onDirty={onDirty} />
          ) : featureView === 'docs' || featureView === 'tests' ? (
            <ContextTab key={`${open.key}-${featureView}`} control={control} area={featureView} onDirty={onDirty} />
          ) : featureView === 'data' ? (
            <DataTab key={open.key} control={control} kind={dataKind} onKind={(kind) => go(`${featurePath(open.key, 'data')}/${kind}`)} />
          ) : featureView === 'logs' ? (
            <LogsTab key={open.key} control={control} />
          ) : (
            <FeatureTab control={control} onNotes={() => setFeatureDialog('notes')} onRebase={() => setFeatureDialog('base')} />
          )
        ) : section === 'code' ? (
          <CodeTab control={control} onDirty={onDirty} />
        ) : section === 'docs' || section === 'tests' ? (
          <ContextTab key={section} control={control} area={section} onDirty={onDirty} />
        ) : section === 'features' && !demo ? (
          <FeaturesTab control={control} />
        ) : section === 'change' && !demo ? (
          <ChangeTab control={control} />
        ) : section === 'data' ? (
          <DataTab control={control} kind={dataKind} onKind={(kind) => go(`${base}/data/${kind}`)} />
        ) : section === 'history' && simple ? (
          <HistoryTab control={control} />
        ) : section === 'versions' ? (
          <VersionsTab control={control} />
        ) : section === 'deployments' ? (
          <DeploymentsTab control={control} />
        ) : section === 'stats' ? (
          <StatsTab control={control} />
        ) : section === 'logs' ? (
          <LogsTab control={control} />
        ) : section === 'showcase' && !demo ? (
          <ShowcaseTab control={control} />
        ) : section === 'source' && !demo ? (
          <SourceTab control={control} onChanged={() => refresh().catch(() => undefined)} />
        ) : section === 'domain' && !hidden ? (
          <DomainTab control={control} />
        ) : section === 'admin' && operator ? (
          <AdminTab control={control} />
        ) : simple ? (
          <SimpleOverview control={control} />
        ) : (
          <SummaryTab control={control} />
        )}
      </main>
    </div>

      <Dialog
        title={rejection?.feature ? t.features.previewRejected : simple ? t.simple.refused : rejection?.version != null ? said.rejected(rejection.version) : said.refused}
        open={rejection !== null}
        onClose={() => setRejection(null)}
        footer={
          <>
            {/* in the simple view, what is wrong with the code is the agent's to fix, not the owner's to read */}
            {simple && agent.state?.available && !changing && (
              <button
                type="button"
                className="btn-ghost"
                onClick={() => {
                  const feature = rejection?.feature;

                  setRejection(null);
                  control.askAgent(feature, feature ? undefined : t.simple.fixRefused);
                }}
              >
                <IconSpark className="h-4 w-4" />
                {t.simple.fix}
              </button>
            )}
            {!simple && (rejection?.version != null || rejection?.feature) && (
              <button
                type="button"
                className="btn-ghost"
                onClick={() => {
                  const { version, feature } = rejection;
                  setRejection(null);

                  if (feature) {
                    control.openFeature(feature, 'code');
                  } else {
                    control.edit(version);
                  }
                }}
              >
                {said.openCode}
              </button>
            )}
            <button type="button" onClick={() => setRejection(null)} className="btn-primary">
              {said.close}
            </button>
          </>
        }
      >
        <p className="text-slate-600 dark:text-slate-400">
          {simple ? t.simple.refusedText : rejection?.feature ? t.features.previewNotCompiling : said.notCompiling}
        </p>
        {!simple && (
          <div className="max-h-72 overflow-y-auto border border-slate-200 dark:border-ink-800">
            <Diagnostics diagnostics={rejection?.diagnostics ?? []} state="idle" onSelect={() => undefined} />
          </div>
        )}
      </Dialog>

      <NewFeatureDialog
        control={control}
        open={creating !== null}
        base={creating?.base}
        files={creating?.files}
        onClose={() => setCreating(null)}
        onCreated={(feature) => created(feature, creating?.files != null)}
      />

      {putting && (
        <MergeDialog
          control={control}
          feature={putting}
          open={merging !== null}
          onClose={() => setMerging(null)}
          onMerged={merged}
          onRebase={() => {
            setMerging(null);

            // marked from the feature's own page, where the dialog for it is
            if (putting.key === open?.key) {
              setFeatureDialog('base');
            } else {
              control.openFeature(putting.key);
            }
          }}
        />
      )}

      {open && (
        <>
          <NotesDialog control={control} feature={open} open={featureDialog === 'notes'} onClose={() => setFeatureDialog(null)} />
          <BaseDialog control={control} feature={open} open={featureDialog === 'base'} onClose={() => setFeatureDialog(null)} />
          <DeleteFeatureDialog
            control={control}
            feature={open}
            open={featureDialog === 'delete'}
            onClose={() => setFeatureDialog(null)}
            onDeleted={() => {
              setFeatureDialog(null);
              toast(t.features.deleted(open.name));
              onDirty(false);
              navigate(`${base}/features`);
              refresh().catch(() => undefined);
            }}
          />
        </>
      )}

      <RenameDialog
        open={renaming}
        current={lambda.publicKey}
        onClose={() => setRenaming(false)}
        onRenamed={(updated) => {
          setLambda(updated);
          setRenaming(false);
          refresh().catch(() => undefined);
          toast(said.moved(platformPath(updated.publicKey)));
        }}
        privateKey={privateKey}
      />

      <Dialog
        title={said.deleteTitle}
        open={removing}
        onClose={() => setRemoving(false)}
        footer={
          <>
            <button type="button" onClick={() => setRemoving(false)} className="btn-ghost">
              {said.cancel}
            </button>
            <button
              type="button"
              className="btn-danger"
              onClick={async () => {
                try {
                  await api.remove(privateKey);
                  navigate('/', { replace: true });
                } catch (error) {
                  toast(error instanceof ApiError ? error.message : said.deleteFailed, 'error');
                }
              }}
            >
              {said.deleteForGood}
            </button>
          </>
        }
      >
        <p className="text-slate-600 dark:text-slate-400">
          {said.deleteText(<code className="font-mono">{lambda.publicKey}</code>)}
        </p>
      </Dialog>
    </div>
    </SharedWordsContext.Provider>
  );
}

/**
 * Which view is showing, at the foot of the sidebar: the simple one or every
 * section. Kept for this lambda in the browser of whoever switches, and
 * nowhere else (see control/view.ts). On a phone the sidebar is a
 * row along the top with no room below it, so the menu has the switch there.
 */
function ViewSwitch({ view, onSwitch }: { view: View; onSwitch: (view: View) => void }) {
  const said = useEditorT().simple;

  const options: { id: View; label: string; title: string; Icon: typeof IconViewSimple }[] = [
    { id: 'simple', label: said.simple, title: said.simpleTitle, Icon: IconViewSimple },
    { id: 'full', label: said.full, title: said.fullTitle, Icon: IconViewFull },
  ];

  return (
    <div className="mt-6 hidden border-t border-slate-200 px-3 pt-4 dark:border-ink-800 md:block">
      <div id="editor-view" className="text-[11px] font-medium uppercase tracking-wide text-slate-400">{said.view}</div>

      <div role="radiogroup" aria-labelledby="editor-view" className="mt-2 grid grid-cols-2 border border-slate-200 p-0.5 dark:border-ink-800">
        {options.map(({ id, label, title, Icon }) => {
          const on = view === id;

          return (
            <button
              key={id}
              type="button"
              role="radio"
              aria-checked={on}
              title={title}
              onClick={() => !on && onSwitch(id)}
              className={`flex items-center justify-center gap-1.5 px-2 py-1.5 text-[13px] transition-colors focus-visible:outline focus-visible:outline-2 focus-visible:-outline-offset-2 focus-visible:outline-accent-500 ${
                on
                  ? 'bg-accent-500/10 font-medium text-accent-700 dark:text-accent-400'
                  : 'text-slate-500 hover:bg-slate-100 hover:text-slate-900 dark:hover:bg-ink-850 dark:hover:text-slate-200'
              }`}
            >
              <Icon className="h-3.5 w-3.5" />
              {label}
            </button>
          );
        })}
      </div>

      <p className="mt-2 text-xs leading-relaxed text-slate-500">{view === 'simple' ? said.simpleNote : said.fullNote}</p>
    </div>
  );
}

/**
 * The feature the page is opened on, in the sidebar: what it is called,
 * where it can be tried, and the two things done with it - trying it, and
 * putting it online. The rest is in its menu.
 *
 * Its edge and its name line up with the views listed underneath, which are
 * the feature's own, so it reads as their heading.
 */
function FeatureCard({ feature, base, privateKey, previewing, unsaved, compact, simple, onPreview, onPutOnline, onDialog, onStop, onBack }: {
  feature: Feature;
  base: string;
  privateKey: string;
  previewing: boolean;
  /** Whether the code view holds something unsaved, which neither the preview nor putting it online would include. */
  unsaved: boolean;
  /** Whether the code is open below it, which has room for little else on a phone. */
  compact: boolean;
  /** Whether the simple view is showing, which leaves the agent to bring a draft up to date and has no use for its files. */
  simple: boolean;
  onPreview: () => void;
  onPutOnline: () => void;
  onDialog: (dialog: 'notes' | 'base' | 'delete') => void;
  onStop: () => void;
  onBack: () => void;
}) {
  const said = useEditorT().features;
  const preview = absoluteAddress(feature.previewPath);

  return (
    <div className="mt-4 border-l-2 border-accent-500 pl-3 dark:border-accent-400 md:-ml-3">
      <Link
        to={`${base}/features`}
        onClick={(event) => { event.preventDefault(); onBack(); }}
        className="text-xs text-slate-500 hover:text-slate-800 hover:underline dark:hover:text-slate-200"
      >
        ← {said.all}
      </Link>

      <div className="mt-1 flex items-start gap-1.5">
        <span className="min-w-0 flex-1 break-words text-[15px] font-semibold leading-snug">{feature.name}</span>

        <Menu label={said.actions} align="start">
          {(close) => (
            <>
              <button type="button" role="menuitem" className={menuItem} onClick={() => { close(); onDialog('notes'); }}>
                {said.editNotes}
              </button>
              {/* up to date already, there is nothing to mark */}
              {!feature.mergeable && !simple && (
                <button type="button" role="menuitem" className={menuItem} onClick={() => { close(); onDialog('base'); }}>
                  {said.moveBase}
                </button>
              )}
              {!simple && (
                <a role="menuitem" href={api.feature.zipUrl(privateKey, feature.key)} className={menuItem} onClick={close}>
                  {said.download}
                </a>
              )}
              {feature.online && (
                <button type="button" role="menuitem" className={menuItem} onClick={() => { close(); onStop(); }}>
                  {said.stopPreview}
                </button>
              )}
              {menuRule}
              <button type="button" role="menuitem" className={`${menuItem} text-red-500`} onClick={() => { close(); onDialog('delete'); }}>
                {said.delete}
              </button>
            </>
          )}
        </Menu>
      </div>

      {/* which version it began from is nothing to the owner, until a newer one means it cannot go online as it is */}
      {!feature.mergeable && (
        <p className="mt-0.5 text-xs text-amber-600 dark:text-amber-400" title={said.behindTitle}>
          {said.behind(feature.newest ?? feature.base)}
        </p>
      )}

      <div className={compact ? 'hidden md:block' : ''}>
        <div className="mt-2">
          <Address url={preview} live={feature.online} primary={false} />
        </div>

        <div className="mt-3 grid gap-1.5">
          {feature.current ? (
            <a href={preview} target="_blank" rel="noreferrer" className="btn-ghost w-full" title={said.openPreviewTitle}>
              <IconExternal />
              {said.openPreview}
            </a>
          ) : (
            <button type="button" onClick={onPreview} disabled={previewing || unsaved} className="btn-primary w-full">
              {previewing ? <IconSpinner /> : <IconPlay />}
              {feature.online ? said.updatePreview : said.deployPreview}
            </button>
          )}
          <button
            type="button"
            onClick={onPutOnline}
            disabled={unsaved}
            className={`${feature.current && feature.mergeable ? 'btn-primary' : 'btn-ghost'} w-full`}
            title={feature.mergeable ? said.mergeTitleShort : said.behindTitle}
          >
            <IconPlay />
            {said.mergeButton}
          </button>
          {unsaved && <p className="text-xs text-amber-700 dark:text-amber-400">{said.saveFirst}</p>}
        </div>
      </div>
    </div>
  );
}

/** A feature link that no longer leads anywhere: merged, deleted, or never there. */
function FeatureMissing({ onBack, onVersions }: { onBack: () => void; onVersions: () => void }) {
  const t = useEditorT();
  const said = t.features;

  return (
    <div className="max-w-xl px-4 py-16 md:px-0">
      <h1 className="text-lg font-semibold">{said.missingTitle}</h1>
      <p className="mt-2 text-sm text-slate-600 dark:text-slate-400">{said.missingText}</p>
      <div className="mt-6 flex flex-wrap gap-2">
        <button type="button" onClick={onVersions} className="btn-primary !px-4 !py-1.5 text-[13px]">{t.frame.sections.versions}</button>
        <button type="button" onClick={onBack} className="btn-ghost !px-3 !py-1.5 text-[13px]">{said.all}</button>
      </div>
    </div>
  );
}

/**
 * One address the lambda answers at, with copying and opening beside it. The
 * main one is the link to follow; any other is there to be found, not to
 * compete with it.
 */
function Address({ url, live, primary }: { url: string; live: boolean; primary: boolean }) {
  const said = useEditorT().frame;
  const shown = url.replace(/^https?:\/\//, '').replace(/\/$/, '');

  return (
    <div className={`flex items-center gap-1 ${primary ? 'text-[13px]' : 'text-xs'}`}>
      <a
        href={live ? url : undefined}
        target="_blank"
        rel="noreferrer"
        title={url}
        className={`min-w-0 flex-1 truncate ${
          !live
            ? 'text-slate-400'
            : primary
              ? 'font-medium text-accent-600 hover:underline dark:text-accent-400'
              : 'text-slate-500 hover:text-accent-600 hover:underline dark:hover:text-accent-400'
        }`}
      >
        {shown}
      </a>
      <CopyButton value={url} />
      {live && (
        <a href={url} target="_blank" rel="noreferrer" className="p-1 text-slate-400 hover:text-slate-700 dark:hover:text-slate-200"
           title={said.openInTab} aria-label={said.open(shown)}>
          <IconExternal className="h-3.5 w-3.5" />
        </a>
      )}
    </div>
  );
}

function CopyButton({ value }: { value: string }) {
  const said = useEditorT().frame;
  const [copied, setCopied] = useState(false);

  return (
    <button
      type="button"
      onClick={async () => {
        try {
          await navigator.clipboard.writeText(value);
          setCopied(true);
          window.setTimeout(() => setCopied(false), 1500);
        } catch {
          // a clipboard that refuses is not worth an error
        }
      }}
      className="p-1 text-slate-400 hover:text-slate-700 dark:hover:text-slate-200"
      title={said.copyAddress}
      aria-label={said.copyAddress}
    >
      {copied ? <IconCheck className="h-3.5 w-3.5 text-emerald-500" /> : <IconCopy className="h-3.5 w-3.5" />}
    </button>
  );
}

function CopyItem({ value, label, title, onDone }: { value: string; label: string; title: string; onDone: () => void }) {
  return (
    <button
      type="button"
      role="menuitem"
      className={menuItem}
      onClick={async () => {
        try {
          await navigator.clipboard.writeText(value);
        } catch {
          // as above
        }

        onDone();
      }}
      title={title}
    >
      {label}
    </button>
  );
}

function RenameDialog({
  open,
  current,
  privateKey,
  onClose,
  onRenamed,
}: {
  open: boolean;
  current: string;
  privateKey: string;
  onClose: () => void;
  onRenamed: (lambda: Lambda) => void;
}) {
  const said = useEditorT().frame;
  const [value, setValue] = useState(current);
  const [error, setError] = useState<string | null>(null);
  const [working, setWorking] = useState(false);

  useEffect(() => {
    if (open) {
      setValue(current);
      setError(null);
    }
  }, [open, current]);

  async function submit() {
    setWorking(true);
    setError(null);

    try {
      onRenamed(await api.changeKey(privateKey, value.trim()));
    } catch (caught) {
      setError(caught instanceof ApiError ? caught.message : said.renameFailed);
    } finally {
      setWorking(false);
    }
  }

  return (
    <Dialog
      title={said.rename}
      open={open}
      onClose={onClose}
      footer={
        <>
          <button type="button" onClick={onClose} className="btn-ghost">
            {said.cancel}
          </button>
          <button type="button" onClick={submit} disabled={working} className="btn-primary">
            {working && <IconSpinner />}
            {said.moveIt}
          </button>
        </>
      }
    >
      <p className="text-slate-600 dark:text-slate-400">{said.renameText}</p>

      <div className="flex items-center gap-2">
        <span className="shrink-0 font-mono text-sm text-slate-500">/lambda/</span>
        <input
          value={value}
          onChange={(event) => setValue(event.target.value)}
          onKeyDown={(event) => event.key === 'Enter' && submit()}
          spellCheck={false}
          autoComplete="off"
          className="field font-mono"
        />
      </div>

      {error && <p className="text-xs text-red-500">{error}</p>}
    </Dialog>
  );
}
