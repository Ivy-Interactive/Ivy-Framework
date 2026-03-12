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

const toolLabels: Record<DrawingTool, string> = {
  [DrawingTool.Freehand]: '✏️',
  [DrawingTool.Line]: '╱',
  [DrawingTool.Rectangle]: '▭',
  [DrawingTool.Circle]: '○',
  [DrawingTool.Text]: 'T',
};

const toolTitles: Record<DrawingTool, string> = {
  [DrawingTool.Freehand]: 'Freehand',
  [DrawingTool.Line]: 'Line',
  [DrawingTool.Rectangle]: 'Rectangle',
  [DrawingTool.Circle]: 'Circle',
  [DrawingTool.Text]: 'Text',
};

export const Toolbar: React.FC<ToolbarProps> = ({
  activeTool,
  color,
  lineWidth,
  onToolChange,
  onColorChange,
  onLineWidthChange,
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
            {toolLabels[tool]}
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
        <select
          value={lineWidth}
          onChange={(e) => onLineWidthChange(Number(e.target.value))}
          title="Line width"
        >
          <option value={2}>Thin</option>
          <option value={4}>Medium</option>
          <option value={8}>Thick</option>
        </select>
      </div>

      <div className="screenshot-toolbar-separator" />

      <div className="screenshot-toolbar-group">
        <button onClick={onUndo} disabled={!canUndo} title="Undo">
          ↩
        </button>
      </div>

      <div style={{ flex: 1 }} />

      <div className="screenshot-toolbar-group">
        <button className="cancel-btn" onClick={onCancel}>
          Cancel
        </button>
        <button className="save-btn" onClick={onSave}>
          Save
        </button>
      </div>
    </div>
  );
};
