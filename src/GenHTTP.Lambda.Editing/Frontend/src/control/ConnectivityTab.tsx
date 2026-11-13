import { useEffect, useState, type ReactNode } from 'react';

import { ApiError, api, type Connectivity } from '@genhttp-lambda/kit/api';
import { Dialog } from '@genhttp-lambda/kit/components/Dialog';
import { IconAlert, IconCheck, IconClose, IconGlobe, IconSpinner } from '@genhttp-lambda/kit/components/Icons';
import { useToast } from '@genhttp-lambda/kit/components/Toast';
import { Ago, Section } from '@genhttp-lambda/kit/ui/ui';
import { useEditorT } from '../i18n';
import type { Control } from './context';

/**
 * Whether the code may connect to other servers, for whoever reads the code:
 * where it stands, whether the code connects at all, what counts - and
 * asking the operator of the site, saying what for.
 *
 * The decision is somebody else's, so this page is mostly about saying it
 * well: the owner names the servers and why, and reads the operator's answer
 * here. Their agent asks the same way with request_connectivity, and that request
 * is the one shown here - the owner can change what it says, or take it
 * back.
 */
export function ConnectivityTab({ control }: { control: Control }) {
  const t = useEditorT();
  const said = t.connectivity;
  const title = t.frame.sections.connectivity;

  const toast = useToast();

  const { lambda, summary, privateKey } = control;
  const connectivity = lambda.connectivity;

  const [purpose, setPurpose] = useState(connectivity.purpose ?? '');
  const [editing, setEditing] = useState(false);
  const [busy, setBusy] = useState(false);
  const [givingUp, setGivingUp] = useState(false);

  // what an agent asked for meanwhile is what the form starts from
  useEffect(() => {
    if (!editing) {
      setPurpose(connectivity.status === 'Requested' ? connectivity.purpose ?? '' : '');
    }
  }, [connectivity.status, connectivity.purpose, editing]);

  const connects = summary?.connects ?? false;

  async function act(action: () => Promise<Connectivity>, done: string) {
    setBusy(true);

    try {
      await action();
      await control.refresh();
      setEditing(false);
      toast(done, 'success');
    } catch (error) {
      toast(error instanceof ApiError ? error.message : said.failed, 'error');
    } finally {
      setBusy(false);
    }
  }

  const ask = (event: React.FormEvent) => {
    event.preventDefault();

    act(() => api.requestConnectivity(privateKey, purpose.trim()), said.asked);
  };

  const state = connectivity.included
    ? { tone: 'good', icon: <IconCheck className="h-4 w-4" />, heading: said.included, text: said.includedText }
    : connectivity.status === 'Allowed'
      ? { tone: 'good', icon: <IconCheck className="h-4 w-4" />, heading: said.allowed, text: said.allowedText(<Ago at={connectivity.changed!} />) }
      : connectivity.status === 'Requested'
        ? { tone: 'wait', icon: <IconSpinner className="h-4 w-4 motion-reduce:animate-none" />, heading: said.requested, text: said.requestedText(<Ago at={connectivity.changed!} />) }
        : connectivity.status === 'Declined'
          ? { tone: 'bad', icon: <IconClose className="h-4 w-4" />, heading: said.declined, text: said.declinedText(<Ago at={connectivity.changed!} />) }
          : { tone: 'none', icon: <IconGlobe className="h-4 w-4" />, heading: said.none, text: said.noneText };

  const tones: Record<string, string> = {
    good: 'border-emerald-500/30 bg-emerald-500/5',
    wait: 'border-amber-500/40 bg-amber-500/5',
    bad: 'border-red-500/30 bg-red-500/5',
    none: '',
  };

  const marks: Record<string, string> = {
    good: 'bg-emerald-500/15 text-emerald-600 dark:text-emerald-400',
    wait: 'bg-amber-500/15 text-amber-600 dark:text-amber-400',
    bad: 'bg-red-500/10 text-red-600 dark:text-red-400',
    none: 'bg-slate-100 text-slate-500 dark:bg-ink-850',
  };

  // the form is there to ask, to ask again, and to change what a request says
  const form = !connectivity.allowed && (connectivity.status !== 'Requested' || editing);

  return (
    <Section title={title} hint={said.hint}>
      <div className="grid gap-8 lg:grid-cols-3">
        <div className="space-y-5 lg:col-span-2">
          {connects && !connectivity.allowed && (
            <p className="flex items-start gap-2 border-l-2 border-amber-500 pl-3 text-[13px] text-amber-700 dark:text-amber-400">
              <IconAlert className="mt-0.5 h-4 w-4 shrink-0" />
              {connectivity.status === 'Requested' ? said.connectsWaiting : said.connects}
            </p>
          )}

          <section className={`surface p-5 ${tones[state.tone]}`}>
            <div className="flex items-start gap-3">
              <span className={`flex h-8 w-8 shrink-0 items-center justify-center rounded-full ${marks[state.tone]}`}>{state.icon}</span>
              <div className="min-w-0 flex-1">
                <h2 className="text-[15px] font-semibold">{state.heading}</h2>
                <p className="mt-1 text-sm text-slate-600 dark:text-slate-400">{state.text}</p>
              </div>
            </div>

            {(connectivity.purpose || connectivity.note) && !form && (
              <dl className="mt-4 grid gap-4 pl-11 text-[13px] sm:grid-cols-2">
                {connectivity.purpose && <Said label={said.purposeSaid}>{connectivity.purpose}</Said>}
                {connectivity.note && <Said label={said.operatorSaid}>{connectivity.note}</Said>}
              </dl>
            )}

            {connectivity.status === 'Declined' && connectivity.note && form && (
              <dl className="mt-4 pl-11 text-[13px]">
                <Said label={said.operatorSaid}>{connectivity.note}</Said>
              </dl>
            )}

            {connectivity.status === 'Requested' && !editing && (
              <div className="mt-4 flex flex-wrap gap-2 pl-11">
                <button type="button" onClick={() => setEditing(true)} disabled={busy} className="btn-ghost !px-3 !py-1.5 text-[13px]">
                  {said.edit}
                </button>
                <button type="button" onClick={() => act(() => api.withdrawConnectivity(privateKey), said.withdrawn)} disabled={busy}
                        className="btn-ghost !px-3 !py-1.5 text-[13px]">
                  {busy && <IconSpinner />}
                  {said.withdraw}
                </button>
              </div>
            )}

            {connectivity.status === 'Allowed' && !connectivity.included && (
              <div className="mt-4 pl-11">
                <button type="button" onClick={() => setGivingUp(true)} disabled={busy} className="btn-ghost !px-3 !py-1.5 text-[13px]">
                  {said.giveUp}
                </button>
              </div>
            )}
          </section>

          {form && (
            <form onSubmit={ask} className="surface p-5">
              <h2 className="text-sm font-medium">
                {editing ? said.editTitle : connectivity.status === 'Declined' ? said.askAgainTitle : said.askTitle}
              </h2>
              <label htmlFor="connectivity-purpose" className="mt-3 block text-[13px] font-medium">{said.purpose}</label>
              <p className="mt-0.5 text-[13px] text-slate-500">{said.purposeHint}</p>
              <textarea
                id="connectivity-purpose"
                value={purpose}
                onChange={(event) => setPurpose(event.target.value)}
                maxLength={1000}
                rows={3}
                placeholder={said.purposeExample}
                className="field mt-2 w-full text-sm"
              />
              <div className="mt-3 flex flex-wrap gap-2">
                <button type="submit" disabled={busy || purpose.trim() === ''} className="btn-primary !px-4 !py-1.5 text-[13px]">
                  {busy && <IconSpinner />}
                  {editing ? said.save : said.send}
                </button>
                {editing && (
                  <button type="button" onClick={() => setEditing(false)} disabled={busy} className="btn-ghost !px-3 !py-1.5 text-[13px]">
                    {said.cancel}
                  </button>
                )}
              </div>
            </form>
          )}
        </div>

        <aside className="text-[13px]">
          <h2 className="text-sm font-medium">{said.coversTitle}</h2>
          <ul className="mt-2 space-y-1.5 text-slate-600 dark:text-slate-400">
            {said.covers.map((item) => (
              <li key={item} className="flex items-start gap-2">
                <IconGlobe className="mt-0.5 h-3.5 w-3.5 shrink-0 text-slate-400" />
                {item}
              </li>
            ))}
          </ul>
          <p className="mt-4 leading-relaxed text-slate-500">{said.notCovered}</p>
        </aside>
      </div>

      <Dialog
        title={said.giveUpTitle}
        open={givingUp}
        onClose={() => setGivingUp(false)}
        footer={
          <>
            <button type="button" onClick={() => setGivingUp(false)} className="btn-ghost">{said.keep}</button>
            <button
              type="button"
              onClick={() => {
                setGivingUp(false);
                act(() => api.withdrawConnectivity(privateKey), said.givenUp);
              }}
              className="btn-danger"
            >
              {said.giveUp}
            </button>
          </>
        }
      >
        <p className="text-sm text-slate-600 dark:text-slate-400">{said.giveUpText}</p>
      </Dialog>
    </Section>
  );
}

