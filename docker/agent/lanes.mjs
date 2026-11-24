/*
 * The order jobs run in.
 *
 * Two lanes, because one of them has no end. A job asked for with the
 * password (unbounded) runs without a turn limit and without a clock, which
 * is the point of it - so sharing the lane of the public text box, it would
 * hold that shut for as long as it felt like running. Neither lane waits on
 * the other.
 *
 *   quick   the jobs on the clock: at most AGENT_CONCURRENCY at once, one
 *           unless the operator gives the machine more.
 *   long    the unbounded ones: at most one per installation at once, as it
 *           always was for the one installation a runner had.
 *
 * Where several installations share the runner, a free place goes to the
 * installation with the fewest jobs running in the lane, and among its jobs
 * to the oldest - so one busy installation does not keep the others' builds
 * waiting behind all of its own. With one installation that is first come,
 * first served.
 *
 * Kept apart from builder.mjs, which starts a server as it is loaded, so that
 * the order can be checked on its own (lanes.test.mjs).
 */

export class Lane {

  /**
   * @param {number} limit how many jobs of the lane run at once, of all installations
   * @param {number} each how many of them one installation may have running
   */
  constructor(limit, each = Infinity) {
    this.limit = limit;
    this.each = each;

    /** The jobs waiting, oldest first. */
    this.queue = [];

    /** How many jobs run, by installation. */
    this.running = new Map();
  }

  /** How many jobs run now, of every installation. */
  get busy() {
    let total = 0;

    for (const count of this.running.values()) total += count;

    return total;
  }

  /** How many jobs of an installation run now. */
  runningOf(host) {
    return this.running.get(host) ?? 0;
  }

  /** How many jobs of an installation wait. */
  waitingOf(host) {
    return this.queue.filter(job => job.host === host).length;
  }

  /** Puts a job at the end of the queue. */
  add(job) {
    this.queue.push(job);
  }

  /** Takes a job that has not started off the queue; whether it was on it. */
  remove(job) {
    const at = this.queue.indexOf(job);

    if (at < 0) return false;

    this.queue.splice(at, 1);
    return true;
  }

  /**
   * How many jobs are ahead of one that waits, counting itself - which is
   * what the pages say as its place - or 0 where it does not wait.
   *
   * The queue's order is a fair estimate rather than a promise: a job of an
   * installation with nothing running may be taken before older ones.
   */
  placeOf(job) {
    return this.queue.indexOf(job) + 1;
  }

  /**
   * The job to start next, taken off the queue and counted as running - or
   * nothing, where the lane is full or nothing that waits may start.
   *
   * Counting it here, rather than when it actually starts, means a second
   * call before it does cannot hand out the same place twice.
   */
  take() {
    if (this.busy >= this.limit) return null;

    let best = -1;

    for (let at = 0; at < this.queue.length; at++) {
      const count = this.runningOf(this.queue[at].host);

      if (count >= this.each) continue;

      // strictly fewer, so the oldest of the installation wins a tie
      if (best < 0 || count < this.runningOf(this.queue[best].host)) best = at;
    }

    if (best < 0) return null;

    const [job] = this.queue.splice(best, 1);

    this.running.set(job.host, this.runningOf(job.host) + 1);

    return job;
  }

  /** Gives back the place of a job that took one, once it has ended. */
  done(job) {
    const count = this.runningOf(job.host) - 1;

    if (count > 0) this.running.set(job.host, count);
    else this.running.delete(job.host);
  }

}
