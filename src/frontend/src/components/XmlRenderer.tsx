import { useState } from 'react';
import { ChevronRight, ChevronDown } from 'lucide-react';

interface XmlRendererProps {
  data: string;
  initialExpanded?: number | null;
}

interface XmlNode {
  type: 'element' | 'text' | 'cdata' | 'comment';
  name?: string;
  attributes?: Record<string, string>;
  children?: XmlNode[];
  value?: string;
}

const renderAttributes = (attributes: Record<string, string>) => {
  return Object.entries(attributes).map(([key, value]) => (
    <span key={key} className="ml-2">
      {' '}
      <span className="text-purple">{key}</span>
      <span className="text-muted-foreground">=</span>
      <span className="text-primary">"{value}"</span>
    </span>
  ));
};

interface XmlNodeComponentProps {
  node: XmlNode;
  path: string;
  expanded: Set<unknown>;
  toggleNode: (path: string) => void;
}

const XmlNodeComponent = ({
  node,
  path,
  expanded,
  toggleNode,
}: XmlNodeComponentProps): React.ReactElement => {
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

  if (hasChildren) {
    return (
      <div>
        <div
          className="flex items-center cursor-pointer hover:bg-accent rounded transition-colors px-1"
          onClick={() => toggleNode(path)}
          role="button"
          tabIndex={0}
          onKeyDown={e => {
            if (e.key === 'Enter' || e.key === ' ') {
              e.preventDefault();
              toggleNode(path);
            }
          }}
        >
          {isExpanded ? (
            <ChevronDown className="h-4 w-4" />
          ) : (
            <ChevronRight className="h-4 w-4" />
          )}
          <span className="text-muted-foreground">{'<'}</span>
          <span className="text-cyan">{node.name}</span>
          {node.attributes && renderAttributes(node.attributes)}
          <span className="text-muted-foreground">{'>'}</span>
        </div>

        {isExpanded && (
          <div className="ml-3 border-l border-border">
            {node.children?.map((child, i) => {
              const childKey = `xml-node-${i}`;
              return (
                <div key={childKey} className="py-1 ml-2">
                  <XmlNodeComponent
                    node={child}
                    path={`${path}.${i}`}
                    expanded={expanded}
                    toggleNode={toggleNode}
                  />
                </div>
              );
            })}
          </div>
        )}

        {isExpanded && (
          <div className="text-muted-foreground ml-1">
            {'</'}
            <span className="text-cyan">{node.name}</span>
            {'>'}
          </div>
        )}
      </div>
    );
  }

  return (
    <div>
      <div className="flex items-center px-1" role="presentation">
        <span className="text-muted-foreground">{'<'}</span>
        <span className="text-cyan">{node.name}</span>
        {node.attributes && renderAttributes(node.attributes)}
        <span className="text-muted-foreground">{' />'}</span>
      </div>
    </div>
  );
};

function collectXmlPaths(
  node: XmlNode,
  path: string,
  maxDepth: number,
  currentDepth: number
): string[] {
  if (currentDepth >= maxDepth) return [];
  if (!node.children || node.children.length === 0) return [];

  const paths = [path];
  node.children.forEach((child, i) => {
    if (child.type === 'element') {
      paths.push(
        ...collectXmlPaths(child, `${path}.${i}`, maxDepth, currentDepth + 1)
      );
    }
  });
  return paths;
}

function collectAllXmlPaths(node: XmlNode, path: string): string[] {
  if (!node.children || node.children.length === 0) return [];

  const paths = [path];
  node.children.forEach((child, i) => {
    if (child.type === 'element') {
      paths.push(...collectAllXmlPaths(child, `${path}.${i}`));
    }
  });
  return paths;
}

export const XmlRenderer = ({ data, initialExpanded }: XmlRendererProps) => {
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

  const parsedXml = parseXml(data);

  const getInitialExpanded = () => {
    if (!parsedXml || initialExpanded === null || initialExpanded === undefined)
      return new Set<string>();
    if (initialExpanded === -1)
      return new Set(collectAllXmlPaths(parsedXml, 'root'));
    if (initialExpanded === 0) return new Set<string>();
    return new Set(collectXmlPaths(parsedXml, 'root', initialExpanded, 0));
  };

  const [expanded, setExpanded] = useState(getInitialExpanded);

  const toggleNode = (path: string) => {
    const newExpanded = new Set(expanded);
    if (newExpanded.has(path)) {
      newExpanded.delete(path);
    } else {
      newExpanded.add(path);
    }
    setExpanded(newExpanded);
  };

  if (!parsedXml) {
    return <div className="text-destructive">Invalid XML string</div>;
  }

  return (
    <div className="w-full max-w-2xl">
      <div className="font-mono text-sm">
        <XmlNodeComponent
          node={parsedXml}
          path="root"
          expanded={expanded}
          toggleNode={toggleNode}
        />
      </div>
    </div>
  );
};

export default XmlRenderer;
