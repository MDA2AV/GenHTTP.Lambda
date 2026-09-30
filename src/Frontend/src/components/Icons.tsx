interface IconProps {
  className?: string;
}

const stroke = {
  fill: 'none',
  stroke: 'currentColor',
  strokeWidth: 1.7,
  strokeLinecap: 'round' as const,
  strokeLinejoin: 'round' as const,
};

function Svg({ className = 'h-4 w-4', children }: IconProps & { children: React.ReactNode }) {
  return (
    <svg viewBox="0 0 24 24" className={className} aria-hidden="true" {...stroke}>
      {children}
    </svg>
  );
}

/**
 * The GenHTTP mark, traced from the icon of the project. The plate is square
 * rather than rounded like the original, because nothing else on these pages
 * has a rounded corner either.
 */
export const IconLogo = ({ className = 'h-6 w-6' }: IconProps) => (
  <svg viewBox="0 0 1024 1024" className={className} aria-hidden="true">
    <rect width="1024" height="1024" fill="#aa55ff" />
    <g transform="translate(0,1024) scale(0.1,-0.1)" fill="#ffffff">
      <path d="M5420 8332 l-25 -20 -2 -364 c-2 -199 -1 -372 2 -384 6 -29 46 -44
114 -44 32 0 80 -7 107 -15 27 -9 113 -28 190 -44 104 -21 152 -35 180 -55 21
-14 44 -26 50 -26 7 0 36 -16 65 -36 30 -20 70 -44 89 -54 19 -10 81 -62 138
-114 69 -65 110 -96 126 -96 24 0 123 73 187 138 20 20 85 80 145 132 59 52
150 134 201 183 51 48 97 87 101 87 5 0 14 11 20 25 15 32 -4 74 -45 100 -15
10 -57 44 -93 75 -36 32 -86 71 -111 86 -50 31 -108 70 -176 116 -24 16 -68
38 -100 49 -32 11 -74 30 -93 43 -19 13 -53 29 -75 36 -22 7 -61 24 -86 37
-25 13 -95 33 -154 44 -60 11 -143 31 -184 44 -43 14 -152 33 -251 45 -287 35
-291 35 -320 12z" /><path d="M4865 8275 c-44 -13 -134 -33 -200 -45 -73 -13 -137 -31 -162 -45
-23 -13 -59 -29 -80 -35 -21 -7 -60 -25 -88 -40 -27 -15 -74 -36 -104 -45
-100 -33 -389 -230 -547 -375 -65 -58 -234 -247 -234 -260 0 -5 -13 -25 -30
-43 -45 -52 -97 -127 -123 -179 -13 -26 -36 -62 -50 -82 -15 -19 -27 -38 -27
-43 0 -4 -16 -33 -35 -65 -20 -32 -43 -81 -51 -110 -9 -29 -28 -75 -44 -103
-15 -27 -33 -68 -40 -90 -7 -22 -23 -61 -36 -87 -13 -25 -33 -93 -45 -150 -11
-56 -31 -128 -44 -158 -13 -30 -30 -94 -39 -142 -9 -47 -23 -103 -32 -123 -31
-74 -64 -566 -64 -945 0 -230 20 -636 39 -802 6 -46 19 -108 29 -138 11 -30
24 -85 31 -122 6 -36 22 -95 35 -130 13 -35 34 -108 45 -163 12 -55 35 -128
51 -162 16 -34 32 -74 36 -90 4 -15 20 -50 35 -78 15 -27 35 -76 44 -107 9
-31 24 -67 34 -80 10 -13 30 -46 44 -75 15 -28 31 -54 37 -58 6 -4 24 -34 40
-68 17 -34 50 -89 75 -122 25 -33 57 -78 73 -100 97 -144 331 -372 486 -475
29 -19 91 -61 138 -92 46 -31 97 -62 113 -68 30 -12 180 -83 265 -126 25 -12
83 -30 130 -39 47 -9 126 -30 175 -46 50 -15 123 -33 163 -39 77 -11 104 -5
116 26 10 26 7 640 -3 667 -15 38 -49 65 -105 83 -28 9 -63 25 -77 35 -14 10
-50 28 -80 39 -153 60 -422 327 -535 530 -17 30 -56 99 -87 152 -32 54 -64
125 -73 158 -8 34 -29 89 -45 123 -16 34 -29 68 -29 75 0 7 -15 52 -34 100
-25 64 -40 127 -55 238 -12 83 -31 182 -41 220 -11 38 -25 130 -30 204 -6 74
-15 180 -22 235 -15 127 -14 481 0 605 7 52 16 154 22 226 6 71 19 163 30 203
11 40 30 136 41 214 11 78 30 167 41 197 11 30 31 87 44 125 13 39 32 86 43
105 10 19 28 66 41 105 12 38 35 88 51 109 16 22 29 45 29 52 0 6 14 29 31 50
17 22 40 57 51 79 57 111 295 353 398 405 25 12 52 28 60 35 31 28 197 108
255 124 44 12 63 23 72 41 10 19 13 117 13 402 -1 412 0 409 -58 407 -15 0
-63 -11 -107 -24z" /><path d="M5438 5169 c-44 -17 -48 -49 -45 -364 l2 -289 25 -13 c19 -10 141
-13 521 -13 487 0 497 0 523 -21 l26 -20 0 -752 c-1 -564 -4 -759 -13 -779
-22 -47 -112 -119 -177 -139 -33 -10 -80 -27 -104 -39 -25 -11 -76 -27 -113
-34 -37 -8 -87 -22 -111 -30 -72 -26 -156 -37 -350 -46 -256 -12 -232 29 -229
-386 l2 -326 24 -19 c30 -25 83 -24 276 1 83 11 191 25 240 30 50 6 128 21
175 35 47 13 137 34 200 46 63 12 135 31 160 44 25 12 63 28 85 35 22 7 63 25
90 40 28 15 68 33 90 40 41 12 136 65 239 132 32 21 64 38 71 38 17 0 225 179
312 269 126 129 113 -20 113 1340 0 1146 -1 1179 -19 1202 l-19 24 -984 2
c-630 1 -993 -2 -1010 -8z" />
    </g>
  </svg>
);

