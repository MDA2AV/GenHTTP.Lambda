import type { EditorMessages } from '../../en/editor';

export const files: EditorMessages['files'] = {
  version: '버전',
  shown: (version, online, newest) => `버전 ${version}${online ? ', 온라인' : newest ? ', 최신' : ''}`,
  optionOnline: ' (온라인)',
  count: (files) => `파일 ${files}개`,
  usage: (files, used, of) => `${files}, ${of} 중 ${used}`,
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
