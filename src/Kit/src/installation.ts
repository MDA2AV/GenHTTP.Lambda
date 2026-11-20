import { useEffect, useState } from 'react';

import { useProfile } from './profile';
import { askPlatform } from './site';

/** Where GenHTTP's community talks - the project's, whoever runs an installation. */
export const DISCORD = 'https://discord.gg/PRkwKrnrB4';

/**
 * Where the source of GenHTTP Lambda is - the platform itself, MIT licensed -
 * which every installation runs, so every installation links to it.
 */
export const REPOSITORY = 'https://github.com/MDA2AV/GenHTTP.Lambda';

/**
 * Whether the pages link to the platform's project - its source on GitHub
 * and its community on Discord: on every installation but one an
 * organization runs for its people (see profile.ts), whose colleagues come
 * to build their tools rather than to find the platform.
 */
export function useProjectLinks(): boolean {
  return useProfile() !== 'enterprise';
}

/**
 * Who runs this installation, as far as a page has to say so: where to write
 * to them, where to report abuse, and whether it has terms, a privacy policy
 * and an imprint to link to.
 *
 * Differs between installations: genhttp.dev has all of it, an installation
 * somebody runs for themselves usually none. A site that knows its own when it
 * is built names them (defineInstallation), so its rendered pages carry them;
 * a component every installation shares - the editor, the published sources
 * - asks the server.
 */
export interface Installation {
  /** Where to write to whoever runs it - about premium, for one. */
  contact: string | null;
  /** Where to report a lambda that does harm. */
  abuse: string | null;
  /** Whether it has its terms, privacy policy and imprint at /terms, /privacy and /imprint. */
  legal: boolean;
  /** The page of its site where a visitor has an app built - the workspace. */
  start: string;
}

let known: Installation | null = null;

/** Names this site's installation, as the bundle starts - for a site that knows it when it is built. */
export function defineInstallation(installation: Installation) {
  known = installation;
}

/** What the installation says about who runs it: what the site named, or what the server says - nothing until then. */
export function useInstallation(): Installation | null {
  const [asked, setAsked] = useState<Installation | null>(null);

  useEffect(() => {
    if (known !== null) {
      return;
    }

    askPlatform()
      .then((platform) => setAsked(platform.operator))
      .catch(() => undefined);
  }, []);

  return known ?? asked;
}

/**
 * Where a visitor has an app built on this installation's site: the
 * workspace at /build, unless the installation says otherwise.
 */
export function useStartPage(): string {
  return useInstallation()?.start ?? '/build';
}
