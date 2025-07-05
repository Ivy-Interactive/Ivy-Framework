import { useEventHandler } from "@/components/EventHandlerContext";
import { ScrollArea } from "@/components/ui/scroll-area";
import { cn } from "@/lib/utils";
import { InternalLink } from "@/types/widgets";
import { Github } from "lucide-react";
import React, { useEffect, useState, useRef } from "react";

interface ArticleWidgetProps {
  id: string;
  children: React.ReactNode[];
  showToc?: boolean;
  showFooter?: boolean;
  previous: InternalLink;
  next: InternalLink;
  documentSource?: string;
}

export const ArticleWidget: React.FC<ArticleWidgetProps> = ({
  id,
  children,
  previous,
  next,
  documentSource,
  showFooter,
  showToc,
}) => {
  const eventHandler = useEventHandler();
  const [contentLoaded, setContentLoaded] = useState(false);
  const articleRef = useRef<HTMLElement>(null);

  // Set content as loaded after initial render
  useEffect(() => {
    // Small delay to ensure any Suspense boundaries have resolved
    const timer = setTimeout(() => {
      setContentLoaded(true);
    }, 500);

    return () => clearTimeout(timer);
  }, []);

  return (
    <div className="flex flex-col gap-8 max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 relative mt-8 ">
      <div className="flex gap-8 flex-grow">
        <article ref={articleRef} className="w-[48rem]">
          <div className="flex flex-col gap-8 flex-grow min-h-[calc(100vh+8rem)]">
            {children}
          </div>
          {showFooter && (
            <footer className="border-t py-8 mt-20">
              <div className="flex flex-col gap-6">
                <div className="flex justify-between items-center">
                  <div className="flex-1">
                    {previous && (
                      <a
                        onClick={() =>
                          eventHandler("OnLinkClick", id, [
                            "app://" + previous.appId,
                          ])
                        }
                        href={"app://" + previous.appId}
                        className="group flex flex-col gap-2 hover:text-primary transition-colors"
                      >
                        <div className="text-sm">← Previous</div>
                        <div className="font-medium text-muted-foreground">
                          {previous.title}
                        </div>
                      </a>
                    )}
                  </div>
                  <div className="flex-1 flex justify-end">
                    {next && (
                      <a
                        onClick={() =>
                          eventHandler("OnLinkClick", id, [
                            "app://" + next.appId,
                          ])
                        }
                        href={"app://" + next.appId}
                        className="group flex flex-col text-right gap-2 hover:text-primary transition-colors"
                      >
                        <div className="text-sm">Next →</div>
                        <div className="font-medium text-muted-foreground">
                          {next.title}
                        </div>
                      </a>
                    )}
                  </div>
                </div>
                {documentSource && (
                  <div className="flex justify-center">
                    <a
                      href={documentSource}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="group flex items-center gap-2 text-sm text-muted-foreground hover:text-primary transition-colors"
                    >
                      <Github className="w-4 h-4" />
                      Edit this document
                    </a>
                  </div>
                )}
              </div>
            </footer>
          )}
        </article>
        {showToc && contentLoaded && (
          <div className="hidden lg:block">
            <TableOfContents className="sticky top-8" articleRef={articleRef} />
          </div>
        )}
      </div>
    </div>
  );
};

type TocItem = {
  id: string;
  text: string;
  level: number;
};

const TableOfContents = ({
  className,
  articleRef,
}: {
  className?: string;
  articleRef: React.RefObject<HTMLElement>;
}) => {
  const [headings, setHeadings] = useState<TocItem[]>([]);
  const [activeId, setActiveId] = useState<string>("");

  useEffect(() => {
    if (!articleRef.current) return;

    const articleElement = articleRef.current;
    const elements = Array.from(
      articleElement.querySelectorAll("h1, h2, h3, h4, h5, h6")
    );

    const items = elements.map((element) => {
      // Generate ID if doesn't exist
      if (!element.id) {
        element.id =
          element.textContent
            ?.toLowerCase()
            .replace(/\s+/g, "-")
            .replace(/[^\w-]/g, "") ?? "";
      }

      return {
        id: element.id,
        text: element.textContent ?? "",
        level: parseInt(element.tagName[1]),
      };
    });

    setHeadings(items);

    const observer = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting) {
            setActiveId(entry.target.id);
          }
        });
      },
      { rootMargin: "0px 0px -80% 0px" }
    );

    elements.forEach((element) => observer.observe(element));

    return () => observer.disconnect();
  }, [articleRef]);

  return (
    <div className={cn("w-64 relative", className)}>
      <div className="font-medium mb-4">Table of Contents</div>
      <ScrollArea>
        <nav className="relative">
          {headings.map((heading) => (
            <a
              key={heading.id}
              href={`#${heading.id}`}
              className={cn(
                "block text-sm py-1 hover:text-primary transition-colors",
                heading.level === 1 ? "pl-0" : `pl-${(heading.level - 1) * 4}`,
                activeId === heading.id
                  ? "text-primary font-medium"
                  : "text-muted-foreground"
              )}
              onClick={(e) => {
                e.preventDefault();
                document.getElementById(heading.id)?.scrollIntoView({
                  behavior: "smooth",
                });
              }}
            >
              {heading.text}
            </a>
          ))}
        </nav>
      </ScrollArea>
    </div>
  );
};
