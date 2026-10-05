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
  demo: '데모라서 모두 읽기 전용이에요. 고치려면 이 데모로 내 람다를 만드세요. ',
  edit: '코드를 직접 편집하세요. 저장하면 새 버전이 생기고, 온라인 버전은 그대로예요. 배포하면 온라인에 올라가요. 먼저 써 보고 싶다면 초안을 만드세요. ',
  editFeature:
    '이 초안의 코드예요. 저장해도 초안에만 남고, 람다의 방문자가 받는 건 하나도 바뀌지 않아요. 배포하면 초안 전용 주소에서 온라인에 올라가 써 볼 수 있어요. 초안을 확정하면 다음 버전이 돼요. ',
  inFeature: (name) => `‘${name}’ 초안`,
  changedElsewhere: '이 초안은 연 뒤에 다른 곳에서 저장됐어요. 아마 에이전트일 거예요. 여기서 저장하기 전에 저장된 내용을 불러오세요. 여기서 바꾼 내용은 그 위에 저장되지 않아요.',
  readAgain: '저장된 내용 불러오기',
  files: (entry, cs, context) => (
    <>
      {entry} 파일이 반환하는 것이 제공되고, 다른 {cs} 파일에는 타입을 두고, 그 밖의 파일은 그대로 제공돼요. 단,{' '}
      {context} 안에 있는 문서, 테스트, 개발 공간은 컴파일되지도 제공되지도 않아요. Ctrl-S로 저장하고, F12로 선언으로
      이동해요.
    </>
  ),
  newer: (version) => ` 여기 열린 것보다 새로운 버전이 있어요 (버전 ${version}).`,
  built: (folder) =>
    `개발 공간의 빌드가 ${folder}에 파일을 써요. 다음 빌드가 여기서 바꾼 내용을 대체해요. 대신 그것을 만드는 원본을 수정하세요.`,
  check: '검사',
  save: '저장',
  deploy: '배포',
  deployPreviewTitle: '저장하고, 써 볼 수 있게 초안 전용 주소에서 온라인에 올리기',
  binary: (size) => `텍스트가 아니라서 편집할 수 없어요. 그대로 제공되고, 크기는 ${size} kB예요.`,
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
};
