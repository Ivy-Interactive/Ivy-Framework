import React from 'react';
import { ResizablePanelGroup, ResizablePanel, ResizableHandle } from "@/components/ui/resizable";
import { camelCase, cn } from '@/lib/utils';
import { getHeight, getWidth } from '@/lib/styles';

interface ResizeablePanelWidgetProps {
  children: React.ReactNode[];
  defaultSize?: number;
  id?: string;
}

export const ResizeablePanelWidget: React.FC<ResizeablePanelWidgetProps> = ({
  children
}) => {
  return (
    <div className='h-full w-full p-4'>
      {children}
    </div>
  );
};

ResizeablePanelWidget.displayName = 'ResizeablePanelWidget';

interface ResizeablePanelGroupWidgetProps {
  id: string;
  children: React.ReactElement<ResizeablePanelWidgetProps>[];
  showHandle?: boolean;
  direction?: 'Horizontal' | 'Vertical';
  width?: string;
  height?: string;
}

export const ResizeablePanelGroupWidget: React.FC<ResizeablePanelGroupWidgetProps> = ({
  id,
  children,
  showHandle = true,
  direction = 'Horizontal',
  width,
  height
}) => {
  const panelWidgets = React.Children.toArray(children).filter((child) => 
    React.isValidElement(child) && 
    (child.type as React.ComponentType<ResizeablePanelWidgetProps>)?.displayName === 'ResizeablePanelWidget'
  );

  if (panelWidgets.length === 0) return <div className='remove-ancestor-padding'></div>;

  const style = {
    ...getWidth(width),
    ...getHeight(height)
  }

  return (
    <ResizablePanelGroup 
      style={style}
      direction={camelCase(direction)} 
      className="remove-ancestor-padding"
      id={id}
    >
      {panelWidgets.map((panelWidget, index) => {
        if (React.isValidElement(panelWidget)) {
          const { defaultSize } = panelWidget.props as ResizeablePanelWidgetProps;
          
          return (
            <React.Fragment key={index}>
              {index > 0 && showHandle && (
                <ResizableHandle 
                  withHandle={showHandle}
                  className={cn(
                    "border",
                    direction === 'Horizontal' ? 'border-r' : 'border-t'
                  )}
                />
              )}
              <ResizablePanel 
                defaultSize={defaultSize ?? Math.floor(100 / panelWidgets.length)}
                className="h-full"
              >
                {panelWidget}
              </ResizablePanel>
            </React.Fragment>
          );
        }
        return null;
      })}
    </ResizablePanelGroup>
  );
}; 