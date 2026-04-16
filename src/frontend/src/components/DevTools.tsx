import { useState, useEffect, useCallback, useRef, useReducer } from "react";
import "./devtools.css";
import { CallSite } from "@/types/widgets";
import { widgetCallSiteRegistry } from "@/widgets/widgetRenderer";
import { LuSend, LuPlus, LuChevronUp } from "react-icons/lu";
import { setThemeGlobal } from "@/components/theme-provider";

interface WidgetInfo {
  id: string;
  type: string;
  element: HTMLElement;
  bounds: DOMRect;
  callSite?: CallSite;
}

const TEXT_EDITABLE_TYPES = ["Ivy.TextBlock", "Ivy.Markdown"];

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
  return type.replace(/^Ivy\./, "");
}

function getDialogPosition(clickPos: { x: number; y: number }) {
  const dialogWidth = 380;
  const dialogHeight = 48;
  return {
    top: Math.min(clickPos.y + 8, window.innerHeight - dialogHeight),
    left: Math.min(clickPos.x, window.innerWidth - dialogWidth),
  };
}

function getTextContent(element: HTMLElement, widgetType: string): string | undefined {
  if (!TEXT_EDITABLE_TYPES.includes(widgetType)) return undefined;
  const content = element.getAttribute("data-content") || element.textContent || "";
  if (!content) return undefined;
  const trimmed = content.trim();
  if (trimmed.length > 50) return trimmed.substring(0, 50).trim() + " (truncated)";
  return trimmed;
}

