(() => {
  const DEPTH_COLORS = [
    [239, 68, 68],   // red
    [59, 130, 246],   // blue
    [34, 197, 94],    // green
    [168, 85, 247],   // purple
    [249, 115, 22],   // orange
    [6, 182, 212],    // cyan
    [236, 72, 153],   // pink
    [234, 179, 8],    // yellow
  ];

  const PADDING_ALPHA = 0.12;
  const BORDER_ALPHA = 0.7;
  const ZERO_SIZE_COLOR = 'rgba(239, 68, 68, 0.9)';
  const LABEL_BG = 'rgba(0, 0, 0, 0.75)';
  const LABEL_COLOR = '#fff';
  const FONT = '10px monospace';

  const container = document.createElement('div');
  container.id = 'ivyml-debug-overlay';
  Object.assign(container.style, {
    position: 'fixed',
    top: '0',
    left: '0',
    width: '100vw',
    height: '100vh',
    pointerEvents: 'none',
    zIndex: '999999',
  });
  document.body.appendChild(container);

  function getDepth(el) {
    let depth = 0;
    let parent = el.parentElement;
    while (parent) {
      if (parent.tagName && parent.tagName.toLowerCase() === 'ivy-widget') depth++;
      parent = parent.parentElement;
    }
    return depth;
  }

  function formatType(type) {
    return (type || '').replace(/^Ivy\./, '');
  }

  const widgets = document.querySelectorAll('ivy-widget');

  widgets.forEach(el => {
    const depth = getDepth(el);
    const color = DEPTH_COLORS[depth % DEPTH_COLORS.length];
    const type = formatType(el.getAttribute('type'));
    const testId = el.querySelector('[data-testid]')?.getAttribute('data-testid');
    const label = testId || type;

    const children = el.children;
    let minX = Infinity, minY = Infinity, maxX = -Infinity, maxY = -Infinity;
    for (let i = 0; i < children.length; i++) {
      const tag = children[i].tagName?.toLowerCase();
      if (tag === 'div' && children[i].id === 'ivyml-debug-overlay') continue;
      const rect = children[i].getBoundingClientRect();
      if (rect.width === 0 && rect.height === 0) continue;
      minX = Math.min(minX, rect.left);
      minY = Math.min(minY, rect.top);
      maxX = Math.max(maxX, rect.right);
      maxY = Math.max(maxY, rect.bottom);
    }

    const isZero = minX === Infinity;
    const bounds = isZero
      ? el.getBoundingClientRect()
      : new DOMRect(minX, minY, maxX - minX, maxY - minY);

    const w = Math.round(bounds.width);
    const h = Math.round(bounds.height);

    if (isZero || (w === 0 && h === 0)) {
      const marker = document.createElement('div');
      Object.assign(marker.style, {
        position: 'fixed',
        top: `${bounds.top - 6}px`,
        left: `${bounds.left - 6}px`,
        width: '12px',
        height: '12px',
        zIndex: '999999',
      });

      const cross = document.createElement('div');
      Object.assign(cross.style, {
        position: 'absolute',
        top: '0',
        left: '0',
        width: '12px',
        height: '12px',
        fontSize: '12px',
        lineHeight: '12px',
        textAlign: 'center',
        color: ZERO_SIZE_COLOR,
        fontWeight: 'bold',
        fontFamily: 'monospace',
      });
      cross.textContent = '✖';
      marker.appendChild(cross);

      const zeroLabel = document.createElement('div');
      Object.assign(zeroLabel.style, {
        position: 'absolute',
        top: '-2px',
        left: '14px',
        background: 'rgba(239, 68, 68, 0.9)',
        color: LABEL_COLOR,
        font: FONT,
        padding: '1px 4px',
        borderRadius: '2px',
        whiteSpace: 'nowrap',
      });
      zeroLabel.textContent = `${label} 0×0`;
      marker.appendChild(zeroLabel);

      container.appendChild(marker);
      return;
    }

    // Padding visualization
    const cs = window.getComputedStyle(el.children[0] || el);
    const pt = parseFloat(cs.paddingTop) || 0;
    const pr = parseFloat(cs.paddingRight) || 0;
    const pb = parseFloat(cs.paddingBottom) || 0;
    const pl = parseFloat(cs.paddingLeft) || 0;
    const hasPadding = pt > 0 || pr > 0 || pb > 0 || pl > 0;

    if (hasPadding) {
      const padColor = `rgba(${color[0]}, ${color[1]}, ${color[2]}, ${PADDING_ALPHA})`;

      // Top padding
      if (pt > 0) {
        const pad = document.createElement('div');
        Object.assign(pad.style, {
          position: 'fixed',
          top: `${bounds.top}px`,
          left: `${bounds.left}px`,
          width: `${w}px`,
          height: `${Math.min(pt, h)}px`,
          background: padColor,
        });
        container.appendChild(pad);
      }
      // Bottom padding
      if (pb > 0) {
        const pad = document.createElement('div');
        Object.assign(pad.style, {
          position: 'fixed',
          top: `${bounds.bottom - Math.min(pb, h)}px`,
          left: `${bounds.left}px`,
          width: `${w}px`,
          height: `${Math.min(pb, h)}px`,
          background: padColor,
        });
        container.appendChild(pad);
      }
      // Left padding
      if (pl > 0) {
        const innerTop = bounds.top + Math.min(pt, h);
        const innerH = h - Math.min(pt, h) - Math.min(pb, h);
        if (innerH > 0) {
          const pad = document.createElement('div');
          Object.assign(pad.style, {
            position: 'fixed',
            top: `${innerTop}px`,
            left: `${bounds.left}px`,
            width: `${Math.min(pl, w)}px`,
            height: `${innerH}px`,
            background: padColor,
          });
          container.appendChild(pad);
        }
      }
      // Right padding
      if (pr > 0) {
        const innerTop = bounds.top + Math.min(pt, h);
        const innerH = h - Math.min(pt, h) - Math.min(pb, h);
        if (innerH > 0) {
          const pad = document.createElement('div');
          Object.assign(pad.style, {
            position: 'fixed',
            top: `${innerTop}px`,
            left: `${bounds.right - Math.min(pr, w)}px`,
            width: `${Math.min(pr, w)}px`,
            height: `${innerH}px`,
            background: padColor,
          });
          container.appendChild(pad);
        }
      }
    }

    // Bounding box border
    const box = document.createElement('div');
    Object.assign(box.style, {
      position: 'fixed',
      top: `${bounds.top}px`,
      left: `${bounds.left}px`,
      width: `${w}px`,
      height: `${h}px`,
      border: `1px solid rgba(${color[0]}, ${color[1]}, ${color[2]}, ${BORDER_ALPHA})`,
      boxSizing: 'border-box',
    });
    container.appendChild(box);

    // Size + type label (skip for tiny widgets)
    if (w >= 40 && h >= 16) {
      const labelTop = bounds.top >= 14 ? bounds.top - 14 : bounds.top;
      const sizeLabel = document.createElement('div');
      Object.assign(sizeLabel.style, {
        position: 'fixed',
        top: `${labelTop}px`,
        left: `${bounds.left}px`,
        background: `rgba(${color[0]}, ${color[1]}, ${color[2]}, 0.85)`,
        color: LABEL_COLOR,
        font: FONT,
        padding: '1px 4px',
        borderRadius: '2px',
        whiteSpace: 'nowrap',
        lineHeight: '12px',
        maxWidth: `${w}px`,
        overflow: 'hidden',
        textOverflow: 'ellipsis',
      });
      sizeLabel.textContent = `${label} ${w}×${h}`;
      container.appendChild(sizeLabel);
    }
  });
})();
