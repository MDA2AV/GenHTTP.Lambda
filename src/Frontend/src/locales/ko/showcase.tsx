import type { Messages } from '../en';

export const showcase: Messages['showcase'] = {
  eyebrow: '쇼케이스',
  title: '여기서 만든 앱, 지금 실행 중',
  intro:
    '만든 사람이 직접 공개한 람다예요. 모두 온라인이라, 카드를 누르면 실제 앱이 열려요. 최근에 쓰인 앱부터 보여 드려요.',
  counted: (total) => (total === 1 ? '람다 1개' : `람다 ${total}개`),
  failed: '쇼케이스를 불러오지 못했어요.',
  loadingMore: '더 불러오는 중…',
  showMore: '더 보기',
  nothingTitle: '아직 올라온 앱이 없어요',
  nothing: (tab) => (
    <>
      잘 돌아가는 앱을 만들었나요? 관리 화면을 열고 {tab('쇼케이스')}를 선택한 다음, 제목과 짧은 설명, 이미지를
      추가하세요. 앱이 온라인인 동안 여기에 나와요.
    </>
  ),
  buildOne: '만들어 보기',
  yoursTitle: '내 앱도 올리고 싶나요?',
  yours: (tab) => (
    <>
      람다의 관리 화면을 열고 {tab('쇼케이스')}를 선택하거나, 람다를 만든 에이전트에게 쇼케이스에 올려 달라고 하세요.
      에디터 키를 가진 사람만 올릴 수 있고, 언제든 다시 내릴 수 있어요.
    </>
  ),
  buildSomething: '만들어 보기',
};

export const card: Messages['card'] = {
  noPicture: '아직 이미지 없음',
  title: '제목',
  description: '방문자가 이 앱으로 할 수 있는 일',
  opens: (title, address) => `${title}, 새 탭에서 ${address} 열기`,
};