export const IconCopy = (p: IconProps) => (
  <Svg {...p}><rect x="9" y="9" width="11" height="11" rx="2" /><path d="M5 15V6a2 2 0 0 1 2-2h9" /></Svg>
);

export const IconCheck = (p: IconProps) => <Svg {...p}><path d="m5 13 4 4L19 7" /></Svg>;

export const IconExternal = (p: IconProps) => (
  <Svg {...p}><path d="M14 4h6v6M20 4l-9 9" /><path d="M18 14v5a1 1 0 0 1-1 1H5a1 1 0 0 1-1-1V7a1 1 0 0 1 1-1h5" /></Svg>
);

export const IconSun = (p: IconProps) => (
  <Svg {...p}><circle cx="12" cy="12" r="4" /><path d="M12 2v2M12 20v2M4.9 4.9l1.4 1.4M17.7 17.7l1.4 1.4M2 12h2M20 12h2M4.9 19.1l1.4-1.4M17.7 6.3l1.4-1.4" /></Svg>
);

export const IconMoon = (p: IconProps) => <Svg {...p}><path d="M20 14.5A8.5 8.5 0 1 1 9.5 4a7 7 0 0 0 10.5 10.5Z" /></Svg>;

export const IconMail = (p: IconProps) => (
  <Svg {...p}><rect x="3" y="5" width="18" height="14" rx="2" /><path d="m4 7 8 6 8-6" /></Svg>
);

export const IconChat = (p: IconProps) => (
  <Svg {...p}><path d="M21 12a8 8 0 0 1-11.6 7.1L4 20l1-4.6A8 8 0 1 1 21 12Z" /><path d="M9 11h.01M12 11h.01M15 11h.01" /></Svg>
);

export const IconPlay = (p: IconProps) => <Svg {...p}><path d="M7 4.5v15l13-7.5Z" /></Svg>;

export const IconStop = (p: IconProps) => <Svg {...p}><rect x="6" y="6" width="12" height="12" rx="2" /></Svg>;

