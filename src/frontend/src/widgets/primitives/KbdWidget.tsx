import { Kbd } from "@/components/Kbd";
import { Densities } from "@/types/density";
import React from "react";

interface KbdWidgetProps {
  children: React.ReactNode;
  keys?: string;
  ghost?: boolean;
  density?: Densities;
}

export const KbdWidget: React.FC<KbdWidgetProps> = ({
  children,
  keys,
  ghost = false,
  density: _density = Densities.Medium,
}) => (
  <Kbd keys={keys} ghost={ghost}>
    {children}
  </Kbd>
);
