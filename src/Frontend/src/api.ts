/**
 * Typed access to /api/v1. Every call goes through `request`, which turns the
 * error shape of the server into an ApiError the pages can render.
 */

export interface Lambda {
  publicKey: string;
  privateKey: string;
  tier: string;
  created: string;
  modified: string;
  activeVersion?: number;
  latestVersion?: number;
  publicPath: string;
  editorPath: string;
  /** When the live version went online; absent while nothing is deployed. */
  deployedAt?: string;
  /** When the deployment will be taken offline again; absent when its tier keeps it online. */
  deployedUntil?: string;
  /** When an untouched lambda is removed altogether; absent when its tier keeps it. */
  keptUntil?: string;
  /** The domain it is configured to answer at, whether or not its tier lets it. */
  domain?: string;
  /** Whether it actually answers at that domain right now. */
  domainServed: boolean;
  /** Where to link to it: its domain while that is served, its path otherwise. See address.ts. */
  address: string;
}

/** The tiers there are. Only an administrator moves a lambda between them. */
export const TIERS = ['Free', 'Premium', 'Demo'] as const;

/** Whether a lambda is one of the installation's demos, which nobody can change. */
export const isDemo = (tier: string) => tier === 'Demo';

/** Whether a tier includes a domain of its own. */
export const allowsDomain = (tier: string) => tier === 'Premium';

/** The domain of a lambda, as its owner sees it. */
export interface DomainState {
  domain?: string;
  tier: string;
  /** Whether the tier includes a domain. */
  allowed: boolean;
  /** Whether requests to the domain reach the lambda now. */
  served: boolean;
  /** What the domain resolves to, as the server sees it. Absent without a domain. */
  dns?: { addresses: string[]; problem?: string };
}

/** Which door something came through. */
export type Origin = 'template' | 'api' | 'agent' | 'admin' | 'system';

export interface VersionInfo {
  version: number;
  created: string;
  /** What the user wanted from the version and why, in their words where possible. */
  specification?: string | null;
  /** What the version changed, in a line. */
  change?: string | null;
  origin?: Origin | null;
}

/** One stretch of time a version was online. */
export interface Activation {
  version: number;
  started: string;
  origin?: Origin | null;
  /** Absent while it is still online. */
  ended?: string | null;
  endedBy?: 'replaced' | 'stopped' | 'expired' | 'admin' | null;
  /** How long it was online, or has been so far. */
  seconds: number;
}

/** One interval of a lambda's traffic. */
export interface TrafficPoint {
  at: string;
  requests: number;
  /** Answered with a server error. */
  failed: number;
  /** Answered with a client error, not found included. */
  rejected: number;
  upgrades: number;
  averageMillis: number;
  bytes: number;
}

export interface LambdaTraffic {
  /** Absent for a lambda nobody has called since the server started. */
  totals?: LambdaActivity | null;
  /** The last hour by the minute, oldest first. */
  minutes: TrafficPoint[];
  /** The last day by the quarter hour, oldest first. */
  quarters: TrafficPoint[];
  statuses: { success: number; redirect: number; clientError: number; serverError: number };
  paths: { path: string; requests: number; failed: number; averageMillis: number }[];
  /** Where it was reached: its own domain, or its path on the platform (no domain). Busiest first. */
  entrances: { domain?: string | null; requests: number }[];
  /** When counting started: the figures are held in memory and a restart begins them again. */
  since: string;
}

/** A line of a lambda's own log. Without the visitor's address, on purpose. */
export interface OwnerLogEntry {
  seq: number;
  at: string;
  level: 'trace' | 'debug' | 'info' | 'warn' | 'error' | 'critical';
  /** Requests, stdout, stderr, or the part of the server that spoke. */
  source: string;
  text: string;
  detail?: string | null;
  country?: string | null;
  agent?: string | null;
  repeats: number;
  /** The lambda's own domain the request was addressed to; absent for its path on the platform. */
  domain?: string | null;
}

export interface OwnerLogPage {
  lines: OwnerLogEntry[];
  cursor: number;
  missed: number;
  /** Whether what the lambda prints is kept at all on this installation. */
  capturing: boolean;
}

