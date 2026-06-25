import { Kbd } from "@/components/Kbd";
import { Densities } from "@/types/density";
import React from "react";

interface KbdWidgetProps {
  children: React.ReactNode;
  content?: string;
  ghost?: boolean;
  density?: Densities;
}

export const KbdWidget: React.FC<KbdWidgetProps> = ({
  children,
  content,
  ghost = false,
  density: _density = Densities.Medium,
}) => (
  <Kbd keys={content} ghost={ghost}>
    {children}
  </Kbd>
);
