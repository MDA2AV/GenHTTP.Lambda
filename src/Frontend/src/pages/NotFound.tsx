import { useT } from '../i18n';
import { Link } from '../i18n/links';
import { usePageMeta } from '../meta';

export function NotFound() {
  const t = useT();

  usePageMeta({ title: t.notFound.title, index: false });

  return (
    <div className="mx-auto flex w-full max-w-xl flex-col items-start px-5 py-24">
      <h1 className="text-2xl font-bold tracking-tight">{t.notFound.heading}</h1>
      <p className="mt-3 text-[15px] text-slate-600 dark:text-slate-400">{t.notFound.text}</p>
      <Link to="/" className="btn-primary mt-8 px-5 py-2.5">
        {t.common.backToStart}
      </Link>
    </div>
  );
}
