import type { EditorMessages } from '../../en/editor';

export const files: EditorMessages['files'] = {
  hint: (b) => (
    <>
      한 버전의 파일, 즉 프로그램이에요. {b('코드')}는 컴파일되고, 그대로 제공되지 않아요. {b('에셋')}(페이지,
      스크립트, 스타일, 이미지)은 코드와 함께 저장되고 함께 배포되고 롤백되며, 코드가 제공하면 공개돼요. 람다가 실행
      중에 보관하는 건 여기 없어요. 그건 {b('데이터')}에 있어요.
    </>
  ),
  scope: (version, data) => (
    <>
      이 파일들은 버전 {version}에 속하고, 버전과 함께 바뀌어요. 람다가 실행 중에 보관하는 건 모든 버전에서 똑같고,{' '}
      {data('데이터')}에 있어요.
    </>
  ),
  edit: '이 버전 편집',
  version: '버전',
  shown: (version, online, newest) => `버전 ${version}${online ? ', 온라인' : newest ? ', 최신' : ''}`,
  optionOnline: ' (온라인)',
  readFailed: '그 버전을 읽지 못했어요.',
  noVersion: '아직 보여 줄 버전이 없어요.',
  label: '파일',
  code: '코드',
  codeWhy: '람다로 컴파일되고, 그대로 제공되지 않아요.',
  count: (files) => `파일 ${files}개`,
  codeUsage: (files, used, of) => `${files}, ${of}자 중 ${used}자`,
  usage: (files, used, of) => `${files}, ${of} 중 ${used}`,
  noCode: '이 버전에는 코드가 없어요.',
  assets: '에셋',
  assetsPublic: '공개: 이 버전이 Assets로 제공해요.',
  assetsPrivate: '코드와 함께 저장되지만, 이 버전은 제공하지 않아요.',
  noAssets: '이 버전에는 없어요.',
  context: '문서와 테스트',
  contextWhy: '컴파일되지도, 제공되지도 않아요. 이 버전에 대해 적어 둔 것으로, 이 버전을 읽거나 고칠 사람을 위한 거예요.',
  contextUsage: (files, size) => `${files}, ${size} (에셋 용량에 포함)`,
  noContext: '이 버전에 대해 아직 작성된 게 없어요.',
  build: '빌드',
  buildWhy: '컴파일되지도 제공되지도 않아요. 코드나 에셋을 수정하는 쪽이 그것을 만들 때 쓰는 원본이에요.',
  noBuild: '없음 - 빌드 도구가 코드나 에셋을 만드는 경우, 그 원본이 여기에 보관돼요.',
  data: '데이터',
  dataPublic: '공개: 온라인 코드가 Workspace로 제공해요.',
  dataPrivate: '람다만 볼 수 있어요. 어떤 버전에도 속하지 않아요.',
  uploadFailed: (path) => `${path} 파일을 업로드하지 못했어요.`,
  deleteFolder: (path, held) =>
    held > 0 ? `${path} 폴더와 그 안의 파일 ${held}개를 삭제할까요?` : `${path} 폴더를 삭제할까요?`,
  deleteFile: (path) => `${path} 파일을 삭제할까요? 람다가 더는 이 파일을 찾을 수 없어요.`,
  deleteFailed: '삭제하지 못했어요.',
  full: '데이터 공간이 가득 찼어요',
  uploadInto: (folder) => `${folder} 폴더에 업로드`,
  upload: '업로드',
  reading: '읽는 중…',
  noData: '아직 없어요. 람다가 실행 중에 저장하는 것이 여기에 나와요.',
  delete: (path) => `${path} 삭제`,
  deleteShort: '삭제',
  fileFailed: '파일을 읽지 못했어요.',
  pick: '파일을 고르면 내용을 볼 수 있어요.',
  tooLarge: (name, size) => (
    <>
      {name} 파일({size})은 너무 커서 여기서 보여 줄 수 없어요.
    </>
  ),
  download: '다운로드',
  readingFile: (name) => `${name} 읽는 중…`,
  missing: (name) => `이 버전에는 ${name} 파일이 없어요.`,
  saved: '저장됨',
  notText: '텍스트가 아니에요. 다운로드해서 확인하세요.',
};
