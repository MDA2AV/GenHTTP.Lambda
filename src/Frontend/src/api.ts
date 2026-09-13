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

export interface VersionContent extends VersionInfo {
  code: string;
}

export interface Diagnostic {
  severity: string;
  id: string;
  message: string;
  line: number;
  column: number;
}

export interface CompilationResult {
  success: boolean;
  diagnostics: Diagnostic[];
}

export interface DeploymentResult extends CompilationResult {
  lambda?: Lambda;
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
}

export interface WorkspaceEntry {
  path: string;
  size: number;
  modified: string;
}

export interface WorkspaceListing {
  files: WorkspaceEntry[];
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
  openSockets: number;
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
  tier: string;
  created: string;
  modified: string;
  activeVersion?: number;
  latestVersion?: number;
  versions: number;
  deployedUntil?: string;
  keptUntil: string;
}

export interface AdminListing {
  lambdas: LambdaOverview[];
  total: number;
  deployed: number;
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

/** The panel is the only part of this API that authenticates. */
const withToken = (token: string, init: RequestInit = {}) => ({
  ...init,
  headers: { ...init.headers, 'X-Admin-Token': token },
});

export const api = {
  platform: () => request<Platform>('/system'),

  activity: () => request<Activity>('/telemetry/lambdas'),

  admin: {
    list: (token: string) => request<AdminListing>('/admin/lambdas', withToken(token)),

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

  telemetry: (minutes: number) => request<Telemetry>(`/telemetry?minutes=${minutes}`),

  checkKey: (key: string) => request<Availability>(`/lambdas/keys/${encodeURIComponent(key)}`),

  publicStatus: (key: string) => request<PublicStatus>(`/lambdas/public/${encodeURIComponent(key)}`),

  create: (publicKey: string | null, template: string | null = null) =>
    request<Lambda>('/lambdas', send({ publicKey, acceptedTerms: true, template })),

  get: (privateKey: string) => request<Lambda>(`/lambdas/${privateKey}`),

  files: (privateKey: string) => request<WorkspaceListing>(`/lambdas/${privateKey}/files`),

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

  save: (privateKey: string, code: string) =>
    request<VersionInfo>(`/lambdas/${privateKey}/versions`, send({ code })),

  check: (privateKey: string, code: string) =>
    request<CompilationResult>(`/lambdas/${privateKey}/check`, send({ code })),

  deploy: (privateKey: string, version?: number) =>
    request<DeploymentResult>(`/lambdas/${privateKey}/deployment`, send({ version: version ?? null }), [422]),

  undeploy: (privateKey: string) =>
    request<Lambda>(`/lambdas/${privateKey}/deployment`, { method: 'DELETE' }),

  changeKey: (privateKey: string, publicKey: string) =>
    request<Lambda>(`/lambdas/${privateKey}/key`, { method: 'PUT', body: JSON.stringify({ publicKey }) }),

  remove: (privateKey: string) => request<void>(`/lambdas/${privateKey}`, { method: 'DELETE' }),
};
