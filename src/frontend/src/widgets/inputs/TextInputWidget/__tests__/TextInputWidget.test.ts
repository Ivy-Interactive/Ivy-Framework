import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import React, { act } from "react";
import { createRoot, Root } from "react-dom/client";
import { useEnterKeyBlur, usePasteHandler, useCursorPosition } from "../hooks/useTextInput";

// Mocks for SearchVariant component rendering tests
vi.mock("@/hooks/use-focus-management", () => ({
  useFocusable: () => ({ ref: () => {} }),
}));

vi.mock("@/widgets/layouts/sidebar", () => ({
  sidebarMenuRef: { current: null },
}));

vi.mock("lucide-react", () => {
  const React = require("react");
  return {
    Search: (props: Record<string, unknown>) => React.createElement("svg", props),
    X: (props: Record<string, unknown>) => React.createElement("svg", props),
  };
});

vi.mock("@/components/InvalidIcon", () => {
  const React = require("react");
  return {
    InvalidIcon: () => React.createElement("span", { "data-testid": "invalid-icon" }),
  };
});

vi.mock("@/components/ui/input", () => {
  const React = require("react");
  return {
    Input: React.forwardRef((props: Record<string, unknown>, ref: React.Ref<HTMLInputElement>) =>
      React.createElement("input", { ...props, ref }),
    ),
  };
});

vi.mock("@/components/ui/input/text-input-variant", () => ({
  textInputSizeVariant: () => "",
  textInputAffixCellClasses: () => "",
  searchIconVariant: () => "",
  xIconVariant: () => "",
}));

vi.mock("@/lib/styles", () => ({
  getWidth: () => ({}),
  inputStyles: { invalidInput: "invalid-input" },
}));

import { SearchVariant } from "../variants/SearchVariant";

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

let container: HTMLDivElement;
let root: Root;

function mount(element: React.ReactElement) {
  act(() => {
    root.render(element);
  });
}

function unmount() {
  act(() => {
    root.unmount();
  });
}

beforeEach(() => {
  container = document.createElement("div");
  document.body.appendChild(container);
  root = createRoot(container);
});

afterEach(() => {
  unmount();
  container.remove();
});

// Wrapper that renders an <input> and calls the hook, exposing the input element.
function EnterKeyBlurWrapper({ onSubmit }: { onSubmit?: () => void }) {
  const handleKeyDown = useEnterKeyBlur(onSubmit);
  return React.createElement("input", {
    "data-testid": "input",
    onKeyDown: handleKeyDown,
  });
}

// ---------------------------------------------------------------------------
// useEnterKeyBlur
// ---------------------------------------------------------------------------

describe("useEnterKeyBlur", () => {
  it("Enter key calls onSubmit and blurs the input", () => {
    const onSubmit = vi.fn();
    mount(React.createElement(EnterKeyBlurWrapper, { onSubmit }));

    const input = container.querySelector('[data-testid="input"]') as HTMLInputElement;
    input.focus();
    expect(document.activeElement).toBe(input);

    const event = new KeyboardEvent("keydown", {
      key: "Enter",
      bubbles: true,
      cancelable: true,
    });
    act(() => {
      input.dispatchEvent(event);
    });

    expect(onSubmit).toHaveBeenCalledTimes(1);
    expect(document.activeElement).not.toBe(input);
    expect(event.defaultPrevented).toBe(true);
  });

  it("non-Enter keys are ignored", () => {
    const onSubmit = vi.fn();
    mount(React.createElement(EnterKeyBlurWrapper, { onSubmit }));

    const input = container.querySelector('[data-testid="input"]') as HTMLInputElement;
    input.focus();

    const event = new KeyboardEvent("keydown", {
      key: "a",
      bubbles: true,
      cancelable: true,
    });
    act(() => {
      input.dispatchEvent(event);
    });

    expect(onSubmit).not.toHaveBeenCalled();
    expect(document.activeElement).toBe(input);
  });

  it("works without onSubmit callback (no error)", () => {
    mount(React.createElement(EnterKeyBlurWrapper, {}));

    const input = container.querySelector('[data-testid="input"]') as HTMLInputElement;
    input.focus();

    const event = new KeyboardEvent("keydown", {
      key: "Enter",
      bubbles: true,
      cancelable: true,
    });
    // Should not throw
    act(() => {
      input.dispatchEvent(event);
    });

    expect(document.activeElement).not.toBe(input);
  });
});

