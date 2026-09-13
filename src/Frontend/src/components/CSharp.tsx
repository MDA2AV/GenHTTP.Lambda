/**
 * Colours a fixed C# snippet without loading an editor to do it.
 *
 * The real editor is Monaco, three megabytes of it, and the landing page is
 * the one screen where that cost is paid before anyone has asked for anything.
 * A snippet that never changes needs a tokeniser, not a language service - and
 * the colours are the ones the editor uses, so the example looks like what you
 * get when you click through.
 */
const KEYWORDS = new Set([
  'var', 'new', 'return', 'record', 'class', 'struct', 'public', 'private', 'internal', 'static',
  'readonly', 'const', 'void', 'int', 'string', 'bool', 'double', 'long', 'byte', 'async', 'await',
  'if', 'else', 'foreach', 'for', 'while', 'in', 'using', 'namespace', 'true', 'false', 'null', 'this',
]);

// one pass, alternation ordered so a keyword inside a string is never seen
const TOKENS = /(\/\/[^\n]*)|("(?:[^"\\]|\\.)*")|(\b\d+(?:\.\d+)?\b)|([A-Za-z_][A-Za-z0-9_]*)/g;

const CLASS: Record<string, string> = {
  comment: 'text-grey-500 italic',
  string: 'text-[#188038] dark:text-[#81c995]',
  number: 'text-[#e37400] dark:text-[#fdd663]',
  keyword: 'text-[#1a73e8] dark:text-[#8ab4f8]',
  type: 'text-[#129eaf] dark:text-[#78d9ec]',
};

export function CSharp({ code }: { code: string }) {
  const parts: React.ReactNode[] = [];

  let last = 0;
  let key = 0;

  for (const match of code.matchAll(TOKENS)) {
    const [text, comment, str, num, word] = match;
    const at = match.index;

    if (at > last) {
      parts.push(code.slice(last, at));
    }

    let kind: string | null = null;

    if (comment) {
      kind = 'comment';
    } else if (str) {
      kind = 'string';
    } else if (num) {
      kind = 'number';
    } else if (word) {
      // a keyword, or a type if it is capitalised and not reached through a
      // dot - which keeps method names plain, the way the editor shows them.
      // Convention rather than grammar, and close enough for one example.
      const member = code[at - 1] === '.';

      kind = KEYWORDS.has(word) ? 'keyword' : !member && /^[A-Z]/.test(word) ? 'type' : null;
    }

    parts.push(kind === null ? text : <span key={key++} className={CLASS[kind]}>{text}</span>);

    last = at + text.length;
  }

  parts.push(code.slice(last));

  return <>{parts}</>;
}
