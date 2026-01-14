import React, { useEffect, useRef, useCallback } from 'react';
import { Terminal as XTerm } from '@xterm/xterm';
import { FitAddon } from '@xterm/addon-fit';
import { WebLinksAddon } from '@xterm/addon-web-links';
import '@xterm/xterm/css/xterm.css';
import { IvyEventHandler } from './types';
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
  onIvyEvent: IvyEventHandler;
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
  onIvyEvent,
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
}) => {
  const containerRef = useRef<HTMLDivElement>(null);
  const terminalRef = useRef<XTerm | null>(null);
  const fitAddonRef = useRef<FitAddon | null>(null);
  const initialContentWrittenRef = useRef(false);

  const handleData = useCallback(
    (data: string) => {
      if (events.includes('OnData')) {
        onIvyEvent('OnData', id, [data]);
      }
    },
    [events, onIvyEvent, id]
  );

  const handleResize = useCallback(
    (size: { cols: number; rows: number }) => {
      if (events.includes('OnResize')) {
        onIvyEvent('OnResize', id, [{ cols: size.cols, rows: size.rows }]);
      }
    },
    [events, onIvyEvent, id]
  );

  const handleTitleChange = useCallback(
    (title: string) => {
      if (events.includes('OnTitleChange')) {
        onIvyEvent('OnTitleChange', id, [title]);
      }
    },
    [events, onIvyEvent, id]
  );

  useEffect(() => {
    if (!containerRef.current) return;

    const mergedTheme = { ...defaultTheme, ...theme };

    const term = new XTerm({
      cols: cols,
      rows: rows,
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

    term.open(containerRef.current);

    if (!cols && !rows) {
      fitAddon.fit();
    }

    terminalRef.current = term;
    fitAddonRef.current = fitAddon;

    term.onData(handleData);
    term.onResize(handleResize);
    term.onTitleChange(handleTitleChange);

    if (initialContent && !initialContentWrittenRef.current) {
      term.write(initialContent);
      initialContentWrittenRef.current = true;
    }

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

    if (containerRef.current) {
      resizeObserver.observe(containerRef.current);
    }

    return () => {
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
    handleTitleChange,
  ]);

  const style: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
    overflow: 'hidden',
  };

  return <div ref={containerRef} style={style} />;
};
