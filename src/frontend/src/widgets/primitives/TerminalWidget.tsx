import { cn } from "@/lib/utils";
import CopyToClipboardButton from "@/components/CopyToClipboardButton";

const EMPTY_ARRAY: never[] = [];

export interface TerminalLine {
  content: string;
  isCommand?: boolean;
  prompt?: string;
}

export interface TerminalWidgetProps {
  lines: TerminalLine[];
  title?: string;
  showHeader?: boolean;
  showCopyButton?: boolean;
}

const TerminalWidget = ({
  lines = EMPTY_ARRAY,
  title,
  showHeader = true,
  showCopyButton = true,
}: TerminalWidgetProps) => {
  const commandColor = "text-white";
  const outputColor = "text-muted-foreground";

  const commandsText = lines
    .filter((line) => line.isCommand)
    .map((line) => line.content)
    .join("\n");
  const hasCommands = commandsText.length > 0;

  return (
    <div
      role="terminal"
      className={cn("rounded-lg overflow-hidden border border-border shadow-md")}
    >
      {showHeader && (
        <div className="bg-zinc-800 px-4 py-2 flex items-center">
          <div className="flex gap-1">
            <div className="w-3 h-3 bg-red-500 rounded-full"></div>
            <div className="w-3 h-3 bg-yellow-500 rounded-full"></div>
            <div className="w-3 h-3 bg-green-500 rounded-full"></div>
          </div>
          <div className="text-zinc-400 text-body font-medium flex-1 text-center">{title}</div>
        </div>
      )}
      <div className="relative">
        {showCopyButton && hasCommands && (
          <div className="absolute top-2 right-2 z-50">
            <CopyToClipboardButton
              textToCopy={commandsText}
              className="bg-zinc-800 text-white hover:bg-zinc-700"
            />
          </div>
        )}
        <div className="bg-zinc-900 p-4 font-mono text-body overflow-x-auto slim-scrollbar">
          {lines.map((line, i) => {
            const lineKey = `term-line-${i}`;
            return (
              <div
                key={lineKey}
                role="log"
                aria-label={line.isCommand ? "Command" : "Output"}
                className={cn("whitespace-pre-wrap", i > 0 ? "mt-1" : "")}
              >
                <div className="flex">
                  <div className="w-8 flex-shrink-0 relative flex items-start mt-1">
                    {line.isCommand ? (
                      <span className="text-primary select-none pointer-events-none w-full text-center leading-none -mt-0.5">
                        {">"}
                      </span>
                    ) : (
                      <span className="text-primary select-none pointer-events-none w-full text-center leading-none">
                        {" "}
                      </span>
                    )}
                  </div>
                  <span
                    role="terminal-text"
                    className={cn("text-sm", line.isCommand ? commandColor : outputColor)}
                  >
                    {line.content}
                  </span>
                </div>
              </div>
            );
          })}
        </div>
      </div>
    </div>
  );
};

export default TerminalWidget;
