import { JsonRenderer } from "@/components/JsonRenderer";
import { getWidth, getHeight } from "@/lib/styles";
import React from "react";

interface JsonWidgetProps {
  id: string;
  content: string;
  expanded?: number | null;
  width?: string;
  height?: string;
}

const JsonWidget: React.FC<JsonWidgetProps> = ({ id, content, expanded, width, height }) => (
  <div style={{ ...getWidth(width), ...getHeight(height) }}>
    <JsonRenderer data={content} key={id} initialExpanded={expanded} />
  </div>
);

export default JsonWidget;
