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
  /** When the deployment will be taken offline again. */
  deployedUntil?: string;
  /** When an untouched lambda is removed altogether. */
  keptUntil: string;
}

export interface VersionInfo {
  version: number;
  created: string;
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
  /** The snippet, which is the first of the files. */
  code: string;
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

export interface Availability {
  publicKey: string;
  available: boolean;
  reason?: string;
}

export interface PublicStatus {
  publicKey: string;
  exists: boolean;
  deployed: boolean;
}

export interface Completion {
  label: string;
  kind: string;
  detail: string;
  insert?: string;
}

export interface Template {
  id: string;
  name: string;
  description: string;
  code: string;
  /** Reachable by link, but not offered in the picker. */
  hidden: boolean;
}

export interface TemplateGroup {
  id: string;
  name: string;
  description: string;
  templates: Template[];
}

export interface Platform {
  terms: string;
  templates: TemplateGroup[];
  maxCodeLength: number;
  deploymentLifetimeHours: number;
  retentionDays: number;
  imports: string[];
  completions: Completion[];
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

export interface Example {
  id: string;
  name: string;
  description: string;
  publicKey: string;
  path: string;
  /** What is worth calling underneath it, which is rarely the root. */
  tryPath: string;
  /** Reached by opening a socket rather than by asking for a page. */
  socket: boolean;
  code: string;
  /** Every file it is made of, the snippet first. */
  files: LambdaFile[];
  /** They are prepared after startup, so one can exist but not yet answer. */
  live: boolean;
}

/** What the menu needs: everything but the code. */
export type ExampleSummary = Omit<Example, 'code' | 'files'>;

export interface ExampleGroup {
  id: string;
  name: string;
  examples: ExampleSummary[];
}

export interface ExampleListing {
  groups: ExampleGroup[];
}

export interface WorkspaceEntry {
  path: string;
  size: number;
  modified: string;
}

export interface WorkspaceListing {
  files: WorkspaceEntry[];
  folders: string[];
  usedBytes: number;
  quotaBytes: number;
  maxFiles: number;
  maxFileSize: number;
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
  keptUntil: string;
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

/** The panel and the figures behind it are the parts of this API that authenticate. */
const withToken = (token: string, init: RequestInit = {}) => ({
  ...init,
  headers: { ...init.headers, 'X-Admin-Token': token },
});

export const api = {
  platform: () => request<Platform>('/system'),

  /** The text box on /build, and the agent behind it. */
  build: {
    available: () => request<{ available: boolean; perDay: number; secondModel: boolean }>('/build'),
    start: (prompt: string, editor?: string, model?: string, password?: string) =>
      request<{ id: string }>('/build', send({ prompt, editor, model, password })),
    progress: (id: string) =>
      request<{ state: string; events: string[]; result: BuildResult | null }>(`/build/${id}`),
  },

  activity: (token: string) => request<Activity>('/telemetry/lambdas', withToken(token)),

  admin: {
    list: (token: string, search: string, page: number) =>
      request<AdminListing>(
        `/admin/lambdas?page=${page}${search === '' ? '' : `&search=${encodeURIComponent(search)}`}`,
        withToken(token),
      ),

    code: (token: string, publicKey: string, version?: number) =>
      request<VersionContent>(
        `/admin/lambdas/${encodeURIComponent(publicKey)}/code${version ? `?version=${version}` : ''}`,
        withToken(token),
      ),

    undeploy: (token: string, publicKey: string) =>
      request<void>(`/admin/lambdas/${encodeURIComponent(publicKey)}/deployment`,
        withToken(token, { method: 'DELETE' })),

    remove: (token: string, publicKey: string) =>
      request<void>(`/admin/lambdas/${encodeURIComponent(publicKey)}`,
        withToken(token, { method: 'DELETE' })),
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

  examples: () => request<ExampleListing>('/examples'),

  example: (id: string) => request<Example>(`/examples/${encodeURIComponent(id)}`),

  checkKey: (key: string) => request<Availability>(`/lambdas/keys/${encodeURIComponent(key)}`),

  publicStatus: (key: string) => request<PublicStatus>(`/lambdas/public/${encodeURIComponent(key)}`),

  create: (publicKey: string | null, template: string | null = null) =>
    request<Lambda>('/lambdas', send({ publicKey, acceptedTerms: true, template })),

  get: (privateKey: string) => request<Lambda>(`/lambdas/${privateKey}`),

  files: (privateKey: string) => request<WorkspaceListing>(`/lambdas/${privateKey}/files`),

  createFolder: (privateKey: string, path: string) =>
    request<WorkspaceListing>(
      `/lambdas/${privateKey}/files/folder?path=${encodeURIComponent(path)}`,
      { method: 'PUT' }),

  readFile: (privateKey: string, path: string) =>
    request<{ path: string; content: string; size: number }>(
      `/lambdas/${privateKey}/files/content?path=${encodeURIComponent(path)}`,
    ),

  writeFile: (privateKey: string, path: string, content: string) =>
    request<WorkspaceEntry>(`/lambdas/${privateKey}/files/content?path=${encodeURIComponent(path)}`, {
      method: 'PUT',
      body: JSON.stringify({ content }),
    }),

  deleteFile: (privateKey: string, path: string) =>
    request<void>(`/lambdas/${privateKey}/files/content?path=${encodeURIComponent(path)}`, { method: 'DELETE' }),

  versions: (privateKey: string) => request<VersionInfo[]>(`/lambdas/${privateKey}/versions`),

  version: (privateKey: string, version: number) =>
    request<VersionContent>(`/lambdas/${privateKey}/versions/${version}`),

  save: (privateKey: string, files: LambdaFile[]) =>
    request<VersionInfo>(`/lambdas/${privateKey}/versions`, send({ files })),

  completions: (privateKey: string, code: string, line: number, column: number) =>
    request<{ completions: ResolvedCompletion[] }>(`/lambdas/${privateKey}/completions`,
      send({ code, line, column })),

  /** Where the project zip is. A plain link, so the browser does the saving. */
  downloadUrl: (privateKey: string) => `${base}/lambdas/${privateKey}/download`,

  definition: (privateKey: string, files: LambdaFile[], file: string, line: number, column: number) =>
    request<{ file: string | null; line: number; column: number; length: number }>(
      `/lambdas/${privateKey}/definition`,
      send({ files, file, line, column })),

  semantics: (privateKey: string, code: string) =>
    request<{ tokens: SemanticToken[] }>(`/lambdas/${privateKey}/semantics`, send({ code })),

  check: (privateKey: string, files: LambdaFile[]) =>
    request<CompilationResult>(`/lambdas/${privateKey}/check`, send({ files })),

  deploy: (privateKey: string, version?: number) =>
    request<DeploymentResult>(`/lambdas/${privateKey}/deployment`, send({ version: version ?? null }), [422]),

  undeploy: (privateKey: string) =>
    request<Lambda>(`/lambdas/${privateKey}/deployment`, { method: 'DELETE' }),

  changeKey: (privateKey: string, publicKey: string) =>
    request<Lambda>(`/lambdas/${privateKey}/key`, { method: 'PUT', body: JSON.stringify({ publicKey }) }),

  remove: (privateKey: string) => request<void>(`/lambdas/${privateKey}`, { method: 'DELETE' }),
};