/** The dashboard in one answer. */
export interface LambdaSummary {
  lambda: Lambda;
  live?: VersionInfo | null;
  latest?: VersionInfo | null;
  versions: number;
  activation?: Activation | null;
  traffic: {
    hourRequests: number;
    hourFailed: number;
    dayRequests: number;
    dayFailed: number;
    dayRejected: number;
    averageMillis: number;
    upgrades: number;
    /** Requests per hour over the last day, oldest first. */
    hourly: number[];
    lastSeen?: string | null;
    since: string;
  };
  recentProblems: OwnerLogEntry[];
  storage: {
    version?: number | null;
    codeFiles: number;
    codeCharacters: number;
    assets: number;
    assetBytes: number;
    workspaceFiles: number;
    workspaceBytes: number;
    servesAssets: boolean;
    servesWorkspace: boolean;
    /** Whether the owner left the workspace switched on. */
    workspaceEnabled: boolean;
    /** Whether the code of that version uses the workspace at all, and so fails where it does once it is off. */
    usesWorkspace: boolean;
  };
  /** What this lambda may use in its tier. Nothing counts its C# files or assets, only what they come to. */
  limits: {
    codeCharacters: number;
    assetBytes: number;
    workspaceBytes: number;
    versions: number;
    deploymentLifetimeHours: number;
    retentionDays: number;
    /** How many features it may have open at once. */
    features: number;
  };
}

export interface LambdaFile {
  name: string;
  code: string;

  /**
   * "base64" for a file that is not text, absent otherwise.
   *
   * Carried through the editor even though nothing here reads it, because
   * dropping it on the way back out would turn an image into a text file
   * full of the letters of its own encoding.
   */
  encoding?: string | null;
}

export interface VersionContent extends VersionInfo {
  /** Every file of the version, lambda.cs first. */
  files: LambdaFile[];
}

export interface Diagnostic {
  severity: string;
  id: string;
  message: string;
  line: number;
  column: number;
  /** Which file it is in; absent when it is about none of them. */
  file?: string;
}

export interface CompilationResult {
  success: boolean;
  diagnostics: Diagnostic[];
}

export interface DeploymentResult extends CompilationResult {
  lambda?: Lambda;
}

export interface Deployment {
  deployed: boolean;
  version?: number;
  deployedAt?: string;
  deployedUntil?: string;
}

export interface BuildResult {
  ok: boolean;
  url?: string;
  editorUrl?: string;
  publicKey?: string;
  privateKey?: string;
  summary?: string;
  error?: string;
  detail?: string;
  deployed?: boolean;
}

/**
 * One thing the agent did while changing a lambda. Facts rather than words,
 * so the control center says them in its own language; only what the agent
 * said itself (kind "say") is prose.
 */
export interface AgentStep {
  /** Seconds into the run. */
  at: number;
  /**
   * What it did. feature is starting one, update changing its notes or its
   * base, merge making it a version, discard deleting it.
   */
  kind:
    | 'say' | 'guide' | 'demos' | 'read' | 'logs' | 'create' | 'write' | 'check' | 'deploy' | 'upload' | 'delete' | 'list'
    | 'feature' | 'update' | 'merge' | 'discard' | 'other';
  /** What the agent said, for "say". */
  text?: string | null;
  /** The tool, for "other". */
  tool?: string | null;
  /** The files written, or the one read. */
  files?: string[] | null;
  /** The files a write removed. */
  removed?: string[] | null;
  /** Whether a write replaced every file rather than some. */
  whole?: boolean | null;
  /** The workspace path of an upload or a removal. */
  path?: string | null;
  /** Whether the tool has answered. */
  done?: boolean | null;
  /** The version read, saved, merged into, put online, or made a feature's base. */
  version?: number | null;
  /** Whether a write or a deployment went online - at the preview address, for a feature. */
  online?: boolean | null;
  /** The name of the feature it was about, once a tool said it. */
  feature?: string | null;
  /** Whether it was about a feature rather than the lambda itself. */
  preview?: boolean | null;
  /** How many errors the compiler found. */
  errors?: number | null;
  /** How many errors the log held. */
  problems?: number | null;
  /** Why the platform refused the step, in its words. */
  problem?: string | null;
}

/** How a change ended, from what the tools answered. */
export interface ChangeResult {
  /** Whether it saved anything - into a feature, or as a version. */
  ok: boolean;
  /** The newest version it saved, merging included. */
  version?: number | null;
  /** The version it put online, if it put one there. */
  online?: number | null;
  deployed?: boolean | null;
  /** Whether the last version it compiled compiles, where that is known. */
  compiles?: boolean | null;
  /** The version online when it started. */
  before?: number | null;
  unchanged?: boolean | null;
  cancelled?: boolean | null;
  /** What cut it short, whether or not it had saved something by then. */
  reason?: 'unauthorised' | 'timeout' | 'turns' | 'nothing' | null;
  /** What the agent said at the end, for the owner. */
  summary?: string | null;
  /** In English, for when there are no words of our own for it. */
  error?: string | null;
  detail?: string | null;
  /** The feature it left the change in, by its key, when it did not merge it. */
  feature?: string | null;
  featureName?: string | null;
  /** Whether that feature's preview is online, to be tried. */
  preview?: boolean | null;
}

export type ChangeState = 'queued' | 'running' | 'done' | 'failed' | 'cancelled';

