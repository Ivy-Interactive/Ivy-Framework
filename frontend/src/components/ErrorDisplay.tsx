import React, { useState } from 'react';
import { Button } from './ui/button';
import { ClipboardCopy, Check } from 'lucide-react';
import { Prism as SyntaxHighlighter } from 'react-syntax-highlighter';
import { createPrismTheme } from '@/lib/ivy-prism-theme';

interface ErrorDisplayProps {
  title?: string | null;
  message?: string | null;
  stackTrace?: string | null;
}

export const ErrorDisplay: React.FC<ErrorDisplayProps> = ({
  title,
  message,
  stackTrace,
}) => {
  const [copied, setCopied] = useState(false);

  const copyToClipboard = () => {
    const errorDetails = [
      title && `Title: ${title}`,
      message && `Message: ${message}`,
      stackTrace && `Stack Trace:\n${stackTrace}`,
    ]
      .filter(Boolean)
      .join('\n\n');

    navigator.clipboard.writeText(errorDetails);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  return (
    <div className="space-y-4">
      {title && (
        <div>
          <h4 className="text-sm font-medium mb-2">Type</h4>
          <p>{title}</p>
        </div>
      )}

      {message && (
        <div>
          <h4 className="text-sm font-medium mb-2">Message</h4>
          <p>{message}</p>
        </div>
      )}

      {stackTrace && (
        <div>
          <h4 className="text-sm font-medium mb-2">Stack Trace</h4>
          <SyntaxHighlighter
            language="csharp"
            style={createPrismTheme()}
            wrapLongLines={false}
            showLineNumbers={false}
          >
            {stackTrace}
          </SyntaxHighlighter>
        </div>
      )}

      <Button
        onClick={copyToClipboard}
        className="mt-4 flex items-center gap-2"
        variant="outline"
      >
        {copied ? (
          <Check className="h-4 w-4 text-primary animate-in fade-in duration-500" />
        ) : (
          <ClipboardCopy className="h-4 w-4" />
        )}
        Copy Details
      </Button>
    </div>
  );
};