export const IconSave = (p: IconProps) => (
  <Svg {...p}><path d="M5 4h11l3 3v13H5Z" /><path d="M9 4v5h6V4M8 20v-5h8v5" /></Svg>
);

export const IconPlus = (p: IconProps) => <Svg {...p}><path d="M12 5v14M5 12h14" /></Svg>;

export const IconChevronDown = (p: IconProps) => <Svg {...p}><path d="m6 9 6 6 6-6" /></Svg>;

export const IconLock = (p: IconProps) => (
  <Svg {...p}><rect x="4.5" y="10.5" width="15" height="10" /><path d="M8 10.5V7a4 4 0 0 1 8 0v3.5" /></Svg>
);

export const IconUnlock = (p: IconProps) => (
  <Svg {...p}><rect x="4.5" y="10.5" width="15" height="10" /><path d="M8 10.5V7a4 4 0 0 1 7.7-1.5" /></Svg>
);

export const IconFolder = (p: IconProps) => (
  <Svg {...p}><path d="M3 7a2 2 0 0 1 2-2h4l2 2h8a2 2 0 0 1 2 2v8a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z" /></Svg>
);

export const IconTrash = (p: IconProps) => (
  <Svg {...p}><path d="M4 7h16M10 11v6M14 11v6" /><path d="M6 7l1 13h10l1-13M9 7V4h6v3" /></Svg>
);

export const IconHistory = (p: IconProps) => (
  <Svg {...p}><path d="M4 12a8 8 0 1 0 2.3-5.6" /><path d="M4 4v4h4M12 8v4.5l3 1.8" /></Svg>
);

export const IconAlert = (p: IconProps) => (
  <Svg {...p}><path d="M12 4 2.5 20h19Z" /><path d="M12 10v4M12 17.5v.01" /></Svg>
);

export const IconKey = (p: IconProps) => (
  <Svg {...p}><circle cx="8" cy="12" r="4" /><path d="M12 12h9M17 12v4M20 12v3" /></Svg>
);

export const IconDots = (p: IconProps) => (
  <Svg {...p}><path d="M5 12h.01M12 12h.01M19 12h.01" strokeWidth={3} /></Svg>
);

export const IconInfo = (p: IconProps) => (
  <Svg {...p}><circle cx="12" cy="12" r="9" /><path d="M12 11v5.5M12 7.5v.01" /></Svg>
);

export const IconUpload = (p: IconProps) => (
  <Svg {...p}><path d="M12 16V4M7 9l5-5 5 5M4 20h16" /></Svg>
);

export const IconSpark = (p: IconProps) => (
  <Svg {...p}><path d="M12 3.5 13.8 9l5.7 1.8-5.7 1.8L12 18.5l-1.8-5.9L4.5 10.8 10.2 9Z" /></Svg>
);

export const IconSpinner = ({ className = 'h-4 w-4' }: IconProps) => (
  <svg viewBox="0 0 24 24" className={`${className} animate-spin`} aria-hidden="true" {...stroke}>
    <path d="M12 3a9 9 0 1 0 9 9" />
  </svg>
);

/** A globe, for the callers and where they are. */
export const IconGlobe = ({ className = 'h-4 w-4' }: IconProps) => (
  <svg viewBox="0 0 24 24" className={className} aria-hidden="true" {...stroke}>
    <circle cx="12" cy="12" r="9" />
    <path d="M3 12h18" />
    <path d="M12 3a15 15 0 0 1 0 18a15 15 0 0 1 0-18Z" />
  </svg>
);

/** Stacked sheets, for folding identical lines into one. */
export const IconLayers = ({ className = 'h-4 w-4' }: IconProps) => (
  <svg viewBox="0 0 24 24" className={className} aria-hidden="true" {...stroke}>
    <path d="m12 3 8 4.5-8 4.5-8-4.5L12 3Z" />
    <path d="m4 12 8 4.5 8-4.5" />
    <path d="m4 16.5 8 4.5 8-4.5" />
  </svg>
);

