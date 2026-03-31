import { XmlRenderer } from "@/components/XmlRenderer";
import { getWidth, getHeight } from "@/lib/styles";
import React from "react";

interface XmlWidgetProps {
  id: string;
  content: string;
  expanded?: number | null;
  width?: string;
  height?: string;
}

const XmlWidget: React.FC<XmlWidgetProps> = ({ id, content, expanded, width, height }) => (
  <div style={{ ...getWidth(width), ...getHeight(height) }}>
    <XmlRenderer data={content} key={id} initialExpanded={expanded} />
  </div>
);

export default XmlWidget;
