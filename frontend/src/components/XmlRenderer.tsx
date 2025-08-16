import { useState } from 'react';
import { ChevronRight, ChevronDown } from 'lucide-react';

interface XmlRendererProps {
  data: string;
}

interface XmlNode {
  type: 'element' | 'text' | 'cdata' | 'comment';
  name?: string;
  attributes?: Record<string, string>;
  children?: XmlNode[];
  value?: string;
}

export const XmlRenderer = ({ data }: XmlRendererProps) => {
  const [expanded, setExpanded] = useState(new Set());

  const parseXml = (xmlString: string): XmlNode | null => {
    try {
      const parser = new DOMParser();
      const xmlDoc = parser.parseFromString(xmlString, 'text/xml');

      if (xmlDoc.getElementsByTagName('parsererror').length > 0) {
        throw new Error('Invalid XML');
      }

      const convertDomToNode = (domNode: Node): XmlNode | null => {
        if (domNode.nodeType === Node.TEXT_NODE) {
          const text = domNode.textContent?.trim() || '';
          return text ? { type: 'text', value: text } : null;
        }

        if (domNode.nodeType === Node.COMMENT_NODE) {
          return {
            type: 'comment',
            value: domNode.textContent || '',
          };
        }

        if (domNode.nodeType === Node.CDATA_SECTION_NODE) {
          return {
            type: 'cdata',
            value: domNode.textContent || '',
          };
        }

        if (domNode.nodeType === Node.ELEMENT_NODE) {
          const element = domNode as Element;
          const attributes: Record<string, string> = {};

          element.getAttributeNames().forEach(attr => {
            attributes[attr] = element.getAttribute(attr) || '';
          });

          const children = Array.from(element.childNodes)
            .map(child => convertDomToNode(child))
            .filter((node): node is XmlNode => node !== null);

          return {
            type: 'element',
            name: element.tagName,
            attributes:
              Object.keys(attributes).length > 0 ? attributes : undefined,
            children: children.length > 0 ? children : undefined,
          };
        }

        return null;
      };

      return convertDomToNode(xmlDoc.documentElement);
    } catch (error) {
      console.error(error);
      return null;
    }
  };

  const toggleNode = (path: string) => {
    const newExpanded = new Set(expanded);
    if (newExpanded.has(path)) {
      newExpanded.delete(path);
    } else {
      newExpanded.add(path);
    }
    setExpanded(newExpanded);
  };

  const renderAttributes = (attributes: Record<string, string>) => {
    return Object.entries(attributes).map(([key, value]) => (
      <span key={key} className="ml-2">
        {' '}
        <span className="text-violet">{key}</span>
        <span className="text-muted-foreground">=</span>
        <span className="text-primary">"{value}"</span>
      </span>
    ));
  };

  const renderNode = (node: XmlNode, path: string): React.ReactElement => {
    if (node.type === 'text') {
      return <span className="text-foreground">{node.value}</span>;
    }

    if (node.type === 'comment') {
      return (
        <span className="text-muted-foreground">{`<!--${node.value}-->`}</span>
      );
    }

    if (node.type === 'cdata') {
      return (
        <span className="text-muted-foreground">{`<![CDATA[${node.value}]]>`}</span>
      );
    }

    const hasChildren = node.children && node.children.length > 0;
    const isExpanded = expanded.has(path);

    return (
      <div>
        <div
          className={`flex items-center ${hasChildren ? 'cursor-pointer hover:bg-accent rounded transition-colors' : ''} px-1`}
          onClick={hasChildren ? () => toggleNode(path) : undefined}
        >
          {hasChildren &&
            (isExpanded ? (
              <ChevronDown className="h-4 w-4" />
            ) : (
              <ChevronRight className="h-4 w-4" />
            ))}
          <span className="text-muted-foreground">{'<'}</span>
          <span className="text-blue">{node.name}</span>
          {node.attributes && renderAttributes(node.attributes)}
          <span className="text-muted-foreground">
            {hasChildren ? '>' : ' />'}
          </span>
        </div>

        {hasChildren && isExpanded && (
          <div className="ml-4 border-l border-border">
            {node.children?.map((child, index) => (
              <div key={index} className="py-1 ml-2">
                {renderNode(child, `${path}.${index}`)}
              </div>
            ))}
          </div>
        )}

        {hasChildren && isExpanded && (
          <div className="text-muted-foreground ml-1">
            {'</'}
            <span className="text-blue">{node.name}</span>
            {'>'}
          </div>
        )}
      </div>
    );
  };

  const parsedXml = parseXml(data);

  if (!parsedXml) {
    return <div className="text-destructive">Invalid XML string</div>;
  }

  return (
    <div className="w-full max-w-2xl">
      <div className="font-mono text-sm">{renderNode(parsedXml, 'root')}</div>
    </div>
  );
};

export default XmlRenderer;
