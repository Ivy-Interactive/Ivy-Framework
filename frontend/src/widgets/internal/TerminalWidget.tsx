import { cn } from '@/lib/utils';
import { Circle } from 'lucide-react';

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
  const outputColor = 'text-gray-300';

  return (
    <div
      className={cn(
        'rounded-lg overflow-hidden border border-border shadow-md'
      )}
    >
      {showHeader && (
        <div className="bg-zinc-800 px-4 py-2 flex items-center">
          <div className="flex items-center gap-1.5">
            <Circle className="h-3 w-3 fill-red-500 text-red-500" />
            <Circle className="h-3 w-3 fill-yellow-500 text-yellow-500" />
            <Circle className="h-3 w-3 fill-green-500 text-green-500" />
          </div>
          <div className="text-zinc-400 text-sm font-medium flex-1 text-center">
            {title}
          </div>
          <div className="w-[70px]"></div>
        </div>
      )}
      <div className="bg-zinc-900 p-4 font-mono text-sm overflow-x-auto">
        {lines.map((line, index) => (
          <div
            key={index}
            className={cn('whitespace-pre-wrap', index > 0 ? 'mt-1' : '')}
          >
            <div className="flex pl-2">
              {line.isCommand && (
                <span className="text-primary select-none pointer-events-none mr-2">
                  {'> '}
                </span>
              )}
              {!line.isCommand && <div style={{ paddingLeft: '25px' }}></div>}
              <span className={line.isCommand ? commandColor : outputColor}>
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