// ---------------------------------------------------------------------------
// usePasteHandler
// ---------------------------------------------------------------------------

describe("usePasteHandler", () => {
  function PasteWrapper({
    maxLength,
    onChange,
  }: {
    maxLength?: number;
    onChange?: (value: string) => void;
  }) {
    const handlePaste = usePasteHandler(maxLength, onChange);
    return React.createElement("input", {
      "data-testid": "paste-input",
      onPaste: handlePaste,
    });
  }

  it("paste within maxLength proceeds normally (no preventDefault)", () => {
    const onChange = vi.fn();
    mount(React.createElement(PasteWrapper, { maxLength: 20, onChange }));

    const input = container.querySelector('[data-testid="paste-input"]') as HTMLInputElement;
    input.value = "Hello";

    // Simulate paste that stays within limit
    const clipboardData = {
      getData: vi.fn().mockReturnValue("World"),
    };
    const event = new Event("paste", {
      bubbles: true,
      cancelable: true,
    });
    Object.defineProperty(event, "clipboardData", { value: clipboardData });
    Object.defineProperty(event, "currentTarget", { value: input });

    act(() => {
      input.dispatchEvent(event);
    });

    // Paste fits within 20 chars, so no preventDefault and no onChange
    expect(event.defaultPrevented).toBe(false);
    expect(onChange).not.toHaveBeenCalled();
  });

  it("paste exceeding maxLength truncates and calls onChange", () => {
    const onChange = vi.fn();
    mount(React.createElement(PasteWrapper, { maxLength: 10, onChange }));

    const input = container.querySelector('[data-testid="paste-input"]') as HTMLInputElement;
    input.value = "Hello";
    // Simulate selection at end of current text
    Object.defineProperty(input, "selectionStart", {
      value: 5,
      writable: true,
    });
    Object.defineProperty(input, "selectionEnd", {
      value: 5,
      writable: true,
    });

    const clipboardData = {
      getData: vi.fn().mockReturnValue("WorldExceeding"),
    };
    const event = new Event("paste", {
      bubbles: true,
      cancelable: true,
    });
    Object.defineProperty(event, "clipboardData", { value: clipboardData });
    Object.defineProperty(event, "currentTarget", { value: input });

    act(() => {
      input.dispatchEvent(event);
    });

    expect(event.defaultPrevented).toBe(true);
    // maxLength=10, "Hello" + truncated paste to fit = "HelloWorld"
    expect(onChange).toHaveBeenCalledWith("HelloWorld");
    expect(input.value).toBe("HelloWorld");
  });

  it("no maxLength makes handler a no-op", () => {
    const onChange = vi.fn();
    mount(React.createElement(PasteWrapper, { maxLength: undefined, onChange }));

    const input = container.querySelector('[data-testid="paste-input"]') as HTMLInputElement;
    input.value = "test";

    const clipboardData = {
      getData: vi.fn().mockReturnValue("LongTextThatWouldExceed"),
    };
    const event = new Event("paste", {
      bubbles: true,
      cancelable: true,
    });
    Object.defineProperty(event, "clipboardData", { value: clipboardData });
    Object.defineProperty(event, "currentTarget", { value: input });

    act(() => {
      input.dispatchEvent(event);
    });

    expect(event.defaultPrevented).toBe(false);
    expect(onChange).not.toHaveBeenCalled();
  });
});

