import type { EditorMessages } from '../../en/editor';

export const clone: EditorMessages['clone'] = {
  button: '클론',
  title: 'git으로 클론하기',
  intro:
    '내 도구와 코딩 에이전트로 작업해 보세요. 저장소는 앱이 실행되는 프로젝트 그대로이고, 모든 버전은 main의 커밋, 모든 초안은 브랜치예요.',
  keyWarning: '주소에 에디터 키가 들어 있어요. 이 주소를 가진 사람은 앱을 바꿀 수 있으니, 공유할 때는 빼 주세요.',
  draft: (branch) => <>이 초안은 브랜치 {branch}예요.</>,
  pushing: '푸시하기',
  toMain: (deploy) => <>main에 푸시한 커밋은 다음 버전이 되지만 아직 온라인은 아니에요. 푸시할 때 {deploy}를 붙이면 온라인으로 전환돼요.</>,
  toBranch: '푸시한 브랜치는 초안이 되고, 미리 보기는 전용 주소로 온라인에 올라가요.',
  agents: (file) => <>저장소의 {file}에 코딩 에이전트가 알아야 할 나머지가 적혀 있어요.</>,
  readOnly: '데모는 읽기 전용이에요. 클론해서 읽어 보고, 바꾸려면 데모로 나만의 람다를 시작하세요.',
  copy: '복사',
  copied: '복사됨',
};
