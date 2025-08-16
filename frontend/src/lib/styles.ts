export const inputStyles = {
  invalid:
    'bg-destructive border-destructive text-destructive-foreground placeholder-destructive-foreground focus:ring-destructive focus:border-destructive',
  invalidInput:
    'border-destructive text-destructive-foreground placeholder-destructive-foreground focus:ring-destructive focus:border-destructive',
};

export const getWidth = (width?: string): React.CSSProperties => {
  if (!width) return {};

  const [wantedWidth, minWidth, maxWidth] = width.split(',');

  return {
    ..._getWantedWidth(wantedWidth),
    ..._getMinWidth(minWidth),
    ..._getMaxWidth(maxWidth),
  };
};

const _getWantedWidth = (width?: string): React.CSSProperties => {
  if (!width) return {};
  const [sizeType, value] = width.split(':');
  switch (sizeType.toLowerCase()) {
    case 'units':
      return { width: `${parseFloat(value) * 0.25}rem` };
    case 'px':
      return { width: `${value}px` };
    case 'rem':
      return { width: `${value}rem` };
    case 'fraction':
      return {
        width: `${parseFloat(value) * 100}%`,
      };
    case 'fraction-gap':
      return {
        flexBasis: `${parseFloat(value) * 100}%`,
        flexShrink: 1,
        flexGrow: 0,
        minWidth: 0, // Allow shrinking below flex-basis to accommodate gaps
      };
    case 'full':
      return { width: '100%' };
    case 'fit':
      return { width: 'fit-content' };
    case 'screen':
      return { width: '100vw' };
    case 'mincontent':
      return { width: 'min-content' };
    case 'maxcontent':
      return { width: 'max-content' };
    case 'auto':
      return { width: 'auto' };
    case 'grow':
      return { flexGrow: parseFloat(value) || 1 };
    case 'shrink':
      return { flexShrink: parseFloat(value) || 1 };
    default:
      console.warn(`Unknown size type: ${sizeType}`);
      return {};
  }
};

const _getMinWidth = (width?: string): React.CSSProperties => {
  if (!width) return {};
  const [sizeType, value] = width.split(':');
  switch (sizeType.toLowerCase()) {
    case 'units':
      return { minWidth: `${parseFloat(value) * 0.25}rem` };
    case 'px':
      return { minWidth: `${value}px` };
    case 'rem':
      return { minWidth: `${value}rem` };
    case 'fraction':
      return {
        minWidth: `${parseFloat(value) * 100}%`,
      };
    case 'fraction-gap':
      return {
        minWidth: 0, // Allow shrinking to accommodate gaps
      };
    case 'full':
      return { minWidth: '100%' };
    case 'fit':
      return { minWidth: 'fit-content' };
    case 'screen':
      return { minWidth: '100vw' };
    case 'mincontent':
      return { minWidth: 'min-content' };
    case 'maxcontent':
      return { minWidth: 'max-content' };
    case 'auto':
      return { minWidth: 'auto' };
    default:
      console.warn(`Unknown size type: ${sizeType}`);
      return {};
  }
};

const _getMaxWidth = (width?: string): React.CSSProperties => {
  if (!width) return {};
  const [sizeType, value] = width.split(':');
  switch (sizeType.toLowerCase()) {
    case 'units':
      return { maxWidth: `${parseFloat(value) * 0.25}rem` };
    case 'px':
      return { maxWidth: `${value}px` };
    case 'rem':
      return { maxWidth: `${value}rem` };
    case 'fraction':
      return {
        maxWidth: `${parseFloat(value) * 100}%`,
      };
    case 'fraction-gap':
      return {
        maxWidth: `${parseFloat(value) * 100}%`,
      };
    case 'full':
      return { maxWidth: '100%' };
    case 'fit':
      return { maxWidth: 'fit-content' };
    case 'screen':
      return { maxWidth: '100vw' };
    case 'mincontent':
      return { maxWidth: 'min-content' };
    case 'maxcontent':
      return { maxWidth: 'max-content' };
    case 'auto':
      return { maxWidth: 'auto' };
    default:
      console.warn(`Unknown size type: ${sizeType}`);
      return {};
  }
};

export const getHeight = (height?: string): React.CSSProperties => {
  if (!height) return {};

  const [wantedHeight, minHeight, maxHeight] = height.split(',');

  return {
    ..._getWantedHeight(wantedHeight),
    ..._getMinHeight(minHeight),
    ..._getMaxHeight(maxHeight),
  };
};

