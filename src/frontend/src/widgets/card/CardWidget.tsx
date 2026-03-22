import { Card, CardContent, CardFooter, CardHeader } from "@/components/ui/card";
import { getAspectRatio, getHeight, getWidth } from "@/lib/styles";
import { cn } from "@/lib/utils";
import { useEventHandler } from "@/components/event-handler";
import React, { useCallback } from "react";
import { Densities } from "@/types/density";
import { cardStyles, getSizeClasses } from "./styles";

const EMPTY_ARRAY: never[] = [];

interface CardWidgetProps {
  id: string;
  events: string[];
  width?: string;
  height?: string;
  aspectRatio?: number;
  hoverVariant?: "None" | "Pointer" | "PointerAndTranslate";
  density?: Densities;
  disabled?: boolean;
  "data-testid"?: string;
  slots?: {
    Header?: React.ReactNode[];
    Content?: React.ReactNode[];
    Footer?: React.ReactNode[];
  };
}

export const CardWidget: React.FC<CardWidgetProps> = ({
  id,
  events = EMPTY_ARRAY,
  width = "Full",
  height,
  aspectRatio,
  hoverVariant = "None",
  density = Densities.Medium,
  disabled,
  slots,
  "data-testid": testId,
}) => {
  const eventHandler = useEventHandler();
  const sizeClasses = getSizeClasses(density);

  const styles = {
    ...getWidth(width),
    ...getHeight(height),
    ...getAspectRatio(aspectRatio),
  };

  const footerIsEmpty = !slots?.Footer || slots.Footer.length === 0;
  const headerIsEmpty = !slots?.Header || slots.Header.length === 0;

  const handleClick = useCallback(() => {
    if (disabled) return;
    if (events.includes("OnClick")) eventHandler("OnClick", id, []);
  }, [id, eventHandler, events, disabled]);

  const hoverClass =
    hoverVariant === "None" || disabled
      ? cardStyles.hover.none
      : hoverVariant === "Pointer"
        ? cardStyles.hover.pointer
        : cardStyles.hover.pointerAndTranslate;

  return (
    <Card
      role="region"
      data-testid={testId}
      style={styles}
      disabled={disabled}
      className={cn(cardStyles.container, hoverClass)}
      onClick={disabled ? undefined : handleClick}
    >
      {!headerIsEmpty ? (
        <CardHeader className={cn(cardStyles.header.base, sizeClasses.header)}>
          {slots?.Header}
        </CardHeader>
      ) : (
        <></>
      )}
      <CardContent
        className={cn(
          cardStyles.content.base,
          sizeClasses.content,
          headerIsEmpty && sizeClasses.contentNoHeader,
        )}
      >
        {slots?.Content}
      </CardContent>
      {!footerIsEmpty && (
        <CardFooter className={cn(cardStyles.footer.base, sizeClasses.footer)}>
          {slots?.Footer}
        </CardFooter>
      )}
    </Card>
  );
};
