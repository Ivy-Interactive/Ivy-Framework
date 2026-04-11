import { Kbd } from "@/components/Kbd";
import { Densities } from "@/types/density";
import React from "react";

interface KbdWidgetProps {
  children: React.ReactNode;
  density?: Densities;
}

export const KbdWidget: React.FC<KbdWidgetProps> = ({
  children,
  density: _density = Densities.Medium,
}) => <Kbd>{children}</Kbd>;
