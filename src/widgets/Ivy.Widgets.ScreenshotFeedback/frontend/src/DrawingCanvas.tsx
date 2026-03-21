import React, { useRef, useState, useEffect, useCallback } from 'react';
import { DrawingTool, Shape, Point } from './types';

interface DrawingCanvasProps {
  screenshotCanvas: HTMLCanvasElement | null;
  activeTool: DrawingTool;
  color: string;
  lineWidth: number;
  shapes: Shape[];
  onShapeAdd: (shape: Shape) => void;
}

function drawArrowhead(ctx: CanvasRenderingContext2D, from: Point, to: Point, headLen: number) {
  const angle = Math.atan2(to.y - from.y, to.x - from.x);
  ctx.beginPath();
  ctx.moveTo(to.x, to.y);
  ctx.lineTo(to.x - headLen * Math.cos(angle - Math.PI / 6), to.y - headLen * Math.sin(angle - Math.PI / 6));
  ctx.moveTo(to.x, to.y);
  ctx.lineTo(to.x - headLen * Math.cos(angle + Math.PI / 6), to.y - headLen * Math.sin(angle + Math.PI / 6));
  ctx.stroke();
}

function drawCallout(ctx: CanvasRenderingContext2D, anchor: Point, label: Point, number: number, text: string, color: string, lw: number, fontSize: number = 14, circleRadius: number = 14) {
  const r = circleRadius;
  ctx.strokeStyle = color;
  ctx.fillStyle = color;
  ctx.lineWidth = lw;
  ctx.lineCap = 'round';

  // Line from label to anchor
  ctx.beginPath();
  ctx.moveTo(label.x, label.y);
  ctx.lineTo(anchor.x, anchor.y);
  ctx.stroke();

  // Arrowhead at anchor
  const headLen = Math.max(10, lw * 3);
  drawArrowhead(ctx, label, anchor, headLen);

  // Number circle at label
  ctx.beginPath();
  ctx.arc(label.x, label.y, r, 0, Math.PI * 2);
  ctx.fill();

  // Number
  ctx.fillStyle = '#fff';
  ctx.font = `bold ${fontSize}px sans-serif`;
  ctx.textAlign = 'center';
  ctx.textBaseline = 'middle';
  ctx.fillText(String(number), label.x, label.y);

  // Text
  if (text) {
    ctx.fillStyle = color;
    ctx.font = `${fontSize}px sans-serif`;
    ctx.textAlign = 'left';
    ctx.textBaseline = 'middle';
    ctx.fillText(text, label.x + r + 6, label.y + fontSize * 0.05);
  }

  ctx.textAlign = 'start';
  ctx.textBaseline = 'alphabetic';
}

function drawCensor(ctx: CanvasRenderingContext2D, start: Point, end: Point, _color: string, _lw: number, srcCanvas?: HTMLCanvasElement | null) {
  const x = Math.min(start.x, end.x);
  const y = Math.min(start.y, end.y);
  const w = Math.abs(end.x - start.x);
  const h = Math.abs(end.y - start.y);
  if (w < 4 || h < 4) return;

  // Heavy blur: downscale then upscale to create pixelation + blur effect
  const blurSize = 16;

  if (srcCanvas) {
    const srcCtx = srcCanvas.getContext('2d');
    if (srcCtx) {
      // Create tiny canvas for heavy downscale
      const tiny = document.createElement('canvas');
      const tw = Math.max(1, Math.ceil(w / blurSize));
      const th = Math.max(1, Math.ceil(h / blurSize));
      tiny.width = tw;
      tiny.height = th;
      const tCtx = tiny.getContext('2d');
      if (tCtx) {
        // Draw region scaled down (browser averages pixels)
        tCtx.drawImage(srcCanvas, x, y, w, h, 0, 0, tw, th);
        // Draw back scaled up with smoothing disabled for blocky look
        ctx.save();
        ctx.imageSmoothingEnabled = false;
        ctx.drawImage(tiny, 0, 0, tw, th, x, y, w, h);
        // Draw again with smoothing for a softer pass on top
        ctx.globalAlpha = 0.5;
        ctx.imageSmoothingEnabled = true;
        ctx.drawImage(tiny, 0, 0, tw, th, x, y, w, h);
        ctx.restore();
        return;
      }
    }
  }

  // Fallback: solid dark fill
  ctx.fillStyle = '#222';
  ctx.fillRect(x, y, w, h);
}

