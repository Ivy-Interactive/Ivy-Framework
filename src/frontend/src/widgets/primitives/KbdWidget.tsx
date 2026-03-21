import { Kbd } from '@/components/Kbd';
import React from 'react';

interface KbdWidgetProps {
  children: React.ReactNode;
}

export const KbdWidget: React.FC<KbdWidgetProps> = ({ children }) => (
  <Kbd>{children}</Kbd>
);
