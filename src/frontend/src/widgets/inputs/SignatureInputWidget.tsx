import { useEventHandler } from '@/components/event-handler';
import { InvalidIcon } from '@/components/InvalidIcon';
import { inputStyles } from '@/lib/styles';
import { cn } from '@/lib/utils';
import { Eraser } from 'lucide-react';
import React, { useCallback, useEffect, useRef, useState } from 'react';

interface Point {
  x: number;
  y: number;
}

interface SignatureInputWidgetProps {
  id: string;
  value?: string | null;
  disabled?: boolean;
  invalid?: string;
  events?: string[];
  pen?: string;
  background?: string;
  penThickness?: number;
  placeholder?: string;
  'data-testid'?: string;
}

const colorMap: Record<string, string> = {
  black: '#000000',
  white: '#ffffff',
  slate: '#64748b',
  gray: '#6b7280',
  zinc: '#71717a',
  neutral: '#737373',
  stone: '#78716c',
  red: '#ef4444',
  orange: '#f97316',
  amber: '#f59e0b',
  yellow: '#eab308',
  lime: '#84cc16',
  green: '#22c55e',
  emerald: '#10b981',
  teal: '#14b8a6',
  cyan: '#06b6d4',
  sky: '#0ea5e9',
  blue: '#3b82f6',
  indigo: '#6366f1',
  violet: '#8b5cf6',
  purple: '#a855f7',
  fuchsia: '#d946ef',
  pink: '#ec4899',
  rose: '#f43f5e',
  primary: '#3b82f6',
  secondary: '#6b7280',
  destructive: '#ef4444',
  success: '#22c55e',
  warning: '#f59e0b',
  info: '#0ea5e9',
  muted: '#6b7280',
};

function resolveColor(color: string | undefined, fallback: string): string {
  if (!color) return fallback;
  if (color.startsWith('#') || color.startsWith('rgb')) return color;
  return colorMap[color.toLowerCase()] ?? fallback;
}

