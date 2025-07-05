import { ChevronRight, ChevronDown } from "lucide-react";
import { useState } from "react";

interface JsonRendererProps {
  data: any;
}

export const JsonRenderer = ({ data }: JsonRendererProps) => {
  const [expanded, setExpanded] = useState(new Set());

  let parsedData = data;
  if (typeof data === "string") {
    try {
      parsedData = JSON.parse(data);
    } catch (error) {
      return <div className="text-red-600">Invalid JSON string</div>;
    }
  }

  const toggleNode = (path: string) => {
    const newExpanded = new Set(expanded);
    if (newExpanded.has(path)) {
      newExpanded.delete(path);
    } else {
      newExpanded.add(path);
    }
    setExpanded(newExpanded);
  };

  const renderValue = (value: any, path: string): JSX.Element => {
    if (value === null) return <span className="text-gray-500">null</span>;
    if (typeof value === "boolean")
      return <span className="text-purple-600">{value.toString()}</span>;
    if (typeof value === "number")
      return <span className="text-blue-600">{value}</span>;
    if (typeof value === "string")
      return <span className="text-green-600">"{value}"</span>;

    const isArray = Array.isArray(value);
    const isEmpty = Object.keys(value).length === 0;

    if (isEmpty) {
      return <span className="text-gray-500">{isArray ? "[]" : "{}"}</span>;
    }

    const isExpanded = expanded.has(path);

    return (
      <div>
        <div
          onClick={() => toggleNode(path)}
          className="flex items-center cursor-pointer hover:bg-gray-100 rounded px-1"
        >
          {isExpanded ? (
            <ChevronDown className="h-4 w-4" />
          ) : (
            <ChevronRight className="h-4 w-4" />
          )}
          <span className="text-gray-500">{isArray ? "[" : "{"}</span>
        </div>

        {isExpanded && (
          <div className="ml-4 border-l border-gray-200">
            {Object.entries(value).map(([key, val]) => (
              <div key={key} className="py-1 ml-2">
                <span className="text-red-600">
                  {isArray ? "" : `"${key}"`}
                </span>
                {!isArray && <span className="text-gray-500">: </span>}
                {renderValue(val, `${path}.${key}`)}
              </div>
            ))}
          </div>
        )}

        {isExpanded && (
          <div className="text-gray-500 ml-1">{isArray ? "]" : "}"}</div>
        )}
      </div>
    );
  };

  return (
    <div className="w-full max-w-2xl">
      <div className="font-mono text-sm">{renderValue(parsedData, "root")}</div>
    </div>
  );
};