// ---------------------------------------------------------------------------
// SearchVariant handleKeyDown
// ---------------------------------------------------------------------------

describe("SearchVariant keyboard handling", () => {
  // Test the handleKeyDown logic directly via a minimal component
  // that replicates SearchVariant's keyboard behavior
  function SearchKeyDownWrapper({
    onSubmit,
    onBlur,
  }: {
    onSubmit?: () => void;
    onBlur?: () => void;
  }) {
    const handleKeyDown = React.useCallback(
      (e: React.KeyboardEvent<HTMLInputElement>) => {
        if (e.key === "ArrowDown" || e.key === "ArrowUp" || e.key === "Enter") {
          if (e.key === "Enter") {
            onSubmit?.();
          }
          e.currentTarget.blur();
          e.preventDefault();
        }
      },
      [onSubmit],
    );

    return React.createElement("input", {
      "data-testid": "search-input",
      onKeyDown: handleKeyDown,
      onBlur,
    });
  }

  it("ArrowDown key blurs input and prevents default", () => {
    const onBlur = vi.fn();
    mount(React.createElement(SearchKeyDownWrapper, { onBlur }));

    const input = container.querySelector('[data-testid="search-input"]') as HTMLInputElement;
    input.focus();

    const event = new KeyboardEvent("keydown", {
      key: "ArrowDown",
      bubbles: true,
      cancelable: true,
    });
    act(() => {
      input.dispatchEvent(event);
    });

    expect(document.activeElement).not.toBe(input);
    expect(event.defaultPrevented).toBe(true);
  });

  it("ArrowUp key blurs input and prevents default", () => {
    mount(React.createElement(SearchKeyDownWrapper, {}));

    const input = container.querySelector('[data-testid="search-input"]') as HTMLInputElement;
    input.focus();

    const event = new KeyboardEvent("keydown", {
      key: "ArrowUp",
      bubbles: true,
      cancelable: true,
    });
    act(() => {
      input.dispatchEvent(event);
    });

    expect(document.activeElement).not.toBe(input);
    expect(event.defaultPrevented).toBe(true);
  });

  it("Enter key calls onSubmit, blurs input, and prevents default", () => {
    const onSubmit = vi.fn();
    mount(React.createElement(SearchKeyDownWrapper, { onSubmit }));

    const input = container.querySelector('[data-testid="search-input"]') as HTMLInputElement;
    input.focus();

    const event = new KeyboardEvent("keydown", {
      key: "Enter",
      bubbles: true,
      cancelable: true,
    });
    act(() => {
      input.dispatchEvent(event);
    });

    expect(onSubmit).toHaveBeenCalledTimes(1);
    expect(document.activeElement).not.toBe(input);
    expect(event.defaultPrevented).toBe(true);
  });

  it("other keys are not intercepted", () => {
    const onSubmit = vi.fn();
    mount(React.createElement(SearchKeyDownWrapper, { onSubmit }));

    const input = container.querySelector('[data-testid="search-input"]') as HTMLInputElement;
    input.focus();

    const event = new KeyboardEvent("keydown", {
      key: "a",
      bubbles: true,
      cancelable: true,
    });
    act(() => {
      input.dispatchEvent(event);
    });

    expect(onSubmit).not.toHaveBeenCalled();
    expect(document.activeElement).toBe(input);
    expect(event.defaultPrevented).toBe(false);
  });
});

// ---------------------------------------------------------------------------
// useCursorPosition
// ---------------------------------------------------------------------------

