import React, { ComponentType, forwardRef } from "react";
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "@/components/ui/tooltip";

type WithTooltipProps<P> = P &
  React.JSX.IntrinsicAttributes & {
    tooltipText?: string;
    className?: string;
    style?: React.CSSProperties;
  };

function withTooltip<P extends React.JSX.IntrinsicAttributes>(Component: ComponentType<P>) {
  const WithTooltip = forwardRef<unknown, WithTooltipProps<P>>(function TooltipHOC(props, ref) {
    const { tooltipText, className, style, ...restProps } = props;

    const ariaLabel = tooltipText && !(restProps as any)["aria-label"] ? tooltipText : undefined;

    const componentWithStyles = (
      <Component
        className={className}
        style={style}
        aria-label={ariaLabel}
        ref={ref}
        {...(restProps as unknown as P)}
      />
    );

    if (!tooltipText) {
      return componentWithStyles;
    }

    return (
      <TooltipProvider>
        <Tooltip>
          <TooltipTrigger asChild>{componentWithStyles}</TooltipTrigger>
          <TooltipContent className="bg-popover text-popover-foreground shadow-md">
            {tooltipText}
          </TooltipContent>
        </Tooltip>
      </TooltipProvider>
    );
  });

  WithTooltip.displayName = `withTooltip(${Component.displayName || Component.name || "Component"})`;

  return WithTooltip;
}

export default withTooltip;
