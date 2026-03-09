import { useState, useEffect, useCallback, useRef } from 'react';
import SpeechRecognition, {
  useSpeechRecognition,
} from 'react-speech-recognition';
import './devtools.css';
import { CallSite } from '@/types/widgets';
import {
  widgetCallSiteRegistry,
  setWidgetContentOverride,
  clearWidgetContentOverride,
} from '@/widgets/widgetRenderer';
import { LuTrash2, LuTextCursor, LuSend, LuPlus } from 'react-icons/lu';
import { FaMagic } from 'react-icons/fa';

type DialogAction = 'modify' | 'delete' | 'text-edit';

interface WidgetInfo {
  id: string;
  type: string;
  element: HTMLElement;
  bounds: DOMRect;
  callSite?: CallSite;
}

const TEXT_EDITABLE_TYPES = ['Ivy.TextBlock', 'Ivy.Markdown'];

function getWidgetBounds(wrapperElement: HTMLElement): DOMRect {
  const children = wrapperElement.children;
  if (children.length === 0) return new DOMRect(0, 0, 0, 0);

  let minX = Infinity,
    minY = Infinity,
    maxX = -Infinity,
    maxY = -Infinity;

  for (let i = 0; i < children.length; i++) {
    const rect = children[i].getBoundingClientRect();
    if (rect.width === 0 && rect.height === 0) continue;
    minX = Math.min(minX, rect.left);
    minY = Math.min(minY, rect.top);
    maxX = Math.max(maxX, rect.right);
    maxY = Math.max(maxY, rect.bottom);
  }

  if (minX === Infinity) return new DOMRect(0, 0, 0, 0);
  return new DOMRect(minX, minY, maxX - minX, maxY - minY);
}

function formatWidgetType(type: string): string {
  return type.replace(/^Ivy\./, '');
}

function getDialogPosition(clickPos: { x: number; y: number }) {
  const dialogWidth = 320;
  const dialogHeight = 280;
  return {
    top: Math.min(clickPos.y + 8, window.innerHeight - dialogHeight),
    left: Math.min(clickPos.x, window.innerWidth - dialogWidth),
  };
}

function getTextContent(
  element: HTMLElement,
  widgetType: string
): string | undefined {
  if (!TEXT_EDITABLE_TYPES.includes(widgetType)) return undefined;
  const content =
    element.getAttribute('data-content') || element.textContent || '';
  if (!content) return undefined;
  const trimmed = content.trim();
  if (trimmed.length > 50)
    return trimmed.substring(0, 50).trim() + ' (truncated)';
  return trimmed;
}

function MicButton({
  listening,
  onClick,
}: {
  listening: boolean;
  onClick: () => void;
}) {
  return (
    <button
      onClick={onClick}
      className={`ivy-devtools-mic-btn ${listening ? 'ivy-devtools-mic-active' : ''}`}
      title={listening ? 'Stop dictation' : 'Start dictation'}
      type="button"
    >
      <svg
        width="16"
        height="16"
        viewBox="0 0 24 24"
        fill="none"
        stroke="currentColor"
        strokeWidth="2"
      >
        <path d="M12 2a3 3 0 0 0-3 3v7a3 3 0 0 0 6 0V5a3 3 0 0 0-3-3Z" />
        <path d="M19 10v2a7 7 0 0 1-14 0v-2" />
        <line x1="12" x2="12" y1="19" y2="22" />
      </svg>
    </button>
  );
}

