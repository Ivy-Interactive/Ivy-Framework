import React from "react";
import { Densities } from "@/types/density";
import { getWidth, getHeight } from "@/lib/styles";
import Icon from "@/components/Icon";
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "@/components/ui/tooltip";

interface FieldWidgetProps {
  id: string;
  label: string;
  description?: string;
  required: boolean;
  help?: string;
  children?: React.ReactNode;
  density?: Densities;
  width?: string;
  height?: string;
  labelPosition?: "Top" | "Left" | 0 | 1;
  slots?: {
    Tools?: React.ReactNode[];
  };
}

export const FieldWidget: React.FC<FieldWidgetProps> = ({
  label,
  description,
  required,
  help,
  children,
  density = Densities.Medium,
  width,
  height,
  labelPosition,
  slots,
}) => {
  const childrenRef = React.useRef<HTMLDivElement>(null);
  const [inputId, setInputId] = React.useState<string | undefined>(undefined);
  const [prevChildren, setPrevChildren] = React.useState(children);
  const [shouldCheckId, setShouldCheckId] = React.useState(true);

  if (children !== prevChildren) {
    setPrevChildren(children);
    setShouldCheckId(true);
  }

  React.useEffect(() => {
    if (shouldCheckId) {
      const el = childrenRef.current?.querySelector("input, select, textarea");
      if (el?.id) setInputId(el.id);
      setShouldCheckId(false);
    }
  }, [shouldCheckId]);

  const labelSizeClass =
    density === Densities.Small ? "text-xs" : density === Densities.Large ? "text-base" : "text-sm";
  const descriptionSizeClass =
    density === Densities.Small ? "text-xs" : density === Densities.Large ? "text-sm" : "text-xs";

  const gapClass =
    density === Densities.Small ? "gap-1" : density === Densities.Large ? "gap-3" : "gap-2";

  const styles: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
  };

  const flexClass = width || height ? "" : "flex-1";

  const isLeft = labelPosition === "Left" || labelPosition === 1;

  const tools = slots?.Tools;
  const hasTools = Array.isArray(tools) ? tools.length > 0 : Boolean(tools);

  const labelContent = label && (
    <>
      <label
        htmlFor={inputId}
        className={`${labelSizeClass} font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70`}
      >
        {label} {required && <span className="font-mono text-primary">*</span>}
      </label>
      {help && (
        <TooltipProvider>
          <Tooltip>
            <TooltipTrigger asChild>
              <button
                type="button"
                aria-label="Help"
                className="inline-flex items-center justify-center focus:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 rounded-sm"
              >
                <Icon
                  name="Info"
                  size="14"
                  className="text-muted-foreground hover:text-foreground transition-colors"
                />
              </button>
            </TooltipTrigger>
            <TooltipContent className="bg-popover text-popover-foreground shadow-md">
              {help}
            </TooltipContent>
          </Tooltip>
        </TooltipProvider>
      )}
    </>
  );

  if (isLeft) {
    return (
      <div className={`flex flex-col sm:flex-row ${gapClass} ${flexClass} min-w-0`} style={styles}>
        {(label || hasTools) && (
          <div className="flex flex-col gap-2 min-w-[120px] w-1/4 sm:w-1/3 pt-2 sm:pt-0 sm:mt-2.5 self-start">
            {label && <div className="flex items-center gap-1.5">{labelContent}</div>}
            {hasTools && <div className="flex flex-col gap-1">{tools}</div>}
          </div>
        )}
        <div ref={childrenRef} className="flex-1 flex flex-col gap-2 min-w-0">
          {children}
          {description && (
            <p className={`${descriptionSizeClass} text-muted-foreground`}>{description}</p>
          )}
        </div>
      </div>
    );
  }

  return (
    <div className={`flex flex-col ${gapClass} ${flexClass} min-w-0`} style={styles}>
      {(label || hasTools) && (
        // The label row height is driven purely by the label so a field with tools has the
        // exact same padding as one without (tools are absolutely positioned, centered on the
        // label line, and right-aligned). items-end keeps the label bottom-aligned, and the
        // min-height only kicks in when there is no label so a tools-only row stays visible.
        <div
          className={`relative flex items-end gap-1.5 ${!label && hasTools ? "min-h-[1.25rem]" : ""}`}
        >
          {labelContent}
          {hasTools && (
            <div className="absolute right-0 top-1/2 -translate-y-1/2 flex items-center gap-1">
              {tools}
            </div>
          )}
        </div>
      )}
      <div ref={childrenRef}>{children}</div>
      {description && (
        <p className={`${descriptionSizeClass} text-muted-foreground`}>{description}</p>
      )}
    </div>
  );
};
