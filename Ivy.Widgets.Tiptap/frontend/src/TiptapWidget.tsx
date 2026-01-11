import React, { useCallback, useEffect } from 'react';
import { useEditor, EditorContent } from '@tiptap/react';
import StarterKit from '@tiptap/starter-kit';
import Placeholder from '@tiptap/extension-placeholder';

/**
 * Event handler type provided by Ivy to external widgets.
 */
type IvyEventHandler = (
  eventName: string,
  widgetId: string,
  args: unknown[]
) => void;

/**
 * Props interface matching the C# Tiptap widget properties.
 */
interface TiptapWidgetProps {
  id: string;
  content?: string;
  placeholder?: string;
  editable?: boolean;
  autoFocus?: boolean;
  minHeight?: number;
  maxHeight?: number;
  showToolbar?: boolean;
  events?: string[];
  onIvyEvent?: IvyEventHandler;
}

interface ToolbarButtonProps {
  onClick: () => void;
  isActive?: boolean;
  disabled?: boolean;
  title: string;
  children: React.ReactNode;
}

const ToolbarButton: React.FC<ToolbarButtonProps> = ({
  onClick,
  isActive,
  disabled,
  title,
  children,
}) => (
  <button
    type="button"
    onClick={onClick}
    disabled={disabled}
    title={title}
    className={`p-1.5 rounded transition-colors ${
      isActive
        ? 'bg-primary text-primary-foreground'
        : 'hover:bg-muted text-foreground'
    } ${disabled ? 'opacity-50 cursor-not-allowed' : 'cursor-pointer'}`}
  >
    {children}
  </button>
);

const ToolbarDivider: React.FC = () => (
  <div className="w-px h-6 bg-border mx-1" />
);

/**
 * Tiptap - A rich text editor widget for Ivy.
 */
