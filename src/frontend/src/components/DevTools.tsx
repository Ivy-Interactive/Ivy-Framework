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
import { LuTrash2, LuTextCursor, LuCheck } from 'react-icons/lu';
import { FaMagic } from 'react-icons/fa';
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from '@/components/ui/tooltip';

// Types
type DevToolMode = 'off' | 'modify' | 'delete' | 'text-edit';

interface WidgetInfo {
  id: string;
  type: string;
  element: HTMLElement;
  bounds: DOMRect;
  callSite?: CallSite;
}

interface ChangeRequest {
  id: string;
  index: number;
  widgetId: string;
  widgetType: string;
  prompt: string;
  bounds: DOMRect;
  callSite?: CallSite;
  action: 'modify' | 'delete' | 'text-edit';
  currentContent?: string;
}

// Constants
const TEXT_EDITABLE_TYPES = ['Ivy.TextBlock', 'Ivy.Markdown'];

// Utility functions
function getWidgetBounds(wrapperElement: HTMLElement): DOMRect {
  const children = wrapperElement.children;
  if (children.length === 0) {
    return new DOMRect(0, 0, 0, 0);
  }

  let minX = Infinity;
  let minY = Infinity;
  let maxX = -Infinity;
  let maxY = -Infinity;

  for (let i = 0; i < children.length; i++) {
    const rect = children[i].getBoundingClientRect();
    if (rect.width === 0 && rect.height === 0) continue;
    minX = Math.min(minX, rect.left);
    minY = Math.min(minY, rect.top);
    maxX = Math.max(maxX, rect.right);
    maxY = Math.max(maxY, rect.bottom);
  }

  if (minX === Infinity) {
    return new DOMRect(0, 0, 0, 0);
  }

  return new DOMRect(minX, minY, maxX - minX, maxY - minY);
}

function formatWidgetType(type: string): string {
  return type.replace(/^Ivy\./, '');
}

