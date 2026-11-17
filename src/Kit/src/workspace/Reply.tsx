import type { ReactNode } from 'react';

/**
 * What the agent answered, with the little formatting a model writes even
 * when asked for plain words: paragraphs, a list, something in bold or in
 * backticks. Made into elements rather than markup, so nothing the model
 * wrote is ever HTML - and without the markdown renderer the editor uses,
 * which this page would otherwise download for a sentence or two.
 */
export function Reply({ text }: { text: string }) {
  const blocks = text.trim().split(/\n\s*\n/);

  return (
    <div className="space-y-3 whitespace-normal">
      {blocks.map((block, index) => {
        const lines = block.split('\n').map((line) => line.trimEnd());
        const bullets = lines.every((line) => /^\s*([-*•]|\d+[.)])\s+/.test(line));

        if (bullets) {
          const ordered = /^\s*\d/.test(lines[0]);
          const items = lines.map((line) => line.replace(/^\s*([-*•]|\d+[.)])\s+/, ''));
          const List = ordered ? 'ol' : 'ul';

          return (
            <List key={index} className={`space-y-1 pl-5 ${ordered ? 'list-decimal' : 'list-disc'}`}>
              {items.map((item, at) => (
                <li key={at}>{inline(item)}</li>
              ))}
            </List>
          );
        }

        return (
          <p key={index}>
            {lines.map((line, at) => (
              <span key={at}>
                {at > 0 && <br />}
                {inline(line.replace(/^#+\s+/, ''))}
              </span>
            ))}
          </p>
        );
      })}
    </div>
  );
}

/** Bold and code within a line; everything else as it was written. */
function inline(line: string): ReactNode[] {
  return line.split(/(\*\*[^*]+\*\*|`[^`]+`)/g).map((part, index) => {
    if (/^\*\*[^*]+\*\*$/.test(part)) {
      return (
        <strong key={index} className="font-semibold">
          {part.slice(2, -2)}
        </strong>
      );
    }

    if (/^`[^`]+`$/.test(part)) {
      return (
        <code key={index} className="rounded bg-slate-100 px-1 py-px font-mono text-[13px] dark:bg-ink-850">
          {part.slice(1, -1)}
        </code>
      );
    }

    return part;
  });
}
