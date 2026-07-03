import React, { useEffect, useRef, useCallback, useState } from "react";
import { Terminal as XTerm } from "@xterm/xterm";
import { FitAddon } from "@xterm/addon-fit";
import { WebLinksAddon } from "@xterm/addon-web-links";
import { ClipboardAddon } from "@xterm/addon-clipboard";
import { Unicode11Addon } from "@xterm/addon-unicode11";
import { CanvasAddon } from "@xterm/addon-canvas";
import xtermStyles from "@xterm/xterm/css/xterm.css?inline";
import { EventHandler, StreamSubscriber } from "./types";
import { getWidth, getHeight } from "./styles";

type CursorStyle = "Block" | "Underline" | "Bar";

interface TerminalTheme {
  background?: string;
  foreground?: string;
  cursor?: string;
  cursorAccent?: string;
  selection?: string;
  black?: string;
  red?: string;
  green?: string;
  yellow?: string;
  blue?: string;
  magenta?: string;
  cyan?: string;
  white?: string;
  brightBlack?: string;
  brightRed?: string;
  brightGreen?: string;
  brightYellow?: string;
  brightBlue?: string;
  brightMagenta?: string;
  brightCyan?: string;
  brightWhite?: string;
}

interface TerminalProps {
  id: string;
  width?: string;
  height?: string;
  events?: string[];
  eventHandler: EventHandler;
  subscribeToStream?: StreamSubscriber;
  cols?: number;
  rows?: number;
  fontFamily?: string;
  lineHeight?: number;
  cursorBlink?: boolean;
  cursorStyle?: CursorStyle;
  scrollback?: number;
  theme?: TerminalTheme;
  initialContent?: string;
  stream?: { id: string };
  closed?: boolean;
  allowClipboard?: boolean;
  autoFocus?: boolean;
  loading?: boolean;
  loadingText?: string;
  background?: string;
  foreground?: string;
}

const defaultTheme: TerminalTheme = {
  background: "#000000",
  foreground: "#d4d4d4",
  cursor: "#aeafad",
  cursorAccent: "#000000",
  selection: "rgba(255, 255, 255, 0.3)",
  black: "#000000",
  red: "#cd3131",
  green: "#0dbc79",
  yellow: "#e5e510",
  blue: "#2472c8",
  magenta: "#bc3fbc",
  cyan: "#11a8cd",
  white: "#e5e5e5",
  brightBlack: "#666666",
  brightRed: "#f14c4c",
  brightGreen: "#23d18b",
  brightYellow: "#f5f543",
  brightBlue: "#3b8eea",
  brightMagenta: "#d670d6",
  brightCyan: "#29b8db",
  brightWhite: "#ffffff",
};

const mapCursorStyle = (style?: CursorStyle): "block" | "underline" | "bar" => {
  switch (style) {
    case "Underline":
      return "underline";
    case "Bar":
      return "bar";
    case "Block":
    default:
      return "block";
  }
};

