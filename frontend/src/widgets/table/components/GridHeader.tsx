import { Button } from '@/components/ui/button';
import { RefreshCw } from 'lucide-react';
import { useGlideDataGrid } from '../hooks/useGlideDataGrid';
import { classNames } from '../styles';
import { filterDataTableData } from '../utils/dataFilter';
import { FilterInput } from './FilterInput';
import { StatusBadge } from './StatusBadge';

export function GridHeader() {
  const { props, loading, refresh } = useGlideDataGrid();
  const {
    title,
    description,
    showStatus = true,
    showRefreshButton = true,
  } = props;

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
        <FilterInput onFilter={filterDataTableData} />
        {showStatus && <StatusBadge />}
        {showRefreshButton && (
          <Button
            variant="outline"
            size="sm"
            onClick={refresh}
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
