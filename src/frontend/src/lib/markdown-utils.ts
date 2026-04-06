import React from "react";

export type GitHubAlertType = "NOTE" | "TIP" | "IMPORTANT" | "WARNING" | "CAUTION";

export interface GitHubAlertStyle {
  icon: string;
  className: string;
  iconColor: string;
  title: string;
}

export const githubAlertStyles: Record<GitHubAlertType, GitHubAlertStyle> = {
  NOTE: {
    icon: "Info",
    className: "border-cyan/20 bg-cyan/10 text-foreground",
    iconColor: "text-cyan",
    title: "Note",
  },
  TIP: {
    icon: "Lightbulb",
    className: "border-emerald/20 bg-emerald/10 text-foreground",
    iconColor: "text-emerald",
    title: "Tip",
  },
  IMPORTANT: {
    icon: "MessageSquare",
    className: "border-purple/20 bg-purple/10 text-foreground",
    iconColor: "text-purple",
    title: "Important",
  },
  WARNING: {
    icon: "TriangleAlert",
    className: "border-amber/20 bg-amber/10 text-foreground",
    iconColor: "text-amber",
    title: "Warning",
  },
  CAUTION: {
    icon: "OctagonAlert",
    className: "border-destructive/20 bg-destructive/10 text-foreground",
    iconColor: "text-destructive",
    title: "Caution",
  },
};

const ALERT_TYPES = new Set<string>(["NOTE", "TIP", "IMPORTANT", "WARNING", "CAUTION"]);

function extractTextContent(node: React.ReactNode): string {
  if (typeof node === "string") return node;
  if (typeof node === "number") return String(node);
  if (!node) return "";
  if (Array.isArray(node)) return node.map(extractTextContent).join("");
  if (React.isValidElement(node)) {
    const props = node.props as { children?: React.ReactNode };
    return extractTextContent(props.children);
  }
  return "";
}

/**
 * Strips the `[!TYPE]` marker from the first text node in a React element's children,
 * preserving any remaining inline elements (bold, links, code, etc.).
 */
function stripAlertMarker(children: React.ReactNode, markerLength: number): React.ReactNode {
  const childArray = React.Children.toArray(children);
  if (childArray.length === 0) return children;

  const first = childArray[0];
  if (typeof first === "string") {
    const stripped = first.slice(markerLength);
    // If the stripped text is empty or only whitespace, drop it
    const remaining = stripped.replace(/^\n/, "");
    if (remaining.length === 0) {
      return childArray.slice(1);
    }
    return [remaining, ...childArray.slice(1)];
  }
  // If first child is an element, the marker text must be nested inside it
  return children;
}

export interface ParsedGitHubAlert {
  type: GitHubAlertType;
  content: React.ReactNode;
}

/**
 * Parses React children of a blockquote to detect GitHub alert syntax.
 *
 * GitHub alerts follow this pattern:
 * ```
 * > [!NOTE]
 * > Content here
 * ```
 *
 * In react-markdown, this becomes a blockquote with paragraph children.
 * The first paragraph's text content starts with `[!TYPE]\n`.
 */
export function parseGitHubAlert(children: React.ReactNode): ParsedGitHubAlert | null {
  const childArray = React.Children.toArray(children);
  if (childArray.length === 0) return null;

  // Find the first paragraph element
  const firstChild = childArray[0];
  if (!React.isValidElement(firstChild)) return null;

  // react-markdown renders blockquote children as <p> elements
  // The type could be 'p' string or a component
  const elementType = firstChild.type;
  if (typeof elementType !== "string" && typeof elementType !== "function") return null;

  const firstProps = firstChild.props as { children?: React.ReactNode };
  const textContent = extractTextContent(firstProps.children);

  // Match [!TYPE] at start, followed by newline or end of text
  const match = textContent.match(/^\[!(NOTE|TIP|IMPORTANT|WARNING|CAUTION)\]\s*/);
  if (!match) return null;

  const type = match[1] as GitHubAlertType;
  if (!ALERT_TYPES.has(type)) return null;

  // Strip the marker from the first paragraph's children
  const strippedFirstChildren = stripAlertMarker(firstProps.children, match[0].length);

  // Check if first paragraph has remaining content after stripping
  const hasRemainingContent =
    React.Children.toArray(strippedFirstChildren).length > 0 &&
    extractTextContent(strippedFirstChildren).trim().length > 0;

  // Reconstruct content: modified first paragraph + remaining children
  let content: React.ReactNode;
  if (hasRemainingContent) {
    // Clone the first paragraph with stripped children
    const modifiedFirst = React.cloneElement(firstChild, {}, strippedFirstChildren);
    content = childArray.length > 1 ? [modifiedFirst, ...childArray.slice(1)] : modifiedFirst;
  } else {
    // First paragraph only had the marker, use remaining paragraphs
    content = childArray.length > 1 ? childArray.slice(1) : null;
  }

  return { type, content };
}
