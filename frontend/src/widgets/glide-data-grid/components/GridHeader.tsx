import { Button } from '@/components/ui/button';
import { RefreshCw } from 'lucide-react';
import { classNames } from '../styles';
import { DataTableData } from '../types';
import { StatusBadge } from './StatusBadge';

interface GridHeaderProps {
  title?: string;
  description?: string;
  showStatus: boolean;
  showRefreshButton: boolean;
  loading: boolean;
  isStreaming: boolean;
  data: DataTableData | null;
  onRefresh: () => void;
}

export function GridHeader({
  title,
  description,
  showStatus,
  showRefreshButton,
  loading,
  isStreaming,
  data,
  onRefresh,
}: GridHeaderProps) {
  if (!title && !description && !showStatus && !showRefreshButton) {
    return null;
  }

  return (
    <div className={classNames.header.wrapper}>
      <div className={classNames.header.content}>
        {title && <h3 className={classNames.header.title}>{title}</h3>}
        {description && (
          <p className={classNames.header.description}>{description}</p>
        )}
      </div>
      <div className={classNames.header.actions}>
        {showStatus && (
          <StatusBadge
            loading={loading}
            isStreaming={isStreaming}
            data={data}
          />
        )}
        {showRefreshButton && (
          <Button
            variant="outline"
            size="sm"
            onClick={onRefresh}
            disabled={loading}
            className={classNames.button.refresh}
          >
            <RefreshCw className={classNames.icon.refreshIcon(loading)} />
            Refresh
          </Button>
        )}
      </div>
    </div>
  );
}
