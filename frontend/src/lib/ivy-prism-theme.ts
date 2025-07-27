import { CSSProperties } from 'react';

// Create a theme that uses CSS variables for dynamic theming
const createIvyPrismTheme = (): Record<string, CSSProperties> => ({
  'code[class*="language-"]': {
    fontFamily: '"IBM Plex Mono", monospace',
    fontSize: '1em',
    background: 'transparent',
    color: 'var(--foreground)',
  },
  'pre[class*="language-"]': {
    fontFamily: '"IBM Plex Mono", monospace',
    fontSize: '1em',
    background: 'transparent',
    padding: '1em',
    margin: 0,
    color: 'var(--foreground)',
    overflow: 'auto',
    border: '1px solid var(--border)',
    borderRadius: '0.5em',
  },
  comment: { color: 'var(--muted-foreground)', fontStyle: 'italic' },
  string: { color: 'var(--primary)' },
  number: { color: 'var(--foreground)' },
  boolean: { color: 'var(--violet)' },
  keyword: { color: 'var(--violet)' },
  function: { color: 'var(--blue)' },
  className: { color: 'var(--blue)' },
  operator: { color: 'var(--foreground)' },
  punctuation: { color: 'var(--foreground)' },
  tag: { color: 'var(--violet)' },
  attrName: { color: 'var(--blue)' },
  attrValue: { color: 'var(--primary)' },
  plain: { color: 'var(--foreground)' },
  // HTML specific tokens
  'tag.tag': { color: 'var(--violet)' },
  'tag.attr-name': { color: 'var(--blue)' },
  'tag.attr-value': { color: 'var(--primary)' },
  'tag.punctuation': { color: 'var(--foreground)' },
  // CSS specific tokens
  property: { color: 'var(--blue)' },
  selector: { color: 'var(--violet)' },
  'selector.class': { color: 'var(--blue)' },
  'selector.id': { color: 'var(--blue)' },
  'selector.pseudo-class': { color: 'var(--violet)' },
  'selector.pseudo-element': { color: 'var(--violet)' },
  unit: { color: 'var(--foreground)' },
  color: { color: 'var(--primary)' },
  'function.function': { color: 'var(--blue)' },
  'function.url': { color: 'var(--primary)' },
  // JSON specific tokens
  'property.string': { color: 'var(--blue)' },
  'property.number': { color: 'var(--blue)' },
  'property.boolean': { color: 'var(--blue)' },
  'property.null': { color: 'var(--blue)' },
  // Additional tokens for better coverage
  variable: { color: 'var(--blue)' },
  constant: { color: 'var(--violet)' },
  symbol: { color: 'var(--violet)' },
  deleted: { color: 'var(--destructive)' },
  inserted: { color: 'var(--primary)' },
  changed: { color: 'var(--orange)' },
  important: { color: 'var(--destructive)', fontWeight: 'bold' },
  bold: { fontWeight: 'bold' },
  italic: { fontStyle: 'italic' },
  entity: { color: 'var(--blue)' },
  url: { color: 'var(--primary)' },
  atrule: { color: 'var(--violet)' },
  rule: { color: 'var(--violet)' },
  regex: { color: 'var(--primary)' },
  prolog: { color: 'var(--muted-foreground)' },
  doctype: { color: 'var(--muted-foreground)' },
  cdata: { color: 'var(--muted-foreground)' },
  namespace: { color: 'var(--muted-foreground)' },
  builtin: { color: 'var(--blue)' },
  char: { color: 'var(--primary)' },
  directive: { color: 'var(--violet)' },
  'directive.tag': { color: 'var(--violet)' },
  'directive.attr-name': { color: 'var(--blue)' },
  'directive.attr-value': { color: 'var(--primary)' },
  'directive.punctuation': { color: 'var(--foreground)' },
});

// Export the theme factory function
export const createPrismTheme = createIvyPrismTheme;

// Export a default theme instance for backward compatibility
const ivyPrismTheme = createIvyPrismTheme();

export default ivyPrismTheme;
