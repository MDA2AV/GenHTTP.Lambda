import { absoluteAddress } from '../address';
import { isDemo, type Feature, type Lambda } from '../api';
import { CloneAddress, Command, Popover } from '../components/Clone';
import { IconBranch, IconLock } from '../components/Icons';
import { useEditorT } from '../i18n';

/**
 * Where a lambda is cloned with git, the way a repository page offers it: a
 * button that opens the address, the commands to start with, and what
 * pushing does.
 *
 * For somebody who works with code, so the full view has it - on the
 * overview, which is the lambda's front page, and beside its code. In a draft
 * it says which branch the draft is.
 *
 * The address holds the editor key, as the editor's own does: the platform
 * has no accounts, and the key is what lets a push in. So it says so.
 */
export function CloneMenu({ lambda, feature }: { lambda: Lambda; feature?: Feature | null }) {
  const said = useEditorT().clone;

  const url = absoluteAddress(lambda.gitPath);
  const demo = isDemo(lambda.tier);

  const words = { copy: said.copy, copied: said.copied };

  const commands = feature
    ? `git clone ${url}\ncd ${lambda.publicKey}\ngit switch ${feature.branch}`
    : `git clone ${url}\ncd ${lambda.publicKey}\ndotnet run`;

  const code = 'rounded-sm bg-slate-100 px-1 py-px font-mono text-[12px] text-ink-800 dark:bg-ink-850 dark:text-slate-200';

  return (
    <Popover
      label={<><IconBranch className="h-3.5 w-3.5" />{said.button}</>}
      title={said.title}
      button="btn-ghost !px-3 !py-1.5 text-[13px]"
      width={416}
    >
      <p className="flex items-center gap-2 text-sm font-semibold">
        <IconBranch className="h-4 w-4 text-slate-400" />
        {said.title}
      </p>
      <p className="mt-1.5 text-[13px] leading-relaxed text-slate-600 dark:text-slate-400">{said.intro}</p>

      <div className="mt-3"><CloneAddress url={url} words={words} /></div>

      {!demo && (
        <p className="mt-2 flex gap-1.5 text-[12px] leading-relaxed text-amber-700 dark:text-amber-400">
          <IconLock className="mt-0.5 h-3.5 w-3.5 shrink-0" />
          {said.keyWarning}
        </p>
      )}

      <Command text={commands} words={words} />

      {feature && (
        <p className="mt-2 text-[13px] text-slate-600 dark:text-slate-400">{said.draft(<code className={code}>{feature.branch}</code>)}</p>
      )}

      <div className="mt-4 border-t border-slate-200 pt-3 dark:border-ink-800">
        {demo ? (
          <p className="text-[13px] leading-relaxed text-slate-600 dark:text-slate-400">{said.readOnly}</p>
        ) : (
          <>
            <p className="text-[11px] font-medium uppercase tracking-wide text-slate-400">{said.pushing}</p>
            <ul className="mt-2 space-y-1.5 text-[13px] leading-relaxed text-slate-600 dark:text-slate-400">
              <li>{said.toMain(<code className={code}>git push -o deploy</code>)}</li>
              <li>{said.toBranch}</li>
              <li>{said.agents(<code className={code}>AGENTS.md</code>)}</li>
            </ul>
          </>
        )}
      </div>
    </Popover>
  );
}
