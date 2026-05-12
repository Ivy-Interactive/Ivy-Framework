export function splitLines(jsonStream: string | undefined): string[] {
  if (!jsonStream) return [];
  return jsonStream
    .split("\n")
    .map((l) => l.trim())
    .filter((l) => l.length > 0);
}

export function tryParse(line: string): Record<string, unknown> | null {
  try {
    const v = JSON.parse(line);
    return typeof v === "object" && v !== null ? (v as Record<string, unknown>) : null;
  } catch {
    return null;
  }
}

export function contentToString(content: unknown): string {
  if (typeof content === "string") return content;
  if (Array.isArray(content)) {
    return content
      .map((c) =>
        c && typeof c === "object"
          ? ((c as Record<string, unknown>).text ??
            (c as Record<string, unknown>).content ??
            JSON.stringify(c))
          : String(c),
      )
      .join("\n");
  }
  if (content && typeof content === "object") {
    const obj = content as Record<string, unknown>;
    return (obj.text ?? obj.stdout ?? obj.content ?? JSON.stringify(content)) as string;
  }
  return String(content ?? "");
}