/** A change asked of the agent, and how it is getting on. */
export interface ChangeJob {
  id: string;
  state: ChangeState;
  prompt: string;
  /** Whether it was asked to merge the change and put it online. */
  deploy: boolean;
  model: string;
  /** The feature it was asked to go on with; absent when it starts one of its own. */
  feature?: string | null;
  /** The version online when it was asked for. */
  before?: number | null;
  steps: AgentStep[];
  result?: ChangeResult | null;
  /** Its place in the queue; zero once it runs. */
  waiting: number;
  /** How long it has been running, counted by the agent. */
  seconds: number;
  /** How long it may run, in seconds; absent when there is no clock on it. */
  limit?: number | null;
}

/** What the Change section needs to draw itself. */
export interface AgentState {
  /** Whether this installation has an agent at all. */
  available: boolean;
  /** Builds and changes one address may ask for in a day. */
  perDay: number;
  left: number;
  /** Whether a second model is offered, behind a password. */
  secondModel: boolean;
  /** The change under way, or the last one while the agent remembers it (an hour). */
  job?: ChangeJob | null;
}

export interface ChangeRequest {
  prompt: string;
  /** Merge it and put it online once it works, rather than leave it in its feature to be tried. */
  deploy: boolean;
  model?: string;
  password?: string;
  /** The language of the control center, for the agent to fall back on. */
  language?: string;
  /** The feature to go on with; left out, the agent starts a new one. */
  feature?: string;
}

/** Whether a change is still waiting or working. */
export const isActive = (job?: ChangeJob | null) => job?.state === 'queued' || job?.state === 'running';

/** Everything anybody may know about a public key. */
export interface KeyStatus {
  /** The key the way it would be stored. */
  publicKey: string;
  /** Whether it is a key a lambda could have at all. */
  valid: boolean;
  /** Whether a new lambda could be created with it. */
  available: boolean;
  /** Whether a lambda has it. */
  exists: boolean;
  /** Whether that lambda is online. */
  deployed: boolean;
  /** Why it cannot be claimed, if it cannot. */
  reason?: string;
}

export interface Completion {
  label: string;
  kind: string;
  detail: string;
  insert?: string;
}

/** Something a new lambda can be started from: a demo to copy, or nothing much. */
export interface Starter {
  id: string;
  /** What somebody would want to build, in their words. */
  title: string;
  description: string;
  /** Where the demo it copies runs, to look at first; left out for the empty lambda. */
  demo?: string | null;
}

export interface Platform {
  terms: string;
  starters: Starter[];
  maxCodeLength: number;
  deploymentLifetimeHours: number;
  retentionDays: number;
  imports: string[];
  completions: Completion[];
  /** Whether the box on /build has an agent behind it. */
  build: { available: boolean; perDay: number; secondModel: boolean };
}

export interface TelemetrySample {
  taken: string;
  managedBytes: number;
  heapCommittedBytes: number;
  heapFragmentedBytes: number;
  workingSetBytes: number;
  privateBytes: number;
  residentBytes: number;
  anonymousBytes: number;
  jitBytes: number;
  assemblyBytes: number;
  otherFileBytes: number;
  swapBytes: number;
  gen0Collections: number;
  gen1Collections: number;
  gen2Collections: number;
  allocatedBytes: number;
  pausePercentage: number;
  cpuPercentage: number;
  threads: number;
  requests: number;
  failed: number;
  upgrades: number;
  inFlight: number;
  openSockets: number;
  averageMillis: number;
  openConnections: number;
  acceptedConnections: number;
  connections: number;
  fileDescriptors: number;
  socketDescriptors: number;
  ringDescriptors: number;
}

export interface Telemetry {
  server: {
    engine: string;
    version: string;
    runtime: string;
    platform: string;
    serverGarbageCollection: boolean;
    processors: number;
    started: string;
    uptimeSeconds: number;
  };
  traffic: { requests: number; failed: number; upgrades: number; openSockets: number };
  platform: { lambdas: number; deployed: number; versions: number };
  latest: TelemetrySample;
  intervalSeconds: number;
  samples: TelemetrySample[];
  events: EventHistory;
}

/** What has happened on the platform, counted by day. */
export interface EventHistory {
  /** yyyy-MM-dd, oldest first. */
  days: string[];
  series: { kind: string; counts: number[]; total: number }[];
}

export interface WorkspaceEntry {
  path: string;
  size: number;
  modified: string;
}

export interface WorkspaceListing {
  files: WorkspaceEntry[];
  folders: string[];
  /** The room the files and folders take, counted in blocks of 4 KB as the quota is. */
  usedBytes: number;
  quotaBytes: number;
  /** Whether the owner left the workspace switched on; off, it holds nothing. */
  enabled: boolean;
}

/**
 * One kind of data a lambda can keep. Data belongs to the lambda rather than
 * to a version: every version shares it, and deploying or rolling back leaves
 * it alone. The workspace is the only kind so far; more are meant to follow,
 * listed the same way.
 */
