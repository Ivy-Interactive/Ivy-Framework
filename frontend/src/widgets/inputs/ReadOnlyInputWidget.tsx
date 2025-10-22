import CopyToClipboardButton from '@/components/CopyToClipboardButton';
import React from 'react';

interface ReadOnlyInputWidgetProps {
  id: string;
  value: string | number | boolean | null | undefined;
  showCopyButton?: boolean;
}

export const ReadOnlyInputWidget: React.FC<ReadOnlyInputWidgetProps> = ({
  id,
  value,
  showCopyButton = true,
}) => {
  return (
    <div
      key={id}
      className="text-body text-muted-foreground flex flex-row items-center w-full"
    >
      <div className="flex-1 overflow-hidden text-ellipsis whitespace-nowrap">
        {value && value}
        {!value && '-'}
      </div>
      {showCopyButton && (
        <CopyToClipboardButton textToCopy={String(value || '')} label="" />
      )}
    </div>
  );
};
