import type { EditorMessages } from '../../en/editor';

export const deployments: EditorMessages['deployments'] = {
  hint: (until) =>
    `Yayına alınan sürüm, insanlar kullandığı sürece yayında kalır${until ? ` (kimse kullanmazsa ${until} tarihine kadar)` : ''}. Yeniden yayına almak ya da herhangi bir ziyaret bu süreyi sıfırlar.`,
  takeOffline: 'Yayından kaldır',
  readFailed: 'Geçmiş okunamadı.',
  reading: 'Geçmiş okunuyor…',
  none: 'Henüz hiçbir şey yayına alınmadı.',
  noDescription: 'Açıklama yok',
  deployed: (when, by) => `${when} tarihinde yayına alındı (${by})`,
  duration: 'Ne kadar yayında kaldı',
  online: 'yayında',
  short: {
    replaced: 'yenisiyle değişti',
    stopped: 'kaldırıldı',
    expired: 'süresi doldu',
    admin: 'operatör kaldırdı',
    ended: 'sona erdi',
  },
  putBack: (version) => `${version}. sürümü yeniden yayına al`,
  timeline: 'Son yedi günde yayında olanlar',
  block: (version, from, to) => `Sürüm ${version}, ${from} – ${to ?? 'şimdi'}`,
  weekAgo: 'bir hafta önce',
  now: 'şimdi',
};
