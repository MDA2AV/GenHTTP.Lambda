import type { ReactNode } from 'react';

import { shownAddress } from '../address';

interface Props {
  /** Where the lambda answers, as the server sent it. */
  address: string;
  className?: string;
  /** What the link says; the address itself when left out. */
  children?: ReactNode;
}

/**
 * A link to a running lambda, opened in a new tab: it is somebody's app, not
 * a page of this one.
 */
export function LambdaLink({ address, className, children }: Props) {
  return (
    <a href={address} target="_blank" rel="noreferrer" className={className}>
      {children ?? shownAddress(address)}
    </a>
  );
}
