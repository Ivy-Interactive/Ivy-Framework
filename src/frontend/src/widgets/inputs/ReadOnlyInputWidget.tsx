import CopyToClipboardButton from "@/components/CopyToClipboardButton";
import React from "react";
import { useEventHandler } from "@/components/event-handler";
import { EMPTY_ARRAY } from "@/lib/constants";
import { Densities } from "@/types/density";
import { cn } from "@/lib/utils";

interface ReadOnlyInputWidgetProps {
  id: string;
  value: string | number | boolean | null | undefined;
  showCopyButton?: boolean;
  autoFocus?: boolean;
  events?: string[];
  density?: Densities;
}

const textSizeMap: Record<Densities, string> = {
  [Densities.Small]: "text-sm",
  [Densities.Medium]: "text-body",
  [Densities.Large]: "text-lg",
};

export const ReadOnlyInputWidget: React.FC<ReadOnlyInputWidgetProps> = ({
  id,
  value,
  showCopyButton = true,
  events = EMPTY_ARRAY,
  autoFocus,
  density = Densities.Medium,
}) => {
  const eventHandler = useEventHandler();
  return (
    <div
      key={id}
      className={cn(
        textSizeMap[density],
        "text-muted-foreground flex flex-row items-center w-full focus:outline-none",
      )}
      onBlur={() => {
        if (events.includes("OnBlur")) eventHandler("OnBlur", id, []);
      }}
      onFocus={() => {
        if (events.includes("OnFocus")) eventHandler("OnFocus", id, []);
      }}
      tabIndex={0}
      autoFocus={autoFocus}
    >
      <div className="flex-1">{value != null && value !== "" ? String(value) : "-"}</div>
      {showCopyButton && <CopyToClipboardButton textToCopy={String(value || "")} label="" />}
    </div>
  );
};
