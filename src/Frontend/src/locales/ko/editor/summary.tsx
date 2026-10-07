import type { EditorMessages } from '../../en/editor';

export const summary: EditorMessages['summary'] = {
  reading: '상태를 확인하는 중…',
  readDocs: '문서 읽기',
  hint: (since, kept, retention, tier) =>
    `트래픽은 서버가 마지막으로 시작된 뒤부터 집계해요(${since}). ` +
    (kept
      ? `람다는 사람들이 쓰는 동안 온라인으로 유지되고, 방문도 수정도 없이 ${retention}일이 지나면 삭제돼요.`
      : `이 람다는 ${tier} 플랜이라, 방문이 뜸해도 계속 온라인이고 삭제되지 않아요.`),
  onlineFor: (duration, version) => (
    <>
      온라인이 된 지 {duration('한참')} 됐어요. 지금은 버전 {version} 실행 중이에요.
    </>
  ),
  offline: '오프라인이에요. 버전을 배포하기 전까지는 아무것도 제공되지 않아요.',
  nothing: '아직 작성된 게 없어요.',
  requestsToday: '오늘 요청',
  lastHour: (count) => `최근 1시간 ${count}건`,
  hourly: '최근 하루 동안의 시간당 요청',
  failed: '실패',
  failedTitle: (failed, rejected) =>
    `최근 하루 동안 서버 오류 ${failed}건, 찾을 수 없음 또는 거부 ${rejected}건`,
  average: '평균 응답 시간',
  noneYet: '아직 없음',
  lastVisit: '마지막 방문',
  problems: '최근에 문제가 있었어요',
  openLog: '로그 열기',
  latest: '최근 변경',
  allVersions: '모든 버전',
  noDescription: '설명 없음',
  version: (version) => `버전 ${version}`,
  notOnline: '아직 온라인 아님',
  wanted: '요청 내용',
  noVersions: '아직 버전이 없어요.',
  inProgress: '작업 중',
  allFeatures: '모든 초안',
  previewOnline: '미리 보기가 온라인이에요',
  previewOffline: '미리 보기가 오프라인이에요',
  behind: '최신이 아님',
  storage: '저장 공간',
  versionAllowance: '코드와 리소스',
  data: '데이터',
};
