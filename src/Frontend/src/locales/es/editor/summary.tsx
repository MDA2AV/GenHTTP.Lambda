import type { EditorMessages } from '../../en/editor';

export const summary: EditorMessages['summary'] = {
  reading: 'Consultando su estado…',
  readDocs: 'Leer la documentación',
  hint: (since, kept, retention, tier) =>
    `El tráfico se cuenta desde el último arranque del servidor (${since}). ` +
    (kept
      ? `Una lambda sigue en línea mientras la gente la usa, y se elimina tras ${retention} días sin visitas ni cambios.`
      : `Esta lambda está en el plan ${tier}, que la mantiene en línea y guardada aunque nadie la use.`),
  onlineFor: (duration, version) => (
    <>
      En línea desde hace {duration('un rato')}, sirviendo la versión {version}.
    </>
  ),
  offline: 'Fuera de línea. No se sirve nada hasta que despliegues una versión.',
  nothing: 'Todavía no hay nada escrito.',
  requestsToday: 'peticiones hoy',
  lastHour: (count) => `${count} en la última hora`,
  hourly: 'Peticiones por hora en el último día',
  failed: 'fallidas',
  failedTitle: (failed, rejected) =>
    `${failed} errores del servidor, ${rejected} no encontradas o rechazadas, en el último día`,
  average: 'tiempo medio de respuesta',
  noneYet: 'ninguna todavía',
  lastVisit: 'última visita',
  problems: 'Algo falló hace poco',
  openLog: 'Abrir los logs',
  latest: 'Último cambio',
  allVersions: 'Todas las versiones',
  noDescription: 'Sin descripción',
  version: (version) => `Versión ${version}`,
  notOnline: 'todavía no está en línea',
  wanted: 'Lo que se pidió',
  noVersions: 'Todavía no hay versiones.',
  inProgress: 'En curso',
  allFeatures: 'Todos los borradores',
  previewOnline: 'Su vista previa está en línea',
  previewOffline: 'Su vista previa está fuera de línea',
  behind: 'desactualizado',
  storage: 'Almacenamiento',
  versionAllowance: 'Código y recursos',
  data: 'Datos',
};
