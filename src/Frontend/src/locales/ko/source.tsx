import type { SourceMessages } from '../en/source';

/** 공개된 소스 코드 페이지(/source와 그 아래 각 프로젝트)의 한국어 문구. */
export const source: SourceMessages = {
  shell: {
    section: '오픈 소스',
    home: 'GenHTTP Lambda 첫 페이지',
  },

  lambda: {
    label: '람다란?',
    text: 'GenHTTP Lambda에서 돌아가는 웹앱이에요. 원하는 걸 말하면 AI 에이전트가 C#으로 작성하고, 몇 분 안에 전용 주소에서 온라인이 돼요. 모든 버전이 무엇을 바꿨는지와 함께 남아요.',
    build: '나도 만들어 보기',
  },

  catalog: {
    eyebrow: '오픈 소스',
    title: '여기서 만든 앱, 어떻게 만들었는지 보세요',
    intro:
      '만든 사람이 코드를 공개한 람다예요. 모든 버전과 버전마다 바뀐 내용, 문서, 테스트를 볼 수 있어요. 여기서 읽거나, .NET이 돌아가는 곳이라면 어디서든 실행되는 프로젝트로 다운로드하세요.',
    searchLabel: '프로젝트 검색',
    searchPlaceholder: '이름이나 하는 일로 검색',
    orderLabel: '정렬',
    orders: {
      stars: '별표 많은 순',
      updated: '최근 변경순',
      published: '최근 공개순',
    },
    counted: (total) => `프로젝트 ${total}개`,
    failed: '프로젝트를 불러오지 못했어요.',
    loadingMore: '더 불러오는 중…',
    showMore: '더 보기',
    nothingTitle: '아직 공개된 앱이 없어요',
    nothing: (tab) => (
      <>
        다른 사람이 배울 만한 걸 만들었나요? 관리 화면을 열고 {tab('오픈 소스')}를 선택한 다음 라이선스를 고르면,
        코드가 여기에 나와요.
      </>
    ),
    noMatchTitle: '일치하는 항목이 없어요',
    noMatch: (query) => `‘${query}’에 해당하는 공개 프로젝트가 없어요.`,
    clear: '모든 프로젝트 보기',
    yoursTitle: '내 앱도 공개하고 싶나요?',
    yours: (tab) => (
      <>
        람다의 관리 화면을 열고 {tab('오픈 소스')}를 선택하거나, 람다를 만든 에이전트에게 공개해 달라고 하세요. 에디터
        키를 가진 사람만 공개할 수 있고, 라이선스도 그 사람이 골라요. 앱이 보관하는 것(기록, 파일, 키)은 절대 포함되지
        않아요.
      </>
    ),
    build: '만들어 보기',
    online: '온라인',
    offline: '오프라인',
    changed: (ago) => `${ago} 변경`,
    stars: (count) => `별표 ${count}개`,
  },

  project: {
    loading: '소스 코드를 불러오는 중…',
    failed: '소스 코드를 불러오지 못했어요.',
    missingTitle: '여기에 공개된 소스 코드가 없어요',
    missing: '만든 사람이 공개를 내렸거나, 이 주소에 람다가 있었던 적이 없을 수 있어요.',
    all: '모든 프로젝트',
    by: (name) => `만든 사람: ${name}`,
    versions: (count) => `버전 ${count}개`,
    onlineAt: (address) => <>온라인: {address}</>,
    offline: '지금은 오프라인',
    openApp: '앱 열기',
    opens: (address) => `새 탭에서 ${address} 열기`,
    published: (ago) => `${ago} 공개`,
    changed: (ago) => `${ago} 변경`,
    picture: (name) => `${name} 화면`,
    tabsLabel: '읽을 내용',
    tabs: {
      code: '코드',
      docs: '문서',
      tests: '테스트',
      changes: '변경 내역',
    },
  },

  versions: {
    label: '버전',
    choose: '다른 버전 읽기',
    newest: '최신',
    online: '온라인',
    older: (version, ago, newest) =>
      `${ago} 저장된 버전 ${version}의 내용을 보고 있어요. 최신 버전은 따로 있어요 (버전 ${newest}).`,
    toNewest: '최신 버전 읽기',
    noChange: '변경 메모 없음',
  },

  star: {
    star: '별표',
    add: '이 프로젝트에 별표 주기',
    remove: '별표 취소',
    count: (count) => `별표 ${count}개`,
    failed: '별표를 저장하지 못했어요.',
  },
  clone: {
    button: '코드',
    title: 'git으로 클론하기',
    what: (oldest, newest) =>
      oldest === newest
        ? `이 버전은 main의 커밋이고, v${newest} 태그가 붙어 있어요.`
        : `모든 버전이 main의 커밋으로 들어 있고, v${oldest}부터 v${newest}까지 태그가 붙어 있어요. main이 가장 최신이에요.`,
    readOnly:
      '읽기 전용이에요. 이를 바탕으로 만들려면 나만의 람다를 시작해서 이 파일들을 옮기세요. 방법은 클론 안의 AGENTS.md에, 할 수 있는 일은 라이선스에 적혀 있어요.',
  },

  download: {
    title: (version) => `버전 ${version} 프로젝트`,
    what:
      'Dockerfile, 문서, 테스트, 라이선스가 들어 있는 .NET 10 프로젝트예요. 앱이 보관하는 것(기록, 저장한 파일, 키)은 들어 있지 않아요.',
    zip: 'ZIP 다운로드',
    preparing: '프로젝트를 준비하는 중…',
    slow: '버전을 처음 다운로드할 때는 그 자리에서 준비하느라 조금 걸려요.',
    failed: '프로젝트를 준비하지 못했어요. 잠시 후 다시 시도해 주세요.',
    run: '실행 방법',
    local: '.NET 10 SDK로:',
    container: '또는 컨테이너로:',
    agent: '또는 폴더를 내 코딩 에이전트에게 넘겨서 그 위에 더 만들 수도 있어요. 라이선스는 지켜 주세요.',
    copy: '복사',
    copied: '복사됨',
  },

  tree: {
    label: '파일',
    files: (count) => `파일 ${count}개`,
    packing: '이 버전을 준비하는 중…',
    packingSlow: '버전은 누군가 처음 읽을 때 준비돼요. 큰 버전은 조금 걸려요.',
    failed: '이 버전의 파일을 불러오지 못했어요.',
    legend: '범례',
    kinds: {
      code: '람다 자체의 코드',
      asset: '제공하는 것: 페이지, 스크립트, 스타일, 이미지, 그리고 데이터베이스 마이그레이션',
      docs: '앱이 무엇이고, 왜 이렇게 만들어졌는지',
      tests: '테스트 방법',
      build: '빌드 도구로 코드나 에셋을 만드는 원본',
      platform: '플랫폼을 대신하는 것',
      project: '호스트, 빌드, 컨테이너, 라이선스',
    },
    short: {
      code: '코드',
      asset: '제공',
      docs: '문서',
      tests: '테스트',
      build: '빌드',
      platform: '플랫폼',
      project: '프로젝트',
    },
  },

  file: {
    loading: '불러오는 중…',
    failed: '이 파일을 불러오지 못했어요.',
    missing: (path) => `이 버전에는 ${path} 파일이 없어요.`,
    binary: '이 파일은 텍스트가 아니에요.',
    tooLarge: '이 파일은 너무 길어서 여기서 보여 줄 수 없어요.',
    download: '다운로드',
    raw: '원본',
    rawTitle: '파일을 있는 그대로 열기',
    copy: '복사',
    copied: '복사됨',
    lines: (count) => `${count}줄`,
    plain: '파일이 길어서 색 구분 없이 보여 줘요.',
    line: (line) => `줄 ${line}`,
  },

  docs: {
    pages: '페이지',
    product: '앱 소개',
    decisions: '설계 결정',
    loading: '불러오는 중…',
    failed: '이 페이지를 불러오지 못했어요.',
    noneTitle: '이 버전에 대해 작성된 게 없어요',
    none: '문서는 docs/에 들어가요. 앱이 무엇이고, 누구를 위한 것이며, 왜 이렇게 만들어졌는지 적는 곳이에요.',
  },

  tests: {
    files: '스크립트와 데이터',
    noneTitle: '이 버전에는 테스트에 대한 설명이 없어요',
    none: '테스트 방법은 tests/README.md에 적고, 실행하는 스크립트는 그 옆에 둬요.',
  },

  changes: {
    title: '모든 버전 (최신순)',
    intro: '버전은 한 번 저장하면 바뀌지 않아요. 버전마다 무엇을 바꿨는지 한 줄로 적혀 있어요.',
    agent: '에이전트 작성',
    online: '온라인',
    browse: '코드 읽기',
    noChange: '메모 없음',
  },

  licenses: {
    MIT: '라이선스와 저작권 고지를 함께 두기만 하면, 누구나 어디에든 쓰고, 고치고, 배포할 수 있어요.',
    'Apache-2.0': 'MIT와 같고, 기여한 모든 사람에게서 특허 사용 허가도 받아요. 바꾼 부분에는 바꿨다고 표시해야 해요.',
    'BSD-3-Clause': 'MIT와 같고, 이걸로 만든 것을 홍보할 때 저작자의 이름을 쓸 수 없어요.',
    'MPL-2.0': '이 파일들을 바꾼 내용은 같은 라이선스를 유지해야 해요. 다른 어떤 라이선스의 코드와도 함께 쓸 수 있어요.',
    'GPL-3.0-or-later': '바꿨든 안 바꿨든, 배포하는 사람은 소스 코드도 같은 라이선스로 함께 배포해야 해요.',
    'AGPL-3.0-or-later': 'GPL과 같고, 바꾼 사본을 네트워크로 사람들에게 제공하는 것도 배포로 봐요.',
    Unlicense: '퍼블릭 도메인으로 공개돼 있어요. 누구나 아무 조건 없이 무엇이든 할 수 있어요.',
  },

  kinds: {
    Permissive: '허용형',
    Copyleft: '카피레프트',
    PublicDomain: '퍼블릭 도메인',
  },
};