export function DevTools() {
  const [enabled, setEnabled] = useState(false);

  useEffect(() => {
    const handler = (e: MessageEvent) => {
      if (e.data?.type === "DEVTOOLS_SET_ENABLED") {
        setEnabled(e.data.token === "true");
      }
      if (e.data?.type === "SET_THEME_MODE") {
        const mode = e.data.mode;
        if (mode === "light" || mode === "dark") {
          setThemeGlobal(mode);
        }
      }
    };
    window.addEventListener("message", handler);
    return () => window.removeEventListener("message", handler);
  }, []);

  const [devState, dispatchDev] = useReducer(
    (
      state: {
        highlightedWidget: WidgetInfo | null;
        widgetStack: HTMLElement[];
        dialogWidget: WidgetInfo | null;
        dialogText: string;
        clickPosition: { x: number; y: number };
      },
      action: Partial<typeof state> | ((prev: typeof state) => Partial<typeof state>),
    ) => {
      const updates = typeof action === "function" ? action(state) : action;
      return { ...state, ...updates };
    },
    {
      highlightedWidget: null,
      widgetStack: [],
      dialogWidget: null,
      dialogText: "",
      clickPosition: { x: 0, y: 0 },
    },
  );
  const { highlightedWidget, widgetStack, dialogWidget, dialogText, clickPosition } = devState;

  const inputRef = useRef<HTMLInputElement>(null);
  const dialogRef = useRef<HTMLDivElement>(null);

  const getWidgetInfo = useCallback((element: HTMLElement): WidgetInfo => {
    const widgetId = element.getAttribute("id")!;
    const widgetType = element.getAttribute("type")!;
    const bounds = getWidgetBounds(element);
    const callSite = widgetCallSiteRegistry.get(widgetId);
    return { id: widgetId, type: widgetType, element, bounds, callSite };
  }, []);

  const closeDialog = useCallback(() => {
    dispatchDev({
      dialogWidget: null,
      dialogText: "",
      highlightedWidget: null,
    });
  }, [dispatchDev]);

  const postChange = useCallback(
    (forward: boolean) => {
      if (!dialogWidget) return;
      const prompt = dialogText.trim();

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
          action: "modify",
          currentContent: getTextContent(dialogWidget.element, dialogWidget.type),
          forward,
        },
      ];

      window.parent.postMessage({ type: "DEVTOOLS_APPLY_CHANGES", payload, forward }, "*");
      closeDialog();
    },
    [dialogWidget, dialogText, closeDialog],
  );

  const handleAdd = useCallback(() => postChange(false), [postChange]);
  const handleSend = useCallback(() => postChange(true), [postChange]);

  const handleSelectParent = useCallback(() => {
    if (!dialogWidget) return;
    const currentIndex = widgetStack.findIndex((el) => el === dialogWidget.element);
    const parentIndex = currentIndex + 1;
    if (parentIndex < widgetStack.length) {
      dispatchDev({ dialogWidget: getWidgetInfo(widgetStack[parentIndex]) });
    }
  }, [dialogWidget, widgetStack, getWidgetInfo, dispatchDev]);

  const handleMouseOver = useCallback(
    (e: MouseEvent) => {
      if (dialogWidget) return;

      const target = e.target as HTMLElement;
      if (target.closest(".ivy-devtools")) return;

      e.stopPropagation();

      const widgetWrapper = target.closest("ivy-widget") as HTMLElement | null;
      if (!widgetWrapper) {
        dispatchDev({ highlightedWidget: null, widgetStack: [] });
        return;
      }

      const stack: HTMLElement[] = [];
      let current: HTMLElement | null = widgetWrapper;
      while (current) {
        stack.push(current);
        current = current.parentElement?.closest("ivy-widget") as HTMLElement | null;
      }
      dispatchDev({
        widgetStack: stack,
        highlightedWidget: getWidgetInfo(widgetWrapper),
      });
    },
    [dialogWidget, getWidgetInfo, dispatchDev],
  );

  const handleWheel = useCallback(
    (e: WheelEvent) => {
      if (widgetStack.length === 0 || dialogWidget) return;

      const target = e.target as HTMLElement;
      if (target.closest(".ivy-devtools")) return;

      e.stopPropagation();

      const currentIndex = widgetStack.findIndex((el) => el === highlightedWidget?.element);
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
    [widgetStack, highlightedWidget, dialogWidget, getWidgetInfo, dispatchDev],
  );

  const handleClick = useCallback(
    (e: MouseEvent) => {
      if (dialogWidget) return;

      const target = e.target as HTMLElement;
      if (target.closest(".ivy-devtools")) return;

      e.preventDefault();
      e.stopPropagation();

      if (!highlightedWidget) return;

      dispatchDev({
        clickPosition: { x: e.clientX, y: e.clientY },
        dialogWidget: highlightedWidget,
        dialogText: "",
      });
      setTimeout(() => inputRef.current?.focus(), 0);
    },
    [highlightedWidget, dialogWidget, dispatchDev],
  );

  const handleKeyDown = useCallback(
    (e: KeyboardEvent) => {
      if (e.key === "Escape" && dialogWidget) {
        closeDialog();
      }
      if (e.key === "Enter" && !e.shiftKey && dialogWidget) {
        e.preventDefault();
        handleSend();
      }
      if (e.key === "Enter" && e.shiftKey && dialogWidget) {
        e.preventDefault();
        handleAdd();
      }
    },
    [dialogWidget, closeDialog, handleSend, handleAdd],
  );

  useEffect(() => {
    if (!enabled) return;

    if (!dialogWidget) {
      document.addEventListener("mouseover", handleMouseOver, true);
      document.addEventListener("click", handleClick, true);
      document.addEventListener("wheel", handleWheel, {
        passive: true,
        capture: true,
      });
      document.body.style.cursor = "crosshair";

      document.addEventListener("keydown", handleKeyDown);

      return () => {
        document.removeEventListener("mouseover", handleMouseOver, true);
        document.removeEventListener("click", handleClick, true);
        document.removeEventListener("keydown", handleKeyDown);
        document.removeEventListener("wheel", handleWheel, true);
        document.body.style.cursor = "";
      };
    } else {
      const handleClickOutside = (e: MouseEvent) => {
        if (dialogRef.current && !dialogRef.current.contains(e.target as Node)) {
          closeDialog();
        }
      };
      document.addEventListener("mousedown", handleClickOutside, true);
      document.addEventListener("keydown", handleKeyDown);

      return () => {
        document.removeEventListener("mousedown", handleClickOutside, true);
        document.removeEventListener("keydown", handleKeyDown);
      };
    }
  }, [
    enabled,
    dialogWidget,
    handleMouseOver,
    handleClick,
    handleKeyDown,
    handleWheel,
    closeDialog,
  ]);

  useEffect(() => {
    if (!enabled) return;

    const activeWidget = dialogWidget ?? highlightedWidget;
    if (!activeWidget) return;

    const { bounds, type } = activeWidget;
    if (bounds.width === 0 && bounds.height === 0) return;

    const isSelected = !!dialogWidget;
    const overlay = document.createElement("div");
    overlay.className = `ivy-devtools ivy-devtools-overlay${isSelected ? " ivy-devtools-overlay--selected" : ""}`;
    Object.assign(overlay.style, {
      top: `${bounds.top}px`,
      left: `${bounds.left}px`,
      width: `${bounds.width}px`,
      height: `${bounds.height}px`,
    });

    const label = document.createElement("div");
    label.className = "ivy-devtools-label";
    label.innerHTML = `<div class="ivy-devtools-label-type">${formatWidgetType(type)}</div>`;
    overlay.appendChild(label);
    document.body.appendChild(overlay);

    return () => overlay.remove();
  }, [enabled, highlightedWidget, dialogWidget]);

  if (!enabled) return null;

  return (
    <div className="ivy-devtools-container">
      {dialogWidget && (
        <div
          ref={dialogRef}
          className="ivy-devtools ivy-devtools-dialog"
          style={getDialogPosition(clickPosition)}
        >
          <input
            ref={inputRef}
            type="text"
            value={dialogText}
            onChange={(e) => dispatchDev({ dialogText: e.target.value })}
            placeholder="Write anything..."
            className="ivy-devtools-input"
          />
          <button onClick={handleSend} className="ivy-devtools-icon-btn" title="Send (Enter)">
            <LuSend size={14} />
          </button>
          <button onClick={handleAdd} className="ivy-devtools-icon-btn" title="Queue (Shift+Enter)">
            <LuPlus size={14} />
          </button>
          <button
            onClick={handleSelectParent}
            className="ivy-devtools-icon-btn"
            title="Select parent"
          >
            <LuChevronUp size={14} />
          </button>
        </div>
      )}
    </div>
  );
}
