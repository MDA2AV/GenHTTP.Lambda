import type { Messages } from '../en';

export const connect: Messages['connect'] = {
  address: 'エージェント用のアドレス',
  editorLink: '編集用リンク（エージェント以外には教えないでください）',
  yourAgent: '使っているエージェント',
  terminal: 'ターミナル',
  setups: {
    claudeCode: 'ターミナルで一度だけ実行してください。以降はどのプロジェクトからでも、Claude Codeでここのアプリを作成・変更できます。',
    claude: (strong) => (
      <>
        Web版またはデスクトップ版のClaudeで{strong('設定')}を開き、{strong('Connectors')}から{strong('Add custom connector')}
        を選びます。上のURLを貼り付けて保存すれば完了です。
      </>
    ),
    cursor: 'これをCursorのMCP設定か、下のファイルに追加して再読み込みします。',
    vscode: 'これをプロジェクトに保存し、Copilot ChatのMCPビューからサーバーを起動します。',
  },
  elsewhere:
    'ほかのツールを使っていますか？　Windsurf、Codex、Zedなど、たいていのエージェントは設定からリモートMCPサーバーを追加できます。上のURLを設定してください。',
};
