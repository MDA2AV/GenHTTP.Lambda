/*
 * The agent's only way off the machine.
 *
 * It is a CONNECT proxy and nothing else: no caching, no plain HTTP
 * forwarding, no rewriting. The agent sits on a network with no route out, so
 * the set of hosts named here is the complete list of places anything it does
 * can reach - which is the point of it, and is easier to be sure of in fifty
 * lines than in a configuration file for a general purpose proxy.
 */

import { createServer } from 'node:http';
import { connect } from 'node:net';

const PORT = Number(process.env.PROXY_PORT ?? 3128);

const ALLOWED = (process.env.PROXY_ALLOW ?? 'api.anthropic.com,console.anthropic.com,statsig.anthropic.com,claude.ai')
  .split(',')
  .map(h => h.trim().toLowerCase())
  .filter(Boolean);

const permitted = host =>
  ALLOWED.some(allowed => host === allowed || host.endsWith('.' + allowed));

const server = createServer((req, res) => {
  // anything that is not CONNECT is a plain request, and the agent has no
  // business making one: everything it legitimately does is TLS
  res.writeHead(405, { 'content-type': 'text/plain' });
  res.end('this proxy only does CONNECT\n');
});

server.on('connect', (req, client, head) => {
  const [host, port = '443'] = String(req.url ?? '').split(':');
  const name = host.toLowerCase();

  if (!permitted(name) || port !== '443') {
    console.log(`refused ${req.url}`);
    client.write('HTTP/1.1 403 Forbidden\r\n\r\n');
    return client.destroy();
  }

  const upstream = connect(Number(port), name, () => {
    client.write('HTTP/1.1 200 Connection Established\r\n\r\n');
    if (head?.length) upstream.write(head);
    upstream.pipe(client);
    client.pipe(upstream);
  });

  const drop = () => { upstream.destroy(); client.destroy(); };

  upstream.on('error', drop);
  client.on('error', drop);

  console.log(`allowed ${name}`);
});

server.listen(PORT, () => console.log(`egress proxy on ${PORT}, allowing: ${ALLOWED.join(', ')}`));
