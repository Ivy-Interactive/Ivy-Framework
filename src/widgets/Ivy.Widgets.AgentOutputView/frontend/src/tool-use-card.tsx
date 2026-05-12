import React, { ReactNode, useState } from "react";

interface ToolCall {
  name: string;
  input: Record<string, unknown>;
  result?: string;
}

interface ToolUseCardProps {
  tools: ToolCall[];
  children?: ReactNode;
}

function displayInput(name: string, input: Record<string, unknown>): string {
  if (name === "Bash" && typeof input.command === "string") return input.command;
  if ((name === "Write" || name === "Edit") && typeof input.file_path === "string") {
    let s = `File: ${input.file_path}`;
    if (typeof input.content === "string") {
      s += `\n${input.content.slice(0, 500)}${input.content.length > 500 ? "\n…" : ""}`;
    }
    return s;
  }
  if (name === "Read" && typeof input.file_path === "string") return `File: ${input.file_path}`;
  return JSON.stringify(input, null, 2);
}

function inputSummary(name: string, input: Record<string, unknown>): string {
  if (name === "Bash" && typeof input.command === "string") return input.command;
  if (typeof input.file_path === "string") return input.file_path;
  if (typeof input.path === "string") return input.path;
  if (typeof input.pattern === "string") return input.pattern;
  return "";
}

function basename(p: string): string {
  if (!p) return p;
  const i = Math.max(p.lastIndexOf("/"), p.lastIndexOf("\\"));
  return i >= 0 ? p.slice(i + 1) : p;
}

export const ToolUseCard: React.FC<ToolUseCardProps> = ({ tools, children }) => {
  const active = tools.some((t) => t.result === undefined);
  const [open, setOpen] = useState(true);

  const handleToggle = () => setOpen((o) => !o);

  const name = tools[0].name;
  const count = tools.length;
  const last = tools[tools.length - 1];

  // Header preview: prefer the last tool's result, else its input summary
  let headerPreview = "";
  if (last.result != null) {
    headerPreview = last.result.length > 120 ? last.result.slice(0, 120) + "…" : last.result;
  } else {
    const s = inputSummary(last.name, last.input);
    if (s) headerPreview = basename(s);
  }

  return (
    <div className="aov-tool">
      <div
        className="aov-tool-header"
        onClick={handleToggle}
        role="button"
        tabIndex={0}
        onKeyDown={(e) => {
          if (e.key === "Enter" || e.key === " ") {
            e.preventDefault();
            handleToggle();
          }
        }}
      >
        <span className={`aov-tool-chevron ${open ? "open" : ""}`}>▸</span>
        <span className="aov-tool-name">{name}</span>
        {count > 1 && <span className="aov-tool-count">×{count}</span>}
        {headerPreview && <span className="aov-tool-preview">{headerPreview}</span>}
        {active && <span className="aov-tool-running">running…</span>}
      </div>
      {open && (
        <div className="aov-tool-body">
          {tools.map((t, i) => (
            <ToolEntry key={i} index={i} total={count} tool={t} />
          ))}
          {children}
        </div>
      )}
    </div>
  );
};

const ToolEntry: React.FC<{ index: number; total: number; tool: ToolCall }> = ({
  index,
  total,
  tool,
}) => {
  const inputDisplay = displayInput(tool.name, tool.input);
  const pending = tool.result === undefined;
  return (
    <div className={`aov-tool-entry ${pending ? "pending" : ""}`}>
      {total > 1 && (
        <div className="aov-tool-entry-marker">
          {tool.name} {index + 1}/{total}
          {pending && <span className="aov-tool-entry-pending"> · running…</span>}
        </div>
      )}
      <div className="aov-tool-section-label">$ input</div>
      <pre className="aov-tool-pre">
        <code>{inputDisplay}</code>
      </pre>
      {tool.result != null && (
        <>
          <div className="aov-tool-section-label">› output</div>
          <pre className="aov-tool-pre">
            <code>{tool.result}</code>
          </pre>
        </>
      )}
    </div>
  );
};
