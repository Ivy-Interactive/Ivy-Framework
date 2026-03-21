import React, { useCallback, useEffect, useMemo, useState, useRef } from 'react';
import { useEditor, EditorContent } from '@tiptap/react';
import './tiptap.css';
import StarterKit from '@tiptap/starter-kit';
import Placeholder from '@tiptap/extension-placeholder';
import {
  LuBold,
  LuItalic,
  LuStrikethrough,
  LuCode,
  LuHeading1,
  LuHeading2,
  LuHeading3,
  LuList,
  LuListOrdered,
  LuQuote,
  LuSquareCode,
  LuMinus,
  LuUndo2,
  LuRedo2,
} from 'react-icons/lu';
import { getWidth, getHeight } from './styles';
import { EventHandler } from './types';

interface TiptapInputProps {
  id: string;
  value?: string;
  placeholder?: string;
  disabled?: boolean;
  editable?: boolean;
  autoFocus?: boolean;
  width?: string;
  height?: string;
  showToolbar?: boolean;
  nullable?: boolean;
  events?: string[];
  eventHandler?: EventHandler;
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
        ? 'bg-secondary text-secondary-foreground'
        : 'hover:bg-muted text-foreground'
    } ${disabled ? 'opacity-50 cursor-not-allowed' : 'cursor-pointer'}`}
  >
    {children}
  </button>
);

const ToolbarDivider: React.FC = () => (
  <div className="w-px h-6 bg-border mx-1" />
);

export const TiptapInput: React.FC<TiptapInputProps> = ({
  id,
  value = '',
  placeholder,
  disabled = false,
  editable = true,
  autoFocus = false,
  width = 'Full',
  height = 'Full',
  showToolbar = true,
  events = [],
  eventHandler,
}) => {
  const hasChangeHandler = events.includes('OnChange');
  const hasFocusHandler = events.includes('OnFocus');
  const hasBlurHandler = events.includes('OnBlur');

  // Local state for immediate updates (like CodeInputWidget pattern)
  const [localValue, setLocalValue] = useState(value || '');
  const [isFocused, setIsFocused] = useState(false);
  const localValueRef = useRef(localValue);

  // Combine disabled and editable - disabled takes precedence
  const isEditable = !disabled && editable;

  // Update local value when server value changes and control is not focused
  useEffect(() => {
    if (!isFocused && value !== localValueRef.current) {
      queueMicrotask(() => setLocalValue(value || ''));
    }
  }, [value, isFocused]);

  // Keep ref in sync with state
  useEffect(() => {
    localValueRef.current = localValue;
  }, [localValue]);

  const editor = useEditor({
    extensions: [
      StarterKit,
      Placeholder.configure({
        placeholder: placeholder ?? 'Start typing...',
      }),
    ],
    content: localValue,
    editable: isEditable,
    autofocus: autoFocus,
    onUpdate: ({ editor }) => {
      const html = editor.getHTML();
      setLocalValue(html);
      if (hasChangeHandler && eventHandler) {
        eventHandler('OnChange', id, [html]);
      }
    },
    onFocus: () => {
      setIsFocused(true);
      if (hasFocusHandler && eventHandler) {
        eventHandler('OnFocus', id, []);
      }
    },
    onBlur: () => {
      setIsFocused(false);
      if (hasBlurHandler && eventHandler) {
        eventHandler('OnBlur', id, []);
      }
    },
  });

  // Sync localValue to editor when it changes (from server sync)
  useEffect(() => {
    if (editor && localValue !== editor.getHTML()) {
      editor.commands.setContent(localValue);
    }
  }, [editor, localValue]);

  // Update editable state when prop changes
  useEffect(() => {
    if (editor) {
      editor.setEditable(isEditable);
    }
  }, [editor, isEditable]);

  const runCommand = useCallback(
    (command: () => boolean) => {
      command();
      editor?.chain().focus().run();
    },
    [editor]
  );

  const containerStyle = useMemo<React.CSSProperties>(() => ({
    ...getWidth(width),
    ...getHeight(height),
  }), [width, height]);

  if (!editor) {
    return null;
  }

  return (
    <div
      className={`rounded-lg border bg-card shadow-sm flex flex-col ${disabled ? 'opacity-60' : ''}`}
      style={containerStyle}
    >
      {showToolbar && isEditable && (
        <div className="flex flex-wrap items-center gap-0.5 p-2 border-b bg-muted/30 shrink-0">
          {/* Text formatting */}
          <ToolbarButton
            onClick={() => runCommand(() => editor.chain().toggleBold().run())}
            isActive={editor.isActive('bold')}
            title="Bold (Ctrl+B)"
          >
            <LuBold size={16} />
          </ToolbarButton>
          <ToolbarButton
            onClick={() => runCommand(() => editor.chain().toggleItalic().run())}
            isActive={editor.isActive('italic')}
            title="Italic (Ctrl+I)"
          >
            <LuItalic size={16} />
          </ToolbarButton>
          <ToolbarButton
            onClick={() => runCommand(() => editor.chain().toggleStrike().run())}
            isActive={editor.isActive('strike')}
            title="Strikethrough"
          >
            <LuStrikethrough size={16} />
          </ToolbarButton>
          <ToolbarButton
            onClick={() => runCommand(() => editor.chain().toggleCode().run())}
            isActive={editor.isActive('code')}
            title="Inline code"
          >
            <LuCode size={16} />
          </ToolbarButton>

          <ToolbarDivider />

          {/* Headings */}
          <ToolbarButton
            onClick={() => runCommand(() => editor.chain().toggleHeading({ level: 1 }).run())}
            isActive={editor.isActive('heading', { level: 1 })}
            title="Heading 1"
          >
            <LuHeading1 size={16} />
          </ToolbarButton>
          <ToolbarButton
            onClick={() => runCommand(() => editor.chain().toggleHeading({ level: 2 }).run())}
            isActive={editor.isActive('heading', { level: 2 })}
            title="Heading 2"
          >
            <LuHeading2 size={16} />
          </ToolbarButton>
          <ToolbarButton
            onClick={() => runCommand(() => editor.chain().toggleHeading({ level: 3 }).run())}
            isActive={editor.isActive('heading', { level: 3 })}
            title="Heading 3"
          >
            <LuHeading3 size={16} />
          </ToolbarButton>

          <ToolbarDivider />

          {/* Lists */}
          <ToolbarButton
            onClick={() => runCommand(() => editor.chain().toggleBulletList().run())}
            isActive={editor.isActive('bulletList')}
            title="Bullet list"
          >
            <LuList size={16} />
          </ToolbarButton>
          <ToolbarButton
            onClick={() => runCommand(() => editor.chain().toggleOrderedList().run())}
            isActive={editor.isActive('orderedList')}
            title="Numbered list"
          >
            <LuListOrdered size={16} />
          </ToolbarButton>

          <ToolbarDivider />

          {/* Block elements */}
          <ToolbarButton
            onClick={() => runCommand(() => editor.chain().toggleBlockquote().run())}
            isActive={editor.isActive('blockquote')}
            title="Quote"
          >
            <LuQuote size={16} />
          </ToolbarButton>
          <ToolbarButton
            onClick={() => runCommand(() => editor.chain().toggleCodeBlock().run())}
            isActive={editor.isActive('codeBlock')}
            title="Code block"
          >
            <LuSquareCode size={16} />
          </ToolbarButton>
          <ToolbarButton
            onClick={() => runCommand(() => editor.chain().setHorizontalRule().run())}
            title="Horizontal rule"
          >
            <LuMinus size={16} />
          </ToolbarButton>

          <ToolbarDivider />

          {/* History */}
          <ToolbarButton
            onClick={() => editor.chain().undo().run()}
            disabled={!editor.can().undo()}
            title="Undo (Ctrl+Z)"
          >
            <LuUndo2 size={16} />
          </ToolbarButton>
          <ToolbarButton
            onClick={() => editor.chain().redo().run()}
            disabled={!editor.can().redo()}
            title="Redo (Ctrl+Y)"
          >
            <LuRedo2 size={16} />
          </ToolbarButton>
        </div>
      )}
      <EditorContent
        editor={editor}
        className="prose prose-sm max-w-none p-4 focus:outline-none flex-1 overflow-auto [&_.tiptap]:outline-none [&_.tiptap:focus]:outline-none [&_.ProseMirror]:outline-none [&_.ProseMirror:focus]:outline-none [&_.ProseMirror]:h-full [&_.ProseMirror_p.is-editor-empty:first-child::before]:content-[attr(data-placeholder)] [&_.ProseMirror_p.is-editor-empty:first-child::before]:text-muted-foreground [&_.ProseMirror_p.is-editor-empty:first-child::before]:float-left [&_.ProseMirror_p.is-editor-empty:first-child::before]:h-0 [&_.ProseMirror_p.is-editor-empty:first-child::before]:pointer-events-none"
      />
    </div>
  );
};

export default TiptapInput;
