import React from 'react';
import { cn } from '@/lib/utils';

interface TreeContextValue {
  showLines: boolean;
}

export const TreeContext = React.createContext<TreeContextValue>({
  showLines: true,
});

interface TreeWidgetProps {
  id: string;
  showLines?: boolean;
  children?: React.ReactNode;
}

export const TreeWidget: React.FC<TreeWidgetProps> = ({
  showLines = true,
  children,
}) => {
  const contextValue = React.useMemo(() => ({ showLines }), [showLines]);

  return (
    <TreeContext.Provider value={contextValue}>
      <div
        className={cn('ivy-tree w-full', showLines && 'ivy-tree--lines')}
        role="tree"
      >
        {children}
      </div>
    </TreeContext.Provider>
  );
};
