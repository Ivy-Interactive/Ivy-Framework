import { useState } from 'react';
import { ChevronRight, ChevronDown } from 'lucide-react';

interface JsonRendererProps {
  data: unknown;
}

export const JsonRenderer = ({ data }: JsonRendererProps) => {
  const [expanded, setExpanded] = useState(new Set());

  let parsedData = data;
  if (typeof data === 'string') {
    try {
      parsedData = JSON.parse(data);
    } catch (error) {
      console.error(error);
      return <div className="text-destructive">Invalid JSON string</div>;
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

  const renderValue = (value: unknown, path: string): React.ReactElement => {
    if (value === null)
      return <span className="text-muted-foreground">null</span>;
    if (typeof value === 'boolean')
      return <span className="text-purple">{value.toString()}</span>;
    if (typeof value === 'number')
      return <span className="text-cyan">{value}</span>;
    if (typeof value === 'string')
      return <span className="text-primary">"{value}"</span>;

    const isArray = Array.isArray(value);
    const isEmpty =
      value && typeof value === 'object' && Object.keys(value).length === 0;

    if (isEmpty) {
      return (
        <span className="text-muted-foreground">{isArray ? '[]' : '{}'}</span>
      );
    }

    const isExpanded = expanded.has(path);

    return (
      <div>
        <div
          onClick={() => toggleNode(path)}
          className="flex items-center cursor-pointer hover:bg-accent rounded px-1 transition-colors"
        >
          {isExpanded ? (
            <ChevronDown className="h-4 w-4" />
          ) : (
            <ChevronRight className="h-4 w-4" />
          )}
          <span className="text-muted-foreground">{isArray ? '[' : '{'}</span>
        </div>

        {isExpanded && (
          <div className="ml-4 border-l border-border">
            {value &&
              typeof value === 'object' &&
              Object.entries(value).map(([key, val]) => (
                <div key={key} className="py-1 ml-2">
                  <span className="text-cyan">{isArray ? '' : `"${key}"`}</span>
                  {!isArray && (
                    <span className="text-muted-foreground">: </span>
                  )}
                  {renderValue(val, `${path}.${key}`)}
                </div>
              ))}
          </div>
        )}

        {isExpanded && (
          <div className="text-muted-foreground ml-1">
            {isArray ? ']' : '}'}
          </div>
        )}
      </div>
    );
  };

  return (
    <div className="w-full max-w-2xl">
      <div className="font-mono text-sm">{renderValue(parsedData, 'root')}</div>
    </div>
  );
};
