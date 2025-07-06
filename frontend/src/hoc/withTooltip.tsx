import { ComponentType } from 'react';
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from '@/components/ui/tooltip';

type WithTooltipProps<P> = P &
  JSX.IntrinsicAttributes & {
    tooltipText?: string;
    className?: string;
    style?: React.CSSProperties;
  };

function withTooltip<P extends JSX.IntrinsicAttributes>(
  Component: ComponentType<P>
) {
  return function TooltipHOC(props: WithTooltipProps<P>) {
    const { tooltipText, className, style, ...restProps } = props;

    const componentWithStyles = (
      <Component className={className} style={style} {...(restProps as P)} />
    );

    if (!tooltipText) {
      return componentWithStyles;
    }

    return (
      <TooltipProvider>
        <Tooltip>
          <TooltipTrigger>{componentWithStyles}</TooltipTrigger>
          <TooltipContent className="bg-popover text-popover-foreground shadow-md">
            {tooltipText}
          </TooltipContent>
        </Tooltip>
      </TooltipProvider>
    );
  };
}

export default withTooltip;
