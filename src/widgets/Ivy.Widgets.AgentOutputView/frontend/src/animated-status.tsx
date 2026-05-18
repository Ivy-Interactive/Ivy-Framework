import React from "react";

interface AnimatedStatusProps {
  statusText: string;
  isComplete: boolean;
  showIcon?: boolean;
}

const SpinnerIcon: React.FC = () => (
  <svg
    xmlns="http://www.w3.org/2000/svg"
    viewBox="0 0 24 24"
    fill="none"
    stroke="currentColor"
    strokeWidth="2"
    strokeLinecap="round"
    strokeLinejoin="round"
    width="14"
    height="14"
    aria-hidden="true"
  >
    <path d="M21 12a9 9 0 1 1-6.219-8.56" />
  </svg>
);

const CheckIcon: React.FC = () => (
  <svg
    xmlns="http://www.w3.org/2000/svg"
    viewBox="0 0 24 24"
    fill="none"
    stroke="currentColor"
    strokeWidth="2"
    strokeLinecap="round"
    strokeLinejoin="round"
    width="14"
    height="14"
    aria-hidden="true"
  >
    <circle cx="12" cy="12" r="10" />
    <path d="m9 12 2 2 4-4" />
  </svg>
);

export const AnimatedStatus: React.FC<AnimatedStatusProps> = ({
  statusText,
  isComplete,
  showIcon = true,
}) => {
  if (isComplete) {
    return (
      <div className="aov-status-done">
        {showIcon && (
          <span className="aov-status-icon">
            <CheckIcon />
          </span>
        )}
        <span className="aov-status-done-label">{statusText}</span>
      </div>
    );
  }

  return (
    <div className="aov-status-running">
      {showIcon && (
        <span className="aov-status-icon aov-status-icon-spin">
          <SpinnerIcon />
        </span>
      )}
      <span className="aov-status-reveal aov-status-shimmer">{statusText}</span>
    </div>
  );
};
