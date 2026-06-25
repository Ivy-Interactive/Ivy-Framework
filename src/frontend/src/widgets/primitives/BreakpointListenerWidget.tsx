import { useEffect, useRef } from "react";
import { useEventHandler } from "@/components/event-handler/hooks";
import { type BreakpointName, useBreakpoint } from "@/hooks/use-responsive";

interface BreakpointListenerWidgetProps {
  id: string;
  events?: string[];
}

const ENUM_NAME: Record<BreakpointName, string> = {
  mobile: "Mobile",
  tablet: "Tablet",
  desktop: "Desktop",
  wide: "Wide",
};

export const BreakpointListenerWidget: React.FC<BreakpointListenerWidgetProps> = ({
  id,
  events,
}) => {
  const eventHandler = useEventHandler();
  const breakpoint = useBreakpoint();
  const lastSent = useRef<BreakpointName | undefined>(undefined);

  useEffect(() => {
    if (!events?.includes("OnChange")) return;
    if (lastSent.current === breakpoint) return;
    lastSent.current = breakpoint;
    eventHandler("OnChange", id, [ENUM_NAME[breakpoint]]);
  }, [breakpoint, eventHandler, id, events]);

  return null;
};

BreakpointListenerWidget.displayName = "BreakpointListenerWidget";
