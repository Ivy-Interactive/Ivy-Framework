import { useState, useEffect, useCallback, useRef, useReducer } from 'react';
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

  const [devState, dispatchDev] = useReducer(
    (
      state: {
        highlightedWidget: WidgetInfo | null;
        widgetStack: HTMLElement[];
        dialogWidget: WidgetInfo | null;
        dialogAction: DialogAction;
        dialogText: string;
        clickPosition: { x: number; y: number };
      },
      action:
        | Partial<typeof state>
        | ((prev: typeof state) => Partial<typeof state>)
    ) => {
      const updates = typeof action === 'function' ? action(state) : action;
      return { ...state, ...updates };
    },
    {
      highlightedWidget: null,
      widgetStack: [],
      dialogWidget: null,
      dialogAction: 'modify',
      dialogText: '',
      clickPosition: { x: 0, y: 0 },
    }
  );
  const {
    highlightedWidget,
    widgetStack,
    dialogWidget,
    dialogAction,
    dialogText,
    clickPosition,
  } = devState;

  const textareaRef = useRef<HTMLTextAreaElement>(null);

  const getWidgetInfo = useCallback((element: HTMLElement): WidgetInfo => {
    const widgetId = element.getAttribute('id')!;
    const widgetType = element.getAttribute('type')!;
    const bounds = getWidgetBounds(element);
    const callSite = widgetCallSiteRegistry.get(widgetId);
    return { id: widgetId, type: widgetType, element, bounds, callSite };
  }, []);

  const closeDialog = useCallback(() => {
    if (dialogWidget && dialogAction === 'text-edit') {
      clearWidgetContentOverride(dialogWidget.id);
    }
    dispatchDev({
      dialogWidget: null,
      dialogText: '',
      dialogAction: 'modify',
      highlightedWidget: null,
    });
  }, [dialogWidget, dialogAction, dispatchDev]);

  const postChange = useCallback(
    (forward: boolean) => {
      if (!dialogWidget) return;
      const finalText = dialogText;

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
    [dialogWidget, dialogAction, dialogText, closeDialog]
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
        dispatchDev({ dialogText: text });
        setWidgetContentOverride(dialogWidget.id, text);
      } else if (dialogAction === action) {
        return;
      } else {
        dispatchDev({ dialogText: '' });
      }

      dispatchDev({ dialogAction: action });
    },
    [dialogWidget, dialogAction, dispatchDev]
  );

  const handleTextChange = useCallback(
    (value: string) => {
      dispatchDev({ dialogText: value });
      if (dialogAction === 'text-edit' && dialogWidget) {
        setWidgetContentOverride(dialogWidget.id, value);
      }
    },
    [dialogAction, dialogWidget, dispatchDev]
  );

  const handleMouseOver = useCallback(
    (e: MouseEvent) => {
      if (dialogWidget) return;

      const target = e.target as HTMLElement;
      if (target.closest('.ivy-devtools')) return;

      e.stopPropagation();

      const widgetWrapper = target.closest('ivy-widget') as HTMLElement | null;
      if (!widgetWrapper) {
        dispatchDev({ highlightedWidget: null, widgetStack: [] });
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
      dispatchDev({
        widgetStack: stack,
        highlightedWidget: getWidgetInfo(widgetWrapper),
      });
    },
    [dialogWidget, getWidgetInfo, dispatchDev]
  );

  const handleWheel = useCallback(
    (e: WheelEvent) => {
      if (widgetStack.length === 0 || dialogWidget) return;

      const target = e.target as HTMLElement;
      if (target.closest('.ivy-devtools')) return;

      e.stopPropagation();
      e.stopPropagation();

      const currentIndex = widgetStack.findIndex(
        el => el === highlightedWidget?.element
      );
      if (currentIndex === -1) return;

      let newIndex = currentIndex;
      if (e.deltaY < 0) {
        newIndex = Math.min(currentIndex + 1, widgetStack.length - 1);
      } else {
        newIndex = Math.max(currentIndex - 1, 0);
      }

      if (newIndex !== currentIndex) {
        dispatchDev({
          highlightedWidget: getWidgetInfo(widgetStack[newIndex]),
        });
      }
    },
    [widgetStack, highlightedWidget, dialogWidget, getWidgetInfo, dispatchDev]
  );

  const handleClick = useCallback(
    (e: MouseEvent) => {
      if (dialogWidget) return;

      const target = e.target as HTMLElement;
      if (target.closest('.ivy-devtools')) return;

      e.preventDefault();
      e.stopPropagation();

      if (!highlightedWidget) return;

      dispatchDev({
        clickPosition: { x: e.clientX, y: e.clientY },
        dialogWidget: highlightedWidget,
        dialogAction: 'modify',
        dialogText: '',
      });
      setTimeout(() => textareaRef.current?.focus(), 0);
    },
    [highlightedWidget, dialogWidget, dispatchDev]
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
    },
    [dialogWidget, closeDialog, handleAdd]
  );

  useEffect(() => {
    if (!enabled) return;

    if (!dialogWidget) {
      document.addEventListener('mouseover', handleMouseOver, true);
      document.addEventListener('click', handleClick, true);
      document.addEventListener('wheel', handleWheel, {
        passive: true,
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

  const displayValue = dialogText;

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
            />
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
