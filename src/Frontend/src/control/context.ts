import type { Diagnostic, Lambda, LambdaSummary, VersionInfo } from '../api';
import type { Theme } from '../theme';

/**
 * What every tab of the control center is handed: the lambda as last read,
 * and the few things that change it. The tabs read; the frame acts, so a
 * deployment started from the versions tab and one started from the header
 * are the same deployment with the same feedback.
 */
export interface Control {
  privateKey: string;
  lambda: Lambda;
  /** The dashboard figures, refreshed on an interval. Null until the first arrives. */
  summary: LambdaSummary | null;
  /** Every stored version, newest first. */
  versions: VersionInfo[];
  /** What is running right now, so buttons can say so and wait. */
  busy: Busy;
  theme: Theme;
  /** Reads the lambda, its versions and the summary again. */
  refresh: () => Promise<void>;
  /** Puts a version online - the newest when none is named. */
  deploy: (version?: number) => Promise<boolean>;
  undeploy: () => Promise<void>;
  /** Opens a version in the code view. */
  edit: (version?: number) => void;
  /** Opens a version in the files view. */
  browse: (version?: number) => void;
}

export type Busy = 'deploy' | 'undeploy' | null;

/** The compiler's refusal of a deployment, shown by the frame. */
export interface Rejection {
  version?: number;
  diagnostics: Diagnostic[];
}

export type { Lambda };
