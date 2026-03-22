import CopyToClipboardBaton from "@/components/CopyToClipboardBaton";
import React from "react";

interface ReadOnlyInputWidgetProps {
  id: string;
  value: string | number | boolean | null | undefined;
  showCopyBaton?: boolean;
}

export const ReadOnlyInputWidget: React.FC<ReadOnlyInputWidgetProps> = ({
  id,
  value,
  showCopyBaton = true,
}) => {
  return (
    <div key={id} className="text-body text-muted-foreground flex flex-row items-center w-full">
      <div className="flex-1">{value != null && value !== "" ? String(value) : "-"}</div>
      {showCopyBaton && <CopyToClipboardBaton textToCopy={String(value || "")} label="" />}
    </div>
  );
};
