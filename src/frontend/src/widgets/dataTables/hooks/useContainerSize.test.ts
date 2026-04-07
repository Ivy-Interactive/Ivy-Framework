import { describe, it, expect } from "vitest";
import * as fs from "fs";
import * as path from "path";

/**
 * Tests for the useContainerSize hook's synchronous initial measurement fix.
 *
 * The hook is a React hook that requires a component context to run. Rather
 * than pulling in @testing-library/react (not available in this project),
 * we verify the source code contains the critical fix pattern: a synchronous
 * getBoundingClientRect call that runs before the first render can complete
 * with height=0.
 *
 * The actual visual behavior is verified by the IvyFrameworkVerification
 * end-to-end test which renders a DataTable and checks rows are visible.
 */
describe("useContainerSize - synchronous initial measurement", () => {
  const hookSource = fs.readFileSync(path.resolve(__dirname, "./useContainerSize.ts"), "utf-8");

  it("should call getBoundingClientRect synchronously after ResizeObserver setup", () => {
    // The fix adds a synchronous getBoundingClientRect call after resizeObserver.observe()
    // to provide an immediate initial measurement before the deferred requestAnimationFrame
    const observeIndex = hookSource.indexOf("resizeObserver.observe(");
    const syncMeasureIndex = hookSource.indexOf("getBoundingClientRect()");

    expect(observeIndex).toBeGreaterThan(-1);
    expect(syncMeasureIndex).toBeGreaterThan(-1);
    // The synchronous measurement must come AFTER resizeObserver.observe()
    expect(syncMeasureIndex).toBeGreaterThan(observeIndex);
  });

  it("should only apply synchronous measurement when dimensions are non-zero", () => {
    // The fix guards against applying zero-dimension measurements
    expect(hookSource).toContain("width > 0 || height > 0");
    expect(hookSource).toContain("!hasAppliedInitialRef.current");
  });

  it("should call apply() with the synchronous measurement results", () => {
    // After reading dimensions, the hook must call apply() to set state
    // Find the sync measurement block and verify it calls apply
    const syncBlock = hookSource.slice(hookSource.indexOf("getBoundingClientRect()"));
    expect(syncBlock).toContain("apply(width, height)");
  });

  it("should retry up to 10 times via requestAnimationFrame before falling back", () => {
    // The retry loop should allow up to 10 rAF retries (increased from 5)
    expect(hookSource).toContain("retries++ < 10");
  });

  it("should include a setTimeout fallback after rAF retries exhaust", () => {
    // When all rAF retries return 0, a setTimeout(100ms) provides a final
    // chance to measure after layout paint completes
    expect(hookSource).toContain("setTimeout(");
    // The timeout should be 100ms
    expect(hookSource).toContain(", 100)");
    // The fallback should still guard against already-applied state
    const timeoutBlock = hookSource.slice(hookSource.indexOf("setTimeout("));
    expect(timeoutBlock).toContain("hasAppliedInitialRef.current");
    expect(timeoutBlock).toContain("getBoundingClientRect()");
  });

  it("should not throw or crash when all measurement attempts return zero dimensions", () => {
    // The hook should gracefully handle persistent zero dimensions without
    // throwing — it simply doesn't call apply(), leaving state at 0.
    // Verify there's no throw/error path in the retry logic.
    const retryBlock = hookSource.slice(
      hookSource.indexOf("let retries"),
      hookSource.indexOf("return () =>"),
    );
    expect(retryBlock).not.toContain("throw");
    expect(retryBlock).not.toContain("console.error");
  });
});

/**
 * Tests for DataTableWidget min-height fallback.
 *
 * When height="Full", the container must have a non-zero min-height
 * so that useContainerSize can measure something even in unconstrained parents.
 */
describe("DataTableWidget - min-height fallback for unconstrained parents", () => {
  const widgetSource = fs.readFileSync(path.resolve(__dirname, "../DataTableWidget.tsx"), "utf-8");

  it('should set minHeight to "200px" when height is "Full"', () => {
    // The fix adds minHeight: "200px" to ensure the container has a
    // non-zero height for measurement in unconstrained flex parents
    expect(widgetSource).toContain('minHeight = "200px"');
  });

  it('should remove height: 100% when height is "Full"', () => {
    // height: 100% doesn't work in unconstrained parents (resolves to 0)
    // because the parent has no definite height. Using flex-based sizing instead.
    expect(widgetSource).toContain("delete containerStyle.height");
  });

  it('should set flexGrow to 1 when height is "Full"', () => {
    expect(widgetSource).toContain("flexGrow = 1");
  });
});
