import { Alert, AlertDescription } from '@/components/ui/alert';
import { AlertCircle } from 'lucide-react';
import { classNames } from '../styles';

interface ErrorAlertProps {
  error: string;
}

export function ErrorAlert({ error }: ErrorAlertProps) {
  return (
    <div className={classNames.container.error}>
      <Alert variant="destructive">
        <AlertCircle className={classNames.icon.medium} />
        <AlertDescription>{error}</AlertDescription>
      </Alert>
    </div>
  );
}