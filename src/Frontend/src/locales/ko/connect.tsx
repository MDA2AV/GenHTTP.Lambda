import type { Messages } from '../en';

export const connect: Messages['connect'] = {
  address: '에이전트에 넣을 주소',
  editorLink: '에디터 링크 (에이전트 말고는 알려 주지 마세요)',
  yourAgent: '내 에이전트',
  terminal: '터미널',
  setups: {
    claudeCode: '터미널에서 한 번만 실행하세요. 그러면 어느 프로젝트에서든 Claude Code로 여기에 앱을 만들고 바꿀 수 있어요.',
    claude: (strong) => (
      <>
        웹이나 데스크톱의 Claude에서 {strong('설정')}을 열고, {strong('Connectors')}에서{' '}
        {strong('Add custom connector')}를 선택하세요. 위 주소를 붙여 넣고 저장하면 끝이에요.
      </>
    ),
    cursor: 'Cursor의 MCP 설정이나 아래 파일에 이 내용을 추가하고 다시 불러오세요.',
    vscode: '프로젝트에 이 파일을 저장한 뒤, Copilot Chat의 MCP 화면에서 서버를 시작하세요.',
  },
  elsewhere:
    '다른 걸 쓰고 있나요? Windsurf, Codex, Zed 등 대부분의 에이전트는 설정에서 원격 MCP 서버를 추가할 수 있어요. 위 주소를 넣어 주세요.',
};