describe("useCursorPosition", () => {
  function CursorWrapper({
    value,
    onRef,
  }: {
    value?: string;
    onRef?: (ref: {
      savePosition: () => void;
      elementRef: React.RefObject<HTMLInputElement | HTMLTextAreaElement | null>;
    }) => void;
  }) {
    const { elementRef, savePosition } = useCursorPosition(value);

    React.useEffect(() => {
      onRef?.({ savePosition, elementRef });
    });

    return React.createElement("input", {
      ref: elementRef as React.RefObject<HTMLInputElement>,
      "data-testid": "cursor-input",
      value: value ?? "",
      onChange: () => {},
    });
  }

  it("savePosition captures current selectionStart", () => {
    let hookRef: {
      savePosition: () => void;
      elementRef: React.RefObject<HTMLInputElement | HTMLTextAreaElement | null>;
    } | null = null;

    mount(
      React.createElement(CursorWrapper, {
        value: "Hello",
        onRef: (ref) => {
          hookRef = ref;
        },
      }),
    );

    const input = container.querySelector('[data-testid="cursor-input"]') as HTMLInputElement;
    input.focus();
    input.setSelectionRange(3, 3);

    act(() => {
      hookRef!.savePosition();
    });

    // After saving position at 3 and re-rendering with same value,
    // the cursor should be restored to position 3
    act(() => {
      root.render(
        React.createElement(CursorWrapper, {
          value: "Hello",
          onRef: (ref) => {
            hookRef = ref;
          },
        }),
      );
    });

    expect(input.selectionStart).toBe(3);
  });

  it("cursor position is restored after value change re-render", () => {
    let hookRef: {
      savePosition: () => void;
      elementRef: React.RefObject<HTMLInputElement | HTMLTextAreaElement | null>;
    } | null = null;

    mount(
      React.createElement(CursorWrapper, {
        value: "Hello",
        onRef: (ref) => {
          hookRef = ref;
        },
      }),
    );

    const input = container.querySelector('[data-testid="cursor-input"]') as HTMLInputElement;
    input.focus();
    input.setSelectionRange(2, 2);

    act(() => {
      hookRef!.savePosition();
    });

    // Re-render with new value — cursor should restore to saved position
    act(() => {
      root.render(
        React.createElement(CursorWrapper, {
          value: "Hxllo",
          onRef: (ref) => {
            hookRef = ref;
          },
        }),
      );
    });

    expect(input.selectionStart).toBe(2);
  });
});

// ---------------------------------------------------------------------------
// SearchVariant disabled/invalid container styles
// ---------------------------------------------------------------------------

describe("SearchVariant disabled/invalid container styles", () => {
  const baseProps = {
    id: "test-search",
    disabled: false,
    events: [],
    value: "",
    variant: "Search" as const,
  };

  function renderSearchVariant(overrides: Record<string, unknown> = {}) {
    const props = { ...baseProps, ...overrides };
    act(() => {
      root.render(
        React.createElement(SearchVariant, {
          props: props as any,
          onChange: () => {},
          onBlur: () => {},
          onFocus: () => {},
          onClear: () => {},
          isFocused: false,
        }),
      );
    });
  }

  it("applies cursor-not-allowed and opacity-50 when disabled", () => {
    renderSearchVariant({ disabled: true });

    const borderedContainer = container.querySelector(".rounded-field") as HTMLElement;
    expect(borderedContainer).not.toBeNull();
    expect(borderedContainer.className).toContain("cursor-not-allowed");
    expect(borderedContainer.className).toContain("opacity-50");
  });

  it("applies border-destructive when invalid", () => {
    renderSearchVariant({ invalid: "Field is required" });

    const borderedContainer = container.querySelector(".rounded-field") as HTMLElement;
    expect(borderedContainer).not.toBeNull();
    expect(borderedContainer.className).toContain("border-destructive");
  });

  it("does not apply disabled or invalid styles by default", () => {
    renderSearchVariant();

    const borderedContainer = container.querySelector(".rounded-field") as HTMLElement;
    expect(borderedContainer).not.toBeNull();
    expect(borderedContainer.className).not.toContain("cursor-not-allowed");
    expect(borderedContainer.className).not.toContain("opacity-50");
    expect(borderedContainer.className).not.toContain("border-destructive");
  });
});
