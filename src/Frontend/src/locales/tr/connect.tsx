import type { Messages } from '../en';

export const connect: Messages['connect'] = {
  address: 'Ajanınız için adres',
  editorLink: 'Editör linki: yalnızca ajanınız için, başka kimseyle paylaşmayın',
  yourAgent: 'Ajanınız',
  terminal: 'Terminal',
  setups: {
    claudeCode: 'Bunu bir kez terminalde çalıştırın. Bundan sonra Claude Code, herhangi bir projeden burada uygulama oluşturup değiştirebilir.',
    claude: (strong) => (
      <>
        Claude’un web ya da masaüstü uygulamasında {strong('Ayarlar')} bölümünü açın, ardından {strong('Connectors')}{' '}
        bölümüne gidin ve {strong('Add custom connector')} seçeneğine tıklayın. Yukarıdaki adresi yapıştırıp kaydedin.
        Hepsi bu.
      </>
    ),
    cursor: 'Bunu Cursor’ın MCP ayarlarına ya da aşağıdaki dosyaya ekleyin, sonra yeniden yükleyin.',
    vscode: 'Bunu projenize kaydedin, sonra sunucuyu Copilot Chat’in MCP görünümünden başlatın.',
  },
  elsewhere:
    'Başka bir şey mi kullanıyorsunuz? Windsurf, Codex, Zed ve diğer ajanların çoğu, ayarlarından uzak bir MCP sunucusu ekleyebilir. Onlara yukarıdaki adresi verin.',
};
