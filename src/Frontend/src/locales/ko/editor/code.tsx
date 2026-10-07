import type { EditorMessages } from '../../en/editor';

export const code: EditorMessages['code'] = {
  title: '코드',
  version: (version) => `버전 ${version}`,
  edited: ', 수정됨',
  online: ', 온라인',
  loadFailed: '그 버전을 불러오지 못했어요.',
  compiles: '컴파일돼요.',
  notYet: '아직 컴파일되지 않아요.',
  checkFailed: '코드를 검사하지 못했어요.',
  saved: (version) => `저장했어요 (버전 ${version}).`,
  featureSaved: '초안에 저장했어요. 써 보려면 미리 보기를 배포하세요.',
  featureLoadFailed: '초안을 불러오지 못했어요.',
  previewOnline: '미리 보기가 온라인에 올라갔어요.',
  previewRefused: '미리 보기가 바뀌지 않았어요. 아래에서 컴파일러 메시지를 확인하세요.',
  isOnline: (version) => `버전 ${version}, 이제 온라인이에요.`,
  notOnline: '온라인에 올리지 못했어요. 아래에서 컴파일러 메시지를 확인하세요.',
  failed: '잘 안 됐어요.',
  unchanged: '마지막 저장 이후 바뀐 게 없어요.',
  demo: '데모라서 모두 읽기 전용이에요. 고치려면 이 데모로 내 람다를 만드세요.',
  hint: (b) => (
    <>
      한 버전의 파일이에요. 버전의 {b('코드')}는 프로그램과 함께 보관되는 모든 것이에요. 맨 위의 .cs 파일은
      컴파일되고, 나머지 파일(문서, 테스트, 프론트엔드의 빌드 원본)은 버전과 함께 보관될 뿐 컴파일되거나 제공되지
      않아요. {b('리소스')}(페이지, 스크립트, 스타일, 그림, 데이터베이스 마이그레이션)는 실행 중에 읽고 제공되며, 코드가
      제공하는 것은 공개돼요. 저장하면 새 버전이 만들어지고 온라인 상태는 그대로예요. 먼저 시험해 보려면 초안을
      만드세요. Ctrl-S로 저장하고, F12로 선언으로 이동해요.
    </>
  ),
  hintFeature: (b) => (
    <>
      이 초안의 파일이에요. {b('코드')}(맨 위의 .cs 파일은 컴파일되고 나머지는 함께 보관돼요)와 실행 중에 읽고
      제공되는 {b('리소스')}예요. 저장하면 초안에 보관되고 초안 전용 주소에 반영돼요. 초안을 온라인으로 전환하기
      전까지 방문자에게는 아무것도 보이지 않아요.
    </>
  ),
  inFeature: (name) => `‘${name}’ 초안`,
  changedElsewhere: '이 초안은 연 뒤에 다른 곳에서 저장됐어요. 아마 에이전트일 거예요. 여기서 저장하기 전에 저장된 내용을 불러오세요. 여기서 바꾼 내용은 그 위에 저장되지 않아요.',
  readAgain: '저장된 내용 불러오기',
  newer: (version) => `여기 열린 것보다 새로운 버전이 있어요 (버전 ${version}).`,
  check: '검사',
  save: '저장',
  deploy: '배포',
  deployPreviewTitle: '저장하고, 써 볼 수 있게 초안 전용 주소에서 온라인에 올리기',
  binary: (size) => `텍스트가 아니라서 여기서는 편집할 수 없어요. 크기는 ${size}예요.`,
  saveAndDeploy: '저장하고 배포',
  saveVersion: '새 버전 저장',
  fromOlder: (version, newest) =>
    `버전 ${version}에서 시작한 코드인데, 더 새로운 버전 ${newest}도 있어요. 저장하면 버전 ${version} 이후에 바뀐 내용 없이 이 코드가 최신 버전이 돼요.`,
  featureInstead: (start) => (
    <>
      뭔가 시험해 보는 중인가요? {start('대신 새 초안에 넣으세요')}. 전용 주소가 생기고, 제대로 될 때까지 버전은
      저장되지 않아요.
    </>
  ),
  cancel: '취소',
  what: '무엇이 바뀌나요? 선택 사항이고, 기록에 표시돼요.',
  placeholder: '문의 양식 추가',
  goToDefinition: '정의로 이동',
  versionLabel: '버전',
  shown: (version, online, newest) =>
    `버전 ${version}${online ? ', 온라인' : newest ? ', 최신' : ''}`,
  optionOnline: ' (온라인)',
  switchUnsaved: '여기서 바꾼 내용이 저장되지 않았어요. 그래도 다른 버전을 열까요?',
  noVersion: '아직 보여 줄 버전이 없어요.',
  label: '파일',
  codeGroup: '코드',
  codeWhy: '제공되지 않아요. 맨 위의 .cs 파일은 컴파일되고, 나머지는 버전과 함께 보관돼요.',
  resources: '리소스',
  resourcesPublic: '공개: 이 버전이 Resources로 제공해요.',
  resourcesPrivate: '버전과 함께 들어 있지만, 이 버전은 제공하지 않아요.',
  noResources: '이 버전에는 없어요.',
  count: (files) => (files === 1 ? '파일 1개' : `파일 ${files}개`),
  groupUsage: (files, size) => `${files}, ${size}`,
  usage: (used, of) => `이 버전은 코드와 리소스를 합해 한 버전에 허용되는 ${of} 중 ${used}를 쓰고 있어요.`,
  scope: (data) => (
    <>람다가 실행 중에 보관하는 것은 모든 버전이 똑같이 쓰고, {data('데이터')}에 있어요.</>
  ),
  download: '다운로드',
  newIn: (group) => `${group}에 새 파일`,
  uploadIn: (group) => `${group}에 업로드`,
  pick: '파일을 선택하면 내용을 볼 수 있어요.',
};
