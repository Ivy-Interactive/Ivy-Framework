import { useGlideDataGrid } from '../hooks/useGlideDataGrid';
import { classNames } from '../styles';
import { ConnectionAlert } from './ConnectionAlert';
import { ErrorAlert } from './ErrorAlert';
import { GridEditor } from './GridEditor';
import { LoadingSpinner } from './LoadingSpinner';

export function GridContainer() {
  const { data, loading, error, isValidConnection } = useGlideDataGrid();

  if (!isValidConnection) {
    return <ConnectionAlert />;
  }

  return (
    <div className={classNames.container.content}>
      {error && <ErrorAlert error={error} />}
      {loading ? (
        <LoadingSpinner />
      ) : data ? (
        <GridEditor />
      ) : (
        <div className={classNames.container.empty}>No data available</div>
      )}
    </div>
  );
}
