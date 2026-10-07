import type { EditorMessages } from '../../en/editor';

export const tabs: EditorMessages['tabs'] = {
  codeName: '맨 위의 C# 파일: 영문자, 숫자, 하이픈, 밑줄만 쓸 수 있고, 영문자로 시작해서 .cs로 끝나야 하며 40자까지예요.',
  name: '영문자, 숫자, - _ . + @ ( ) [ ] { } $ ~만 쓸 수 있어요. 폴더는 슬래시로 구분하고, 공백이나 점으로 끝나는 이름은 안 돼요.',
  taken: '내보내거나 클론한 람다의 맨 위에는 그 이름의 파일이나 폴더가 이미 있어요. 폴더 안에 넣거나 다른 이름을 쓰세요.',
  lambda: '람다는 더 이상 .lambda/ 폴더를 두지 않아요. 문서는 docs/에, 테스트는 tests/에 두세요.',
  assets: '람다가 제공하는 것은 이제 리소스에 있어요. 거기에 추가하세요.',
  resourceName: '영문자, 숫자, 하이픈, 밑줄, 점만 쓸 수 있고 슬래시로 구분하며, 폴더는 최대 6단계까지예요. 올바른 형식으로 제공되도록 확장자도 필요해요.',
  exists: '같은 이름의 파일이 이미 있어요.',
  remove: (name) => `${name} 파일을 삭제할까요? 내용도 함께 사라져요.`,
  removeFolder: (name, files) => `${name} 폴더와 그 안의 파일 ${files}개를 삭제할까요?`,
  there: (name) => `${name} 파일이 이미 있어요.`,
  entry: '스니펫: 반환하는 것이 그대로 제공돼요',
  errors: '오류 있음',
  removeFile: (name) => `${name} 삭제`,
  removeTitle: '삭제',
  codePlaceholder: 'Store.cs, docs/notes.md 또는 frontend/app.ts',
  resourcePlaceholder: 'web/index.html',
  upload: '파일 업로드',
};
