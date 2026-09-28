import type { Messages } from '../en';

export const connect: Messages['connect'] = {
  address: 'Adres dla twojego agenta',
  editorLink: 'Link do edytora – tylko dla twojego agenta, dla nikogo innego',
  yourAgent: 'Twój agent',
  terminal: 'Terminal',
  setups: {
    claudeCode: 'Uruchom to raz w terminalu. Od tej pory Claude Code może tu tworzyć i zmieniać aplikacje z dowolnego projektu.',
    claude: (strong) => (
      <>
        W Claude (w przeglądarce albo w aplikacji na komputer) otwórz {strong('Ustawienia')}, potem{' '}
        {strong('Connectors')} i wybierz {strong('Add custom connector')}. Wklej adres podany wyżej i zapisz. To
        wszystko.
      </>
    ),
    cursor: 'Dodaj to do ustawień MCP w Cursorze albo do pliku poniżej, a potem przeładuj.',
    vscode: 'Zapisz to w swoim projekcie, a potem uruchom serwer z widoku MCP w Copilot Chat.',
  },
  elsewhere:
    'Używasz czegoś innego? Windsurf, Codex, Zed i większość innych agentów pozwala dodać zdalny serwer MCP w ustawieniach. Podaj im ten sam adres.',
};
