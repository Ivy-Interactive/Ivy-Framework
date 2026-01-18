import React, { useEffect, useRef, useCallback } from 'react';
import { Terminal as XTerm } from '@xterm/xterm';
import { FitAddon } from '@xterm/addon-fit';
import { WebLinksAddon } from '@xterm/addon-web-links';
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
  fontSize?: number;
  fontFamily?: string;
  lineHeight?: number;
  cursorBlink?: boolean;
  cursorStyle?: CursorStyle;
  scrollback?: number;
  theme?: TerminalTheme;
  initialContent?: string;
  stream?: { id: string };
}

const defaultTheme: TerminalTheme = {
  background: '#1e1e1e',
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
  fontSize = 14,
  fontFamily = "Menlo, Monaco, 'Courier New', monospace",
  lineHeight = 1.0,
  cursorBlink = true,
  cursorStyle = 'Block',
  scrollback = 1000,
  theme,
  initialContent,
  stream,
}) => {
  const hostRef = useRef<HTMLDivElement>(null);
  const shadowRootRef = useRef<ShadowRoot | null>(null);
  const terminalContainerRef = useRef<HTMLDivElement | null>(null);
  const terminalRef = useRef<XTerm | null>(null);
  const fitAddonRef = useRef<FitAddon | null>(null);
  const initialContentWrittenRef = useRef(false);

  const handleData = useCallback(
    (data: string) => {
      if (events.includes('OnInput')) {
        eventHandler('OnInput', id, [data]);
      }
    },
    [events, eventHandler, id]
  );

  const handleResize = useCallback(
    (size: { cols: number; rows: number }) => {
      if (events.includes('OnResize')) {
        eventHandler('OnResize', id, [{ cols: size.cols, rows: size.rows }]);
      }
    },
    [events, eventHandler, id]
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

    // Clear any existing terminal content
    container.innerHTML = '';

    const mergedTheme = { ...defaultTheme, ...theme };

    const term = new XTerm({
      ...(cols !== undefined && { cols }),
      ...(rows !== undefined && { rows }),
      fontSize,
      fontFamily,
      lineHeight,
      cursorBlink,
      cursorStyle: mapCursorStyle(cursorStyle),
      scrollback,
      theme: mergedTheme,
      allowProposedApi: true,
    });

    const fitAddon = new FitAddon();
    const webLinksAddon = new WebLinksAddon();

    term.loadAddon(fitAddon);
    term.loadAddon(webLinksAddon);

    let disposed = false;

    // Defer opening until container has dimensions
    requestAnimationFrame(() => {
      if (disposed) return;

      term.open(container);

      if (!cols && !rows) {
        fitAddon.fit();
      }

      terminalRef.current = term;
      fitAddonRef.current = fitAddon;

      term.onData(handleData);
      term.onResize(handleResize);

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
      resizeObserver.disconnect();
      term.dispose();
    };
  }, [
    cols,
    rows,
    fontSize,
    fontFamily,
    lineHeight,
    cursorBlink,
    cursorStyle,
    scrollback,
    theme,
    initialContent,
    handleData,
    handleResize,
  ]);

  // Subscribe to stream data
  useEffect(() => {
    if (!stream?.id || !subscribeToStream) return;

    const unsubscribe = subscribeToStream(stream.id, (data) => {
      if (terminalRef.current && typeof data === 'string') {
        terminalRef.current.write(data);
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
