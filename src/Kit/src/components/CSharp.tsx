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

// Visual Studio's defaults, so the example looks like the editor it opens
const CLASS: Record<string, string> = {
  comment: 'italic text-[#008000] dark:text-[#57A64A]',
  string: 'text-[#A31515] dark:text-[#D69D85]',
  number: 'text-[#098658] dark:text-[#B5CEA8]',
  keyword: 'text-[#0000FF] dark:text-[#569CD6]',
  type: 'text-[#2B91AF] dark:text-[#4EC9B0]',
  method: 'text-[#74531F] dark:text-[#DCDCAA]',
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
      // a keyword; a method if a bracket follows; otherwise a type if it is
      // capitalised and not reached through a dot. Convention rather than
      // grammar, and close enough for one example that never changes.
      const member = code[at - 1] === '.';
      const called = /^\s*\(/.test(code.slice(at + text.length));

      kind = KEYWORDS.has(word)
        ? 'keyword'
        : called
          ? 'method'
          : !member && /^[A-Z]/.test(word)
            ? 'type'
            : null;
    }

    parts.push(kind === null ? text : <span key={key++} className={CLASS[kind]}>{text}</span>);

    last = at + text.length;
  }

  parts.push(code.slice(last));

  return <>{parts}</>;
}
