import React from "react";

interface HtmlRendererProps {
  content: string;
  className?: string;
  allowedTags?: string[];
  onLinkClick?: (
    event: React.MouseEvent<HTMLAnchorElement>,
    href: string
  ) => void;
}

const sanitizeHtml = (
  html: string,
  allowedTags: string[] = [
    "p",
    "div",
    "span",
    "h1",
    "h2",
    "h3",
    "h4",
    "h5",
    "h6",
    "ul",
    "ol",
    "li",
    "a",
    "strong",
    "em",
    "b",
    "i",
    "br",
  ]
): string => {
  const temp = document.createElement("div");
  temp.innerHTML = html;

  // Remove any script tags and inline event handlers
  const scripts = temp.getElementsByTagName("script");
  const elements = temp.getElementsByTagName("*");

  // Remove script tags
  while (scripts[0]) {
    scripts[0].parentNode?.removeChild(scripts[0]);
  }

  // Remove inline event handlers and potentially harmful attributes
  for (let i = 0; i < elements.length; i++) {
    const element = elements[i];
    const attrs = element.attributes;

    for (let j = attrs.length - 1; j >= 0; j--) {
      const attr = attrs[j];
      if (
        attr.name.startsWith("on") ||
        (attr.name === "href" && attr.value.startsWith("javascript:"))
      ) {
        element.removeAttribute(attr.name);
      }
    }

    // Remove elements with tags not in allowedTags
    if (!allowedTags.includes(element.tagName.toLowerCase())) {
      element.parentNode?.removeChild(element);
    }
  }

  return temp.innerHTML;
};

export const HtmlRenderer: React.FC<HtmlRendererProps> = ({
  content,
  className = "",
  allowedTags,
  onLinkClick,
}) => {
  // Sanitize the HTML content
  const sanitizedContent = sanitizeHtml(content, allowedTags);

  // Handle link clicks
  const handleClick = (e: React.MouseEvent<HTMLDivElement>) => {
    const target = e.target as HTMLElement;
    if (target.tagName === "A" && onLinkClick) {
      e.preventDefault();
      const href = target.getAttribute("href") || "";
      onLinkClick(e as unknown as React.MouseEvent<HTMLAnchorElement>, href);
    }
  };

  return (
    <div className={`${className}`}>
      <div
        className="prose max-w-none"
        onClick={handleClick}
        dangerouslySetInnerHTML={{ __html: sanitizedContent }}
      />
    </div>
  );
};