export const TiptapWidget: React.FC<TiptapWidgetProps> = ({
  id,
  content = '',
  placeholder,
  editable = true,
  autoFocus = false,
  minHeight,
  maxHeight,
  showToolbar = true,
  events = [],
  onIvyEvent,
}) => {
  const hasChangeHandler = events.includes('OnChange');
  const hasFocusHandler = events.includes('OnFocus');
  const hasBlurHandler = events.includes('OnBlur');

  const editor = useEditor({
    extensions: [
      StarterKit,
      Placeholder.configure({
        placeholder: placeholder ?? 'Start typing...',
      }),
    ],
    content,
    editable,
    autofocus: autoFocus,
    onUpdate: ({ editor }) => {
      if (hasChangeHandler && onIvyEvent) {
        onIvyEvent('OnChange', id, [editor.getHTML()]);
      }
    },
    onFocus: () => {
      if (hasFocusHandler && onIvyEvent) {
        onIvyEvent('OnFocus', id, []);
      }
    },
    onBlur: () => {
      if (hasBlurHandler && onIvyEvent) {
        onIvyEvent('OnBlur', id, []);
      }
    },
  });

  // Update editor content when prop changes
  useEffect(() => {
    if (editor && content !== editor.getHTML()) {
      editor.commands.setContent(content);
    }
  }, [editor, content]);

  // Update editable state when prop changes
  useEffect(() => {
    if (editor) {
      editor.setEditable(editable);
    }
  }, [editor, editable]);

  const runCommand = useCallback(
    (command: () => boolean) => {
      command();
      editor?.chain().focus().run();
    },
    [editor]
  );

  if (!editor) {
    return null;
  }

  const editorStyle: React.CSSProperties = {
    minHeight: minHeight ? `${minHeight}px` : '150px',
    maxHeight: maxHeight ? `${maxHeight}px` : undefined,
    overflowY: maxHeight ? 'auto' : undefined,
  };

  return (
    <div className="rounded-lg border bg-card shadow-sm">
      {showToolbar && editable && (
        <div className="flex flex-wrap items-center gap-0.5 p-2 border-b bg-muted/30">
          {/* Text formatting */}
          <ToolbarButton
            onClick={() => runCommand(() => editor.chain().toggleBold().run())}
            isActive={editor.isActive('bold')}
            title="Bold (Ctrl+B)"
          >
            <BoldIcon />
          </ToolbarButton>
          <ToolbarButton
            onClick={() => runCommand(() => editor.chain().toggleItalic().run())}
            isActive={editor.isActive('italic')}
            title="Italic (Ctrl+I)"
          >
            <ItalicIcon />
          </ToolbarButton>
          <ToolbarButton
            onClick={() => runCommand(() => editor.chain().toggleStrike().run())}
            isActive={editor.isActive('strike')}
            title="Strikethrough"
          >
            <StrikethroughIcon />
          </ToolbarButton>
          <ToolbarButton
            onClick={() => runCommand(() => editor.chain().toggleCode().run())}
            isActive={editor.isActive('code')}
            title="Inline code"
          >
            <CodeIcon />
          </ToolbarButton>

          <ToolbarDivider />

          {/* Headings */}
          <ToolbarButton
            onClick={() => runCommand(() => editor.chain().toggleHeading({ level: 1 }).run())}
            isActive={editor.isActive('heading', { level: 1 })}
            title="Heading 1"
          >
            <H1Icon />
          </ToolbarButton>
          <ToolbarButton
            onClick={() => runCommand(() => editor.chain().toggleHeading({ level: 2 }).run())}
            isActive={editor.isActive('heading', { level: 2 })}
            title="Heading 2"
          >
            <H2Icon />
          </ToolbarButton>
          <ToolbarButton
            onClick={() => runCommand(() => editor.chain().toggleHeading({ level: 3 }).run())}
            isActive={editor.isActive('heading', { level: 3 })}
            title="Heading 3"
          >
            <H3Icon />
          </ToolbarButton>

          <ToolbarDivider />

          {/* Lists */}
          <ToolbarButton
            onClick={() => runCommand(() => editor.chain().toggleBulletList().run())}
            isActive={editor.isActive('bulletList')}
            title="Bullet list"
          >
            <BulletListIcon />
          </ToolbarButton>
          <ToolbarButton
            onClick={() => runCommand(() => editor.chain().toggleOrderedList().run())}
            isActive={editor.isActive('orderedList')}
            title="Numbered list"
          >
            <OrderedListIcon />
          </ToolbarButton>

          <ToolbarDivider />

          {/* Block elements */}
          <ToolbarButton
            onClick={() => runCommand(() => editor.chain().toggleBlockquote().run())}
            isActive={editor.isActive('blockquote')}
            title="Quote"
          >
            <QuoteIcon />
          </ToolbarButton>
          <ToolbarButton
            onClick={() => runCommand(() => editor.chain().toggleCodeBlock().run())}
            isActive={editor.isActive('codeBlock')}
            title="Code block"
          >
            <CodeBlockIcon />
          </ToolbarButton>
          <ToolbarButton
            onClick={() => runCommand(() => editor.chain().setHorizontalRule().run())}
            title="Horizontal rule"
          >
            <HorizontalRuleIcon />
          </ToolbarButton>

          <ToolbarDivider />

          {/* History */}
          <ToolbarButton
            onClick={() => editor.chain().undo().run()}
            disabled={!editor.can().undo()}
            title="Undo (Ctrl+Z)"
          >
            <UndoIcon />
          </ToolbarButton>
          <ToolbarButton
            onClick={() => editor.chain().redo().run()}
            disabled={!editor.can().redo()}
            title="Redo (Ctrl+Y)"
          >
            <RedoIcon />
          </ToolbarButton>
        </div>
      )}
      <EditorContent
        editor={editor}
        className="prose prose-sm max-w-none p-4 focus:outline-none [&_.ProseMirror]:outline-none [&_.ProseMirror]:min-h-[inherit] [&_.ProseMirror_p.is-editor-empty:first-child::before]:content-[attr(data-placeholder)] [&_.ProseMirror_p.is-editor-empty:first-child::before]:text-muted-foreground [&_.ProseMirror_p.is-editor-empty:first-child::before]:float-left [&_.ProseMirror_p.is-editor-empty:first-child::before]:h-0 [&_.ProseMirror_p.is-editor-empty:first-child::before]:pointer-events-none"
        style={editorStyle}
      />
    </div>
  );
};

// SVG Icons (simple, inline)
const BoldIcon = () => (
  <svg className="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
    <path d="M6 4h8a4 4 0 0 1 4 4 4 4 0 0 1-4 4H6z" />
    <path d="M6 12h9a4 4 0 0 1 4 4 4 4 0 0 1-4 4H6z" />
  </svg>
);

