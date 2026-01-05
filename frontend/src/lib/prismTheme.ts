import { CSSProperties } from 'react';

// Create a theme that uses CSS variables for dynamic theming
const createIvyPrismTheme = (): Record<string, CSSProperties> => ({
  'code[class*="language-"]': {
    fontFamily: 'var(--font-mono)',
    fontSize: '14px',
    background: 'transparent',
    color: 'var(--foreground)',
  },
  'pre[class*="language-"]': {
    fontFamily: 'var(--font-mono)',
    fontSize: '14px',
    background: 'transparent',
    padding: '1em',
    margin: 0,
    color: 'var(--foreground)',
    // Remove overflow: 'auto' to use ScrollArea instead
    // Remove border since it's now applied to ScrollArea container
    borderRadius: '0.5em',
  },
  comment: { color: 'var(--muted-foreground)', fontStyle: 'italic' },
  string: { color: 'var(--primary)' },
  number: { color: 'var(--foreground)' },
  boolean: { color: 'var(--cyan)' },
  keyword: { color: 'var(--purple)' },
  function: { color: 'var(--cyan)' },
  className: { color: 'var(--cyan)' },
  operator: { color: 'var(--foreground)' },
  punctuation: { color: 'var(--foreground)' },
  tag: { color: 'var(--cyan)' },
  attrName: { color: 'var(--pink)' },
  attrValue: { color: 'var(--primary)' },
  plain: { color: 'var(--foreground)' },
  // HTML specific tokens
  'tag.tag': { color: 'var(--purple)' },
  'tag.attr-name': { color: 'var(--purple)' },
  'tag.attr-value': { color: 'var(--primary)' },
  'tag.punctuation': { color: 'var(--foreground)' },
  // CSS specific tokens
  property: { color: 'var(--purple)' },
  selector: { color: 'var(--violet)' },
  'selector.class': { color: 'var(--cyan)' },
  'selector.id': { color: 'var(--cyan)' },
  'selector.pseudo-class': { color: 'var(--violet)' },
  'selector.pseudo-element': { color: 'var(--violet)' },
  unit: { color: 'var(--foreground)' },
  color: { color: 'var(--primary)' },
  'function.function': { color: 'var(--cyan)' },
  'function.url': { color: 'var(--primary)' },
  // JSON specific tokens
  'property.string': { color: 'var(--cyan)' },
  'property.number': { color: 'var(--cyan)' },
  'property.boolean': { color: 'var(--cyan)' },
  'property.null': { color: 'var(--cyan)' },
  // Additional tokens for better coverage
  variable: { color: 'var(--cyan)' },
  constant: { color: 'var(--violet)' },
  symbol: { color: 'var(--violet)' },
  deleted: { color: 'var(--destructive)' },
  inserted: { color: 'var(--primary)' },
  changed: { color: 'var(--orange)' },
  important: { color: 'var(--destructive)', fontWeight: 'bold' },
  bold: { fontWeight: 'bold' },
  italic: { fontStyle: 'italic' },
  entity: { color: 'var(--cyan)' },
  url: { color: 'var(--primary)' },
  atrule: { color: 'var(--violet)' },
  rule: { color: 'var(--violet)' },
  regex: { color: 'var(--primary)' },
  prolog: { color: 'var(--muted-foreground)' },
  doctype: { color: 'var(--muted-foreground)' },
  cdata: { color: 'var(--muted-foreground)' },
  namespace: { color: 'var(--purple)' },
  builtin: { color: 'var(--cyan)' },
  char: { color: 'var(--primary)' },
  directive: { color: 'var(--violet)' },
  'directive.tag': { color: 'var(--violet)' },
  'directive.attr-name': { color: 'var(--cyan)' },
  'directive.attr-value': { color: 'var(--primary)' },
  'directive.punctuation': { color: 'var(--foreground)' },
});

// Export the theme factory function
export const createPrismTheme = createIvyPrismTheme;

// Export a default theme instance for backward compatibility
const ivyPrismTheme = createIvyPrismTheme();

export default ivyPrismTheme;
