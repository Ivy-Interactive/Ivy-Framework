import { cn } from "@/lib/utils";
import React from "react";

interface DetailWidgetProps {
  id: string;
  label: string;
  multiLine?: boolean;
  children?: React.ReactNode[];
}

export const DetailWidget: React.FC<DetailWidgetProps> = ({
  id,
  label,
  children,
  multiLine,
}) => {
  return (
    <div className={cn("border-b flex", multiLine && "flex-col")} key={id}>
      <div className="p-3 pl-0 align-middle text-sm font-bold">{label}</div>
      <div
        className={cn(
          "align-middle text-sm ",
          multiLine && "text-left pb-3 max-w-[500px]",
          !multiLine && "text-right ml-auto p-3 pl-2 pr-0 max-w-[300px]"
        )}
      >
        {children}
      </div>
    </div>
  );
};