export interface DataStore {
  /** Which kind: "workspace", the files the lambda reads and writes. */
  kind: string;
  enabled: boolean;
  /** Whether a lambda has it until its owner decides. */
  default: boolean;
  /** When the owner last switched it; absent while it is as it came. */
  changed?: string | null;
  /** What it holds: files, for the workspace. */
  items: number;
  usedBytes: number;
  quotaBytes: number;
}

/**
 * A change being worked on beside the lambda: it starts as a copy of a
 * version and of the lambda's data, can be tried at an address of its own,
 * and becomes the next version when it is merged.
 */
export interface Feature {
  /** What it is addressed by, and where its preview answers. */
  key: string;
  name: string;
  /** What the user wants from it and why. */
  specification?: string | null;
  /** What it changes, in a line - what the version it becomes will say. */
  change?: string | null;
  /** The version it is based on. */
  base: number;
  /** The newest version of the lambda. */
  newest?: number | null;
  /** Whether it can be merged: it is based on the newest version. */
  mergeable: boolean;
  origin?: Origin | null;
  created: string;
  modified: string;
  /** Whether its preview is online. */
  online: boolean;
  /** Whether its preview is online and serves what the feature holds now, rather than an earlier save. */
  current: boolean;
  /** When its preview last went online, while it is. */
  previewed?: string | null;
  /** Where its preview answers, online or not. */
  previewPath: string;
  /** How many times its files were saved, counting the ones it began with. */
  revision: number;
}

export interface FeatureContent {
  feature: Feature;
  /** Every file, lambda.cs first. */
  files: LambdaFile[];
}

export interface FeaturePreview {
  success: boolean;
  feature: Feature;
  diagnostics: Diagnostic[];
}

export interface FeatureSaved {
  feature: Feature;
  /** How putting the preview online went, when that was asked for. */
  preview?: FeaturePreview | null;
}

export interface FeatureMerge {
  merged: boolean;
  /** The version it became. */
  version?: VersionInfo | null;
  /** Why it was not merged, where its code does not compile. */
  diagnostics: Diagnostic[];
  /** How putting that version online went, when that was asked for. */
  deployment?: DeploymentResult | null;
}

export interface LambdaActivity {
  publicKey: string;
  requests: number;
  failed: number;
  upgrades: number;
  averageMillis: number;
  slowestMillis: number;
  bytesOut: number;
  firstSeen?: string;
  lastSeen?: string;
}

export interface Activity {
  lambdas: LambdaActivity[];
  requests: number;
  upgrades: number;
}

export interface LambdaOverview {
  publicKey: string;
  /** The editor key. Served only to a request carrying the admin token. */
  privateKey: string;
  requests: number;
  failed: number;
  lastSeen?: string;
  tier: string;
  created: string;
  modified: string;
  activeVersion?: number;
  latestVersion?: number;
  versions: number;
  deployedUntil?: string;
  keptUntil?: string;
  domain?: string;
  domainServed: boolean;
}

/** One lambda in full, for the page the panel shows it on. */
export interface AdminLambdaDetail {
  lambda: Lambda;
  traffic: LambdaTraffic;
  versions: VersionInfo[];
  activations: Activation[];
  tiers: string[];
}

/** One line of what the server, or a lambda on it, has said. */
export interface LogEntry {
  /** Counts from one and never repeats; the cursor is built from these. */
  seq: number;
  at: string;
  level: 'trace' | 'debug' | 'info' | 'warn' | 'error' | 'critical';
  /** A logger category for the server, `stdout` or `stderr` for a lambda. */
  source: string;
  /** The lambda it belongs to, absent for the server itself. */
  lambda?: string;
  text: string;
  /** The stack trace, when there is one. */
  detail?: string;
  /** Where it came from. `<claimed> via <peer>` where a proxy said so. */
  client?: string;
  /** What the caller said it was. */
  agent?: string;
  /**
   * Two letter code of the registry the caller's range is allocated under.
   * Where the range is registered, which is not always where the caller is.
   */
  country?: string;
  /**
   * A town and a network, where a database has one — "Aveiro, PT · MEO". A
   * guess from measurement, not a fact from a registry.
   */
  place?: string;
  /** How many identical lines this one stands for; 1 is itself alone. */
  repeats: number;
  /** The lambda's own domain the request was addressed to; absent for the platform. */
  domain?: string;
}

/** One caller the log still holds something about. */
export interface LogCaller {
  client: string;
  place?: string;
  country?: string;
  agent?: string;
  /** Requests, counting a folded line by what it stands for. */
  lines: number;
  failed: number;
  first: string;
  last: string;
}

/** How the run before this one ended, when it did not end cleanly. */
export interface PreviousRun {
  started: string;
  lastSeen: string;
  minutes: number;
  /** Whether anything asked it to stop. */
  signalled: boolean;
  fault?: string;
  workingSet: number;
  requests: number;
  sockets: number;
}

