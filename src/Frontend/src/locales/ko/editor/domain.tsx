import type { EditorMessages } from '../../en/editor';

export const domain: EditorMessages['domain'] = {
  readFailed: '도메인을 읽지 못했어요.',
  reaching: (domain) => `이제 ${domain} 도메인으로 오는 요청이 이 람다로 연결돼요.`,
  saveFailed: '도메인을 저장하지 못했어요.',
  removed: '도메인을 삭제했어요. 이제 람다는 여기 주소에서 다시 응답해요.',
  removeFailed: '도메인을 삭제하지 못했어요.',
  hint:
    '프리미엄 람다는 전용 도메인에서도 응답할 수 있어요. 루트부터 도메인 전체를 쓸 수 있어요. 먼저 도메인이 이 서버를 가리키게 한 다음 여기에 입력하세요. 그때부터 그 도메인으로 오는 요청이 람다로 연결되고, 여기 주소는 방문자를 그 도메인으로 안내해요.',
  loading: '불러오는 중…',
  example: 'your-domain.com',
  open: (domain) => `${domain} 열기`,
  label: '람다가 응답할 도메인',
  serving: (domain) => <>지금 {domain} 도메인에서 응답하고 있어요. 여기 주소는 방문자를 그 도메인으로 안내해요.</>,
  none: '아직 없어요. shop.example.com 같은 하위 도메인이나 example.com 같은 전체 도메인을 쓸 수 있어요.',
  change: '변경',
  use: '이 도메인 사용',
  remove: '삭제',
  confirm: '도메인을 삭제할까요?',
  keep: '그대로 두기',
  confirmText: (domain) => (
    <>
      지금부터 {domain} 도메인으로 오는 요청은 이 람다에 닿지 않아요. 대신 여기 주소가 방문자를 안내하지 않고 다시 직접
      응답해요. 도메인의 DNS 설정은 바뀌지 않아요.
    </>
  ),
  point: '도메인이 이 서버를 가리키게 하기',
  check: '다시 확인',
  records:
    '도메인의 DNS를 관리하는 곳에서 아래 두 레코드를 추가하세요. IPv6로 접속되지 않길 원하면 AAAA 레코드는 빼도 돼요.',
  type: '유형',
  name: '이름',
  value: '값',
  pointsHere: (domain) => <>{domain} 도메인이 이 서버를 가리키고 있어요.</>,
  alsoElsewhere: (addresses) =>
    ` 하지만 이 서버가 아닌 ${addresses} 주소로도 연결돼요. 그쪽으로 간 방문자는 람다에 닿지 못해요.`,
  elsewhere: (addresses) => `지금은 ${addresses} 주소로 연결돼요. 아직 이 서버가 아니에요.`,
  wait: '변경 사항이 모든 곳에 반영되기까지 시간이 걸릴 수 있어요. 최대 이전 레코드의 TTL만큼 걸려요.',
  cname: 'CNAME 레코드를 대신 쓰는 방법',
  cnameText: (target) => (
    <>
      하위 도메인은 CNAME 레코드로 {target} 주소를 가리킬 수도 있어요. 그러면 이 서버의 주소가 바뀌어도 알아서
      따라가요. 단점도 있어요.
    </>
  ),
  cnameRoot: (example) => (
    <>
      루트 도메인({example} 자체)에는 쓸 수 없어요. 모든 도메인이 루트에 갖고 있는 레코드 옆에는 CNAME을 둘 수 없다는
      게 표준이에요. 일부 업체는 대신 쓸 수 있는 ALIAS, ANAME 또는 ‘flattened’ 레코드를 제공해요.
    </>
  ),
  cnameAlone: '같은 이름에 다른 레코드를 둘 수 없어요. 메일용 MX 레코드도, 인증용 TXT 레코드도요.',
  cnameLookup: '방문자가 접속하기 전에 리졸버가 조회를 한 번 더 거쳐요.',
  copy: '복사',
  copyValue: (value) => `${value} 복사`,
};