const _getWantedHeight = (height?: string): React.CSSProperties => {
  if (!height) return {};

  const [sizeType, value] = height.split(':');

  switch (sizeType.toLowerCase()) {
    case 'units': {
      const units = parseFloat(value);
      return { height: `${units * 0.25}rem` };
    }

    case 'px':
      return { height: `${value}px` };

    case 'rem':
      return { height: `${value}rem` };

    case 'fraction': {
      const fraction = parseFloat(value);
      return { height: `${fraction * 100}%` };
    }

    case 'full':
      return { height: '100%' };

    case 'fit':
      return { height: 'fit-content' };

    case 'screen':
      return { height: '100vh' };

    case 'mincontent':
      return { height: 'min-content' };

    case 'maxcontent':
      return { height: 'max-content' };

    case 'auto':
      return { height: 'auto' };

    case 'grow':
      return { flexGrow: parseFloat(value) || 1 };

    case 'shrink':
      return { flexShrink: parseFloat(value) || 1 };

    default:
      console.warn(`Unknown size type: ${sizeType}`);
      return {};
  }
};

const _getMinHeight = (height?: string): React.CSSProperties => {
  if (!height) return {};

  const [sizeType, value] = height.split(':');
  switch (sizeType.toLowerCase()) {
    case 'units':
      return { minHeight: `${parseFloat(value) * 0.25}rem` };
    case 'px':
      return { minHeight: `${value}px` };
    case 'rem':
      return { minHeight: `${value}rem` };
    case 'fraction':
      return { minHeight: `${parseFloat(value) * 100}%` };
    case 'full':
      return { minHeight: '100%' };
    case 'fit':
      return { minHeight: 'fit-content' };
    case 'screen':
      return { minHeight: '100vh' };
    case 'mincontent':
      return { minHeight: 'min-content' };
    case 'maxcontent':
      return { minHeight: 'max-content' };
    case 'auto':
      return { minHeight: 'auto' };
    default:
      console.warn(`Unknown size type: ${sizeType}`);
      return {};
  }
};

const _getMaxHeight = (height?: string): React.CSSProperties => {
  if (!height) return {};

  const [sizeType, value] = height.split(':');
  switch (sizeType.toLowerCase()) {
    case 'units':
      return { maxHeight: `${parseFloat(value) * 0.25}rem` };
    case 'px':
      return { maxHeight: `${value}px` };
    case 'rem':
      return { maxHeight: `${value}rem` };
    case 'fraction':
      return { maxHeight: `${parseFloat(value) * 100}%` };
    case 'full':
      return { maxHeight: '100%' };
    case 'fit':
      return { maxHeight: 'fit-content' };
    case 'screen':
      return { maxHeight: '100vh' };
    case 'mincontent':
      return { maxHeight: 'min-content' };
    case 'maxcontent':
      return { maxHeight: 'max-content' };
    case 'auto':
      return { maxHeight: 'auto' };
    default:
      console.warn(`Unknown size type: ${sizeType}`);
      return {};
  }
};

export type BorderStyle = 'None' | 'Solid' | 'Dashed' | 'Dotted';

export const getBorderStyle = (
  borderStyle: BorderStyle
): React.CSSProperties => {
  return {
    borderStyle: borderStyle.toLowerCase(),
  };
};

export const getBorderThickness = (
  getBorderThickness?: string
): React.CSSProperties => {
  if (!getBorderThickness) return {};

  if (
    typeof getBorderThickness === 'string' &&
    getBorderThickness.indexOf(',') > -1
  ) {
    const [left, top, right, bottom] = getBorderThickness
      .split(',')
      .map(val => (val ? `${parseFloat(val)}px` : '0'));

    return {
      borderWidth: left,
      borderTopWidth: top,
      borderRightWidth: right,
      borderBottomWidth: bottom,
    };
  }

  return {
    borderWidth: `${parseFloat(getBorderThickness)}px`,
  };
};

export type BorderRadius = 'None' | 'Rounded' | 'Full';

export const getBorderRadius = (
  borderRadius?: BorderRadius
): React.CSSProperties => {
  if (!borderRadius) return {};
  return {
    borderRadius:
      borderRadius === 'Rounded'
        ? '0.5rem'
        : borderRadius === 'Full'
          ? '9999px'
          : '0',
  };
};

export const getPadding = (padding?: string): React.CSSProperties => {
  if (!padding) return {};

  if (typeof padding === 'string' && padding.indexOf(',') > -1) {
    const [left, top, right, bottom] = padding
      .split(',')
      .map(val => (val ? `${parseFloat(val) * 0.25}rem` : '0'));

    return {
      paddingLeft: left,
      paddingTop: top,
      paddingRight: right,
      paddingBottom: bottom,
    };
  }
  return {
    padding: `${parseFloat(padding) * 0.25}rem`,
  };
};

