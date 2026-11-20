import { useSyncExternalStore } from 'react';

import { askPlatform, sentProfile } from './site';

/**
 * Who the installation is for: genhttp.dev's public (cloud), somebody and
 * the people around them (home), or an organization and its people
 * (enterprise) - which its operator chooses in the panel, where its host lets
 * them.
 *
 * It changes how the pages speak, never what they do: a section of a catalog
 * may say some of its words its own way for a profile (see wordsFor), and the
 * catalogs hand every page the words of the installation's profile - so no
 * component asks which profile it is. The one thing that is not words, the
 * links to the platform's project, has a hook of its own (useProjectLinks).
 *
 * A page the server rendered says which profile it was rendered for (in
 * `#site-facts`, see site.ts), so the first render draws the same words. Any
 * other asks the server once, and speaks as every profile does until it knows.
 */
export type Profile = 'cloud' | 'home' | 'enterprise';

const PROFILES: readonly Profile[] = ['cloud', 'home', 'enterprise'];

/** Whether something the server said is a profile this build knows. */
export const isProfile = (value: unknown): value is Profile => PROFILES.includes(value as Profile);

let rendering: Profile | null | undefined;

/** Called by the build before rendering a page, never in a browser: a profile, or nothing for the words every profile shares. */
export function prerenderProfile(profile: Profile | null) {
  rendering = profile;
}

let current: Profile | null | undefined;

let asking = false;

const listeners = new Set<() => void>();

/** The profile as far as it is known: the one being rendered, the one the page was sent for, or the one the server said. */
function read(): Profile | null {
  if (rendering !== undefined) {
    return rendering;
  }

  if (current === undefined) {
    const sent = sentProfile();

    current = isProfile(sent) ? sent : null;
  }

  return current;
}

/** Asks the server which profile it is, once, where the page was not sent with one. */
function ask() {
  if (read() !== null || asking) {
    return;
  }

  asking = true;

  askPlatform()
    .then((platform) => {
      if (isProfile(platform.profile)) {
        current = platform.profile;
        listeners.forEach((listener) => listener());
      }
    })
    .catch(() => {
      // asked again by the next page that wants to know
      asking = false;
    });
}

const subscribe = (changed: () => void) => {
  listeners.add(changed);

  ask();

  return () => listeners.delete(changed);
};

/**
 * Who the installation is for, as far as the page knows - nothing until it
 * does, which reads as the words every profile shares.
 */
export function useProfile(): Profile | null {
  return useSyncExternalStore(subscribe, read, read);
}

/** What a section of a catalog says its own way for some profiles, and only that. */
export type ProfileWords<T> = Partial<Record<Profile, DeepPartial<T>>>;

type DeepPartial<T> = T extends readonly unknown[] | ((...args: never[]) => unknown) ? T : T extends object ? { [K in keyof T]?: DeepPartial<T[K]> } : T;

const isPlain = (value: unknown): value is Record<string, unknown> =>
  typeof value === 'object' && value !== null && !Array.isArray(value);

/** One section with a profile's words over its own: objects merged, everything else - a sentence, a list, a function - replaced. */
function over(shared: Record<string, unknown>, own: Record<string, unknown>): Record<string, unknown> {
  const merged: Record<string, unknown> = { ...shared };

  for (const [key, value] of Object.entries(own)) {
    merged[key] = isPlain(value) && isPlain(shared[key]) ? over(shared[key], value) : value;
  }

  return merged;
}

/**
 * A catalog in the words of a profile: each of its sections with what it
 * says for that profile (its `profiles`, see ProfileWords) over what it says
 * for every profile - the catalog as it is where the profile says nothing of
 * its own, or nothing is known yet.
 */
export function wordsFor<T extends object>(catalog: T, profile: Profile | null): T {
  if (profile === null) {
    return catalog;
  }

  let changed = false;

  const sections = Object.entries(catalog).map(([key, section]) => {
    const own = isPlain(section) && isPlain(section.profiles) ? section.profiles[profile] : undefined;

    if (!isPlain(own)) {
      return [key, section] as const;
    }

    changed = true;

    return [key, over(section as Record<string, unknown>, own)] as const;
  });

  return changed ? (Object.fromEntries(sections) as T) : catalog;
}
