import { cn } from "@/lib/utils";
import { Folder, icons } from "lucide-react";
import {
  FaGoogle,
  FaAmazon,
  FaMicrosoft,
  FaGitlab,
  FaBitbucket,
  FaDiscord,
  FaTwitter,
  FaInstagram,
  FaFacebook,
  FaLinkedin,
  FaYoutube,
  FaVimeo,
  FaSlack,
  FaSpotify,
  FaApple,
  FaGithub,
  FaPinterest,
} from "react-icons/fa";
import { FaXTwitter } from "react-icons/fa6";
import { IconType } from "react-icons";
import { VscAzure } from "react-icons/vsc";
import {
  SiNotion,
  SiGithubcopilot,
  SiOpenai,
  SiAnthropic,
  SiGooglegemini,
  SiLinear,
} from "react-icons/si";

interface IconProps {
  name?: string;
  color?: string;
  size?: string | number;
  className?: string;
  style?: React.CSSProperties;
}

const AntigravityIcon = ({ size, color, style, className }: IconProps) => (
  <svg
    width={size || 24}
    height={size || 24}
    viewBox="-4 -4.5 24 24"
    fill="none"
    xmlns="http://www.w3.org/2000/svg"
    style={style}
    className={className}
  >
    <path
      d="M14.0777 13.984C14.945 14.6345 16.2458 14.2008 15.0533 13.0084C11.476 9.53949 12.2349 0 7.79033 0C3.34579 0 4.10461 9.53949 0.527295 13.0084C-0.773543 14.3092 0.635692 14.6345 1.50293 13.984C4.86344 11.7076 4.64663 7.69664 7.79033 7.69664C10.934 7.69664 10.7172 11.7076 14.0777 13.984Z"
      fill={color || "currentColor"}
    />
  </svg>
);

const OpenCodeIcon = ({ size, color, style, className }: IconProps) => (
  <svg
    width={size || 24}
    height={size || 24}
    viewBox="0 0 300 300"
    fill="none"
    xmlns="http://www.w3.org/2000/svg"
    style={style}
    className={className}
  >
    <path d="M210 240H90V120H210V240Z" fill={color || "currentColor"} fillOpacity="0.35" />
    <path d="M210 60H90V240H210V60ZM270 300H30V0H270V300Z" fill={color || "currentColor"} />
  </svg>
);

const IvyCornerIcon = ({ size, color, style, className }: IconProps) => (
  <svg
    width={size || 24}
    height={size || 24}
    viewBox="0 0 12 12"
    fill="none"
    xmlns="http://www.w3.org/2000/svg"
    style={style}
    className={className}
  >
    <path d="M11 1H1V11C6.47 11 11 6.47 11 1Z" fill={color || "currentColor"} />
  </svg>
);

const Icon: React.FC<IconProps> = ({ name, color, size, className, style }) => {
  if (name === "None") {
    return <Folder className="invisible" size={size} />;
  }

  if (name === "IvyCorner") {
    return <IvyCornerIcon size={size} color={color} style={style} className={cn(className)} />;
  }

  if (name === "Antigravity") {
    return <AntigravityIcon size={size} color={color} style={style} className={cn(className)} />;
  }

  if (name === "OpenCode") {
    return <OpenCodeIcon size={size} color={color} style={style} className={cn(className)} />;
  }

  // Handle react-icons
  const reactIcons: { [key: string]: IconType } = {
    Google: FaGoogle,
    Azure: VscAzure,
    Amazon: FaAmazon,
    Microsoft: FaMicrosoft,
    Gitlab: FaGitlab,
    Bitbucket: FaBitbucket,
    Discord: FaDiscord,
    Twitter: FaTwitter,
    Instagram: FaInstagram,
    Facebook: FaFacebook,
    Linkedin: FaLinkedin,
    Youtube: FaYoutube,
    Vimeo: FaVimeo,
    Slack: FaSlack,
    Spotify: FaSpotify,
    Notion: SiNotion,
    Apple: FaApple,
    Github: FaGithub,
    Pinterest: FaPinterest,
    XTwitter: FaXTwitter,
    Copilot: SiGithubcopilot,
    OpenAI: SiOpenai,
    ClaudeCode: SiAnthropic,
    Gemini: SiGooglegemini,
    Linear: SiLinear,
  };

  if (name && name in reactIcons) {
    const ReactIcon = reactIcons[name];
    return <ReactIcon style={style} color={color} size={size} className={cn(className)} />;
  }

  if (!name || !(name in icons)) {
    return null;
  }

  const LucideIcon = icons[name as keyof typeof icons];
  return <LucideIcon style={style} color={color} size={size} className={cn(className)} />;
};

export default Icon;
