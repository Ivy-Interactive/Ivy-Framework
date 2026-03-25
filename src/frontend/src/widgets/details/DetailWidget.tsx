import React from "react";
import { DetailItem } from "@/components/ui/detail";

interface DetailWidgetProps {
  id: string;
  label: string;
  multiline?: boolean;
  children?: React.ReactNode[];
}

export const DetailWidget: React.FC<DetailWidgetProps> = ({ id, label, children, multiline }) => {
  return (
    <DetailItem label={label} multiline={multiline} key={id}>
      {children}
    </DetailItem>
  );
};
