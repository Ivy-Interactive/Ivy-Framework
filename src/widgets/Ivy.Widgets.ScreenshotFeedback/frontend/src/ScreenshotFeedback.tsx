import React, { useState, useEffect, useRef, useCallback } from "react";
import html2canvas from "html2canvas-pro";
import { DrawingTool, Shape, AnnotationData, EventHandler } from "./types";
import { DrawingCanvas } from "./DrawingCanvas";
import { Toolbar } from "./Toolbar";
import "./styles.css";

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
  const [screenshotCanvas, setScreenshotCanvas] =
    useState<HTMLCanvasElement | null>(null);
  const [shapes, setShapes] = useState<Shape[]>([]);
  const [activeTool, setActiveTool] = useState<DrawingTool>(
    DrawingTool.Callout,
  );
  const [color, setColor] = useState("#ef4444");
  const [lineWidth, setLineWidth] = useState(4);
  const [capturing, setCapturing] = useState(false);
  const [submitting, setSubmitting] = useState(false);
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
            return el.getAttribute("data-screenshot-overlay") === "true";
          },
          useCORS: true,
          logging: false,
        })
          .then((canvas) => {
            setScreenshotCanvas(canvas);
            setCapturing(false);
          })
          .catch((err) => {
            console.error("Screenshot capture failed:", err);
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

  const onShapeAdd = useCallback((shape: Shape) => {
    setShapes((prev) => [...prev, shape]);
  }, []);

  const onUndo = useCallback(() => {
    setShapes((prev) => prev.slice(0, -1));
  }, []);

  const buildAnnotationData = useCallback((): AnnotationData => {
    return {
      shapes,
      screenshotWidth: screenshotCanvas?.width ?? 0,
      screenshotHeight: screenshotCanvas?.height ?? 0,
    };
  }, [shapes, screenshotCanvas]);

  const onSave = useCallback(async () => {
    if (!screenshotCanvas) return;

    setSubmitting(true);

    // First: merge canvases and upload the screenshot
    try {
      const mergedCanvas = document.createElement("canvas");
      mergedCanvas.width = screenshotCanvas.width;
      mergedCanvas.height = screenshotCanvas.height;
      const ctx = mergedCanvas.getContext("2d");
      if (!ctx) return;

      ctx.drawImage(screenshotCanvas, 0, 0);

      const annotationCanvas = overlayRef.current?.querySelector(
        ".screenshot-canvas-wrapper canvas:nth-child(2)",
      ) as HTMLCanvasElement | null;
      if (annotationCanvas) {
        ctx.drawImage(annotationCanvas, 0, 0);
      }

      const blob = await new Promise<Blob | null>((resolve) => {
        mergedCanvas.toBlob(resolve, "image/png");
      });

      if (blob && uploadUrl) {
        const getUploadUrl = () => {
          const ivyHostMeta = document.querySelector('meta[name="ivy-host"]');
          if (ivyHostMeta) {
            const host = ivyHostMeta.getAttribute("content");
            return host + uploadUrl;
          }
          return uploadUrl;
        };

        const formData = new FormData();
        formData.append("file", blob, "screenshot.png");

        const response = await fetch(getUploadUrl(), {
          method: "POST",
          body: formData,
        });
        if (!response.ok) {
          console.error("Screenshot upload failed:", response.statusText);
          return;
        }
      }
    } catch (error) {
      console.error("Screenshot save error:", error);
      return;
    } finally {
      setSubmitting(false);
    }

    // Then fire the event (upload is now complete, C# handler can read the content)
    if (events.includes("OnSave")) {
      eventHandler("OnSave", id, [buildAnnotationData()]);
    }
  }, [
    screenshotCanvas,
    uploadUrl,
    events,
    eventHandler,
    id,
    buildAnnotationData,
  ]);

  const onCancel = useCallback(() => {
    if (events.includes("OnCancel")) {
      eventHandler("OnCancel", id, []);
    }
  }, [events, eventHandler, id]);

  // Handle keyboard shortcuts
  useEffect(() => {
    if (!isOpen) return;
    const handleKeyDown = (e: KeyboardEvent) => {
      // Ctrl+S to save
      if (e.key === "s" && (e.ctrlKey || e.metaKey)) {
        e.preventDefault();
        onSave();
        return;
      }
      // Ctrl+Z to undo
      if (e.key === "z" && (e.ctrlKey || e.metaKey)) {
        e.preventDefault();
        onUndo();
        return;
      }
      // ESC — only cancel if no active tool input (text/callout inputs handle their own ESC via stopPropagation)
      if (e.key === "Escape") {
        onCancel();
      }
    };
    window.addEventListener("keydown", handleKeyDown);
    return () => window.removeEventListener("keydown", handleKeyDown);
  }, [isOpen, onCancel, onUndo, onSave]);

  if (!isOpen) return null;

  if (capturing || !screenshotCanvas) {
    return (
      <div className="screenshot-overlay" data-screenshot-overlay="true">
        <div
          style={{
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
            flex: 1,
            color: "hsl(var(--muted-foreground))",
            fontSize: 18,
            fontFamily: "var(--font-sans, sans-serif)",
          }}
        >
          Capturing screenshot...
        </div>
      </div>
    );
  }

  return (
    <div
      className="screenshot-overlay"
      ref={overlayRef}
      data-screenshot-overlay="true"
    >
      <Toolbar
        activeTool={activeTool}
        color={color}
        lineWidth={lineWidth}
        onToolChange={setActiveTool}
        onColorChange={setColor}
        onLineWidthChange={setLineWidth}
        onUndo={onUndo}
        onSave={onSave}
        onCancel={onCancel}
        canUndo={shapes.length > 0}
        submitting={submitting}
      />
      <div className="screenshot-canvas-container">
        <DrawingCanvas
          screenshotCanvas={screenshotCanvas}
          activeTool={activeTool}
          color={color}
          lineWidth={lineWidth}
          shapes={shapes}
          onShapeAdd={onShapeAdd}
        />
      </div>
    </div>
  );
};
