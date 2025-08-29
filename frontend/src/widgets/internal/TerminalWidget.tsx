import { cn } from '@/lib/utils';
import CopyToClipboardButton from '@/components/CopyToClipboardButton';

export interface TerminalLine {
  content: string;
  isCommand?: boolean;
  prompt?: string;
}

export interface TerminalWidgetProps {
  lines: TerminalLine[];
  title?: string;
  showHeader?: boolean;
}

const TerminalWidget = ({ lines, title, showHeader }: TerminalWidgetProps) => {
  const commandColor = 'text-white';
  const outputColor = 'text-muted-foreground';

  // Create a copyable text representation of the terminal content
  const copyableContent = lines
    .map(line => (line.isCommand ? `> ${line.content}` : line.content))
    .join('\n');

  return (
    <div
      className={cn(
        'rounded-lg overflow-hidden border border-border shadow-md relative'
      )}
    >
      {/* Copy button positioned in the top-right corner */}
      <div className="absolute top-2 right-2 z-10">
        <CopyToClipboardButton textToCopy={copyableContent} />
      </div>

      {showHeader && (
        <div className="bg-zinc-800 px-4 py-2 flex items-center">
          <div className="text-zinc-400 text-body font-medium flex-1 text-center">
            {title}
          </div>
          <div className="flex gap-1">
            <div className="w-3 h-3 bg-red-500 rounded-full"></div>
            <div className="w-3 h-3 bg-yellow-500 rounded-full"></div>
            <div className="w-3 h-3 bg-green-500 rounded-full"></div>
          </div>
        </div>
      )}

      {/* Hidden copyable content for better accessibility and copying */}
      <div className="sr-only">
        <pre>{copyableContent}</pre>
      </div>

      <div className="bg-zinc-900 p-4 font-mono text-body overflow-x-auto">
        {lines.map((line, index) => (
          <div
            key={index}
            className={cn('whitespace-pre-wrap', index > 0 ? 'mt-1' : '')}
          >
            <div className="flex">
              <div className="w-8 flex-shrink-0 relative flex items-center">
                {line.isCommand ? (
                  <span className="text-primary select-none pointer-events-none w-full text-center leading-none -mt-0.5">
                    {'>'}
                  </span>
                ) : (
                  <span className="text-primary select-none pointer-events-none w-full text-center leading-none">
                    {' '}
                  </span>
                )}
              </div>
              <span
                className={cn(
                  'text-sm select-text',
                  line.isCommand ? commandColor : outputColor
                )}
              >
                {line.content}
              </span>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

export default TerminalWidget;
