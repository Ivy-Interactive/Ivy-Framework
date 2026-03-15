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

function drawShape(ctx: CanvasRenderingContext2D, shape: Shape) {
  ctx.strokeStyle = shape.color;
  ctx.fillStyle = shape.color;
  ctx.lineWidth = shape.lineWidth;
  ctx.lineCap = 'round';
  ctx.lineJoin = 'round';

  switch (shape.tool) {
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
    case DrawingTool.Text: {
      ctx.font = `${shape.fontSize}px sans-serif`;
      ctx.fillText(shape.text, shape.position.x, shape.position.y);
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
  const [textInput, setTextInput] = useState<{ position: Point; visible: boolean }>({
    position: { x: 0, y: 0 },
    visible: false,
  });
  const [textValue, setTextValue] = useState('');
  const textInputRef = useRef<HTMLInputElement>(null);

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

  // Redraw all shapes whenever shapes array changes
  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    ctx.clearRect(0, 0, canvas.width, canvas.height);
    shapes.forEach((shape) => drawShape(ctx, shape));
  }, [shapes]);

  // Draw preview of current shape being drawn
  const drawPreview = useCallback(
    (currentPoint: Point) => {
      const canvas = canvasRef.current;
      if (!canvas) return;
      const ctx = canvas.getContext('2d');
      if (!ctx) return;

      // Redraw all committed shapes
      ctx.clearRect(0, 0, canvas.width, canvas.height);
      shapes.forEach((shape) => drawShape(ctx, shape));

      // Draw preview
      ctx.strokeStyle = color;
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
        }
      }
    },
    [shapes, color, lineWidth, activeTool, currentPoints, startPoint]
  );

  const handlePointerDown = useCallback(
    (e: React.MouseEvent<HTMLCanvasElement>) => {
      const point = getCanvasPoint(e);

      if (activeTool === DrawingTool.Text) {
        const canvas = canvasRef.current;
        if (!canvas) return;
        const rect = canvas.getBoundingClientRect();
        setTextInput({
          position: { x: e.clientX - rect.left, y: e.clientY - rect.top },
          visible: true,
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
    [activeTool, getCanvasPoint]
  );

  const handlePointerMove = useCallback(
    (e: React.MouseEvent<HTMLCanvasElement>) => {
      if (!isDrawing) return;
      const point = getCanvasPoint(e);

      if (activeTool === DrawingTool.Freehand) {
        setCurrentPoints((prev) => [...prev, point]);
      }
      drawPreview(point);
    },
    [isDrawing, activeTool, getCanvasPoint, drawPreview]
  );

  const handlePointerUp = useCallback(
    (e: React.MouseEvent<HTMLCanvasElement>) => {
      if (!isDrawing || !startPoint) {
        setIsDrawing(false);
        return;
      }

      const endPoint = getCanvasPoint(e);

      switch (activeTool) {
        case DrawingTool.Freehand:
          onShapeAdd({
            tool: DrawingTool.Freehand,
            color,
            lineWidth,
            points: [...currentPoints, endPoint],
          });
          break;
        case DrawingTool.Line:
          onShapeAdd({
            tool: DrawingTool.Line,
            color,
            lineWidth,
            start: startPoint,
            end: endPoint,
          });
          break;
        case DrawingTool.Rectangle:
          onShapeAdd({
            tool: DrawingTool.Rectangle,
            color,
            lineWidth,
            start: startPoint,
            end: endPoint,
          });
          break;
        case DrawingTool.Circle: {
          const dx = endPoint.x - startPoint.x;
          const dy = endPoint.y - startPoint.y;
          onShapeAdd({
            tool: DrawingTool.Circle,
            color,
            lineWidth,
            center: startPoint,
            radius: Math.sqrt(dx * dx + dy * dy),
          });
          break;
        }
      }

      setIsDrawing(false);
      setStartPoint(null);
      setCurrentPoints([]);
    },
    [isDrawing, startPoint, activeTool, color, lineWidth, currentPoints, getCanvasPoint, onShapeAdd]
  );

  const handleTouchStart = useCallback(
    (e: React.TouchEvent<HTMLCanvasElement>) => {
      e.preventDefault();
      const point = getCanvasPoint(e);
      if (activeTool === DrawingTool.Text) return;
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
      const endPoint = getCanvasPoint(e);

      switch (activeTool) {
        case DrawingTool.Freehand:
          onShapeAdd({
            tool: DrawingTool.Freehand,
            color,
            lineWidth,
            points: [...currentPoints, endPoint],
          });
          break;
        case DrawingTool.Line:
          onShapeAdd({
            tool: DrawingTool.Line,
            color,
            lineWidth,
            start: startPoint,
            end: endPoint,
          });
          break;
        case DrawingTool.Rectangle:
          onShapeAdd({
            tool: DrawingTool.Rectangle,
            color,
            lineWidth,
            start: startPoint,
            end: endPoint,
          });
          break;
        case DrawingTool.Circle: {
          const dx = endPoint.x - startPoint.x;
          const dy = endPoint.y - startPoint.y;
          onShapeAdd({
            tool: DrawingTool.Circle,
            color,
            lineWidth,
            center: startPoint,
            radius: Math.sqrt(dx * dx + dy * dy),
          });
          break;
        }
      }

      setIsDrawing(false);
      setStartPoint(null);
      setCurrentPoints([]);
    },
    [isDrawing, startPoint, activeTool, color, lineWidth, currentPoints, getCanvasPoint, onShapeAdd]
  );

  const handleTextSubmit = useCallback(() => {
    if (!textValue.trim()) {
      setTextInput((prev) => ({ ...prev, visible: false }));
      return;
    }

    const canvas = canvasRef.current;
    if (!canvas) return;
    const rect = canvas.getBoundingClientRect();
    const scaleX = canvas.width / rect.width;
    const scaleY = canvas.height / rect.height;

    const fontSize = 16 * Math.max(scaleX, scaleY);

    onShapeAdd({
      tool: DrawingTool.Text,
      color,
      lineWidth,
      position: {
        x: textInput.position.x * scaleX,
        y: textInput.position.y * scaleY,
      },
      text: textValue,
      fontSize,
    });

    setTextInput((prev) => ({ ...prev, visible: false }));
    setTextValue('');
  }, [textValue, textInput.position, color, lineWidth, onShapeAdd]);

  const width = screenshotCanvas?.width ?? 0;
  const height = screenshotCanvas?.height ?? 0;

  // Scale canvas to fit in viewport while maintaining aspect ratio
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
          if (isDrawing) {
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
          }}
          value={textValue}
          onChange={(e) => setTextValue(e.target.value)}
          onKeyDown={(e) => {
            if (e.key === 'Enter') handleTextSubmit();
            if (e.key === 'Escape') {
              setTextInput((prev) => ({ ...prev, visible: false }));
              setTextValue('');
            }
          }}
          onBlur={handleTextSubmit}
        />
      )}
    </div>
  );
};
