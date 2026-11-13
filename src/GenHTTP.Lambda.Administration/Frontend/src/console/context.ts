/**
 * What every section of the administration is handed by the frame around it.
 *
 * The frame holds the token and asks for it; a section only uses it, and says
 * so when the server turns it away - a token that was right when it was typed
 * can stop being right when the host is restarted with another one.
 */
export interface Access {
  token: string;
  /** The server answered as though there were no administration here. */
  deny: () => void;
  dark: boolean;
  /** Something was decided that the frame counts - a request for connectivity, say - so it asks again. */
  changed?: () => void;
}
