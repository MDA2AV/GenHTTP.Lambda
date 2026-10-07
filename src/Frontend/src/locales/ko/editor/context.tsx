import type { EditorMessages } from '../../en/editor';

export const context: EditorMessages['context'] = {
  docs: {
    title: '문서',
    titleSimple: '내 앱 소개',
    hint: '이 앱이 무엇이고, 누구를 위한 것이며, 왜 있는지, 그리고 왜 이렇게 만들어졌는지 적은 거예요. 에이전트가 수정할 때마다 작성하고 버전마다 함께 보관되기 때문에, 이전 버전으로 돌아가면 그 버전에 맞는 문서도 함께 돌아와요.',
    hintSimple: '앱이 무엇을 위한 것이고 왜 필요한지, 요청한 내용을 바탕으로 에이전트가 이해한 대로 적은 거예요. 수정할 때마다 에이전트가 최신 내용으로 고쳐 둬요.',
    inDraft: '이 초안의 문서예요. 초안을 확정하면 앱의 문서가 돼요.',
    pages: { product: '제품', decisions: '설계 결정' },
    emptyTitle: '아직 작성된 게 없어요',
    emptyText: (code) => (
      <>
        에이전트는 수정하면서 문서도 작성해요. 앱이 무엇이고, 누구를 위한 것이며, 왜 있는지는{' '}
        {code('docs/product.md')}에, 왜 이렇게 만들어졌는지는 {code('decisions.md')}에 적어요. 문서는 버전의
        일부로, 코드 옆에 있어요.
      </>
    ),
    emptySimpleTitle: '아직 앱 소개가 없어요',
    emptySimple: '요청한 내용을 바탕으로, 앱이 무엇을 위한 것이고 왜 필요한지 에이전트가 설명해 줄 수 있어요. 그 뒤로는 에이전트가 설명을 최신 내용으로 유지해요.',
    ask: '에이전트에게 작성 요청하기',
    describe: '에이전트에게 설명 요청하기',
    writePrompt: '이 앱의 문서를 작성해 주세요. 앱이 무엇이고, 누구를 위한 것이며, 왜 있는지, 그리고 그 바탕이 된 기술적 결정을 적어 주세요.',
    describePrompt: '이 앱이 무엇을 위한 것이고 왜 필요한지, 내가 ‘앱 소개’에서 읽을 수 있게 설명해 주세요.',
    decisionsPrompt: '이 앱의 바탕이 된 기술적 결정과, 그렇게 결정한 이유를 적어 주세요.',
    missingProduct: '아직 제품 페이지가 없어요',
    missingProductText: '앱이 무엇이고, 누구를 위한 것이며, 사람들이 앱으로 무엇을 하고 왜 그런지를 요청한 사람의 말로 적어요.',
    missingDecisions: '아직 적어 둔 설계 결정이 없어요',
    missingDecisionsText: '앱이 어떻게, 왜 그렇게 만들어졌는지. 데이터를 어떻게 보관하는지, 무엇에 의존하는지, 무엇을 뺐는지. 다음에 고칠 사람이 알아야 할 것들이에요.',
    correctText: '요청한 내용을 바탕으로 에이전트가 작성하고, 수정할 때마다 최신 내용으로 유지해요. 틀리거나 빠진 게 있나요? 에이전트에게 말해 주세요.',
    correct: '에이전트에게 말하기',
    correctPrompt: '앱 설명을 이렇게 고쳐 주세요: ',
    placeholder: '항목을 1년 동안 보관하는 이유 설명',
  },
  tests: {
    title: '테스트',
    hint: '이 앱을 자동으로 테스트하는 방법과, 테스트에 쓰는 스크립트와 데이터예요. 에이전트가 최신 상태로 유지하고, 수정을 마치기 전에 실행해요. 버전마다 함께 보관돼요.',
    inDraft: '이 초안의 테스트예요. 초안을 확정하면 앱의 테스트가 돼요. 그 전에 초안의 미리 보기를 대상으로 실행해 보세요.',
    pages: { testing: '테스트 방법' },
    emptyTitle: '아직 테스트가 없어요',
    emptyText: (code) => (
      <>
        앱을 테스트하는 방법(계속 동작해야 하는 것, 그걸 확인하는 방법, 이를 위한 스크립트 실행 방법)은 에이전트가{' '}
        {code('tests/README.md')}에 적고, 스크립트와 테스트 데이터는 그 옆에 둬요.
      </>
    ),
    ask: '에이전트에게 테스트 작성 요청하기',
    writePrompt: '이 앱의 테스트를 작성해 주세요. 계속 동작해야 하는 것과 그걸 자동으로 확인하는 방법을, 미리 보기를 대상으로 실행할 스크립트와 함께 만들어 주세요.',
    missing: '아직 테스트 방법이 적혀 있지 않아요',
    missingText: '계속 동작해야 하는 것, 각각을 확인하는 방법, 옆에 있는 스크립트를 실행하는 방법.',
    placeholder: '목록이 가득 차면 새 항목을 거부하는지 확인',
  },
  files: '파일',
  noFiles: '페이지 말고 다른 파일은 없어요.',
  none: '없음',
  missingPill: '아직 작성되지 않음',
  changedIn: (version) => `버전 ${version}에서 바뀜`,
  changedInDraft: '이 초안에서 바뀜',
  showChanges: '바뀐 내용 보기',
  hideChanges: '바뀐 내용 숨기기',
  noChanges: '바뀐 게 없어요.',
  edit: '편집',
  olderVersion: '버전은 바뀌지 않아요. 페이지는 최신 버전이나 초안에서 편집해요.',
  writeIt: '직접 작성하기',
  askPage: '에이전트에게 작성 요청하기',
  editInCode: '코드에서 열기',
  cancel: '취소',
  save: '저장',
  write: '작성',
  preview: '미리 보기',
  writeOrPreview: '작성 또는 미리 보기',
  discard: '이 페이지에서 바꾼 내용이 사라져요. 버릴까요?',
  reading: '읽는 중…',
  readFailed: '읽지 못했어요.',
  saveFailed: '저장하지 못했어요.',
  savedDraft: '초안에 저장했어요.',
  savedVersion: (version) => `저장했어요 (버전 ${version}).`,
  savedOnline: (version) => `저장하고 온라인에 올렸어요 (버전 ${version}).`,
  savedNotOnline: (version) => `저장했지만 온라인에 올리지 못했어요 (버전 ${version}).`,
  saveTitle: '새 버전으로 저장',
  saveText: (newest) =>
    `버전은 바뀌지 않기 때문에, 이 페이지는 다음 버전으로 저장돼요. 버전 ${newest} 기준이고, 나머지는 모두 그대로예요.`,
  clash: (version) => `편집을 시작한 뒤에 새 버전(버전 ${version})이 저장됐고, 거기서도 이 페이지가 바뀌었어요. 저장하면 그 내용을 대체해요.`,
  alsoOnline: '온라인에도 올리기',
  alsoOnlineNote: '문서만 바뀌기 때문에 방문자에게 새로 보이는 건 없어요. 다만 온라인 버전이 계속 최신 버전으로 유지돼요.',
  skeleton: {
    product: '# 앱 이름\n\n이 앱이 무엇인지 한두 문장으로.\n\n## 누구를 위한 앱인가\n\n## 사람들이 이 앱으로 하는 일\n\n## 기능과 그 기능이 있는 이유\n\n## 하지 않는 일\n',
    decisions: '# 설계 결정\n\n## 결정 하나\n\n무엇을 결정했는지, 왜 그랬는지, 수정할 때 무엇을 염두에 둬야 하는지.\n',
    testing: '# 테스트 방법\n\n테스트를 실행하는 방법과, 어느 주소를 대상으로 하는지.\n\n## 계속 동작해야 하는 것\n\n| 동작 | 요청 | 기대 결과 |\n|---|---|---|\n| | | |\n',
  },
};
