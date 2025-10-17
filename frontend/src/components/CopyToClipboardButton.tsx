import { useState } from 'react';
import { cn } from '@/lib/utils';

interface CopyToClipboardButtonProps {
  textToCopy?: string;
  label?: string;
  'aria-label'?: string;
}

// Simple overlapping squares icon to match the design
const CopyIcon = ({
  size = 16,
  className,
}: {
  size?: number;
  className?: string;
}) => (
  <svg
    width={size}
    height={size}
    viewBox="0 0 16 16"
    fill="none"
    xmlns="http://www.w3.org/2000/svg"
    className={className}
  >
    <rect
      x="2"
      y="2"
      width="8"
      height="8"
      rx="1"
      stroke="currentColor"
      strokeWidth="1.5"
      fill="none"
    />
    <rect
      x="6"
      y="6"
      width="8"
      height="8"
      rx="1"
      stroke="currentColor"
      strokeWidth="1.5"
      fill="none"
    />
  </svg>
);

const CheckIcon = ({
  size = 16,
  className,
}: {
  size?: number;
  className?: string;
}) => (
  <svg
    width={size}
    height={size}
    viewBox="0 0 16 16"
    fill="none"
    xmlns="http://www.w3.org/2000/svg"
    className={className}
  >
    <path
      d="M13.5 4.5L6 12L2.5 8.5"
      stroke="currentColor"
      strokeWidth="1.5"
      strokeLinecap="round"
      strokeLinejoin="round"
    />
  </svg>
);

const CopyToClipboardButton: React.FC<CopyToClipboardButtonProps> = ({
  textToCopy = '',
  label = '',
  'aria-label': ariaLabel,
}) => {
  const [copied, setCopied] = useState(false);

  const handleCopy = async () => {
    try {
      await navigator.clipboard.writeText(textToCopy);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    } catch (err: unknown) {
      console.error(err);
    }
  };

  return (
    <button
      onClick={handleCopy}
      aria-label={ariaLabel || 'Copy to clipboard'}
      className={cn(
        'flex items-center justify-center w-8 h-8 rounded transition-all duration-200 ease-in-out cursor-pointer',
        'hover:bg-black/5 dark:hover:bg-white/5',
        copied
          ? 'text-green-600 dark:text-green-400'
          : 'text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-200'
      )}
    >
      <span className="relative w-4 h-4">
        <span
          className={cn(
            'absolute inset-0 transform transition-transform duration-200',
            copied ? 'scale-0' : 'scale-100'
          )}
        >
          <CopyIcon size={16} />
        </span>
        <span
          className={cn(
            'absolute inset-0 transform transition-transform duration-200',
            copied ? 'scale-100' : 'scale-0'
          )}
        >
          <CheckIcon size={16} />
        </span>
      </span>
      {label && (
        <span className="text-small-label ml-1">
          {copied ? 'Copied!' : label}
        </span>
      )}
    </button>
  );
};

export default CopyToClipboardButton;
