import type { KitMessages } from '../en';

export const workspace: KitMessages['workspace'] = {
  /** The button at the top of the list of apps, which starts on a new one. */
  newApp: '새 앱',
  /** The heading of the list of apps this browser remembers. */
  yourApps: '내 앱',
  /** Under the list: where it is kept. */
  keptHere: '이 브라우저에만 저장돼요. 다른 곳에서 앱을 열려면 에디터 링크를 사용하세요.',
  /** On a phone, the button that shows the list of apps. */
  showApps: '내 앱',
  closeApps: '목록 닫기',
  /** In the list, beside an app the agent is working on. */
  working: '에이전트가 작업 중이에요',
  /** In the list, beside an app that is not online. */
  offline: '오프라인',

  add: '앱 추가',
  addLabel: '에디터 링크 또는 에디터 키',
  addButton: '추가',
  addNotKey: '에디터 링크나 에디터 키가 아니에요.',
  addUnknown: '이 에디터 키를 가진 앱이 없어요.',
  addDemo: '데모는 아무도 수정할 수 없어요. 에디터에서 열어 나만의 사본을 시작하세요.',
  addFailed: '지금은 추가할 수 없어요. 잠시 후 다시 시도해 주세요.',

  /** The state of an app, at the top of its conversation. */
  online: '온라인',
  blocked: '차단됨',
  openApp: '앱 열기',
  more: '더 보기',
  openEditor: '에디터 열기',
  copyEditor: '에디터 링크 복사',
  forget: '목록에서 제거',
  forgetTitle: '목록에서 제거할까요?',
  forgetText:
    '앱은 계속 온라인에 있고 달라지는 것은 없어요. 이 브라우저만 에디터 링크를 잊어요. 다시 추가하려면 그 링크가 필요해요.',
  cancel: '취소',
  gone: '이 앱은 더 이상 없어요',
  goneText: '앱이 삭제됐거나, 에디터 키가 이 브라우저가 아는 키와 달라졌어요. 목록에서 제거하세요.',
  unreadable: '지금은 앱을 읽을 수 없어요.',

  /** What waits for the owner, above the conversation. */
  attention: {
    offlineTitle: '앱이 오프라인이에요',
    offlineText: '다시 온라인으로 전환할 때까지 아무도 열 수 없어요.',
    putBack: '온라인으로 전환',
    keysTitle: (count) => (count === 1 ? '앱에 키 또는 비밀번호가 필요해요' : `앱에 키와 비밀번호 ${count}개가 필요해요`),
    keysText: '여기에 값을 입력하세요. 앱만 읽을 수 있고, 에이전트를 포함해 누구도 다시 볼 수 없어요.',
    value: '값',
    save: '저장',
    /** A draft the agent left for the owner to try: its name follows. */
    draftTitle: (name) => `써 볼 준비가 됐어요: ${name}`,
    draftText: '자기만의 주소에 테스트 데이터가 있는 초안이에요. 온라인으로 전환하기 전에는 다른 사람에게 보이지 않아요.',
    tryIt: '써 보기',
    putOnline: '온라인으로 전환',
    putOnlineTitle: '이 초안을 온라인으로 전환할까요?',
    putOnlineText: '앱을 여는 모든 사람이 지금부터 이 변경을 보게 돼요. 이전에 온라인이던 내용은 기록에 남아 있어서 되돌릴 수 있어요.',
    putOnlineFailed: '초안이 아직 제대로 동작하지 않아서 온라인으로 전환하지 않았어요. 에이전트에게 고쳐 달라고 하세요.',
    /** A draft begun before the app last changed, which has to take that change in before it can go online. */
    outOfDate: '최신이 아님: 이 초안을 시작한 뒤에 앱이 바뀌었어요.',
    bringUp: '최신으로 맞추기',
    /** What the button above puts into the box, for the agent to do in that draft. */
    bringUpAsk: '이 초안을 지금 온라인에 있는 내용에 맞춰 최신으로 만들고, 초안의 변경은 그대로 유지해 줘.',
    /** Above the box, while the next message goes to a draft rather than a new one. */
    inDraft: (name) => `초안 “${name}”에서`,
    leaveDraft: '대신 새 초안 시작',
    blockedTitle: '이 사이트의 운영자가 이 앱을 차단했어요',
    blockedText: '앱이 오프라인이고 아무것도 수정할 수 없어요. 에디터에서 이유를 확인하세요.',
    failed: '잘 되지 않았어요. 잠시 후 다시 시도해 주세요.',
  },

  /** For an app with no conversation yet. */
  firstTitle: (name) => `${name}에 대해 무엇이 궁금하거나, 무엇을 바꾸고 싶으세요?`,
  firstText: '방문자가 누구인지, 무엇이 입력됐는지, 문제가 없는지 물어보거나, 달라졌으면 하는 점을 말해 보세요.',
  /** What the owner might ask about an app, offered before they type. */
  suggestions: [
    '이번 주에 몇 명이 방문했어?',
    '최근에 사람들이 무엇을 입력했어?',
    '문제가 생긴 게 있어?',
    '휴대폰에서 더 예쁘게 보이게 해 줘',
    '마지막 변경을 되돌려 줘',
  ],

  /** The box at the bottom: for a new app, and for one that exists. */
  placeholderApp: (name) => `${name}에 대해 묻거나 바꿀 점을 말해 보세요…`,
  send: '보내기',
  stop: '중지',
  stopping: '중지하는 중…',
  goOnline: '변경을 바로 온라인으로 전환',
  goOnlineOn: '변경이 잘 되는 즉시 온라인으로 전환돼요.',
  goOnlineOff: '변경은 초안으로 남겨 두니, 먼저 써 볼 수 있어요.',
  model: '모델',
  withPassword: (model) => `${model} (비밀번호)`,
  effort: '사고 수준',
  efforts: {
    low: '낮음',
    medium: '보통',
    high: '높음',
    xhigh: '매우 높음',
    max: '최대',
  },
  ranOn: (model, effort) => `${model}, 사고 수준: ${effort}`,
  left: (left, perDay) => `오늘 ${perDay}회 중 ${left}회 남았어요`,
  noneLeft: (time) => `오늘은 여기까지예요. ${time}부터 다시 요청할 수 있어요.`,
  /** Where the operator of the site switched building off, or there is no agent. */
  noBuilds: '이 사이트에서는 새 앱을 만들지 않아요. 내 AI 어시스턴트를 연결해서 만들어 보세요.',
  /** Where the operator switched asking the agent about an app off. */
  noChanges: '이 사이트에서는 에이전트에게 묻는 기능이 꺼져 있어요. 에디터에서, 또는 내 AI 어시스턴트로 앱을 수정하세요.',
  connectOwn: '내 어시스턴트 연결',

  /** The two sides of the conversation, for whoever cannot see who said what. */
  you: '나',
  agent: '에이전트',
  ahead: (waiting) => (waiting === 1 ? '앞에 요청이 하나 있어요. 다음 차례예요.' : `앞에 요청이 ${waiting}개 있어요.`),
  leaveOpen: '이 페이지를 닫아도 돼요. 에이전트는 계속 작업하고, 돌아오면 답변이 여기에 있어요.',
  showWork: '한 일 보기',

  /** What came of a message, under the agent's answer. */
  results: {
    online: '변경이 온라인에 반영됐어요',
    undo: '되돌리기',
    undoTitle: '이전에 온라인이던 내용으로 되돌리기',
    draft: '초안으로 써 볼 준비가 됐어요',
    draftBroken: '초안이 아직 제대로 동작하지 않아요',
    saved: '변경은 저장됐지만 온라인에는 없어요',
    putOnline: '온라인으로 전환',
    earlier: '앱이 이전 상태로 돌아갔어요',
    stopped: '중지됐어요. 이미 저장한 것은 그대로 남아요.',
    failed: '잘 되지 않았어요.',
    timedOut: '끝나기 전에 시간이 초과됐어요.',
    usedUp: '끝나기 전에 허용된 단계를 모두 썼어요.',
    unauthorised: '에이전트가 로그인하지 못해서 아무것도 실행되지 않았어요. 요청이 아니라 이 사이트의 문제예요.',
    /** A message whose answer never came back: the agent forgot it, or another took its place. */
    lost: '답변이 돌아오지 않았어요. 에이전트가 한 일은 앱의 에디터에 있는 기록에서 볼 수 있어요.',
    /** The settings it changed, each a small mark under the answer. */
    settings: {
      analytics: '지금부터 방문자를 집계해요',
      database: '기록을 켰어요',
      secrets: '키와 비밀번호를 켰어요',
      workspace: '파일 저장을 켰어요',
      source: '코드를 공개했어요',
      connectivity: '다른 서비스 사용 권한을 요청했어요',
      discarded: '초안을 삭제했어요',
    },
    seeSource: '코드 보기',
    /** After the first build: the editor link is remembered here as well. */
    remembered: '이 브라우저도 내 앱 아래에 기억해 둬요.',
  },

  /** What the agent is doing while it works on an app, in the words of this page. */
  steps: {
    reading: '앱을 살펴보고 있어요',
    changing: '변경하고 있어요',
    draft: '초안을 시작하고 있어요',
    upToDate: '초안을 최신으로 맞추고 있어요',
    putOnline: '온라인으로 전환하고 있어요',
    discard: '초안을 삭제하고 있어요',
    visitors: '방문자를 확인하고 있어요',
    publish: '코드를 공개하고 있어요',
    errors: '오류를 확인하고 있어요',
  },
};
