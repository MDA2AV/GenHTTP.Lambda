import type { EditorMessages } from '../../en/editor';

export const build: EditorMessages['build'] = {
  title: '빌드',
  hint: '버전의 에셋이나 코드를 만드는 원본이에요. 앱을 수정하는 쪽(클론에서 작업하는 에이전트)이 빌드 도구를 실행하는 파일이며, 버전마다 함께 보관되고 컴파일되거나 제공되지 않아요. 이 플랫폼은 아무것도 빌드하지 않으므로 여기서는 읽기만 하고 편집하지 않아요.',
  overview: '개요',
  files: '파일',
  scope: (version) =>
    `버전 ${version}의 빌드 원본이에요. 버전과 함께 보관되고 컴파일되거나 제공되지 않으며, 수정하는 쪽이 빌드하고 여기서는 빌드하지 않아요.`,
  scopeDraft: '이 초안의 빌드 원본이에요. 초안과 함께 보관되고 컴파일되거나 제공되지 않으며, 수정하는 쪽이 빌드하고 여기서는 빌드하지 않아요.',
  reading: '빌드 원본을 읽는 중…',
  readFailed: '빌드 원본을 읽지 못했어요.',

  emptyTitle: (version) => `버전 ${version}에는 빌드 원본이 없어요`,
  emptyTitleDraft: '이 초안에는 빌드 원본이 없어요',
  emptyText: (code) => (
    <>
      빌드 도구가 에셋이나 코드를 만드는 경우(컴파일, 번들, 생성), 그 원본 파일을 버전마다 여기에 보관해요. 클론에서는{' '}
      {code('dev/')} 폴더예요. 앱을 수정하는 쪽이 작업하는 곳에서 빌드하고 둘을 함께 저장해요. 이 플랫폼은 아무것도
      빌드하지 않아요. 제공되거나 컴파일되는 그대로 작성된 것에는 필요 없어요.
    </>
  ),
  emptyHow: (code) => (
    <>클론의 {code('AGENTS.md')}가 코딩 에이전트에게 사용 방법을 알려 줘요.</>
  ),

  inVersion: (version) => `버전 ${version}에서`,
  inDraft: '이 초안에서',
  comparedWith: (version) => `버전 ${version}과 비교`,
  first: '이를 보관하는 첫 번째 버전이에요.',
  both: (here, program) =>
    `여기서 ${here}개 파일이 바뀌었고, 코드와 에셋에서는 ${program}개 파일이 바뀌었어요.`,
  hereOnly: (here) =>
    `여기서 ${here}개 파일이 바뀌었지만 코드와 에셋은 바뀌지 않았어요. 바뀐 내용이 그 안에 빌드되는 것이라면 빌드되지 않은 거예요.`,
  programOnly: '여기서는 바뀐 게 없어요.',
  unchanged: '여기서도, 코드와 에셋에서도 바뀐 게 없어요.',
  showChanges: '변경 사항 보기',
  hideChanges: '변경 사항 숨기기',
  noChanges: '여기서는 바뀐 게 없어요.',

  readme: '빌드 방법',
  noReadme: (code) => (
    <>
      빌드 방법을 알려 주는 게 없어요. 명령과 빌드 결과가 놓이는 곳을 적은 {code('README.md')}를 맨 위에 두면 다음
      에이전트가 그것을 보고 빌드해요.
    </>
  ),
  readOnly: '읽기 전용이에요. 빌드하는 곳에서 수정해요.',
  noFiles: '파일이 없어요.',
};
