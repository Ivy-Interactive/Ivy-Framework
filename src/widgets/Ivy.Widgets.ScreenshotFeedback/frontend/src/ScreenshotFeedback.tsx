import React, { useState, useEffect, useRef, useCallback } from 'react';
import html2canvas from 'html2canvas-pro';
import { DrawingTool, Shape, EventHandler } from './types';
import { DrawingCanvas } from './DrawingCanvas';
import { Toolbar } from './Toolbar';
import './styles.css';

interface ScreenshotFeedbackProps {
  id: string;
  uploadUrl?: string;
  isOpen: boolean;
  events?: string[];
  eventHandler: EventHandler;
}

export const ScreenshotFeedback: React.FC<ScreenshotFeedbackProps> = ({
  id,
  uploadUrl,
  isOpen,
  events = [],
  eventHandler,
}) => {
  const [screenshotCanvas, setScreenshotCanvas] = useState<HTMLCanvasElement | null>(null);
  const [shapes, setShapes] = useState<Shape[]>([]);
  const [activeTool, setActiveTool] = useState<DrawingTool>(DrawingTool.Freehand);
  const [color, setColor] = useState('#ef4444');
  const [lineWidth, setLineWidth] = useState(4);
  const [capturing, setCapturing] = useState(false);
  const overlayRef = useRef<HTMLDivElement>(null);
  const prevIsOpenRef = useRef(false);

  // Capture screenshot when isOpen transitions from false to true
  useEffect(() => {
    if (isOpen && !prevIsOpenRef.current) {
      setCapturing(true);
      setShapes([]);

      // Small delay to let the component unmount overlay before capturing
      const timer = setTimeout(() => {
        html2canvas(document.body, {
          ignoreElements: (el) => {
            return el.getAttribute('data-screenshot-overlay') === 'true';
          },
          useCORS: true,
          logging: false,
        })
          .then((canvas) => {
            setScreenshotCanvas(canvas);
            setCapturing(false);
          })
          .catch((err) => {
            console.error('Screenshot capture failed:', err);
            setCapturing(false);
          });
      }, 50);

      return () => clearTimeout(timer);
    }

    if (!isOpen) {
      setScreenshotCanvas(null);
      setShapes([]);
    }

    prevIsOpenRef.current = isOpen;
  }, [isOpen]);

  const handleShapeAdd = useCallback((shape: Shape) => {
    setShapes((prev) => [...prev, shape]);
  }, []);

  const handleUndo = useCallback(() => {
    setShapes((prev) => prev.slice(0, -1));
  }, []);

  const handleSave = useCallback(async () => {
    if (!screenshotCanvas) return;

    // Merge screenshot and annotations into one canvas
    const mergedCanvas = document.createElement('canvas');
    mergedCanvas.width = screenshotCanvas.width;
    mergedCanvas.height = screenshotCanvas.height;
    const ctx = mergedCanvas.getContext('2d');
    if (!ctx) return;

    // Draw screenshot
    ctx.drawImage(screenshotCanvas, 0, 0);

    // Draw annotations from the annotation canvas
    const annotationCanvas = overlayRef.current?.querySelector(
      '.screenshot-canvas-wrapper canvas:nth-child(2)'
    ) as HTMLCanvasElement | null;
    if (annotationCanvas) {
      ctx.drawImage(annotationCanvas, 0, 0);
    }

    // Export as PNG blob
    const blob = await new Promise<Blob | null>((resolve) => {
      mergedCanvas.toBlob(resolve, 'image/png');
    });

    if (!blob) return;

    // Upload if uploadUrl is provided
    if (uploadUrl) {
      const getUploadUrl = () => {
        const ivyHostMeta = document.querySelector('meta[name="ivy-host"]');
        if (ivyHostMeta) {
          const host = ivyHostMeta.getAttribute('content');
          return host + uploadUrl;
        }
        return uploadUrl;
      };

      const formData = new FormData();
      formData.append('file', blob, 'screenshot.png');

      try {
        const response = await fetch(getUploadUrl(), {
          method: 'POST',
          body: formData,
        });
        if (!response.ok) {
          console.error('Screenshot upload failed:', response.statusText);
        }
      } catch (error) {
        console.error('Screenshot upload error:', error);
      }
    }

    if (events.includes('OnSave')) {
      eventHandler('OnSave', id, []);
    }
  }, [screenshotCanvas, uploadUrl, events, eventHandler, id]);

  const handleCancel = useCallback(() => {
    if (events.includes('OnCancel')) {
      eventHandler('OnCancel', id, []);
    }
  }, [events, eventHandler, id]);

  // Handle Escape key
  useEffect(() => {
    if (!isOpen) return;
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') {
        handleCancel();
      }
      if (e.key === 'z' && (e.ctrlKey || e.metaKey)) {
        e.preventDefault();
        handleUndo();
      }
    };
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [isOpen, handleCancel, handleUndo]);

  if (!isOpen) return null;

  if (capturing || !screenshotCanvas) {
    return (
      <div className="screenshot-overlay" data-screenshot-overlay="true">
        <div
          style={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            flex: 1,
            color: '#e0e0e0',
            fontSize: 18,
          }}
        >
          Capturing screenshot...
        </div>
      </div>
    );
  }

  return (
    <div className="screenshot-overlay" ref={overlayRef} data-screenshot-overlay="true">
      <Toolbar
        activeTool={activeTool}
        color={color}
        lineWidth={lineWidth}
        onToolChange={setActiveTool}
        onColorChange={setColor}
        onLineWidthChange={setLineWidth}
        onUndo={handleUndo}
        onSave={handleSave}
        onCancel={handleCancel}
        canUndo={shapes.length > 0}
      />
      <div className="screenshot-canvas-container">
        <DrawingCanvas
          screenshotCanvas={screenshotCanvas}
          activeTool={activeTool}
          color={color}
          lineWidth={lineWidth}
          shapes={shapes}
          onShapeAdd={handleShapeAdd}
        />
      </div>
    </div>
  );
};
