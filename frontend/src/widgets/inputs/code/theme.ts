import { Extension } from '@codemirror/state';
import { EditorView } from '@codemirror/view';
import { HighlightStyle, syntaxHighlighting } from '@codemirror/language';
import { tags } from '@lezer/highlight';

/**
 * Creates a CodeMirror theme that uses CSS variables for dynamic theming.
 * This replaces the massive CSS file with a proper theme system.
 */
export function createIvyCodeTheme(): Extension {
  // Base editor styles using CSS variables
  const baseTheme = EditorView.theme({
    '&': {
      height: '100%',
      maxHeight: '100%',
      width: '100%',
      maxWidth: '100%',
      overflow: 'auto',
      boxSizing: 'border-box',
      fontFamily: 'var(--font-mono)',
      fontSize: '14px',
      lineHeight: '1.5',
      backgroundColor: 'var(--background)',
      color: 'var(--foreground)',
    },
    '.cm-scroller': {
      height: '100%',
      overflow: 'auto',
      width: '100%',
      maxWidth: '100%',
      backgroundColor: 'var(--background)',
    },
    '.cm-content': {
      padding: '10px',
      wordWrap: 'break-word',
      whiteSpace: 'pre-wrap',
      width: 'fit-content',
      maxWidth: '100%',
      boxSizing: 'border-box',
      color: 'var(--foreground)',
      backgroundColor: 'var(--background)',
    },
    '.cm-line': {
      whiteSpace: 'pre-wrap',
      wordBreak: 'break-word',
      maxWidth: '100%',
      backgroundColor: 'var(--background)',
    },
    '.cm-gutters': {
      height: '100%',
      backgroundColor: 'var(--muted)',
      borderRight: '1px solid var(--border)',
      color: 'var(--muted-foreground)',
    },
    '.cm-gutterElement': {
      color: 'var(--muted-foreground)',
    },
    '.cm-focused': {
      outline: 'none',
      borderColor: 'var(--ring)',
    },
    '.cm-activeLine': {
      backgroundColor: 'var(--accent)',
    },
    '.cm-activeLineGutter': {
      backgroundColor: 'var(--accent)',
    },
    '.cm-activeLineGutter .cm-gutterElement': {
      color: 'var(--foreground)',
      fontWeight: 'bold',
    },
    '.cm-selectionBackground': {
      backgroundColor: 'var(--accent)',
    },
    '.cm-selectionMatch': {
      backgroundColor: 'var(--accent)',
    },
    '.cm-cursor': {
      borderLeftColor: 'var(--foreground)',
    },
    // Ensure all editor areas have proper background
    '.cm-editor .cm-content': {
      backgroundColor: 'var(--background)',
    },
    '.cm-editor .cm-scroller': {
      backgroundColor: 'var(--background)',
    },
    '.cm-editor': {
      backgroundColor: 'var(--background)',
    },
    // Container classes for proper sizing
    '.w-tc-editor': {
      height: '100%',
      width: '100%',
      maxWidth: '100%',
      display: 'flex',
      flexDirection: 'column',
      boxSizing: 'border-box',
      backgroundColor: 'var(--background)',
    },
    '.w-tc-editor-text': {
      flex: '1',
      overflow: 'auto',
      width: '100%',
      maxWidth: '100%',
      backgroundColor: 'var(--background)',
    },
  });

  // Syntax highlighting using semantic tags instead of CSS classes
  const highlightStyle = HighlightStyle.define([
    // Comments
    {
      tag: tags.comment,
      color: 'var(--muted-foreground)',
      fontStyle: 'italic',
    },
    {
      tag: tags.lineComment,
      color: 'var(--muted-foreground)',
      fontStyle: 'italic',
    },
    {
      tag: tags.blockComment,
      color: 'var(--muted-foreground)',
      fontStyle: 'italic',
    },

    // Strings and literals
    { tag: tags.string, color: 'var(--primary)' },
    { tag: tags.character, color: 'var(--primary)' },
    { tag: tags.regexp, color: 'var(--primary)' },
    { tag: tags.escape, color: 'var(--red)' },

    // Numbers and constants
    { tag: tags.number, color: 'var(--foreground)' },
    { tag: tags.integer, color: 'var(--foreground)' },
    { tag: tags.float, color: 'var(--foreground)' },
    { tag: tags.bool, color: 'var(--purple)' },
    { tag: tags.null, color: 'var(--purple)' },
    { tag: tags.atom, color: 'var(--purple)' },

    // Keywords and operators
    { tag: tags.keyword, color: 'var(--purple)' },
    { tag: tags.self, color: 'var(--purple)' },
    { tag: tags.operator, color: 'var(--foreground)' },
    { tag: tags.operatorKeyword, color: 'var(--purple)' },
    { tag: tags.modifier, color: 'var(--purple)' },
    { tag: tags.controlKeyword, color: 'var(--purple)' },
    { tag: tags.definitionKeyword, color: 'var(--purple)' },
    { tag: tags.moduleKeyword, color: 'var(--purple)' },

    // Names and identifiers
    { tag: tags.name, color: 'var(--foreground)' },
    { tag: tags.variableName, color: 'var(--foreground)' },
    { tag: tags.function(tags.variableName), color: 'var(--cyan)' },
    { tag: tags.function(tags.propertyName), color: 'var(--cyan)' },
    { tag: tags.propertyName, color: 'var(--foreground)' },
    { tag: tags.attributeName, color: 'var(--cyan)' },
    { tag: tags.className, color: 'var(--cyan)' },
    { tag: tags.labelName, color: 'var(--foreground)' },

    // Types and definitions
    { tag: tags.typeName, color: 'var(--cyan)' },
    { tag: tags.namespace, color: 'var(--cyan)' },
    { tag: tags.macroName, color: 'var(--purple)' },
    { tag: tags.definition(tags.variableName), color: 'var(--cyan)' },
    { tag: tags.definition(tags.propertyName), color: 'var(--cyan)' },

    // Punctuation and structure
    { tag: tags.punctuation, color: 'var(--foreground)' },
    { tag: tags.bracket, color: 'var(--foreground)' },
    { tag: tags.paren, color: 'var(--foreground)' },
    { tag: tags.squareBracket, color: 'var(--foreground)' },
    { tag: tags.brace, color: 'var(--foreground)' },
    { tag: tags.separator, color: 'var(--foreground)' },

    // HTML/XML specific
    { tag: tags.tagName, color: 'var(--cyan)' },
    { tag: tags.angleBracket, color: 'var(--foreground)' },

    // Meta and special
    { tag: tags.meta, color: 'var(--muted-foreground)' },
    { tag: tags.processingInstruction, color: 'var(--muted-foreground)' },
    {
      tag: tags.docString,
      color: 'var(--muted-foreground)',
      fontStyle: 'italic',
    },

    // Emphasis and formatting
    { tag: tags.emphasis, fontStyle: 'italic' },
    { tag: tags.strong, fontWeight: 'bold' },
    { tag: tags.strikethrough, textDecoration: 'line-through' },
    { tag: tags.link, color: 'var(--primary)', textDecoration: 'underline' },
    { tag: tags.heading, color: 'var(--purple)', fontWeight: 'bold' },

    // Invalid and error states
    { tag: tags.invalid, color: 'var(--destructive)' },
    { tag: tags.deleted, color: 'var(--destructive)' },
    { tag: tags.inserted, color: 'var(--primary)' },
    { tag: tags.changed, color: 'var(--orange)' },
  ]);

  return [baseTheme, syntaxHighlighting(highlightStyle)];
}
