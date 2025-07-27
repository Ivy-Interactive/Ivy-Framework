import { Badge } from '@/components/ui/badge';
import { Database, Loader2 } from 'lucide-react';
import { classNames } from '../styles';
import { DataTableData } from '../types';

interface StatusBadgeProps {
  loading: boolean;
  isStreaming: boolean;
  data: DataTableData | null;
}

export function StatusBadge({ loading, isStreaming, data }: StatusBadgeProps) {
  if (loading) {
    return (
      <Badge variant="secondary" className={classNames.badge.container}>
        <Loader2
          className={`${classNames.icon.small} ${classNames.icon.spinner}`}
        />
        Loading...
      </Badge>
    );
  }
  if (isStreaming) {
    return (
      <Badge variant="default" className={classNames.badge.container}>
        <Database className={classNames.icon.small} />
        Streaming
      </Badge>
    );
  }
  if (data) {
    return (
      <Badge variant="outline" className={classNames.badge.container}>
        <Database className={classNames.icon.small} />
        {data.totalRows} rows
      </Badge>
    );
  }
  return null;
}
