interface SlotWidgetProps {
  name: string;
}

export const SlotWidget: React.FC<
    SlotWidgetProps & { children?: React.ReactNode }
> = ({ children }) => {
  return <>{children}</>;
};
