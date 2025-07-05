import { XmlRenderer } from "@/components/XmlRenderer";
import React from "react";

interface XmlWidgetProps {
  id: string;
  content: string;
}

const XmlWidget: React.FC<XmlWidgetProps> = ({ id, content }) => (
  <XmlRenderer data={content} key={id} />
);

export default XmlWidget;
