import type { EditorMessages } from '../../en/editor';

export const analytics: EditorMessages['analytics'] = {
  hint: '플랫폼이 대신 집계해요. 쿠키도, 앱에 넣을 코드도 필요 없고, 방문자가 동의할 것도 없어요.',
  loading: '방문 기록을 불러오는 중…',
  loadFailed: '방문 기록을 불러오지 못했어요.',

  offTitle: '내 앱에 누가 방문하는지 확인하세요',
  offText:
    '하루에 몇 명이 오는지, 어떤 페이지를 여는지, 어떤 사이트에서 오는지, 어느 나라에서 어떤 기기로 오는지를 켠 순간부터 하루 단위로 집계해요.',
  offPrivate: '쿠키도 없고, 방문자가 동의해야 하는 배너도 없어요',
  offAddresses: '주소는 저장하지 않고, 하루에 방문이 몇 번이었는지만 기록해요',
  offKept: (days) =>
    days >= 365
      ? '1년 동안 보관하고, 전부 보여 드려요'
      : `1년 동안 보관하며, 최근 ${days}일만 보여 드려요. 전체는 프리미엄에서 볼 수 있어요`,
  switchOn: '분석 켜기',
  switchedOn: '분석을 켰어요. 첫 방문은 몇 분 안에 나타나요.',
  switchOnFailed: '분석을 켜지 못했어요.',
  demo: '데모의 방문자는 집계하지 않아요. 직접 만든 복사본은 방문자를 집계할 수 있어요.',

  range: '기간',
  days: (days) => (days >= 365 ? '12개월' : `${days}일`),
  premiumRange: '프리미엄에서는 최대 1년까지 볼 수 있어요',

  visitors: '방문자',
  visitorsHint: '서로 다른 사람을 하루에 한 번씩 세어요.',
  views: '페이지 조회수',
  viewsHint: '누군가 페이지를 열 때마다 세어요.',
  hits: '요청',
  hitsHint: '앱이 받은 모든 요청이에요. 페이지와, 페이지가 불러오는 그림과 스크립트를 포함해요.',
  versus: (days) => `직전 ${days}일 대비`,
  noChange: '변화 없음',
  inDays: (days) => (days >= 365 ? '최근 12개월 동안' : `최근 ${days}일 동안`),

  perDay: {
    visitors: '일별 방문자',
    views: '일별 페이지 조회수',
    hits: '일별 요청',
  },
  perWeek: {
    visitors: '주별 방문자',
    views: '주별 페이지 조회수',
    hits: '주별 요청',
  },
  showValues: '값 보기',
  showChart: '차트 보기',
  day: '날짜',
  week: '주 시작일',

  nobody: '이 기간에는 방문자가 없었어요.',
  nobodyYet: '분석을 켠 뒤로 방문자가 없어요. 첫 방문은 몇 분 안에 나타나요.',

  pages: '페이지',
  pagesHint: '사람들이 연 페이지예요. 가장 많이 열린 것부터 보여 줘요.',
  requested: '가장 많이 요청된 것',
  requestedHint: '앱이 받은 모든 요청을 주소별로 보여 줘요. 이 앱에는 사람들이 여는 페이지가 없어요.',
  sources: '방문 경로',
  sourcesHint: '사람들을 보낸 사이트를, 방문자가 있던 페이지별로 보여 줘요. 검색 엔진은 검색 엔진에서 왔다는 것만 알려 주고, 검색어는 알려 주지 않아요.',
  direct: '직접 입력, 북마크 또는 앱',
  countries: '국가',
  countriesHint: '방문자의 인터넷 주소가 등록된 곳이에요. 대개 실제 위치와 같지만, 항상 그런 것은 아니에요.',
  unknownCountry: '알 수 없음',
  devices: '기기',
  device: {
    desktop: '컴퓨터',
    mobile: '휴대폰',
    tablet: '태블릿',
    bot: '봇',
    other: '기타',
  },
  browsers: '브라우저',
  systems: '운영체제',
  crawlers: '검색 엔진과 AI',
  crawlersHint: '찾아온 프로그램이에요. 페이지를 색인하는 검색 엔진, 페이지를 읽는 AI 어시스턴트, 링크 미리보기를 보여 주는 채팅이 있어요.',
  noCrawlers: '이 기간에는 검색 엔진이나 AI가 찾아오지 않았어요.',
  other: '그 외',
  otherBots: '다른 프로그램',
  showAll: (count) => `${count}개 모두 보기`,
  showFewer: '적게 보기',
  share: (part, whole) => `${whole} 중 ${part}`,
  none: '아직 없어요.',

  moreKept: (since) => `${since}부터 집계했어요. 프리미엄에서는 최대 1년까지 전부 볼 수 있어요.`,
  premiumAsk: '프리미엄 문의',
  premiumSubject: '내 앱의 프리미엄',
  premiumBody: (address) => `안녕하세요.\n\n제 앱의 분석을 모두 보고 싶습니다: ${address}\n\n`,

  footer: (minutes) =>
    minutes === 1 ? '하루 단위로, UTC 기준으로 집계해요. 1분마다 갱신돼요.' : `하루 단위로, UTC 기준으로 집계해요. ${minutes}분마다 갱신돼요.`,
  updated: (time) => `마지막 갱신: ${time}.`,
  since: (date) => `${date}부터 집계 중이에요.`,

  switchOff: '분석 끄기',
  switchOffTitle: '분석을 끌까요?',
  switchOffText: '집계가 멈추고, 지금까지 집계한 내용이 모두 삭제돼요. 되돌릴 수 없어요.',
  keep: '계속 집계',
  switchOffConfirm: '끄고 삭제',
  switchedOff: '분석을 껐고, 집계한 내용을 삭제했어요.',
  switchOffFailed: '분석을 끄지 못했어요.',
};