export const SignatureInputWidget: React.FC<SignatureInputWidgetProps> = ({
  id,
  value,
  disabled = false,
  invalid,
  events = [],
  pen,
  background,
  penThickness = 2,
  placeholder,
  'data-testid': dataTestId,
}) => {
  const eventHandler = useEventHandler();
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const containerRef = useRef<HTMLDivElement>(null);
  const [isDrawing, setIsDrawing] = useState(false);
  const [lastPoint, setLastPoint] = useState<Point | null>(null);
  const [hasDrawn, setHasDrawn] = useState(false);
  const pathsRef = useRef<Point[][]>([]);
  const currentPathRef = useRef<Point[]>([]);

  const penColor = resolveColor(pen, '#000000');
  const bgColor = resolveColor(background, '#ffffff');

  const clearCanvas = useCallback(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;
    ctx.fillStyle = bgColor;
    ctx.fillRect(0, 0, canvas.width, canvas.height);
  }, [bgColor]);

  // Initialize canvas and load existing value
  useEffect(() => {
    const canvas = canvasRef.current;
    const container = containerRef.current;
    if (!canvas || !container) return;

    canvas.width = container.clientWidth;
    canvas.height = container.clientHeight;

    if (value) {
      const img = new Image();
      img.onload = () => {
        const ctx = canvas.getContext('2d');
        if (!ctx) return;
        ctx.fillStyle = bgColor;
        ctx.fillRect(0, 0, canvas.width, canvas.height);
        ctx.drawImage(img, 0, 0, canvas.width, canvas.height);
        setHasDrawn(true);
      };
      // Value from C# is raw base64 (byte[] serialized); add data URL prefix for img.src
      img.src = value.startsWith('data:')
        ? value
        : `data:image/png;base64,${value}`;
    } else {
      clearCanvas();
      setTimeout(() => setHasDrawn(false), 0);
      pathsRef.current = [];
    }
  }, [value, bgColor, clearCanvas]);

  const getCanvasPoint = (
    e: React.MouseEvent | React.TouchEvent
  ): Point | null => {
    const canvas = canvasRef.current;
    if (!canvas) return null;
    const rect = canvas.getBoundingClientRect();
    const scaleX = canvas.width / rect.width;
    const scaleY = canvas.height / rect.height;

    if ('touches' in e) {
      const touch = e.touches[0];
      if (!touch) return null;
      return {
        x: (touch.clientX - rect.left) * scaleX,
        y: (touch.clientY - rect.top) * scaleY,
      };
    }
    return {
      x: (e.clientX - rect.left) * scaleX,
      y: (e.clientY - rect.top) * scaleY,
    };
  };

  const drawLine = (from: Point, to: Point) => {
    const canvas = canvasRef.current;
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    ctx.strokeStyle = penColor;
    ctx.lineWidth = penThickness;
    ctx.lineCap = 'round';
    ctx.lineJoin = 'round';

    ctx.beginPath();
    ctx.moveTo(from.x, from.y);
    ctx.lineTo(to.x, to.y);
    ctx.stroke();
  };

  const handleStart = (e: React.MouseEvent | React.TouchEvent) => {
    if (disabled) return;
    e.preventDefault();
    const point = getCanvasPoint(e);
    if (!point) return;
    setIsDrawing(true);
    setLastPoint(point);
    currentPathRef.current = [point];
  };

  const handleMove = (e: React.MouseEvent | React.TouchEvent) => {
    if (!isDrawing || disabled) return;
    e.preventDefault();
    const point = getCanvasPoint(e);
    if (!point || !lastPoint) return;

    drawLine(lastPoint, point);
    currentPathRef.current.push(point);
    setLastPoint(point);
    if (!hasDrawn) setHasDrawn(true);
  };

  const handleEnd = () => {
    if (!isDrawing) return;
    setIsDrawing(false);
    setLastPoint(null);

    if (currentPathRef.current.length > 0) {
      pathsRef.current.push([...currentPathRef.current]);
      currentPathRef.current = [];
    }

    // Emit the canvas as raw base64 (C# deserializes base64 strings to byte[])
    const canvas = canvasRef.current;
    if (!canvas) return;
    const dataUrl = canvas.toDataURL('image/png');
    const base64 = dataUrl.split(',')[1] ?? dataUrl;
    eventHandler('OnChange', id, [base64]);
  };

  const handleClear = () => {
    clearCanvas();
    pathsRef.current = [];
    currentPathRef.current = [];
    setHasDrawn(false);
    eventHandler('OnChange', id, [null]);
  };

  const handleBlur = () => {
    if (events.includes('OnBlur')) {
      eventHandler('OnBlur', id, []);
    }
  };

  return (
    <div
      ref={containerRef}
      className={cn(
        'relative w-full h-full rounded-md border border-input overflow-hidden',
        disabled && 'opacity-50 cursor-not-allowed',
        invalid && inputStyles.invalidInput
      )}
      onBlur={handleBlur}
      tabIndex={0}
      data-testid={dataTestId}
    >
      <canvas
        ref={canvasRef}
        className={cn(
          'block w-full h-full',
          disabled ? 'cursor-not-allowed' : 'cursor-crosshair'
        )}
        onMouseDown={handleStart}
        onMouseMove={handleMove}
        onMouseUp={handleEnd}
        onMouseLeave={handleEnd}
        onTouchStart={handleStart}
        onTouchMove={handleMove}
        onTouchEnd={handleEnd}
      />

      {/* Placeholder */}
      {!hasDrawn && placeholder && (
        <div className="absolute inset-0 flex items-center justify-center pointer-events-none text-muted-foreground text-sm">
          {placeholder}
        </div>
      )}

      {/* Clear button */}
      {hasDrawn && !disabled && (
        <button
          type="button"
          onClick={handleClear}
          className="absolute top-2 right-2 p-1.5 rounded-md bg-background/80 border border-input hover:bg-accent transition-colors cursor-pointer"
          aria-label="Clear signature"
        >
          <Eraser className="h-4 w-4 text-muted-foreground" />
        </button>
      )}

      {/* Invalid icon */}
      {invalid && (
        <div className="absolute top-2 left-2">
          <InvalidIcon message={invalid} />
        </div>
      )}
    </div>
  );
};