function getPopoverPosition(bounds: DOMRect, offsetHeight = 240) {
  return {
    top: Math.min(bounds.top, window.innerHeight - offsetHeight),
    left: Math.min(bounds.right + 10, window.innerWidth - 320),
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
  if (trimmed.length > 50) {
    return trimmed.substring(0, 50).trim() + ' (truncated)';
  }
  return trimmed;
}

// Sub-components
interface MicButtonProps {
  listening: boolean;
  onClick: () => void;
}

function MicButton({ listening, onClick }: MicButtonProps) {
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

interface ToolbarButtonProps {
  icon: React.ReactNode;
  tooltip: string;
  isActive: boolean;
  activeClass: string;
  onClick: () => void;
}

function ToolbarButton({
  icon,
  tooltip,
  isActive,
  activeClass,
  onClick,
}: ToolbarButtonProps) {
  return (
    <Tooltip>
      <TooltipTrigger asChild>
        <button
          onClick={onClick}
          className={`ivy-devtools-icon-btn ${isActive ? activeClass : 'ivy-devtools-btn-muted'}`}
        >
          {icon}
        </button>
      </TooltipTrigger>
      <TooltipContent side="bottom" className="ivy-devtools-tooltip">
        {tooltip}
      </TooltipContent>
    </Tooltip>
  );
}

interface DevToolsPopoverProps {
  widget: WidgetInfo;
  value: string;
  transcript: string;
  listening: boolean;
  isEditing: boolean;
  placeholder: string;
  textareaRef: React.RefObject<HTMLTextAreaElement | null>;
  browserSupportsSpeechRecognition: boolean;
  onChange: (value: string) => void;
  onCancel: () => void;
  onDelete: () => void;
  onDone: () => void;
  onToggleDictation: () => void;
}

function DevToolsPopover({
  widget,
  value,
  transcript,
  listening,
  isEditing,
  placeholder,
  textareaRef,
  browserSupportsSpeechRecognition,
  onChange,
  onCancel,
  onDelete,
  onDone,
  onToggleDictation,
}: DevToolsPopoverProps) {
  const position = getPopoverPosition(widget.bounds);
  const displayValue = listening
    ? value + (transcript ? ' ' + transcript : '')
    : value;

  return (
    <div className="ivy-devtools ivy-devtools-popover" style={position}>
      <div className="ivy-devtools-popover-header">
        <span className="ivy-devtools-popover-type">
          {formatWidgetType(widget.type)}
        </span>
        {widget.callSite?.filePath && (
          <span className="ivy-devtools-callsite-muted">
            {widget.callSite.filePath.split(/[/\\]/).pop()}:
            {widget.callSite.lineNumber}
          </span>
        )}
      </div>
      <div className="ivy-devtools-textarea-wrapper">
        <textarea
          ref={textareaRef}
          value={displayValue}
          onChange={e => onChange(e.target.value)}
          placeholder={placeholder}
          className="ivy-devtools-textarea"
          readOnly={listening}
        />
        {browserSupportsSpeechRecognition && (
          <MicButton listening={listening} onClick={onToggleDictation} />
        )}
      </div>
      <div className="ivy-devtools-popover-actions">
        {isEditing ? (
          <button
            onClick={onDelete}
            className="ivy-devtools-btn ivy-devtools-btn-destructive"
          >
            Delete
          </button>
        ) : (
          <button
            onClick={onCancel}
            className="ivy-devtools-btn ivy-devtools-btn-muted"
          >
            Cancel
          </button>
        )}
        <button
          onClick={onDone}
          className="ivy-devtools-btn ivy-devtools-btn-primary"
        >
          Done
        </button>
      </div>
      <div className="ivy-devtools-popover-hint">
        Ctrl+Enter to save · Esc to {isEditing ? 'delete' : 'cancel'} ·
        Ctrl+Space to dictate
      </div>
    </div>
  );
}

// Main component
export function DevTools() {
  // Core state
  const [mode, setMode] = useState<DevToolMode>('off');
  const [highlightedWidget, setHighlightedWidget] = useState<WidgetInfo | null>(
    null
  );
  const [widgetStack, setWidgetStack] = useState<HTMLElement[]>([]);

  // Modify mode state
  const [selectedWidget, setSelectedWidget] = useState<WidgetInfo | null>(null);
  const [popoverText, setPopoverText] = useState('');

  // Text-edit mode state
  const [textEditWidget, setTextEditWidget] = useState<WidgetInfo | null>(null);
  const [textEditValue, setTextEditValue] = useState('');
  const [originalTextValue, setOriginalTextValue] = useState('');

  // Change requests state
  const [changeRequests, setChangeRequests] = useState<ChangeRequest[]>([]);
  const [editingRequestId, setEditingRequestId] = useState<string | null>(null);
  const [nextIndex, setNextIndex] = useState(1);

  // Refs
  const textareaRef = useRef<HTMLTextAreaElement>(null);
  const textEditRef = useRef<HTMLTextAreaElement>(null);

  // Speech recognition
  const {
    transcript,
    listening,
    resetTranscript,
    browserSupportsSpeechRecognition,
  } = useSpeechRecognition();

  // Helper functions
  const getWidgetInfo = useCallback((element: HTMLElement): WidgetInfo => {
    const widgetId = element.getAttribute('id')!;
    const widgetType = element.getAttribute('type')!;
    const bounds = getWidgetBounds(element);
    const callSite = widgetCallSiteRegistry.get(widgetId);
    return { id: widgetId, type: widgetType, element, bounds, callSite };
  }, []);

  const resetModeState = useCallback(() => {
    setHighlightedWidget(null);
    setWidgetStack([]);
    setSelectedWidget(null);
    setTextEditWidget(null);
  }, []);

  const stopDictation = useCallback(() => {
    if (listening) {
      SpeechRecognition.stopListening();
      resetTranscript();
    }
  }, [listening, resetTranscript]);

  const toggleDictation = useCallback(() => {
    if (listening) {
      SpeechRecognition.stopListening();
      if (transcript) {
        if (textEditWidget) {
          const newValue =
            (textEditValue ? textEditValue + ' ' : '') + transcript;
          setTextEditValue(newValue);
          setWidgetContentOverride(textEditWidget.id, newValue);
        } else {
          setPopoverText(prev => (prev ? prev + ' ' : '') + transcript);
        }
        resetTranscript();
      }
    } else {
      resetTranscript();
      SpeechRecognition.startListening({ continuous: true });
    }
  }, [listening, transcript, resetTranscript, textEditWidget, textEditValue]);

  // Mode toggle handler
  const handleModeToggle = useCallback(
    (newMode: DevToolMode) => {
      if (mode === newMode) {
        setMode('off');
      } else {
        setMode(newMode);
      }
      resetModeState();
    },
    [mode, resetModeState]
  );

  // Event handlers
  const handleMouseOver = useCallback(
    (e: MouseEvent) => {
      if (mode === 'off' || selectedWidget || textEditWidget) return;

      const target = e.target as HTMLElement;
      if (target.closest('.ivy-devtools')) return;

      e.stopPropagation();

      const widgetWrapper = target.closest('ivy-widget') as HTMLElement | null;
      if (!widgetWrapper) {
        setHighlightedWidget(null);
        setWidgetStack([]);
        return;
      }

      // In text-edit mode, only highlight text-editable widgets
      if (mode === 'text-edit') {
        const widgetType = widgetWrapper.getAttribute('type');
        if (!widgetType || !TEXT_EDITABLE_TYPES.includes(widgetType)) {
          setHighlightedWidget(null);
          setWidgetStack([]);
          return;
        }
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
    [mode, selectedWidget, textEditWidget, getWidgetInfo]
  );

  const handleWheel = useCallback(
    (e: WheelEvent) => {
      if (mode === 'off' || widgetStack.length === 0 || selectedWidget) return;

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
    [mode, widgetStack, highlightedWidget, selectedWidget, getWidgetInfo]
  );

  const handleClick = useCallback(
    (e: MouseEvent) => {
      if (mode === 'off' || selectedWidget || textEditWidget) return;

      const target = e.target as HTMLElement;
      if (target.closest('.ivy-devtools')) return;

      e.preventDefault();
      e.stopPropagation();

      if (!highlightedWidget) return;

      if (mode === 'modify') {
        setSelectedWidget(highlightedWidget);
        setPopoverText('');
        setEditingRequestId(null);
        resetTranscript();
        setTimeout(() => textareaRef.current?.focus(), 0);
      } else if (mode === 'delete') {
        const newRequest: ChangeRequest = {
          id: crypto.randomUUID(),
          index: nextIndex,
          widgetId: highlightedWidget.id,
          widgetType: highlightedWidget.type,
          prompt: 'Delete this widget',
          bounds: highlightedWidget.bounds,
          callSite: highlightedWidget.callSite,
          action: 'delete',
          currentContent: getTextContent(
            highlightedWidget.element,
            highlightedWidget.type
          ),
        };
        setChangeRequests(prev => [...prev, newRequest]);
        setNextIndex(prev => prev + 1);
      } else if (mode === 'text-edit') {
        const text =
          highlightedWidget.element.getAttribute('data-content') ||
          highlightedWidget.element.textContent ||
          '';
        setTextEditWidget(highlightedWidget);
        setTextEditValue(text);
        setOriginalTextValue(text);
        resetTranscript();
        setTimeout(() => {
          if (textEditRef.current) {
            textEditRef.current.focus();
            textEditRef.current.setSelectionRange(text.length, text.length);
          }
        }, 0);
      }
    },
    [
      mode,
      highlightedWidget,
      selectedWidget,
      textEditWidget,
      resetTranscript,
      nextIndex,
    ]
  );

  // Modify popover handlers
  const handleModifyCancel = useCallback(() => {
    stopDictation();
    setSelectedWidget(null);
    setPopoverText('');
    setEditingRequestId(null);
    setHighlightedWidget(null);
  }, [stopDictation]);

  const handleModifyDelete = useCallback(() => {
    stopDictation();
    if (editingRequestId) {
      setChangeRequests(prev => prev.filter(r => r.id !== editingRequestId));
    }
    setSelectedWidget(null);
    setPopoverText('');
    setEditingRequestId(null);
    setHighlightedWidget(null);
  }, [editingRequestId, stopDictation]);

  const handleModifyDone = useCallback(() => {
    if (listening) {
      SpeechRecognition.stopListening();
    }
    const finalText = popoverText + (transcript ? ' ' + transcript : '');
    resetTranscript();

    if (selectedWidget && finalText.trim()) {
      if (editingRequestId) {
        setChangeRequests(prev =>
          prev.map(r =>
            r.id === editingRequestId
              ? {
                  ...r,
                  prompt: finalText.trim(),
                  bounds: selectedWidget.bounds,
                }
              : r
          )
        );
      } else {
        const newRequest: ChangeRequest = {
          id: crypto.randomUUID(),
          index: nextIndex,
          widgetId: selectedWidget.id,
          widgetType: selectedWidget.type,
          prompt: finalText.trim(),
          bounds: selectedWidget.bounds,
          callSite: selectedWidget.callSite,
          action: 'modify',
          currentContent: getTextContent(
            selectedWidget.element,
            selectedWidget.type
          ),
        };
        setChangeRequests(prev => [...prev, newRequest]);
        setNextIndex(prev => prev + 1);
      }
    }
    setSelectedWidget(null);
    setPopoverText('');
    setEditingRequestId(null);
    setHighlightedWidget(null);
  }, [
    selectedWidget,
    popoverText,
    editingRequestId,
    nextIndex,
    listening,
    transcript,
    resetTranscript,
  ]);

  // Text-edit popover handlers
  const handleTextEditChange = useCallback(
    (newValue: string) => {
      setTextEditValue(newValue);
      if (textEditWidget) {
        setWidgetContentOverride(textEditWidget.id, newValue);
      }
    },
    [textEditWidget]
  );

  const handleTextEditCancel = useCallback(() => {
    stopDictation();
    if (textEditWidget) {
      clearWidgetContentOverride(textEditWidget.id);
    }
    setTextEditWidget(null);
    setTextEditValue('');
    setOriginalTextValue('');
    setEditingRequestId(null);
    setHighlightedWidget(null);
  }, [textEditWidget, stopDictation]);

  const handleTextEditDelete = useCallback(() => {
    stopDictation();
    if (textEditWidget) {
      clearWidgetContentOverride(textEditWidget.id);
    }
    if (editingRequestId) {
      setChangeRequests(prev => prev.filter(r => r.id !== editingRequestId));
    }
    setTextEditWidget(null);
    setTextEditValue('');
    setOriginalTextValue('');
    setEditingRequestId(null);
    setHighlightedWidget(null);
  }, [textEditWidget, editingRequestId, stopDictation]);

  const handleTextEditDone = useCallback(() => {
    if (listening) {
      SpeechRecognition.stopListening();
    }
    const finalText = textEditValue + (transcript ? ' ' + transcript : '');
    resetTranscript();

    if (textEditWidget && finalText !== originalTextValue) {
      setWidgetContentOverride(textEditWidget.id, finalText);

      if (editingRequestId) {
        setChangeRequests(prev =>
          prev.map(r =>
            r.id === editingRequestId
              ? { ...r, prompt: finalText, bounds: textEditWidget.bounds }
              : r
          )
        );
      } else {
        const newRequest: ChangeRequest = {
          id: crypto.randomUUID(),
          index: nextIndex,
          widgetId: textEditWidget.id,
          widgetType: textEditWidget.type,
          prompt: finalText,
          bounds: textEditWidget.bounds,
          callSite: textEditWidget.callSite,
          action: 'text-edit',
          currentContent: getTextContent(
            textEditWidget.element,
            textEditWidget.type
          ),
        };
        setChangeRequests(prev => [...prev, newRequest]);
        setNextIndex(prev => prev + 1);
      }
    }
    setTextEditWidget(null);
    setTextEditValue('');
    setOriginalTextValue('');
    setEditingRequestId(null);
    setHighlightedWidget(null);
  }, [
    textEditWidget,
    textEditValue,
    originalTextValue,
    nextIndex,
    editingRequestId,
    listening,
    transcript,
    resetTranscript,
  ]);

  // Keyboard handler
  const handleKeyDown = useCallback(
    (e: KeyboardEvent) => {
      if (e.key === 'Escape') {
        if (textEditWidget) {
          if (editingRequestId) {
            handleTextEditDelete();
          } else {
            handleTextEditCancel();
          }
        } else if (selectedWidget) {
          if (editingRequestId) {
            handleModifyDelete();
          } else {
            handleModifyCancel();
          }
        } else if (mode !== 'off') {
          setMode('off');
          resetModeState();
        }
      }
      if (e.key === 'Enter' && e.ctrlKey) {
        e.preventDefault();
        if (selectedWidget) handleModifyDone();
        else if (textEditWidget) handleTextEditDone();
      }
      if (
        e.key === ' ' &&
        e.ctrlKey &&
        (selectedWidget || textEditWidget) &&
        browserSupportsSpeechRecognition
      ) {
        e.preventDefault();
        toggleDictation();
      }
    },
    [
      mode,
      selectedWidget,
      textEditWidget,
      editingRequestId,
      resetModeState,
      handleModifyCancel,
      handleModifyDelete,
      handleModifyDone,
      handleTextEditCancel,
      handleTextEditDelete,
      handleTextEditDone,
      browserSupportsSpeechRecognition,
      toggleDictation,
    ]
  );

  // Marker click handler
  const handleMarkerClick = useCallback(
    (request: ChangeRequest) => {
      if (request.action === 'delete') {
        setChangeRequests(prev => prev.filter(r => r.id !== request.id));
        return;
      }

      const element = document.querySelector(
        `ivy-widget[id="${request.widgetId}"]`
      ) as HTMLElement | null;
      if (!element) return;

      const widgetInfo = getWidgetInfo(element);

      if (request.action === 'text-edit') {
        const originalContent = element.getAttribute('data-content') || '';
        setTextEditWidget(widgetInfo);
        setTextEditValue(request.prompt);
        setOriginalTextValue(originalContent);
        setEditingRequestId(request.id);
        resetTranscript();
        setTimeout(() => {
          if (textEditRef.current) {
            textEditRef.current.focus();
            textEditRef.current.setSelectionRange(
              request.prompt.length,
              request.prompt.length
            );
          }
        }, 0);
      } else {
        setSelectedWidget(widgetInfo);
        setPopoverText(request.prompt);
        setEditingRequestId(request.id);
        resetTranscript();
        setTimeout(() => {
          if (textareaRef.current) {
            textareaRef.current.focus();
            textareaRef.current.setSelectionRange(
              request.prompt.length,
              request.prompt.length
            );
          }
        }, 0);
      }
    },
    [getWidgetInfo, resetTranscript]
  );

  // Apply changes handler
  const handleApplyChanges = useCallback(() => {
    const payload = changeRequests.map(
      ({ widgetId, widgetType, prompt, callSite, action, currentContent }) => ({
        widgetId,
        widgetType,
        prompt,
        callSite,
        action,
        currentContent,
      })
    );
    window.parent.postMessage({ type: 'DEVTOOLS_APPLY_CHANGES', payload }, '*');
    setChangeRequests([]);
    setNextIndex(1);
    setMode('off');
    resetModeState();
  }, [changeRequests, resetModeState]);

  // Event listeners effect
  useEffect(() => {
    if (mode !== 'off' && !selectedWidget && !textEditWidget) {
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
    mode,
    selectedWidget,
    textEditWidget,
    handleMouseOver,
    handleClick,
    handleKeyDown,
    handleWheel,
  ]);

  // Highlight overlay effect
  useEffect(() => {
    if (!highlightedWidget || selectedWidget || textEditWidget) return;

    const { bounds, type } = highlightedWidget;
    if (bounds.width === 0 && bounds.height === 0) return;

    const overlay = document.createElement('div');
    overlay.className = `ivy-devtools ivy-devtools-overlay ${
      mode === 'delete'
        ? 'ivy-devtools-overlay-delete'
        : mode === 'text-edit'
          ? 'ivy-devtools-overlay-text-edit'
          : ''
    }`;
    Object.assign(overlay.style, {
      top: `${bounds.top}px`,
      left: `${bounds.left}px`,
      width: `${bounds.width}px`,
      height: `${bounds.height}px`,
    });

    const label = document.createElement('div');
    label.className = `ivy-devtools-label ${
      mode === 'delete'
        ? 'ivy-devtools-label-delete'
        : mode === 'text-edit'
          ? 'ivy-devtools-label-text-edit'
          : ''
    }`;
    label.innerHTML = `<div class="ivy-devtools-label-type">${formatWidgetType(type)}</div>`;
    overlay.appendChild(label);
    document.body.appendChild(overlay);

    return () => overlay.remove();
  }, [highlightedWidget, selectedWidget, textEditWidget, mode]);

  return (
    <div className="ivy-devtools-container">
      {/* Toolbar */}
      <TooltipProvider delayDuration={300}>
        <div className="ivy-devtools ivy-devtools-toolbar">
          {changeRequests.length > 0 && (
            <button
              onClick={handleApplyChanges}
              className="ivy-devtools-apply-btn"
            >
              <LuCheck size={16} />
              <span>Apply Changes</span>
              <span className="ivy-devtools-badge">
                {changeRequests.length}
              </span>
            </button>
          )}
          <ToolbarButton
            icon={<FaMagic size={14} />}
            tooltip="Modify"
            isActive={mode === 'modify'}
            activeClass="ivy-devtools-btn-primary"
            onClick={() => handleModeToggle('modify')}
          />
          <ToolbarButton
            icon={<LuTrash2 size={16} />}
            tooltip="Delete"
            isActive={mode === 'delete'}
            activeClass="ivy-devtools-btn-destructive"
            onClick={() => handleModeToggle('delete')}
          />
          <ToolbarButton
            icon={<LuTextCursor size={16} />}
            tooltip="Text Edit"
            isActive={mode === 'text-edit'}
            activeClass="ivy-devtools-btn-blue"
            onClick={() => handleModeToggle('text-edit')}
          />
        </div>
      </TooltipProvider>

      {/* Modify Popover */}
      {selectedWidget && (
        <DevToolsPopover
          widget={selectedWidget}
          value={popoverText}
          transcript={transcript}
          listening={listening}
          isEditing={!!editingRequestId}
          placeholder="Describe your change request..."
          textareaRef={textareaRef}
          browserSupportsSpeechRecognition={browserSupportsSpeechRecognition}
          onChange={setPopoverText}
          onCancel={handleModifyCancel}
          onDelete={handleModifyDelete}
          onDone={handleModifyDone}
          onToggleDictation={toggleDictation}
        />
      )}

      {/* Text-Edit Popover */}
      {textEditWidget && (
        <DevToolsPopover
          widget={textEditWidget}
          value={textEditValue}
          transcript={transcript}
          listening={listening}
          isEditing={!!editingRequestId}
          placeholder="Enter text..."
          textareaRef={textEditRef}
          browserSupportsSpeechRecognition={browserSupportsSpeechRecognition}
          onChange={handleTextEditChange}
          onCancel={handleTextEditCancel}
          onDelete={handleTextEditDelete}
          onDone={handleTextEditDone}
          onToggleDictation={toggleDictation}
        />
      )}

      {/* Change Request Markers */}
      {changeRequests.map(request => (
        <div
          key={request.id}
          className={`ivy-devtools ivy-devtools-marker ${
            request.action === 'delete'
              ? 'ivy-devtools-marker-delete'
              : request.action === 'text-edit'
                ? 'ivy-devtools-marker-text-edit'
                : 'ivy-devtools-marker-modify'
          }`}
          style={{
            top: request.bounds.top - 10,
            left: request.bounds.left - 10,
          }}
          onClick={() => handleMarkerClick(request)}
        >
          {request.index}
        </div>
      ))}
    </div>
  );
}
