import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';

import { api, refusedToken, type AdminLambdaStorage, type AdminStorage } from '@genhttp-lambda/kit/api';
import { IconSpinner } from '@genhttp-lambda/kit/components/Icons';
import { useToast } from '@genhttp-lambda/kit/components/Toast';
import { Empty, Section, TierBadge } from '@genhttp-lambda/kit/ui/ui';
import { BLUE, Fill, GREY, Ranked, Segments } from './charts';
import { diskState, lambdaPage, type Access } from './context';
import { Card, Notice, Reading, Tile, Tiles, actionButton, bytes, plural, share, since } from './parts';

/** What each part of the data directory is, as the operator reads it. */
const PARTS: Record<string, { label: string; note: string }> = {
  versions: { label: 'Versions', note: 'The code and resources of every version kept' },
  drafts: { label: 'Drafts', note: 'Features being worked on, each with a copy of the data' },
  databases: { label: 'Databases', note: 'The records of the lambdas' },
  workspaces: { label: 'Workspaces', note: 'The files the lambdas saved' },
  git: { label: 'Git histories', note: 'The commits of the lambdas that were cloned' },
  sources: { label: 'Published sources', note: 'Versions packed for /source - made again when missing' },
  compiled: { label: 'Compiled lambdas', note: 'What they were compiled to - made again when missing' },
  resources: { label: 'Unpacked resources', note: 'What they serve, unpacked - made again when missing' },
  platform: { label: 'The platform’s database', note: 'The lambdas, their versions and settings, the secrets' },
  analytics: { label: 'Analytics', note: 'What was counted of the visits, for a year' },
  locations: { label: 'Address locations', note: 'Where an address is, for the log and the analytics - downloaded again when missing' },
  crawlers: { label: 'Crawler lists', note: 'What the analytics tell bots by - downloaded again when missing' },
  other: { label: 'Other', note: 'The secrets key, how the last run ended, whatever else is there' },
};

/**
 * The room the installation has, and what takes it: the volume - read as the
 * page is opened, since that is what runs out - and what the data directory
 * holds, by part and by lambda, as measured within the hour.
 */
export function StorageSection({ access }: { access: Access }) {
  const { token, deny, dark } = access;

  const toast = useToast();

  const [storage, setStorage] = useState<AdminStorage | null>(null);
  const [failure, setFailure] = useState<string | null>(null);
  const [measuring, setMeasuring] = useState(false);

  async function measure() {
    setMeasuring(true);

    try {
      setStorage(await api.admin.measureStorage(token));
      toast('Measured again.', 'success');
    } catch (error) {
      if (refusedToken(error)) {
        deny();
      } else {
        toast('The storage could not be measured.', 'error');
      }
    } finally {
      setMeasuring(false);
    }
  }

  useEffect(() => {
    api.admin.storage(token)
      .then(setStorage)
      .catch((error) => (refusedToken(error) ? deny() : setFailure('The storage could not be read.')));
  }, [token, deny]);

  if (storage === null) {
    return (
      <Section title="Storage">
        <Reading what="the storage" failure={failure} />
      </Section>
    );
  }

  const { disk } = storage;
  const ours = storage.parts.reduce((sum, p) => sum + p.bytes, 0);
  const files = storage.parts.reduce((sum, p) => sum + p.files, 0);
  const state = diskState(disk);

  return (
    <Section
      title="Storage"
      hint={
        <>
          Everything the platform keeps is in its data directory, <span className="font-mono">{storage.directory}</span>. The free space is read as
          this page opens; what takes the room is measured every hour, by walking over every file.
        </>
      }
      actions={
        <button type="button" onClick={measure} disabled={measuring} className={`btn-ghost ${actionButton}`}
                title="Walks over every file now, rather than at the next hour">
          {measuring && <IconSpinner />}
          Measure again
        </button>
      }
    >
      {state && disk && (
        <div className="mb-6">
          <Notice tone={state === 'bad' ? 'problem' : 'wanted'} title={state === 'bad' ? 'The volume has almost run out of room' : 'The volume is running out of room'}>
            {bytes(disk.free)} are left. Lambdas that cannot write their data fail, and so does saving a version. Removing old drafts and
            lambdas nobody uses frees the most; the compiled lambdas, unpacked resources and published sources are made again when they are missing.
          </Notice>
        </div>
      )}

      <Tiles columns={3}>
        <Tile label="Free" value={disk ? bytes(disk.free) : 'unknown'} tone={state === 'bad' ? 'bad' : state === 'warn' ? 'warn' : undefined}
              detail={disk ? `${share(disk.free, disk.size)} of the volume` : 'the system does not say'}>
          {disk && <Fill used={disk.size - disk.free} of={disk.size} label="Used of the volume" />}
        </Tile>
        <Tile label="Taken by this installation" value={bytes(ours)} detail={`${plural(files, 'file')}, ${plural(storage.lambdas, 'lambda')}`} />
        <Tile label="The volume" value={disk ? bytes(disk.size) : 'unknown'}
              detail={storage.measured ? `measured ${since(storage.measured)} ago` : 'not measured yet'} />
      </Tiles>

      {disk && (
        <div className="mt-6">
          <Card title="The volume" hint="What this installation takes of it, what everything else on it takes, and what is free.">
            <Segments
              label="The volume"
              dark={dark}
              unit={bytes}
              parts={[
                { label: 'This installation', value: ours, color: BLUE },
                { label: 'Everything else on it', value: Math.max(0, disk.size - disk.free - ours), color: GREY },
                { label: 'Free', value: disk.free, color: ['#d2e3fc', '#394457'] },
              ]}
            />
          </Card>
        </div>
      )}

      <div className="mt-6">
        <Card title="What the data directory holds" hint="By part, the largest first.">
          <Ranked
            limit={Object.keys(PARTS).length}
            empty="Nothing was measured yet."
            rows={storage.parts.map((part) => ({
              key: part.kind,
              label: (
                <>
                  {PARTS[part.kind]?.label ?? part.kind}
                  <span className="ml-2 text-xs text-slate-500">{plural(part.files, 'file')}</span>
                </>
              ),
              value: part.bytes,
              said: bytes(part.bytes),
              aside: share(part.bytes, ours),
            }))}
          />
          <details className="mt-4 border-t border-grey-300 pt-3 text-xs text-slate-500 dark:border-ink-800">
            <summary className="cursor-pointer text-accent-600 dark:text-accent-400">What each part is</summary>
            <dl className="mt-2 space-y-1">
              {storage.parts.filter((p) => PARTS[p.kind]).map((part) => (
                <div key={part.kind} className="flex gap-2">
                  <dt className="w-40 shrink-0 text-ink-900 dark:text-slate-300">{PARTS[part.kind].label}</dt>
                  <dd>{PARTS[part.kind].note}</dd>
                </div>
              ))}
            </dl>
          </details>
        </Card>
      </div>

      <div className="mt-6">
        <Card title="The largest lambdas" hint="What each takes: its data against what its tier allows it, its versions, its drafts and what is kept for it." flush>
          <LambdaTable lambdas={storage.largest} />
        </Card>
      </div>

      <div className="mt-6">
        <Card
          title="Beyond their allowance"
          hint="Lambdas whose data is more than their tier allows - after a move to a tier with less room, or a lower limit. It is kept and read as before; nothing more is written until it is within the allowance."
          flush
        >
          {storage.over.length === 0 ? <Empty>Every lambda is within its allowance.</Empty> : <LambdaTable lambdas={storage.over} />}
        </Card>
      </div>
    </Section>
  );
}

