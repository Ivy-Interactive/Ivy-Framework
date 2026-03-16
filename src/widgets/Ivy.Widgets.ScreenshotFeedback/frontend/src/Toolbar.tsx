import React from 'react';
import { DrawingTool } from './types';

interface ToolbarProps {
  activeTool: DrawingTool;
  color: string;
  lineWidth: number;
  onToolChange: (tool: DrawingTool) => void;
  onColorChange: (color: string) => void;
  onLineWidthChange: (width: number) => void;
  onUndo: () => void;
  onSave: () => void;
  onCancel: () => void;
  canUndo: boolean;
}

const ToolIcon: React.FC<{ tool: DrawingTool }> = ({ tool }) => {
  const size = 16;
  const props = { width: size, height: size, viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', strokeWidth: 2, strokeLinecap: 'round' as const, strokeLinejoin: 'round' as const };

  switch (tool) {
    case DrawingTool.Callout:
      return <svg {...props}><circle cx="7" cy="7" r="6" fill="currentColor" stroke="none" /><text x="7" y="7" textAnchor="middle" dominantBaseline="central" fill={props.stroke === 'currentColor' ? '#0a1a0f' : '#fff'} fontSize="9" fontWeight="bold" stroke="none">1</text><line x1="13" y1="13" x2="20" y2="20" /><line x1="20" y1="18" x2="20" y2="23" stroke="none" /></svg>;
    case DrawingTool.Freehand:
      return <svg {...props}><path d="M3 17c1.5-3 3-5 5-5s3 2 5 2 3.5-2 5-5" /><path d="M20 5l-2 2" /></svg>;
    case DrawingTool.Arrow:
      return <svg {...props}><line x1="5" y1="19" x2="19" y2="5" /><polyline points="12 5 19 5 19 12" /></svg>;
    case DrawingTool.Line:
      return <svg {...props}><line x1="5" y1="19" x2="19" y2="5" /></svg>;
    case DrawingTool.Rectangle:
      return <svg {...props}><rect x="3" y="3" width="18" height="18" rx="2" /></svg>;
    case DrawingTool.Circle:
      return <svg {...props}><circle cx="12" cy="12" r="10" /></svg>;
    case DrawingTool.Censor:
      return <svg {...props}><rect x="2" y="8" width="20" height="8" rx="1" fill="currentColor" stroke="none" /><line x1="4" y1="5" x2="12" y2="5" strokeWidth={1.5} /><line x1="8" y1="19" x2="18" y2="19" strokeWidth={1.5} /></svg>;
    case DrawingTool.Text:
      return <svg {...props}><polyline points="4 7 4 4 20 4 20 7" /><line x1="9" y1="20" x2="15" y2="20" /><line x1="12" y1="4" x2="12" y2="20" /></svg>;
  }
};

const toolTitles: Record<DrawingTool, string> = {
  [DrawingTool.Callout]: 'Callout',
  [DrawingTool.Freehand]: 'Freehand',
  [DrawingTool.Arrow]: 'Arrow',
  [DrawingTool.Line]: 'Line',
  [DrawingTool.Rectangle]: 'Rectangle',
  [DrawingTool.Circle]: 'Circle',
  [DrawingTool.Censor]: 'Censor',
  [DrawingTool.Text]: 'Text',
};

export const Toolbar: React.FC<ToolbarProps> = ({
  activeTool,
  color,
  lineWidth: _lineWidth,
  onToolChange,
  onColorChange,
  onLineWidthChange: _onLineWidthChange,
  onUndo,
  onSave,
  onCancel,
  canUndo,
}) => {
  return (
    <div className="screenshot-toolbar">
      <div className="screenshot-toolbar-group">
        {Object.values(DrawingTool).map((tool) => (
          <button
            key={tool}
            className={activeTool === tool ? 'active' : ''}
            onClick={() => onToolChange(tool)}
            title={toolTitles[tool]}
          >
            <ToolIcon tool={tool} />
          </button>
        ))}
      </div>

      <div className="screenshot-toolbar-separator" />

      <div className="screenshot-toolbar-group">
        <input
          type="color"
          value={color}
          onChange={(e) => onColorChange(e.target.value)}
          title="Color"
        />
      </div>

      <div className="screenshot-toolbar-separator" />

      <div className="screenshot-toolbar-group">
        <button onClick={onUndo} disabled={!canUndo} title="Undo (Ctrl+Z)">
          <svg width={16} height={16} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} strokeLinecap="round" strokeLinejoin="round">
            <polyline points="1 4 1 10 7 10" />
            <path d="M3.51 15a9 9 0 1 0 2.13-9.36L1 10" />
          </svg>
        </button>
      </div>

      <div style={{ flex: 1 }} />

      <div className="screenshot-toolbar-group">
        <button className="cancel-btn" onClick={onCancel}>
          Cancel
        </button>
        <button className="save-btn" onClick={onSave}>
          Submit
        </button>
      </div>
    </div>
  );
};
