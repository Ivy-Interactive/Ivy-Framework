import { useState } from 'react';
import { Copy, Check } from 'lucide-react';
import { cn } from '@/lib/utils';
import { buttonVariants } from '@/components/ui/button/variants';

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

  const isIconOnly = !label;

  return (
    <button
      onClick={handleCopy}
      aria-label={ariaLabel || 'Copy to clipboard'}
      className={cn(
        isIconOnly
          ? buttonVariants({ variant: 'ghost', size: 'icon' })
          : 'flex items-center gap-1 px-3 py-2 rounded-lg transition-all duration-200 ease-in-out cursor-pointer hover:bg-accent hover:shadow-sm border-0',
        !isIconOnly &&
          (copied
            ? 'bg-primary text-primary-foreground'
            : 'bg-transparent text-muted-foreground hover:text-foreground'),
        copied && isIconOnly && 'bg-primary text-primary-foreground'
      )}
    >
      <span className="relative w-4 h-4">
        <span
          className={cn(
            'absolute inset-0 transform transition-transform duration-200',
            copied ? 'scale-0' : 'scale-100'
          )}
        >
          <Copy className="h-4 w-4" />
        </span>
        <span
          className={cn(
            'absolute inset-0 transform transition-transform duration-200',
            copied ? 'scale-100' : 'scale-0'
          )}
        >
          <Check className="h-4 w-4" />
        </span>
      </span>
      {label && (
        <span className="text-small-label">{copied ? 'Copied!' : label}</span>
      )}
    </button>
  );
};

export default CopyToClipboardButton;
