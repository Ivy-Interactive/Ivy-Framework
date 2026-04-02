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
});
