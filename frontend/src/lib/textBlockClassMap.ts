// Shared Ivy text block and HTML tag class map for all text/heading variants
export const textBlockClassMap: Record<string, string> = {
  // Display Typography - Large Headlines
  H1: 'text-display-60 scroll-m-20 mt-8 mb-4',
  H2: 'text-display-40 scroll-m-20 mt-8 mb-4',
  H3: 'text-display-30 scroll-m-20 mt-4 mb-4',
  H4: 'text-display-25 scroll-m-20 mt-4 mb-4',

  // Body Typography - Main Content
  P: 'text-body scroll-m-20',

  // Lead Typography - Prominent Text
  Lead: 'text-lead text-muted-foreground',

  // Blockquote styling
  Blockquote: 'border-l-2 pl-6 italic mb-2 text-body',

  // Inline code styling
  InlineCode:
    'relative rounded bg-muted px-[0.3rem] py-[0.3rem] font-mono text-sm',

  // HTML tag equivalents
  h1: 'text-display-60 scroll-m-20 mt-8 mb-4',
  h2: 'text-display-40 scroll-m-20 mt-8 mb-4',
  h3: 'text-display-30 scroll-m-20 mt-4 mb-4',
  h4: 'text-display-25 scroll-m-20 mt-4 mb-4',
  p: 'text-body scroll-m-20',
  strong: 'font-semibold',
  em: 'italic',
  ul: 'ml-6 list-disc [&>li:first-child]:mt-0 mb-2 text-body',
  ol: 'ml-6 list-decimal [&>li:first-child]:mt-0 mb-2 text-body',
  li: 'mt-1',
  a: 'text-primary underline brightness-90 hover:brightness-100',
  blockquote: 'border-l-2 pl-6 italic mb-2 text-body',
  table: 'w-full border-collapse border border-border mb-2',
  thead: 'bg-muted',
  tr: 'border border-border',
  th: 'border border-border px-4 py-2 text-left font-bold text-large-label',
  td: 'border border-border px-4 py-2 text-body',
  code: 'relative rounded bg-muted px-[0.3rem] py-[0.3rem] font-mono text-sm',
  img: 'max-w-full h-auto cursor-zoom-in mb-2',
};

// Container class for text content with consistent spacing
export const textContainerClass = 'flex flex-col gap-4';
