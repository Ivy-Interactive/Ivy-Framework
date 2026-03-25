import { DialogHeader, DialogTitle } from "@/components/ui/dialog";
import React from "react";

interface DialogHeaderWidgetProps {
  id: string;
  title: string;
  hideCloseButton?: boolean;
}

export const DialogHeaderWidget: React.FC<DialogHeaderWidgetProps> = ({
  title,
  hideCloseButton,
}) => (
  <DialogHeader className="flex gap-2" hideCloseButton={hideCloseButton}>
    <DialogTitle>{title}</DialogTitle>
  </DialogHeader>
);
