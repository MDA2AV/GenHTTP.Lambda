import { useEffect, useState, type ReactNode } from 'react';

import { askPlatform } from '@genhttp-lambda/kit/site';

/*
 * What the front pages of the community edition share - the one for whoever
 * runs it and the one for an organization's colleagues (see Landing).
 */

/** The light behind the top of a front page. */
export function Aurora() {
  return (
    <div className="aurora-field" aria-hidden="true">
      <div className="aurora aurora-a" />
      <div className="aurora aurora-b" />
      <div className="aurora-clearing" />
    </div>
  );
}

/** The heading of a section of a front page. */
export function Heading({ children, icon }: { children: ReactNode; icon?: ReactNode }) {
  return (
    <h2 className="flex items-center gap-2.5 text-2xl font-light tracking-tight">
      {icon && <span className="text-accent-600 dark:text-accent-400">{icon}</span>}
      {children}
    </h2>
  );
}

/**
 * Whether the box on /build has an agent behind it. Until the server says,
 * it is offered, so a page is rendered with it for whoever reads it
 * unscripted.
 */
export function useBuildOffered(): boolean | null {
  const [available, setAvailable] = useState<boolean | null>(null);

  useEffect(() => {
    askPlatform()
      .then((platform) => setAvailable(platform.build.available))
      .catch(() => setAvailable(false));
  }, []);

  return available;
}
