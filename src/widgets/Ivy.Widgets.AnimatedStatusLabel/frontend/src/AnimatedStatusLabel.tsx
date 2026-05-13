import React, { ReactNode } from "react";
import "./AnimatedStatusLabel.css";

type EventHandler = (eventName: string, widgetId: string, args: unknown[]) => void;

interface AnimatedStatusLabelProps {
  id: string;
  statusText: string;
  isComplete: boolean;
  showIcon?: boolean;
  rightLabel?: string;
  eventHandler?: EventHandler;
  onIvyEvent?: EventHandler;
  events?: string[];
  children?: ReactNode;
}

export const AnimatedStatusLabel: React.FC<AnimatedStatusLabelProps> = ({
  statusText,
  isComplete,
  showIcon = true,
  rightLabel,
  children,
}) => {
  const childArray = React.Children.toArray(children);
  const spinnerIcon = childArray[0] ?? null;
  const doneIcon = childArray[1] ?? null;

  if (isComplete) {
    return (
      <div className="asl-inline-done-row asl-enter">
        {showIcon && <span className="asl-icon">{doneIcon}</span>}
        <span className="asl-done-label text-sm">{statusText}</span>
        {rightLabel && <span className="asl-right-label text-sm">{rightLabel}</span>}
      </div>
    );
  }

  return (
    <div className="animated-status-container text-sm flex flex-col gap-2 w-full">
      <div className="asl-status-row select-none w-full">
        <div className="asl-status-left">
          {showIcon && <span className="asl-icon asl-icon-spin">{spinnerIcon}</span>}
          <span className="status-reveal status-loading">{statusText}</span>
        </div>
        {rightLabel && <span className="asl-right-label text-sm">{rightLabel}</span>}
      </div>
    </div>
  );
};
