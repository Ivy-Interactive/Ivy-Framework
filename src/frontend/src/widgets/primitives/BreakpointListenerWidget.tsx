import { useEffect, useRef } from "react";
import { useEventHandler } from "@/components/event-handler/hooks";
import { type BreakpointName, useBreakpoint } from "@/hooks/use-responsive";

interface BreakpointListenerWidgetProps {
  id: string;
  events?: string[];
}

// The C# `Breakpoint` enum serializes by name (PascalCase). Map the frontend's lowercase
// breakpoint name to the matching enum name so the round-tripped value deserializes server-side.
const ENUM_NAME: Record<BreakpointName, string> = {
  mobile: "Mobile",
  tablet: "Tablet",
  desktop: "Desktop",
  wide: "Wide",
};

/**
 * Invisible widget that reports the active responsive breakpoint back to the server. Renders
 * nothing; it observes the breakpoint via {@link useBreakpoint} and fires the `OnChange` event
 * on mount and whenever the breakpoint changes, letting server-side code branch on screen size.
 */
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
