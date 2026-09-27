/**
 * Where a lambda answers, and how that address reads.
 *
 * Which one to link to is decided by the server, which sends it along as the
 * `address` of a lambda and the `path` of a showcase entry: the root of its own
 * domain while its tier serves one, its path on the platform otherwise. What is
 * here only builds and prints addresses, so the rule stays in one place.
 */

/** Where a lambda answers on the platform. */
export const platformPath = (publicKey: string) => `/lambda/${publicKey}/`;

/** Where a lambda answers at a domain of its own. */
export const domainAddress = (domain: string) => `https://${domain}/`;

/** Whether an address leads to a domain rather than to a path on the platform. */
export const isDomain = (address: string) => !address.startsWith('/');

/** The address as a complete URL, to copy or to hand to somebody. */
export const absoluteAddress = (address: string) =>
  isDomain(address) ? address : `${window.location.origin}${address}`;

/** The address as it reads in a sentence: a domain bare, a path as it is. */
export const shownAddress = (address: string) =>
  isDomain(address) ? address.replace(/^https?:\/\//, '').replace(/\/$/, '') : address;
