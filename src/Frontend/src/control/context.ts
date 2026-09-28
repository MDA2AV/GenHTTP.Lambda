import type { Diagnostic, Lambda, LambdaSummary, VersionInfo } from '../api';
import type { Theme } from '../theme';
import type { AgentControl } from './agent';

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
  /** The agent changing this lambda, followed wherever the owner is. */
  agent: AgentControl;
  /** Opens the data of the lambda, which no version holds. */
  openData: () => void;
  /**
   * Starts a new version as a copy of one - the newest when none is named -
   * and says so. The copy becomes the newest, so it is where work carries on.
   * Resolves to its number, or to nothing when it could not be made.
   */
  startVersion: (from?: number) => Promise<number | null>;
}

export type Busy = 'deploy' | 'undeploy' | null;

/** The compiler's refusal of a deployment, shown by the frame. */
export interface Rejection {
  version?: number;
  diagnostics: Diagnostic[];
}

export type { Lambda };
