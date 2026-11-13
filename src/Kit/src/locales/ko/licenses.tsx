import type { KitMessages } from '../en';

export const licenses: KitMessages['licenses'] = {
  MIT: '라이선스와 저작권 고지를 함께 두기만 하면, 누구나 어디에든 쓰고, 고치고, 배포할 수 있어요.',
  'Apache-2.0': 'MIT와 같고, 기여한 모든 사람에게서 특허 사용 허가도 받아요. 바꾼 부분에는 바꿨다고 표시해야 해요.',
  'BSD-3-Clause': 'MIT와 같고, 이걸로 만든 것을 홍보할 때 저작자의 이름을 쓸 수 없어요.',
  'MPL-2.0': '이 파일들을 바꾼 내용은 같은 라이선스를 유지해야 해요. 다른 어떤 라이선스의 코드와도 함께 쓸 수 있어요.',
  'GPL-3.0-or-later': '바꿨든 안 바꿨든, 배포하는 사람은 소스 코드도 같은 라이선스로 함께 배포해야 해요.',
  'AGPL-3.0-or-later': 'GPL과 같고, 바꾼 사본을 네트워크로 사람들에게 제공하는 것도 배포로 봐요.',
  Unlicense: '퍼블릭 도메인으로 공개돼 있어요. 누구나 아무 조건 없이 무엇이든 할 수 있어요.',
};

export const kinds: KitMessages['kinds'] = {
  Permissive: '허용형',
  Copyleft: '카피레프트',
  PublicDomain: '퍼블릭 도메인',
};
