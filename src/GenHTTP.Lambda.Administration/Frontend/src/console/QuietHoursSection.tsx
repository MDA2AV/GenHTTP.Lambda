import { useEffect, useState } from 'react';

import { api, refusedToken, type TypicalDay } from '@genhttp-lambda/kit/api';
import { Ago, Section } from '@genhttp-lambda/kit/ui/ui';
import type { Access } from './context';
import { Card, Fact, Facts, Reading, count, share } from './parts';
import { DayChart, counted, quietStretch, zone } from './quiet';

/**
 * When the installation is usually quiet: every request the server answers,
 * by the hour of a typical day - what work that can wait is planned by. Read
 * only: it is counted, not set.
 */
export function QuietHoursSection({ access }: { access: Access }) {
  const { token, deny, dark } = access;

  const [day, setDay] = useState<TypicalDay | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    api.admin.quietHours(token)
      .then(setDay)
      .catch((problem) => {
        if (refusedToken(problem)) {
          deny();
        } else {
          setError('The quiet hours could not be read.');
        }
      });
  }, [token, deny]);

  const total = day ? day.hours.reduce((sum, h) => sum + h, 0) : 0;

  return (
    <Section title="Quiet hours" hint="Every request the server answers, by the hour of a typical day: counted in memory, written down hourly, a week ago counting half as much as today.">
      {day === null ? (
        <Reading what="the quiet hours" failure={error} />
      ) : (
        <div className="space-y-6">
          <Card title="A typical day" hint={`Requests per hour, on your clock (${zone()}).`}>
            {counted(day) ? (
              <DayChart day={day} dark={dark} label="Requests the server answers on a typical day" />
            ) : (
              <p className="text-[13px] text-slate-500">Nothing counted yet: the first hour is written down an hour after the server starts.</p>
            )}
          </Card>

          <Card title="What it says">
            <Facts>
              <Fact label="Quietest">
                {day.known ? <>{quietStretch(day)} <span className="text-slate-500">({zone()})</span></> : 'Not known yet'}
              </Fact>
              <Fact label="Of a day's requests">{day.known ? share(day.quietShare, 1) : '-'}</Fact>
              <Fact label="On a typical day">{counted(day) ? `${count(Math.round(total))} requests` : '-'}</Fact>
              <Fact label="Counted since">{day.since ? <Ago at={day.since} /> : '-'}</Fact>
              <Fact label="Written down">{day.updated ? <Ago at={day.updated} /> : '-'}</Fact>
              <Fact label="Used by">Database optimization, under Settings</Fact>
            </Facts>

            {!day.known && (
              <p className="mt-4 text-[13px] text-slate-500">
                Known once every hour of the day was counted, over a day at the least. Until then the databases are optimized from 03:00 on the server’s clock.
              </p>
            )}
          </Card>
        </div>
      )}
    </Section>
  );
}
