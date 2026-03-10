'use client';

import { m, LazyMotion, domAnimation } from 'framer-motion';
import { cn } from '@/lib/utils';

interface TextShimmerProps {
  children: string;
  as?: React.ElementType;
  className?: string;
  duration?: number;
  spread?: number;
}

// Use component properties instead of string factory (avoids deprecation warnings)
const motionComponents = {
  p: m.p,
  span: m.span,
  div: m.div,
  h1: m.h1,
  h2: m.h2,
  h3: m.h3,
  h4: m.h4,
  h5: m.h5,
  h6: m.h6,
} as const;

export function TextShimmer({
  children,
  as: Component = 'p',
  className,
  duration = 2,
  spread = 2,
}: TextShimmerProps) {
  const MotionComponent =
    (motionComponents as Record<string, typeof m.div>)[
      (Component as unknown as string) || 'p'
    ] ?? m.p;

  const dynamicSpread = children.length * spread;

  return (
    <LazyMotion features={domAnimation}>
      <MotionComponent
        className={cn(
          'relative inline-block bg-[length:250%_100%,auto] bg-clip-text',
          'text-transparent [--base-color:#a1a1aa] [--base-gradient-color:#000]',
          '[--bg:linear-gradient(90deg,#0000_calc(50%-var(--spread)),var(--base-gradient-color),#0000_calc(50%+var(--spread)))] [background-repeat:no-repeat,padding-box]',
          'dark:[--base-color:#71717a] dark:[--base-gradient-color:#ffffff] dark:[--bg:linear-gradient(90deg,#0000_calc(50%-var(--spread)),var(--base-gradient-color),#0000_calc(50%+var(--spread)))]',
          className
        )}
        initial={{ backgroundPosition: '100% center' }}
        animate={{ backgroundPosition: '0% center' }}
        transition={{
          repeat: Infinity,
          duration,
          ease: 'linear',
        }}
        style={
          {
            '--spread': `${dynamicSpread}px`,
            backgroundImage: `var(--bg), linear-gradient(var(--base-color), var(--base-color))`,
          } as React.CSSProperties
        }
      >
        {children}
      </MotionComponent>
    </LazyMotion>
  );
}
