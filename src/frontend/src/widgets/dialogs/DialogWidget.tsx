import { Dialog, DialogContent } from "@/components/ui/dialog";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";
import React, { useState } from "react";
import { useEventHandler } from "@/components/event-handler";
import { getWidth } from "@/lib/styles";
import { cn } from "@/lib/utils";

interface DialogWidgetProps {
  id: string;
  children?: React.ReactNode;
  width?: string;
  events?: string[];
  closedBy?: string;
  confirmationMessage?: string;
}

const EMPTY_EVENTS: string[] = [];

export const DialogWidget: React.FC<DialogWidgetProps> = ({
  id,
  children,
  width,
  events = EMPTY_EVENTS,
  closedBy = "Any",
  confirmationMessage,
}) => {
  const eventHandler = useEventHandler();
  const isVisible = true;
  const [confirmingClose, setConfirmingClose] = useState(false);

  const closedByNormalized = (closedBy || "any").toLowerCase();
  const requiresCloseConfirmation = Boolean(confirmationMessage);

  const widthStyles = getWidth(width);
  const styles = {
    ...widthStyles,
    ...(width && widthStyles.width && !widthStyles.maxWidth ? { maxWidth: widthStyles.width } : {}),
  };

  const emitClose = () => {
    if (events.includes("OnClose")) eventHandler("OnClose", id, []);
  };

  return (
    <Dialog
      open={true}
      onOpenChange={() => {
        if (requiresCloseConfirmation) {
          setConfirmingClose(true);
          return;
        }
        emitClose();
      }}
    >
      <DialogContent
        style={styles}
        className={cn(isVisible && "alert-animate-enter")}
        aria-describedby={undefined}
        onInteractOutside={
          closedByNormalized === "closerequest" || closedByNormalized === "none"
            ? (e) => e.preventDefault()
            : undefined
        }
        onEscapeKeyDown={closedByNormalized === "none" ? (e) => e.preventDefault() : undefined}
        onOpenAutoFocus={(e) => {
          const container = e.currentTarget as HTMLElement | null;
          const target = container?.querySelector<HTMLElement>("[autofocus]");
          if (target) {
            e.preventDefault();
            target.focus();
          } else {
            e.preventDefault();
          }
        }}
        onCloseAutoFocus={(e) => e.preventDefault()}
      >
        {children}
        {requiresCloseConfirmation && (
          <AlertDialog open={confirmingClose} onOpenChange={setConfirmingClose}>
            <AlertDialogContent>
              <AlertDialogHeader>
                <AlertDialogTitle>Are you sure?</AlertDialogTitle>
                <AlertDialogDescription>{confirmationMessage}</AlertDialogDescription>
              </AlertDialogHeader>
              <AlertDialogFooter>
                <AlertDialogCancel>Cancel</AlertDialogCancel>
                <AlertDialogAction
                  onClick={() => {
                    setConfirmingClose(false);
                    emitClose();
                  }}
                >
                  Close
                </AlertDialogAction>
              </AlertDialogFooter>
            </AlertDialogContent>
          </AlertDialog>
        )}
      </DialogContent>
    </Dialog>
  );
};
