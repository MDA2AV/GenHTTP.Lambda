import type { Messages } from '../en';

export const imprint: Messages['imprint'] = {
  title: '법적 고지',
  intro:
    '이 사이트를 누가 운영하는지 알려 드려요. 독일 법(§ 5 DDG)은 독일에서 운영하는 모든 사이트가 이런 정보를 찾기 쉬운 한곳에 밝히도록 하고 있어요. 이 페이지가 바로 그곳이에요.',
  sections: {
    providerTitle: '서비스 제공자',
    contactTitle: '문의',
    contact: (mail, abuse) => (
      <>
        이메일: {mail}. 피해를 주는 람다를 신고하려면 {abuse}로 메일을 보내 주세요.
      </>
    ),
    editorialTitle: '콘텐츠 책임자',
    editorial:
      '독일 법(MStV 제18조 제2항)에 따라 이 사이트의 페이지를 책임지는 사람이에요. 여기에 호스팅된 람다는 각 소유자가 직접 작성한 것이라 해당하지 않아요.',
    dsaTitle: '디지털 서비스법(DSA)에 따른 연락 창구',
    dsa: (mail) => (
      <>
        당국, 유럽연합 집행위원회, 그리고 이 서비스를 쓰는 누구나 {mail}로 독일어나 영어로 연락할 수 있어요(DSA 제11조·제12조).
      </>
    ),
  },
};
