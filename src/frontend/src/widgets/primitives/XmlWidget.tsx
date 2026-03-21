import { XmlRenderer } from '@/components/XmlRenderer';
import React from 'react';

interface XmlWidgetProps {
  id: string;
  content: string;
  expanded?: number | null;
}

const XmlWidget: React.FC<XmlWidgetProps> = ({ id, content, expanded }) => (
  <XmlRenderer data={content} key={id} initialExpanded={expanded} />
);

export default XmlWidget;