export const getMargin = (margin?: string): React.CSSProperties => {
  if (!margin) return {};

  if (typeof margin === 'string' && margin.indexOf(',') > -1) {
    const [left, top, right, bottom] = margin
      .split(',')
      .map(val => (val ? `${parseFloat(val) * 0.25}rem` : '0'));

    return {
      marginLeft: left,
      marginTop: top,
      marginRight: right,
      marginBottom: bottom,
    };
  }
  return {
    margin: `${parseFloat(margin) * 0.25}rem`,
  };
};

export const getGap = (gap?: number): React.CSSProperties => {
  if (!gap) return {};
  return {
    gap: `${gap * 0.25}rem`,
  };
};

export type Overflow = 'Clip' | 'Ellipsis' | 'Auto';

export const getOverflow = (overflow?: Overflow): React.CSSProperties => {
  if (!overflow) return {};

  if (overflow === 'Clip') {
    return {
      overflow: 'hidden',
    };
  }

  if (overflow === 'Ellipsis') {
    return {
      overflow: 'hidden',
      textOverflow: 'ellipsis',
      whiteSpace: 'nowrap',
    };
  }

  return {
    overflow: overflow.toLowerCase(),
  };
};

export type Orientation = 'Horizontal' | 'Vertical';

export type Align =
  | 'TopLeft'
  | 'TopRight'
  | 'TopCenter'
  | 'BottomLeft'
  | 'BottomRight'
  | 'BottomCenter'
  | 'Left'
  | 'Right'
  | 'Center'
  | 'Stretch';

export const getAlign = (
  orientation: Orientation,
  align?: Align
): React.CSSProperties => {
  const styles: React.CSSProperties = {
    display: 'flex',
  };

  styles.flexDirection = orientation === 'Horizontal' ? 'row' : 'column';

  // Prevent wrapping in horizontal layouts so that fractional widths (e.g., flex: 1, width: 50%) correctly share available space; wrapping would break the intended distribution.
  if (orientation === 'Horizontal') {
    styles.flexWrap = 'nowrap';
    styles.width = '100%';
    styles.minWidth = '100%';
    // Default to flex-start for horizontal layouts so fractional widths work properly
    if (!align) {
      styles.justifyContent = 'flex-start';
    }
  }

  if (orientation === 'Horizontal') {
    // Horizontal layout alignment
    switch (align) {
      case 'TopLeft':
      case 'TopCenter':
      case 'TopRight':
        styles.alignItems = 'flex-start';
        break;
      case 'BottomLeft':
      case 'BottomCenter':
      case 'BottomRight':
        styles.alignItems = 'flex-end';
        break;
      case 'Left':
      case 'Right':
      case 'Center':
        styles.alignItems = 'center';
        break;
      case 'Stretch':
        styles.alignItems = 'stretch !important';
        break;
    }

    switch (align) {
      case 'TopLeft':
      case 'Left':
      case 'BottomLeft':
        styles.justifyContent = 'flex-start';
        break;
      case 'TopRight':
      case 'Right':
      case 'BottomRight':
        styles.justifyContent = 'flex-end';
        break;
      case 'TopCenter':
      case 'Center':
      case 'BottomCenter':
        styles.justifyContent = 'center';
        break;
    }
  } else {
    // Vertical layout alignment
    // Set horizontal alignment (Left/Center/Right)
    switch (align) {
      case 'TopLeft':
      case 'Left':
      case 'BottomLeft':
        styles.alignItems = 'flex-start';
        break;
      case 'TopRight':
      case 'Right':
      case 'BottomRight':
        styles.alignItems = 'flex-end';
        break;
      case 'TopCenter':
      case 'Center':
      case 'BottomCenter':
        styles.alignItems = 'center';
        break;
      case 'Stretch':
        styles.alignItems = 'stretch !important';
        break;
    }

    // Set vertical alignment (Top/Center/Bottom)
    switch (align) {
      case 'TopLeft':
      case 'TopCenter':
      case 'TopRight':
        styles.justifyContent = 'flex-start';
        break;
      case 'Left':
      case 'Center':
      case 'Right':
        styles.justifyContent = 'center';
        break;
      case 'BottomLeft':
      case 'BottomCenter':
      case 'BottomRight':
        styles.justifyContent = 'flex-end';
        break;
    }
  }

  return styles;
};

export const getColor = (
  color?: string,
  cssProperty: 'color' | 'backgroundColor' | 'borderColor' = 'color',
  role: 'background' | 'foreground' = 'background',
  percentage: number | undefined = undefined
) => {
  if (!color) return {};
  const varName =
    color.toLowerCase() + (role === 'background' ? '' : '-foreground');
  if (percentage && percentage > -100 && percentage < 100) {
    return {
      [cssProperty]: `color-mix(in srgb, var(--${varName}),${percentage > 0 ? 'white' : 'black'} ${Math.abs(percentage)}%)`,
    };
  }
  return {
    [cssProperty]: 'var(--' + varName + ')',
  };
};
