import { ChevronRight, ChevronDown } from "lucide-react";
import { useState } from "react";

interface XmlRendererProps {
  data: string;
}

interface XmlNode {
  type: "element" | "text" | "cdata" | "comment";
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
      const xmlDoc = parser.parseFromString(xmlString, "text/xml");

      if (xmlDoc.getElementsByTagName("parsererror").length > 0) {
        throw new Error("Invalid XML");
      }

      const convertDomToNode = (domNode: Node): XmlNode | null => {
        if (domNode.nodeType === Node.TEXT_NODE) {
          const text = domNode.textContent?.trim() || "";
          return text ? { type: "text", value: text } : null;
        }

        if (domNode.nodeType === Node.COMMENT_NODE) {
          return {
            type: "comment",
            value: domNode.textContent || "",
          };
        }

        if (domNode.nodeType === Node.CDATA_SECTION_NODE) {
          return {
            type: "cdata",
            value: domNode.textContent || "",
          };
        }

        if (domNode.nodeType === Node.ELEMENT_NODE) {
          const element = domNode as Element;
          const attributes: Record<string, string> = {};

          element.getAttributeNames().forEach((attr) => {
            attributes[attr] = element.getAttribute(attr) || "";
          });

          const children = Array.from(element.childNodes)
            .map((child) => convertDomToNode(child))
            .filter((node): node is XmlNode => node !== null);

          return {
            type: "element",
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
        {" "}
        <span className="text-purple-600">{key}</span>
        <span className="text-gray-500">=</span>
        <span className="text-green-600">"{value}"</span>
      </span>
    ));
  };

  const renderNode = (node: XmlNode, path: string): JSX.Element => {
    if (node.type === "text") {
      return <span className="text-gray-800">{node.value}</span>;
    }

    if (node.type === "comment") {
      return <span className="text-gray-500">{`<!--${node.value}-->`}</span>;
    }

    if (node.type === "cdata") {
      return (
        <span className="text-gray-600">{`<![CDATA[${node.value}]]>`}</span>
      );
    }

    const hasChildren = node.children && node.children.length > 0;
    const isExpanded = expanded.has(path);

    return (
      <div>
        <div
          className={`flex items-center ${
            hasChildren ? "cursor-pointer hover:bg-gray-100 rounded" : ""
          } px-1`}
          onClick={hasChildren ? () => toggleNode(path) : undefined}
        >
          {hasChildren &&
            (isExpanded ? (
              <ChevronDown className="h-4 w-4" />
            ) : (
              <ChevronRight className="h-4 w-4" />
            ))}
          <span className="text-gray-500">{"<"}</span>
          <span className="text-blue-600">{node.name}</span>
          {node.attributes && renderAttributes(node.attributes)}
          <span className="text-gray-500">{hasChildren ? ">" : " />"}</span>
        </div>

        {hasChildren && isExpanded && (
          <div className="ml-4 border-l border-gray-200">
            {node.children?.map((child, index) => (
              <div key={index} className="py-1 ml-2">
                {renderNode(child, `${path}.${index}`)}
              </div>
            ))}
          </div>
        )}

        {hasChildren && isExpanded && (
          <div className="text-gray-500 ml-1">
            {"</"}
            <span className="text-blue-600">{node.name}</span>
            {">"}
          </div>
        )}
      </div>
    );
  };

  const parsedXml = parseXml(data);

  if (!parsedXml) {
    return <div className="text-red-600">Invalid XML string</div>;
  }

  return (
    <div className="w-full max-w-2xl">
      <div className="font-mono text-sm">{renderNode(parsedXml, "root")}</div>
    </div>
  );
};

export default XmlRenderer;
