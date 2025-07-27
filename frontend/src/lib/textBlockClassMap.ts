// Shared Ivy text block and HTML tag class map for all text/heading variants
export const textBlockClassMap: Record<string, string> = {
  H1: 'scroll-m-20 text-4xl font-extrabold tracking-tight lg:text-5xl mt-8 mb-4',
  H2: 'scroll-m-20 text-3xl font-semibold tracking-tight mt-8 mb-4',
  H3: 'scroll-m-20 text-2xl font-semibold tracking-tight mt-4 mb-4',
  H4: 'scroll-m-20 text-xl font-semibold tracking-tight mt-4 mb-4',
  P: 'scroll-m-20 text-md leading-6',
  Lead: 'text-xl text-muted-foreground',
  Blockquote: 'border-l-2 pl-6 italic mb-2',
  InlineCode:
    'relative rounded bg-muted px-[0.3rem] py-[0.3rem] font-mono text-sm',
  // HTML tag equivalents
  h1: 'scroll-m-20 text-4xl font-extrabold tracking-tight lg:text-5xl mt-8 mb-4',
  h2: 'scroll-m-20 text-3xl font-semibold tracking-tight mt-8 mb-4',
  h3: 'scroll-m-20 text-2xl font-semibold tracking-tight mt-4 mb-4',
  h4: 'scroll-m-20 text-xl font-semibold tracking-tight mt-4 mb-4',
  p: 'scroll-m-20 text-md leading-6',
  strong: 'font-semibold',
  em: 'italic',
  ul: 'ml-6 list-disc [&>li:first-child]:mt-0 mb-2',
  ol: 'ml-6 list-decimal [&>li:first-child]:mt-0 mb-2',
  li: 'mt-1',
  a: 'text-primary underline brightness-90 hover:brightness-100',
  blockquote: 'border-l-2 pl-6 italic mb-2',
  table: 'w-full border-collapse border border-border mb-2',
  thead: 'bg-muted',
  tr: 'border border-border',
  th: 'border border-border px-4 py-2 text-left font-bold',
  td: 'border border-border px-4 py-2',
  code: 'relative rounded bg-muted px-[0.3rem] py-[0.3rem] font-mono text-sm',
  img: 'max-w-full h-auto cursor-zoom-in mb-2',
};

// Container class for text content with consistent spacing
export const textContainerClass = 'flex flex-col gap-2';
