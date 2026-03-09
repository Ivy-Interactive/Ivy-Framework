import { getColor, getOverflow, getWidth, Overflow } from '@/lib/styles';
import { cn } from '@/lib/utils';
import React, { useState, useEffect } from 'react';
import { typography } from '../../lib/styles';
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from '@/components/ui/tooltip';
import MarkdownRenderer from '@/components/MarkdownRenderer';
import {
  widgetContentOverrides,
  subscribeToContentOverride,
} from '@/widgets/widgetRenderer';
import { Scales } from '@/types/scale';
import { TextAlignment } from '@/types/textAlignment';

type TextBlockVariant =
  | 'Literal'
  | 'H1'
  | 'H2'
  | 'H3'
  | 'H4'
  | 'H5'
  | 'H6'
  | 'P'
  | 'Inline'
  | 'Block'
  | 'Blockquote'
  | 'InlineCode'
  | 'Lead'
  | 'Muted'
  | 'Danger'
  | 'Warning'
  | 'Success'
  | 'Label'
  | 'Strong'
  | 'Display';

interface TextBlockWidgetProps {
  id: string;
  content: string;
  variant: TextBlockVariant;
  width?: string;
  strikeThrough?: boolean;
  color: string;
  noWrap?: boolean;
  overflow?: Overflow;
  bold?: boolean;
  italic?: boolean;
  muted?: boolean;
  scale?: Scales;
  textAlignment?: TextAlignment;
}

interface VariantMap {
  [key: string]: React.FC<{
    children: string;
    className?: string;
    style?: React.CSSProperties;
  }>;
}
const variantMap: VariantMap = {
  Literal: ({ children, className, style }) => (
    <span className={className} style={style}>
      {children}
    </span>
  ),
  H1: ({ children, className, style }) => (
    <h1 className={cn(typography.h1, className)} style={style}>
      {children}
    </h1>
  ),
  H2: ({ children, className, style }) => (
    <h2 className={cn(typography.h2, className)} style={style}>
      {children}
    </h2>
  ),
  H3: ({ children, className, style }) => (
    <h3 className={cn(typography.h3, className)} style={style}>
      {children}
    </h3>
  ),
  H4: ({ children, className, style }) => (
    <h4 className={cn(typography.h4, className)} style={style}>
      {children}
    </h4>
  ),
  H5: ({ children, className, style }) => (
    <h5 className={cn(typography.h5, className)} style={style}>
      {children}
    </h5>
  ),
  H6: ({ children, className, style }) => (
    <h6 className={cn(typography.h6, className)} style={style}>
      {children}
    </h6>
  ),
  Block: ({ children, className, style }) => {
    const spanRef = React.useRef<HTMLSpanElement>(null);
    const [isTruncated, setIsTruncated] = React.useState(false);
    const [showTooltip, setShowTooltip] = React.useState(false);
    React.useEffect(() => {
      const checkTruncation = () => {
        const el = spanRef.current;
        if (el) {
          setIsTruncated(el.scrollWidth > el.clientWidth);
        }
      };
      checkTruncation();
      window.addEventListener('resize', checkTruncation);
      return () => {
        window.removeEventListener('resize', checkTruncation);
      };
    }, [children, style]);
    return (
      <div className={cn(typography.block, className)} style={style}>
        <TooltipProvider>
          <Tooltip>
            <TooltipTrigger asChild>
              <span
                ref={spanRef}
                className="overflow-hidden text-ellipsis"
                onMouseEnter={() => setShowTooltip(true)}
                onMouseLeave={() => setShowTooltip(false)}
              >
                {children}
              </span>
            </TooltipTrigger>
            {showTooltip && isTruncated && typeof children === 'string' && (
              <TooltipContent className="bg-popover text-popover-foreground shadow-md">
                {children}
              </TooltipContent>
            )}
          </Tooltip>
        </TooltipProvider>
      </div>
    );
  },
  P: ({ children, className, style }) => (
    <p className={cn(typography.p, className)} style={style}>
      {children}
    </p>
  ),
  Inline: ({ children, className, style }) => (
    <span className={cn(className)} style={style}>
      {children}
    </span>
  ),
  Blockquote: ({ children, className, style }) => (
    <blockquote className={cn(typography.blockquote, className)} style={style}>
      {children}
    </blockquote>
  ),
  InlineCode: ({ children, className, style }) => (
    <code className={cn(typography.code, className)} style={style}>
      {children}
    </code>
  ),
  Lead: ({ children, className, style }) => (
    <div className={cn(typography.lead, className)} style={style}>
      <MarkdownRenderer content={children} />
    </div>
  ),
  Muted: ({ children, className, style }) => (
    <div className={cn(typography.muted, className)} style={style}>
      {children}
    </div>
  ),
  Danger: ({ children, className, style }) => (
    <div className={cn(typography.danger, className)} style={style}>
      {children}
    </div>
  ),
  Warning: ({ children, className, style }) => (
    <div className={cn(typography.warning, className)} style={style}>
      {children}
    </div>
  ),
  Success: ({ children, className, style }) => (
    <div className={cn(typography.success, className)} style={style}>
      {children}
    </div>
  ),
  Label: ({ children, className, style }) => (
    <div className={cn(typography.label, className)} style={style}>
      {children}
    </div>
  ),
  Strong: ({ children, className, style }) => (
    <strong className={cn(typography.strong, className)} style={style}>
      {children}
    </strong>
  ),
  Display: ({ children, className, style }) => (
    <div className={cn(typography.display, className)} style={style}>
      {children}
    </div>
  ),
};

export const TextBlockWidget: React.FC<TextBlockWidgetProps> = ({
  id,
  content = '',
  variant = 'Literal',
  width,
  color,
  strikeThrough,
  noWrap,
  overflow,
  bold,
  italic,
  muted,
  scale,
  textAlignment,
}) => {
  const [, forceUpdate] = useState(0);

  // Subscribe to content override changes
  useEffect(() => {
    return subscribeToContentOverride(id, () => forceUpdate(n => n + 1));
  }, [id]);

  // Use override content if available, otherwise use prop
  const displayContent = widgetContentOverrides.get(id) ?? content;

  const styles: React.CSSProperties = {
    ...getWidth(width),
    ...getColor(color, 'color', 'background'),
    ...getOverflow(overflow),
    wordBreak: 'normal',
    overflowWrap: 'break-word',
    ...(textAlignment && {
      textAlign:
        textAlignment.toLowerCase() as React.CSSProperties['textAlign'],
    }),
  };

  const scaleClasses: Record<string, string> = {
    [Scales.Small]: typography.small,
    [Scales.Large]: typography.large,
  };

  const Component = variantMap[variant];
  return (
    <Component
      style={styles}
      className={cn(
        strikeThrough && 'line-through',
        noWrap && 'whitespace-nowrap',
        bold && 'font-semibold',
        italic && 'italic',
        muted && 'text-muted-foreground',
        scale && scaleClasses[scale]
      )}
    >
      {displayContent}
    </Component>
  );
};
