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
import { SiNotion } from "react-icons/si";

interface IconProps {
  name?: string;
  color?: string;
  size?: string | number;
  className?: string;
  style?: React.CSSProperties;
}

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
    <path d="M11 1H1V11C6.47368 11 11 6.47368 11 1Z" fill={color || "currentColor"} />
  </svg>
);

const Icon: React.FC<IconProps> = ({ name, color, size, className, style }) => {
  if (name === "None") {
    return <Folder className="invisible" size={size} />;
  }

  if (name === "IvyCorner") {
    return <IvyCornerIcon size={size} color={color} style={style} className={cn(className)} />;
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
