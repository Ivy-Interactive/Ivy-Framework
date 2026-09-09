import { describe, expect, it, beforeEach, afterEach, vi } from "vitest";
import type { ReactElement } from "react";
import { createRoot, type Root } from "react-dom/client";
import { DiffView, NARROW_BREAKPOINT } from "./DiffView";

/**
 * The test bundle uses production React (NODE_ENV forced to "production" in
 * vite.config), where `act` is unavailable — so renders/updates are flushed
 * with a real macrotask tick instead.
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

const SINGLE_FILE_DIFF = `--- a/file.txt
+++ b/file.txt
@@ -1,3 +1,3 @@
 line 1
-line 2
+line 2 modified
 line 3`;

const MULTI_FILE_DIFF = `diff --git a/file.txt b/file.txt
${SINGLE_FILE_DIFF}
diff --git a/other.txt b/other.txt
--- a/other.txt
+++ b/other.txt
@@ -1,2 +1,2 @@
-old
+new
 keep`;

function emitResize(width: number) {
  measuredWidth = width;
  for (const o of observers) {
    o.cb([{ contentRect: { width } }]);
  }
}

async function render(node: ReactElement) {
  root.render(node);
  await tick();
}

describe("DiffView responsive behavior", () => {
  beforeEach(() => {
    observers = [];
    measuredWidth = NARROW_BREAKPOINT + 400;

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

  const noop = () => {};

  // react-diff-view marks the table with a diff-split / diff-unified class.
  function splitTableCount() {
    return container.querySelectorAll(".diff.diff-split").length;
  }

  function unifiedTableCount() {
    return container.querySelectorAll(".diff.diff-unified").length;
  }

  it("renders side-by-side split on a wide container", async () => {
    await render(
      <DiffView id="d1" onIvyEvent={noop} diff={SINGLE_FILE_DIFF} viewType="Split" />,
    );
    expect(splitTableCount()).toBeGreaterThan(0);
    expect(unifiedTableCount()).toBe(0);
  });

  it("forces unified (inline) view when the container is narrow", async () => {
    await render(
      <DiffView id="d2" onIvyEvent={noop} diff={SINGLE_FILE_DIFF} viewType="Split" />,
    );
    expect(splitTableCount()).toBeGreaterThan(0);

    emitResize(NARROW_BREAKPOINT - 50);
    await tick();
    expect(splitTableCount()).toBe(0);
    expect(unifiedTableCount()).toBeGreaterThan(0);
  });

  it("restores split view when the container grows wide again", async () => {
    await render(
      <DiffView id="d3" onIvyEvent={noop} diff={SINGLE_FILE_DIFF} viewType="Split" />,
    );
    emitResize(NARROW_BREAKPOINT - 50);
    await tick();
    expect(splitTableCount()).toBe(0);

    emitResize(NARROW_BREAKPOINT + 400);
    await tick();
    expect(splitTableCount()).toBeGreaterThan(0);
  });

  it("does not show the file dropdown on a wide container", async () => {
    await render(<DiffView id="d4" onIvyEvent={noop} diff={MULTI_FILE_DIFF} viewType="Split" />);
    expect(container.querySelector('select[aria-label="Jump to file"]')).toBeNull();
  });

  it("shows the jump-to-file dropdown on a narrow container with multiple files", async () => {
    await render(<DiffView id="d5" onIvyEvent={noop} diff={MULTI_FILE_DIFF} viewType="Split" />);
    emitResize(NARROW_BREAKPOINT - 50);
    await tick();

    const select = container.querySelector('select[aria-label="Jump to file"]');
    expect(select).not.toBeNull();
    // One <option> per file plus the disabled placeholder.
    const options = select!.querySelectorAll("option");
    expect(options.length).toBe(3);
  });

  it("keeps all files rendered (jump navigation, not single-file view) when narrow", async () => {
    await render(<DiffView id="d6" onIvyEvent={noop} diff={MULTI_FILE_DIFF} viewType="Split" />);
    emitResize(NARROW_BREAKPOINT - 50);
    await tick();
    // Both files still render their diff tables; the dropdown only navigates.
    expect(unifiedTableCount()).toBe(2);
  });

  it("does not show the dropdown for a single narrow file", async () => {
    await render(<DiffView id="d7" onIvyEvent={noop} diff={SINGLE_FILE_DIFF} viewType="Split" />);
    emitResize(NARROW_BREAKPOINT - 50);
    await tick();
    expect(container.querySelector('select[aria-label="Jump to file"]')).toBeNull();
  });

  it("keys collapsed state by file and resets when diff changes", async () => {
    await render(<DiffView id="d8" onIvyEvent={noop} diff={MULTI_FILE_DIFF} collapsible={true} />);
    const header = container.querySelector(".cursor-pointer") as HTMLElement;
    expect(header).not.toBeNull();
    expect(container.querySelector(".diff-line")).not.toBeNull();

    // Toggle collapse
    header.click();
    await tick();
    // One file collapsed, second file still has lines
    expect(container.querySelectorAll(".diff-line").length).toBeGreaterThan(0);

    // Re-render with new diff
    await render(<DiffView id="d8" onIvyEvent={noop} diff={`diff --git a/newfile.txt b/newfile.txt\n--- a/newfile.txt\n+++ b/newfile.txt\n@@ -1 +1 @@\n-a\n+b`} collapsible={true} />);
    await tick();
    // After diff change, files should not stay collapsed
    expect(container.querySelectorAll(".diff-line").length).toBeGreaterThan(0);
  });
});
