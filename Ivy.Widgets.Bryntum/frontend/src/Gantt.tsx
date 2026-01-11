import React from 'react';
import { IvyEventHandler } from './types';
import { getWidth, getHeight } from './styles';

interface GanttProps {
  /** Unique widget ID provided by Ivy */
  $id: string;
  /** Width prop from Ivy */
  $width?: string;
  /** Height prop from Ivy */
  $height?: string;
  /** Event handler provided by Ivy */
  $onEvent: IvyEventHandler;
  /** Label text */
  label?: string;
}

export const Gantt: React.FC<GanttProps> = ({
  $id,
  $width,
  $height,
  $onEvent,
  label,
}) => {
  const handleClick = () => {
    // Call back to C# via the event handler
    $onEvent('onClick', $id, []);
  };

  const style: React.CSSProperties = {
    ...getWidth($width),
    ...getHeight($height),
  };

  return (
    <div style={style} className="p-4 border rounded-lg">
      <button
        onClick={handleClick}
        className="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 transition-colors"
      >
        {label ?? 'Click me'}
      </button>
    </div>
  );
};
