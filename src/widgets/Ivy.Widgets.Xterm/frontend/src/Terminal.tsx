import React, { useEffect, useRef, useCallback } from 'react';
import { Terminal as XTerm } from '@xterm/xterm';
import { FitAddon } from '@xterm/addon-fit';
import { WebLinksAddon } from '@xterm/addon-web-links';
import { ClipboardAddon } from '@xterm/addon-clipboard';
import { Unicode11Addon } from '@xterm/addon-unicode11';
import xtermStyles from '@xterm/xterm/css/xterm.css?inline';
import { EventHandler, StreamSubscriber } from './types';
import { getWidth, getHeight } from './styles';

type CursorStyle = 'Block' | 'Underline' | 'Bar';

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
}

const defaultTheme: TerminalTheme = {
  background: '#000000',
  foreground: '#d4d4d4',
  cursor: '#aeafad',
  cursorAccent: '#000000',
  selection: 'rgba(255, 255, 255, 0.3)',
  black: '#000000',
  red: '#cd3131',
  green: '#0dbc79',
  yellow: '#e5e510',
  blue: '#2472c8',
  magenta: '#bc3fbc',
  cyan: '#11a8cd',
  white: '#e5e5e5',
  brightBlack: '#666666',
  brightRed: '#f14c4c',
  brightGreen: '#23d18b',
  brightYellow: '#f5f543',
  brightBlue: '#3b8eea',
  brightMagenta: '#d670d6',
  brightCyan: '#29b8db',
  brightWhite: '#ffffff',
};

const mapCursorStyle = (style?: CursorStyle): 'block' | 'underline' | 'bar' => {
  switch (style) {
    case 'Underline':
      return 'underline';
    case 'Bar':
      return 'bar';
    case 'Block':
    default:
      return 'block';
  }
};

