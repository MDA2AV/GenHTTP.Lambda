import type { LambdaFile } from '../api';

/**
 * What changed between two versions, file by file and line by line.
 *
 * A longest common subsequence over the lines, after the prefix and suffix
 * the two share are set aside - which is nearly all of a file between two
 * versions an agent wrote a minute apart, and what keeps the table small
 * enough to fill. Past a size it gives up on lines and says so, rather than
 * making the page wait on a quadratic.
 */

export type LineKind = 'same' | 'added' | 'removed';

export interface DiffLine {
  kind: LineKind;
  text: string;
  /** The line number on its side: the old one for removed, the new one otherwise. */
  number: number;
}

export interface Hunk {
  lines: DiffLine[];
}

export interface FileDiff {
  name: string;
  status: 'added' | 'removed' | 'changed' | 'same';
  added: number;
  removed: number;
  /** Absent for a binary file, or one too large to compare line by line. */
  hunks?: Hunk[];
  binary: boolean;
}

/** More cells than this and the lines are not compared. */
const MOST = 4_000_000;

const CONTEXT = 3;

export function compare(before: LambdaFile[], after: LambdaFile[]): FileDiff[] {
  const old = new Map(before.map((file) => [file.name, file]));
  const now = new Map(after.map((file) => [file.name, file]));

  const names = [...new Set([...after.map((f) => f.name), ...before.map((f) => f.name)])];

  return names.map((name) => {
    const a = old.get(name);
    const b = now.get(name);

    const binary = a?.encoding === 'base64' || b?.encoding === 'base64';

    if (binary) {
      const status = !a ? 'added' : !b ? 'removed' : a.code === b.code ? 'same' : 'changed';
      return { name, status, added: 0, removed: 0, binary };
    }

    const left = a ? split(a.code) : [];
    const right = b ? split(b.code) : [];

    const lines = diffLines(left, right);

    const added = lines?.filter((l) => l.kind === 'added').length ?? right.length;
    const removed = lines?.filter((l) => l.kind === 'removed').length ?? left.length;

    const status = !a ? 'added' : !b ? 'removed' : added + removed === 0 ? 'same' : 'changed';

    return { name, status, added, removed, hunks: lines ? group(lines) : undefined, binary };
  });
}

function split(code: string): string[] {
  return code === '' ? [] : code.replace(/\r\n/g, '\n').split('\n');
}

function diffLines(a: string[], b: string[]): DiffLine[] | null {
  let start = 0;

  while (start < a.length && start < b.length && a[start] === b[start]) {
    start++;
  }

  let endA = a.length;
  let endB = b.length;

  while (endA > start && endB > start && a[endA - 1] === b[endB - 1]) {
    endA--;
    endB--;
  }

  const middleA = a.slice(start, endA);
  const middleB = b.slice(start, endB);

  if (middleA.length * middleB.length > MOST) {
    return null;
  }

  const result: DiffLine[] = [];

  for (let i = 0; i < start; i++) {
    result.push({ kind: 'same', text: a[i], number: i + 1 });
  }

  // lengths of the common subsequence of the suffixes, filled from the end
  const rows = middleA.length;
  const cols = middleB.length;
  const table = new Uint32Array((rows + 1) * (cols + 1));

  for (let i = rows - 1; i >= 0; i--) {
    for (let j = cols - 1; j >= 0; j--) {
      table[i * (cols + 1) + j] =
        middleA[i] === middleB[j]
          ? table[(i + 1) * (cols + 1) + j + 1] + 1
          : Math.max(table[(i + 1) * (cols + 1) + j], table[i * (cols + 1) + j + 1]);
    }
  }

  let i = 0;
  let j = 0;

  while (i < rows || j < cols) {
    if (i < rows && j < cols && middleA[i] === middleB[j]) {
      result.push({ kind: 'same', text: middleB[j], number: start + j + 1 });
      i++;
      j++;
    } else if (i < rows && (j === cols || table[(i + 1) * (cols + 1) + j] >= table[i * (cols + 1) + j + 1])) {
      // what went comes before what replaced it, the way a diff is read
      result.push({ kind: 'removed', text: middleA[i], number: start + i + 1 });
      i++;
    } else {
      result.push({ kind: 'added', text: middleB[j], number: start + j + 1 });
      j++;
    }
  }

  for (let k = endB; k < b.length; k++) {
    result.push({ kind: 'same', text: b[k], number: k + 1 });
  }

  return result;
}

/** The changed lines with a few either side, joined where they would overlap. */
function group(lines: DiffLine[]): Hunk[] {
  const hunks: Hunk[] = [];

  let current: DiffLine[] | null = null;
  let quiet = 0;

  for (let index = 0; index < lines.length; index++) {
    const line = lines[index];

    if (line.kind !== 'same') {
      if (!current) {
        current = lines.slice(Math.max(0, index - CONTEXT), index);
      }

      current.push(line);
      quiet = 0;
      continue;
    }

    if (current) {
      current.push(line);
      quiet++;

      if (quiet > CONTEXT * 2) {
        // more unchanged lines than two contexts' worth: close the hunk and
        // give back what belongs to the next one
        current.splice(current.length - (quiet - CONTEXT));
        hunks.push({ lines: current });
        current = null;
        quiet = 0;
      }
    }
  }

  if (current) {
    if (quiet > CONTEXT) {
      current.splice(current.length - (quiet - CONTEXT));
    }

    hunks.push({ lines: current });
  }

  return hunks;
}
