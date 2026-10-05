import type { EditorMessages } from '../../en/editor';

export const versions: EditorMessages['versions'] = {
  hint: (limit) =>
    `버전은 프로그램, 즉 코드와 에셋이에요. 한 번 저장하면 바뀌지 않아서, 어느 버전이든 비교할 수 있고 저장했던 그대로 다시 온라인에 올릴 수 있어요. 버전마다 무엇을 요청했고 무엇이 바뀌었는지 남아 있어요. 람다를 바꾸려면 초안을 만드세요. 제대로 되면 다음 버전이 돼요. 버전이 ${limit}개를 넘으면 가장 오래된 것부터 삭제되지만, 온라인 버전은 삭제되지 않아요.`,
  none: '아직 버전이 없어요.',
  noDescription: '설명 없음',
  online: '온라인',
  putOnline: '이 버전을 온라인에 올리기',
  rollBackTitle: '이전 버전을 다시 온라인에 올리기',
  deploy: '배포',
  rollBack: '롤백',
  readFailed: '이 버전을 읽지 못했어요.',
  comparing: '비교하는 중…',
  unchanged: '이전 버전과 달라진 게 없어요.',
  first: '첫 번째 버전이에요.',
  status: { added: '추가됨', removed: '삭제됨', changed: '변경됨', same: '같음' },
  groups: {
    code: '코드',
    assets: '에셋',
    build: '빌드',
    context: '문서와 테스트',
  },
  browse: '파일 둘러보기',
  docs: '문서 읽기',
  build: '빌드 원본 보기',
  edit: '여기서부터 편집',
  feature: '여기서 초안 만들기',
  featureTitle: '이 버전을 람다와 따로 고쳐 보고, 제대로 되면 확정해서 다음 버전으로 만들기',
  binary: '텍스트가 아니라서 줄 단위로 비교할 수 없어요.',
  tooLarge: '너무 커서 줄 단위로 비교할 수 없어요.',
};
