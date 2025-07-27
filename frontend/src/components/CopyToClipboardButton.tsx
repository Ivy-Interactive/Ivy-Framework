import { useState } from 'react';
import { Copy, Check } from 'lucide-react';
import { cn } from '@/lib/utils';

interface CopyToClipboardButtonProps {
  textToCopy?: string;
  label?: string;
  'aria-label'?: string;
}

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
        'flex items-center gap-1 px-3 py-2 rounded-lg transition-all duration-200 ease-in-out cursor-pointer',
        'hover:bg-accent hover:shadow-sm',
        copied
          ? 'bg-primary/10 text-primary'
          : 'bg-transparent text-muted-foreground hover:text-foreground'
      )}
    >
      <span className="relative w-4 h-4">
        <span
          className={cn(
            'absolute inset-0 transform transition-transform duration-200',
            copied ? 'scale-0' : 'scale-100'
          )}
        >
          <Copy size={16} />
        </span>
        <span
          className={cn(
            'absolute inset-0 transform transition-transform duration-200',
            copied ? 'scale-100' : 'scale-0'
          )}
        >
          <Check size={16} />
        </span>
      </span>
      {label && <span className="text-xs">{copied ? 'Copied!' : label}</span>}
    </button>
  );
};

export default CopyToClipboardButton;
