import { useState, useEffect, useCallback, useRef } from 'react';
import SpeechRecognition, {
  useSpeechRecognition,
} from 'react-speech-recognition';
import { CallSite } from '@/types/widgets';
import { widgetCallSiteRegistry } from '@/widgets/widgetRenderer';

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
  description: string;
  bounds: DOMRect;
  callSite?: CallSite;
}

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

export function DevTools() {
  const [inspectMode, setInspectMode] = useState(false);
  const [highlightedWidget, setHighlightedWidget] = useState<WidgetInfo | null>(
    null
  );
  const [widgetStack, setWidgetStack] = useState<HTMLElement[]>([]);
  const [selectedWidget, setSelectedWidget] = useState<WidgetInfo | null>(null);
  const [changeRequests, setChangeRequests] = useState<ChangeRequest[]>([]);
  const [popoverText, setPopoverText] = useState('');
  const [editingRequestId, setEditingRequestId] = useState<string | null>(null);
  const [nextIndex, setNextIndex] = useState(1);
  const textareaRef = useRef<HTMLTextAreaElement>(null);

  const {
    transcript,
    listening,
    resetTranscript,
    browserSupportsSpeechRecognition,
  } = useSpeechRecognition();

  const toggleDictation = useCallback(() => {
    if (listening) {
      SpeechRecognition.stopListening();
      // Append final transcript
      if (transcript) {
        setPopoverText(prev => (prev ? prev + ' ' : '') + transcript);
        resetTranscript();
      }
    } else {
      resetTranscript();
      SpeechRecognition.startListening({ continuous: true });
    }
  }, [listening, transcript, resetTranscript]);

  const getWidgetInfo = useCallback((element: HTMLElement): WidgetInfo => {
    const widgetId = element.getAttribute('id')!;
    const widgetType = element.getAttribute('type')!;
    const bounds = getWidgetBounds(element);
    const callSite = widgetCallSiteRegistry.get(widgetId);
    return { id: widgetId, type: widgetType, element, bounds, callSite };
  }, []);

  const handleMouseOver = useCallback(
    (e: MouseEvent) => {
      if (!inspectMode || selectedWidget) return;

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
        const parent = current.parentElement?.closest(
          'ivy-widget'
        ) as HTMLElement | null;
        current = parent;
      }
      setWidgetStack(stack);
      setHighlightedWidget(getWidgetInfo(widgetWrapper));
    },
    [inspectMode, selectedWidget, getWidgetInfo]
  );

  const handleWheel = useCallback(
    (e: WheelEvent) => {
      if (!inspectMode || widgetStack.length === 0 || selectedWidget) return;

      const target = e.target as HTMLElement;
      if (target.closest('.ivy-devtools')) return;

      e.preventDefault();
      e.stopPropagation();

      const currentIndex = widgetStack.findIndex(
        el => el === highlightedWidget?.element
      );
      if (currentIndex === -1) return;

      let newIndex: number;
      if (e.deltaY < 0) {
        newIndex = Math.min(currentIndex + 1, widgetStack.length - 1);
      } else {
        newIndex = Math.max(currentIndex - 1, 0);
      }

      if (newIndex !== currentIndex) {
        setHighlightedWidget(getWidgetInfo(widgetStack[newIndex]));
      }
    },
    [inspectMode, widgetStack, highlightedWidget, selectedWidget, getWidgetInfo]
  );

  const handleClick = useCallback(
    (e: MouseEvent) => {
      if (!inspectMode || selectedWidget) return;

      const target = e.target as HTMLElement;
      if (target.closest('.ivy-devtools')) return;

      e.preventDefault();
      e.stopPropagation();

      if (highlightedWidget) {
        setSelectedWidget(highlightedWidget);
        setPopoverText('');
        setEditingRequestId(null);
        resetTranscript();
        setTimeout(() => textareaRef.current?.focus(), 0);
      }
    },
    [inspectMode, highlightedWidget, selectedWidget, resetTranscript]
  );

  const handleDelete = useCallback(() => {
    if (listening) {
      SpeechRecognition.stopListening();
      resetTranscript();
    }
    if (editingRequestId) {
      setChangeRequests(prev => prev.filter(r => r.id !== editingRequestId));
    }
    setSelectedWidget(null);
    setPopoverText('');
    setEditingRequestId(null);
  }, [editingRequestId, listening, resetTranscript]);

  const handleOk = useCallback(() => {
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
                  description: finalText.trim(),
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
          description: finalText.trim(),
          bounds: selectedWidget.bounds,
          callSite: selectedWidget.callSite,
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

  const handleKeyDown = useCallback(
    (e: KeyboardEvent) => {
      if (e.key === 'Escape') {
        if (selectedWidget) {
          handleDelete();
        } else if (inspectMode) {
          setInspectMode(false);
          setHighlightedWidget(null);
          setWidgetStack([]);
        }
      }
      if (e.key === 'Enter' && e.ctrlKey && selectedWidget) {
        e.preventDefault();
        handleOk();
      }
      if (
        e.key === ' ' &&
        e.ctrlKey &&
        selectedWidget &&
        browserSupportsSpeechRecognition
      ) {
        e.preventDefault();
        toggleDictation();
      }
    },
    [
      inspectMode,
      selectedWidget,
      handleDelete,
      handleOk,
      browserSupportsSpeechRecognition,
      toggleDictation,
    ]
  );

  const handleMarkerClick = (request: ChangeRequest) => {
    const element = document.querySelector(
      `ivy-widget[id="${request.widgetId}"]`
    ) as HTMLElement | null;
    if (element) {
      const widgetInfo = getWidgetInfo(element);
      setSelectedWidget(widgetInfo);
      setPopoverText(request.description);
      setEditingRequestId(request.id);
      resetTranscript();
      setTimeout(() => textareaRef.current?.focus(), 0);
    }
  };

  const handleApplyChanges = () => {
    const payload = changeRequests.map(request => ({
      widgetId: request.widgetId,
      widgetType: request.widgetType,
      description: request.description,
      callSite: request.callSite,
    }));

    window.parent.postMessage({ type: 'DEVTOOLS_APPLY_CHANGES', payload }, '*');

    setChangeRequests([]);
    setNextIndex(1);
  };

  useEffect(() => {
    if (inspectMode && !selectedWidget) {
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
    inspectMode,
    selectedWidget,
    handleMouseOver,
    handleClick,
    handleKeyDown,
    handleWheel,
  ]);

  // Render highlight overlay
  useEffect(() => {
    if (!highlightedWidget || selectedWidget) return;

    const { bounds, type } = highlightedWidget;
    if (bounds.width === 0 && bounds.height === 0) return;

    const overlay = document.createElement('div');
    overlay.className = 'ivy-devtools-overlay';
    overlay.style.top = `${bounds.top}px`;
    overlay.style.left = `${bounds.left}px`;
    overlay.style.width = `${bounds.width}px`;
    overlay.style.height = `${bounds.height}px`;

    const label = document.createElement('div');
    label.className = 'ivy-devtools-label';
    label.innerHTML = `<div class="ivy-devtools-label-type">${type.replace(/^Ivy\./, '')}</div>`;
    overlay.appendChild(label);

    document.body.appendChild(overlay);

    return () => {
      overlay.remove();
    };
  }, [highlightedWidget, selectedWidget]);

  return (
    <>
      {/* Toolbar */}
      <div className="ivy-devtools ivy-devtools-toolbar">
        {changeRequests.length > 0 && (
          <button
            onClick={handleApplyChanges}
            className="ivy-devtools-btn ivy-devtools-btn-success"
          >
            Apply {changeRequests.length} change
            {changeRequests.length > 1 ? 's' : ''}
          </button>
        )}
        <button
          onClick={() => {
            setInspectMode(!inspectMode);
            if (inspectMode) {
              setHighlightedWidget(null);
              setWidgetStack([]);
              setSelectedWidget(null);
            }
          }}
          className={`ivy-devtools-btn ${inspectMode ? 'ivy-devtools-btn-primary' : 'ivy-devtools-btn-muted'}`}
          title={inspectMode ? 'Cancel (Esc)' : 'Select element to modify'}
        >
          <svg
            width="16"
            height="16"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth="2"
          >
            <path d="M3 3l7.07 16.97 2.51-7.39 7.39-2.51L3 3z" />
            <path d="M13 13l6 6" />
          </svg>
          {inspectMode ? 'Select Widget' : 'Modify'}
        </button>
      </div>

      {/* Popover for selected widget */}
      {selectedWidget && (
        <div
          className="ivy-devtools ivy-devtools-popover"
          style={{
            top: Math.min(selectedWidget.bounds.top, window.innerHeight - 240),
            left: Math.min(
              selectedWidget.bounds.right + 10,
              window.innerWidth - 320
            ),
          }}
        >
          <div className="ivy-devtools-popover-header">
            <span className="ivy-devtools-popover-type">
              {selectedWidget.type.replace(/^Ivy\./, '')}
              {selectedWidget.callSite && (
                <span className="ivy-devtools-callsite-muted">
                  @{selectedWidget.callSite.filePath.split(/[/\\]/).pop()}:
                  {selectedWidget.callSite.lineNumber}
                </span>
              )}
            </span>
          </div>
          <div className="ivy-devtools-textarea-wrapper">
            <textarea
              ref={textareaRef}
              value={
                listening
                  ? popoverText + (transcript ? ' ' + transcript : '')
                  : popoverText
              }
              onChange={e => setPopoverText(e.target.value)}
              placeholder="Describe your change request..."
              className="ivy-devtools-textarea"
              readOnly={listening}
            />
            {browserSupportsSpeechRecognition && (
              <button
                onClick={toggleDictation}
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
            )}
          </div>
          <div className="ivy-devtools-popover-actions">
            <button
              onClick={handleDelete}
              className="ivy-devtools-btn ivy-devtools-btn-destructive"
            >
              Delete
            </button>
            <button
              onClick={handleOk}
              className="ivy-devtools-btn ivy-devtools-btn-primary"
            >
              Ok
            </button>
          </div>
          <div className="ivy-devtools-popover-hint">
            Ctrl+Enter to save · Esc to delete · Ctrl+Space to dictate
          </div>
        </div>
      )}

      {/* Markers for saved change requests */}
      {changeRequests.map(request => (
        <div
          key={request.id}
          className="ivy-devtools ivy-devtools-marker"
          style={{
            top: request.bounds.top - 10,
            left: request.bounds.left - 10,
          }}
          onClick={() => handleMarkerClick(request)}
          title={request.description}
        >
          {request.index}
        </div>
      ))}
    </>
  );
}
