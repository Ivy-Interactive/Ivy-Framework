import React from 'react';
import { Button } from './ui/button';
import { ClipboardCopy } from 'lucide-react';

interface ExceptionProps {
  title?: string | null;
  message?: string | null;
  stackTrace?: string | null;
}

export class Exception extends React.PureComponent<ExceptionProps> {
  copyToClipboard = () => {
    const { title, message, stackTrace } = this.props;
    const errorDetails = [
      title && `Title: ${title}`,
      message && `Message: ${message}`,
      stackTrace && `Stack Trace:\n${stackTrace}`,
    ]
      .filter(Boolean)
      .join('\n\n');

    navigator.clipboard.writeText(errorDetails);
  };

  render() {
    const { title, message, stackTrace } = this.props;
    return (
      <div className="space-y-4">
        <div className="prose">
          <h2>Oops! Something went wrong :(</h2>
          {title && <h3>{title}</h3>}
          {message && <p>{message}</p>}
          {stackTrace && (
            <details className="mt-4" style={{ whiteSpace: 'pre-wrap' }} open>
              <pre>{stackTrace}</pre>
            </details>
          )}
        </div>

        <Button
          onClick={this.copyToClipboard}
          className="mt-4 flex items-center gap-2"
          variant="secondary"
        >
          <ClipboardCopy className="h-4 w-4" />
          Copy Error Details
        </Button>
      </div>
    );
  }
}
