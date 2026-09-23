import { useCallback, useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';

import { ApiError, api, type Example as ExampleModel } from '../api';
import { CSharp } from '../components/CSharp';
import { IconCheck, IconSpinner } from '../components/Icons';
import { useToast } from '../components/Toast';

/**
 * One example: what it answers, and what it is made of.
 *
 * The code is shown rather than edited, and that is not a disabled editor - it
 * is a lambda whose editor key nobody has. Changing it means cloning it, which
 * creates one of your own from the same code, and that is the button.
 */
export function Example() {
  const { id } = useParams<{ id: string }>();
  const [open, setOpen] = useState<string | null>(null);
  const navigate = useNavigate();
  const toast = useToast();

  const [example, setExample] = useState<ExampleModel | null>(null);
  const [missing, setMissing] = useState(false);
  const [tried, setTried] = useState<{ status: number; body: string } | null>(null);
  const [calling, setCalling] = useState(false);
  const [frames, setFrames] = useState<string[] | null>(null);
  const [connecting, setConnecting] = useState(false);

  const load = useCallback(async () => {
    if (id === undefined) {
      return;
    }

    try {
      setExample(await api.example(id));
      setMissing(false);
    } catch (error) {
      if (error instanceof ApiError && error.status === 404) {
        setMissing(true);
      } else {
        toast('The example could not be read.', 'error');
      }
    }
  }, [id, toast]);

  useEffect(() => {
    setExample(null);
    setTried(null);
    setFrames(null);
    setOpen(null);
    load();
  }, [load]);

  /*
   * Calling it from here rather than only linking it: the point of an example
   * that is already running is that you can see what it answers without
   * leaving the page that explains it.
   */
  async function call() {
    if (example === null) {
      return;
    }

    setCalling(true);

    try {
      const response = await fetch(example.tryPath, { headers: { Accept: 'application/json, text/*' } });
      const body = await response.text();

      setTried({ status: response.status, body: body.slice(0, 2000) });
    } catch {
      toast('The example could not be reached.', 'error');
    } finally {
      setCalling(false);
    }
  }

  /*
   * A websocket example cannot be shown by asking for a page - what it does
   * only happens once a socket is open - so this opens one, says hello and
   * shows what comes back, then closes it again.
   */
  function connect() {
    if (example === null) {
      return;
    }

    setConnecting(true);
    setFrames([]);

    const address = new URL(example.tryPath, window.location.href);

    address.protocol = address.protocol === 'https:' ? 'wss:' : 'ws:';

    const socket = new WebSocket(address);

    const add = (line: string) => setFrames((seen) => [...(seen ?? []), line]);

    const timer = window.setTimeout(() => socket.close(), 6000);

    socket.onopen = () => {
      add('→ connected');
      socket.send('hello from the examples page');
      add('→ sent "hello from the examples page"');
    };

    socket.onmessage = (event) => add(`← ${typeof event.data === 'string' ? event.data : '(binary frame)'}`);

    socket.onerror = () => add('× the socket reported an error');

    socket.onclose = (event) => {
      window.clearTimeout(timer);
      add(`→ closed${event.code ? ` (${event.code})` : ''}`);
      setConnecting(false);
    };
  }

  if (missing) {
    return (
      <div className="mx-auto w-full max-w-2xl px-5 py-24 text-center">
        <h1 className="text-xl font-bold tracking-tight">No such example</h1>
        <p className="mt-2 text-sm text-slate-600 dark:text-slate-400">
          It may have been renamed. <Link className="text-accent-600 hover:underline dark:text-accent-400" to="/">Start from the front page</Link>.
        </p>
      </div>
    );
  }

  if (example === null) {
    return (
      <div className="mx-auto flex max-w-3xl items-center gap-2 px-5 py-14 text-sm text-slate-500">
        <IconSpinner /> Reading the example…
      </div>
    );
  }

  return (
    <div className="mx-auto w-full max-w-3xl px-5 py-12">
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">{example.name}</h1>
          <p className="mt-2 text-[15px] leading-relaxed text-slate-600 dark:text-slate-400">
            {example.description}
          </p>
        </div>

        <button
          type="button"
          onClick={() => navigate(`/editor/create?template=${example.id}&invited=1`)}
          className="btn-primary shrink-0 px-5 py-2.5"
        >
          Clone and edit
        </button>
      </div>

      <p className="mt-3 text-sm text-slate-500">
        This one belongs to the installation, so it cannot be changed. Cloning gives you your own copy of the
        same code, at your own address, which you can do whatever you like with.
      </p>

      <section className="surface mt-8 p-4">
        <div className="flex flex-wrap items-center justify-between gap-3">
          <div>
            <h2 className="text-sm font-medium">It is running here</h2>
            {example.socket ? (
              <span className="mt-0.5 block font-mono text-sm text-slate-600 dark:text-slate-400">
                {`ws${window.location.protocol === 'https:' ? 's' : ''}://${window.location.host}${example.tryPath}`}
              </span>
            ) : (
              <a
                href={example.tryPath}
                target="_blank"
                rel="noreferrer"
                className="mt-0.5 block font-mono text-sm text-accent-600 hover:underline dark:text-accent-400"
              >
                {example.tryPath}
              </a>
            )}
          </div>

          <div className="flex items-center gap-2">
            {example.live ? (
              <span className="chip bg-emerald-500/10 text-emerald-600 dark:text-emerald-400">
                <IconCheck className="h-3.5 w-3.5" /> live
              </span>
            ) : (
              <span className="chip bg-amber-500/10 text-amber-600 dark:text-amber-400">starting…</span>
            )}

            {example.socket ? (
              <button
                type="button"
                onClick={connect}
                disabled={connecting || !example.live}
                className="btn-ghost !px-3 !py-1.5 text-sm"
              >
                {connecting ? 'Talking…' : 'Open a socket'}
              </button>
            ) : (
              <button
                type="button"
                onClick={call}
                disabled={calling || !example.live}
                className="btn-ghost !px-3 !py-1.5 text-sm"
              >
                {calling ? 'Calling…' : 'Call it'}
              </button>
            )}
          </div>
        </div>

        {frames !== null && (
          <div className="mt-3 border-t border-slate-200 pt-3 dark:border-ink-800">
            <pre className="max-h-48 overflow-auto whitespace-pre-wrap font-mono text-xs text-slate-700 dark:text-slate-300">
              {frames.join('\n') || 'opening…'}
            </pre>
          </div>
        )}

        {tried !== null && (
          <div className="mt-3 border-t border-slate-200 pt-3 dark:border-ink-800">
            <div className="text-xs text-slate-500">HTTP {tried.status}</div>
            <pre className="mt-1 max-h-48 overflow-auto whitespace-pre-wrap break-all font-mono text-xs text-slate-700 dark:text-slate-300">
              {tried.body || '(no body)'}
            </pre>
          </div>
        )}
      </section>

      <section className="surface mt-5">
        <div className="border-b border-slate-200 px-4 py-3 dark:border-ink-800">
          <h2 className="text-sm font-medium">What it is made of</h2>
          <p className="mt-0.5 text-xs text-slate-500">
            The whole of it. Whatever <span className="font-mono">lambda.cs</span> returns is what is hosted
            at the address above; the other files are compiled beside it.
          </p>
        </div>

        {example.files.length > 1 && (
          <div className="flex flex-wrap gap-1 border-b border-slate-200 bg-slate-50 px-2 py-1.5 dark:border-ink-800 dark:bg-ink-950">
            {example.files.map((file) => {
              const showing = (open ?? example.files[0].name) === file.name;

              return (
                <button
                  key={file.name}
                  type="button"
                  onClick={() => setOpen(file.name)}
                  className={`px-3 py-1 font-mono text-xs ${
                    showing
                      ? 'bg-white text-slate-900 dark:bg-ink-900 dark:text-slate-100'
                      : 'text-slate-500 hover:bg-white/70 dark:hover:bg-ink-900/70'
                  }`}
                >
                  {file.name}
                </button>
              );
            })}
          </div>
        )}

        <pre className="overflow-x-auto px-4 py-3 font-mono text-[13px] leading-relaxed">
          <CSharp code={(example.files.find((f) => f.name === (open ?? example.files[0]?.name)) ?? example.files[0])?.code ?? ''} />
        </pre>
      </section>
    </div>
  );
}