const ItalicIcon = () => (
  <svg className="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
    <line x1="19" y1="4" x2="10" y2="4" />
    <line x1="14" y1="20" x2="5" y2="20" />
    <line x1="15" y1="4" x2="9" y2="20" />
  </svg>
);

const StrikethroughIcon = () => (
  <svg className="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
    <path d="M16 4H9a3 3 0 0 0-3 3v0a3 3 0 0 0 3 3h6" />
    <path d="M8 20h7a3 3 0 0 0 3-3v0a3 3 0 0 0-3-3h-6" />
    <line x1="4" y1="12" x2="20" y2="12" />
  </svg>
);

const CodeIcon = () => (
  <svg className="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
    <polyline points="16 18 22 12 16 6" />
    <polyline points="8 6 2 12 8 18" />
  </svg>
);

const H1Icon = () => (
  <svg className="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
    <path d="M4 12h8" />
    <path d="M4 18V6" />
    <path d="M12 18V6" />
    <path d="M17 10v8" />
    <path d="M17 10l3-2" />
  </svg>
);

const H2Icon = () => (
  <svg className="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
    <path d="M4 12h8" />
    <path d="M4 18V6" />
    <path d="M12 18V6" />
    <path d="M21 18h-4c0-4 4-3 4-6 0-1.5-2-2.5-4-1" />
  </svg>
);

const H3Icon = () => (
  <svg className="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
    <path d="M4 12h8" />
    <path d="M4 18V6" />
    <path d="M12 18V6" />
    <path d="M17.5 10.5c1.5-1 3.5 0 3.5 1.5a2 2 0 0 1-2 2" />
    <path d="M17 17.5c2 1.5 4 .3 4-1.5a2 2 0 0 0-2-2" />
  </svg>
);

const BulletListIcon = () => (
  <svg className="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
    <line x1="9" y1="6" x2="20" y2="6" />
    <line x1="9" y1="12" x2="20" y2="12" />
    <line x1="9" y1="18" x2="20" y2="18" />
    <circle cx="5" cy="6" r="1" fill="currentColor" />
    <circle cx="5" cy="12" r="1" fill="currentColor" />
    <circle cx="5" cy="18" r="1" fill="currentColor" />
  </svg>
);

const OrderedListIcon = () => (
  <svg className="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
    <line x1="10" y1="6" x2="21" y2="6" />
    <line x1="10" y1="12" x2="21" y2="12" />
    <line x1="10" y1="18" x2="21" y2="18" />
    <text x="3" y="7" fontSize="6" fill="currentColor" stroke="none">1</text>
    <text x="3" y="13" fontSize="6" fill="currentColor" stroke="none">2</text>
    <text x="3" y="19" fontSize="6" fill="currentColor" stroke="none">3</text>
  </svg>
);

const QuoteIcon = () => (
  <svg className="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
    <path d="M3 21c3 0 7-1 7-8V5c0-1.25-.756-2.017-2-2H4c-1.25 0-2 .75-2 1.972V11c0 1.25.75 2 2 2 1 0 1 0 1 1v1c0 1-1 2-2 2s-1 .008-1 1.031V21z" />
    <path d="M15 21c3 0 7-1 7-8V5c0-1.25-.757-2.017-2-2h-4c-1.25 0-2 .75-2 1.972V11c0 1.25.75 2 2 2h.75c0 2.25.25 4-2.75 4v3z" />
  </svg>
);

const CodeBlockIcon = () => (
  <svg className="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
    <rect x="3" y="3" width="18" height="18" rx="2" />
    <polyline points="9 8 5 12 9 16" />
    <polyline points="15 8 19 12 15 16" />
  </svg>
);

const HorizontalRuleIcon = () => (
  <svg className="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
    <line x1="3" y1="12" x2="21" y2="12" />
  </svg>
);

const UndoIcon = () => (
  <svg className="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
    <path d="M3 7v6h6" />
    <path d="M21 17a9 9 0 0 0-9-9 9 9 0 0 0-6 2.3L3 13" />
  </svg>
);

const RedoIcon = () => (
  <svg className="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
    <path d="M21 7v6h-6" />
    <path d="M3 17a9 9 0 0 1 9-9 9 9 0 0 1 6 2.3l3 2.7" />
  </svg>
);

export default TiptapWidget;
