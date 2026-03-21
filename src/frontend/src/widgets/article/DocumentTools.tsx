import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { TooltipProvider } from "@/components/ui/tooltip";
import { useToast } from "@/hooks/use-toast";
import { Copy, Download, ChevronDown } from "lucide-react";
import React from "react";

interface DocumentToolsProps {
  articleRef: React.RefObject<HTMLElement | null>;
  documentSource?: string;
  title?: string;
}

export const DocumentTools: React.FC<DocumentToolsProps> = ({
  articleRef,
  documentSource,
  title = "document",
}) => {
  const { toast } = useToast();

  const generateFileName = (): string => {
    if (title && title !== "document") {
      return title
        .toLowerCase()
        .replace(/\s+/g, "-")
        .replace(/[^a-z0-9-]/g, "");
    }

    if (articleRef.current) {
      const firstHeading = articleRef.current.querySelector("h1");
      if (firstHeading?.textContent) {
        return firstHeading.textContent
          .toLowerCase()
          .replace(/\s+/g, "-")
          .replace(/[^a-z0-9-]/g, "")
          .substring(0, 50);
      }
    }

    if (documentSource) {
      try {
        const url = new URL(documentSource);
        const pathParts = url.pathname.split("/");
        const fileName = pathParts[pathParts.length - 1];

        if (fileName && fileName.includes(".")) {
          return fileName
            .split(".")[0]
            .toLowerCase()
            .replace(/[^a-z0-9-]/g, "-");
        }
      } catch {
        // Ignore URL parsing errors
      }
    }

    const timestamp = new Date().toISOString().split("T")[0];
    return `ivy-document-${timestamp}`;
  };

  const fetchMarkdown = async (): Promise<string> => {
    const path = window.location.pathname;
    const cleanPath = path.endsWith("/") ? path.slice(0, -1) : path;

    if (!cleanPath || cleanPath === "") {
      throw new Error("No markdown available for the root page");
    }

    const url = `${cleanPath}.md`;

    const response = await fetch(url);

    if (!response.ok) {
      throw new Error(`Failed to fetch markdown: ${response.statusText || response.status}`);
    }

    return await response.text();
  };

  const stripMarkdownLinks = (markdown: string): string =>
    markdown.replace(/\[([^\]]*)\]\([^)]*\)/g, "$1");

  const copyTextContent = async () => {
    try {
      toast({
        title: "Loading Content...",
        description: "Fetching markdown from server...",
      });

      const rawContent = await fetchMarkdown();

      if (!rawContent.trim()) {
        toast({
          title: "Copy Failed",
          description: "No content found to copy",
          variant: "destructive",
        });
        return;
      }

      const content = stripMarkdownLinks(rawContent);
      await navigator.clipboard.writeText(content);
      toast({
        title: "Copied!",
        description: "Markdown copied to clipboard",
      });
    } catch (error) {
      console.error("Error copying markdown:", error);
      toast({
        title: "Copy Failed",
        description: error instanceof Error ? error.message : "Failed to copy content",
        variant: "destructive",
      });
    }
  };

  const saveAsMarkdown = async () => {
    try {
      toast({
        title: "Preparing Download...",
        description: "Fetching markdown from server...",
      });

      const rawContent = await fetchMarkdown();

      if (!rawContent.trim()) {
        toast({
          title: "Download Failed",
          description: "No content found to download",
          variant: "destructive",
        });
        return;
      }

      const content = stripMarkdownLinks(rawContent);
      // Create and download the file
      const blob = new Blob([content], { type: "text/markdown" });
      const url = URL.createObjectURL(blob);
      const link = document.createElement("a");
      link.href = url;
      link.download = `${generateFileName()}.md`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      URL.revokeObjectURL(url);

      toast({
        title: "Downloaded!",
        description: "Document saved as markdown file",
      });
    } catch (error) {
      console.error("Error downloading markdown:", error);
      toast({
        title: "Download Failed",
        description: error instanceof Error ? error.message : "Failed to download markdown",
        variant: "destructive",
      });
    }
  };

  return (
    <TooltipProvider>
      <div className="flex">
        <Button
          variant="ghost"
          size="sm"
          onClick={copyTextContent}
          className="h-8 px-2 flex items-center gap-1 rounded-r-none border-r border-border/50"
        >
          <Copy className="w-4 h-4" />
          <span className="text-xs">Copy Page</span>
        </Button>

        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button
              variant="ghost"
              size="sm"
              className="h-8 px-1 flex items-center gap-1 rounded-l-none"
            >
              <ChevronDown className="w-4 h-4" />
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end" className="w-48">
            <DropdownMenuItem onClick={saveAsMarkdown}>
              <Download className="w-4 h-4 mr-2" />
              Download as Markdown
            </DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenu>
      </div>
    </TooltipProvider>
  );
};
