import React, { useState } from 'react';
import { Button } from './ui/button';
import { ClipboardCopy, Check, X } from 'lucide-react';
import { Prism as SyntaxHighlighter } from 'react-syntax-highlighter';
import { createPrismTheme } from '@/lib/ivy-prism-theme';

interface ErrorDisplayProps {
  title?: string | null;
  message?: string | null;
  stackTrace?: string | null;
  onDismiss?: () => void;
}

export const ErrorDisplay: React.FC<ErrorDisplayProps> = ({
  title,
  message,
  stackTrace,
  onDismiss,
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
    <div className="bg-destructive/10 border border-destructive/20 rounded-lg p-4 shadow-lg max-w-full sm:max-w-md md:max-w-lg lg:max-w-2xl">
      <div className="flex items-start justify-between mb-4">
        <div className="flex-1">
          <h3 className="text-lg font-semibold text-destructive mb-2">
            {title || 'Exception'}
          </h3>
          {message && (
            <p className="text-sm text-foreground/90 leading-relaxed">
              {message}
            </p>
          )}
        </div>
        {onDismiss && (
          <Button
            variant="ghost"
            size="sm"
            onClick={onDismiss}
            className="h-8 w-8 p-0 ml-2 text-foreground/50 hover:text-foreground hover:bg-destructive/20 transition-colors cursor-pointer"
          >
            <X className="h-4 w-4" />
          </Button>
        )}
      </div>

      {stackTrace && (
        <div className="mb-4">
          <h4 className="text-sm font-medium mb-2 text-foreground">
            Stack Trace
          </h4>
          <div className="bg-muted/50 border border-border rounded-md overflow-hidden">
            <SyntaxHighlighter
              language="csharp"
              style={createPrismTheme()}
              wrapLongLines={false}
              showLineNumbers={false}
              customStyle={{
                margin: 0,
                borderRadius: 0,
                fontSize: '0.75rem',
                lineHeight: '1.4',
              }}
            >
              {stackTrace}
            </SyntaxHighlighter>
          </div>
        </div>
      )}

      <Button
        onClick={copyToClipboard}
        className="flex items-center gap-2 cursor-pointer"
        variant="outline"
        size="sm"
      >
        {copied ? (
          <Check className="h-4 w-4 text-primary" />
        ) : (
          <ClipboardCopy className="h-4 w-4" />
        )}
        Copy Details
      </Button>
    </div>
  );
};
