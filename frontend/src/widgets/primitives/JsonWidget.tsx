import { JsonRenderer } from "@/components/JsonRenderer";
import React from "react";

interface JsonWidgetProps {
  id: string;
  content: string;
}

const JsonWidget: React.FC<JsonWidgetProps> = ({ id, content }) => (
  <JsonRenderer data={content} key={id} />
);

export default JsonWidget;
