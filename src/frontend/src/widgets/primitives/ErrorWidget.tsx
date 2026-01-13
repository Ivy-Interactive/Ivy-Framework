import { ErrorDisplay } from '@/components/ErrorDisplay';
import React from 'react';

interface ErrorWidgetProps {
  title?: string | null;
  message?: string | null;
  stackTrace?: string | null;
}

export const ErrorWidget: React.FC<ErrorWidgetProps> = ({
  title,
  message,
  stackTrace,
}) => <ErrorDisplay title={title} message={message} stackTrace={stackTrace} />;
