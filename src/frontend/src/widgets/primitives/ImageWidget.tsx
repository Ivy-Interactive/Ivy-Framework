import { getHeight, getWidth } from "@/lib/styles";
import { getIvyHost, convertAppUrlToPath } from "@/lib/utils";
import {
  validateImageUrl,
  isFullUrl,
  normalizeRelativePath,
  validateLinkUrl,
  isExternalUrl,
  isAppProtocol,
} from "@/lib/url";
import { useEventHandler } from "@/components/event-handler";
import React, { useCallback } from "react";

interface ImageWidgetProps {
  id: string;
  src: string | undefined | null;
  alt?: string;
  caption?: string;
  link?: string;
  objectFit?: "Cover" | "Contain" | "Fill" | "None" | "ScaleDown";
  events?: string[];
  width?: string;
  height?: string;
}

const getImageUrl = (url: string | undefined | null): string | null => {
  if (!url) return null;

  // Validate and sanitize image URL to prevent open redirect vulnerabilities
  const validatedUrl = validateImageUrl(url);
  if (!validatedUrl) {
    return null;
  }

  // If it's already a full URL (http/https/data/blob/app), return it
  if (isFullUrl(validatedUrl)) {
    return validatedUrl;
  }

  // For relative paths, construct full URL with Ivy host
  // validatedUrl is already a safe relative path (starts with / or was normalized)
  const relativePath = normalizeRelativePath(validatedUrl);
  return `${getIvyHost()}${relativePath}`;
};

const getLinkProps = (link: string): { href: string; target?: string; rel?: string } | null => {
  const validatedUrl = validateLinkUrl(link);
  if (validatedUrl === "#") return null;

  if (isAppProtocol(validatedUrl)) {
    return { href: convertAppUrlToPath(validatedUrl) };
  }

  if (isExternalUrl(validatedUrl)) {
    return {
      href: validatedUrl,
      target: "_blank",
      rel: "noopener noreferrer",
    };
  }

  // Relative path
  const relativePath = normalizeRelativePath(validatedUrl);
  return { href: `${getIvyHost()}${relativePath}` };
};

const objectFitMap: Record<string, React.CSSProperties["objectFit"]> = {
  Cover: "cover",
  Contain: "contain",
  Fill: "fill",
  None: "none",
  ScaleDown: "scale-down",
};

export const ImageWidget: React.FC<ImageWidgetProps> = ({
  id,
  src,
  alt,
  caption,
  link,
  objectFit,
  events,
  width = "MinContent",
  height = "MinContent",
}) => {
  const eventHandler = useEventHandler();
  const hasOnClick = events?.includes("OnClick") ?? false;

  const handleClick = useCallback(() => {
    if (hasOnClick) eventHandler("OnClick", id, []);
  }, [id, eventHandler, hasOnClick]);

  const styles: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
    ...(objectFit ? { objectFit: objectFitMap[objectFit] } : {}),
  };

  // getImageUrl handles null/undefined and validates the URL internally
  const validatedImageSrc = getImageUrl(src);
  if (!validatedImageSrc) {
    // Show error message for missing or invalid URLs
    return (
      <div
        key={id}
        style={styles}
        className="flex items-center justify-center bg-destructive/10 text-destructive rounded border-2 border-dashed border-destructive/25 p-4"
        role="alert"
        aria-label="Invalid image URL"
      >
        <span className="text-sm">{!src ? "No image source provided" : "Invalid image URL"}</span>
      </div>
    );
  }

  // OnClick takes precedence over Link
  const linkProps = !hasOnClick && link ? getLinkProps(link) : null;

  const altText = alt ?? caption ?? "";

  const clickProps = hasOnClick
    ? { onClick: handleClick, style: { cursor: "pointer" as const } }
    : {};

  const imgStyles: React.CSSProperties = objectFit
    ? {
        width: "100%",
        height: "100%",
        objectFit: objectFitMap[objectFit],
      }
    : {};

  const imgElement = (
    <img src={validatedImageSrc} alt={altText} style={imgStyles} {...clickProps} />
  );

  const wrappedImg = linkProps ? (
    <a {...linkProps} style={{ cursor: "pointer" }}>
      {imgElement}
    </a>
  ) : (
    imgElement
  );

  if (caption) {
    return (
      <figure key={id} style={styles} className="flex flex-col items-center">
        {wrappedImg}
        <figcaption className="text-sm text-muted-foreground mt-1">{caption}</figcaption>
      </figure>
    );
  }

  if (linkProps) {
    return (
      <a key={id} {...linkProps} style={{ ...styles, cursor: "pointer" }}>
        <img src={validatedImageSrc} alt={altText} style={imgStyles} />
      </a>
    );
  }

  return (
    <img
      src={validatedImageSrc}
      key={id}
      style={{ ...styles, ...(hasOnClick ? { cursor: "pointer" } : {}) }}
      alt={altText}
      onClick={hasOnClick ? handleClick : undefined}
    />
  );
};