export function IconDownload(props: IconProps) {
  return (
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8"
         strokeLinecap="round" strokeLinejoin="round" className={props.className ?? 'h-4 w-4'}>
      <path d="M12 3v12" />
      <path d="m7 11 5 5 5-5" />
      <path d="M4 19h16" />
    </svg>
  );
}

/** Three lines, for the menu that holds what a phone has no room for. */
export const IconMenu = (p: IconProps) => <Svg {...p}><path d="M4 7h16M4 12h16M4 17h16" /></Svg>;

export const IconClose = (p: IconProps) => <Svg {...p}><path d="M6 6l12 12M18 6 6 18" /></Svg>;

/** A paper plane, for sending something out into the world. */
export const IconSend = (p: IconProps) => (
  <Svg {...p}><path d="M21 3 10 14M21 3l-7 18-4-7-7-4 18-7Z" /></Svg>
);

/** An eye, for reading what is there. */
export const IconEye = (p: IconProps) => (
  <Svg {...p}><path d="M2.5 12S6 5.5 12 5.5 21.5 12 21.5 12 18 18.5 12 18.5 2.5 12 2.5 12Z" /><circle cx="12" cy="12" r="2.8" /></Svg>
);

/** A pencil, for writing code. */
export const IconPencil = (p: IconProps) => (
  <Svg {...p}><path d="M4 20h4L19 9a2.8 2.8 0 0 0-4-4L4 16Z" /><path d="m13.5 6.5 4 4" /></Svg>
);

/** Lines of text, for a log. */
export const IconList = (p: IconProps) => (
  <Svg {...p}><path d="M8 6h12M8 12h12M8 18h12M4 6h.01M4 12h.01M4 18h.01" /></Svg>
);

/** An open book, for a guide. */
export const IconBook = (p: IconProps) => (
  <Svg {...p}><path d="M12 6.5C10 5 7 4.5 3.5 5v13c3.5-.5 6.5 0 8.5 1.5 2-1.5 5-2 8.5-1.5V5C17 4.5 14 5 12 6.5Z" /><path d="M12 6.5v13" /></Svg>
);

/** A flask, for the tests of a lambda. */
export const IconBeaker = (p: IconProps) => (
  <Svg {...p}><path d="M9 3.5h6" /><path d="M10 3.5v6L4.8 18.3A1.5 1.5 0 0 0 6.1 20.5h11.8a1.5 1.5 0 0 0 1.3-2.2L14 9.5v-6" /><path d="M7.5 14.5h9" /></Svg>
);

/** A wrench, for compiling. */
export const IconWrench = (p: IconProps) => (
  <Svg {...p}><path d="M14.5 5.5a4 4 0 0 0 4.9 4.9L20 11l-9 9a2.1 2.1 0 0 1-3-3l9-9 .6.6a4 4 0 0 0-4.9-4.9l2.3 2.3-1.4 1.4Z" /></Svg>
);

/** A page being written on, for a draft - a copy of the app a change is tried on. */
export const IconDraft = (p: IconProps) => (
  <Svg {...p}><path d="M13 20.5H6.5a2 2 0 0 1-2-2v-13a2 2 0 0 1 2-2H13l4.5 4.5v3" /><path d="M13 3.5V8h4.5" /><path d="M8 12h4" /><path d="M8 15.5h2" /><path d="M14 21v-2.2l5-5a1.6 1.6 0 0 1 2.2 2.2l-5 5Z" /></Svg>
);

/** One card with a line or two on it, for the simple view: the app and little else. */
export const IconViewSimple = (p: IconProps) => (
  <Svg {...p}><path d="M4 5.5h16v13H4Z" /><path d="M8 10.5h8M8 14h5" /></Svg>
);

/** A sidebar beside a page of lines, for the full view: every section. */
export const IconViewFull = (p: IconProps) => (
  <Svg {...p}><path d="M3.5 4.5h17v15h-17Z" /><path d="M9 4.5v15" /><path d="M12 9h5.5M12 12h5.5M12 15h3.5M5.5 8.5h1.5M5.5 11.5h1.5" /></Svg>
);