function drawShape(ctx: CanvasRenderingContext2D, shape: Shape, _srcCanvas?: HTMLCanvasElement | null) {
  ctx.strokeStyle = shape.color;
  ctx.fillStyle = shape.color;
  ctx.lineWidth = shape.lineWidth;
  ctx.lineCap = 'round';
  ctx.lineJoin = 'round';

  switch (shape.tool) {
    case DrawingTool.Callout:
      drawCallout(ctx, shape.anchor, shape.label, shape.number, shape.text, shape.color, shape.lineWidth, shape.fontSize, Math.round(shape.fontSize * 0.9));
      break;
    case DrawingTool.Freehand: {
      if (shape.points.length < 2) return;
      ctx.beginPath();
      ctx.moveTo(shape.points[0].x, shape.points[0].y);
      for (let i = 1; i < shape.points.length; i++) {
        ctx.lineTo(shape.points[i].x, shape.points[i].y);
      }
      ctx.stroke();
      break;
    }
    case DrawingTool.Arrow: {
      ctx.beginPath();
      ctx.moveTo(shape.start.x, shape.start.y);
      ctx.lineTo(shape.end.x, shape.end.y);
      ctx.stroke();
      drawArrowhead(ctx, shape.start, shape.end, Math.max(12, shape.lineWidth * 4));
      break;
    }
    case DrawingTool.Line: {
      ctx.beginPath();
      ctx.moveTo(shape.start.x, shape.start.y);
      ctx.lineTo(shape.end.x, shape.end.y);
      ctx.stroke();
      break;
    }
    case DrawingTool.Rectangle: {
      const x = Math.min(shape.start.x, shape.end.x);
      const y = Math.min(shape.start.y, shape.end.y);
      const w = Math.abs(shape.end.x - shape.start.x);
      const h = Math.abs(shape.end.y - shape.start.y);
      ctx.strokeRect(x, y, w, h);
      break;
    }
    case DrawingTool.Circle: {
      ctx.beginPath();
      ctx.arc(shape.center.x, shape.center.y, shape.radius, 0, Math.PI * 2);
      ctx.stroke();
      break;
    }
    case DrawingTool.Censor:
      drawCensor(ctx, shape.start, shape.end, shape.color, shape.lineWidth, _srcCanvas);
      break;
    case DrawingTool.Text: {
      ctx.font = `${shape.fontSize}px sans-serif`;
      ctx.textBaseline = 'top';
      ctx.fillText(shape.text, shape.position.x, shape.position.y + shape.fontSize * 0.05);
      break;
    }
  }
}

