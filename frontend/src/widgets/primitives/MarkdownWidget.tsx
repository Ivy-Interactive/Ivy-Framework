import { useEventHandler } from "@/components/EventHandlerContext";
import MarkdownRenderer from "@/components/MarkdownRenderer";
import React from "react";

interface MarkdownWidgetProps {
  id: string;
  content: string;
}

const MarkdownWidget: React.FC<MarkdownWidgetProps> = ({ id, content }) => {
  const eventHandler = useEventHandler();
  return (
    <MarkdownRenderer
      content={content}
      key={id}
      onLinkClick={(href) => eventHandler("OnLinkClick", id, [href])}
    />
  );
};

export default MarkdownWidget;