/**
 * Asking for the app to use other services, in the words of the simple view:
 * a dialog with what it is for, which the overview's tile and the dialog of a
 * change that did not go online open alike.
 */
export function AskForServices({ control, open, onClose }: { control: Control; open: boolean; onClose: () => void }) {
  const said = useEditorT().simple;

  const toast = useToast();

  const [purpose, setPurpose] = useState('');
  const [busy, setBusy] = useState(false);

  useEffect(() => {
    if (open) {
      setPurpose(control.lambda.connectivity.status === 'Requested' ? control.lambda.connectivity.purpose ?? '' : '');
    }
  }, [open, control.lambda.connectivity]);

  async function send(event: React.FormEvent) {
    event.preventDefault();

    setBusy(true);

    try {
      await api.requestConnectivity(control.privateKey, purpose.trim());
      await control.refresh();
      onClose();
      toast(said.servicesAsked, 'success');
    } catch (error) {
      toast(error instanceof ApiError ? error.message : said.servicesFailed, 'error');
    } finally {
      setBusy(false);
    }
  }

  return (
    <Dialog
      title={said.servicesDialog}
      open={open}
      onClose={onClose}
      footer={
        <>
          <button type="button" onClick={onClose} className="btn-ghost">{said.servicesCancel}</button>
          <button type="submit" form="ask-services" disabled={busy || purpose.trim() === ''} className="btn-primary">
            {busy && <IconSpinner />}
            {said.servicesSend}
          </button>
        </>
      }
    >
      <form id="ask-services" onSubmit={send}>
        <label htmlFor="services-purpose" className="block text-sm font-medium">{said.servicesPurpose}</label>
        <p className="mt-0.5 text-[13px] text-slate-500">{said.servicesPurposeHint}</p>
        <textarea
          id="services-purpose"
          value={purpose}
          onChange={(event) => setPurpose(event.target.value)}
          maxLength={1000}
          rows={3}
          placeholder={said.servicesExample}
          className="field mt-2 w-full text-sm"
        />
      </form>
    </Dialog>
  );
}

function Said({ label, children }: { label: string; children: ReactNode }) {
  return (
    <div className="min-w-0">
      <dt className="text-xs text-slate-500">{label}</dt>
      <dd className="mt-1 whitespace-pre-line break-words border-l-2 border-slate-200 pl-3 text-slate-700 dark:border-ink-700 dark:text-slate-300">
        {children}
      </dd>
    </div>
  );
}