export interface LogPage {
  lines: LogEntry[];
  /** Ask from here next time. */
  cursor: number;
  /** Lines dropped before this reader reached them. */
  missed: number;
  capacity: number;
  written: number;
  /** Whether what lambdas print is being kept at all. */
  capturing: boolean;
  /** Whether caller addresses are being recorded. */
  addresses: boolean;
  /** Absent when the run before this one stopped the way it meant to. */
  previous?: PreviousRun;
}

export interface AdminListing {
  lambdas: LambdaOverview[];
  total: number;
  deployed: number;
  /** How many the search matched; the pages are counted from this. */
  matched: number;
  page: number;
  pages: number;
}

export interface SemanticToken {
  line: number;
  column: number;
  length: number;
  kind: string;
}

export interface ResolvedCompletion {
  label: string;
  kind: string;
  detail: string;
  documentation?: string;
}

/** A lambda as its owner presents it on the showcase page. */
export interface ShowcaseEntry {
  publicKey: string;
  title: string;
  description: string;
  /** Where to link to it: its domain while that is served, its path otherwise. See address.ts. */
  path: string;
  /** Its picture, versioned so it can be cached for good. */
  imagePath: string;
  imageType: string;
  imageBytes: number;
  /** Only online lambdas are listed; the owner sees theirs either way. */
  online: boolean;
  created: string;
  updated: string;
}

export interface ShowcaseListing {
  entries: ShowcaseEntry[];
  total: number;
  /** Where the next page starts; absent after the last one. */
  next?: number | null;
}

export interface ShowcaseLimits {
  title: number;
  description: number;
  imageBytes: number;
  imageTypes: string[];
  /** How an entry should read. */
  tone: string;
}

export interface OwnShowcase {
  showcase?: ShowcaseEntry | null;
  limits: ShowcaseLimits;
}

/** Which pages the site links to, as the operator switched them. */
export interface Features {
  enterprise: boolean;
}

/** What the operator can switch on or off in the panel. */
export interface AdminSettings {
  enterprisePage: boolean;
}

export class ApiError extends Error {
  constructor(readonly status: number, message: string) {
    super(message);
  }
}

const base = '/api/v1';

