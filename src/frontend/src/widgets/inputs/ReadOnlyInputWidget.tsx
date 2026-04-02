import CopyToClipboardButton from "@/components/CopyToClipboardButton";
import React from "react";
import { useEventHandler } from "@/components/event-handler";
import { EMPTY_ARRAY } from "@/lib/constants";

interface ReadOnlyInputWidgetProps {
  id: string;
  value: string | number | boolean | null | undefined;
  showCopyButton?: boolean;
  autoFocus?: boolean;
  events?: string[];
}

export const ReadOnlyInputWidget: React.FC<ReadOnlyInputWidgetProps> = ({
  id,
  value,
  showCopyButton = true,
  events = EMPTY_ARRAY,
  autoFocus,
}) => {
  const eventHandler = useEventHandler();
  return (
    <div
      key={id}
      className="text-body text-muted-foreground flex flex-row items-center w-full focus:outline-none"
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
