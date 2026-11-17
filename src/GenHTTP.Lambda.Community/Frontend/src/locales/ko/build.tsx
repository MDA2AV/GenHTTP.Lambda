import type { Messages } from '../en';

export const build: Messages['build'] = {
  title: '말로 설명해서 앱 만들기',
  intro:
    '이 설치본의 빌드 에이전트에게 필요한 것을 말해 보세요. 에이전트가 앱을 만들어 여기에 온라인으로 올리고 내 목록에 남겨 둬요. 나중에 같은 방식으로 앱에 대해 묻거나 수정할 수 있어요.',
  /** Under the box: the two other ways to an app, each a link around its words. */
  other: (ship, create) => (
    <>
      아니면 {ship('내 에이전트를 연결')}하거나, {create('직접 코드를 작성')}해도 돼요.
    </>
  ),
};
