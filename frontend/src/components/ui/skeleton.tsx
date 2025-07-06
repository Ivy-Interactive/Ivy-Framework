import { cn } from '@/lib/utils';

function Skeleton({
  className,
  ...props
}: React.HTMLAttributes<HTMLDivElement>) {
  return (
    <div
      style={{
        backgroundColor: 'color-mix(in srgb, var(--primary) 10%, transparent)',
      }}
      className={cn('animate-pulse rounded-md', className)}
      {...props}
    />
  );
}

export { Skeleton };
