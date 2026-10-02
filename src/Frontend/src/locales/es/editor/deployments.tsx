import type { EditorMessages } from '../../en/editor';

export const deployments: EditorMessages['deployments'] = {
  hint: (until) =>
    `Un despliegue sigue en línea mientras la gente lo usa${until ? ` (si nadie lo usa, hasta ${until})` : ''}. Volver a desplegar, o cualquier visita, reinicia ese plazo.`,
  takeOffline: 'Desconectar',
  readFailed: 'No se pudo leer el historial.',
  reading: 'Leyendo el historial…',
  none: 'Todavía no se ha desplegado nada.',
  noDescription: 'Sin descripción',
  deployed: (when, by) => `Desplegado ${when} (${by})`,
  duration: 'Tiempo en línea',
  online: 'en línea',
  short: {
    replaced: 'reemplazado',
    stopped: 'desconectado',
    expired: 'caducado',
    admin: 'por el operador',
    ended: 'finalizado',
  },
  putBack: (version) => `Volver a poner en línea la versión ${version}`,
  timeline: 'Lo que estuvo en línea en los últimos siete días',
  block: (version, from, to) => `Versión ${version}: ${from} – ${to ?? 'ahora'}`,
  weekAgo: 'hace una semana',
  now: 'ahora',
};
