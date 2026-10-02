import type { EditorMessages } from '../../en/editor';

export const deployments: EditorMessages['deployments'] = {
  hint: (until) =>
    `배포는 사람들이 쓰는 동안 계속 온라인이에요${until ? `. 아무도 쓰지 않으면 ${until}까지 유지돼요` : ''}. 다시 배포하거나 누군가 방문하면 이 기간이 새로 시작돼요.`,
  takeOffline: '오프라인으로 전환',
  readFailed: '기록을 읽지 못했어요.',
  reading: '기록을 읽는 중…',
  none: '아직 배포한 적이 없어요.',
  noDescription: '설명 없음',
  deployed: (when, by) => `${when} 배포 (${by})`,
  duration: '온라인이었던 시간',
  online: '온라인',
  short: {
    replaced: '교체됨',
    stopped: '오프라인 전환',
    expired: '만료됨',
    admin: '운영자가 중단',
    ended: '종료됨',
  },
  putBack: (version) => `버전 ${version} 다시 온라인에 올리기`,
  timeline: '최근 7일 동안 온라인이었던 버전',
  block: (version, from, to) => `버전 ${version}, ${from} ~ ${to ?? '지금'}`,
  weekAgo: '일주일 전',
  now: '지금',
};