export function DevTools() {
  const [enabled, setEnabled] = useState(false);

  useEffect(() => {
    const handler = (e: MessageEvent) => {
      if (e.data?.type === 'DEVTOOLS_SET_ENABLED') {
        setEnabled(e.data.token === 'true');
      }
    };
    window.addEventListener('message', handler);
    return () => window.removeEventListener('message', handler);
  }, []);

  const [highlightedWidget, setHighlightedWidget] = useState<WidgetInfo | null>(
    null
  );
  const [widgetStack, setWidgetStack] = useState<HTMLElement[]>([]);

  const [dialogWidget, setDialogWidget] = useState<WidgetInfo | null>(null);
  const [dialogAction, setDialogAction] = useState<DialogAction>('modify');
  const [dialogText, setDialogText] = useState('');
  const [clickPosition, setClickPosition] = useState({ x: 0, y: 0 });

  const textareaRef = useRef<HTMLTextAreaElement>(null);

  const {
    transcript,
    listening,
    resetTranscript,
    browserSupportsSpeechRecognition,
  } = useSpeechRecognition();

  const getWidgetInfo = useCallback((element: HTMLElement): WidgetInfo => {
    const widgetId = element.getAttribute('id')!;
    const widgetType = element.getAttribute('type')!;
    const bounds = getWidgetBounds(element);
    const callSite = widgetCallSiteRegistry.get(widgetId);
    return { id: widgetId, type: widgetType, element, bounds, callSite };
  }, []);

  const closeDialog = useCallback(() => {
    if (listening) {
      SpeechRecognition.stopListening();
      resetTranscript();
    }
    if (dialogWidget && dialogAction === 'text-edit') {
      clearWidgetContentOverride(dialogWidget.id);
    }
    setDialogWidget(null);
    setDialogText('');
    setDialogAction('modify');
    setHighlightedWidget(null);
  }, [listening, resetTranscript, dialogWidget, dialogAction]);

  const toggleDictation = useCallback(() => {
    if (listening) {
      SpeechRecognition.stopListening();
      if (transcript) {
        setDialogText(prev => (prev ? prev + ' ' : '') + transcript);
        if (dialogAction === 'text-edit' && dialogWidget) {
          setDialogText(prev => {
            setWidgetContentOverride(dialogWidget.id, prev);
            return prev;
          });
        }
        resetTranscript();
      }
    } else {
      resetTranscript();
      SpeechRecognition.startListening({ continuous: true });
    }
  }, [listening, transcript, resetTranscript, dialogAction, dialogWidget]);

  const postChange = useCallback(
    (forward: boolean) => {
      if (!dialogWidget) return;
      if (listening) SpeechRecognition.stopListening();
      const finalText = dialogText + (transcript ? ' ' + transcript : '');
      resetTranscript();

      const prompt =
        dialogAction === 'delete'
          ? finalText.trim() || 'Delete this widget'
          : finalText.trim();

      if (!prompt) {
        closeDialog();
        return;
      }

      const payload = [
        {
          widgetId: dialogWidget.id,
          widgetType: dialogWidget.type,
          prompt,
          callSite: dialogWidget.callSite,
          action: dialogAction,
          currentContent: getTextContent(
            dialogWidget.element,
            dialogWidget.type
          ),
          forward,
        },
      ];

      window.parent.postMessage(
        { type: 'DEVTOOLS_APPLY_CHANGES', payload, forward },
        '*'
      );
      closeDialog();
    },
    [
      dialogWidget,
      dialogAction,
      dialogText,
      listening,
      transcript,
      resetTranscript,
      closeDialog,
    ]
  );

  const handleAdd = useCallback(() => postChange(false), [postChange]);
  const handleSend = useCallback(() => postChange(true), [postChange]);

  const handleActionChange = useCallback(
    (action: DialogAction) => {
      if (!dialogWidget) return;

      if (dialogAction === 'text-edit' && action !== 'text-edit') {
        clearWidgetContentOverride(dialogWidget.id);
      }

      if (action === 'text-edit') {
        const text =
          dialogWidget.element.getAttribute('data-content') ||
          dialogWidget.element.textContent ||
          '';
        setDialogText(text);
        setWidgetContentOverride(dialogWidget.id, text);
      } else if (dialogAction === action) {
        return;
      } else {
        setDialogText('');
      }

      setDialogAction(action);
    },
    [dialogWidget, dialogAction]
  );

  const handleTextChange = useCallback(
    (value: string) => {
      setDialogText(value);
      if (dialogAction === 'text-edit' && dialogWidget) {
        setWidgetContentOverride(dialogWidget.id, value);
      }
    },
    [dialogAction, dialogWidget]
  );

  const handleMouseOver = useCallback(
    (e: MouseEvent) => {
      if (dialogWidget) return;

      const target = e.target as HTMLElement;
      if (target.closest('.ivy-devtools')) return;

      e.stopPropagation();

      const widgetWrapper = target.closest('ivy-widget') as HTMLElement | null;
      if (!widgetWrapper) {
        setHighlightedWidget(null);
        setWidgetStack([]);
        return;
      }

      const stack: HTMLElement[] = [];
      let current: HTMLElement | null = widgetWrapper;
      while (current) {
        stack.push(current);
        current = current.parentElement?.closest(
          'ivy-widget'
        ) as HTMLElement | null;
      }
      setWidgetStack(stack);
      setHighlightedWidget(getWidgetInfo(widgetWrapper));
    },
    [dialogWidget, getWidgetInfo]
  );

  const handleWheel = useCallback(
    (e: WheelEvent) => {
      if (widgetStack.length === 0 || dialogWidget) return;

      const target = e.target as HTMLElement;
      if (target.closest('.ivy-devtools')) return;

      e.preventDefault();
      e.stopPropagation();

      const currentIndex = widgetStack.findIndex(
        el => el === highlightedWidget?.element
      );
      if (currentIndex === -1) return;

      const newIndex =
        e.deltaY < 0
          ? Math.min(currentIndex + 1, widgetStack.length - 1)
          : Math.max(currentIndex - 1, 0);

      if (newIndex !== currentIndex) {
        setHighlightedWidget(getWidgetInfo(widgetStack[newIndex]));
      }
    },
    [widgetStack, highlightedWidget, dialogWidget, getWidgetInfo]
  );

  const handleClick = useCallback(
    (e: MouseEvent) => {
      if (dialogWidget) return;

      const target = e.target as HTMLElement;
      if (target.closest('.ivy-devtools')) return;

      e.preventDefault();
      e.stopPropagation();

      if (!highlightedWidget) return;

      setClickPosition({ x: e.clientX, y: e.clientY });
      setDialogWidget(highlightedWidget);
      setDialogAction('modify');
      setDialogText('');
      resetTranscript();
      setTimeout(() => textareaRef.current?.focus(), 0);
    },
    [highlightedWidget, dialogWidget, resetTranscript]
  );

  const handleKeyDown = useCallback(
    (e: KeyboardEvent) => {
      if (e.key === 'Escape' && dialogWidget) {
        closeDialog();
      }
      if (e.key === 'Enter' && e.ctrlKey && dialogWidget) {
        e.preventDefault();
        handleAdd();
      }
      if (
        e.key === ' ' &&
        e.ctrlKey &&
        dialogWidget &&
        browserSupportsSpeechRecognition
      ) {
        e.preventDefault();
        toggleDictation();
      }
    },
    [
      dialogWidget,
      closeDialog,
      handleAdd,
      browserSupportsSpeechRecognition,
      toggleDictation,
    ]
  );

  useEffect(() => {
    if (!enabled) return;

    if (!dialogWidget) {
      document.addEventListener('mouseover', handleMouseOver, true);
      document.addEventListener('click', handleClick, true);
      document.addEventListener('wheel', handleWheel, {
        passive: false,
        capture: true,
      });
      document.body.style.cursor = 'crosshair';
    }

    document.addEventListener('keydown', handleKeyDown);

    return () => {
      document.removeEventListener('mouseover', handleMouseOver, true);
      document.removeEventListener('click', handleClick, true);
      document.removeEventListener('keydown', handleKeyDown);
      document.removeEventListener('wheel', handleWheel, true);
      document.body.style.cursor = '';
    };
  }, [
    enabled,
    dialogWidget,
    handleMouseOver,
    handleClick,
    handleKeyDown,
    handleWheel,
  ]);

  useEffect(() => {
    if (!enabled || !highlightedWidget || dialogWidget) return;

    const { bounds, type } = highlightedWidget;
    if (bounds.width === 0 && bounds.height === 0) return;

    const overlay = document.createElement('div');
    overlay.className = 'ivy-devtools ivy-devtools-overlay';
    Object.assign(overlay.style, {
      top: `${bounds.top}px`,
      left: `${bounds.left}px`,
      width: `${bounds.width}px`,
      height: `${bounds.height}px`,
    });

    const label = document.createElement('div');
    label.className = 'ivy-devtools-label';
    label.innerHTML = `<div class="ivy-devtools-label-type">${formatWidgetType(type)}</div>`;
    overlay.appendChild(label);
    document.body.appendChild(overlay);

    return () => overlay.remove();
  }, [enabled, highlightedWidget, dialogWidget]);

  const displayValue = listening
    ? dialogText + (transcript ? ' ' + transcript : '')
    : dialogText;

  if (!enabled) return null;

  return (
    <div className="ivy-devtools-container">
      {dialogWidget && (
        <div
          className="ivy-devtools ivy-devtools-dialog"
          style={getDialogPosition(clickPosition)}
        >
          <div className="ivy-devtools-dialog-toggles">
            <button
              className={`ivy-devtools-toggle-btn ${dialogAction === 'modify' ? 'ivy-devtools-toggle-active' : ''}`}
              onClick={() => handleActionChange('modify')}
            >
              <FaMagic size={12} />
              Change
            </button>
            <button
              className={`ivy-devtools-toggle-btn ${dialogAction === 'text-edit' ? 'ivy-devtools-toggle-active' : ''}`}
              onClick={() => handleActionChange('text-edit')}
            >
              <LuTextCursor size={14} />
              Edit Text
            </button>
            <button
              className={`ivy-devtools-toggle-btn ${dialogAction === 'delete' ? 'ivy-devtools-toggle-active' : ''}`}
              onClick={() => handleActionChange('delete')}
            >
              <LuTrash2 size={14} />
              Delete
            </button>
          </div>
          <div className="ivy-devtools-textarea-wrapper">
            <textarea
              ref={textareaRef}
              value={displayValue}
              onChange={e => handleTextChange(e.target.value)}
              placeholder="Write anything..."
              className="ivy-devtools-textarea"
              readOnly={listening}
            />
            {browserSupportsSpeechRecognition && (
              <MicButton listening={listening} onClick={toggleDictation} />
            )}
          </div>
          <div className="ivy-devtools-dialog-actions">
            <button
              onClick={handleAdd}
              className="ivy-devtools-btn ivy-devtools-btn-muted"
            >
              <LuPlus size={14} />
              Add To Prompt
            </button>
            <button
              onClick={handleSend}
              className="ivy-devtools-btn ivy-devtools-btn-outlined"
            >
              <LuSend size={14} />
              Send direct
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
