import { Alert, AlertDescription } from '@/components/ui/alert';
import { AlertCircle } from 'lucide-react';
import { classNames } from '../styles';

export function ConnectionAlert() {
  return (
    <div className={classNames.container.error}>
      <Alert>
        <AlertCircle className={classNames.icon.medium} />
        <AlertDescription>
          Connection configuration is required for GlideDataGrid widget
        </AlertDescription>
      </Alert>
    </div>
  );
}
