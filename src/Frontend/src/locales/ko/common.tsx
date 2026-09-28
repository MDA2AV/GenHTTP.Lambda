import type { Messages } from '../en';

export const shell: Messages['shell'] = {
  main: '주 메뉴',
  build: '웹사이트 만들기',
  ship: '배포하기',
  showcase: '쇼케이스',
  enterprise: '엔터프라이즈',
  docs: '문서',
  admin: '관리자',
  lightMode: '라이트 모드로 전환',
  darkMode: '다크 모드로 전환',
  openMenu: '메뉴 열기',
  closeMenu: '메뉴 닫기',
  language: '언어',
  terms: '이용약관',
  privacy: '개인정보 처리방침',
  imprint: '법적 고지',
  writeCode: '코드 직접 작성하기',
  contact: '문의',
};

export const common: Messages['common'] = {
  loading: '불러오는 중…',
  loadingEditor: '에디터를 불러오는 중…',
  editorFailed: '에디터를 불러오지 못했어요',
  editorFailedWhy: '보통 이 탭을 열어 둔 사이에 사이트가 업데이트돼서 그래요.',
  reload: '새로고침',
  backToStart: '처음으로',
  tryAgain: '다시 시도',
  copy: '복사',
  copied: '복사됨',
  copyToClipboard: '클립보드에 복사',
  openInNewTab: '새 탭에서 열기',
  close: '닫기',
  operatorCountry: '독일',
};

export const notFound: Messages['notFound'] = {
  title: '페이지를 찾을 수 없어요',
  heading: '없는 페이지예요',
  text: '링크가 오래됐거나, 링크가 가리키던 람다가 삭제됐을 수 있어요.',
};

export const missing: Messages['missing'] = {
  title: '여기서 실행 중인 게 없어요',
  heading: '여기서 실행 중인 게 없어요',
  notDeployed: (key) => (
    <>
      {key}에 람다가 있지만, 지금은 배포되어 있지 않아요. 무료 플랜에서는 쓰는 동안 계속 온라인이고, 한 달 동안 방문도
      수정도 없으면 내려가요. 에디터 링크가 있는 사람은 누구나 다시 온라인에 올릴 수 있어요.
    </>
  ),
  unknown: (key) => (
    <>
      {key}에는 람다가 없어요. 처음부터 없던 키이거나, 연결된 람다가 삭제됐을 수 있어요.
    </>
  ),
  create: '이 주소로 람다 만들기',
};

export const abuse: Messages['abuse'] = {
  report: '신고하기',
  title: '람다 신고하기',
  write: '메일 보내기',
  subject: '악용 신고',
  intro:
    '여기서는 누구나 코드를 올릴 수 있어요. 그래서 가끔은 올라오면 안 되는 것도 올라와요. 여기서 호스팅되는 페이지가 사람들을 속이거나, 무언가를 공격하거나, 권리가 없는 자료를 쓰고 있다면 알려 주세요. 저희가 내릴게요.',
  how: (mailbox, strong, path) => (
    <>
      {mailbox} 주소로 메일을 보내 주세요. {strong('페이지 주소')}와 무엇이 문제인지 한 문장만 적어 주시면 돼요. 페이지
      주소는 {path} 형태예요. 스크린샷이 있으면 도움이 돼요. 계정이 없어도, 이 사이트를 쓰지 않아도 괜찮아요.
    </>
  ),
  next: (strong, terms) => (
    <>
      {strong('신고하면 이렇게 돼요.')} 사람이 직접 읽어요. {terms('이용약관')}을 어긴 람다라면 보통 하루 안에 내려요.
      누가 올렸는지는 알려 드릴 수 없고, 모든 신고에 답장을 드리지는 못해요. 그래도 신고는 하나도 빠짐없이 읽어요.
    </>
  ),
  danger:
    '누군가 당장 위험에 처했거나 범죄가 벌어지고 있다면, 경찰 등 관계 기관에도 꼭 신고해 주세요. 저희는 페이지를 내릴 수 있을 뿐, 그 이상은 할 수 없어요.',
};
