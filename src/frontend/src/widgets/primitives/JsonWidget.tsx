import { JsonRenderer } from '@/components/JsonRenderer';
import React from 'react';

interface JsonWidgetProps {
  id: string;
  content: string;
  expanded?: number | null;
}

const JsonWidget: React.FC<JsonWidgetProps> = ({ id, content, expanded }) => (
  <JsonRenderer data={content} key={id} initialExpanded={expanded} />
);

export default JsonWidget;
