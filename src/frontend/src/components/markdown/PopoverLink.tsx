import React from "react";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";

interface PopoverLinkProps {
  content: string;
  children: React.ReactNode;
}

export const PopoverLink: React.FC<PopoverLinkProps> = ({ content, children }) => {
  return (
    <Popover>
      <PopoverTrigger asChild>
        <span
          className="cursor-pointer"
          style={{
            textDecoration: "underline dotted",
            textUnderlineOffset: "3px",
            textDecorationColor: "var(--primary)",
            color: "var(--primary)",
          }}
          role="button"
          tabIndex={0}
        >
          {children}
        </span>
      </PopoverTrigger>
      <PopoverContent className="w-auto max-w-xs rounded-selector bg-popover px-3 py-1.5 text-xs text-popover-foreground shadow-md">
        {content}
      </PopoverContent>
    </Popover>
  );
};
