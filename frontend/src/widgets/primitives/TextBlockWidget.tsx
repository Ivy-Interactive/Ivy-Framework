import { getColor, getOverflow, getWidth, Overflow } from '@/lib/styles';
import { cn } from '@/lib/utils';
import React from 'react';

type TextBlockVariant =
  | 'Literal'
  | 'H1'
  | 'H2'
  | 'H3'
  | 'H4'
  | 'P'
  | 'Inline'
  | 'Block'
  | 'Blockquote'
  | 'InlineCode'
  | 'Lead'
  | 'Large'
  | 'Small'
  | 'Muted'
  | 'Danger'
  | 'Warning'
  | 'Success'
  | 'Label';

interface TextBlockWidgetProps {
  content: string;
  variant: TextBlockVariant;
  width?: string;
  strikeThrough?: boolean;
  color: string;
  noWrap?: boolean;
  overflow?: Overflow;
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
    <h1
      className={cn(
        'scroll-m-20 text-4xl font-extrabold tracking-tight lg:text-5xl',
        className
      )}
      style={style}
    >
      {children}
    </h1>
  ),
  H2: ({ children, className, style }) => (
    <h2
      className={cn(
        'scroll-m-20 text-3xl font-semibold tracking-tight first:mt-0',
        className
      )}
      style={style}
    >
      {children}
    </h2>
  ),
  H3: ({ children, className, style }) => (
    <h3
      className={cn(
        'scroll-m-20 text-2xl font-semibold tracking-tight',
        className
      )}
      style={style}
    >
      {children}
    </h3>
  ),
  H4: ({ children, className, style }) => (
    <h4
      className={cn(
        'scroll-m-20 text-xl font-semibold tracking-tight',
        className
      )}
      style={style}
    >
      {children}
    </h4>
  ),
  Block: ({ children, className, style }) => (
    <div className={className} style={style}>
      {children}
    </div>
  ),
  P: ({ children, className, style }) => (
    <p
      className={cn('scroll-m-20 text-md leading-relaxed', className)}
      style={style}
    >
      {children}
    </p>
  ),
  Inline: ({ children, className, style }) => (
    <span className={cn(className)} style={style}>
      {children}
    </span>
  ),
  Blockquote: ({ children, className, style }) => (
    <blockquote
      className={cn('border-l-2 pl-6 italic', className)}
      style={style}
    >
      {children}
    </blockquote>
  ),
  InlineCode: ({ children, className, style }) => (
    <code
      className={cn(
        'relative rounded bg-muted px-[0.3rem] py-[0.3rem] font-mono text-sm',
        className
      )}
      style={style}
    >
      {children}
    </code>
  ),
  Lead: ({ children, className, style }) => (
    <p className={cn('text-xl text-muted-foreground', className)} style={style}>
      {children}
    </p>
  ),
  Large: ({ children, className, style }) => (
    <div className={cn('text-lg font-semibold', className)} style={style}>
      {children}
    </div>
  ),
  Small: ({ children, className, style }) => (
    <div
      className={cn('text-sm font-medium leading-none', className)}
      style={style}
    >
      {children}
    </div>
  ),
  Muted: ({ children, className, style }) => (
    <div
      className={cn('text-sm text-muted-foreground', className)}
      style={style}
    >
      {children}
    </div>
  ),
  Danger: ({ children, className, style }) => (
    <div className={cn('text-red-600 font-semibold', className)} style={style}>
      {children}
    </div>
  ),
  Warning: ({ children, className, style }) => (
    <div
      className={cn('text-yellow-600 font-semibold', className)}
      style={style}
    >
      {children}
    </div>
  ),
  Success: ({ children, className, style }) => (
    <div
      className={cn('text-green-600 font-semibold', className)}
      style={style}
    >
      {children}
    </div>
  ),
  Label: ({ children, className, style }) => (
    <div
      className={cn('text-sm font-medium leading-none mt-2 -mb-2', className)}
      style={style}
    >
      {children}
    </div>
  ),
};

export const TextBlockWidget: React.FC<TextBlockWidgetProps> = ({
  content,
  variant,
  width,
  color,
  strikeThrough,
  noWrap,
  overflow,
}) => {
  const styles: React.CSSProperties = {
    ...getWidth(width),
    ...getColor(color, 'color', 'background'),
    ...getOverflow(overflow),
  };

  const Component = variantMap[variant];
  return (
    <Component
      style={styles}
      className={cn(
        strikeThrough && 'line-through',
        noWrap && 'whitespace-nowrap'
      )}
    >
      {content}
    </Component>
  );
};
