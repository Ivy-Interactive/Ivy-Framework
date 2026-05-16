import React, { useState } from "react";

interface ToolCall {
  name: string;
  input: Record<string, unknown>;
  result?: string;
  isError?: boolean;
}

interface ToolUseCardProps {
  tool: ToolCall;
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

const ChevronDownIcon: React.FC = () => (
  <svg
    xmlns="http://www.w3.org/2000/svg"
    viewBox="0 0 24 24"
    fill="none"
    stroke="currentColor"
    strokeWidth="2"
    strokeLinecap="round"
    strokeLinejoin="round"
    width="14"
    height="14"
    aria-hidden="true"
  >
    <path d="m6 9 6 6 6-6" />
  </svg>
);

type ToolStatus = "running" | "success" | "error";

function getToolStatus(tool: ToolCall): ToolStatus {
  if (tool.result === undefined) return "running";
  if (tool.isError) return "error";
  return "success";
}

export const ToolUseCard: React.FC<ToolUseCardProps> = ({ tool }) => {
  const status = getToolStatus(tool);
  const [open, setOpen] = useState(false);

  const handleToggle = () => setOpen((o) => !o);

  const summary = inputSummary(tool.name, tool.input);
  const headerPreview = summary ? basename(summary) : "";

  return (
    <div className={`aov-tool ${open ? "open" : ""}`}>
      <div
        className="aov-tool-header"
        onClick={handleToggle}
        role="button"
        tabIndex={0}
        aria-expanded={open}
        onKeyDown={(e) => {
          if (e.key === "Enter" || e.key === " ") {
            e.preventDefault();
            handleToggle();
          }
        }}
      >
        <span className={`aov-tool-chevron ${open ? "open" : ""}`}>
          <ChevronDownIcon />
        </span>
        <span className={`aov-tool-status aov-tool-status--${status}`} />
        <span className="aov-tool-name">{tool.name}</span>
        {headerPreview && <span className="aov-tool-preview">{headerPreview}</span>}
      </div>
      {open && (
        <div className="aov-tool-body">
          <pre className="aov-tool-pre">
            <code>{displayInput(tool.name, tool.input)}</code>
          </pre>
          {tool.result != null && (
            <>
              <hr className="aov-tool-separator" />
              <pre className="aov-tool-pre">
                <code>{tool.result}</code>
              </pre>
            </>
          )}
        </div>
      )}
    </div>
  );
};