export const Terminal: React.FC<TerminalProps> = ({
  id,
  width = 'Full',
  height = 'Full',
  events = [],
  eventHandler,
  subscribeToStream,
  cols,
  rows,
  fontFamily = "Geist Mono, Menlo, Monaco, 'Courier New', monospace, 'Segoe UI Emoji', 'Apple Color Emoji', 'Noto Color Emoji'",
  lineHeight = 1.0,
  cursorBlink = true,
  cursorStyle = 'Block',
  scrollback = 1000,
  theme,
  initialContent,
  stream,
  closed = false,
  allowClipboard = true,
}) => {
  const hostRef = useRef<HTMLDivElement>(null);
  const shadowRootRef = useRef<ShadowRoot | null>(null);
  const terminalContainerRef = useRef<HTMLDivElement | null>(null);
  const terminalRef = useRef<XTerm | null>(null);
  const fitAddonRef = useRef<FitAddon | null>(null);
  const initialContentWrittenRef = useRef(false);

  const isReadOnly = closed || !events.includes('OnInput');

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
      if (!isReadOnlyRef.current) {
        eventHandlerRef.current('OnInput', id, [data]);
      }
    },
    [id]
  );

  const handleResize = useCallback(
    (size: { cols: number; rows: number }) => {
      if (eventsRef.current.includes('OnResize')) {
        eventHandlerRef.current('OnResize', id, [{ cols: size.cols, rows: size.rows }]);
      }
    },
    [id]
  );

  const handleLinkClick = useCallback(
    (event: MouseEvent, uri: string) => {
      // Only activate link if CTRL is held
      if (!event.ctrlKey) return;

      if (eventsRef.current.includes('OnLinkClick')) {
        eventHandlerRef.current('OnLinkClick', id, [uri]);
      } else {
        // Default behavior: open link in new tab
        window.open(uri, '_blank', 'noopener,noreferrer');
      }
    },
    [id]
  );

  // Initialize Shadow DOM and terminal
  useEffect(() => {
    if (!hostRef.current) return;

    // Create shadow root if it doesn't exist
    if (!shadowRootRef.current) {
      shadowRootRef.current = hostRef.current.attachShadow({ mode: 'open' });

      // Inject xterm styles into shadow root
      const styleEl = document.createElement('style');
      styleEl.textContent = xtermStyles + `
        :host {
          display: block;
          width: 100%;
          height: 100%;
        }
        .terminal-container {
          width: 100%;
          height: 100%;
          background: #000000;
          padding: 10px 0px 10px 10px;
          box-sizing: border-box;
        }
      `;
      shadowRootRef.current.appendChild(styleEl);

      // Create terminal container inside shadow root
      const container = document.createElement('div');
      container.className = 'terminal-container';
      shadowRootRef.current.appendChild(container);
      terminalContainerRef.current = container;
    }

    const container = terminalContainerRef.current;
    if (!container) return;

    // Clear any existing terminal content and reset state
    container.innerHTML = '';
    initialContentWrittenRef.current = false;

    const mergedTheme = { ...defaultTheme, ...theme };

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
    term.unicode.activeVersion = '11';

    const fitAddon = new FitAddon();
    term.loadAddon(fitAddon);

    // Create tooltip element for link hover
    const tooltip = document.createElement('div');
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
    tooltip.textContent = 'Ctrl+Click to Follow Link';
    document.body.appendChild(tooltip);

    // Load WebLinks addon with custom handler and hover
    const webLinksAddon = new WebLinksAddon(handleLinkClick, {
      hover: (event: MouseEvent, _uri: string) => {
        tooltip.style.display = 'block';
        tooltip.style.left = `${event.clientX + 10}px`;
        tooltip.style.top = `${event.clientY + 10}px`;
      },
      leave: () => {
        tooltip.style.display = 'none';
      },
    });
    term.loadAddon(webLinksAddon);

    // Load Clipboard addon if allowed (handles copy)
    if (allowClipboard) {
      const clipboardAddon = new ClipboardAddon();
      term.loadAddon(clipboardAddon);
    }

    // Manual paste handler for Shadow DOM compatibility
    const handlePaste = (e: ClipboardEvent) => {
      if (!allowClipboardRef.current || term.options.disableStdin) return;
      e.preventDefault();
      const text = e.clipboardData?.getData('text');
      if (text) {
        term.paste(text);
      }
    };

    // Keyboard-based paste handler (Ctrl+V / Cmd+V) using Clipboard API
    const handleKeyDown = async (e: KeyboardEvent) => {
      if (!allowClipboardRef.current || term.options.disableStdin) return;
      if ((e.ctrlKey || e.metaKey) && e.key === 'v') {
        e.preventDefault();
        try {
          const text = await navigator.clipboard.readText();
          if (text) {
            term.paste(text);
          }
        } catch {
          // Clipboard API not available or permission denied, fall back to paste event
        }
      }
    };

    container.addEventListener('paste', handlePaste);
    container.addEventListener('keydown', handleKeyDown);

    let disposed = false;

    // Defer opening until container has dimensions
    requestAnimationFrame(() => {
      if (disposed) return;

      term.open(container);

      terminalRef.current = term;
      fitAddonRef.current = fitAddon;

      // Register handlers before fit() so initial size is reported
      term.onData(handleData);
      term.onResize(handleResize);

      if (!cols && !rows) {
        fitAddon.fit();
        // Fire initial size since fit() may not trigger onResize if size matches default
        handleResize({ cols: term.cols, rows: term.rows });
      }

      if (initialContent && !initialContentWrittenRef.current) {
        term.write(initialContent);
        initialContentWrittenRef.current = true;
      }
    });

    const handleWindowResize = () => {
      if (!cols && !rows && fitAddonRef.current) {
        fitAddonRef.current.fit();
      }
    };

    window.addEventListener('resize', handleWindowResize);

    const resizeObserver = new ResizeObserver(() => {
      if (!cols && !rows && fitAddonRef.current) {
        fitAddonRef.current.fit();
      }
    });

    resizeObserver.observe(hostRef.current);

    return () => {
      disposed = true;
      window.removeEventListener('resize', handleWindowResize);
      container.removeEventListener('paste', handlePaste);
      container.removeEventListener('keydown', handleKeyDown);
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
        terminalRef.current.write('\x1b[?25l'); // Hide cursor
      } else {
        terminalRef.current.write('\x1b[?25h'); // Show cursor
      }
    }
  }, [isReadOnly]);

  // Subscribe to stream data (base64 encoded to preserve control characters)
  useEffect(() => {
    if (!stream?.id || !subscribeToStream) return;

    const unsubscribe = subscribeToStream(stream.id, (data) => {
      if (terminalRef.current && typeof data === 'string') {
        try {
          // Decode base64 to binary string, then to UTF-8
          const binaryString = atob(data);
          const bytes = new Uint8Array(binaryString.length);
          for (let i = 0; i < binaryString.length; i++) {
            bytes[i] = binaryString.charCodeAt(i);
          }
          const text = new TextDecoder('utf-8').decode(bytes);
          // Debug: log first chunk to verify encoding
          if (bytes.length > 0 && bytes.length < 100) {
            console.log('[Terminal] base64 decoded:', {
              rawLength: data.length,
              decodedLength: bytes.length,
              firstBytes: Array.from(bytes.slice(0, 20)),
              text: text.slice(0, 50)
            });
          }
          terminalRef.current.write(text);
        } catch (e) {
          // Fallback for non-base64 data
          console.warn('[Terminal] base64 decode failed, using raw data:', e, { data: data.slice(0, 50) });
          terminalRef.current.write(data);
        }
      } else {
        console.warn('[Terminal] unexpected data type:', typeof data, data);
      }
    });

    return unsubscribe;
  }, [stream?.id, subscribeToStream]);

  const style: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
    overflow: 'hidden',
  };

  return <div ref={hostRef} style={style} />;
};
