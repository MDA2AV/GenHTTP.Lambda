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

export const IconLogo = ({ className = 'h-6 w-6' }: IconProps) => (
  <svg viewBox="0 0 32 32" className={className} aria-hidden="true">
    <rect width="32" height="32" rx="7" className="fill-accent-500" />
    <path d="M11 10 6.5 16 11 22M21 10l4.5 6-4.5 6M18.5 8l-5 16" stroke="white" strokeWidth="2.2" fill="none" strokeLinecap="round" strokeLinejoin="round" />
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

export const IconPlay = (p: IconProps) => <Svg {...p}><path d="M7 4.5v15l13-7.5Z" /></Svg>;

export const IconStop = (p: IconProps) => <Svg {...p}><rect x="6" y="6" width="12" height="12" rx="2" /></Svg>;

export const IconSave = (p: IconProps) => (
  <Svg {...p}><path d="M5 4h11l3 3v13H5Z" /><path d="M9 4v5h6V4M8 20v-5h8v5" /></Svg>
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

export const IconSpark = (p: IconProps) => (
  <Svg {...p}><path d="M12 3.5 13.8 9l5.7 1.8-5.7 1.8L12 18.5l-1.8-5.9L4.5 10.8 10.2 9Z" /></Svg>
);

export const IconSpinner = ({ className = 'h-4 w-4' }: IconProps) => (
  <svg viewBox="0 0 24 24" className={`${className} animate-spin`} aria-hidden="true" {...stroke}>
    <path d="M12 3a9 9 0 1 0 9 9" />
  </svg>
);
