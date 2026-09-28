import type { Messages } from '../en';

export const connect: Messages['connect'] = {
  address: 'Alamat untuk agen Anda',
  editorLink: 'Link editor: hanya untuk agen Anda, jangan dibagikan ke orang lain',
  yourAgent: 'Agen Anda',
  terminal: 'Terminal',
  setups: {
    claudeCode: 'Jalankan ini sekali di terminal. Setelah itu, Claude Code bisa membuat dan mengubah aplikasi di sini dari proyek mana pun.',
    claude: (strong) => (
      <>
        Di Claude versi web atau desktop, buka {strong('Pengaturan')}, lalu {strong('Connectors')}, dan pilih{' '}
        {strong('Add custom connector')}. Tempel alamat di atas, lalu simpan. Selesai.
      </>
    ),
    cursor: 'Tambahkan ini ke pengaturan MCP di Cursor, atau ke file di bawah, lalu muat ulang.',
    vscode: 'Simpan ini di proyek Anda, lalu jalankan server dari tampilan MCP di Copilot Chat.',
  },
  elsewhere:
    'Pakai tool lain? Windsurf, Codex, Zed, dan kebanyakan agen lain bisa menambahkan server MCP remote di pengaturannya. Berikan alamat di atas.',
};
