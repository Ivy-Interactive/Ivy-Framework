import { Loader2 } from 'lucide-react';
import { classNames } from '../styles';

export function LoadingSpinner() {
  return (
    <div className={classNames.container.loading}>
      <div className={classNames.loading.container}>
        <Loader2
          className={`${classNames.icon.medium} ${classNames.icon.spinner}`}
        />
        <span>Loading data...</span>
      </div>
    </div>
  );
}
