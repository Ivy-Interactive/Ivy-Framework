import { describe, expect, it, beforeEach, afterEach, vi } from "vitest";
import { createRoot, type Root } from "react-dom/client";
import { useIsNarrow, NARROW_BREAKPOINT } from "./DiffView";

/**
 * Drives the real useIsNarrow hook through a mocked ResizeObserver so we can
 * assert that the narrow state tracks the container width (not the viewport).
 *
 * The test bundle uses production React (NODE_ENV is forced to "production" in
 * vite.config), where `act` is unavailable — so we flush React's commit and
 * passive effects with a real macrotask tick instead.
 */
type ResizeCb = (entries: Array<{ contentRect: { width: number } }>) => void;

let observers: Array<{ cb: ResizeCb }>;
let container: HTMLDivElement;
let root: Root;
let measuredWidth: number;

// Two macrotasks: one for React to commit + run the mount effect, a second for
// the re-render triggered by the effect's setIsNarrow to flush.
const tick = async () => {
  await new Promise((resolve) => setTimeout(resolve, 0));
  await new Promise((resolve) => setTimeout(resolve, 0));
};

function HookProbe({ onValue }: { onValue: (v: boolean) => void }) {
  const [ref, isNarrow] = useIsNarrow();
  onValue(isNarrow);
  return <div ref={ref} />;
}

function emitResize(width: number) {
  for (const o of observers) {
    o.cb([{ contentRect: { width } }]);
  }
}

describe("useIsNarrow", () => {
  beforeEach(() => {
    observers = [];
    measuredWidth = 0;

    vi.stubGlobal(
      "ResizeObserver",
      class {
        cb: ResizeCb;
        constructor(cb: ResizeCb) {
          this.cb = cb;
        }
        observe() {
          observers.push({ cb: this.cb });
        }
        disconnect() {
          observers = observers.filter((o) => o.cb !== this.cb);
        }
        unobserve() {}
      },
    );

    // The hook reads getBoundingClientRect().width for its initial measurement.
    vi.spyOn(HTMLElement.prototype, "getBoundingClientRect").mockImplementation(
      () => ({ width: measuredWidth }) as DOMRect,
    );

    container = document.createElement("div");
    document.body.appendChild(container);
    root = createRoot(container);
  });

  afterEach(async () => {
    root.unmount();
    await tick();
    container.remove();
    vi.restoreAllMocks();
    vi.unstubAllGlobals();
  });

  it("starts narrow when the container is initially below the breakpoint", async () => {
    measuredWidth = NARROW_BREAKPOINT - 1;
    let value = false;
    root.render(<HookProbe onValue={(v) => (value = v)} />);
    await tick();
    expect(value).toBe(true);
  });

  it("starts wide when the container is initially at or above the breakpoint", async () => {
    measuredWidth = NARROW_BREAKPOINT;
    let value = true;
    root.render(<HookProbe onValue={(v) => (value = v)} />);
    await tick();
    expect(value).toBe(false);
  });

  it("becomes narrow when the container shrinks below the breakpoint", async () => {
    measuredWidth = NARROW_BREAKPOINT + 200;
    let value = true;
    root.render(<HookProbe onValue={(v) => (value = v)} />);
    await tick();
    expect(value).toBe(false);

    emitResize(NARROW_BREAKPOINT - 100);
    await tick();
    expect(value).toBe(true);
  });

  it("returns to wide when the container grows back above the breakpoint", async () => {
    measuredWidth = NARROW_BREAKPOINT - 100;
    let value = false;
    root.render(<HookProbe onValue={(v) => (value = v)} />);
    await tick();
    expect(value).toBe(true);

    emitResize(NARROW_BREAKPOINT + 100);
    await tick();
    expect(value).toBe(false);
  });

  it("ignores a zero width (unmeasured / detached container)", async () => {
    measuredWidth = 0;
    let value = true;
    root.render(<HookProbe onValue={(v) => (value = v)} />);
    await tick();
    expect(value).toBe(false);
  });

  it("stops observing after unmount", async () => {
    measuredWidth = NARROW_BREAKPOINT + 100;
    root.render(<HookProbe onValue={() => {}} />);
    await tick();
    expect(observers.length).toBe(1);
    root.unmount();
    await tick();
    expect(observers.length).toBe(0);
  });
});
