import React, { useState } from "react";

interface ToolUseCardProps {
  name: string;
  input: Record<string, unknown>;
  result?: string;
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

export const ToolUseCard: React.FC<ToolUseCardProps> = ({ name, input, result }) => {
  const [open, setOpen] = useState(false);
  const inputDisplay = displayInput(name, input);
  const resultPreview =
    result != null ? (result.length > 120 ? result.slice(0, 120) + "…" : result) : null;

  return (
    <div className="aov-tool">
      <div
        className="aov-tool-header"
        onClick={() => setOpen((o) => !o)}
        role="button"
        tabIndex={0}
        onKeyDown={(e) => {
          if (e.key === "Enter" || e.key === " ") {
            e.preventDefault();
            setOpen((o) => !o);
          }
        }}
      >
        <span className={`aov-tool-chevron ${open ? "open" : ""}`}>▸</span>
        <span className="aov-tool-name">{name}</span>
        {resultPreview != null && <span className="aov-tool-preview">{resultPreview}</span>}
        {result == null && <span className="aov-tool-running">running…</span>}
      </div>
      {open && (
        <div className="aov-tool-body">
          <div className="aov-tool-section-label">$ input</div>
          <pre className="aov-tool-pre">
            <code>{inputDisplay}</code>
          </pre>
          {result != null && (
            <>
              <div className="aov-tool-section-label">› output</div>
              <pre className="aov-tool-pre">
                <code>{result}</code>
              </pre>
            </>
          )}
        </div>
      )}
    </div>
  );
};
