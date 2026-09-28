import type { Messages } from '../en';

export const create: Messages['create'] = {
  title: '람다 만들기',
  whatTitle: '무엇을 만들고 싶나요?',
  whatText:
    '가장 비슷한 걸 고르면, 이미 잘 돌아가는 앱의 사본으로 시작해요. 마음대로 고쳐도 돼요. 빈 상태에서 시작해도 되고요.',
  seeIt: '실행 중인 모습 보기',
  startFrom: '이걸로 시작하기',
  starters: {
    'demo-crud': {
      title: '목록 관리하기',
      description: '여럿이 항목을 추가하고, 고치고, 체크하는 목록. 할 일, 메모, 북마크, 작은 재고 관리까지.',
    },
    'demo-registration': {
      title: '회원가입 받기',
      description: '사람들이 가입하고 로그인하는 계정, 그리고 본인만 볼 수 있는 페이지.',
    },
    'demo-game': {
      title: '같이 하는 게임',
      description: '여러 사람이 각자 브라우저에서 동시에, 실시간으로 즐기는 게임.',
    },
    'demo-files': {
      title: '파일·사진 공유',
      description: '누군가 사진이나 문서를 올리면 다른 사람들도 모두 볼 수 있는 앱.',
    },
    'demo-live': {
      title: '실시간으로 보여 주기',
      description: '무언가 바뀌는 순간 저절로 업데이트되는 페이지. 투표, 점수, 대시보드 등.',
    },
    empty: {
      title: '처음부터 만들기',
      description: '빈 람다에서 시작해서 원하는 걸 자유롭게 만드세요.',
    },
  },

  addressTitle: '주소 정하기',
  fromDemo: (title) => (
    <>{title}: 데모의 사본으로 람다를 시작해요. 안에 있는 건 모두 마음대로 고칠 수 있어요.</>
  ),
  fromNothing: '빈 람다로 시작해요. 원하는 건 무엇이든 만들 수 있어요.',
  pickAgain: '다시 고르기',
  publicKey: '공개 키',
  free: (key) => `‘${key}’ 주소는 쓸 수 있어요.`,
  keyHint: '영문 소문자, 숫자, 하이픈만 쓸 수 있고 3자 이상이어야 해요. 비워 두면 무작위로 정해져요.',
  accept: '이용약관에 동의해요',
  fullTerms: '이용약관 전문 보기',
  back: '뒤로',
  creating: '만드는 중…',
  submit: '람다 만들기',
  keepLink: '다음 화면에 에디터 링크가 나와요. 다시 들어올 수 있는 유일한 방법이니 꼭 보관하세요.',
  failed: '람다를 만들지 못했어요.',
};
