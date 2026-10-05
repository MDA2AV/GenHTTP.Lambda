import { useEffect, useState } from 'react';
import { useSearchParams } from 'react-router-dom';

import { ApiError, api, type VersionContent } from '../api';
import { IconChevronDown, IconLayers } from '../components/Icons';
import { useEditorT } from '../i18n';
import type { Control } from './context';
import { GroupList, Tree, Viewer, sizeOf, type Selection } from './FileBrowser';
import { bytes, servesAssets } from './format';
import { Exposure } from './SummaryTab';
import { Section, pill } from './ui';
import { isAsset, isBuild, isCode, isContext } from './written';

/**
 * The files of one version - the program - and which of them anybody on the
 * internet can reach.
 *
 * Code and assets belong to a version, so the version is this section's view,
 * and nothing else is here: what the lambda keeps while it runs belongs to
 * the lambda rather than to any version, and has a section of its own.
 *
 * What is written about the version - its documentation and its tests - is
 * shown too, as the third thing it holds, never compiled and never served:
 * this is where all of a version's files are, and its own sections are where
 * it is read. So is what it is built from, its build folder, as the fourth
 * - where a version keeps one, as its section is there only then.
 */
export function FilesTab({ control }: { control: Control }) {
  const t = useEditorT();
  const said = t.files;
  const [params, setParams] = useSearchParams();

  const { lambda, versions, summary } = control;

  const wanted = Number(params.get('version')) || lambda.activeVersion || lambda.latestVersion || versions[0]?.version;

  const [content, setContent] = useState<VersionContent | null>(null);
  const [failure, setFailure] = useState<string | null>(null);
  const [selected, setSelected] = useState<Selection | null>(null);

  useEffect(() => {
    if (wanted == null) {
      return;
    }

    let alive = true;

    setContent(null);

    api
      .version(control.privateKey, wanted)
      .then((found) => {
        if (!alive) {
          return;
        }

        setContent(found);

        // the snippet is where reading starts, unless something else is open
        setSelected((was) =>
          was && found.files.some((f) => f.name === was.path)
            ? was
            : { group: 'code', path: found.files[0]?.name ?? 'lambda.cs' });
      })
      .catch((error) => alive && setFailure(error instanceof ApiError ? error.message : said.readFailed));

    return () => {
      alive = false;
    };
  }, [control.privateKey, wanted, said]);

  const files = content?.files ?? [];
  const code = files.filter((f) => isCode(f.name));
  const assets = files.filter((f) => isAsset(f.name));
  const context = files.filter((f) => isContext(f.name));
  const build = files.filter((f) => isBuild(f.name));

  const source = code.map((f) => f.code).join('\n');
  const limits = summary?.limits;

  const version = versions.find((v) => v.version === wanted);

  return (
    <Section
      title={t.frame.sections.files}
      hint={said.hint((text) => <b>{text}</b>)}
      actions={
        wanted != null && (
          <button type="button" onClick={() => control.edit(wanted)} className="btn-ghost !px-3 !py-1.5 text-[13px]">
            {said.edit}
          </button>
        )
      }
      pills={
        versions.length > 0 && (
          <label className={`${pill(true)} relative cursor-pointer pr-7`}>
            <span className="sr-only">{said.version}</span>
            <span>{said.shown(wanted, wanted === lambda.activeVersion, wanted === lambda.latestVersion)}</span>
            <IconChevronDown className="pointer-events-none absolute right-2.5 h-3.5 w-3.5" />
            <select
              value={wanted}
              onChange={(event) => setParams({ version: event.target.value }, { replace: true })}
              className="absolute inset-0 cursor-pointer opacity-0"
            >
              {versions.map((v) => (
                <option key={v.version} value={v.version}>
                  {v.version}
                  {v.version === lambda.activeVersion ? said.optionOnline : ''}
                  {v.change ? ` - ${v.change.slice(0, 60)}` : ''}
                </option>
              ))}
            </select>
          </label>
        )
      }
    >
      {wanted == null ? (
        <p className="text-sm text-slate-500">{said.noVersion}</p>
      ) : (
        <>
          {version?.change && <p className="-mt-1 mb-3 text-[13px] text-slate-500">{version.change}</p>}

          <p className="mb-4 flex items-start gap-2 text-[13px] text-slate-600 dark:text-slate-400">
            <IconLayers className="mt-0.5 h-3.5 w-3.5 shrink-0 text-slate-400" />
            <span>
              {said.scope(wanted, (text) => (
                <button type="button" onClick={() => control.openData()} className="text-accent-500 hover:underline">
                  {text}
                </button>
              ))}
            </span>
          </p>

          {failure && <p className="mb-4 text-sm text-red-500">{failure}</p>}

          <div className="grid gap-5 lg:grid-cols-[17rem,1fr]">
            <nav aria-label={said.label} className="space-y-5 lg:max-h-[40rem] lg:overflow-y-auto">
              <GroupList
                title={said.code}
                exposure={<Exposure open={false} why={said.codeWhy} />}
                usage={limits && said.codeUsage(
                  said.count(code.length),
                  code.reduce((total, f) => total + f.code.length, 0).toLocaleString(),
                  limits.codeCharacters.toLocaleString(),
                )}
              >
                <Tree
                  entries={code.map((f) => ({ path: f.name, size: sizeOf(f) }))}
                  selected={selected?.group === 'code' ? selected.path : null}
                  onSelect={(path) => setSelected({ group: 'code', path })}
                  empty={said.noCode}
                />
              </GroupList>

              <GroupList
                title={said.assets}
                exposure={<Exposure open={servesAssets(source)} why={servesAssets(source) ? said.assetsPublic : said.assetsPrivate} />}
                usage={limits && said.usage(
                  said.count(assets.length),
                  bytes(assets.reduce((total, f) => total + sizeOf(f), 0)),
                  bytes(limits.assetBytes),
                )}
              >
                <Tree
                  entries={assets.map((f) => ({ path: f.name, size: sizeOf(f) }))}
                  selected={selected?.group === 'assets' ? selected.path : null}
                  onSelect={(path) => setSelected({ group: 'assets', path })}
                  empty={said.noAssets}
                />
              </GroupList>

              <GroupList
                title={said.context}
                exposure={<Exposure open={false} why={said.contextWhy} />}
                usage={said.contextUsage(said.count(context.length), bytes(context.reduce((total, f) => total + sizeOf(f), 0)))}
              >
                <Tree
                  entries={context.map((f) => ({ path: f.name, size: sizeOf(f) }))}
                  selected={selected?.group === 'context' ? selected.path : null}
                  onSelect={(path) => setSelected({ group: 'context', path })}
                  empty={said.noContext}
                />
              </GroupList>

              {build.length > 0 && (
                <GroupList
                  title={said.build}
                  exposure={<Exposure open={false} why={said.buildWhy} />}
                  usage={said.contextUsage(said.count(build.length), bytes(build.reduce((total, f) => total + sizeOf(f), 0)))}
                  action={
                    <button type="button" onClick={() => control.openBuild(wanted)} className="text-[12px] text-accent-500 hover:underline">
                      {t.frame.sections.build}
                    </button>
                  }
                >
                  <Tree
                    entries={build.map((f) => ({ path: f.name, size: sizeOf(f) }))}
                    selected={selected?.group === 'build' ? selected.path : null}
                    onSelect={(path) => setSelected({ group: 'build', path })}
                    empty={said.count(0)}
                  />
                </GroupList>
              )}
            </nav>

            <Viewer control={control} selection={selected} files={files} listing={null} />
          </div>
        </>
      )}
    </Section>
  );
}