function LambdaTable({ lambdas }: { lambdas: AdminLambdaStorage[] }) {
  if (lambdas.length === 0) {
    return <Empty>No lambda was measured yet.</Empty>;
  }

  return (
    <div className="overflow-x-auto">
      <table className="w-full whitespace-nowrap text-left text-[13px]">
        <thead className="text-xs text-slate-500">
          <tr className="border-b border-grey-300 dark:border-ink-800">
            <th className="px-4 py-2 font-medium">Lambda</th>
            <th className="px-3 py-2 font-medium">Tier</th>
            <th className="w-48 px-3 py-2 font-medium">Data, of its allowance</th>
            <th className="px-3 py-2 text-right font-medium">Versions</th>
            <th className="px-3 py-2 text-right font-medium">Drafts</th>
            <th className="px-3 py-2 text-right font-medium" title="Its git history, its packed source, what it was compiled and unpacked to">Kept for it</th>
            <th className="px-4 py-2 text-right font-medium">All of it</th>
          </tr>
        </thead>
        <tbody>
          {lambdas.map((lambda) => (
            <tr key={lambda.publicKey} className="border-b border-grey-300 last:border-0 dark:border-ink-800">
              <td className="px-4 py-2">
                <Link to={lambdaPage(lambda.publicKey)} className="font-mono text-accent-600 hover:underline dark:text-accent-400">{lambda.publicKey}</Link>
              </td>
              <td className="px-3 py-2"><TierBadge tier={lambda.tier} /></td>
              <td className="px-3 py-2">
                <div className="flex items-center justify-between gap-2 tabular-nums">
                  <span className={lambda.dataBytes > lambda.allowedBytes ? 'text-red-600 dark:text-red-400' : ''}>{bytes(lambda.dataBytes)}</span>
                  <span className="text-slate-500">{bytes(lambda.allowedBytes)}</span>
                </div>
                <div className="mt-1"><Fill used={lambda.dataBytes} of={lambda.allowedBytes} warn={0.7} label={`Data of ${lambda.publicKey}`} /></div>
              </td>
              <td className="px-3 py-2 text-right tabular-nums text-slate-500">{bytes(lambda.versionBytes)}</td>
              <td className="px-3 py-2 text-right tabular-nums text-slate-500">{bytes(lambda.draftBytes)}</td>
              <td className="px-3 py-2 text-right tabular-nums text-slate-500">{bytes(lambda.otherBytes)}</td>
              <td className="px-4 py-2 text-right font-medium tabular-nums">{bytes(lambda.totalBytes)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
