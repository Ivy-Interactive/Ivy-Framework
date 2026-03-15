import { describe, it, expect } from 'vitest';
import { createElement } from 'react';
import { createRoot } from 'react-dom/client';
import { act } from 'react';
import { GridLayoutWidget, GridLayoutWidgetProps } from './GridLayoutWidget';

function renderGrid(props: Partial<GridLayoutWidgetProps>, children: string[]) {
  const container = document.createElement('div');
  document.body.appendChild(container);
  const root = createRoot(container);
  act(() => {
    root.render(
      createElement(
        GridLayoutWidget,
        { columns: 2, rows: 2, padding: '0,0,0,0', ...props } as never,
        ...children
      )
    );
  });
  return container;
}

describe('GridLayoutWidget', () => {
  it('renders children in a grid', () => {
    const container = renderGrid({ columns: 2, rows: 2 }, ['A', 'B', 'C', 'D']);

    const grid = container.firstElementChild as HTMLElement;
    expect(grid.style.display).toBe('grid');
    expect(grid.style.gridTemplateColumns).toBe('repeat(2, minmax(0, 1fr))');
    expect(grid.style.gridTemplateRows).toBe('repeat(2, minmax(0, 1fr))');
  });

  it('applies gridRow span style to cells with childRowSpan', () => {
    const container = renderGrid(
      { columns: 2, rows: 3, childRowSpan: [undefined, 2] },
      ['A', 'B']
    );

    const grid = container.firstElementChild as HTMLElement;
    const cells = Array.from(grid.children) as HTMLElement[];

    expect(cells[0].style.gridRow).toBe('');
    expect(cells[1].style.gridRow).toBe('span 2');
  });

  it('applies alignSelf stretch on cells with rowSpan > 1', () => {
    const container = renderGrid(
      { columns: 2, rows: 3, childRowSpan: [undefined, 2] },
      ['A', 'B']
    );

    const grid = container.firstElementChild as HTMLElement;
    const cells = Array.from(grid.children) as HTMLElement[];

    expect(cells[0].style.alignSelf).toBe('');
    expect(cells[1].style.alignSelf).toBe('stretch');
  });

  it('applies items-stretch class on cells with rowSpan > 1', () => {
    const container = renderGrid(
      { columns: 2, rows: 3, childRowSpan: [undefined, 2] },
      ['A', 'B']
    );

    const grid = container.firstElementChild as HTMLElement;
    const cells = Array.from(grid.children) as HTMLElement[];

    expect(cells[0].className).toContain('items-center');
    expect(cells[0].className).not.toContain('items-stretch');

    expect(cells[1].className).toContain('items-stretch');
    expect(cells[1].className).not.toContain('items-center');
  });

  it('applies gridColumn span and justifySelf stretch for columnSpan > 1', () => {
    const container = renderGrid(
      { columns: 3, rows: 2, childColumnSpan: [2, undefined] },
      ['A', 'B']
    );

    const grid = container.firstElementChild as HTMLElement;
    const cells = Array.from(grid.children) as HTMLElement[];

    expect(cells[0].style.gridColumn).toBe('span 2');
    expect(cells[0].style.justifySelf).toBe('stretch');

    expect(cells[1].style.gridColumn).toBe('');
    expect(cells[1].style.justifySelf).toBe('');
  });

  it('does not apply stretch override when alignSelf is explicitly set', () => {
    const container = renderGrid(
      { columns: 2, rows: 3, childRowSpan: [2], childAlignSelf: ['TopLeft'] },
      ['A']
    );

    const grid = container.firstElementChild as HTMLElement;
    const cell = grid.children[0] as HTMLElement;

    expect(cell.style.alignSelf).not.toBe('stretch');
  });

  it('applies both row and column span together', () => {
    const container = renderGrid(
      { columns: 3, rows: 3, childRowSpan: [2], childColumnSpan: [2] },
      ['A']
    );

    const grid = container.firstElementChild as HTMLElement;
    const cell = grid.children[0] as HTMLElement;

    expect(cell.style.gridRow).toBe('span 2');
    expect(cell.style.gridColumn).toBe('span 2');
    expect(cell.style.alignSelf).toBe('stretch');
    expect(cell.style.justifySelf).toBe('stretch');
  });
});
