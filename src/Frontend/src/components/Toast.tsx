import { createContext, useCallback, useContext, useMemo, useState } from 'react';

import { IconAlert, IconCheck } from './Icons';

type Tone = 'success' | 'error';

interface Message {
  id: number;
  tone: Tone;
  text: string;
}

const ToastContext = createContext<(text: string, tone?: Tone) => void>(() => {});

export const useToast = () => useContext(ToastContext);

export function ToastHost({ children }: { children: React.ReactNode }) {
  const [messages, setMessages] = useState<Message[]>([]);

  const show = useCallback((text: string, tone: Tone = 'success') => {
    const id = Date.now() + Math.random();

    setMessages((current) => [...current, { id, tone, text }]);

    window.setTimeout(() => setMessages((current) => current.filter((m) => m.id !== id)), 4500);
  }, []);

  const value = useMemo(() => show, [show]);

  return (
    <ToastContext.Provider value={value}>
      {children}
      <div className="pointer-events-none fixed bottom-4 right-4 z-50 flex w-[min(24rem,calc(100vw-2rem))] flex-col gap-2">
        {messages.map((message) => (
          <div
            key={message.id}
            role="status"
            className={`surface pointer-events-auto flex items-start gap-2.5 px-3.5 py-2.5 text-sm shadow-lg ${
              message.tone === 'error' ? 'border-red-300 dark:border-red-900/70' : ''
            }`}
          >
            {message.tone === 'error' ? (
              <IconAlert className="mt-0.5 h-4 w-4 shrink-0 text-red-500" />
            ) : (
              <IconCheck className="mt-0.5 h-4 w-4 shrink-0 text-emerald-500" />
            )}
            <span className="min-w-0 break-words">{message.text}</span>
          </div>
        ))}
      </div>
    </ToastContext.Provider>
  );
}
