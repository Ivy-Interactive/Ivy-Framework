import {
  BorderRadius,
  BorderStyle,
  getBorderStyle,
  getBorderThickness,
  getBoxRadius,
  getColor,
  getHeight,
  getWidth,
} from "@/lib/styles";
import { cn, getIvyHost, convertAppUrlToPath } from "@/lib/utils";
import {
  validateImageUrl,
  isFullUrl,
  normalizeRelativePath,
  validateLinkUrl,
  isExternalUrl,
  isAppProtocol,
} from "@/lib/url";
import { useEventHandler } from "@/components/event-handler";
import type { HoverEffect } from "@/widgets/primitives/BoxWidget";
import { cardStyles } from "@/widgets/card/styles";
import { ImageOverlay } from "@/components/markdown/ImageOverlay";
import { useImageOverlayContext } from "@/components/markdown/ImageOverlayContext";
import React, { useCallback, useEffect, useState } from "react";

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
  borderColor?: string;
  borderOpacity?: number;
  borderRadius?: BorderRadius;
  borderStyle?: BorderStyle;
  borderThickness?: string;
  hoverVariant?: HoverEffect;
  overlay?: boolean;
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

const getHoverClass = (hoverVariant?: HoverEffect): string | null => {
  if (!hoverVariant || hoverVariant === "None") return null;
  if (hoverVariant === "Pointer") return cardStyles.hover.pointer;
  if (hoverVariant === "Shadow") return cardStyles.hover.shadow;
  return cardStyles.hover.pointerAndTranslate;
};

const getBorderRadiusStyle = (borderRadius?: BorderRadius): React.CSSProperties => {
  if (!borderRadius || borderRadius === "None") return { borderRadius: "0" };
  if (borderRadius === "Full") return { borderRadius: "9999px" };
  return getBoxRadius();
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
  borderColor,
  borderOpacity,
  borderRadius = "None",
  borderStyle = "None",
  borderThickness = "0",
  hoverVariant = "None",
  overlay = false,
}) => {
  const eventHandler = useEventHandler();
  const hasOnClick = events?.includes("OnClick") ?? false;
  const [showOverlay, setShowOverlay] = useState(false);
  const imageOverlayContext = useImageOverlayContext();

  const validatedSrc = getImageUrl(src);
  const altText = alt ?? caption ?? "";

  // Register with context for sibling navigation
  useEffect(() => {
    if (overlay && validatedSrc && imageOverlayContext) {
      imageOverlayContext.register(id, validatedSrc, altText);
      return () => imageOverlayContext.unregister(id);
    }
  }, [overlay, validatedSrc, altText, id, imageOverlayContext]);

  const handleClick = useCallback(() => {
    if (overlay) {
      if (imageOverlayContext) {
        imageOverlayContext.open(id);
      } else {
        setShowOverlay(true);
      }
    } else if (hasOnClick) {
      eventHandler("OnClick", id, []);
    }
  }, [id, eventHandler, hasOnClick, overlay, imageOverlayContext]);

  const outerStyles: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
  };

  const hasBorder = borderStyle !== "None" || borderColor != null || borderThickness !== "0";
  const hasHover = hoverVariant !== "None";
  const needsContainer = hasBorder || hasHover;

  const containerStyles: React.CSSProperties = needsContainer
    ? {
        ...getBorderStyle(borderStyle),
        ...getBorderThickness(borderThickness),
        ...getBorderRadiusStyle(borderRadius),
        ...getColor(borderColor, "borderColor", "background", borderOpacity),
        overflow: "hidden",
        display: "inline-block",
      }
    : {};

  if (!validatedSrc) {
    // Show error message for missing or invalid URLs
    return (
      <div
        key={id}
        style={outerStyles}
        className="flex items-center justify-center bg-destructive/10 text-destructive rounded border-2 border-dashed border-destructive/25 p-4"
        role="alert"
        aria-label="Invalid image URL"
      >
        <span className="text-sm">{!src ? "No image source provided" : "Invalid image URL"}</span>
      </div>
    );
  }

  // Overlay takes precedence over OnClick and Link
  const linkProps = !overlay && !hasOnClick && link ? getLinkProps(link) : null;

  const imgStyles: React.CSSProperties = objectFit
    ? {
        width: "100%",
        height: "100%",
        objectFit: objectFitMap[objectFit],
      }
    : {};

  const hoverClass = getHoverClass(hoverVariant);

  const imgElement = <img src={validatedSrc} alt={altText} style={imgStyles} />;

  const wrappedImg = linkProps ? (
    <a {...linkProps} style={{ cursor: "pointer" }}>
      {imgElement}
    </a>
  ) : (
    imgElement
  );

  const content = caption ? (
    <figure className="flex flex-col items-center">
      {wrappedImg}
      <figcaption className="text-sm text-muted-foreground mt-1">{caption}</figcaption>
    </figure>
  ) : linkProps ? (
    <a {...linkProps} style={{ cursor: "pointer" }}>
      <img src={validatedSrc} alt={altText} style={imgStyles} />
    </a>
  ) : (
    <img src={validatedSrc} alt={altText} style={imgStyles} />
  );

  const isClickable = overlay || hasOnClick || linkProps;

  // Only render standalone overlay when no context is available (fallback for single images)
  const overlayElement =
    overlay && showOverlay && !imageOverlayContext ? (
      <ImageOverlay src={validatedSrc} alt={altText} onClose={() => setShowOverlay(false)} />
    ) : null;

  if (needsContainer || overlay || hasOnClick) {
    return (
      <>
        <div
          key={id}
          style={{
            ...outerStyles,
            ...containerStyles,
            ...(isClickable ? { cursor: overlay ? "zoom-in" : "pointer" } : {}),
          }}
          className={cn(hoverClass)}
          onClick={overlay || hasOnClick ? handleClick : undefined}
          onKeyDown={
            overlay || hasOnClick
              ? (e) => {
                  if (e.key === "Enter" || e.key === " ") {
                    e.preventDefault();
                    handleClick();
                  }
                }
              : undefined
          }
          role={overlay || hasOnClick ? "button" : undefined}
          tabIndex={overlay || hasOnClick ? 0 : undefined}
        >
          {content}
        </div>
        {overlayElement}
      </>
    );
  }

  // Simple case: no border, no hover, no click
  return <img src={validatedSrc} key={id} style={outerStyles} alt={altText} />;
};
