import type { EditorMessages } from '../../en/editor';

export const logs: EditorMessages['logs'] = {
  readFailed: '로그를 읽지 못했어요.',
  hint: (capturing) =>
    '요청, 람다가 출력한 내용, 문제가 생긴 곳을 실시간으로 보여 줘요.' +
    (capturing ? '' : ' 이 서버는 람다의 출력을 보관하지 않아서, 요청과 오류만 나와요.') +
    ' 메모리에 보관되고 이 서버의 모든 람다가 함께 쓰기 때문에, 몇 분에서 몇 시간 전까지만 남고 재시작하면 비워져요. 방문자의 IP 주소는 표시하지 않아요.',
  featureHint: (capturing) =>
    '이 초안의 미리 보기가 응답하고, 출력하고, 던진 것을 실시간으로 보여 줘요.' +
    (capturing ? '' : ' 이 서버는 람다의 출력을 보관하지 않아서, 요청과 오류만 나와요.') +
    ' 람다 자체의 로그와는 따로 보관되고, 람다 로그에는 미리 보기가 나오지 않아요. 메모리에 보관되기 때문에 몇 분에서 몇 시간 전까지만 남아요.',
  nothingPreview: '아직 없어요. 초안의 미리 보기를 열면 요청이 여기에 나와요.',
  search: '검색',
  searchLabel: '로그 검색',
  resume: '새 줄을 실시간으로 보기',
  pause: '읽는 동안 새 줄 추가 멈추기',
  paused: '일시정지',
  live: '실시간',
  show: '표시',
  all: '전체',
  requests: '요청',
  output: '출력',
  problems: '문제',
  reading: '로그를 읽는 중…',
  noProblems: '로그에 남아 있는 문제가 없어요.',
  nothing: '아직 없어요. 람다 주소를 열면 요청이 여기에 나와요.',
  noMatch: '일치하는 항목이 없어요.',
  identical: (count) => `같은 줄 ${count}개`,
  at: (domain) => `, ${domain} 도메인`,
  from: (country) => `, ${country}에서 접속`,
};