async function request<T>(path: string, init?: RequestInit, allow: number[] = []): Promise<T> {
  let response: Response;

  try {
    response = await fetch(`${base}${path}`, {
      ...init,
      headers: { Accept: 'application/json', ...(init?.body ? { 'Content-Type': 'application/json' } : {}), ...init?.headers },
    });
  } catch {
    throw new ApiError(0, 'The server could not be reached.');
  }

  if (!response.ok && !allow.includes(response.status)) {
    throw new ApiError(response.status, await describe(response));
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

async function describe(response: Response): Promise<string> {
  try {
    const body = await response.json();
    return body.message ?? body.error ?? `The server responded with ${response.status}.`;
  } catch {
    return `The server responded with ${response.status}.`;
  }
}

const send = (body: unknown) => ({ method: 'POST', body: JSON.stringify(body) });

/** A lambda of one file, for the questions that are only ever about one. */
const single = (code: string): LambdaFile[] => [{ name: 'lambda.cs', code }];

/** The panel and the figures behind it are the parts of this API that authenticate. */
const withToken = (token: string, init: RequestInit = {}) => ({
  ...init,
  headers: { ...init.headers, 'X-Admin-Token': token },
});

export const api = {
  platform: () => request<Platform>('/system'),

  features: () => request<Features>('/system/features'),

  /** The text box on /build, and the agent behind it. Whether there is one is in `platform`. */
  builds: {
    start: (prompt: string, model?: string, password?: string) =>
      request<{ id: string; queued: number }>('/builds', send({ prompt, model, password })),
    progress: (id: string) =>
      request<{ state: string; events: string[]; result: BuildResult | null; waiting: number }>(`/builds/${id}`),
  },

  activity: (token: string) => request<Activity>('/telemetry/lambdas', withToken(token)),

  admin: {
    list: (token: string, search: string, page: number, tier = '') =>
      request<AdminListing>(
        `/admin/lambdas?page=${page}${search === '' ? '' : `&search=${encodeURIComponent(search)}`}${tier === '' ? '' : `&tier=${tier}`}`,
        withToken(token),
      ),

    lambda: (token: string, publicKey: string) =>
      request<AdminLambdaDetail>(`/admin/lambdas/${encodeURIComponent(publicKey)}`, withToken(token)),

    tier: (token: string, publicKey: string, tier: string) =>
      request<AdminLambdaDetail>(`/admin/lambdas/${encodeURIComponent(publicKey)}/tier`,
        withToken(token, { method: 'PUT', body: JSON.stringify({ tier }) })),

    domain: (token: string, publicKey: string, domain: string | null) =>
      request<AdminLambdaDetail>(`/admin/lambdas/${encodeURIComponent(publicKey)}/domain`,
        withToken(token, { method: 'PUT', body: JSON.stringify({ domain }) })),

    deploy: (token: string, publicKey: string, version?: number) =>
      request<DeploymentResult>(`/admin/lambdas/${encodeURIComponent(publicKey)}/deployment/start`,
        withToken(token, { method: 'POST', body: JSON.stringify({ version: version ?? null }) }), [422]),

    version: (token: string, publicKey: string, version: number) =>
      request<VersionContent>(`/admin/lambdas/${encodeURIComponent(publicKey)}/versions/${version}`, withToken(token)),

    undeploy: (token: string, publicKey: string) =>
      request<void>(`/admin/lambdas/${encodeURIComponent(publicKey)}/deployment/stop`,
        withToken(token, { method: 'POST' })),

    remove: (token: string, publicKey: string) =>
      request<void>(`/admin/lambdas/${encodeURIComponent(publicKey)}`,
        withToken(token, { method: 'DELETE' })),

    settings: (token: string) => request<AdminSettings>('/admin/settings', withToken(token)),

    saveSettings: (token: string, settings: AdminSettings) =>
      request<AdminSettings>('/admin/settings', withToken(token, { method: 'PUT', body: JSON.stringify(settings) })),
  },

  /**
   * The tail of the log. Behind the token without exception - unlike the
   * figures, this is whatever somebody's code decided to print.
   */
  logs: (
    token: string,
    options: { since?: number; lambda?: string; level?: string; client?: string; limit?: number } = {},
  ) => {
    const query = new URLSearchParams();

    // no cursor means "whatever is there now", which the server answers with
    // the tail rather than the whole ring
    if (options.since !== undefined) query.set('since', String(options.since));
    if (options.lambda) query.set('lambda', options.lambda);
    if (options.level) query.set('level', options.level);
    if (options.client) query.set('client', options.client);
    if (options.limit) query.set('limit', String(options.limit));

    return request<LogPage>(`/logs?${query}`, withToken(token));
  },

  /** Everyone the log still holds something about, busiest first. */
  logCallers: (token: string, limit = 500) =>
    request<LogCaller[]>(`/logs/callers?limit=${limit}`, withToken(token)),

  telemetry: (minutes: number, token: string) =>
    request<Telemetry>(`/telemetry?minutes=${minutes}&days=30`, withToken(token)),

  showcases: (skip = 0, take = 12) => request<ShowcaseListing>(`/showcases/?skip=${skip}&take=${take}`),

  /** The agent of the installation, changing one lambda for whoever holds its key. */
  agent: {
    state: (privateKey: string) => request<AgentState>(`/lambdas/${privateKey}/agent`),
    start: (privateKey: string, change: ChangeRequest) =>
      request<AgentState>(`/lambdas/${privateKey}/agent/start`, send(change)),
    stop: (privateKey: string) => request<AgentState>(`/lambdas/${privateKey}/agent/stop`, { method: 'POST' }),
  },

  showcase: (privateKey: string) => request<OwnShowcase>(`/lambdas/${privateKey}/showcase`),

  /** Leave the image out to keep the one there is. */
  saveShowcase: (privateKey: string, entry: { title: string; description: string; image?: string }) =>
    request<ShowcaseEntry>(`/lambdas/${privateKey}/showcase`, { method: 'PUT', body: JSON.stringify(entry) }),

  removeShowcase: (privateKey: string) => request<void>(`/lambdas/${privateKey}/showcase`, { method: 'DELETE' }),

  domain: (privateKey: string) => request<DomainState>(`/lambdas/${privateKey}/domain`),

  setDomain: (privateKey: string, domain: string) =>
    request<DomainState>(`/lambdas/${privateKey}/domain`, { method: 'PUT', body: JSON.stringify({ domain }) }),

  removeDomain: (privateKey: string) => request<DomainState>(`/lambdas/${privateKey}/domain`, { method: 'DELETE' }),

  key: (key: string) => request<KeyStatus>(`/keys/${encodeURIComponent(key)}`),

  create: (publicKey: string | null, template: string | null = null) =>
    request<Lambda>('/lambdas', send({ publicKey, acceptedTerms: true, template })),

  get: (privateKey: string) => request<Lambda>(`/lambdas/${privateKey}`),

  changeKey: (privateKey: string, publicKey: string) =>
    request<Lambda>(`/lambdas/${privateKey}`, { method: 'PATCH', body: JSON.stringify({ publicKey }) }),

  remove: (privateKey: string) => request<void>(`/lambdas/${privateKey}`, { method: 'DELETE' }),

  /** Where the project zip is. A plain link, so the browser does the saving. */
  exportUrl: (privateKey: string) => `${base}/lambdas/${privateKey}/export`,

  versions: (privateKey: string) => request<VersionInfo[]>(`/lambdas/${privateKey}/versions`),

  version: (privateKey: string, version: number) =>
    request<VersionContent>(`/lambdas/${privateKey}/versions/${version}`),

  /** Stores a version; the change and the specification are the why, kept beside the what. */
  save: (privateKey: string, files: LambdaFile[], change?: string, specification?: string) =>
    request<VersionInfo>(`/lambdas/${privateKey}/versions`, send({ files, change: change || null, specification: specification || null })),

  deployment: (privateKey: string) => request<Deployment>(`/lambdas/${privateKey}/deployment`),

  /** Every stretch of time something was online, newest first. */
  deployments: (privateKey: string) => request<Activation[]>(`/lambdas/${privateKey}/deployment/history`),

  summary: (privateKey: string) => request<LambdaSummary>(`/lambdas/${privateKey}/summary`),

  traffic: (privateKey: string) => request<LambdaTraffic>(`/lambdas/${privateKey}/traffic`),

  /** The lambda's own log. No cursor answers with the tail. */
  lambdaLogs: (privateKey: string, options: { since?: number; level?: string; limit?: number } = {}) => {
    const query = new URLSearchParams();

    if (options.since !== undefined) query.set('since', String(options.since));
    if (options.level) query.set('level', options.level);
    if (options.limit) query.set('limit', String(options.limit));

    return request<OwnerLogPage>(`/lambdas/${privateKey}/logs?${query}`);
  },

  deploy: (privateKey: string, version?: number) =>
    request<DeploymentResult>(`/lambdas/${privateKey}/deployment/start`, send({ version: version ?? null }), [422]),

  undeploy: (privateKey: string) =>
    request<Lambda>(`/lambdas/${privateKey}/deployment/stop`, { method: 'POST' }),

  /*
   * A workspace path travels as one segment with its slashes encoded, so
   * "logs/today.txt" is asked for as "logs%2Ftoday.txt".
   */
  files: (privateKey: string) => request<WorkspaceListing>(`/lambdas/${privateKey}/files`),

  createFolder: (privateKey: string, path: string) =>
    request<WorkspaceListing>(`/lambdas/${privateKey}/folders/${encodeURIComponent(path)}`, { method: 'PUT' }),

  readFile: (privateKey: string, path: string) =>
    request<{ path: string; content: string; size: number }>(
      `/lambdas/${privateKey}/files/${encodeURIComponent(path)}`,
    ),

  /** A file as it is, streamed: for a download link, and for anything too large to send as base64. */
  fileUrl: (privateKey: string, path: string) => `${base}/lambdas/${privateKey}/files/${encodeURIComponent(path)}/content`,

  /** Sends a file as it is, streamed to the disk however large it is. */
  uploadFile: (privateKey: string, path: string, content: Blob) =>
    request<WorkspaceEntry>(`/lambdas/${privateKey}/files/${encodeURIComponent(path)}/content`, {
      method: 'PUT',
      body: content,
      headers: { 'Content-Type': 'application/octet-stream' },
    }),

  deleteFile: (privateKey: string, path: string) =>
    request<void>(`/lambdas/${privateKey}/files/${encodeURIComponent(path)}`, { method: 'DELETE' }),

  /** Every kind of data there is, and how the lambda has it. */
  data: (privateKey: string) => request<DataStore[]>(`/lambdas/${privateKey}/data`),

  /** Switches a kind of data on. */
  enableData: (privateKey: string, kind: string) =>
    request<DataStore>(`/lambdas/${privateKey}/data/${encodeURIComponent(kind)}`, { method: 'PUT' }),

  /** Switches a kind of data off, deleting everything it held. */
  disableData: (privateKey: string, kind: string) =>
    request<DataStore>(`/lambdas/${privateKey}/data/${encodeURIComponent(kind)}`, { method: 'DELETE' }),

  /**
   * The features of a lambda: changes worked on beside it, each with a copy
   * of the data and a preview of its own, until they are merged or deleted.
   */
  feature: {
    list: (privateKey: string) => request<Feature[]>(`/lambdas/${privateKey}/features`),

    /** Starts one from a version - the newest when none is named. */
    create: (privateKey: string, name: string, specification?: string, base?: number) =>
      request<Feature>(`/lambdas/${privateKey}/features`, send({ name, specification: specification || null, base: base ?? null })),

    get: (privateKey: string, feature: string) => request<FeatureContent>(`/lambdas/${privateKey}/features/${feature}`),

    /** What is left out stays as it is. */
    update: (privateKey: string, feature: string, update: { name?: string; specification?: string; change?: string; base?: number }) =>
      request<Feature>(`/lambdas/${privateKey}/features/${feature}`, { method: 'PATCH', body: JSON.stringify(update) }),

    remove: (privateKey: string, feature: string) => request<void>(`/lambdas/${privateKey}/features/${feature}`, { method: 'DELETE' }),

    /**
     * Replaces every file it holds, and puts its preview online as well if
     * asked. Given the save the files were made from, it is refused with 409
     * when the feature was saved again since.
     */
    save: (privateKey: string, feature: string, files: LambdaFile[], deploy = false, revision?: number) =>
      request<FeatureSaved>(`/lambdas/${privateKey}/features/${feature}/files${deploy ? '?deploy=true' : ''}`, {
        method: 'PUT',
        body: JSON.stringify({ files, revision: revision ?? null }),
      }),

    /** Puts what it holds online at its preview address. A failure answers 422, and the preview stays as it was. */
    preview: (privateKey: string, feature: string) =>
      request<FeaturePreview>(`/lambdas/${privateKey}/features/${feature}/preview/start`, { method: 'POST' }, [422]),

    stop: (privateKey: string, feature: string) =>
      request<Feature>(`/lambdas/${privateKey}/features/${feature}/preview/stop`, { method: 'POST' }),

    /** What its preview has been doing, kept apart from the lambda's own log. */
    logs: (privateKey: string, feature: string, options: { since?: number; level?: string; limit?: number } = {}) => {
      const query = new URLSearchParams();

      if (options.since !== undefined) query.set('since', String(options.since));
      if (options.level) query.set('level', options.level);
      if (options.limit) query.set('limit', String(options.limit));

      return request<OwnerLogPage>(`/lambdas/${privateKey}/features/${feature}/logs?${query}`);
    },

    /** Its copy of the lambda's data, kind by kind. */
    data: (privateKey: string, feature: string) => request<DataStore[]>(`/lambdas/${privateKey}/features/${feature}/data`),

    /** Throws its copy of the data away and copies the lambda's again. */
    refresh: (privateKey: string, feature: string) =>
      request<Feature>(`/lambdas/${privateKey}/features/${feature}/data/refresh`, { method: 'POST' }),

    /**
     * Makes it the next version and deletes it. Answers 422 while its code
     * does not compile, and refuses with 409 while it is not based on the
     * newest version.
     */
    merge: (privateKey: string, feature: string, merge: { deploy: boolean; change?: string; specification?: string }) =>
      request<FeatureMerge>(`/lambdas/${privateKey}/features/${feature}/merge`, send({
        deploy: merge.deploy,
        change: merge.change || null,
        specification: merge.specification || null,
      }), [422]),

    zipUrl: (privateKey: string, feature: string) => `${base}/lambdas/${privateKey}/features/${feature}/zip`,

    /** Its copy of the workspace, addressed the way the lambda's own is. */
    workspace: {
      list: (privateKey: string, feature: string) =>
        request<WorkspaceListing>(`/lambdas/${privateKey}/features/${feature}/workspace`),

      read: (privateKey: string, feature: string, path: string) =>
        request<{ path: string; content: string; size: number }>(
          `/lambdas/${privateKey}/features/${feature}/workspace/${encodeURIComponent(path)}`,
        ),

      url: (privateKey: string, feature: string, path: string) =>
        `${base}/lambdas/${privateKey}/features/${feature}/workspace/${encodeURIComponent(path)}/content`,

      upload: (privateKey: string, feature: string, path: string, content: Blob) =>
        request<WorkspaceEntry>(`/lambdas/${privateKey}/features/${feature}/workspace/${encodeURIComponent(path)}/content`, {
          method: 'PUT',
          body: content,
          headers: { 'Content-Type': 'application/octet-stream' },
        }),

      remove: (privateKey: string, feature: string, path: string) =>
        request<void>(`/lambdas/${privateKey}/features/${feature}/workspace/${encodeURIComponent(path)}`, { method: 'DELETE' }),

    },
  },

  /*
   * Questions for the compiler. They share one request shape; the ones about
   * a single file are sent just that file, which is all the editor's
   * providers have at hand.
   */
  check: (privateKey: string, files: LambdaFile[]) =>
    request<CompilationResult>(`/lambdas/${privateKey}/code/check`, send({ files })),

  semantics: (privateKey: string, code: string) =>
    request<{ tokens: SemanticToken[] }>(`/lambdas/${privateKey}/code/semantics`, send({ files: single(code) })),

  completions: (privateKey: string, code: string, line: number, column: number) =>
    request<{ completions: ResolvedCompletion[] }>(`/lambdas/${privateKey}/code/completions`,
      send({ files: single(code), line, column })),

  definition: (privateKey: string, files: LambdaFile[], file: string, line: number, column: number) =>
    request<{ file: string | null; line: number; column: number; length: number }>(
      `/lambdas/${privateKey}/code/definition`,
      send({ files, file, line, column })),
};
