import { describe, it, expect, beforeEach, afterEach, vi } from "vitest";
import React, { act, useRef } from "react";
import { createRoot, Root } from "react-dom/client";
import { useBreakpoint, type BreakpointName } from "../use-responsive";

let container: HTMLDivElement;
let root: Root;

function mount(element: React.ReactElement) {
  act(() => {
    root.render(element);
  });
}

beforeEach(() => {
  container = document.createElement("div");
  document.body.appendChild(container);
  root = createRoot(container);
});

afterEach(() => {
  act(() => {
    root.unmount();
  });
  container.remove();
  vi.unstubAllGlobals();
  vi.restoreAllMocks();
});

function setViewportWidth(width: number) {
  Object.defineProperty(window, "innerWidth", { value: width, configurable: true, writable: true });
}

/**
 * A controllable ResizeObserver stand-in. happy-dom does no layout, so the real
 * observer never fires; this lets a test trigger the callback on demand.
 */
function stubResizeObserver() {
  const callbacks: ResizeObserverCallback[] = [];
  class FakeResizeObserver {
    constructor(cb: ResizeObserverCallback) {
      callbacks.push(cb);
    }
    observe() {}
    unobserve() {}
    disconnect() {}
  }
  vi.stubGlobal("ResizeObserver", FakeResizeObserver);
  return {
    fireAll: () => {
      act(() => {
        callbacks.forEach((cb) => cb([], {} as ResizeObserver));
      });
    },
  };
}

/** Pins clientWidth on the node the instant it attaches, before effects run. */
function pinWidth(width: number) {
  return (el: HTMLDivElement | null) => {
    if (el) Object.defineProperty(el, "clientWidth", { value: width, configurable: true });
  };
}

function Display({ width }: { width?: number }) {
  const ref = useRef<HTMLDivElement>(null);
  const bp = useBreakpoint(width !== undefined ? ref : undefined);
  return (
    <div
      ref={(el) => {
        ref.current = el;
        if (width !== undefined) pinWidth(width)(el);
      }}
      data-testid="bp"
    >
      {bp}
    </div>
  );
}

function readBp(): BreakpointName {
  return container.querySelector('[data-testid="bp"]')!.textContent as BreakpointName;
}

describe("useBreakpoint", () => {
  it("falls back to the viewport width when no container ref is supplied", () => {
    setViewportWidth(500);
    mount(<Display />);
    expect(readBp()).toBe("mobile");
  });

  it("derives the breakpoint from the container width, not the viewport", () => {
    // Wide viewport, but a narrow content container (e.g. sidebar open) — the
    // breakpoint must follow the container so buttons collapse correctly.
    setViewportWidth(1920);
    stubResizeObserver();
    mount(<Display width={700} />);
    expect(readBp()).toBe("tablet");
  });

  it("re-evaluates when the observed container changes width", async () => {
    setViewportWidth(1920);
    const { fireAll } = stubResizeObserver();

    let measured = 1100;
    function Resizable() {
      // A getter lets the test mutate the reported width between observer fires.
      const setRef = (el: HTMLDivElement | null) => {
        if (el)
          Object.defineProperty(el, "clientWidth", { get: () => measured, configurable: true });
      };
      const ref = useRef<HTMLDivElement>(null);
      const bp = useBreakpoint(ref);
      return (
        <div
          ref={(el) => {
            ref.current = el;
            setRef(el);
          }}
          data-testid="bp"
        >
          {bp}
        </div>
      );
    }

    mount(<Resizable />);
    expect(readBp()).toBe("wide");

    // Sidebar opens — content shrinks below the desktop band. The observer
    // callback is debounced by 100ms (RESIZE_DEBOUNCE_MS) before re-measuring.
    measured = 800;
    fireAll();
    await act(async () => {
      await new Promise((r) => setTimeout(r, 150));
    });
    expect(readBp()).toBe("desktop");
  });
});
