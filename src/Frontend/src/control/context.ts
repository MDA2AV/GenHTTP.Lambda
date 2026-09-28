import type { Diagnostic, Feature, Lambda, LambdaFile, LambdaSummary, VersionInfo } from '../api';
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
  /** The features being worked on, most recently changed first. */
  features: Feature[];
  /** What is running right now, so buttons can say so and wait. */
  busy: Busy;
  theme: Theme;
  /** Reads the lambda, its versions, its features and the summary again. */
  refresh: () => Promise<void>;
  /** Puts a version online - the newest when none is named. */
  deploy: (version?: number) => Promise<boolean>;
  undeploy: () => Promise<void>;
  /** Opens a version in the code view - or, in a feature, the feature's code. */
  edit: (version?: number) => void;
  /** Opens a version in the files view. */
  browse: (version?: number) => void;
  /** The agent changing this lambda, followed wherever the owner is. */
  agent: AgentControl;
  /** Opens the data of the lambda, which no version holds. */
  openData: () => void;
  /**
   * Asks for a new feature, starting from a version - the newest when none is
   * named - and holding the files given instead of that version's, if any.
   */
  startFeature: (base?: number, files?: LambdaFile[]) => void;
  /** Opens a feature, at one of its views. */
  openFeature: (key: string, view?: FeatureView) => void;
  /**
   * Asks to put a feature online - to merge it, and deploy the version it
   * becomes - wherever the owner is: the frame holds the dialog.
   */
  putOnline: (key: string) => void;
  /** Asks the agent to go on with a feature, with a request to start from if there is an obvious one. */
  askAgent: (feature?: string, prompt?: string) => void;
  /** The feature being worked on, when the control center is opened on one. */
  feature: FeatureControl | null;
}

/** The views of a feature: what it is and changes, its code, its copy of the data, what its preview said. */
export type FeatureView = 'overview' | 'code' | 'data' | 'logs';

/**
 * A feature the control center is opened on. The frame holds it, so the
 * sidebar, the header and the views all show the same state of it, and a
 * preview deployed from the code view is the one the sidebar links to.
 */
export interface FeatureControl {
  info: Feature;
  /** Reads the feature again, and the lambda with it. */
  refresh: () => Promise<void>;
  /** Puts what the feature holds online at its preview address, with the frame's feedback. */
  preview: () => Promise<boolean>;
  /** Whether its preview is being put online right now. */
  previewing: boolean;
}

export type Busy = 'deploy' | 'undeploy' | null;

/** The compiler's refusal of a deployment, shown by the frame. */
export interface Rejection {
  version?: number;
  /** Set when it was the preview of a feature that was refused. */
  feature?: string;
  diagnostics: Diagnostic[];
}

export type { Lambda };