export const Terminal: React.FC<TerminalProps> = ({
  id,
  width = "Full",
  height = "Full",
  events = [],
  eventHandler,
  subscribeToStream,
  cols,
  rows,
  fontFamily = "'Cascadia Mono', Geist Mono, Menlo, Monaco, 'Courier New', monospace, 'Segoe UI Emoji', 'Apple Color Emoji', 'Noto Color Emoji'",
  lineHeight = 1.0,
  cursorBlink = true,
  cursorStyle = "Block",
  scrollback = 1000,
  theme,
  initialContent,
  stream,
  closed = false,
  allowClipboard = true,
  autoFocus = true,
  loading = false,
  loadingText = "Loading...",
  background,
  foreground,
}) => {
  const hostRef = useRef<HTMLDivElement>(null);
  const shadowRootRef = useRef<ShadowRoot | null>(null);
  const terminalContainerRef = useRef<HTMLDivElement | null>(null);
  const terminalRef = useRef<XTerm | null>(null);
  const fitAddonRef = useRef<FitAddon | null>(null);
  const initialContentWrittenRef = useRef(false);
  const terminalReadyRef = useRef(false);

  // Loading overlay: visible while `loading` is set, auto-hidden once visible
  // output arrives on the stream. ConPTY and shells emit escape-sequence
  // preambles (window title, cursor homing, mode sets) within milliseconds of
  // spawn — long before the process prints anything — so control sequences
  // and whitespace must not count as output.
  const streamDataReceivedRef = useRef(false);
  const [streamDataReceived, setStreamDataReceived] = useState(false);

  const markStreamDataIfVisible = useCallback((text: string) => {
    if (streamDataReceivedRef.current) return;
    const visible = text
      .replace(/\x1b\][\s\S]*?(\x07|\x1b\\|$)/g, "") // OSC (e.g. title set)
      .replace(/\x1b\[[0-9;?]*[ -/]*[@-~]/g, "") // CSI
      .replace(/\x1b[\s\S]/g, "") // other ESC sequences
      .replace(/[\x00-\x1f\x7f]/g, "") // remaining control chars
      .trim();
    if (visible.length > 0) {
      streamDataReceivedRef.current = true;
      setStreamDataReceived(true);
    }
  }, []);

  const isReadOnly = closed || !events.includes("OnInput");

  // Use refs to keep callbacks stable and avoid terminal recreation
  const isReadOnlyRef = useRef(isReadOnly);
  const eventsRef = useRef(events);
  const eventHandlerRef = useRef(eventHandler);
  const cursorBlinkRef = useRef(cursorBlink);
  const allowClipboardRef = useRef(allowClipboard);
  isReadOnlyRef.current = isReadOnly;
  eventsRef.current = events;
  eventHandlerRef.current = eventHandler;
  cursorBlinkRef.current = cursorBlink;
  allowClipboardRef.current = allowClipboard;

  const handleData = useCallback(
    (data: string) => {
      if (!isReadOnlyRef.current && typeof eventHandlerRef.current === "function") {
        try {
          eventHandlerRef.current("OnInput", id, [data]);
        } catch { }
      }
    },
    [id],
  );

  const handleResize = useCallback(
    (size: { cols: number; rows: number }) => {
      if (eventsRef.current.includes("OnResize") && typeof eventHandlerRef.current === "function") {
        try {
          eventHandlerRef.current("OnResize", id, [{ cols: size.cols, rows: size.rows }]);
        } catch { }
      }
    },
    [id],
  );

  const handleLinkClick = useCallback(
    (event: MouseEvent, uri: string) => {
      // Only activate link if CTRL is held
      if (!event.ctrlKey) return;

      if (
        eventsRef.current.includes("OnLinkClick") &&
        typeof eventHandlerRef.current === "function"
      ) {
        eventHandlerRef.current("OnLinkClick", id, [uri]);
      } else {
        // Default behavior: open link in new tab
        window.open(uri, "_blank", "noopener,noreferrer");
      }
    },
    [id],
  );

  // Initialize Shadow DOM and terminal
  useEffect(() => {
    if (!hostRef.current) return;

    // Create shadow root if it doesn't exist
    if (!shadowRootRef.current) {
      shadowRootRef.current = hostRef.current.attachShadow({ mode: "open" });

      // Inject xterm styles into shadow root
      const styleEl = document.createElement("style");
      styleEl.textContent =
        xtermStyles +
        `
        :host {
          display: block;
          width: 100%;
          height: 100%;
        }
        .terminal-container {
          width: 100%;
          height: 100%;
          background: var(--background) !important;
          padding: 10px 0px 10px 10px;
          box-sizing: border-box;
        }
        .xterm-screen .xterm-rows {
          color: var(--foreground) !important;
        }
        .xterm-viewport {
          background: var(--background) !important;
        }
        .xterm .xterm-viewport {
          background: var(--background) !important;
        }
        .xterm-dom-renderer-owner-1 .xterm-rows {
          color: var(--foreground) !important;
        }
      `;
      shadowRootRef.current.appendChild(styleEl);

      // Create terminal container inside shadow root
      const container = document.createElement("div");
      container.className = "terminal-container";
      shadowRootRef.current.appendChild(container);
      terminalContainerRef.current = container;
    }

    const container = terminalContainerRef.current;
    if (!container) return;

    // Clear any existing terminal content and reset state
    container.innerHTML = "";
    initialContentWrittenRef.current = false;
    terminalReadyRef.current = false;
    streamDataReceivedRef.current = false;
    setStreamDataReceived(false);

    const mergedTheme = { ...defaultTheme, ...theme };

    // Set CSS variables on the container to scope them to the terminal
    if (background) {
      const bg = `var(--${background.toLowerCase()})`;
      container.style.setProperty("--background", bg);
      const computedBg = getComputedStyle(container).getPropertyValue("--" + background.toLowerCase()).trim();
      if (computedBg) mergedTheme.background = computedBg;
    } else {
      container.style.setProperty("--background", mergedTheme.background || "#000000");
    }
    if (foreground) {
      const fg = `var(--${foreground.toLowerCase()})`;
      container.style.setProperty("--foreground", fg);
      const computedFg = getComputedStyle(container).getPropertyValue("--" + foreground.toLowerCase()).trim();
      if (computedFg) mergedTheme.foreground = computedFg;
    } else {
      container.style.setProperty("--foreground", mergedTheme.foreground || "#d4d4d4");
    }

    const term = new XTerm({
      ...(cols !== undefined && { cols }),
      ...(rows !== undefined && { rows }),
      fontSize: 14,
      fontFamily,
      lineHeight,
      cursorBlink: isReadOnly ? false : cursorBlink,
      cursorStyle: mapCursorStyle(cursorStyle),
      scrollback,
      theme: mergedTheme,
      allowProposedApi: true,
      disableStdin: isReadOnly,
    });

    // Load Unicode11 addon for emoji support
    const unicode11Addon = new Unicode11Addon();
    term.loadAddon(unicode11Addon);
    term.unicode.activeVersion = "11";

    const fitAddon = new FitAddon();
    term.loadAddon(fitAddon);

    // Create tooltip element for link hover
    const tooltip = document.createElement("div");
    tooltip.style.cssText = `
      position: fixed;
      background: #f5f5f5;
      color: #1a1a1a;
      padding: 4px 8px;
      border-radius: 4px;
      font-size: 12px;
      font-family: Geist, system-ui, sans-serif;
      pointer-events: none;
      z-index: 10000;
      display: none;
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
      border: 1px solid #e0e0e0;
    `;
    tooltip.textContent = "Ctrl+Click to Follow Link";
    document.body.appendChild(tooltip);

    // Load WebLinks addon with custom handler and hover
    const webLinksAddon = new WebLinksAddon(handleLinkClick, {
      hover: (event: MouseEvent, _uri: string) => {
        tooltip.style.display = "block";
        tooltip.style.left = `${event.clientX + 10}px`;
        tooltip.style.top = `${event.clientY + 10}px`;
      },
      leave: () => {
        tooltip.style.display = "none";
      },
    });
    term.loadAddon(webLinksAddon);

    // Load Clipboard addon if allowed (handles copy)
    if (allowClipboard) {
      const clipboardAddon = new ClipboardAddon();
      term.loadAddon(clipboardAddon);
    }

    // Paste handler for browser paste events (right-click paste, etc.) on
    // xterm's internal textarea. Defined here (outer scope) so the cleanup
    // closure below can remove the exact same listener reference.
    const handleTextareaPaste = (e: ClipboardEvent) => {
      if (!allowClipboardRef.current || term.options.disableStdin) return;
      e.preventDefault();
      const text = e.clipboardData?.getData("text");
      if (text) {
        term.paste(text);
      }
    };

    let disposed = false;

    // Container padding (see .terminal-container CSS: "10px 0px 10px 10px").
    const PAD_TOP = 10;
    const PAD_BOTTOM = 10;

    // FitAddon computes rows from the fractional cell height, but the canvas
    // renderer rounds each row up to whole pixels. Over many rows that excess
    // can exceed the bottom padding, pushing the last row past the container
    // and clipping it. After fitting, if the rendered terminal overflows the
    // available height, drop one row so the final line stays fully visible.
    const doFit = () => {
      const fa = fitAddonRef.current;
      const t = terminalRef.current;
      if (!fa || !t) return;
      fa.fit();
      const rendered = t.element?.offsetHeight ?? 0;
      const available = container.clientHeight - PAD_TOP - PAD_BOTTOM;
      if (rendered > available + 1 && t.rows > 1) {
        t.resize(t.cols, t.rows - 1);
      }
    };

    // Defer opening until container has dimensions
    requestAnimationFrame(() => {
      if (disposed) return;

      term.open(container);

      // Intercept Ctrl+V / Cmd+V before xterm consumes the key event. xterm.js
      // captures keyboard input on its own internal hidden <textarea>, so
      // container-level keydown listeners never fire — attachCustomKeyEventHandler
      // runs before xterm processes the key, which is the standard approach for
      // clipboard handling inside a Shadow DOM.
      term.attachCustomKeyEventHandler((e: KeyboardEvent) => {
        if (!allowClipboardRef.current || term.options.disableStdin) return true;
        if (e.type === "keydown" && (e.ctrlKey || e.metaKey) && e.key === "v") {
          e.preventDefault();
          navigator.clipboard
            .readText()
            .then((text) => {
              if (text) term.paste(text);
            })
            .catch(() => {
              // Clipboard API unavailable — browser paste event will handle it
            });
          return false; // Prevent xterm from processing this key
        }
        // Allow Ctrl+C to copy selected text (xterm handles this natively with ClipboardAddon)
        return true;
      });

      if (term.textarea) {
        term.textarea.addEventListener("paste", handleTextareaPaste);
      }

      // Canvas renderer draws box-drawing and block-element glyphs procedurally
      // (customGlyphs, on by default), so the Claude logo and other block art
      // tile seamlessly like Windows Terminal instead of relying on font metrics.
      // Must be loaded after open(); the canvas renderer works reliably inside
      // the Shadow DOM (unlike WebGL). Fall back to the DOM renderer on failure.
      try {
        const canvasAddon = new CanvasAddon();
        term.loadAddon(canvasAddon);

        // The canvas renderer caches glyphs in a texture atlas on first paint.
        // If a webfont (e.g. Geist Mono) hasn't finished loading yet, missing
        // glyphs get cached as tofu until the cell is invalidated. Once fonts
        // are ready, drop the atlas and repaint so those glyphs render correctly.
        if (document.fonts?.ready) {
          document.fonts.ready.then(() => {
            if (disposed) return;
            try {
              canvasAddon.clearTextureAtlas();
              term.refresh(0, term.rows - 1);
            } catch {
              // Addon disposed or refresh failed — ignore.
            }
          });
        }
      } catch {
        // Canvas unsupported — DOM renderer remains in place.
      }

      terminalRef.current = term;
      fitAddonRef.current = fitAddon;

      // Register handlers before fit() so initial size is reported
      term.onData(handleData);
      term.onResize(handleResize);

      if (!cols && !rows) {
        doFit();
        // Fire initial size since fit() may not trigger onResize if size matches default
        handleResize({ cols: term.cols, rows: term.rows });
      }

      if (initialContent && !initialContentWrittenRef.current) {
        term.write(initialContent);
        initialContentWrittenRef.current = true;
      }

      // Mark terminal as ready after initialization is complete
      terminalReadyRef.current = true;

      // Automatically focus the terminal on mount so it receives keyboard
      // input immediately, unless read-only or auto focus is disabled.
      if (autoFocus && !isReadOnly) {
        term.focus();
      }
    });

    const handleWindowResize = () => {
      if (!cols && !rows && terminalReadyRef.current && fitAddonRef.current) {
        doFit();
      }
    };

    window.addEventListener("resize", handleWindowResize);

    const resizeObserver = new ResizeObserver(() => {
      if (!cols && !rows && terminalReadyRef.current && fitAddonRef.current) {
        doFit();
      }
    });

    resizeObserver.observe(hostRef.current);

    return () => {
      disposed = true;
      terminalReadyRef.current = false;
      window.removeEventListener("resize", handleWindowResize);
      if (term.textarea) {
        term.textarea.removeEventListener("paste", handleTextareaPaste);
      }
      resizeObserver.disconnect();

      // Remove tooltip
      tooltip.remove();

      term.dispose();
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id, stream?.id]);

  // Handle closed/readonly state changes without recreating terminal
  useEffect(() => {
    if (terminalRef.current) {
      terminalRef.current.options.disableStdin = isReadOnly;
      terminalRef.current.options.cursorBlink = isReadOnly ? false : cursorBlinkRef.current;
      // Hide/show cursor using ANSI escape sequences
      if (isReadOnly) {
        terminalRef.current.write("\x1b[?25l"); // Hide cursor
      } else {
        terminalRef.current.write("\x1b[?25h"); // Show cursor
      }
    }
  }, [isReadOnly]);

  // Subscribe to stream data (base64 encoded to preserve control characters)
  useEffect(() => {
    if (!stream?.id || !subscribeToStream) return;

    const decoder = new TextDecoder("utf-8");

    const unsubscribe = subscribeToStream(stream.id, (data) => {
      if (terminalRef.current && typeof data === "string") {
        try {
          // Decode base64 to binary string, then to UTF-8
          const binaryString = atob(data);
          const bytes = new Uint8Array(binaryString.length);
          for (let i = 0; i < binaryString.length; i++) {
            bytes[i] = binaryString.charCodeAt(i);
          }
          const text = decoder.decode(bytes, { stream: true });
          // Debug: log first chunk to verify encoding
          if (bytes.length > 0 && bytes.length < 100) {
            console.log("[Terminal] base64 decoded:", {
              rawLength: data.length,
              decodedLength: bytes.length,
              firstBytes: Array.from(bytes.slice(0, 20)),
              text: text.slice(0, 50),
            });
          }
          markStreamDataIfVisible(text);
          terminalRef.current.write(text);
        } catch (e) {
          // Fallback for non-base64 data
          console.warn("[Terminal] base64 decode failed, using raw data:", e, {
            data: data.slice(0, 50),
          });
          markStreamDataIfVisible(data);
          terminalRef.current.write(data);
        }
      } else {
        console.warn("[Terminal] unexpected data type:", typeof data, data);
      }
    });

    return unsubscribe;
  }, [stream?.id, subscribeToStream, markStreamDataIfVisible]);

  const style: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
    overflow: "hidden",
    position: "relative",
  };

  const showLoading = loading && !streamDataReceived && !closed;

  const overlayForeground = foreground
    ? `var(--${foreground.toLowerCase()})`
    : defaultTheme.foreground;

  return (
    <div style={style}>
      <div ref={hostRef} style={{ width: "100%", height: "100%" }} />
      {showLoading && (
        <div
          style={{
            position: "absolute",
            inset: 0,
            display: "flex",
            flexDirection: "column",
            alignItems: "center",
            justifyContent: "center",
            gap: "12px",
            pointerEvents: "none",
          }}
        >
          <style>{`@keyframes ivy-xterm-loading-spin { to { transform: rotate(360deg); } }`}</style>
          <div
            style={{
              width: "24px",
              height: "24px",
              borderRadius: "50%",
              border: "3px solid rgba(128, 128, 128, 0.3)",
              borderTopColor: overlayForeground,
              animation: "ivy-xterm-loading-spin 0.8s linear infinite",
            }}
          />
          <span
            style={{
              color: overlayForeground,
              fontFamily,
              fontSize: "13px",
            }}
          >
            {loadingText}
          </span>
        </div>
      )}
    </div>
  );
};
