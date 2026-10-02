import type { ReactNode } from 'react';

type Node = ReactNode;

export const domain = {
  readFailed: 'The domain could not be read.',
  reaching: (domain: string) => `Requests to ${domain} now reach this lambda.`,
  saveFailed: 'The domain could not be saved.',
  removed: 'The domain is removed. The lambda still answers at its address here.',
  removeFailed: 'The domain could not be removed.',
  hint:
    'A premium lambda can answer at a domain of its own - the whole of it, from the root down - as well as at its address here. Point the domain at this server, enter it here, and requests to it reach the lambda.',
  loading: 'Loading…',
  example: 'your-domain.com',
  open: (domain: string) => `Open ${domain}`,
  label: 'The domain it answers at',
  serving: (domain: Node) => <>Serving {domain} now, besides its address here.</>,
  none: 'None yet. A subdomain such as shop.example.com, or a whole domain such as example.com.',
  change: 'Change',
  use: 'Use this domain',
  remove: 'Remove',
  confirm: 'Remove the domain?',
  keep: 'Keep it',
  confirmText: (domain: Node) => (
    <>
      Requests to {domain} stop reaching this lambda at once. Its address here stays as it is, and so does whatever the
      domain's DNS says.
    </>
  ),
  point: 'Point the domain at this server',
  check: 'Check again',
  records:
    'At whoever manages the DNS of the domain, add these two records. Leave out the AAAA record if you would rather not be reachable over IPv6.',
  type: 'Type',
  name: 'Name',
  value: 'Value',
  pointsHere: (domain: Node) => <>{domain} points here.</>,
  alsoElsewhere: (addresses: string) =>
    ` It also resolves to ${addresses}, which is not this server - visitors sent there will not reach the lambda.`,
  elsewhere: (addresses: string) => `It resolves to ${addresses}, which is not this server yet.`,
  wait: 'A change can take a while to be seen everywhere - up to the time to live of the old record.',
  cname: 'Using a CNAME record instead',
  cnameText: (target: Node) => (
    <>
      A subdomain can point at {target} with a CNAME record instead, and then follows this server if its addresses ever
      change. It has drawbacks:
    </>
  ),
  cnameRoot: (example: Node) => (
    <>
      It cannot be used for a whole domain ({example} itself): the standard does not allow a CNAME next to the records
      every domain has at its root. Some providers offer an ALIAS, ANAME or "flattened" record that works there
      instead.
    </>
  ),
  cnameAlone: 'Nothing else can sit on the same name - no MX record for mail, no TXT record for verifications.',
  cnameLookup: "Visitors' resolvers make one more lookup before they arrive.",
  copy: 'Copy',
  copyValue: (value: string) => `Copy ${value}`,
};
