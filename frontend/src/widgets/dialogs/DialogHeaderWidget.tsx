import { DialogHeader, DialogTitle } from "@/components/ui/dialog";
import React from "react";

interface DialogHeaderWidgetProps {
  id: string;
  title: string;
}

export const DialogHeaderWidget: React.FC<DialogHeaderWidgetProps> = ({
  title,
}) => (
  <DialogHeader className="flex gap-2">
    <DialogTitle>{title}</DialogTitle>
  </DialogHeader>
);
