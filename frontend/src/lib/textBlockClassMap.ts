// Shared Ivy text block and HTML tag class map - shadcn typography with Ivy spacing
export const textBlockClassMap: Record<string, string> = {
  // shadcn Typography with original Ivy spacing
  H1: 'text-4xl font-extrabold tracking-tight scroll-m-20 mt-10 mb-2',
  H2: 'text-3xl font-semibold tracking-tight scroll-m-20 mt-10 mb-2',
  H3: 'text-2xl font-semibold tracking-tight scroll-m-20 mt-6 mb-2',
  H4: 'text-xl font-semibold tracking-tight scroll-m-20 mt-4 mb-4',

  // Body Typography
  P: 'leading-7 scroll-m-20',

  // Lead Typography - Prominent Text
  Lead: 'text-sm text-muted-foreground',

  // Blockquote styling with original spacing
  Blockquote: 'border-l-2 pl-6 italic mb-2',

  // Inline code styling with original spacing
  InlineCode:
    'relative rounded bg-muted px-[0.25rem] py-[0.05rem] font-mono text-sm font-semibold',

  // HTML tag equivalents with original spacing
  h1: 'text-4xl font-extrabold tracking-tight scroll-m-20 mt-10 mb-2',
  h2: 'text-3xl font-semibold tracking-tight scroll-m-20 mt-10 mb-2',
  h3: 'text-2xl font-semibold tracking-tight scroll-m-20 mt-6 mb-2',
  h4: 'text-xl font-semibold tracking-tight scroll-m-20 mt-6 mb-2',
  p: 'text-sm leading-relaxed scroll-m-20',
  strong: 'font-semibold',
  em: 'italic',
  ul: 'ml-6 list-disc [&>li:first-child]:mt-0 mb-2',
  ol: 'ml-6 list-decimal [&>li:first-child]:mt-0 mb-2',
  li: 'mt-1 text-sm',
  a: 'text-primary underline brightness-90 hover:brightness-100',
  blockquote: 'border-l-2 pl-6 italic mb-2',
  table: 'w-full border-collapse border border-border mb-2',
  thead: 'bg-muted',
  tr: 'border border-border',
  th: 'border border-border px-4 py-2 text-left font-bold',
  td: 'border border-border px-4 py-2',
  code: 'relative rounded bg-muted px-[0.25rem] py-[0.05rem] font-mono text-sm font-semibold',
  img: 'max-w-full h-auto cursor-zoom-in mb-2',
};

// Container class for text content with consistent spacing
export const textContainerClass = 'flex flex-col gap-4';
