import type { AdminOverview } from '@genhttp-lambda/kit/api';

/**
 * What every section of the administration is handed by the frame around it.
 *
 * The frame holds the token and asks for it; a section only uses it, and says
 * so when the server turns it away - a token that was right when it was typed
 * can stop being right when the host is restarted with another one.
 */
export interface Access {
  token: string;
  /** The server answered as though there were no administration here. */
  deny: () => void;
  dark: boolean;
  /**
   * The installation at a glance, as the frame last read it - read again as
   * the operator opens a section, which is what keeps the marks beside the
   * sections current. Nothing until the first answer.
   */
  overview: AdminOverview | null;
  /** Something was decided that the frame counts - a request for connectivity, say - so it asks again. */
  changed: () => void;
}

/** Where a lambda's page in the panel is. */
export const lambdaPage = (publicKey: string) => `/admin/lambdas/${encodeURIComponent(publicKey)}`;

/** The listing of the lambdas, narrowed as these say. */
export const lambdasWhere = (filters: Record<string, string>) => `/admin/lambdas?${new URLSearchParams(filters)}`;

/** Free space that runs low - below a tenth of the volume, or two gigabytes - and that has run out: below a twentieth, or half a gigabyte. */
export function diskState(disk?: { size: number; free: number } | null): 'bad' | 'warn' | null {
  if (!disk || disk.size <= 0) {
    return null;
  }

  const left = disk.free / disk.size;

  if (left < 0.05 || disk.free < 512 * 1024 * 1024) {
    return 'bad';
  }

  if (left < 0.1 || disk.free < 2 * 1024 * 1024 * 1024) {
    return 'warn';
  }

  return null;
}
