import { describe, it, expect } from 'vitest';
import { useDrag } from './useDrag';

describe('useDrag', () => {
  it('should be a function', () => {
    expect(typeof useDrag).toBe('function');
  });

  it('should have correct function signature (parameter count)', () => {
    expect(useDrag.length).toBe(12);
  });

  it('should be named correctly', () => {
    expect(useDrag.name).toBe('useDrag');
  });

  it('should be importable as a named export', () => {
    expect(useDrag).toBeDefined();
    expect(typeof useDrag).toBe('function');
  });
});
