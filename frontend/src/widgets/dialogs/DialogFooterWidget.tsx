import { DialogFooter } from "@/components/ui/dialog";
import React from "react";

interface DialogFooterWidgetProps {
  id: string;
  children?: React.ReactNode;
}

export const DialogFooterWidget: React.FC<DialogFooterWidgetProps> = ({
  children,
}) => <DialogFooter>{children}</DialogFooter>;
