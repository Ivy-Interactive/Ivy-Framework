import { useEventHandler } from "@/components/event-handler";
import { getHeight, getWidth } from "@/lib/styles";
import React from "react";

interface SvgWidgetProps {
  id: string;
  content: string;
  width?: string;
  height?: string;
  events?: string[];
}

const EMPTY_EVENTS: string[] = [];

export const SvgWidget: React.FC<SvgWidgetProps> = ({
  id,
  content,
  width = "Auto",
  height = "Auto",
  events = EMPTY_EVENTS,
}) => {
  const eventHandler = useEventHandler();
  const styles: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
  };

  const ref = React.useRef<HTMLDivElement>(null);

  React.useEffect(() => {
    if (ref.current) {
      ref.current.innerHTML = content;

      // Attach click listener for link interception
      if (events.includes("OnLinkClick")) {
        const handleClick = (event: MouseEvent) => {
          let target = event.target as HTMLElement | null;

          // Walk up the DOM tree to find the nearest <a> element
          while (target && target !== ref.current) {
            if (target.tagName === "A" || target.tagName === "a") {
              const href = target.getAttribute("href");
              if (href) {
                event.preventDefault();
                eventHandler("OnLinkClick", id, [href]);
                return;
              }
            }
            target = target.parentElement;
          }
        };

        ref.current.addEventListener("click", handleClick);

        // Cleanup listener on unmount or content change
        return () => {
          ref.current?.removeEventListener("click", handleClick);
        };
      }
    }
  }, [content, events, eventHandler, id]);

  return <div key={id} ref={ref} style={styles} />;
};
