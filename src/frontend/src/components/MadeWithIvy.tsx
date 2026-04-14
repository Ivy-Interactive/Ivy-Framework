"use client";
import { useState, useEffect } from "react";
import IvyLogo from "./IvyLogo";

function toGitHubUrl(value: string): string {
  if (value.startsWith("http")) return value;
  return `https://github.com/${value}`;
}

export default function MadeWithIvy() {
  const [isHovered, setIsHovered] = useState(false);
  const [shouldShow, setShouldShow] = useState(false);
  const [gitHubUrl] = useState<string | null>(() => {
    if (typeof document === "undefined") return null;
    const meta = document.querySelector('meta[name="github-url"]');
    return meta?.getAttribute("content") || null;
  });

  useEffect(() => {
    const checkWindowSize = () => {
      setShouldShow(window.innerWidth >= 600 && window.innerHeight >= 600);
    };

    checkWindowSize();
    window.addEventListener("resize", checkWindowSize);

    return () => window.removeEventListener("resize", checkWindowSize);
  }, []);

  if (!shouldShow) return null;

  const linkUrl = gitHubUrl
    ? toGitHubUrl(gitHubUrl)
    : "https://github.com/Ivy-Interactive/Ivy-Framework";

  const handleClick = () => {
    window.open(linkUrl, "_blank");
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === "Enter" || e.key === " ") {
      e.preventDefault();
      window.open(linkUrl, "_blank");
    }
  };

  return (
    <div
      className="fixed bottom-0 right-0 z-100 overflow-hidden rounded-tl-full "
      onMouseEnter={() => setIsHovered(true)}
      onMouseLeave={() => setIsHovered(false)}
    >
      <div
        className={`
          rounded-tl-full
          flex
          items-end
          justify-end
          transition-all
          duration-300
          ease-in-out
          origin-bottom-right
          cursor-pointer
          ${isHovered ? "w-48 h-48" : "w-16 h-16"}
          bg-ivy-green
        `}
        onClick={handleClick}
        role="button"
        tabIndex={0}
        onKeyDown={handleKeyDown}
      >
        <div
          className={`
            flex
            flex-col
            items-right
            gap-1.5
            transition-opacity
            duration-300
            m-4
            text-background
            ${isHovered ? "opacity-100" : "opacity-0"}
          `}
        >
          <span className="font-mono font-bold opacity-70">MADE WITH</span>
          <IvyLogo className="w-24" />
        </div>
      </div>
    </div>
  );
}
