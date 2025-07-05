import { useEventHandler } from "@/components/EventHandlerContext";
import React from "react";

interface ColorInputWidgetProps {
  id: string;
  value: any;
}

export const ColorInputWidget: React.FC<ColorInputWidgetProps> = ({
  id,
  value,
}) => {
  const eventHandler = useEventHandler();
  return (
    <div className="flex items-center space-x-4">
      <input
        value={value}
        onChange={(e) => eventHandler("OnChange", id, [e.target.value])}
        onBlur={(e) => eventHandler("OnBlur", id, [e.target.value])}
        type="color"
        className="w-10 h-10 p-1 border border-gray-300 rounded-lg cursor-pointer"
      />
    </div>
  );
};