export const DrawingCanvas: React.FC<DrawingCanvasProps> = ({
  screenshotCanvas,
  activeTool,
  color,
  lineWidth,
  shapes,
  onShapeAdd,
}) => {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const [isDrawing, setIsDrawing] = useState(false);
  const [startPoint, setStartPoint] = useState<Point | null>(null);
  const [currentPoints, setCurrentPoints] = useState<Point[]>([]);

  // Callout two-click state
  const [calloutAnchor, setCalloutAnchor] = useState<Point | null>(null);
  const [calloutHover, setCalloutHover] = useState<Point | null>(null);

  const [textInput, setTextInput] = useState<{ position: Point; visible: boolean; mode: 'text' | 'callout'; canvasAnchor?: Point; canvasLabel?: Point }>({
    position: { x: 0, y: 0 },
    visible: false,
    mode: 'text',
  });
  const [textValue, setTextValue] = useState('');
  const textInputRef = useRef<HTMLInputElement>(null);

  const nextCalloutNumber = shapes.filter((s) => s.tool === DrawingTool.Callout).length + 1;

  const getCanvasPoint = useCallback(
    (e: React.MouseEvent<HTMLCanvasElement> | React.TouchEvent<HTMLCanvasElement>): Point => {
      const canvas = canvasRef.current;
      if (!canvas) return { x: 0, y: 0 };
      const rect = canvas.getBoundingClientRect();
      const scaleX = canvas.width / rect.width;
      const scaleY = canvas.height / rect.height;

      if ('touches' in e) {
        const touch = e.touches[0] || e.changedTouches[0];
        return {
          x: (touch.clientX - rect.left) * scaleX,
          y: (touch.clientY - rect.top) * scaleY,
        };
      }
      return {
        x: (e.clientX - rect.left) * scaleX,
        y: (e.clientY - rect.top) * scaleY,
      };
    },
    []
  );

  // Redraw
  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    shapes.forEach((shape) => drawShape(ctx, shape, screenshotCanvas));

    // Compute scaled font size for callout previews
    const rect = canvas.getBoundingClientRect();
    const cScale = canvas.width / (rect.width || 1);
    const cFontSize = Math.round(16 * cScale);
    const cRadius = Math.round(cFontSize * 0.9);

    // Draw pending callout while text input is visible
    if (textInput.visible && textInput.mode === 'callout' && textInput.canvasAnchor && textInput.canvasLabel) {
      drawCallout(ctx, textInput.canvasAnchor, textInput.canvasLabel, nextCalloutNumber, '', color, lineWidth, cFontSize, cRadius);
    }

    // Draw callout preview (first click placed, hovering for second click)
    if (calloutAnchor && calloutHover && !textInput.visible) {
      drawCallout(ctx, calloutAnchor, calloutHover, nextCalloutNumber, '', color, lineWidth, cFontSize, cRadius);
    }
  }, [shapes, textInput, nextCalloutNumber, color, lineWidth, calloutAnchor, calloutHover]);

  // Draw preview for drag-based tools
  const drawPreview = useCallback(
    (currentPoint: Point) => {
      const canvas = canvasRef.current;
      if (!canvas) return;
      const ctx = canvas.getContext('2d');
      if (!ctx) return;

      ctx.clearRect(0, 0, canvas.width, canvas.height);
      shapes.forEach((shape) => drawShape(ctx, shape, screenshotCanvas));

      ctx.strokeStyle = color;
      ctx.fillStyle = color;
      ctx.lineWidth = lineWidth;
      ctx.lineCap = 'round';
      ctx.lineJoin = 'round';

      if (activeTool === DrawingTool.Freehand && currentPoints.length > 1) {
        ctx.beginPath();
        ctx.moveTo(currentPoints[0].x, currentPoints[0].y);
        for (let i = 1; i < currentPoints.length; i++) {
          ctx.lineTo(currentPoints[i].x, currentPoints[i].y);
        }
        ctx.lineTo(currentPoint.x, currentPoint.y);
        ctx.stroke();
      } else if (startPoint) {
        switch (activeTool) {
          case DrawingTool.Arrow:
            ctx.beginPath();
            ctx.moveTo(startPoint.x, startPoint.y);
            ctx.lineTo(currentPoint.x, currentPoint.y);
            ctx.stroke();
            drawArrowhead(ctx, startPoint, currentPoint, Math.max(12, lineWidth * 4));
            break;
          case DrawingTool.Line:
            ctx.beginPath();
            ctx.moveTo(startPoint.x, startPoint.y);
            ctx.lineTo(currentPoint.x, currentPoint.y);
            ctx.stroke();
            break;
          case DrawingTool.Rectangle: {
            const x = Math.min(startPoint.x, currentPoint.x);
            const y = Math.min(startPoint.y, currentPoint.y);
            const w = Math.abs(currentPoint.x - startPoint.x);
            const h = Math.abs(currentPoint.y - startPoint.y);
            ctx.strokeRect(x, y, w, h);
            break;
          }
          case DrawingTool.Circle: {
            const dx = currentPoint.x - startPoint.x;
            const dy = currentPoint.y - startPoint.y;
            const radius = Math.sqrt(dx * dx + dy * dy);
            ctx.beginPath();
            ctx.arc(startPoint.x, startPoint.y, radius, 0, Math.PI * 2);
            ctx.stroke();
            break;
          }
          case DrawingTool.Censor:
            drawCensor(ctx, startPoint, currentPoint, color, lineWidth, screenshotCanvas);
            break;
        }
      }
    },
    [shapes, color, lineWidth, activeTool, currentPoints, startPoint, screenshotCanvas]
  );

  const handlePointerDown = useCallback(
    (e: React.MouseEvent<HTMLCanvasElement>) => {
      if (textInput.visible) return;
      const point = getCanvasPoint(e);

      // Callout: two-click flow
      if (activeTool === DrawingTool.Callout) {
        if (!calloutAnchor) {
          // First click: set the anchor (target)
          setCalloutAnchor(point);
        } else {
          // Second click: set the label position, show text input
          const canvas = canvasRef.current;
          if (!canvas) return;
          const rect = canvas.getBoundingClientRect();
          const scaleX = canvas.width / rect.width;
          const scaleY = canvas.height / rect.height;
          setTextInput({
            position: { x: point.x / scaleX + 22, y: point.y / scaleY - 8 },
            visible: true,
            mode: 'callout',
            canvasAnchor: calloutAnchor,
            canvasLabel: point,
          });
          setTextValue('');
          setCalloutHover(null);
          setTimeout(() => textInputRef.current?.focus(), 0);
        }
        return;
      }

      if (activeTool === DrawingTool.Text) {
        const canvas = canvasRef.current;
        if (!canvas) return;
        const rect = canvas.getBoundingClientRect();
        setTextInput({
          position: { x: e.clientX - rect.left, y: e.clientY - rect.top },
          visible: true,
          mode: 'text',
        });
        setTextValue('');
        setTimeout(() => textInputRef.current?.focus(), 0);
        return;
      }

      setIsDrawing(true);
      setStartPoint(point);
      if (activeTool === DrawingTool.Freehand) {
        setCurrentPoints([point]);
      }
    },
    [activeTool, getCanvasPoint, calloutAnchor, textInput.visible]
  );

  const handlePointerMove = useCallback(
    (e: React.MouseEvent<HTMLCanvasElement>) => {
      // Callout hover preview
      if (activeTool === DrawingTool.Callout && calloutAnchor && !textInput.visible) {
        setCalloutHover(getCanvasPoint(e));
        return;
      }

      if (!isDrawing) return;
      const point = getCanvasPoint(e);

      if (activeTool === DrawingTool.Freehand) {
        setCurrentPoints((prev) => [...prev, point]);
      }
      drawPreview(point);
    },
    [isDrawing, activeTool, getCanvasPoint, drawPreview, calloutAnchor, textInput.visible]
  );

  const finalizeShape = useCallback(
    (endPoint: Point) => {
      if (!startPoint) return;

      switch (activeTool) {
        case DrawingTool.Freehand:
          onShapeAdd({ tool: DrawingTool.Freehand, color, lineWidth, points: [...currentPoints, endPoint] });
          break;
        case DrawingTool.Arrow:
          onShapeAdd({ tool: DrawingTool.Arrow, color, lineWidth, start: startPoint, end: endPoint });
          break;
        case DrawingTool.Line:
          onShapeAdd({ tool: DrawingTool.Line, color, lineWidth, start: startPoint, end: endPoint });
          break;
        case DrawingTool.Rectangle:
          onShapeAdd({ tool: DrawingTool.Rectangle, color, lineWidth, start: startPoint, end: endPoint });
          break;
        case DrawingTool.Circle: {
          const dx = endPoint.x - startPoint.x;
          const dy = endPoint.y - startPoint.y;
          onShapeAdd({ tool: DrawingTool.Circle, color, lineWidth, center: startPoint, radius: Math.sqrt(dx * dx + dy * dy) });
          break;
        }
        case DrawingTool.Censor:
          onShapeAdd({ tool: DrawingTool.Censor, color, lineWidth, start: startPoint, end: endPoint });
          break;
      }
    },
    [startPoint, activeTool, color, lineWidth, currentPoints, onShapeAdd]
  );

  const handlePointerUp = useCallback(
    (e: React.MouseEvent<HTMLCanvasElement>) => {
      if (activeTool === DrawingTool.Callout) return; // callout uses clicks, not drag
      if (!isDrawing || !startPoint) {
        setIsDrawing(false);
        return;
      }
      finalizeShape(getCanvasPoint(e));
      setIsDrawing(false);
      setStartPoint(null);
      setCurrentPoints([]);
    },
    [isDrawing, startPoint, getCanvasPoint, finalizeShape, activeTool]
  );

  const handleTouchStart = useCallback(
    (e: React.TouchEvent<HTMLCanvasElement>) => {
      e.preventDefault();
      if (activeTool === DrawingTool.Text || activeTool === DrawingTool.Callout) return;
      const point = getCanvasPoint(e);
      setIsDrawing(true);
      setStartPoint(point);
      if (activeTool === DrawingTool.Freehand) {
        setCurrentPoints([point]);
      }
    },
    [activeTool, getCanvasPoint]
  );

  const handleTouchMove = useCallback(
    (e: React.TouchEvent<HTMLCanvasElement>) => {
      e.preventDefault();
      if (!isDrawing) return;
      const point = getCanvasPoint(e);
      if (activeTool === DrawingTool.Freehand) {
        setCurrentPoints((prev) => [...prev, point]);
      }
      drawPreview(point);
    },
    [isDrawing, activeTool, getCanvasPoint, drawPreview]
  );

  const handleTouchEnd = useCallback(
    (e: React.TouchEvent<HTMLCanvasElement>) => {
      e.preventDefault();
      if (!isDrawing || !startPoint) {
        setIsDrawing(false);
        return;
      }
      finalizeShape(getCanvasPoint(e));
      setIsDrawing(false);
      setStartPoint(null);
      setCurrentPoints([]);
    },
    [isDrawing, startPoint, getCanvasPoint, finalizeShape]
  );

  const handleTextSubmit = useCallback(() => {
    const canvas = canvasRef.current;
    if (!canvas) {
      setTextInput((prev) => ({ ...prev, visible: false }));
      return;
    }
    const rect = canvas.getBoundingClientRect();
    const scaleX = canvas.width / rect.width;
    const scaleY = canvas.height / rect.height;

    if (textInput.mode === 'callout') {
      if (textInput.canvasAnchor && textInput.canvasLabel) {
        const rect = canvas.getBoundingClientRect();
        const cScale = canvas.width / (rect.width || 1);
        const cFontSize = Math.round(16 * cScale);
        onShapeAdd({
          tool: DrawingTool.Callout,
          color,
          lineWidth,
          anchor: textInput.canvasAnchor,
          label: textInput.canvasLabel,
          number: nextCalloutNumber,
          text: textValue.trim(),
          fontSize: cFontSize,
        });
      }
      setTextInput((prev) => ({ ...prev, visible: false }));
      setTextValue('');
      setCalloutAnchor(null);
      setCalloutHover(null);
      return;
    }

    if (!textValue.trim()) {
      setTextInput((prev) => ({ ...prev, visible: false }));
      return;
    }

    const fontSize = 16 * Math.max(scaleX, scaleY);
    onShapeAdd({
      tool: DrawingTool.Text,
      color,
      lineWidth,
      position: { x: textInput.position.x * scaleX, y: textInput.position.y * scaleY },
      text: textValue,
      fontSize,
    });
    setTextInput((prev) => ({ ...prev, visible: false }));
    setTextValue('');
  }, [textValue, textInput, color, lineWidth, onShapeAdd, nextCalloutNumber]);

  const width = screenshotCanvas?.width ?? 0;
  const height = screenshotCanvas?.height ?? 0;

  const maxWidth = window.innerWidth - 32;
  const maxHeight = window.innerHeight - 80;
  const scale = Math.min(1, maxWidth / width, maxHeight / height);
  const displayWidth = width * scale;
  const displayHeight = height * scale;

  return (
    <div className="screenshot-canvas-wrapper">
      {screenshotCanvas && (
        <canvas
          ref={(el) => {
            if (el && screenshotCanvas) {
              const ctx = el.getContext('2d');
              if (ctx) {
                el.width = screenshotCanvas.width;
                el.height = screenshotCanvas.height;
                ctx.drawImage(screenshotCanvas, 0, 0);
              }
            }
          }}
          style={{ width: displayWidth, height: displayHeight, display: 'block' }}
        />
      )}
      <canvas
        ref={canvasRef}
        width={width}
        height={height}
        style={{
          position: 'absolute',
          top: 0,
          left: 0,
          width: displayWidth,
          height: displayHeight,
          cursor: activeTool === DrawingTool.Text ? 'text' : 'crosshair',
        }}
        onMouseDown={handlePointerDown}
        onMouseMove={handlePointerMove}
        onMouseUp={handlePointerUp}
        onMouseLeave={() => {
          if (isDrawing && activeTool !== DrawingTool.Callout) {
            setIsDrawing(false);
            setStartPoint(null);
            setCurrentPoints([]);
          }
        }}
        onTouchStart={handleTouchStart}
        onTouchMove={handleTouchMove}
        onTouchEnd={handleTouchEnd}
      />
      {textInput.visible && (
        <input
          ref={textInputRef}
          className="screenshot-text-input"
          style={{
            left: textInput.position.x,
            top: textInput.position.y,
            color: color,
            fontSize: '16px',
            fontFamily: 'sans-serif',
          }}
          placeholder={textInput.mode === 'callout' ? 'Add note...' : ''}
          value={textValue}
          onChange={(e) => setTextValue(e.target.value)}
          onKeyDown={(e) => {
            if (e.key === 'Enter') {
              e.preventDefault();
              handleTextSubmit();
            }
            if (e.key === 'Escape') {
              e.stopPropagation();
              setTextInput((prev) => ({ ...prev, visible: false }));
              setTextValue('');
              setCalloutAnchor(null);
              setCalloutHover(null);
            }
          }}
          onBlur={handleTextSubmit}
        />
      )}
    </div>
  );
};
