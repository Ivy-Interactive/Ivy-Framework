import { useEventHandler } from "@/components/event-handler";
import { InvalidIcon } from "@/components/InvalidIcon";
import { useThemeWithMonitoring } from "@/components/theme-provider";
import { inputStyles } from "@/lib/styles";
import { getCSSVariable } from "@/lib/theme";
import { cn } from "@/lib/utils";
import { Eraser } from "lucide-react";
import React, { useCallback, useEffect, useRef, useState } from "react";
import { EMPTY_ARRAY } from "@/lib/constants";
import { Densities } from "@/types/density";

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
  density?: Densities;
  "data-testid"?: string;
}

const placeholderTextMap: Record<Densities, string> = {
  [Densities.Small]: "text-xs",
  [Densities.Medium]: "text-sm",
  [Densities.Large]: "text-base",
};

const clearButtonMap: Record<Densities, string> = {
  [Densities.Small]: "p-1 top-1 right-1",
  [Densities.Medium]: "p-1.5 top-2 right-2",
  [Densities.Large]: "p-2 top-3 right-3",
};

const clearIconMap: Record<Densities, string> = {
  [Densities.Small]: "h-3 w-3",
  [Densities.Medium]: "h-4 w-4",
  [Densities.Large]: "h-5 w-5",
};

export function resolveColor(color: string | undefined, fallback: string): string {
  if (!color) return fallback;
  if (color.startsWith("#") || color.startsWith("rgb")) return color;
  const cssValue = getCSSVariable(`--${color.toLowerCase()}`);
  return cssValue || fallback;
}

export const SignatureInputWidget: React.FC<SignatureInputWidgetProps> = ({
  id,
  value,
  disabled = false,
  invalid,
  events = EMPTY_ARRAY,
  pen,
  background,
  penThickness = 2,
  placeholder,
  density = Densities.Medium,
  "data-testid": dataTestId,
}) => {
  const eventHandler = useEventHandler();
  const { colors, isDark } = useThemeWithMonitoring({
    monitorDOM: true,
    monitorSystem: true,
  });
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const containerRef = useRef<HTMLDivElement>(null);
  const isFocusedRef = useRef(false);
  const [isDrawing, setIsDrawing] = useState(false);
  const [lastPoint, setLastPoint] = useState<Point | null>(null);
  const [hasDrawn, setHasDrawn] = useState(false);
  const pathsRef = useRef<Point[][]>([]);
  const currentPathRef = useRef<Point[]>([]);

  const defaultPen = colors.foreground || (isDark ? "#e4e4e7" : "#000000");
  const defaultBg = colors.background || (isDark ? "#09090b" : "#ffffff");
  const penColor = resolveColor(pen, defaultPen);
  const bgColor = resolveColor(background, defaultBg);

  const clearCanvas = useCallback(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;
    const ctx = canvas.getContext("2d");
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
        const ctx = canvas.getContext("2d");
        if (!ctx) return;
        ctx.fillStyle = bgColor;
        ctx.fillRect(0, 0, canvas.width, canvas.height);
        ctx.drawImage(img, 0, 0, canvas.width, canvas.height);
        setHasDrawn(true);
      };
      // Value from C# is raw base64 (byte[] serialized); add data URL prefix for img.src
      img.src = value.startsWith("data:") ? value : `data:image/png;base64,${value}`;
    } else {
      clearCanvas();
      setTimeout(() => setHasDrawn(false), 0);
      pathsRef.current = [];
    }
  }, [value, bgColor, clearCanvas]);

  const getCanvasPoint = (e: React.MouseEvent | React.TouchEvent): Point | null => {
    const canvas = canvasRef.current;
    if (!canvas) return null;
    const rect = canvas.getBoundingClientRect();
    const scaleX = canvas.width / rect.width;
    const scaleY = canvas.height / rect.height;

    if ("touches" in e) {
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
    const ctx = canvas.getContext("2d");
    if (!ctx) return;

    ctx.strokeStyle = penColor;
    ctx.lineWidth = penThickness;
    ctx.lineCap = "round";
    ctx.lineJoin = "round";

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
    const dataUrl = canvas.toDataURL("image/png");
    const base64 = dataUrl.split(",")[1] ?? dataUrl;
    eventHandler("OnChange", id, [base64]);
  };

  const handleClear = () => {
    clearCanvas();
    pathsRef.current = [];
    currentPathRef.current = [];
    setHasDrawn(false);
    eventHandler("OnChange", id, [null]);
  };

  const handleBlur = useCallback(() => {
    if (disabled) return;
    if (!isFocusedRef.current) return;
    if (events.includes("OnBlur")) eventHandler("OnBlur", id, []);
    isFocusedRef.current = false;
  }, [disabled, events, eventHandler, id]);

  const handleFocus = useCallback(() => {
    if (disabled) return;
    if (isFocusedRef.current) return;
    if (events.includes("OnFocus")) eventHandler("OnFocus", id, []);
    isFocusedRef.current = true;
  }, [disabled, events, eventHandler, id]);

  // Ensure focus/blur are reliable even with a non-focusable canvas + drawing events.
  useEffect(() => {
    if (!events?.includes("OnBlur") && !events?.includes("OnFocus")) return;

    const onPointerDown = (e: PointerEvent) => {
      const el = containerRef.current;
      if (!el) return;

      const target = e.target as Node | null;
      const clickedInside = !!target && el.contains(target);

      if (clickedInside) {
        el.focus();
        handleFocus();
        return;
      }

      // Clicked outside
      if (isFocusedRef.current) {
        handleBlur();
      }
    };

    window.addEventListener("pointerdown", onPointerDown, true);
    return () => window.removeEventListener("pointerdown", onPointerDown, true);
  }, [events, handleBlur, handleFocus]);

  return (
    <div
      ref={containerRef}
      className={cn(
        "relative w-full h-full rounded-md border border-input overflow-hidden",
        disabled && "opacity-50 cursor-not-allowed",
        invalid && inputStyles.invalidInput,
      )}
      onBlur={handleBlur}
      onFocus={handleFocus}
      tabIndex={disabled ? -1 : 0}
      onPointerDown={() => {
        if (disabled) return;
        containerRef.current?.focus();
        handleFocus();
      }}
      data-testid={dataTestId}
    >
      <canvas
        ref={canvasRef}
        className={cn("block w-full h-full", disabled ? "cursor-not-allowed" : "cursor-crosshair")}
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
        <div
          className={cn(
            "absolute inset-0 flex items-center justify-center pointer-events-none text-muted-foreground",
            placeholderTextMap[density],
          )}
        >
          {placeholder}
        </div>
      )}

      {/* Clear button */}
      {hasDrawn && !disabled && (
        <button
          type="button"
          onClick={handleClear}
          className={cn(
            "absolute rounded-md bg-background/80 border border-input hover:bg-accent transition-colors cursor-pointer",
            clearButtonMap[density],
          )}
          aria-label="Clear signature"
        >
          <Eraser className={cn(clearIconMap[density], "text-muted-foreground")} />
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
