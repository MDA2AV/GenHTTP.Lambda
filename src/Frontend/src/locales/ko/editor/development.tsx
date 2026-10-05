import type { EditorMessages } from '../../en/editor';

export const development: EditorMessages['development'] = {
  title: '개발 공간',
  hint: '도구 체인으로 에셋을 빌드하는 경우, 버전의 에셋을 만드는 원본이에요. 프론트엔드의 프로젝트와 소스, 설정, 잠금 파일이 들어 있어요. 버전마다 함께 보관되며 컴파일되거나 제공되지 않아요. 이를 수정하는 쪽(클론에서 작업하는 에이전트)이 작업하는 곳에서 빌드하고, 빌드 결과와 함께 저장해요. 이 플랫폼은 아무것도 빌드하지 않아요. 그래서 여기서는 읽기만 할 수 있고 편집할 수 없어요.',
  overview: '개요',
  files: '파일',
  scope: (version) =>
    `버전 ${version}의 에셋을 만드는 원본이에요. 버전과 함께 보관되며, 컴파일되거나 제공되지 않고, 수정하는 쪽이 빌드하며 여기서는 빌드하지 않아요.`,
  scopeDraft: '이 초안의 에셋을 만드는 원본이에요. 초안과 함께 보관되며, 컴파일되거나 제공되지 않고, 수정하는 쪽이 빌드하며 여기서는 빌드하지 않아요.',
  reading: '개발 공간을 읽는 중…',
  readFailed: '개발 공간을 읽을 수 없어요.',

  emptyTitle: (version) => `버전 ${version}에는 개발 공간이 없어요`,
  emptyTitleDraft: '이 초안에는 개발 공간이 없어요',
  emptyText: (code) => (
    <>
      Vite와 함께 React, Vue, Svelte, TypeScript, Tailwind 같은 도구 체인으로 프론트엔드를 빌드하는 경우, 그
      프로젝트(에셋을 만드는 원본)가 버전마다 여기에 보관돼요. 에이전트가 작업하는 곳에서 빌드하고 소스를 빌드
      결과와 함께 저장해요. 클론에서는 {code('dev/')} 폴더예요. 일반 HTML, CSS, JavaScript로 만든 프론트엔드에는
      필요 없어요.
    </>
  ),
  emptyHow: (code) => (
    <>클론의 {code('AGENTS.md')}가 코딩 에이전트에게 설정 방법을 알려 줘요.</>
  ),

  projects: '프로젝트',
  atTheTop: '개발 공간 자체',
  kinds: {
    npm: 'npm',
    deno: 'Deno',
    cargo: 'Rust',
    go: 'Go',
    python: 'Python',
    dotnet: '.NET',
    php: 'PHP',
    ruby: 'Ruby',
    maven: 'Maven',
    gradle: 'Gradle',
    make: 'Make',
  },
  builtWith: '빌드 도구',
  build: '빌드',
  noBuild: 'package.json에 빌드 스크립트가 없어요.',
  into: '빌드 결과 위치',
  intoAssets: (folder, files, size) => (
    <>
      에셋의 {folder} - 이 버전에서 {files === 1 ? '파일 1개' : `파일 ${files}개`}, {size}
    </>
  ),
  intoNothing: (folder) => <>에셋의 {folder} - 이 버전에서는 비어 있어요</>,
  packages: '패키지',
  packagesCount: (runtime, tooling) =>
    `실행용 ${runtime}개, 빌드용 ${tooling}개`,
  showPackages: '보기',
  hidePackages: '숨기기',
  runtime: '실행용',
  tooling: '빌드용',
  missing: (page, files) => (
    <>
      {page}: 에셋에 없는 파일 {files.length}개를 참조해요 ({files.slice(0, 3).join(', ')}{files.length > 3 ? ', …' : ''}).
      빌드가 쓴 내용이 전부 저장되지 않아서 페이지가 로드되지 않아요.
    </>
  ),
  noLock: '잠금 파일이 없어요. 다음 빌드에서 지난번과 다른 버전의 패키지가 설치될 수 있어요.',
  noIgnore: '.gitignore가 없어요. 도구 체인이 설치하고 빌드한 것이 버전에 들어갈 수 있어요.',

  inVersion: (version) => `버전 ${version}에서`,
  inDraft: '이 초안에서',
  comparedWith: (version) => `버전 ${version}과 비교`,
  first: '이것을 가진 첫 번째 버전이에요.',
  both: (here, assets) =>
    `여기서 파일 ${here}개, 에셋에서 파일 ${assets}개가 바뀌었어요.`,
  hereOnly: (here) =>
    `여기서 파일 ${here}개가 바뀌었고, 에셋은 바뀌지 않았어요. 빌드가 필요 없는 변경이 아니라면 방문자에게는 이전과 같은 것이 보여요.`,
  builtOnly: (folder) => (
    <>{folder}로 빌드되는 내용은 바뀌었지만 여기는 바뀐 게 없어요. 빌드 결과를 직접 고친 내용은 다음 빌드에서 사라져요.</>
  ),
  assetsOnly: '여기서는 바뀐 게 없어요.',
  unchanged: '여기도 에셋도 바뀐 게 없어요.',
  showChanges: '변경 내용 보기',
  hideChanges: '변경 내용 숨기기',
  noChanges: '여기서는 바뀐 게 없어요.',

  readme: '빌드 방법',
  noReadme: (code) => (
    <>
      빌드 방법이 적혀 있지 않아요. 개발 공간 맨 위의 {code('README.md')}에 명령어와 빌드 결과가 가는 곳을 적어 두면
      다음 에이전트가 그것을 보고 빌드해요.
    </>
  ),
  readOnly: '읽기 전용이에요. 빌드하는 곳에서 수정해요.',
  noFiles: '파일이 없어요.',
};
