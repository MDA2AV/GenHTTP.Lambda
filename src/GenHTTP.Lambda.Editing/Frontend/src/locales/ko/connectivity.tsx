import type { EditorMessages } from '../en';

export const connectivity: EditorMessages['connectivity'] = {
  hint:
    '코드가 다른 서버에 연결할 수 있는지 정해요. API 호출, 소켓 열기, 메일 보내기 등이 해당돼요. 람다는 서버 하나를 함께 쓰기 때문에 람다마다 허용해요. 프리미엄 람다는 바로 쓸 수 있고, 그 외 람다는 이 사이트의 운영자가 허용한 뒤에 쓸 수 있어요.',

  included: '프리미엄에 포함',
  includedText: '코드가 다른 서버에 연결할 수 있어요. 프리미엄에 포함되어 있어요.',
  allowed: '허용됨',
  allowedText: (ago) => <>이 사이트의 운영자가 {ago} 허용했어요. 코드가 다른 서버에 연결할 수 있어요.</>,
  requested: '운영자를 기다리는 중',
  requestedText: (ago) => (
    <>{ago} 요청했어요. 이 사이트의 운영자가 허용하면 연결하는 코드가 컴파일돼요. 그때까지는 거부돼요.</>
  ),
  declined: '거절됨',
  declinedText: (ago) => <>이 사이트의 운영자가 {ago} 거절했어요. 용도를 더 자세히 적어서 다시 요청할 수 있어요.</>,
  none: '허용되지 않음',
  noneText:
    '코드가 다른 서버에 연결할 수 없어요. API를 호출하거나 소켓을 열거나 메일을 보내는 코드는 컴파일되지 않아요. 람다에 필요하다면 아래에서 요청하세요.',
  operatorSaid: '운영자의 답변',
  purposeSaid: '용도',

  connects: '코드가 다른 서버에 연결하는데, 이 람다는 아직 그럴 수 없어요. 허용될 때까지 컴파일되지 않아요.',
  connectsWaiting: '코드가 다른 서버에 연결해요. 운영자가 허용하면 컴파일돼요.',

  askTitle: '요청하기',
  askAgainTitle: '다시 요청하기',
  editTitle: '용도',
  purpose: '무엇에 연결하고, 왜 필요한가요?',
  purposeHint: '서버나 서비스의 이름과, 람다가 그것으로 무엇을 하는지 적어 주세요. 이 사이트의 운영자가 읽고 결정해요.',
  purposeExample: 'api.open-meteo.com, 첫 페이지의 일기 예보용',
  send: '요청 보내기',
  save: '저장',
  cancel: '취소',
  edit: '용도 수정',
  asked: '요청했어요. 이 사이트의 운영자가 결정해요.',
  askFailed: '요청을 보내지 못했어요.',
  withdraw: '요청 취소',
  withdrawn: '요청을 취소했어요.',
  giveUp: '포기하기',
  givenUp: '이제 코드가 다른 서버에 연결할 수 없어요.',
  giveUpTitle: '다른 서버에 연결하는 것을 포기할까요?',
  giveUpText:
    '다음 요청부터 코드가 다른 서버에 연결할 수 없고, 온라인 버전도 연결하는 부분에서 작동하지 않아요. 다시 쓰려면 다시 요청하세요.',
  keep: '유지',
  failed: '실패했어요.',

  coversTitle: '해당하는 것',
  covers: ['HttpClient와 WebRequest', '소켓과 WebSocket 클라이언트', '메일(SmtpClient)', 'DNS와 ping', '주소에서 불러오는 XML'],
  notCovered:
    '브라우저에서 일어나는 일은 해당하지 않아요. 람다의 페이지는 다른 사이트에서 직접 가져올 수 있고, 방문자, 웹훅, 웹소켓은 예전처럼 람다에 도달해요. 브라우저가 호출할 수 있는 공개 API는 페이지에서 호출하는 것이 가장 좋아요. 허용이 필요 없어요.',

  refused: '코드가 다른 서버에 연결해요',
  refusedText: '이 람다는 아직 그럴 수 없어요. 요청하면 이 사이트의 운영자가 결정해요.',
  openConnectivity: '요청하기',
};
