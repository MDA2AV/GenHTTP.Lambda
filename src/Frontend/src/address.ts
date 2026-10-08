/**
 * Where a lambda answers, and how that address reads.
 *
 * Every lambda answers at an address of its own - a subdomain of the hosting
 * domain named after its key, https://quiz.genhttp.run/ - or at a domain of
 * its own while its tier serves one. Which one to link to is decided by the
 * server, which sends it along as the `address` of a lambda, a showcase entry
 * or a source; the addresses are always complete. What is here only builds and
 * prints them, so the rule stays in one place.
 */

/** The key in the address template the server sends with the platform. */
const KEY = '{key}';

/** The address of the lambda with a key, from the template: https://{key}.genhttp.run/. */
export const lambdaAddress = (template: string, publicKey: string) => template.split(KEY).join(publicKey);

/**
 * What an address reads around the key, without the scheme and the slash at
 * its end - "" and ".genhttp.run" - to put either side of a field the key is
 * typed into.
 */
export function aroundKey(template: string): { before: string; after: string } {
  const shown = shownAddress(template);
  const at = shown.indexOf(KEY);

  return at < 0 ? { before: '', after: '' } : { before: shown.slice(0, at), after: shown.slice(at + KEY.length) };
}

/** Where a lambda answers at a domain of its own. */
export const domainAddress = (domain: string) => `https://${domain}/`;

/** A path on this site as a complete URL, to copy or to hand to somebody: a preview, a repository. */
export const absoluteAddress = (address: string) =>
  address.startsWith('/') ? `${window.location.origin}${address}` : address;

/** The address as it reads in a sentence: the host, without the scheme and the slash after it. */
export const shownAddress = (address: string) => address.replace(/^https?:\/\//, '').replace(/\/$/, '');

/** The host of an address without its port, as the log names it. */
export const hostOf = (address: string) => shownAddress(address).split('/')[0].split(':')[0];
