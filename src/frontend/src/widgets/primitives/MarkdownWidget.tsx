import { useEventHandler } from "@/components/event-handler";
import MarkdownRenderer from "@/components/MarkdownRenderer";
import { getWidth, getHeight } from "@/lib/styles";
import React, { useCallback, useState, useEffect } from "react";
import { widgetContentOverrides, subscribeToContentOverride } from "@/widgets/widgetRenderer";

import "@/styles/markdown-spacing.css";

import { Densities } from "@/types/density";
import { TextAlignment } from "@/types/textAlignment";

interface MarkdownWidgetProps {
  id: string;
  content: string;
  density?: Densities;
  textAlignment?: TextAlignment;
  dangerouslyAllowLocalFiles?: boolean;
  width?: string;
  height?: string;
  events?: string[];
}

const EMPTY_EVENTS: string[] = [];

const MarkdownWidget: React.FC<MarkdownWidgetProps> = ({
  id,
  content = "",
  density = Densities.Medium,
  textAlignment,
  dangerouslyAllowLocalFiles = false,
  width,
  height,
  events = EMPTY_EVENTS,
}) => {
  const eventHandler = useEventHandler();
  const [, forceUpdate] = useState(0);

  // Subscribe to content override changes
  useEffect(() => {
    return subscribeToContentOverride(id, () => forceUpdate((n) => n + 1));
  }, [id]);

  const handleLinkClick = useCallback(
    (href: string) => {
      if (events.includes("OnLinkClick")) eventHandler("OnLinkClick", id, [href]);
    },
    [eventHandler, id, events],
  );

  // Use override content if available, otherwise use prop
  const displayContent = widgetContentOverrides.get(id) ?? content;

  const getScaleStyle = (s: Densities): React.CSSProperties => {
    switch (s) {
      case Densities.Small:
        return {
          transform: "scale(0.85)",
          width: "117.65%",
          transformOrigin: "top left",
        };
      case Densities.Large:
        return {
          transform: "scale(1.15)",
          width: "86.96%",
          transformOrigin: "top left",
        };
      default:
        return {};
    }
  };

  const styles: React.CSSProperties = {
    display: "flex",
    flexDirection: "column",
    wordBreak: "normal",
    overflowWrap: "break-word",
    ...(textAlignment && {
      textAlign: textAlignment.toLowerCase() as React.CSSProperties["textAlign"],
    }),
    ...getScaleStyle(density),
    ...getWidth(width),
    ...getHeight(height),
  };

  return (
    <div className="markdown-widget w-full" style={styles}>
      <MarkdownRenderer
        key={id}
        content={displayContent}
        onLinkClick={handleLinkClick}
        dangerouslyAllowLocalFiles={dangerouslyAllowLocalFiles}
      />
    </div>
  );
};

export default React.memo(MarkdownWidget);
