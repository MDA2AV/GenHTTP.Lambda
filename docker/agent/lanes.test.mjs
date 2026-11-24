/*
 * The order jobs run in: how many at once, and whose next.
 * CI runs it with node --test 'docker/agent/*.test.mjs'.
 */

import { test } from 'node:test';
import assert from 'node:assert/strict';

import { Lane } from './lanes.mjs';

const A = { name: 'a' };
const B = { name: 'b' };

const job = (host, id) => ({ host, id });

/** Takes until the lane gives nothing more, by id. */
function drain(lane) {
  const taken = [];
  let next;

  while ((next = lane.take()) !== null) taken.push(next.id);

  return taken;
}

test('one at a time by default, first come first served', () => {
  const lane = new Lane(1);

  lane.add(job(A, 1));
  lane.add(job(A, 2));

  const first = lane.take();

  assert.equal(first.id, 1);
  assert.equal(lane.take(), null);
  assert.equal(lane.placeOf(lane.queue[0]), 1);

  lane.done(first);

  assert.equal(lane.take().id, 2);
});

test('as many run at once as the lane allows', () => {
  const lane = new Lane(3);

  for (let id = 1; id <= 5; id++) lane.add(job(A, id));

  assert.deepEqual(drain(lane), [1, 2, 3]);
  assert.equal(lane.busy, 3);
  assert.equal(lane.runningOf(A), 3);
  assert.equal(lane.waitingOf(A), 2);
});

test('a free place goes to the installation with the fewest jobs running', () => {
  const lane = new Lane(2);

  lane.add(job(A, 'a1'));
  lane.add(job(A, 'a2'));
  lane.add(job(A, 'a3'));
  lane.add(job(B, 'b1'));

  // b1 is newer than a2, but A has a job running and B none
  assert.deepEqual(drain(lane), ['a1', 'b1']);

  lane.done({ host: A });

  // both have one running now: the oldest of the two goes first
  assert.deepEqual(drain(lane), ['a2']);
});

test('the unbounded lane runs one job per installation, and every installation one', () => {
  const lane = new Lane(Infinity, 1);

  lane.add(job(A, 'a1'));
  lane.add(job(A, 'a2'));
  lane.add(job(B, 'b1'));

  assert.deepEqual(drain(lane), ['a1', 'b1']);
  assert.equal(lane.waitingOf(A), 1);

  lane.done({ host: A });

  assert.deepEqual(drain(lane), ['a2']);
});

test('a job taken off the queue before it starts is gone from it', () => {
  const lane = new Lane(1);
  const waiting = job(A, 2);

  lane.add(job(A, 1));
  lane.add(waiting);

  lane.take();

  assert.equal(lane.remove(waiting), true);
  assert.equal(lane.remove(waiting), false);
  assert.equal(lane.placeOf(waiting), 0);
  assert.equal(lane.waitingOf(A), 0);
});
